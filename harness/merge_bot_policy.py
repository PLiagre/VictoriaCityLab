"""Politique de chemins de la fusion automatique."""

from __future__ import annotations

import fnmatch
from typing import Iterable


ALLOWLIST = (
    "Assets/CityLabHost/**",
    "Automation/Proofs/**",
    "Architecture/inbox/**",
    "Architecture/reviews/**",
    "Architecture/decisions/**",
    "Architecture/archive/**",
    "Architecture/audit-ledger.jsonl",
    "harness/queue/**",
    "hermes/DASHBOARD.md",
    "Packages/com.victoria.citymode.assets/**",
    "Packages/com.victoria.citymode.contracts/**",
    "Packages/com.victoria.citymode.presentation/**",
    "Packages/com.victoria.citymode/**",
    "ProjectSettings/**",
    "Docs/**",
    "Tools/**",
)

DENYLIST = (
    ".github/workflows/**",
    "harness/*.py",
    "harness/pipeline/**",
    "AGENTS.md",
    "Assets/Vendor/**",
    "ProjectSettings/ProjectVersion.txt",
    "Tools/check_roadmap.ps1",
)


def allowed_path(path: str) -> bool:
    normalized = path.replace("\\", "/")
    if any(fnmatch.fnmatchcase(normalized, pattern) for pattern in DENYLIST):
        return False
    return any(fnmatch.fnmatchcase(normalized, pattern) for pattern in ALLOWLIST)


def validate_paths(paths: Iterable[str]) -> tuple[bool, list[str]]:
    refused = sorted(path for path in paths if not allowed_path(path))
    return not refused, refused
