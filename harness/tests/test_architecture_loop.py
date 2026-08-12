from __future__ import annotations

import hashlib
import json
import tempfile
import unittest
from unittest import mock
from pathlib import Path

from harness.audit_decision import decide
from harness.audit_ledger import LedgerError, append_event, read_events, validate_events
from harness.audit_schema import SchemaError, validate_audit
from harness.merge_bot_policy import validate_paths
from harness.pipeline.supervisor import Supervisor
from harness.pipeline.actors import resolve_command
from harness.verdict_audit import validate_brief


class ArchitectureLoopTests(unittest.TestCase):
    def test_windows_wrappers_are_resolved_without_a_shell(self) -> None:
        with mock.patch("harness.pipeline.actors.os.name", "nt"), mock.patch(
            "harness.pipeline.actors.shutil.which",
            side_effect=lambda value: {
                "codex.cmd": r"C:\Npm\codex.cmd",
                "node.exe": r"C:\Node\node.exe",
                "agent": r"C:\Cursor\agent.CMD",
                "powershell.exe": r"C:\Windows\powershell.exe",
            }.get(value),
        ), mock.patch("harness.pipeline.actors.Path.exists", return_value=True):
            self.assertEqual(r"C:\Node\node.exe", resolve_command(["codex", "exec"])[0])
            self.assertIn("codex.js", resolve_command(["codex", "exec"])[1])
            cursor = resolve_command(["agent", "--print", "prompt"])
            self.assertEqual(r"C:\Windows\powershell.exe", cursor[0])
            self.assertIn("cursor-agent.ps1", cursor[5])

    def test_full_audit_state_machine(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            ledger = Path(temp) / "ledger.jsonl"
            audit_id = "CURSOR-20260812T120000Z-demo"
            for event, actor in [
                ("AUDIT_PROPOSED", "cursor"),
                ("AUDIT_CHALLENGED", "claude"),
                ("AUDIT_APPROVED", "policy"),
                ("AUDIT_CONVERTED", "orchestrator"),
                ("AUDIT_IMPLEMENTED", "codex"),
                ("AUDIT_VERIFIED", "github-actions"),
                ("AUDIT_ARCHIVED", "orchestrator"),
            ]:
                append_event(ledger, audit_id=audit_id, event=event, actor=actor)
            events = read_events(ledger)
            validate_events(events)
            self.assertEqual(7, len(events))

    def test_ledger_rejects_skipped_challenge(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            ledger = Path(temp) / "ledger.jsonl"
            audit_id = "CURSOR-20260812T120000Z-demo"
            append_event(ledger, audit_id=audit_id, event="AUDIT_PROPOSED", actor="cursor")
            with self.assertRaises(LedgerError):
                append_event(ledger, audit_id=audit_id, event="AUDIT_APPROVED", actor="policy")

    def test_cursor_schema_is_fail_closed(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp) / "CURSOR-20260812T120000Z-demo.md"
            path.write_text(
                "---\n"
                "audit_id: CURSOR-20260812T120000Z-demo\n"
                "auditor: cursor-agent\n"
                "target_branch: main\n"
                f"target_commit: {'a' * 40}\n"
                "created_at: 2026-08-12T12:00:00Z\n"
                "audit_type: architecture-and-qa\n"
                "status: PROPOSED\n"
                "implementation_authorized: false\n"
                "ci_changes_authorized: false\n"
                "code_changes_authorized: false\n"
                "---\n\n# Audit\n",
                encoding="utf-8",
            )
            self.assertEqual("cursor-agent", validate_audit(path)["auditor"])
            path.write_text(path.read_text(encoding="utf-8").replace("code_changes_authorized: false", "code_changes_authorized: true"), encoding="utf-8")
            with self.assertRaises(SchemaError):
                validate_audit(path)

    def test_policy_decision_is_deterministic(self) -> None:
        self.assertEqual("APPROVED", decide("FINDING-1: CONFIRMED")["decision"])
        self.assertEqual("REJECTED", decide("FINDING-1: REFUTED")["decision"])
        self.assertEqual("REJECTED", decide("FINDING-1: NEEDS_OWNER")["decision"])

    def test_merge_policy_allows_production_and_evidence_lanes(self) -> None:
        ok, refused = validate_paths([
            "Automation/Proofs/a.json",
            "Architecture/inbox/a.md",
            "Packages/com.victoria.citymode/Runtime/Simulation/Foo.cs",
            "Assets/CityLabHost/Adapted/foo.prefab",
            "Docs/VALIDATION.md",
        ])
        self.assertTrue(ok)
        self.assertEqual([], refused)
        ok, refused = validate_paths([".github/workflows/pwn.yml"])
        self.assertFalse(ok)
        self.assertEqual([".github/workflows/pwn.yml"], refused)
        ok, refused = validate_paths(["Packages/com.victoria.citymode/Tests/EditMode/Bypass.cs"])
        self.assertFalse(ok)
        self.assertEqual(["Packages/com.victoria.citymode/Tests/EditMode/Bypass.cs"], refused)

    def test_supervisor_stops_budget_and_plateau(self) -> None:
        supervisor = Supervisor(max_iterations=3, plateau_limit=2)
        self.assertEqual("CONTINUE", supervisor.register("a"))
        self.assertEqual("STOP_PLATEAU", supervisor.register("a"))
        supervisor = Supervisor(max_iterations=2, plateau_limit=3)
        self.assertEqual("CONTINUE", supervisor.register("a"))
        self.assertEqual("STOP_BUDGET", supervisor.register("b"))

    def test_mechanical_verdict_checks_actor_separation_and_hash(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            brief = Path(temp)
            (brief / "deliverables").mkdir()
            (brief / "brief.md").write_text("brief", encoding="utf-8")
            (brief / "eval-rubric.md").write_text("rubric", encoding="utf-8")
            evidence = brief / "deliverables" / "proof.json"
            evidence.write_text("{}\n", encoding="utf-8")
            digest = hashlib.sha256(evidence.read_bytes()).hexdigest()
            (brief / "deliverables" / "generator-log.md").write_text("codex", encoding="utf-8")
            (brief / "deliverables" / "manifest.json").write_text(
                json.dumps({
                    "producer": "codex", "evaluator": "claude",
                    "producer_session": "codex-1", "evaluator_session": "claude-1",
                    "evidence": [{"path": "deliverables/proof.json", "sha256": digest}],
                }), encoding="utf-8",
            )
            (brief / "verdict.md").write_text("Verdict: PASS\n", encoding="utf-8")
            self.assertEqual([str(evidence)], validate_brief(brief))

    def test_proof_lane_is_automergeable_but_not_production(self) -> None:
        ok, refused = validate_paths([
            "Automation/Proofs/CITYLAB-20260812T120000Z/mechanical-evidence.json"
        ])
        self.assertTrue(ok)
        self.assertEqual([], refused)


if __name__ == "__main__":
    unittest.main()
