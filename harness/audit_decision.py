"""Politique deterministe de decision sur un contre-audit."""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path


VERDICT_RE = re.compile(r"^FINDING-(\d+)\s*:\s*(CONFIRMED|PARTIAL|REFUTED|NEEDS_OWNER)\s*$", re.MULTILINE)


def decide(review_text: str) -> dict[str, object]:
    verdicts = [(int(number), verdict) for number, verdict in VERDICT_RE.findall(review_text)]
    if not verdicts:
        raise ValueError("aucun verdict FINDING-n lisible")
    retained = [number for number, verdict in verdicts if verdict in {"CONFIRMED", "PARTIAL"}]
    if retained:
        return {"decision": "APPROVED", "retained_points": retained, "reason": "policy: confirmed-or-partial"}
    if all(verdict == "REFUTED" for _, verdict in verdicts):
        return {"decision": "REJECTED", "retained_points": [], "reason": "policy: all-refuted"}
    return {"decision": "REJECTED", "retained_points": [], "reason": "policy: no-owner-in-full-auto"}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("review", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    result = decide(args.review.read_text(encoding="utf-8"))
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

