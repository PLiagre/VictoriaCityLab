from __future__ import annotations

import io
import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import Mock, patch

from harness.pipeline.full_auto import (
    Increment,
    PipelineError,
    codex_command,
    dry_run_plan,
    load_config,
    parse_increments,
    requires_critical_audit,
    select_increment,
    validate_change_scope,
    write_console,
    run_streaming,
)


class FullAutoTests(unittest.TestCase):
    def test_parses_the_single_in_progress_increment(self) -> None:
        text = """
## Sessions Codex ordonnées
| Ordre | Suivi | Tâche | Incrément de session | Preuve de fermeture de l'incrément |
|---:|---|---|---|---|
| 01 | PROUVÉ | `M3-BUILD-01` | Echafaudages. | Tests. |
| 02 | EN_COURS | `M3-BUILD-01` | Usure et réparation. | Reload et HUD. |

## Suite
"""
        increments = parse_increments(text)
        self.assertEqual(2, len(increments))
        self.assertEqual("Usure et réparation.", increments[1].increment)

    def test_selection_fails_closed_on_multiple_in_progress_items(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            roadmap = Path(temp) / "ROADMAP.md"
            roadmap.write_text(
                "## Sessions Codex ordonnées\n"
                "| 01 | EN_COURS | `A-01` | A. | A. |\n"
                "| 02 | EN_COURS | `B-01` | B. | B. |\n",
                encoding="utf-8",
            )
            with self.assertRaises(PipelineError):
                select_increment(roadmap)

    def test_protected_paths_are_never_publishable(self) -> None:
        config = {
            "protected_prefixes": [".github/workflows/", "Assets/Vendor/"],
            "production_prefixes": [],
            "required_docs_for_production": [],
        }
        with self.assertRaises(PipelineError):
            validate_change_scope(["Assets/Vendor/source.fbx"], config)

    def test_production_requires_all_three_tracking_documents(self) -> None:
        config = {
            "protected_prefixes": [],
            "production_prefixes": ["Packages/"],
            "required_docs_for_production": [
                "Docs/ROADMAP.md",
                "Docs/PROTOTYPE_STATUS.md",
                "Docs/VALIDATION.md",
            ],
        }
        with self.assertRaises(PipelineError):
            validate_change_scope(["Packages/com.victoria.citymode/a.cs"], config)
        validate_change_scope(
            [
                "Packages/com.victoria.citymode/a.cs",
                "Docs/ROADMAP.md",
                "Docs/PROTOTYPE_STATUS.md",
                "Docs/VALIDATION.md",
            ],
            config,
        )

    def test_repository_config_is_full_auto_and_bounded(self) -> None:
        config = load_config()
        self.assertEqual("full_auto", config["mode"])
        self.assertGreaterEqual(config["max_iterations"], 1)
        self.assertLessEqual(config["max_iterations"], 3)
        self.assertTrue(config["publish"]["auto_merge"])
        self.assertEqual("pipeline/critical-audit", config["critical_audit"]["label"])
        self.assertIn("REL-SHIP", config["critical_audit"]["task_ids"])

    def test_dry_run_plan_exposes_kill_switches_and_roles(self) -> None:
        config = load_config()
        plan = dry_run_plan(
            Increment(2, "EN_COURS", "M3-BUILD-01", "Usure", "Tests"),
            config,
            True,
        )
        self.assertIn("independent-evaluator", plan["gates"])
        self.assertGreaterEqual(len(plan["kill_switches"]), 3)

    def test_cursor_audit_is_reserved_for_declared_critical_tasks(self) -> None:
        config = load_config()
        critical = Increment(3, "EN_COURS", "M3-FH-02", "Package", "Tests")
        routine = Increment(9, "EN_COURS", "M3-BUILD-01", "Usure", "Tests")
        self.assertTrue(requires_critical_audit(critical, config))
        self.assertFalse(requires_critical_audit(routine, config))
        self.assertEqual("critical", dry_run_plan(critical, config, True)["cursor_audit"])
        self.assertEqual("skipped", dry_run_plan(routine, config, True)["cursor_audit"])

    def test_critical_label_is_applied_before_auto_merge_label(self) -> None:
        source = (Path(__file__).resolve().parents[1] / "pipeline" / "full_auto.py").read_text(
            encoding="utf-8"
        )
        publish_source = source.split("def publish(", 1)[1].split("def write_report", 1)[0]
        self.assertLess(
            publish_source.index('"pipeline/critical-audit"'),
            publish_source.index('"pipeline/auto-merge"'),
        )

    def test_evaluator_schema_is_strict_json(self) -> None:
        schema_path = Path(__file__).resolve().parents[1] / "schemas" / "evaluator.schema.json"
        schema = json.loads(schema_path.read_text(encoding="utf-8"))
        self.assertFalse(schema["additionalProperties"])
        self.assertEqual(["PASS", "REJECT"], schema["properties"]["verdict"]["enum"])

    def test_approval_policy_is_a_global_codex_option(self) -> None:
        command = codex_command(
            model="gpt-5.6-sol",
            sandbox="read-only",
            output_file=Path("evaluation.json"),
        )
        self.assertEqual(
            ["codex", "--ask-for-approval", "never", "exec"], command[:4]
        )

    def test_console_output_survives_legacy_windows_encoding(self) -> None:
        raw = io.BytesIO()
        stream = io.TextIOWrapper(raw, encoding="cp1252")

        write_console("données → simulation", stream)
        stream.flush()

        self.assertEqual("données \\u2192 simulation", raw.getvalue().decode("cp1252"))

    def test_post_exit_console_interrupt_becomes_retryable_actor_failure(self) -> None:
        process = Mock()
        process.communicate.side_effect = KeyboardInterrupt
        process.poll.return_value = 0
        process.returncode = 0
        process.stdout = Mock()

        with tempfile.TemporaryDirectory() as temp, patch(
            "harness.pipeline.full_auto.ROOT", Path(temp)
        ), patch("harness.pipeline.full_auto.subprocess.Popen", return_value=process):
            result = run_streaming(
                "generator",
                ["actor"],
                stdin_text="prompt",
                timeout_seconds=10,
                output_file=Path(temp) / "actor.log",
            )

        self.assertEqual(130, result.returncode)
        process.stdout.close.assert_called_once_with()

    def test_live_actor_console_interrupt_is_not_swallowed(self) -> None:
        process = Mock()
        process.communicate.side_effect = KeyboardInterrupt
        process.poll.return_value = None

        with tempfile.TemporaryDirectory() as temp, patch(
            "harness.pipeline.full_auto.subprocess.Popen", return_value=process
        ), self.assertRaises(KeyboardInterrupt):
            run_streaming(
                "generator",
                ["actor"],
                stdin_text="prompt",
                timeout_seconds=10,
                output_file=Path(temp) / "actor.log",
            )


if __name__ == "__main__":
    unittest.main()
