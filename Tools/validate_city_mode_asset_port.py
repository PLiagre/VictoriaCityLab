#!/usr/bin/env python3
"""Validate the governed M3-FH-06 City Mode asset port."""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

from PIL import Image, ImageStat


ROOT = Path(__file__).resolve().parents[1]
MANIFEST_PATH = ROOT / "Docs" / "Integration" / "city-mode-asset-port-v1.json"
PACKAGE_PATH = ROOT / "Packages" / "com.victoria.citymode.assets"
HOST_PATH = ROOT / "Tools" / "UnityHosts" / "CityModeAssetHost"
SHA256 = re.compile(r"^[0-9a-f]{64}$")
GUID = re.compile(r"^[0-9a-f]{32}$")
EXPECTED_ORDER = ["common", "biome", "city"]
EXPECTED_LICENSES = {
    "LicenseRef-Victoria-Original",
    "LicenseRef-Unity-Asset-Store-EULA",
}


class AssetPortValidationError(RuntimeError):
    """Raised when the port manifest or its proof drifts."""


def _require(condition: bool, message: str) -> None:
    if not condition:
        raise AssetPortValidationError(message)


def _load_manifest() -> dict:
    try:
        return json.loads(MANIFEST_PATH.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise AssetPortValidationError(f"cannot read asset port manifest: {exc}") from exc


def _hash(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def _guid(path: Path) -> str:
    meta = Path(str(path) + ".meta")
    _require(meta.is_file(), f"Unity meta is missing: {meta.relative_to(ROOT)}")
    match = re.search(r"^guid: ([0-9a-f]{32})$", meta.read_text(encoding="utf-8"), re.MULTILINE)
    _require(match is not None, f"Unity GUID is invalid: {meta.relative_to(ROOT)}")
    return match.group(1)


def _validate_lfs(paths: list[str]) -> None:
    command = ["git", "check-attr", "filter", "--", *paths]
    result = subprocess.run(
        command,
        cwd=ROOT,
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    _require(result.returncode == 0, f"git check-attr failed: {result.stderr.strip()}")
    observed: dict[str, str] = {}
    for line in result.stdout.splitlines():
        parts = line.rsplit(": ", 2)
        if len(parts) == 3:
            observed[parts[0].replace("\\", "/")] = parts[2]
    for path in paths:
        _require(observed.get(path) == "lfs", f"ported binary is not routed through LFS: {path}")


def _validate_static_runtime() -> None:
    package_files = [
        path
        for path in PACKAGE_PATH.rglob("*")
        if path.is_file() and path.suffix.lower() in {".cs", ".asmdef", ".json"}
    ]
    package_text = "\n".join(path.read_text(encoding="utf-8") for path in package_files)
    for forbidden in (
        "Resources.Load",
        "LocalCitySimulation",
        "CitySaveService",
        "RuntimeInitializeOnLoadMethod",
        "SceneManager",
        "Assets/CityLabHost",
    ):
        _require(forbidden not in package_text, f"portable asset package references {forbidden}")

    host_text = "\n".join(
        path.read_text(encoding="utf-8")
        for path in (HOST_PATH / "Assets").rglob("*.cs")
        if path.is_file()
    )
    _require("Resources.Load" not in host_text, "integrated asset host uses Resources.Load")
    for forbidden in ("LocalCitySimulation", "CitySaveService", "city_fixture_1001"):
        _require(forbidden not in host_text, f"asset proof host includes laboratory authority: {forbidden}")

    package = json.loads((PACKAGE_PATH / "package.json").read_text(encoding="utf-8"))
    _require(package.get("name") == "com.victoria.citymode.assets", "asset package name drifted")
    _require(package.get("dependencies") in (None, {}), "asset package gained a package dependency")

    host_manifest = json.loads((HOST_PATH / "Packages" / "manifest.json").read_text(encoding="utf-8"))
    dependencies = host_manifest.get("dependencies", {})
    _require(
        dependencies.get("com.unity.render-pipelines.universal") == "17.0.4",
        "asset proof host must use URP 17.0.4",
    )
    _require(
        dependencies.get("com.victoria.citymode.assets")
        == "file:../../../../Packages/com.victoria.citymode.assets",
        "asset proof host does not import the portable package",
    )
    for forbidden in (
        "com.victoria.citymode",
        "com.victoria.citymode.contracts",
        "com.victoria.citymode.presentation",
        "com.unity.inputsystem",
        "com.unity.ai.navigation",
    ):
        _require(forbidden not in dependencies, f"asset proof host imports forbidden dependency: {forbidden}")


def _validate_proofs(manifest: dict) -> int:
    proof = manifest.get("proof", {})
    build_log = ROOT / proof.get("buildLog", "")
    player_log = ROOT / proof.get("playerLog", "")
    playmode = ROOT / proof.get("playModeResults", "")
    _require(build_log.is_file(), "asset host build log is missing")
    _require("CITY_MODE_ASSET_BUILD_OK" in build_log.read_text(encoding="utf-8", errors="replace"),
             "asset host build proof is missing")
    _require(player_log.is_file(), "asset host player log is missing")
    player_text = player_log.read_text(encoding="utf-8", errors="replace")
    _require("CITY_MODE_ASSET_PLAYER_OK" in player_text, "asset host player proof is missing")
    _require(playmode.is_file(), "asset host PlayMode results are missing")
    try:
        test_run = ET.parse(playmode).getroot()
    except (OSError, ET.ParseError) as exc:
        raise AssetPortValidationError(f"invalid PlayMode result: {exc}") from exc
    _require(test_run.attrib.get("result") == "Passed", "asset host PlayMode suite did not pass")

    captures = proof.get("captures", [])
    _require(len(captures) == 3, "exactly three zoom captures are required")
    capture_hashes: set[str] = set()
    for capture in captures:
        path = ROOT / capture
        _require(path.is_file(), f"asset zoom capture is missing: {capture}")
        header = path.read_bytes()[:24]
        _require(header.startswith(b"\x89PNG\r\n\x1a\n"), f"asset capture is not PNG: {capture}")
        _require(path.stat().st_size > 10_000, f"asset capture is empty: {capture}")
        capture_hashes.add(_hash(path))
        with Image.open(path) as image:
            rgb = image.convert("RGB")
            _require(rgb.size == (1280, 720), f"asset capture size drifted: {capture}")
            variance = sum(ImageStat.Stat(rgb).var)
            _require(variance > 100.0, f"asset capture has no visual information: {capture}")
            magenta = sum(
                1
                for pixel in rgb.get_flattened_data()
                if pixel[0] > 245 and pixel[1] < 12 and pixel[2] > 245
            )
            _require(magenta == 0, f"asset capture contains magenta pixels: {capture}")
    _require(len(capture_hashes) == 3, "asset zoom captures are not distinct")
    return len(captures)


def validate(*, require_proofs: bool = False) -> dict[str, int | str]:
    manifest = _load_manifest()
    _require(manifest.get("schema") == 1, "asset port schema must be 1")
    _require(manifest.get("revision") == "city-mode-asset-port-v1", "asset port revision drifted")
    authority = manifest.get("authority", {})
    _require(
        authority.get("forgeHistoryMergeCommit")
        == "36f0c2eda52a9ade6286682c2a353cd13d01f101",
        "ForgeHistory boundary commit drifted",
    )
    _require(authority.get("contentOnly") is True, "asset port must remain content-only")
    _require(
        authority.get("containsSimulationClockOrSave") is False,
        "asset port claims a simulation, clock or save authority",
    )
    _require(manifest.get("loadOrder") == EXPECTED_ORDER, "asset partition load order drifted")
    _require(manifest.get("unloadOrder") == list(reversed(EXPECTED_ORDER)),
             "asset partition unload order drifted")
    partitions = manifest.get("partitions", {})
    _require(list(partitions) == EXPECTED_ORDER, "asset partitions must be common/biome/city")
    for name, partition in partitions.items():
        _require(partition.get("sceneAddress") == "Asset" + name.title(),
                 f"asset scene address drifted: {name}")
        _require(partition.get("residentBudgetBytes", 0) > 0, f"asset budget missing: {name}")

    assets = manifest.get("assets", [])
    _require(len(assets) == 11, "asset port must contain exactly 11 approved binaries")
    ids: set[str] = set()
    target_guids: set[str] = set()
    lfs_paths: list[str] = []
    counts = {partition: 0 for partition in EXPECTED_ORDER}
    for asset in assets:
        asset_id = asset.get("id", "")
        _require(asset_id and asset_id not in ids, f"duplicate asset id: {asset_id}")
        ids.add(asset_id)
        partition = asset.get("partition")
        _require(partition in counts, f"invalid asset partition: {asset_id}")
        counts[partition] += 1

        source_relative = asset.get("sourcePath", "")
        target_relative = asset.get("targetPath", "")
        _require(source_relative.startswith("Assets/CityLabHost/"), f"unapproved source root: {asset_id}")
        _require(
            target_relative.startswith("Packages/com.victoria.citymode.assets/Runtime/Content/"),
            f"target escaped portable content root: {asset_id}",
        )
        source = ROOT / source_relative
        target = ROOT / target_relative
        _require(source.is_file(), f"asset source is missing: {asset_id}")
        _require(target.is_file(), f"asset target is missing: {asset_id}")
        declared_hash = asset.get("sha256", "")
        _require(SHA256.fullmatch(declared_hash) is not None, f"invalid asset hash: {asset_id}")
        _require(_hash(source) == declared_hash, f"source hash drifted: {asset_id}")
        _require(_hash(target) == declared_hash, f"target hash drifted: {asset_id}")
        _require(source.read_bytes() == target.read_bytes(), f"source/target bytes differ: {asset_id}")
        _require(target.stat().st_size == asset.get("bytes"), f"asset byte size drifted: {asset_id}")

        source_guid = asset.get("sourceGuid", "")
        target_guid = asset.get("targetGuid", "")
        _require(GUID.fullmatch(source_guid) is not None, f"invalid source GUID: {asset_id}")
        _require(GUID.fullmatch(target_guid) is not None, f"invalid target GUID: {asset_id}")
        _require(_guid(source) == source_guid, f"source GUID drifted: {asset_id}")
        _require(_guid(target) == target_guid, f"target GUID drifted: {asset_id}")
        _require(source_guid != target_guid, f"ported asset reused source GUID: {asset_id}")
        _require(target_guid not in target_guids, f"duplicate target GUID: {asset_id}")
        target_guids.add(target_guid)

        _require(asset.get("license") in EXPECTED_LICENSES, f"invalid asset licence: {asset_id}")
        provenance = asset.get("provenance", "").split("#", 1)[0]
        _require(provenance and (ROOT / provenance).is_file(), f"asset provenance is missing: {asset_id}")
        _require(asset.get("lfs") is True, f"asset is not declared LFS: {asset_id}")
        lfs_paths.append(target_relative)

    _require(counts == {"common": 6, "biome": 2, "city": 3}, "asset partition counts drifted")
    _validate_lfs(lfs_paths)
    _validate_static_runtime()
    licences = (PACKAGE_PATH / "LICENSES.md").read_text(encoding="utf-8")
    for license_name in EXPECTED_LICENSES:
        _require(license_name in licences, f"package licence notice is missing: {license_name}")

    captures = _validate_proofs(manifest) if require_proofs else 0
    return {
        "revision": manifest["revision"],
        "assets": len(assets),
        "partitions": len(partitions),
        "captures": captures,
        "source_target_hashes": len(assets),
        "upstream_writes": 0,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--allow-missing-proofs",
        action="store_true",
        help="validate the static port before Unity proofs have been produced",
    )
    args = parser.parse_args()
    try:
        result = validate(require_proofs=not args.allow_missing_proofs)
    except AssetPortValidationError as exc:
        print(f"CITYLAB_ASSET_PORT_ERROR {exc}")
        return 1
    print(
        "CITYLAB_ASSET_PORT_OK "
        f"revision={result['revision']} assets={result['assets']} "
        f"partitions={result['partitions']} captures={result['captures']} "
        f"source_target_hashes={result['source_target_hashes']} "
        f"upstream_writes={result['upstream_writes']}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
