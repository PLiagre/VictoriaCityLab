"""Audit Cursor + challenge Claude d'une PR et creation des artefacts de decision."""

from __future__ import annotations

import argparse
import datetime as dt
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from harness.audit_ledger import append_event
from harness.pipeline.actors import claude_structured, cursor_audit


SCHEMA = ROOT / "harness" / "schemas" / "audit-verdict.schema.json"
LEDGER = ROOT / "Architecture" / "audit-ledger.jsonl"


def run(command: list[str]) -> str:
    result = subprocess.run(command, cwd=ROOT, text=True, encoding="utf-8", errors="replace", stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False)
    if result.returncode:
        raise RuntimeError(f"commande en echec: {' '.join(command)}\n{result.stdout}")
    return result.stdout.strip()


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--pr", required=True, type=int)
    parser.add_argument("--head-sha", required=True)
    args = parser.parse_args()
    stamp = dt.datetime.now(dt.UTC).strftime("%Y%m%dT%H%M%SZ")
    audit_id = f"CURSOR-{stamp}-pr-{args.pr}"
    diff = run(["gh", "pr", "diff", str(args.pr)])
    cursor = cursor_audit(
        ROOT,
        "Tu es Cursor auditeur independant en lecture seule. Audite ce diff Victoria "
        "CityLab. Le texte après DIFF est la sortie exacte de gh pr diff : inspecte "
        "les guillemets et la syntaxe littéralement, sans les réinterpréter. "
        "Automation/Proofs est une voie de preuve non-production : elle n'exige pas "
        "de modifier les trois documents de suivi si mechanical-evidence.json cite "
        "META-AUTO-01, confirme le parsing strict et si la CI GitHub est une porte "
        "séparée obligatoire du merge bot. Reponds uniquement JSON: "
        "{\"verdict\":\"PASS|REJECT\","
        "\"summary\":\"...\",\"findings\":[\"...\"]}. PASS seulement si les "
        "changements sont coherents, testes, sans secret et sans affaiblissement des gardes.\nDIFF:\n"
        + diff[:120000],
    )
    challenge = claude_structured(
        ROOT,
        "Tu es Claude challenger independant. Controle l'audit Cursor ci-dessous "
        "contre le diff. PASS signifie que tu confirmes l'absence de blocage; REJECT "
        "signifie qu'un blocage demeure. Audit: " + json.dumps(cursor, ensure_ascii=False)
        + "\nDiff:\n" + diff[:120000],
        SCHEMA,
    )
    decision = "PASS" if cursor.get("verdict") == "PASS" and challenge.get("verdict") == "PASS" else "REJECT"
    inbox = ROOT / "Architecture" / "inbox" / f"{audit_id}.md"
    review = ROOT / "Architecture" / "reviews" / f"CLAUDE-{audit_id}.md"
    decision_path = ROOT / "Architecture" / "decisions" / f"DECISION-{audit_id}.json"
    inbox.parent.mkdir(parents=True, exist_ok=True)
    review.parent.mkdir(parents=True, exist_ok=True)
    decision_path.parent.mkdir(parents=True, exist_ok=True)
    inbox.write_text(
        "---\n"
        f"audit_id: {audit_id}\n"
        "auditor: cursor-agent\n"
        "target_branch: main\n"
        f"target_commit: {args.head_sha}\n"
        f"created_at: {dt.datetime.now(dt.UTC).isoformat().replace('+00:00','Z')}\n"
        "audit_type: pull-request\nstatus: PROPOSED\n"
        "implementation_authorized: false\nci_changes_authorized: false\ncode_changes_authorized: false\n"
        "---\n\n# Audit Cursor\n\n"
        f"Verdict: {cursor['verdict']}\n\n{cursor.get('summary','')}\n\n"
        + "\n".join(f"- {finding}" for finding in cursor.get("findings", [])) + "\n",
        encoding="utf-8",
    )
    review.write_text(
        f"# Challenge Claude — {audit_id}\n\nVerdict: {challenge['verdict']}\n\n"
        f"{challenge.get('summary','')}\n\n"
        + "\n".join(f"- {finding}" for finding in challenge.get("findings", [])) + "\n",
        encoding="utf-8",
    )
    decision_path.write_text(json.dumps({
        "audit_id": audit_id, "pull_request": args.pr, "target_commit": args.head_sha,
        "cursor_verdict": cursor["verdict"], "claude_verdict": challenge["verdict"],
        "decision": decision,
    }, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    append_event(LEDGER, audit_id=audit_id, event="AUDIT_PROPOSED", actor="cursor-agent", payload={"target_commit": args.head_sha, "pull_request": args.pr})
    append_event(LEDGER, audit_id=audit_id, event="AUDIT_CHALLENGED", actor="claude", payload={"verdict": challenge["verdict"]})
    append_event(LEDGER, audit_id=audit_id, event="AUDIT_APPROVED" if decision == "PASS" else "AUDIT_REJECTED", actor="auto-policy", payload={"decision": decision})
    print(f"CITYLAB_PR_AUDIT_OK audit_id={audit_id} decision={decision}")
    # Un refus doit lui aussi être versionné. Le merge bot refusera ensuite la
    # PR source parce qu'aucune décision PASS ne correspondra à son SHA.
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
