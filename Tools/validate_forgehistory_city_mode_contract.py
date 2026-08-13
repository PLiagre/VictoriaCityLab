#!/usr/bin/env python3
"""Offline validation for the ForgeHistory ↔ City Mode v1 boundary."""

from __future__ import annotations

import hashlib
import json
import re
import sys
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
SCHEMA_PATH = ROOT / "Docs/Integration/Schemas/forgehistory-city-mode-v1.schema.json"
EXAMPLES_PATH = ROOT / "Docs/Integration/Schemas/forgehistory-city-mode-v1.examples.json"
CONTRACT_PATH = ROOT / "Packages/com.victoria.citymode.contracts/Runtime/ForgeHistoryCityModeContracts.cs"
CONTRACT_ASMDEF_PATH = ROOT / "Packages/com.victoria.citymode.contracts/Runtime/Victoria.CityMode.Contracts.asmdef"
CONTRACT_PACKAGE_PATH = ROOT / "Packages/com.victoria.citymode.contracts/package.json"
BOOTSTRAP_PATH = ROOT / "Packages/com.victoria.citymode/Runtime/CityLabBootstrap.cs"
LAB_SCENE_PATH = ROOT / "Assets/CityLabHost/Scenes/CityLab.unity"
DOCUMENT_PATH = ROOT / "Docs/Integration/FORGEHISTORY_CITY_MODE_CONTRACT.md"
PINNED_FORGEHISTORY_SHA = "268e8aab151452b0c740a44a7cc97ca3fd37e311"


class ContractValidationError(ValueError):
    pass


def load_json(path: Path) -> dict[str, Any]:
    with path.open(encoding="utf-8") as stream:
        value = json.load(stream)
    if not isinstance(value, dict):
        raise ContractValidationError(f"{path}: expected a JSON object")
    return value


def _matches_type(value: Any, expected: str) -> bool:
    if expected == "object":
        return isinstance(value, dict)
    if expected == "string":
        return isinstance(value, str)
    if expected == "integer":
        return isinstance(value, int) and not isinstance(value, bool)
    if expected == "boolean":
        return isinstance(value, bool)
    raise ContractValidationError(f"unsupported schema type: {expected}")


def _validate_constraints(value: Any, constraints: dict[str, Any], path: str) -> None:
    expected_type = constraints.get("type")
    if expected_type and not _matches_type(value, expected_type):
        raise ContractValidationError(f"{path}: expected {expected_type}")
    if "const" in constraints and value != constraints["const"]:
        raise ContractValidationError(f"{path}: expected constant {constraints['const']!r}")
    if "enum" in constraints and value not in constraints["enum"]:
        raise ContractValidationError(f"{path}: value {value!r} is not allowed")
    if isinstance(value, int) and not isinstance(value, bool):
        if "minimum" in constraints and value < constraints["minimum"]:
            raise ContractValidationError(f"{path}: below minimum")
        if "maximum" in constraints and value > constraints["maximum"]:
            raise ContractValidationError(f"{path}: above maximum")
    if isinstance(value, str):
        if "minLength" in constraints and len(value) < constraints["minLength"]:
            raise ContractValidationError(f"{path}: string is too short")
        if "maxLength" in constraints and len(value) > constraints["maxLength"]:
            raise ContractValidationError(f"{path}: string is too long")
        if "pattern" in constraints and re.fullmatch(constraints["pattern"], value) is None:
            raise ContractValidationError(f"{path}: pattern mismatch")


def _condition_matches(value: dict[str, Any], condition: dict[str, Any]) -> bool:
    for name, constraints in condition.get("properties", {}).items():
        if name not in value:
            return False
        try:
            _validate_constraints(value[name], constraints, name)
        except ContractValidationError:
            return False
    return True


def validate_document(kind: str, value: dict[str, Any], schema: dict[str, Any]) -> None:
    try:
        definition = schema["$defs"][kind]
    except KeyError as error:
        raise ContractValidationError(f"missing schema definition: {kind}") from error
    if not isinstance(value, dict):
        raise ContractValidationError(f"{kind}: expected object")
    required = set(definition.get("required", []))
    missing = sorted(required.difference(value))
    if missing:
        raise ContractValidationError(f"{kind}: missing {', '.join(missing)}")
    properties = definition.get("properties", {})
    if definition.get("additionalProperties") is False:
        extra = sorted(set(value).difference(properties))
        if extra:
            raise ContractValidationError(f"{kind}: unexpected {', '.join(extra)}")
    for name, constraints in properties.items():
        if name in value:
            _validate_constraints(value[name], constraints, f"{kind}.{name}")
    for branch in definition.get("allOf", []):
        if _condition_matches(value, branch.get("if", {})):
            for name, constraints in branch.get("then", {}).get("properties", {}).items():
                if name in value:
                    _validate_constraints(value[name], constraints, f"{kind}.{name}")


def validate_examples(schema: dict[str, Any], examples: dict[str, Any]) -> None:
    mapping = {
        "launchContext": "CityLaunchContext",
        "snapshot": "CitySnapshotEnvelope",
        "intent": "CityIntentEnvelope",
        "acceptedReceipt": "CityIntentReceipt",
        "conflictReceipt": "CityIntentReceipt",
    }
    if examples.get("protocolVersion") != 1:
        raise ContractValidationError("examples: protocolVersion must be 1")
    for example_name, definition_name in mapping.items():
        if example_name not in examples:
            raise ContractValidationError(f"examples: missing {example_name}")
        validate_document(definition_name, examples[example_name], schema)

    context = examples["launchContext"]
    snapshot = examples["snapshot"]
    intent = examples["intent"]
    receipt = examples["acceptedReceipt"]
    identity = (context["sessionId"], context["cityId"])
    if (intent["sessionId"], intent["cityId"]) != identity:
        raise ContractValidationError("intent does not match launch identity")
    if (receipt["sessionId"], receipt["cityId"]) != identity:
        raise ContractValidationError("receipt does not match launch identity")
    if snapshot["cityId"] != context["cityId"]:
        raise ContractValidationError("snapshot does not match launch city")
    if snapshot["stateRevision"] != intent["expectedStateRevision"]:
        raise ContractValidationError("intent does not target snapshot revision")
    if receipt["resultingStateRevision"] <= snapshot["stateRevision"]:
        raise ContractValidationError("accepted receipt must advance the revision")
    digest = hashlib.sha256(snapshot["payloadJson"].encode("utf-8")).hexdigest()
    if digest != snapshot["payloadSha256"]:
        raise ContractValidationError("snapshot payloadSha256 mismatch")


def validate_sources() -> None:
    contract = CONTRACT_PATH.read_text(encoding="utf-8")
    contract_asmdef = load_json(CONTRACT_ASMDEF_PATH)
    contract_package = load_json(CONTRACT_PACKAGE_PATH)
    bootstrap = BOOTSTRAP_PATH.read_text(encoding="utf-8")
    lab_scene = LAB_SCENE_PATH.read_text(encoding="utf-8")
    document = DOCUMENT_PATH.read_text(encoding="utf-8")
    required_symbols = (
        "CityLaunchContext",
        "CitySnapshotEnvelope",
        "CityIntentEnvelope",
        "CityIntentReceipt",
        "ICityModeSnapshotSource",
        "ICityModeIntentSink",
        "CityModeSession",
        "SessionAlreadyActive",
        "public const int Current = 1",
    )
    for symbol in required_symbols:
        if symbol not in contract:
            raise ContractValidationError(f"C# contract is missing {symbol}")
    forbidden_runtime_dependencies = (
        "using UnityEngine",
        "LocalCitySimulation",
        "CitySaveService",
        "Resources.Load",
        "RuntimeInitializeOnLoadMethod",
    )
    for dependency in forbidden_runtime_dependencies:
        if dependency in contract:
            raise ContractValidationError(f"C# boundary depends on {dependency}")
    if contract_asmdef.get("name") != "Victoria.CityMode.Contracts":
        raise ContractValidationError("contract assembly name is not stable")
    if contract_asmdef.get("references") != [] or contract_asmdef.get("noEngineReferences") is not True:
        raise ContractValidationError("contract assembly is not isolated from Unity")
    if contract_package.get("name") != "com.victoria.citymode.contracts":
        raise ContractValidationError("contract package name is not stable")
    if contract_package.get("dependencies"):
        raise ContractValidationError("contract package must not pull Unity packages")
    if "RuntimeInitializeOnLoadMethod" in bootstrap:
        raise ContractValidationError("laboratory bootstrap still starts globally")
    if "public static CityLabGame StartLaboratory()" not in bootstrap:
        raise ContractValidationError("laboratory bootstrap is not explicit")
    if "guid: ee33757c3fdb97e4682fbec371dd6b7b" not in lab_scene:
        raise ContractValidationError("laboratory scene no longer owns CityLabGame explicitly")
    if PINNED_FORGEHISTORY_SHA not in document:
        raise ContractValidationError("architecture document is not pinned to the audited commit")
    if "lecture seule" not in document:
        raise ContractValidationError("architecture document does not preserve read-only upstream")
    if "Matrice d'autorité" not in document or "Demandes amont à soumettre à Hermes" not in document:
        raise ContractValidationError("architecture document is incomplete")


def validate_repository() -> None:
    schema = load_json(SCHEMA_PATH)
    examples = load_json(EXAMPLES_PATH)
    if schema.get("$schema") != "https://json-schema.org/draft/2020-12/schema":
        raise ContractValidationError("schema must use JSON Schema draft 2020-12")
    expected_definitions = {
        "CityLaunchContext",
        "CitySnapshotEnvelope",
        "CityIntentEnvelope",
        "CityIntentReceipt",
    }
    if set(schema.get("$defs", {})) != expected_definitions:
        raise ContractValidationError("schema definition set is incomplete or unexpected")
    validate_examples(schema, examples)
    validate_sources()


def main() -> int:
    try:
        validate_repository()
    except (ContractValidationError, OSError, json.JSONDecodeError) as error:
        print(f"CITYLAB_FORGEHISTORY_CONTRACT_ERROR {error}", file=sys.stderr)
        return 1
    print("CITYLAB_FORGEHISTORY_CONTRACT_OK protocol=1 documents=5 upstream_writes=0")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
