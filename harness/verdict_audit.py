"""Porte mecanique d'un brief produit par la boucle multi-acteurs."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path


class VerdictError(RuntimeError):
    pass


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    digest.update(path.read_bytes())
    return digest.hexdigest()


def validate_brief(brief_dir: Path) -> list[str]:
    required = [
        brief_dir / "brief.md",
        brief_dir / "eval-rubric.md",
        brief_dir / "deliverables" / "manifest.json",
        brief_dir / "deliverables" / "generator-log.md",
        brief_dir / "verdict.md",
    ]
    missing = [str(path) for path in required if not path.is_file()]
    if missing:
        raise VerdictError("fichiers absents: " + ", ".join(missing))
    manifest = json.loads(required[2].read_text(encoding="utf-8"))
    if manifest.get("producer") != "codex":
        raise VerdictError("producer doit valoir codex")
    if manifest.get("evaluator") != "claude":
        raise VerdictError("evaluator doit valoir claude")
    if manifest.get("producer_session") == manifest.get("evaluator_session"):
        raise VerdictError("producteur et evaluateur ne peuvent partager la meme session")
    evidence = manifest.get("evidence", [])
    if not evidence:
        raise VerdictError("aucune preuve declaree")
    checks: list[str] = []
    for item in evidence:
        path = brief_dir / item["path"]
        if not path.is_file():
            raise VerdictError(f"preuve absente: {path}")
        if sha256(path) != item["sha256"]:
            raise VerdictError(f"hash invalide: {path}")
        checks.append(str(path))
    verdict = required[4].read_text(encoding="utf-8")
    if "Verdict: PASS" not in verdict:
        raise VerdictError("verdict independant PASS absent")
    return checks


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("brief_dir", type=Path)
    args = parser.parse_args()
    checks = validate_brief(args.brief_dir)
    print(f"CITYLAB_VERDICT_ACCEPT evidence={len(checks)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

