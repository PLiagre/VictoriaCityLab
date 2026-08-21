"""Parseur NUnit du worker Unity Windows.

Refuse un XML absent, illisible, vide, ou qui contient un échec. Cette
porte tourne sans Unity : les preuves rouge/verte vivent dans
`Tools/tests/test_unity_windows_worker.py`.
"""

from __future__ import annotations

import json
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Any


class UnityNunitError(RuntimeError):
    """Résultat EditMode absent, vide ou rouge."""


def _int_attr(node: ET.Element, *names: str) -> int | None:
    for name in names:
        raw = node.attrib.get(name)
        if raw is None or raw == "":
            continue
        try:
            return int(raw)
        except ValueError as exc:
            raise UnityNunitError(f"attribut {name} non entier: {raw}") from exc
    return None


def parse_nunit(path: Path) -> dict[str, Any]:
    if not path.is_file():
        raise UnityNunitError(f"résultats Unity absents: {path}")
    try:
        root = ET.fromstring(path.read_text(encoding="utf-8"))
    except (OSError, ET.ParseError) as exc:
        raise UnityNunitError(f"XML NUnit illisible: {path}: {exc}") from exc

    if root.tag not in {"test-run", "test-results"}:
        raise UnityNunitError(f"racine NUnit inattendue: {root.tag}")

    total = _int_attr(root, "total", "testcasecount")
    passed = _int_attr(root, "passed")
    failed = _int_attr(root, "failed", "failures")
    skipped = _int_attr(root, "skipped", "not-run")
    result = root.attrib.get("result", "")

    if total is None:
        total = len(root.findall(".//test-case"))
    if failed is None:
        failed = sum(
            1
            for case in root.findall(".//test-case")
            if case.attrib.get("result", "").lower() in {"failed", "error"}
        )
    if passed is None:
        passed = sum(
            1
            for case in root.findall(".//test-case")
            if case.attrib.get("result", "").lower() == "passed"
        )
    if skipped is None:
        skipped = 0

    if total <= 0:
        raise UnityNunitError("suite EditMode vide")
    if failed > 0 or result.lower() == "failed":
        raise UnityNunitError(
            f"EditMode rouge total={total} passed={passed} failed={failed}"
        )
    if passed <= 0:
        raise UnityNunitError("aucun test EditMode réussi")

    return {
        "check": "unity-windows",
        "result": "Passed",
        "total": total,
        "passed": passed,
        "failed": failed,
        "skipped": skipped,
        "source": str(path),
    }


def write_summary(payload: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2, sort_keys=True) + "\n", encoding="utf-8")


def main(argv: list[str] | None = None) -> int:
    import argparse

    parser = argparse.ArgumentParser(description="Valide un XML EditMode Unity.")
    parser.add_argument("xml", type=Path)
    parser.add_argument("--summary", type=Path, required=True)
    args = parser.parse_args(argv)
    payload = parse_nunit(args.xml)
    write_summary(payload, args.summary)
    print(
        "CITYLAB_UNITY_WINDOWS_OK "
        f"total={payload['total']} passed={payload['passed']} failed={payload['failed']}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
