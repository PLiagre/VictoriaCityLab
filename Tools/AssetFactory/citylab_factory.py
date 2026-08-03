#!/usr/bin/env python3
"""Point d'entree hors Unity de l'Asset Factory CityLab.

Cette premiere tranche ne transforme encore aucun asset. Elle etablit le contrat
de production : decouverte de Blender, inventaire immuable des sources Vendor et
empreintes reproductibles avant toute adaptation.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


SCHEMA_VERSION = 1
MODEL_EXTENSIONS = {".blend", ".dae", ".fbx", ".glb", ".gltf", ".obj"}
TEXTURE_EXTENSIONS = {
    ".bmp", ".exr", ".hdr", ".jpeg", ".jpg", ".png", ".psd", ".tga", ".tif", ".tiff"
}


class FactoryError(RuntimeError):
    """Erreur attendue et lisible de la Factory."""


@dataclass(frozen=True)
class FactoryPaths:
    project_root: Path
    config_path: Path


def default_paths() -> FactoryPaths:
    project_root = Path(__file__).resolve().parents[2]
    return FactoryPaths(project_root, project_root / "AssetFactory" / "config.json")


def load_config(paths: FactoryPaths) -> dict:
    if not paths.config_path.is_file():
        raise FactoryError(f"Configuration absente : {paths.config_path}")
    config = json.loads(paths.config_path.read_text(encoding="utf-8"))
    if config.get("schema") != SCHEMA_VERSION:
        raise FactoryError(
            f"Schema de configuration non supporte : {config.get('schema')!r} "
            f"(attendu {SCHEMA_VERSION})"
        )
    return config


def relative_posix(path: Path, root: Path) -> str:
    return path.resolve().relative_to(root.resolve()).as_posix()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def stable_file_record(path: Path, project_root: Path) -> dict:
    return {
        "path": relative_posix(path, project_root),
        "bytes": path.stat().st_size,
        "sha256": sha256_file(path),
    }


def configured_source_map(config: dict) -> dict[str, dict]:
    result: dict[str, dict] = {}
    for source in config.get("sources", []):
        rel = Path(source["path"]).as_posix().rstrip("/")
        if rel in result:
            raise FactoryError(f"Source dupliquee dans la configuration : {rel}")
        result[rel] = source
    return result


def source_directories(config: dict, project_root: Path) -> list[tuple[Path, dict | None]]:
    assets_dir = project_root / config.get("assets_root", "Assets")
    if not assets_dir.is_dir():
        raise FactoryError(f"Dossier Assets absent : {assets_dir}")

    registered = configured_source_map(config)
    ignored_names = set(config.get("ignored_asset_roots", []))
    found: dict[str, tuple[Path, dict | None]] = {}

    for rel, metadata in registered.items():
        candidate = project_root / rel
        if not candidate.is_dir():
            raise FactoryError(f"Source enregistree absente : {rel}")
        found[rel] = (candidate, metadata)

    if config.get("discover_unregistered_asset_roots", True):
        for child in sorted(assets_dir.iterdir(), key=lambda item: item.name.casefold()):
            if not child.is_dir() or child.name in ignored_names:
                continue
            rel = relative_posix(child, project_root)
            found.setdefault(rel, (child, None))

    return [found[key] for key in sorted(found, key=str.casefold)]


def iter_candidate_files(source_dir: Path) -> Iterable[Path]:
    for path in source_dir.rglob("*"):
        if path.is_file() and path.suffix.lower() != ".meta":
            yield path


def build_source_record(
    source_dir: Path,
    metadata: dict | None,
    project_root: Path,
) -> dict:
    models: list[dict] = []
    textures: list[dict] = []
    counters = {"files": 0, "prefabs": 0, "materials": 0, "unity_packages": 0}

    for path in sorted(iter_candidate_files(source_dir), key=lambda item: item.as_posix().casefold()):
        counters["files"] += 1
        extension = path.suffix.lower()
        if extension in MODEL_EXTENSIONS:
            models.append(stable_file_record(path, project_root))
        elif extension in TEXTURE_EXTENSIONS:
            textures.append(stable_file_record(path, project_root))
        elif extension == ".prefab":
            counters["prefabs"] += 1
        elif extension == ".mat":
            counters["materials"] += 1
        elif extension == ".unitypackage":
            counters["unity_packages"] += 1

    registered = metadata is not None
    provenance = (metadata or {}).get("provenance", {})
    warnings: list[str] = []
    if not registered:
        warnings.append("source_non_enregistree")
    if not provenance.get("store_url"):
        warnings.append("url_store_manquante")
    if provenance.get("license_status") != "verified":
        warnings.append("licence_a_verifier")
    if not models:
        warnings.append("aucun_modele_importable_par_blender")

    return {
        "id": (metadata or {}).get("id", re.sub(r"[^a-z0-9]+", "_", source_dir.name.lower()).strip("_")),
        "path": relative_posix(source_dir, project_root),
        "registered": registered,
        "provenance": provenance,
        "counts": {
            **counters,
            "model_candidates": len(models),
            "texture_candidates": len(textures),
        },
        "models": models,
        "textures": textures,
        "warnings": warnings,
    }


def build_inventory(config: dict, project_root: Path) -> dict:
    sources = [
        build_source_record(source_dir, metadata, project_root)
        for source_dir, metadata in source_directories(config, project_root)
    ]
    return {
        "schema": SCHEMA_VERSION,
        "generator": "Tools/AssetFactory/citylab_factory.py",
        "policy": {
            "sources_immutable": True,
            "unity_required": False,
            "adapted_output_root": config["adapted_output_root"],
        },
        "summary": {
            "sources": len(sources),
            "registered_sources": sum(1 for source in sources if source["registered"]),
            "model_candidates": sum(source["counts"]["model_candidates"] for source in sources),
            "texture_candidates": sum(source["counts"]["texture_candidates"] for source in sources),
            "sources_requiring_provenance_review": sum(
                1 for source in sources if source["warnings"]
            ),
        },
        "sources": sources,
    }


def discover_unregistered_sources(config: dict, project_root: Path) -> list[dict]:
    """Return stable summaries for new top-level Asset Store folders."""
    candidates = []
    for source_dir, metadata in source_directories(config, project_root):
        if metadata is not None:
            continue
        record = build_source_record(source_dir, None, project_root)
        candidates.append({
            "id": record["id"],
            "path": record["path"],
            "model_candidates": record["counts"]["model_candidates"],
            "texture_candidates": record["counts"]["texture_candidates"],
            "warnings": record["warnings"],
        })
    return candidates


def inventory_output_path(config: dict, project_root: Path) -> Path:
    return project_root / config.get(
        "inventory_output", "AssetFactory/Reports/source_inventory.json"
    )


def render_json(payload: dict) -> str:
    return json.dumps(payload, ensure_ascii=False, indent=2, sort_keys=False) + "\n"


def scan(config: dict, project_root: Path, check: bool) -> int:
    output = inventory_output_path(config, project_root)
    rendered = render_json(build_inventory(config, project_root))
    if check:
        current = output.read_text(encoding="utf-8") if output.is_file() else ""
        if current != rendered:
            print(f"ASSET_FACTORY_INVENTORY_STALE path={relative_posix(output, project_root)}")
            return 1
        print(f"ASSET_FACTORY_INVENTORY_OK path={relative_posix(output, project_root)}")
        return 0

    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(rendered, encoding="utf-8", newline="\n")
    payload = json.loads(rendered)
    summary = payload["summary"]
    print(
        "ASSET_FACTORY_SCAN_OK "
        f"sources={summary['sources']} models={summary['model_candidates']} "
        f"textures={summary['texture_candidates']} "
        f"review={summary['sources_requiring_provenance_review']} "
        f"output={relative_posix(output, project_root)}"
    )
    return 0


def validate_construction_contract(recipe: dict) -> list[str]:
    if recipe.get("category") != "building":
        return []
    errors: list[str] = []
    if recipe.get("construction_schema") != "AssetFactory/Schemas/building_construction.schema.json":
        errors.append("schema_construction_manquant")
    construction = recipe.get("construction", {})
    stages = construction.get("stages", [])
    expected = ("foundation", "frame", "roof", "details")
    if construction.get("mode") != "cumulative_layers" or len(stages) != len(expected):
        errors.append("phases_construction_invalides")
    else:
        cursor = 0.0
        for index, (stage, expected_id) in enumerate(zip(stages, expected), start=1):
            progress = stage.get("progress", [])
            if stage.get("id") != expected_id or stage.get("order") != index:
                errors.append(f"phase_{index}_ordre_invalide")
            if (len(progress) != 2 or not all(isinstance(value, (int, float)) for value in progress)
                    or abs(progress[0] - cursor) > 0.000001 or progress[1] <= progress[0]):
                errors.append(f"phase_{index}_progression_invalide")
            else:
                cursor = float(progress[1])
            if not stage.get("required_categories"):
                errors.append(f"phase_{index}_materiaux_manquants")
        if abs(cursor - 1.0) > 0.000001:
            errors.append("progression_construction_incomplete")

    variants = recipe.get("variants", [])
    if not isinstance(variants, list) or len(variants) < 3:
        errors.append("variantes_insuffisantes")
    else:
        identifiers: set[str] = set()
        for index, variant in enumerate(variants):
            identifier = variant.get("id", "")
            if not re.fullmatch(r"[a-z0-9]+", identifier) or identifier in identifiers:
                errors.append(f"variante_{index}_id_invalide")
            identifiers.add(identifier)
            if not isinstance(variant.get("seed_offset"), int):
                errors.append(f"variante_{index}_graine_invalide")
            palette = variant.get("palette", {})
            for key in ("wood", "wood_highlight", "roof", "roof_accent"):
                if not re.fullmatch(r"#[0-9a-fA-F]{6}", palette.get(key, "")):
                    errors.append(f"variante_{index}_{key}_invalide")
            if not isinstance(variant.get("chimney"), bool):
                errors.append(f"variante_{index}_cheminee_invalide")
    return errors


def validate_texture_recipe(recipe: dict, adapted_output_root: str) -> list[str]:
    errors: list[str] = []
    if recipe.get("schema") != SCHEMA_VERSION:
        errors.append("schema_invalide")
    if not re.fullmatch(r"texture_[a-z0-9]+(?:_[a-z0-9]+)*", recipe.get("id", "")):
        errors.append("id_invalide")
    if recipe.get("status") not in {"reviewed", "approved"}:
        errors.append("statut_invalide")
    if recipe.get("texture_schema") != "AssetFactory/Schemas/pbr_trim.schema.json":
        errors.append("schema_texture_manquant")
    if not isinstance(recipe.get("seed"), int):
        errors.append("graine_manquante")

    resolution = recipe.get("resolution")
    if (not isinstance(resolution, int) or resolution < 512 or resolution > 4096
            or resolution & (resolution - 1)):
        errors.append("resolution_invalide")

    required_maps = {"BaseColor", "Normal", "AO", "Roughness", "Metallic", "VariationMask"}
    maps = recipe.get("maps")
    if not isinstance(maps, list) or set(maps) != required_maps or len(maps) != len(required_maps):
        errors.append("cartes_pbr_invalides")

    regions = recipe.get("regions")
    if not isinstance(regions, list) or len(regions) < 3:
        errors.append("regions_insuffisantes")
    else:
        cursor = 0.0
        seen_ids: set[str] = set()
        for index, region in enumerate(regions):
            region_id = region.get("id")
            lower = region.get("v_min")
            upper = region.get("v_max")
            if not region_id or region_id in seen_ids:
                errors.append(f"region_{index}_id_invalide")
            seen_ids.add(region_id)
            if (not isinstance(lower, (int, float)) or not isinstance(upper, (int, float))
                    or abs(float(lower) - cursor) > 1e-5 or float(upper) <= float(lower)):
                errors.append(f"region_{index}_intervalle_invalide")
            else:
                cursor = float(upper)
        if abs(cursor - 1.0) > 1e-5:
            errors.append("regions_couverture_incomplete")

    outputs = recipe.get("outputs", {})
    output_prefix = Path(adapted_output_root).as_posix().rstrip("/") + "/"
    if not Path(outputs.get("published", "")).as_posix().startswith(output_prefix):
        errors.append("sortie_hors_adapted_factory")
    if not Path(outputs.get("workbench", "")).as_posix().startswith("AssetFactory/Workbench/"):
        errors.append("workbench_invalide")
    budgets = recipe.get("budgets", {})
    if not isinstance(budgets.get("total_bytes_max"), int) or budgets.get("total_bytes_max", 0) <= 0:
        errors.append("budget_octets_invalide")
    if not isinstance(budgets.get("contrast_min"), (int, float)) or budgets.get("contrast_min", -1) < 0:
        errors.append("budget_contraste_invalide")
    return errors


def validate_recipe(recipe: dict, inventory: dict, adapted_output_root: str) -> list[str]:
    if recipe.get("texture_schema") or str(recipe.get("id", "")).startswith("texture_"):
        return validate_texture_recipe(recipe, adapted_output_root)

    errors: list[str] = []
    recipe_id = recipe.get("id", "")
    if recipe.get("schema") != SCHEMA_VERSION:
        errors.append("schema_invalide")
    if not re.fullmatch(r"(?:building|prop|unit)_[a-z0-9]+(?:_[a-z0-9]+)*", recipe_id):
        errors.append("id_invalide")
    if not isinstance(recipe.get("seed"), int):
        errors.append("graine_manquante")
    if recipe.get("status") not in {"draft", "reviewed", "approved"}:
        errors.append("statut_invalide")
    errors.extend(validate_construction_contract(recipe))

    planned_output = recipe.get("planned_output", "")
    output_prefix = Path(adapted_output_root).as_posix().rstrip("/") + "/"
    if not Path(planned_output).as_posix().startswith(output_prefix):
        errors.append("sortie_hors_adapted_factory")

    indexed: dict[str, tuple[dict, dict]] = {}
    for source in inventory.get("sources", []):
        for model in source.get("models", []):
            indexed[model["path"]] = (source, model)

    inputs = recipe.get("inputs")
    if not isinstance(inputs, list) or not inputs:
        errors.append("entrees_manquantes")
        return errors

    seen_roles: set[str] = set()
    for index, component in enumerate(inputs):
        label = f"entree_{index}"
        role = component.get("role")
        if not role or role in seen_roles:
            errors.append(f"{label}_role_invalide")
        seen_roles.add(role)

        source_path = Path(component.get("path", "")).as_posix()
        indexed_record = indexed.get(source_path)
        if indexed_record is None:
            errors.append(f"{label}_source_absente_inventaire")
            continue
        source, model = indexed_record
        if not source.get("registered"):
            errors.append(f"{label}_source_non_enregistree")
        if source.get("provenance", {}).get("license_status") != "verified":
            errors.append(f"{label}_licence_non_verifiee")
        if component.get("sha256") != model.get("sha256"):
            errors.append(f"{label}_hash_incorrect")

    return errors


def check_recipes(config: dict, project_root: Path, requested: Path | None) -> int:
    inventory_path = inventory_output_path(config, project_root)
    if not inventory_path.is_file():
        raise FactoryError("Inventaire absent : executer la commande scan")
    inventory = json.loads(inventory_path.read_text(encoding="utf-8"))

    if requested:
        paths = [requested if requested.is_absolute() else project_root / requested]
    else:
        paths = sorted((project_root / "AssetFactory" / "Recipes").glob("*.json"))
    if not paths:
        raise FactoryError("Aucune recette JSON a verifier")

    failures = 0
    for path in paths:
        recipe = json.loads(path.read_text(encoding="utf-8"))
        errors = validate_recipe(recipe, inventory, config["adapted_output_root"])
        if errors:
            failures += 1
            print(f"ASSET_FACTORY_RECIPE_ERROR path={relative_posix(path, project_root)} errors={','.join(errors)}")
        else:
            component_count = len(recipe.get("inputs", recipe.get("maps", [])))
            print(
                f"ASSET_FACTORY_RECIPE_OK path={relative_posix(path, project_root)} "
                f"id={recipe['id']} components={component_count}"
            )
    return 1 if failures else 0


def validate_admission_profile(profile: dict, inventory: dict, config: dict) -> list[str]:
    errors: list[str] = []
    if profile.get("schema") != SCHEMA_VERSION:
        errors.append("schema_invalide")
    if not re.fullmatch(r"admission_[a-z0-9]+(?:_[a-z0-9]+)*", profile.get("id", "")):
        errors.append("id_invalide")
    if profile.get("status") != "approved":
        errors.append("statut_non_approuve")
    if profile.get("admission_schema") != "AssetFactory/Schemas/vendor_admission.schema.json":
        errors.append("schema_admission_manquant")

    source_spec = profile.get("source", {})
    source = next((entry for entry in inventory.get("sources", [])
                   if entry.get("id") == source_spec.get("id")), None)
    if source is None or source.get("path") != Path(source_spec.get("path", "")).as_posix():
        errors.append("source_absente_inventaire")
    elif not source.get("registered"):
        errors.append("source_non_enregistree")
    provenance = source_spec.get("provenance", {})
    required_provenance = ("kind", "store_url", "version", "acquired", "license", "license_status")
    for key in required_provenance:
        if not provenance.get(key):
            errors.append(f"provenance_{key}_manquante")
    if provenance.get("license_status") != "verified":
        errors.append("licence_non_verifiee")
    if source is not None:
        registered_provenance = source.get("provenance", {})
        for key in required_provenance:
            if provenance.get(key) != registered_provenance.get(key):
                errors.append(f"provenance_{key}_divergente")

    indexed = {
        model["path"]: model
        for inventory_source in inventory.get("sources", [])
        for model in inventory_source.get("models", [])
    }
    inputs = profile.get("inputs")
    if not isinstance(inputs, list) or not inputs:
        errors.append("entrees_manquantes")
    else:
        roles: set[str] = set()
        for index, component in enumerate(inputs):
            role = component.get("role")
            if not role or role in roles:
                errors.append(f"entree_{index}_role_invalide")
            roles.add(role)
            path = Path(component.get("path", "")).as_posix()
            model = indexed.get(path)
            if model is None:
                errors.append(f"entree_{index}_source_absente")
            elif component.get("sha256") != model.get("sha256"):
                errors.append(f"entree_{index}_hash_incorrect")
            if source is not None and not path.startswith(source["path"].rstrip("/") + "/"):
                errors.append(f"entree_{index}_hors_source")

    adaptation = profile.get("adaptation_profile", {})
    if adaptation.get("source_policy") != "immutable":
        errors.append("source_policy_non_immuable")
    if not isinstance(adaptation.get("metric_scale"), (int, float)) or adaptation.get("metric_scale", 0) <= 0:
        errors.append("echelle_metrique_invalide")
    if adaptation.get("up_axis") not in {"Y", "Z"}:
        errors.append("axe_vertical_invalide")
    if adaptation.get("forward_axis") not in {"X", "-X", "Y", "-Y", "Z", "-Z"}:
        errors.append("axe_avant_invalide")
    if not adaptation.get("operations"):
        errors.append("operations_manquantes")
    if not adaptation.get("material_strategy") or not adaptation.get("lod_strategy"):
        errors.append("strategie_adaptation_incomplete")
    planned_root = Path(adaptation.get("planned_output_root", "")).as_posix().rstrip("/")
    allowed_root = Path(config["adapted_output_root"]).as_posix().rstrip("/")
    if planned_root != allowed_root and not planned_root.startswith(allowed_root + "/"):
        errors.append("sortie_hors_adapted_factory")
    return sorted(set(errors))


def admission_profiles(project_root: Path, requested: Path | None) -> list[Path]:
    if requested:
        return [requested if requested.is_absolute() else project_root / requested]
    return sorted((project_root / "AssetFactory" / "AdmissionProfiles").glob("*.json"))


def check_admissions(config: dict, project_root: Path, requested: Path | None,
                     write_report: bool) -> int:
    inventory_path = inventory_output_path(config, project_root)
    if not inventory_path.is_file():
        raise FactoryError("Inventaire absent : executer la commande scan")
    inventory = json.loads(inventory_path.read_text(encoding="utf-8"))
    paths = admission_profiles(project_root, requested)
    if not paths:
        raise FactoryError("Aucun profil d'admission JSON a verifier")
    results = []
    failures = 0
    for path in paths:
        profile = json.loads(path.read_text(encoding="utf-8"))
        errors = validate_admission_profile(profile, inventory, config)
        results.append({
            "profile": relative_posix(path, project_root),
            "id": profile.get("id"),
            "source": profile.get("source", {}).get("id"),
            "inputs": len(profile.get("inputs", [])),
            "status": "passed" if not errors else "failed",
            "errors": errors,
        })
        if errors:
            failures += 1
            print(f"ASSET_FACTORY_ADMISSION_ERROR path={relative_posix(path, project_root)} errors={','.join(errors)}")
        else:
            print(f"ASSET_FACTORY_ADMISSION_OK path={relative_posix(path, project_root)} inputs={len(profile['inputs'])}")
    if write_report:
        output = project_root / "AssetFactory" / "Reports" / "vendor_admission.json"
        payload = {
            "schema": SCHEMA_VERSION,
            "generator": "Tools/AssetFactory/citylab_factory.py admission-check",
            "unity_launched": False,
            "profiles": results,
        }
        output.parent.mkdir(parents=True, exist_ok=True)
        temporary = output.with_suffix(".json.tmp")
        temporary.write_text(render_json(payload), encoding="utf-8", newline="\n")
        temporary.replace(output)
    return 1 if failures else 0


def publication_entries(manifest: dict) -> list[dict]:
    assets = manifest.get("assets")
    if not isinstance(assets, list) or not assets:
        raise FactoryError("Manifest sans liste assets publiable")
    return assets


def publish_from_manifest(config: dict, project_root: Path, manifest_path: Path,
                          publish: bool) -> int:
    path = manifest_path if manifest_path.is_absolute() else project_root / manifest_path
    manifest = json.loads(path.read_text(encoding="utf-8"))
    entries = publication_entries(manifest)
    adapted_root = Path(config["adapted_output_root"]).as_posix().rstrip("/") + "/"
    total_bytes = 0
    for index, entry in enumerate(entries):
        source_rel = Path(entry.get("generated_path", "")).as_posix()
        destination_rel = Path(entry.get("path", "")).as_posix()
        expected_hash = entry.get("fbx_sha256") or entry.get("sha256")
        if not source_rel.startswith("AssetFactory/Workbench/"):
            raise FactoryError(f"Publication {index}: source hors workbench")
        if not destination_rel.startswith(adapted_root):
            raise FactoryError(f"Publication {index}: destination hors Adapted Factory")
        source = project_root / source_rel
        destination = project_root / destination_rel
        if not source.is_file() or sha256_file(source) != expected_hash:
            raise FactoryError(f"Publication {index}: hash source incorrect")
        total_bytes += source.stat().st_size
        if destination.exists() and sha256_file(destination) != expected_hash:
            raise FactoryError(f"Publication {index}: collision destination")
        if publish and not destination.exists():
            destination.parent.mkdir(parents=True, exist_ok=True)
            temporary = destination.with_suffix(destination.suffix + ".tmp")
            shutil.copy2(source, temporary)
            if sha256_file(temporary) != expected_hash:
                temporary.unlink(missing_ok=True)
                raise FactoryError(f"Publication {index}: hash temporaire incorrect")
            temporary.replace(destination)
    if publish and manifest.get("status_after_publication"):
        manifest["status"] = manifest["status_after_publication"]
        manifest["publication"] = {
            "mode": "explicit_atomic_copy",
            "assets": len(entries),
            "bytes": total_bytes,
            "unity_launched": False,
        }
        temporary_manifest = path.with_suffix(path.suffix + ".tmp")
        temporary_manifest.write_text(render_json(manifest), encoding="utf-8", newline="\n")
        temporary_manifest.replace(path)
    print(
        "ASSET_FACTORY_PUBLICATION_OK "
        f"mode={'publish' if publish else 'dry-run'} assets={len(entries)} bytes={total_bytes} "
        "atomic=true unity_launched=false"
    )
    return 0


def blender_candidates(config: dict, project_root: Path) -> Iterable[Path]:
    override = os.environ.get("CITYLAB_BLENDER")
    if override:
        yield Path(override)

    configured = config.get("blender", {}).get("executable")
    if configured:
        candidate = Path(configured)
        yield candidate if candidate.is_absolute() else project_root / candidate

    for pattern in config.get("blender", {}).get("windows_globs", []):
        pattern_path = Path(pattern)
        parent = pattern_path.parent
        if parent.is_dir():
            yield from sorted(parent.glob(pattern_path.name), reverse=True)


def find_blender(config: dict, project_root: Path) -> Path:
    for candidate in blender_candidates(config, project_root):
        if candidate.is_file():
            return candidate.resolve()
    raise FactoryError(
        "Blender introuvable. Installer Blender LTS ou definir CITYLAB_BLENDER."
    )


def blender_version(executable: Path) -> str:
    process = subprocess.run(
        [str(executable), "--version"],
        check=False,
        capture_output=True,
        text=True,
        timeout=30,
    )
    if process.returncode != 0:
        raise FactoryError(f"Blender a retourne le code {process.returncode}")
    first_line = (process.stdout or "").splitlines()[0].strip()
    return first_line or "version inconnue"


def doctor(config: dict, project_root: Path, as_json: bool) -> int:
    blender = find_blender(config, project_root)
    payload = {
        "status": "ok",
        "unity_launched": False,
        "python": sys.version.split()[0],
        "blender": {"path": str(blender), "version": blender_version(blender)},
        "configured_sources": len(config.get("sources", [])),
        "adapted_output_root": config["adapted_output_root"],
    }
    if as_json:
        print(render_json(payload), end="")
    else:
        print(
            "ASSET_FACTORY_DOCTOR_OK "
            f"blender=\"{payload['blender']['version']}\" "
            f"python={payload['python']} unity_launched=false"
        )
    return 0


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    defaults = default_paths()
    parser = argparse.ArgumentParser(description="Asset Factory CityLab (hors Unity)")
    parser.add_argument("--project-root", type=Path, default=defaults.project_root)
    parser.add_argument("--config", type=Path, default=None)
    subparsers = parser.add_subparsers(dest="command", required=True)

    doctor_parser = subparsers.add_parser("doctor", help="Verifier Python et Blender")
    doctor_parser.add_argument("--json", action="store_true")

    scan_parser = subparsers.add_parser("scan", help="Inventorier les sources Vendor")
    scan_parser.add_argument("--check", action="store_true", help="Refuser un inventaire perime")

    recipe_parser = subparsers.add_parser("recipe-check", help="Verifier les recettes et leurs hashes")
    recipe_parser.add_argument("recipe", nargs="?", type=Path, help="Recette precise, sinon toutes")

    discovery_parser = subparsers.add_parser("admission-discover", help="Detecter les nouveaux dossiers Vendor")
    discovery_parser.add_argument("--json", action="store_true")

    admission_parser = subparsers.add_parser("admission-check", help="Verifier les profils d'admission")
    admission_parser.add_argument("profile", nargs="?", type=Path, help="Profil precis, sinon tous")
    admission_parser.add_argument("--write-report", action="store_true")

    publish_parser = subparsers.add_parser("publication-check", help="Simuler ou executer une copie atomique")
    publish_parser.add_argument("manifest", type=Path)
    publish_parser.add_argument("--publish", action="store_true")
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    project_root = args.project_root.resolve()
    config_path = args.config.resolve() if args.config else project_root / "AssetFactory" / "config.json"
    paths = FactoryPaths(project_root, config_path)
    try:
        config = load_config(paths)
        if args.command == "doctor":
            return doctor(config, project_root, args.json)
        if args.command == "scan":
            return scan(config, project_root, args.check)
        if args.command == "recipe-check":
            return check_recipes(config, project_root, args.recipe)
        if args.command == "admission-discover":
            candidates = discover_unregistered_sources(config, project_root)
            payload = {"schema": SCHEMA_VERSION, "unity_launched": False, "candidates": candidates}
            if args.json:
                print(render_json(payload), end="")
            else:
                print(f"ASSET_FACTORY_ADMISSION_DISCOVERY_OK candidates={len(candidates)} unity_launched=false")
            return 0
        if args.command == "admission-check":
            return check_admissions(config, project_root, args.profile, args.write_report)
        if args.command == "publication-check":
            return publish_from_manifest(config, project_root, args.manifest, args.publish)
        raise FactoryError(f"Commande inconnue : {args.command}")
    except (FactoryError, json.JSONDecodeError, OSError, subprocess.SubprocessError) as error:
        print(f"ASSET_FACTORY_ERROR {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
