"""Orchestrateur full-auto adapte de l'architecture ForgeHistory.

Le programme selectionne l'unique increment EN_COURS de la roadmap, lance un
Generateur Codex, execute des portes mecaniques, puis lance Claude en lecture
seule comme Evaluateur. Un PASS crée une pull request qui reste soumise à
l'audit Cursor, au challenge Claude et au merge bot. Tout doute échoue fermé.
"""

from __future__ import annotations

import argparse
import contextlib
import datetime as dt
import json
import os
import re
import shutil
import subprocess
import sys
import time
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Any, Iterable, Sequence, TextIO

ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from harness.pipeline.actors import ActorError, claude_structured, hermes_plan, resolve_command


DEFAULT_CONFIG = ROOT / "harness" / "pipeline" / "config.json"
GENERATOR_PROMPT = ROOT / "harness" / "prompts" / "generator.md"
EVALUATOR_PROMPT = ROOT / "harness" / "prompts" / "evaluator.md"
EVALUATOR_SCHEMA = ROOT / "harness" / "schemas" / "evaluator.schema.json"
RUN_ROOT = ROOT / "Logs" / "FullAuto"
LOCK_PATH = RUN_ROOT / ".pipeline.lock"

INCREMENT_RE = re.compile(
    r"^\|\s*(?P<order>\d+)\s*\|\s*(?P<status>[^|]+?)\s*\|\s*"
    r"`(?P<task>[^`]+)`\s*\|\s*(?P<increment>[^|]+?)\s*\|\s*"
    r"(?P<proof>[^|]+?)\s*\|\s*$"
)


class PipelineError(RuntimeError):
    """Erreur attendue, affichee sans traceback par le CLI."""


@dataclass(frozen=True)
class Increment:
    order: int
    status: str
    task: str
    increment: str
    proof: str


@dataclass(frozen=True)
class CommandResult:
    name: str
    command: list[str]
    returncode: int
    duration_seconds: float
    output_file: str

    @property
    def passed(self) -> bool:
        return self.returncode == 0


def load_config(path: Path = DEFAULT_CONFIG) -> dict[str, Any]:
    try:
        config = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise PipelineError(f"configuration illisible: {path}: {exc}") from exc
    required = {"mode", "roadmap", "max_iterations", "generator", "evaluator", "publish"}
    missing = sorted(required - config.keys())
    if missing:
        raise PipelineError(f"configuration incomplete: {', '.join(missing)}")
    if config["mode"] not in {"manual", "full_auto"}:
        raise PipelineError("mode doit valoir manual ou full_auto")
    if not isinstance(config["max_iterations"], int) or config["max_iterations"] < 1:
        raise PipelineError("max_iterations doit etre un entier positif")
    return config


def parse_increments(text: str) -> list[Increment]:
    increments: list[Increment] = []
    in_queue = False
    for line in text.splitlines():
        if line.strip().startswith("## Sessions Codex ordonnees") or line.strip().startswith(
            "## Sessions Codex ordonnées"
        ):
            in_queue = True
            continue
        if in_queue and line.startswith("## "):
            break
        if not in_queue:
            continue
        match = INCREMENT_RE.match(line)
        if match:
            increments.append(
                Increment(
                    order=int(match.group("order")),
                    status=match.group("status").strip(),
                    task=match.group("task").strip(),
                    increment=match.group("increment").strip(),
                    proof=match.group("proof").strip(),
                )
            )
    return increments


def select_increment(roadmap: Path) -> Increment:
    increments = parse_increments(roadmap.read_text(encoding="utf-8"))
    active = [item for item in increments if item.status == "EN_COURS"]
    if len(active) != 1:
        raise PipelineError(
            f"la roadmap doit contenir exactement un increment EN_COURS, trouve: {len(active)}"
        )
    return active[0]


def run_capture(command: Sequence[str], *, check: bool = True) -> str:
    completed = subprocess.run(
        resolve_command(command), cwd=ROOT, text=True, encoding="utf-8", errors="replace",
        stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False,
    )
    if check and completed.returncode != 0:
        raise PipelineError(
            f"commande en echec ({completed.returncode}): {' '.join(command)}\n{completed.stdout}"
        )
    return completed.stdout


def git_lines(*args: str) -> list[str]:
    return [line for line in run_capture(["git", *args]).splitlines() if line.strip()]


def changed_files() -> list[str]:
    return sorted(set(git_lines("diff", "--name-only") + git_lines("diff", "--cached", "--name-only") + git_lines("ls-files", "--others", "--exclude-standard")))


def normalized(path: str) -> str:
    return path.replace("\\", "/")


def matching_prefixes(paths: Iterable[str], prefixes: Iterable[str]) -> list[str]:
    normalized_prefixes = tuple(normalized(prefix) for prefix in prefixes)
    return sorted(
        normalized(path)
        for path in paths
        if any(normalized(path).startswith(prefix) for prefix in normalized_prefixes)
    )


def validate_change_scope(paths: Sequence[str], config: dict[str, Any]) -> None:
    protected = matching_prefixes(paths, config.get("protected_prefixes", []))
    if protected:
        raise PipelineError(
            "publication refusee: chemins proteges modifies:\n- " + "\n- ".join(protected)
        )
    production = matching_prefixes(paths, config.get("production_prefixes", []))
    if production:
        required = {normalized(path) for path in config.get("required_docs_for_production", [])}
        missing = sorted(required - {normalized(path) for path in paths})
        if missing:
            raise PipelineError(
                "publication refusee: une modification de production doit synchroniser:\n- "
                + "\n- ".join(missing)
            )


def check_preflight(config: dict[str, Any], *, allow_dirty: bool, publishing: bool) -> Increment:
    if config["mode"] != "full_auto":
        raise PipelineError("pipeline desactive: mode != full_auto")
    if os.environ.get("CITYLAB_FULL_AUTO_PAUSE", "").lower() in {"1", "true", "yes"}:
        raise PipelineError("pipeline en pause via CITYLAB_FULL_AUTO_PAUSE")
    if LOCK_PATH.with_suffix(".pause").exists():
        raise PipelineError(f"pipeline en pause via {LOCK_PATH.with_suffix('.pause')}")
    if not shutil.which("git"):
        raise PipelineError("git introuvable")
    if not shutil.which("codex"):
        raise PipelineError("codex CLI introuvable")
    if publishing and not shutil.which("gh"):
        raise PipelineError("gh CLI introuvable pour la publication")
    dirty = git_lines("status", "--porcelain")
    if dirty and not allow_dirty:
        raise PipelineError(
            "worktree sale: le full-auto exige un depart propre pour attribuer ses changements"
        )
    roadmap = ROOT / config["roadmap"]
    return select_increment(roadmap)


@contextlib.contextmanager
def exclusive_lock() -> Iterable[None]:
    LOCK_PATH.parent.mkdir(parents=True, exist_ok=True)
    try:
        descriptor = os.open(LOCK_PATH, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
    except FileExistsError as exc:
        raise PipelineError(f"un run full-auto est deja actif: {LOCK_PATH}") from exc
    with os.fdopen(descriptor, "w", encoding="utf-8") as stream:
        json.dump({"pid": os.getpid(), "started_at": dt.datetime.now(dt.UTC).isoformat()}, stream)
    try:
        yield
    finally:
        with contextlib.suppress(OSError):
            LOCK_PATH.unlink()


def render_prompt(template: Path, values: dict[str, str]) -> str:
    text = template.read_text(encoding="utf-8")
    for key, value in values.items():
        text = text.replace("{" + key + "}", value)
    return text


def write_console(text: str, stream: TextIO | None = None) -> None:
    """Write actor output without letting a legacy Windows code page stop the run."""
    target = stream or sys.stdout
    encoding = getattr(target, "encoding", None) or "utf-8"
    safe_text = text.encode(encoding, errors="backslashreplace").decode(encoding)
    target.write(safe_text)
    target.flush()


def run_streaming(
    name: str,
    command: Sequence[str],
    *,
    stdin_text: str | None,
    timeout_seconds: int,
    output_file: Path,
) -> CommandResult:
    output_file.parent.mkdir(parents=True, exist_ok=True)
    started = time.monotonic()
    interrupted_after_actor_exit = False
    with output_file.open("w", encoding="utf-8") as log:
        process = subprocess.Popen(
            resolve_command(command), cwd=ROOT, stdin=subprocess.PIPE if stdin_text is not None else None,
            stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True,
            encoding="utf-8", errors="replace",
        )
        try:
            stdout, _ = process.communicate(stdin_text, timeout=timeout_seconds)
        except subprocess.TimeoutExpired:
            process.kill()
            stdout, _ = process.communicate()
            stdout += f"\nTIMEOUT after {timeout_seconds}s\n"
        except KeyboardInterrupt:
            # On Windows, an actor may exit while a detached tool still owns its
            # stdout pipe. Closing that tool can emit CTRL_C to the waiting
            # reader. Preserve a real cancellation while turning this post-exit
            # signal into a bounded failed iteration that the loop can repair.
            if process.poll() is None:
                raise
            interrupted_after_actor_exit = True
            stdout = (
                "ACTOR_OUTPUT_INTERRUPTED_AFTER_EXIT: detached tool closed the "
                "Windows console stream; retry this iteration.\n"
            )
            with contextlib.suppress(OSError):
                if process.stdout is not None:
                    process.stdout.close()
        log.write(stdout)
        write_console(stdout)
    return CommandResult(
        name=name,
        command=list(command),
        returncode=(
            130 if interrupted_after_actor_exit
            else process.returncode if process.returncode is not None else 124
        ),
        duration_seconds=round(time.monotonic() - started, 3),
        output_file=str(output_file.relative_to(ROOT)),
    )


def codex_command(
    *, model: str, sandbox: str, output_file: Path, schema: Path | None = None
) -> list[str]:
    command = [
        "codex", "--ask-for-approval", "never", "exec", "-C", str(ROOT),
        "--model", model, "--sandbox", sandbox,
        "--output-last-message", str(output_file), "-",
    ]
    if schema is not None:
        command[command.index("--output-last-message"):command.index("--output-last-message")] = [
            "--output-schema", str(schema)
        ]
    return command


def roadmap_gate_command() -> list[str]:
    if os.name == "nt":
        return [
            "powershell", "-ExecutionPolicy", "Bypass", "-File",
            str(ROOT / "Tools" / "check_roadmap.ps1"),
        ]
    return ["pwsh", "-File", str(ROOT / "Tools" / "check_roadmap.ps1")]


def mechanical_gates(run_dir: Path) -> list[CommandResult]:
    commands = [
        ("roadmap", roadmap_gate_command()),
        (
            "harness-tests",
            [sys.executable, "-m", "unittest", "discover", "-s", "harness/tests", "-v"],
        ),
        ("diff-check", ["git", "diff", "--check"]),
    ]
    results: list[CommandResult] = []
    for name, command in commands:
        results.append(
            run_streaming(
                name, command, stdin_text=None, timeout_seconds=600,
                output_file=run_dir / "gates" / f"{name}.log",
            )
        )
    return results


def diff_for_evaluation(limit: int = 240_000) -> str:
    diff = run_capture(["git", "diff", "--no-ext-diff", "--binary"], check=False)
    untracked = git_lines("ls-files", "--others", "--exclude-standard")
    if untracked:
        diff += "\nUNTRACKED FILES:\n" + "\n".join(untracked)
    if len(diff) > limit:
        return diff[:limit] + f"\n[diff tronque a {limit} caracteres]"
    return diff


def parse_evaluation(path: Path) -> dict[str, Any]:
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise PipelineError(f"sortie Evaluateur illisible: {path}: {exc}") from exc
    if data.get("verdict") not in {"PASS", "REJECT"}:
        raise PipelineError("verdict Evaluateur absent ou invalide")
    if not isinstance(data.get("blocking_findings"), list):
        raise PipelineError("blocking_findings Evaluateur invalide")
    if data["verdict"] == "PASS" and data["blocking_findings"]:
        raise PipelineError("verdict incoherent: PASS avec constats bloquants")
    return data


def slug(value: str) -> str:
    return re.sub(r"[^a-z0-9]+", "-", value.lower()).strip("-")


def create_branch(increment: Increment, config: dict[str, Any], run_id: str) -> str:
    branch = f"{config['branch_prefix']}/{slug(increment.task)}-{run_id.lower()}"
    run_capture(["git", "switch", "-c", branch])
    return branch


def publish(
    increment: Increment,
    config: dict[str, Any],
    branch: str,
    run_dir: Path,
    evaluation: dict[str, Any],
) -> str:
    paths = changed_files()
    if not paths:
        raise PipelineError("aucun changement a publier")
    validate_change_scope(paths, config)
    run_capture(["git", "add", "-A"])
    run_capture(["git", "commit", "-m", f"auto({increment.task}): increment {increment.order:02d}"])
    remote = config["publish"]["remote"]
    base = config["publish"]["base_branch"]
    run_capture(["git", "push", "-u", remote, branch])
    body_file = run_dir / "pull-request.md"
    body_file.write_text(
        "\n".join(
            [
                f"Roadmap: `{increment.task}` / increment {increment.order:02d}",
                "",
                increment.increment,
                "",
                "Preuve attendue:",
                increment.proof,
                "",
                "Evaluation independante:",
                evaluation["summary"],
                "",
                f"Run local: `{run_dir.relative_to(ROOT)}`",
            ]
        ),
        encoding="utf-8",
    )
    title = f"auto({increment.task}): {increment.increment[:72]}"
    url = run_capture(
        [
            "gh", "pr", "create", "--base", base, "--head", branch,
            "--title", title, "--body-file", str(body_file),
        ]
    ).strip().splitlines()[-1]
    if config["publish"].get("auto_merge"):
        run_capture(["gh", "pr", "edit", url, "--add-label", "pipeline/auto-merge"])
    return url


def write_report(run_dir: Path, payload: dict[str, Any]) -> None:
    run_dir.mkdir(parents=True, exist_ok=True)
    (run_dir / "run-report.json").write_text(
        json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )


def dry_run_plan(increment: Increment, config: dict[str, Any], publishing: bool) -> dict[str, Any]:
    return {
        "mode": config["mode"],
        "increment": asdict(increment),
        "max_iterations": config["max_iterations"],
        "generator_model": config["generator"]["model"],
        "evaluator_model": "claude:" + config["evaluator"]["model"],
        "publishing": publishing,
        "gates": ["roadmap", "harness-tests", "diff-check", "independent-evaluator"],
        "kill_switches": [
            "config mode=manual",
            "CITYLAB_FULL_AUTO_PAUSE=1",
            str(LOCK_PATH.with_suffix(".pause").relative_to(ROOT)),
            "GitHub label pipeline/pause",
        ],
    }


def execute(config: dict[str, Any], *, allow_dirty: bool, publishing: bool, dry_run: bool) -> int:
    increment = check_preflight(config, allow_dirty=allow_dirty, publishing=publishing)
    if dry_run:
        print(json.dumps(dry_run_plan(increment, config, publishing), ensure_ascii=False, indent=2))
        return 0
    run_id = dt.datetime.now(dt.UTC).strftime("%Y%m%dT%H%M%SZ")
    run_dir = RUN_ROOT / f"{run_id}-{slug(increment.task)}-{increment.order:02d}"
    run_dir.mkdir(parents=True, exist_ok=False)
    branch = ""
    if publishing:
        branch = create_branch(increment, config, run_id)
    report: dict[str, Any] = {
        "schema": 1,
        "run_id": run_id,
        "increment": asdict(increment),
        "branch": branch,
        "status": "RUNNING",
        "iterations": [],
    }
    try:
        project_plan = hermes_plan(
            ROOT,
            "Tu es Hermes chef de projet Victoria CityLab. A partir de l'increment "
            f"{increment.task} suivant: {increment.increment}. Donne au Generateur un plan "
            "court, ordonne, respectant determinisme, sauvegarde, HUD, tests et preuves.",
        )
    except ActorError as exc:
        raise PipelineError(f"Hermes chef de projet indisponible: {exc}") from exc
    report["hermes_plan"] = project_plan
    feedback = "Plan Hermes:\n" + project_plan
    for iteration in range(1, config["max_iterations"] + 1):
        iteration_dir = run_dir / f"iteration-{iteration:02d}"
        generator_message = iteration_dir / "generator-final.md"
        prompt = render_prompt(
            GENERATOR_PROMPT,
            {
                "increment": increment.increment,
                "proof": increment.proof,
                "feedback": feedback,
            },
        )
        generator_result = run_streaming(
            "generator",
            codex_command(
                model=config["generator"]["model"], sandbox="danger-full-access",
                output_file=generator_message,
            ),
            stdin_text=prompt,
            timeout_seconds=config["generator"]["timeout_seconds"],
            output_file=iteration_dir / "generator.log",
        )
        gate_results = mechanical_gates(iteration_dir)
        iteration_report: dict[str, Any] = {
            "iteration": iteration,
            "generator": asdict(generator_result),
            "gates": [asdict(result) | {"passed": result.passed} for result in gate_results],
        }
        if not generator_result.passed or not all(result.passed for result in gate_results):
            feedback = "Les portes mecaniques ont echoue. Lis les logs de cette iteration et corrige-les."
            iteration_report["verdict"] = "MECHANICAL_REJECT"
            report["iterations"].append(iteration_report)
            write_report(run_dir, report)
            continue
        evaluator_output = iteration_dir / "evaluator.json"
        evaluator_prompt = render_prompt(
            EVALUATOR_PROMPT,
            {
                "increment": increment.increment,
                "proof": increment.proof,
                "gates": json.dumps(iteration_report["gates"], ensure_ascii=False, indent=2),
                "generator_message": generator_message.read_text(encoding="utf-8", errors="replace"),
                "diff": diff_for_evaluation(),
            },
        )
        started = time.monotonic()
        try:
            evaluation = claude_structured(ROOT, evaluator_prompt, EVALUATOR_SCHEMA)
            evaluator_output.parent.mkdir(parents=True, exist_ok=True)
            evaluator_output.write_text(
                json.dumps(evaluation, ensure_ascii=False, indent=2) + "\n",
                encoding="utf-8",
            )
            evaluator_result = CommandResult(
                name="claude-evaluator",
                command=["claude", "-p", "--permission-mode", "plan"],
                returncode=0,
                duration_seconds=round(time.monotonic() - started, 3),
                output_file=str(evaluator_output.relative_to(ROOT)),
            )
        except (ActorError, OSError, ValueError) as exc:
            (iteration_dir / "evaluator-error.log").write_text(str(exc), encoding="utf-8")
            evaluator_result = CommandResult(
                name="claude-evaluator",
                command=["claude", "-p", "--permission-mode", "plan"],
                returncode=1,
                duration_seconds=round(time.monotonic() - started, 3),
                output_file=str((iteration_dir / "evaluator-error.log").relative_to(ROOT)),
            )
        iteration_report["evaluator"] = asdict(evaluator_result)
        if not evaluator_result.passed:
            feedback = "L'Evaluateur n'a pas produit de verdict exploitable. Corrige le lot et les preuves."
            iteration_report["verdict"] = "EVALUATOR_ERROR"
            report["iterations"].append(iteration_report)
            write_report(run_dir, report)
            continue
        evaluation = parse_evaluation(evaluator_output)
        iteration_report["evaluation"] = evaluation
        iteration_report["verdict"] = evaluation["verdict"]
        report["iterations"].append(iteration_report)
        if evaluation["verdict"] == "REJECT":
            feedback = "\n".join(evaluation["blocking_findings"]) or evaluation["summary"]
            write_report(run_dir, report)
            continue
        report["status"] = "PASS"
        report["completed_at"] = dt.datetime.now(dt.UTC).isoformat()
        if publishing:
            report["pull_request"] = publish(
                increment, config, branch, run_dir, evaluation
            )
            report["status"] = "PUBLISHED"
        write_report(run_dir, report)
        print(f"CITYLAB_FULL_AUTO_OK run={run_id} task={increment.task} order={increment.order:02d}")
        return 0
    report["status"] = "STUCK"
    report["completed_at"] = dt.datetime.now(dt.UTC).isoformat()
    write_report(run_dir, report)
    if publishing and shutil.which("gh"):
        with contextlib.suppress(PipelineError):
            run_capture(
                [
                    "gh", "issue", "create", "--title",
                    f"pipeline-stuck: {increment.task} increment {increment.order:02d}",
                    "--body", f"Le run `{run_id}` a epuise {config['max_iterations']} iterations.",
                    "--label", "pipeline-stuck",
                ]
            )
    raise PipelineError(
        f"pipeline bloque apres {config['max_iterations']} iterations; rapport: {run_dir}"
    )


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Orchestrateur full-auto Victoria CityLab")
    parser.add_argument("--config", type=Path, default=DEFAULT_CONFIG)
    parser.add_argument("--allow-dirty", action="store_true")
    parser.add_argument("--publish", action="store_true", help="branche, commit, push, PR et auto-merge")
    parser.add_argument("--dry-run", action="store_true", help="preflight et plan sans agent ni ecriture")
    return parser


def main(argv: Sequence[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    try:
        config = load_config(args.config)
        publishing = bool(args.publish)
        if publishing and not config["publish"].get("enabled"):
            raise PipelineError("publication desactivee dans config.json")
        with exclusive_lock():
            return execute(
                config, allow_dirty=args.allow_dirty, publishing=publishing, dry_run=args.dry_run
            )
    except PipelineError as exc:
        print(f"CITYLAB_FULL_AUTO_ERROR {exc}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
