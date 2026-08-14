import copy
import unittest
from unittest import mock

from Tools import validate_unity_render_convergence as validator


class UnityRenderConvergenceTests(unittest.TestCase):
    def test_repository_decision_is_valid(self):
        result = validator.validate()
        self.assertEqual("urp", result["pipeline"])
        self.assertEqual(3, result["map_goldens"])
        self.assertEqual(0, result["upstream_writes"])

    def test_changed_urp_golden_is_rejected(self):
        real_load = validator._load_json

        def changed_load(path):
            document = real_load(path)
            if path == validator.DECISION_PATH:
                document = copy.deepcopy(document)
                document["probe"]["mapGoldens"][0]["urpSha256"] = "0" * 64
            return document

        with mock.patch.object(validator, "_load_json", side_effect=changed_load):
            with self.assertRaisesRegex(validator.ConvergenceValidationError, "map golden changed"):
                validator.validate()

    def test_magenta_capture_is_rejected(self):
        real_load = validator._load_json

        def changed_load(path):
            document = real_load(path)
            if path == validator.DECISION_PATH:
                document = copy.deepcopy(document)
                document["probe"]["cityCapture"]["magentaPixels"] = 1
            return document

        with mock.patch.object(validator, "_load_json", side_effect=changed_load):
            with self.assertRaisesRegex(validator.ConvergenceValidationError, "contains magenta"):
                validator.validate()


if __name__ == "__main__":
    unittest.main()
