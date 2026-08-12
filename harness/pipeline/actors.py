"""Adaptateurs locaux pour Hermes, Cursor, Codex et Claude."""

from __future__ import annotations

import json
import os
import re
import subprocess
from pathlib import Path
from typing import Any, Sequence


class ActorError(RuntimeError):
    pass


def run(command: Sequence[str], *, cwd: Path, timeout: int = 900) -> str:
    result = subprocess.run(
        list(command), cwd=cwd, text=True, encoding="utf-8", errors="replace",
        stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=timeout, check=False,
    )
    if result.returncode:
        raise ActorError(f"acteur en echec ({result.returncode}): {' '.join(command[:3])}\n{result.stdout}")
    return result.stdout.strip()


def extract_json(text: str) -> dict[str, Any]:
    candidates = [text.strip()]
    candidates.extend(re.findall(r"\{(?:[^{}]|\{[^{}]*\})*\}", text, flags=re.DOTALL))
    for candidate in reversed(candidates):
        try:
            value = json.loads(candidate)
        except json.JSONDecodeError:
            continue
        if isinstance(value, dict):
            if isinstance(value.get("structured_output"), dict):
                return value["structured_output"]
            if isinstance(value.get("result"), str):
                try:
                    nested = json.loads(value["result"])
                    if isinstance(nested, dict):
                        return nested
                except json.JSONDecodeError:
                    pass
            return value
    raise ActorError("aucun objet JSON lisible dans la sortie acteur")


def hermes_plan(root: Path, prompt: str) -> str:
    profile = os.environ.get("CITYLAB_HERMES_PROFILE", "citylab-local-orchestrator")
    return run(["hermes", "-p", profile, "-z", prompt], cwd=root, timeout=600)


def cursor_audit(root: Path, prompt: str) -> dict[str, Any]:
    output = run(
        [
            "agent", "--print", "--mode", "plan", "--trust", "--workspace", str(root),
            "--output-format", "text", prompt,
        ],
        cwd=root,
        timeout=1200,
    )
    value = extract_json(output)
    if value.get("verdict") not in {"PASS", "REJECT"}:
        raise ActorError("Cursor n'a pas rendu un verdict PASS/REJECT")
    value["raw_output"] = output
    return value


def codex_generate(root: Path, prompt: str, schema_path: Path) -> dict[str, Any]:
    output_path = root / "Logs" / "FullAuto" / "codex-proof-output.json"
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output = run(
        [
            "codex", "--ask-for-approval", "never", "exec", "-C", str(root),
            "--model", os.environ.get("CODEX_MODEL", "gpt-5.6-sol"),
            "--sandbox", "read-only", "--output-schema", str(schema_path),
            "--output-last-message", str(output_path), prompt,
        ],
        cwd=root,
        timeout=1200,
    )
    value = json.loads(output_path.read_text(encoding="utf-8"))
    value["raw_output"] = output
    return value


def claude_structured(root: Path, prompt: str, schema_path: Path) -> dict[str, Any]:
    schema = json.loads(schema_path.read_text(encoding="utf-8"))
    schema.pop("$schema", None)
    output = run(
        [
            "claude", "-p", "--permission-mode", "plan", "--output-format", "json",
            "--json-schema", json.dumps(schema, separators=(",", ":")), prompt,
        ],
        cwd=root,
        timeout=1200,
    )
    return extract_json(output)
