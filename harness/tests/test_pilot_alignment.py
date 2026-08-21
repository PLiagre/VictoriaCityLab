from __future__ import annotations

import json
import unittest
from pathlib import Path

from harness.pipeline.full_auto import load_config


ROOT = Path(__file__).resolve().parents[2]
WORKFLOWS = ROOT / ".github" / "workflows"


def _read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


class PilotAlignmentTests(unittest.TestCase):
    def test_repository_is_manual_without_auto_merge(self) -> None:
        config = load_config()
        self.assertEqual("manual", config["mode"])
        self.assertFalse(config["publish"]["auto_merge"])
        self.assertFalse(config["publish"]["enabled"])
        self.assertEqual("agent", config["branch_prefix"])

    def test_auto_policy_does_not_enable_auto_merge(self) -> None:
        policy = json.loads((ROOT / "harness" / "pipeline" / "auto_policy.json").read_text(
            encoding="utf-8"
        ))
        self.assertEqual("manual", policy["mode"])
        dumped = json.dumps(policy)
        self.assertNotIn("enable_auto_merge", dumped)
        self.assertNotIn("open_pr_and_enable_auto_merge", dumped)

    def test_personal_runner_workflows_are_dispatch_only(self) -> None:
        forbidden = ("pull_request:", "pull_request_target:")
        for name in (
            "unity-windows.yml",
            "full-auto.yml",
            "merge-bot.yml",
            "pipeline-audit.yml",
            "pipeline-verify.yml",
            "hermes-dashboard.yml",
            "hermes-daily.yml",
        ):
            text = _read(WORKFLOWS / name)
            for token in forbidden:
                self.assertNotIn(token, text, msg=f"{name} déclenche {token}")
            self.assertNotIn("gh pr merge", text, msg=f"{name} fusionne encore")

    def test_retired_producer_has_no_schedule(self) -> None:
        text = _read(WORKFLOWS / "full-auto.yml")
        self.assertIn("retired", text.lower())
        self.assertNotIn("schedule:", text)
        self.assertNotIn("run_full_auto.ps1", text)

    def test_daily_cron_is_read_only_on_github_hosted(self) -> None:
        text = _read(WORKFLOWS / "hermes-daily.yml")
        self.assertIn("schedule:", text)
        self.assertIn("ubuntu-latest", text)
        self.assertIn("hermes/crons/quotidien.sh", text)
        self.assertNotIn("self-hosted", text)
        self.assertNotIn("git push", text)

    def test_unity_worker_labels_and_manual_sha_gate(self) -> None:
        text = _read(WORKFLOWS / "unity-windows.yml")
        self.assertIn("name: unity-windows", text)
        self.assertIn("[self-hosted, windows, x64, unity]", text)
        self.assertIn("workflow_dispatch:", text)
        self.assertIn("run_unity_windows_worker.ps1", text)

    def test_hosted_ci_may_use_pull_request_but_not_a_personal_runner(self) -> None:
        text = _read(WORKFLOWS / "full-auto-ci.yml")
        self.assertIn("pull_request:", text)
        self.assertIn("windows-latest", text)
        self.assertNotIn("self-hosted", text)
        self.assertIn("Tools.tests.test_unity_windows_worker", text)

    def test_hermes_cron_refuses_merge_and_product_code(self) -> None:
        script = _read(ROOT / "hermes" / "crons" / "quotidien.sh")
        self.assertIn("Jamais de fusion", script)
        self.assertNotIn("git push", script)
        self.assertNotIn("gh pr merge", script)
        self.assertNotIn("forgepilot", script)

    def test_claude_guidance_exists_and_forbids_merge(self) -> None:
        text = _read(ROOT / "CLAUDE.md")
        self.assertIn("lecture seule", text.lower())
        self.assertIn("harness/queue/briefs/", text)
        self.assertIn("LocalCitySimulation", text)
        self.assertIn("ne fusionne pas", text.lower())


if __name__ == "__main__":
    unittest.main()
