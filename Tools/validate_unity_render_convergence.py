#!/usr/bin/env python3
"""Validate the versioned M3-FH-03 Unity/render convergence decision."""

from __future__ import annotations

import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
DECISION_PATH = ROOT / "Docs" / "Integration" / "unity-render-convergence-v1.json"
MATRIX_PATH = ROOT / "Docs" / "Integration" / "UNITY_RENDER_DEPENDENCY_MATRIX.md"
PROJECT_VERSION_PATH = ROOT / "ProjectSettings" / "ProjectVersion.txt"
CITYLAB_MANIFEST_PATH = ROOT / "Packages" / "manifest.json"
CITYLAB_LOCK_PATH = ROOT / "Packages" / "packages-lock.json"
PRESENTATION_PACKAGE_PATH = ROOT / "Packages" / "com.victoria.citymode.presentation" / "package.json"
PRESENTATION_RUNTIME_PATH = ROOT / "Packages" / "com.victoria.citymode.presentation" / "Runtime"
MINIMAL_HOST_MANIFEST_PATH = (
    ROOT / "Tools" / "UnityHosts" / "CityModeMinimalHost" / "Packages" / "manifest.json"
)

SHA256_PATTERN = re.compile(r"^[0-9A-F]{64}$")
EXPECTED_HOST_PACKAGES = {
    "com.unity.entities": "1.3.15",
    "com.unity.burst": "1.8.19",
    "com.unity.collections": "2.5.7",
    "com.unity.mathematics": "1.3.2",
    "com.unity.render-pipelines.universal": "17.0.4",
}
EXPECTED_LAB_ADAPTERS = {
    "com.unity.inputsystem": "1.13.1",
    "com.unity.ai.navigation": "2.0.6",
}
FORBIDDEN_CORE_PACKAGES = {
    "com.unity.entities",
    "com.unity.inputsystem",
    "com.unity.ai.navigation",
    "com.unity.render-pipelines.universal",
}


class ConvergenceValidationError(RuntimeError):
    """Raised when the render/dependency decision drifts."""


def _load_json(path: Path) -> dict:
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ConvergenceValidationError(f"cannot read {path.relative_to(ROOT)}: {exc}") from exc


def _require(condition: bool, message: str) -> None:
    if not condition:
        raise ConvergenceValidationError(message)


def validate() -> dict[str, int | str]:
    decision = _load_json(DECISION_PATH)
    _require(decision.get("schemaVersion") == 1, "schemaVersion must be 1")
    _require(
        decision.get("forgeHistoryCommit") == "268e8aab151452b0c740a44a7cc97ca3fd37e311",
        "ForgeHistory audit commit drifted",
    )
    _require(decision.get("unityVersion") == "6000.0.43f1", "decision Unity version drifted")

    selected = decision.get("selectedPipeline", {})
    _require(selected.get("decision") == "urp", "the selected pipeline must be URP")
    _require(
        selected.get("package") == "com.unity.render-pipelines.universal"
        and selected.get("version") == "17.0.4",
        "selected URP package/version drifted",
    )
    _require(
        decision.get("integratedHostPackages") == EXPECTED_HOST_PACKAGES,
        "integrated host package matrix drifted",
    )
    _require(
        decision.get("laboratoryAdapters") == EXPECTED_LAB_ADAPTERS,
        "laboratory adapter matrix drifted",
    )
    _require(
        set(decision.get("productionCoreExclusions", [])) == FORBIDDEN_CORE_PACKAGES,
        "production core exclusions drifted",
    )

    project_version = PROJECT_VERSION_PATH.read_text(encoding="utf-8")
    _require("m_EditorVersion: 6000.0.43f1" in project_version, "CityLab Unity version drifted")

    citylab_manifest = _load_json(CITYLAB_MANIFEST_PATH).get("dependencies", {})
    for package, version in {
        "com.unity.render-pipelines.universal": "17.0.4",
        **EXPECTED_LAB_ADAPTERS,
    }.items():
        _require(citylab_manifest.get(package) == version, f"CityLab manifest drifted for {package}")

    citylab_lock = _load_json(CITYLAB_LOCK_PATH).get("dependencies", {})
    for package, version in {
        "com.unity.burst": "1.8.19",
        "com.unity.collections": "2.5.1",
        "com.unity.mathematics": "1.3.2",
    }.items():
        _require(
            citylab_lock.get(package, {}).get("version") == version,
            f"CityLab laboratory lock drifted for {package}",
        )

    presentation_dependencies = _load_json(PRESENTATION_PACKAGE_PATH).get("dependencies", {})
    _require(
        presentation_dependencies == {"com.victoria.citymode.contracts": "0.1.0"},
        "production presentation package gained a non-contract dependency",
    )
    runtime_text = "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted(PRESENTATION_RUNTIME_PATH.rglob("*"))
        if path.is_file() and path.suffix in {".cs", ".asmdef"}
    )
    for forbidden in ("Unity.Entities", "UnityEngine.InputSystem", "Unity.AI.Navigation"):
        _require(forbidden not in runtime_text, f"production presentation references {forbidden}")

    minimal_dependencies = _load_json(MINIMAL_HOST_MANIFEST_PATH).get("dependencies", {})
    for forbidden in FORBIDDEN_CORE_PACKAGES:
        _require(forbidden not in minimal_dependencies, f"minimal host imports {forbidden}")

    probe = decision.get("probe", {})
    _require(probe.get("mode") == "disposable-git-archive", "probe must remain disposable")
    _require(probe.get("upstreamWrites") == 0, "probe reports upstream writes")
    _require(probe.get("builtPlayerBytes", 0) > 0, "probe player size is missing")

    goldens = probe.get("mapGoldens", [])
    _require(len(goldens) == 3, "exactly three map goldens are required")
    for golden in goldens:
        before = golden.get("builtInSha256", "")
        after = golden.get("urpSha256", "")
        _require(SHA256_PATTERN.fullmatch(before) is not None, "invalid Built-in golden hash")
        _require(SHA256_PATTERN.fullmatch(after) is not None, "invalid URP golden hash")
        _require(before == after, f"map golden changed under URP: {golden.get('name')}")

    for capture_name in ("fullMapPlayerCapture", "cityCapture"):
        capture = probe.get(capture_name, {})
        _require(SHA256_PATTERN.fullmatch(capture.get("sha256", "")) is not None, f"invalid {capture_name} hash")
        _require(capture.get("width") == 1920 and capture.get("height") == 1080, f"invalid {capture_name} size")
        _require(capture.get("magentaPixels") == 0, f"{capture_name} contains magenta pixels")

    profiles = probe.get("profileMillisecondsPerFrame", {})
    frame_budget = profiles.get("frameBudget")
    _require(isinstance(frame_budget, (int, float)) and frame_budget > 0, "invalid frame budget")
    for pipeline in ("builtIn", "urp"):
        profile = profiles.get(pipeline, {})
        _require(profile.get("gpuMap", frame_budget) < frame_budget, f"{pipeline} GPU map misses budget")
        _require(
            profile.get("wiredGpuPath", frame_budget) < frame_budget,
            f"{pipeline} wired GPU path misses budget",
        )

    matrix = MATRIX_PATH.read_text(encoding="utf-8")
    for required_text in (
        "URP 17.0.4",
        "Collections | manifeste `2.5.3`, lock `2.5.7`",
        "0 pixel magenta",
        "Hermes",
    ):
        _require(required_text in matrix, f"matrix documentation missing: {required_text}")

    return {
        "pipeline": "urp",
        "host_packages": len(EXPECTED_HOST_PACKAGES),
        "map_goldens": len(goldens),
        "magenta_pixels": 0,
        "upstream_writes": 0,
    }


def main() -> int:
    try:
        result = validate()
    except ConvergenceValidationError as exc:
        print(f"CITYLAB_RENDER_CONVERGENCE_ERROR {exc}")
        return 1
    print(
        "CITYLAB_RENDER_CONVERGENCE_OK "
        f"pipeline={result['pipeline']} "
        f"host_packages={result['host_packages']} "
        f"map_goldens={result['map_goldens']} "
        f"magenta_pixels={result['magenta_pixels']} "
        f"upstream_writes={result['upstream_writes']}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
