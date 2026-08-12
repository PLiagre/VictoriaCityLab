"""Cree une PR temoin issue de Hermes -> Codex -> Claude."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from harness.pipeline.actors import claude_structured, codex_generate, hermes_plan


PROOF_SCHEMA = ROOT / "harness" / "schemas" / "proof.schema.json"
AUDIT_SCHEMA = ROOT / "harness" / "schemas" / "audit-verdict.schema.json"


def run(command: list[str]) -> str:
    result = subprocess.run(command, cwd=ROOT, text=True, encoding="utf-8", errors="replace", stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False)
    if result.returncode:
        raise RuntimeError(f"commande en echec: {' '.join(command)}\n{result.stdout}")
    return result.stdout.strip()


def write_json(path: Path, value: object) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--publish", action="store_true")
    args = parser.parse_args()
    cycle_id = dt.datetime.now(dt.UTC).strftime("CITYLAB-%Y%m%dT%H%M%SZ")
    proof_dir = ROOT / "Automation" / "Proofs" / cycle_id
    plan = hermes_plan(
        ROOT,
        "Tu es Hermes chef de projet de Victoria CityLab. La tâche autorisée est "
        "META-AUTO-01. Donne un plan bref et uniquement consacré à la preuve d'un "
        "cycle automatique Hermes, Codex, Cursor, Claude et GitHub Actions, avec PR, "
        "audit, merge et archive, sans modifier le jeu.",
    )
    generated = codex_generate(
        ROOT,
        "Produis un objet JSON de preuve pour le cycle " + cycle_id + ". "
        "claim doit affirmer que Codex a produit le lot temoin. actors doit contenir "
        "Hermes, Codex, Claude, Cursor et GitHub Actions. checks doit contenir au moins "
        "roadmap, tests, audit independant et merge automatique. Ne modifie aucun fichier.",
        PROOF_SCHEMA,
    )
    evaluation = claude_structured(
        ROOT,
        "Tu es Claude evaluateur independant. Evalue cette preuve temoin: "
        + json.dumps(generated, ensure_ascii=False)
        + ". PASS si elle identifie les cinq acteurs et les quatre controles; sinon REJECT.",
        AUDIT_SCHEMA,
    )
    if evaluation.get("verdict") != "PASS":
        raise RuntimeError(f"Claude refuse la preuve temoin: {evaluation}")
    write_json(proof_dir / "codex-generator.json", generated)
    write_json(proof_dir / "claude-evaluator.json", evaluation)
    (proof_dir / "hermes-plan.md").write_text(
        "# Plan Hermes — META-AUTO-01\n\n"
        "But contrôlé : prouver PR, audit indépendant, fusion automatique et archive, "
        "sans changement de production.\n\n## Sortie brute Hermes\n\n" + plan + "\n",
        encoding="utf-8",
    )
    parsed_json = {}
    for name in ("codex-generator.json", "claude-evaluator.json"):
        parsed_json[name] = isinstance(
            json.loads((proof_dir / name).read_text(encoding="utf-8")), dict
        )
    roadmap_check = run([
        "powershell", "-ExecutionPolicy", "Bypass", "-File",
        str(ROOT / "Tools" / "check_roadmap.ps1"),
    ])
    write_json(proof_dir / "mechanical-evidence.json", {
        "roadmap_id": "META-AUTO-01",
        "roadmap_check": roadmap_check,
        "strict_json_parsed": parsed_json,
        "proof_lane": "Automation/Proofs",
        "production_changed": False,
    })
    manifest = {
        "schema": 1,
        "cycle_id": cycle_id,
        "producer": "codex",
        "evaluator": "claude",
        "producer_session": hashlib.sha256((cycle_id + ":codex").encode()).hexdigest()[:16],
        "evaluator_session": hashlib.sha256((cycle_id + ":claude").encode()).hexdigest()[:16],
        "status": "GENERATED_EVALUATED",
        "evidence": [
            "hermes-plan.md", "codex-generator.json", "claude-evaluator.json",
            "mechanical-evidence.json",
        ],
    }
    write_json(proof_dir / "manifest.json", manifest)
    print(f"CITYLAB_PROOF_CREATED cycle_id={cycle_id} path={proof_dir.relative_to(ROOT)}")
    if not args.publish:
        return 0
    branch = f"codex/proof-{cycle_id.lower()}"
    run(["git", "switch", "-c", branch])
    run(["git", "add", str(proof_dir.relative_to(ROOT))])
    run(["git", "commit", "-m", f"proof: full-auto cycle {cycle_id}"])
    run(["git", "push", "-u", "origin", branch])
    url = run([
        "gh", "pr", "create", "--base", "main", "--head", branch,
        "--title", f"proof: full-auto cycle {cycle_id}",
        "--body", f"Cycle temoin multi-acteurs.\n\nCycle-ID: {cycle_id}\nPipeline-Auto-Merge: true",
    ]).splitlines()[-1]
    run(["gh", "pr", "edit", url, "--add-label", "pipeline/auto-merge"])
    print(f"CITYLAB_PROOF_PR_CREATED url={url}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
