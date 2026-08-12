"""Validation stdlib des audits Cursor et contre-audits Claude."""

from __future__ import annotations

import argparse
import re
from pathlib import Path


SHA_RE = re.compile(r"^[0-9a-f]{40}$")
AUDIT_ID_RE = re.compile(r"^CURSOR-[0-9]{8}T[0-9]{6}Z-[a-z0-9-]+$")


class SchemaError(RuntimeError):
    pass


def parse_frontmatter(path: Path) -> dict[str, str]:
    lines = path.read_text(encoding="utf-8").splitlines()
    if not lines or lines[0] != "---":
        raise SchemaError("frontmatter ouvrant absent")
    try:
        end = lines.index("---", 1)
    except ValueError as exc:
        raise SchemaError("frontmatter fermant absent") from exc
    values: dict[str, str] = {}
    for line in lines[1:end]:
        if not line.strip() or line.lstrip().startswith("#"):
            continue
        if ":" not in line:
            raise SchemaError(f"ligne frontmatter invalide: {line}")
        key, value = line.split(":", 1)
        values[key.strip()] = value.strip()
    return values


def validate_audit(path: Path) -> dict[str, str]:
    values = parse_frontmatter(path)
    required = {
        "audit_id", "auditor", "target_branch", "target_commit", "created_at",
        "audit_type", "status", "implementation_authorized",
        "ci_changes_authorized", "code_changes_authorized",
    }
    missing = sorted(required - values.keys())
    if missing:
        raise SchemaError(f"champs absents: {', '.join(missing)}")
    if values["audit_id"] != path.stem or not AUDIT_ID_RE.match(values["audit_id"]):
        raise SchemaError("audit_id invalide ou different du nom de fichier")
    if values["auditor"] not in {"cursor-agent", "cursor-cloud"}:
        raise SchemaError("auditor doit identifier Cursor")
    if not SHA_RE.match(values["target_commit"]):
        raise SchemaError("target_commit doit etre un SHA complet")
    if values["status"] != "PROPOSED":
        raise SchemaError("un audit entrant doit etre PROPOSED")
    for field in ("implementation_authorized", "ci_changes_authorized", "code_changes_authorized"):
        if values[field].lower() != "false":
            raise SchemaError(f"{field} doit valoir false")
    return values


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("audit", type=Path)
    args = parser.parse_args()
    values = validate_audit(args.audit)
    print(f"CITYLAB_AUDIT_SCHEMA_OK audit_id={values['audit_id']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

