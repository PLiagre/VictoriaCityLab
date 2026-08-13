from __future__ import annotations

import copy
import unittest

from Tools.validate_forgehistory_city_mode_contract import (
    ContractValidationError,
    EXAMPLES_PATH,
    SCHEMA_PATH,
    load_json,
    validate_document,
    validate_examples,
    validate_repository,
)


class ForgeHistoryCityModeContractTests(unittest.TestCase):
    def setUp(self) -> None:
        self.schema = load_json(SCHEMA_PATH)
        self.examples = load_json(EXAMPLES_PATH)

    def test_repository_contract_is_self_consistent(self) -> None:
        validate_repository()

    def test_contract_assembly_and_host_lifecycle_are_explicit(self) -> None:
        validate_repository()

    def test_examples_match_versioned_schema(self) -> None:
        validate_examples(self.schema, self.examples)

    def test_incomplete_launch_context_is_rejected(self) -> None:
        context = copy.deepcopy(self.examples["launchContext"])
        del context["cityId"]
        with self.assertRaises(ContractValidationError):
            validate_document("CityLaunchContext", context, self.schema)

    def test_incoherent_time_policy_is_rejected(self) -> None:
        context = copy.deepcopy(self.examples["launchContext"])
        context["worldTimeScalePermille"] = 1000
        with self.assertRaises(ContractValidationError):
            validate_document("CityLaunchContext", context, self.schema)

    def test_negative_revision_is_rejected(self) -> None:
        intent = copy.deepcopy(self.examples["intent"])
        intent["expectedStateRevision"] = -1
        with self.assertRaises(ContractValidationError):
            validate_document("CityIntentEnvelope", intent, self.schema)

    def test_bad_snapshot_hash_is_rejected_by_coherence_gate(self) -> None:
        examples = copy.deepcopy(self.examples)
        examples["snapshot"]["payloadSha256"] = "0" * 64
        with self.assertRaises(ContractValidationError):
            validate_examples(self.schema, examples)

    def test_accepted_receipt_with_error_is_rejected(self) -> None:
        receipt = copy.deepcopy(self.examples["acceptedReceipt"])
        receipt["errorCode"] = 10
        with self.assertRaises(ContractValidationError):
            validate_document("CityIntentReceipt", receipt, self.schema)


if __name__ == "__main__":
    unittest.main()
