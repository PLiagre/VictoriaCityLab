import copy
import unittest
from unittest import mock

from Tools import validate_city_mode_asset_port as validator


class CityModeAssetPortTests(unittest.TestCase):
    def test_repository_port_is_static_and_governed(self):
        result = validator.validate(require_proofs=False)
        self.assertEqual(11, result["assets"])
        self.assertEqual(3, result["partitions"])
        self.assertEqual(0, result["upstream_writes"])

    def test_changed_target_hash_is_rejected(self):
        real = validator._load_manifest()
        changed = copy.deepcopy(real)
        changed["assets"][0]["sha256"] = "0" * 64
        with mock.patch.object(validator, "_load_manifest", return_value=changed):
            with self.assertRaisesRegex(validator.AssetPortValidationError, "source hash drifted"):
                validator.validate(require_proofs=False)

    def test_reused_source_guid_is_rejected(self):
        real = validator._load_manifest()
        changed = copy.deepcopy(real)
        changed["assets"][0]["targetGuid"] = changed["assets"][0]["sourceGuid"]
        with mock.patch.object(validator, "_load_manifest", return_value=changed):
            with self.assertRaisesRegex(validator.AssetPortValidationError, "target GUID drifted|reused source GUID"):
                validator.validate(require_proofs=False)

    def test_partition_order_drift_is_rejected(self):
        real = validator._load_manifest()
        changed = copy.deepcopy(real)
        changed["loadOrder"] = ["common", "city", "biome"]
        with mock.patch.object(validator, "_load_manifest", return_value=changed):
            with self.assertRaisesRegex(validator.AssetPortValidationError, "load order drifted"):
                validator.validate(require_proofs=False)


if __name__ == "__main__":
    unittest.main()
