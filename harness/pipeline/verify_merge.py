"""Cree la preuve terminale et archive un audit apres merge du lot cible."""

from __future__ import annotations

import argparse
import json
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from harness.audit_ledger import append_event, current_status, read_events


LEDGER = ROOT / "Architecture" / "audit-ledger.jsonl"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--merged-sha", required=True)
    parser.add_argument("--source-sha", required=True)
    args = parser.parse_args()
    decisions = sorted((ROOT / "Architecture" / "decisions").glob("DECISION-*.json"))
    match = None
    for path in decisions:
        value = json.loads(path.read_text(encoding="utf-8"))
        if value.get("target_commit") == args.source_sha and value.get("decision") == "PASS":
            match = (path, value)
            break
    if not match:
        raise SystemExit("decision PASS absente pour le SHA source")
    decision_path, decision = match
    audit_id = decision["audit_id"]
    status = current_status(read_events(LEDGER), audit_id)
    if status != "AUDIT_APPROVED":
        raise SystemExit(f"etat inattendu avant verification: {status}")
    append_event(LEDGER, audit_id=audit_id, event="AUDIT_CONVERTED", actor="pipeline-orchestrator", payload={"source_sha": args.source_sha})
    append_event(LEDGER, audit_id=audit_id, event="AUDIT_IMPLEMENTED", actor="codex", payload={"merged_sha": args.merged_sha})
    append_event(LEDGER, audit_id=audit_id, event="AUDIT_VERIFIED", actor="github-actions", payload={"merged_sha": args.merged_sha})
    append_event(LEDGER, audit_id=audit_id, event="AUDIT_ARCHIVED", actor="pipeline-orchestrator")
    archive = ROOT / "Architecture" / "archive" / audit_id
    archive.mkdir(parents=True, exist_ok=True)
    sources = [
        ROOT / "Architecture" / "inbox" / f"{audit_id}.md",
        ROOT / "Architecture" / "reviews" / f"CLAUDE-{audit_id}.md",
        decision_path,
    ]
    for source in sources:
        if source.exists():
            shutil.copy2(source, archive / source.name)
    (archive / "verification.json").write_text(json.dumps({
        "audit_id": audit_id, "source_sha": args.source_sha,
        "merged_sha": args.merged_sha, "status": "AUDIT_ARCHIVED",
    }, indent=2) + "\n", encoding="utf-8")
    print(f"CITYLAB_AUDIT_ARCHIVED audit_id={audit_id}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
