"""Publie et manifeste le pilote de bâtiments sans lancer Unity."""

from __future__ import annotations

import argparse
import hashlib
import json
import shutil
from pathlib import Path


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", type=Path, default=Path.cwd())
    parser.add_argument("--catalog", type=Path, default=Path("AssetFactory/Catalogs/building_pilot.json"))
    parser.add_argument("--publish", action="store_true")
    return parser.parse_args()


def main() -> int:
    args = arguments()
    root = args.project_root.resolve()
    catalog_path = args.catalog if args.catalog.is_absolute() else root / args.catalog
    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    target_root = root / "Assets/CityLabHost/Adapted/Factory/Models"
    target_root.mkdir(parents=True, exist_ok=True)
    budget = catalog["budgets"]["lod_triangles_max"]
    manifest = {
        "schema": 1,
        "id": catalog["id"],
        "status": "published_pending_unity_import_validation" if args.publish else "dry_run",
        "catalog": catalog_path.relative_to(root).as_posix(),
        "construction": {"stages": catalog["construction_stages"], "lod_per_stage": 3},
        "families": [],
        "gates": {"unity_launched": False}
    }
    failures: list[str] = []
    for family in catalog["families"]:
        record = {
            "id": family["id"],
            "function": family["function"],
            "wall_system": family["wall_system"],
            "roof_system": family["roof_system"],
            "identity_markers": family["identity_markers"],
            "variants": []
        }
        for variant in catalog["variants"]:
            asset_id = family["id"] + "_" + variant["id"]
            metrics_path = root / "AssetFactory/Reports" / f"{asset_id}_metrics.json"
            source = root / "AssetFactory/Workbench/Models" / f"{asset_id}.fbx"
            target = target_root / f"{asset_id}.fbx"
            if not metrics_path.is_file() or not source.is_file():
                failures.append(asset_id + ":outputs_missing")
                continue
            metrics = json.loads(metrics_path.read_text(encoding="utf-8"))
            triangles = [metrics["triangles"][key] for key in ("lod0", "lod1", "lod2")]
            if any(value > maximum for value, maximum in zip(triangles, budget)):
                failures.append(asset_id + ":lod_budget")
            if len(metrics.get("construction_phase_triangles", {})) != 4:
                failures.append(asset_id + ":construction_phases")
            if args.publish:
                shutil.copy2(source, target)
            source_hash = sha256(source)
            published_hash = sha256(target) if target.is_file() else None
            if args.publish and source_hash != published_hash:
                failures.append(asset_id + ":published_hash")
            record["variants"].append({
                "id": variant["id"],
                "seed": metrics["seed"],
                "lod_triangles": triangles,
                "canonical_mesh_sha256": metrics["canonical_mesh_sha256"],
                "fbx_sha256": source_hash,
                "published_fbx": target.relative_to(root).as_posix() if args.publish else None
            })
        manifest["families"].append(record)
    manifest["gates"].update({
        "family_count": len(manifest["families"]),
        "variant_count": sum(len(family["variants"]) for family in manifest["families"]),
        "budgets_and_phases": "failed" if failures else "passed",
        "published_copy_hashes": "failed" if failures else ("passed" if args.publish else "not_run"),
        "unity_import": "pending_no_unity_launch"
    })
    manifest["failures"] = failures
    output = root / "AssetFactory/Manifests/building_pilot.json"
    rendered = json.dumps(manifest, ensure_ascii=False, indent=2) + "\n"
    temporary = output.with_suffix(".json.tmp")
    temporary.write_text(rendered, encoding="utf-8", newline="\n")
    temporary.replace(output)
    if failures:
        print("CITYLAB_BUILDING_PILOT_ERROR " + " ".join(failures))
        return 1
    print(f"CITYLAB_BUILDING_PILOT_OK families={len(manifest['families'])} "
          f"variants={manifest['gates']['variant_count']} publish={str(args.publish).lower()} "
          f"unity_launched=false")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
