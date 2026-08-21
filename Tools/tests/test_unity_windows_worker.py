from __future__ import annotations

import tempfile
import unittest
from pathlib import Path

from Tools.unity_nunit import UnityNunitError, parse_nunit, write_summary


ROOT = Path(__file__).resolve().parents[2]
FIXTURES = Path(__file__).resolve().parent / "fixtures"


class UnityWindowsWorkerTests(unittest.TestCase):
    def test_green_xml_is_accepted(self) -> None:
        payload = parse_nunit(FIXTURES / "unity-nunit-green.xml")
        self.assertEqual("Passed", payload["result"])
        self.assertEqual(3, payload["total"])
        self.assertEqual(3, payload["passed"])
        self.assertEqual(0, payload["failed"])
        with tempfile.TemporaryDirectory() as temp:
            summary = Path(temp) / "summary.json"
            write_summary(payload, summary)
            self.assertTrue(summary.is_file())
            self.assertIn("unity-windows", summary.read_text(encoding="utf-8"))

    def test_red_xml_is_rejected(self) -> None:
        with self.assertRaises(UnityNunitError):
            parse_nunit(FIXTURES / "unity-nunit-red.xml")

    def test_empty_suite_is_rejected(self) -> None:
        with self.assertRaises(UnityNunitError):
            parse_nunit(FIXTURES / "unity-nunit-empty.xml")

    def test_missing_file_is_rejected(self) -> None:
        with self.assertRaises(UnityNunitError):
            parse_nunit(FIXTURES / "does-not-exist.xml")

    def test_worker_script_pins_unity_and_forbids_quit_with_run_tests(self) -> None:
        script = (ROOT / "Tools" / "run_unity_windows_worker.ps1").read_text(encoding="utf-8")
        self.assertIn("6000.0.43f1", script)
        self.assertIn("-runTests", script)
        self.assertIn("Assert-OwnedSha", script)
        self.assertNotIn("'-quit'", script)
        self.assertIn("run_unity_locked.py", script)

    def test_workflow_is_manual_on_personal_runner_and_named_unity_windows(self) -> None:
        workflow = (ROOT / ".github" / "workflows" / "unity-windows.yml").read_text(
            encoding="utf-8"
        )
        self.assertIn("name: unity-windows", workflow)
        self.assertIn("workflow_dispatch:", workflow)
        self.assertIn("self-hosted", workflow)
        self.assertIn("unity", workflow)
        self.assertIn("lfs: true", workflow)
        self.assertNotIn("pull_request:", workflow)
        self.assertNotIn("pull_request_target:", workflow)
        self.assertNotIn("schedule:", workflow)
        self.assertNotIn("gh pr merge", workflow)


if __name__ == "__main__":
    unittest.main()
