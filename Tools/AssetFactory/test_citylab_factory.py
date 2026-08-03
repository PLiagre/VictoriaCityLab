from __future__ import annotations

import json
import hashlib
import tempfile
import unittest
from pathlib import Path

from Tools.AssetFactory.citylab_factory import (
    build_inventory,
    discover_unregistered_sources,
    publish_from_manifest,
    render_json,
    sha256_file,
    validate_admission_profile,
    validate_recipe,
    validate_construction_contract,
    validate_texture_recipe,
)


class InventoryTests(unittest.TestCase):
    project_root = Path(__file__).resolve().parents[2]

    def test_inventory_is_deterministic_and_flags_unregistered_sources(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            registered = root / "Assets" / "Publisher" / "Pack"
            unregistered = root / "Assets" / "New Pack"
            registered.mkdir(parents=True)
            unregistered.mkdir(parents=True)
            (registered / "house.fbx").write_bytes(b"house")
            (registered / "wall.png").write_bytes(b"wall")
            (registered / "house.prefab").write_text("prefab", encoding="utf-8")
            (registered / "house.fbx.meta").write_text("ignored", encoding="utf-8")
            (unregistered / "tower.obj").write_bytes(b"tower")

            config = {
                "assets_root": "Assets",
                "adapted_output_root": "Assets/CityLabHost/Adapted/Factory",
                "ignored_asset_roots": ["CityLabHost"],
                "discover_unregistered_asset_roots": True,
                "sources": [
                    {
                        "id": "publisher_pack",
                        "path": "Assets/Publisher",
                        "provenance": {
                            "store_url": "https://example.invalid/pack",
                            "license_status": "verified",
                        },
                    }
                ],
            }

            first = build_inventory(config, root)
            second = build_inventory(config, root)

            self.assertEqual(render_json(first), render_json(second))
            self.assertEqual(first["summary"]["model_candidates"], 2)
            self.assertEqual(first["summary"]["texture_candidates"], 1)
            self.assertEqual(first["sources"][0]["warnings"], [
                "source_non_enregistree",
                "url_store_manquante",
                "licence_a_verifier",
            ])
            publisher = first["sources"][1]
            self.assertEqual(publisher["counts"]["prefabs"], 1)
            self.assertEqual(publisher["models"][0]["sha256"], sha256_file(registered / "house.fbx"))

    def test_recipe_rejects_hash_drift_and_output_outside_adapted(self) -> None:
        inventory = {
            "sources": [
                {
                    "registered": True,
                    "provenance": {"license_status": "verified"},
                    "models": [{"path": "Assets/Vendor/wall.fbx", "sha256": "expected"}],
                }
            ]
        }
        recipe = {
            "schema": 1,
            "id": "building_house_frontier_01",
            "seed": 42,
            "status": "draft",
            "planned_output": "Assets/Vendor/overwritten.fbx",
            "inputs": [{"role": "wall", "path": "Assets/Vendor/wall.fbx", "sha256": "changed"}],
        }

        errors = validate_recipe(recipe, inventory, "Assets/CityLabHost/Adapted/Factory")

        self.assertIn("sortie_hors_adapted_factory", errors)
        self.assertIn("entree_0_hash_incorrect", errors)

    def test_building_contract_requires_four_contiguous_stages_and_three_variants(self) -> None:
        recipe = {
            "category": "building",
            "construction_schema": "AssetFactory/Schemas/building_construction.schema.json",
            "construction": {
                "mode": "cumulative_layers",
                "stages": [
                    {"id": "foundation", "order": 1, "progress": [0.0, 0.25], "required_categories": ["stone"]},
                    {"id": "frame", "order": 2, "progress": [0.25, 0.55], "required_categories": ["wood"]},
                    {"id": "roof", "order": 3, "progress": [0.55, 0.8], "required_categories": ["roofing"]},
                    {"id": "details", "order": 4, "progress": [0.8, 1.0], "required_categories": ["fixtures"]},
                ],
            },
            "variants": [
                {"id": identifier, "seed_offset": index, "chimney": index % 2 == 0,
                 "palette": {"wood": "#110b05", "wood_highlight": "#412611",
                             "roof": "#27140c", "roof_accent": "#55321c"}}
                for index, identifier in enumerate(("a", "b", "c"))
            ],
        }

        self.assertEqual([], validate_construction_contract(recipe))
        recipe["construction"]["stages"][2]["progress"] = [0.6, 0.8]
        recipe["variants"] = recipe["variants"][:2]
        errors = validate_construction_contract(recipe)
        self.assertIn("phase_3_progression_invalide", errors)
        self.assertIn("variantes_insuffisantes", errors)

    def test_building_pilot_declares_coherent_envelopes_and_identity_markers(self) -> None:
        catalog = json.loads((self.project_root / "AssetFactory/Catalogs/building_pilot.json")
                             .read_text(encoding="utf-8"))
        families = {family["function"]: family for family in catalog["families"]}
        self.assertEqual(7, len(families))
        self.assertEqual("dressed_stone", families["chapel"]["wall_system"])
        self.assertEqual("brick_stone", families["blacksmith"]["wall_system"])
        self.assertEqual("timber_plank", families["granary"]["wall_system"])
        self.assertEqual("timber_plank", families["barn"]["wall_system"])
        for family in families.values():
            self.assertIn(family["roof_system"], {"clay_tile", "wood_shingle", "slate"})
            self.assertGreaterEqual(len(family["identity_markers"]), 5)

    def test_character_proposals_cover_all_roles_and_source_hash(self) -> None:
        catalog = json.loads((self.project_root / "AssetFactory/Catalogs/character_proposals.json")
                             .read_text(encoding="utf-8"))
        proposals = catalog["proposals"]
        self.assertEqual(8, len(proposals))
        self.assertEqual(
            {"worker", "wealthy", "peasant", "religious", "soldier", "noble", "bourgeois", "beggar"},
            {proposal["role"] for proposal in proposals},
        )
        self.assertEqual({"male", "female"}, {proposal["gender"] for proposal in proposals})
        self.assertEqual({"child", "adult", "elder"}, {proposal["age"] for proposal in proposals})
        source = self.project_root / catalog["source"]["path"]
        self.assertEqual(catalog["source"]["sha256"], sha256_file(source))

    def test_character_factory_publishes_complete_rigged_matrix(self) -> None:
        catalog = json.loads((self.project_root / "AssetFactory/Catalogs/character_factory.json")
                             .read_text(encoding="utf-8"))
        manifest = json.loads((self.project_root / "AssetFactory/Manifests/character_factory.json")
                              .read_text(encoding="utf-8"))
        self.assertEqual(6, len(catalog["body_bases"]))
        self.assertEqual(4, len(catalog["morphologies"]))
        self.assertEqual(8, len(catalog["role_capsules"]))
        self.assertEqual({"male", "female"}, set(catalog["genders"]))
        self.assertEqual({"child", "adult", "elder"}, set(catalog["ages"]))
        self.assertEqual(set(catalog["social_roles"]),
                         {role["id"] for role in catalog["role_capsules"]})
        self.assertEqual(52, catalog["rig"]["expected_deform_bones"])
        self.assertEqual(24, manifest["contract"]["body_assets"])
        self.assertEqual(8, manifest["contract"]["role_capsules"])
        self.assertEqual(32, len(manifest["assets"]))
        self.assertEqual(32, len({asset["canonical_sha256"] for asset in manifest["assets"]}))
        self.assertFalse(manifest["gates"]["unity_launched"])
        self.assertFalse(manifest["gates"]["unity_humanoid_import"])
        for asset in manifest["assets"]:
            path = self.project_root / asset["path"]
            self.assertTrue(path.is_file())
            self.assertTrue(asset["path"].startswith("Assets/CityLabHost/Adapted/Factory/Characters/"))
            self.assertEqual(asset["fbx_sha256"], sha256_file(path))
            self.assertEqual(3, len(asset["lod_triangles"]))

    def test_admission_discovers_new_pack_and_validates_production_profile(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            pack = root / "Assets" / "New Medieval Pack"
            pack.mkdir(parents=True)
            (pack / "building.fbx").write_bytes(b"new-pack")
            config = {
                "assets_root": "Assets",
                "adapted_output_root": "Assets/CityLabHost/Adapted/Factory",
                "ignored_asset_roots": ["CityLabHost"],
                "discover_unregistered_asset_roots": True,
                "sources": [],
            }
            candidates = discover_unregistered_sources(config, root)
            self.assertEqual(1, len(candidates))
            self.assertEqual("Assets/New Medieval Pack", candidates[0]["path"])
            self.assertEqual(1, candidates[0]["model_candidates"])
            self.assertIn("licence_a_verifier", candidates[0]["warnings"])

        config = json.loads((self.project_root / "AssetFactory/config.json")
                            .read_text(encoding="utf-8"))
        inventory = json.loads((self.project_root / "AssetFactory/Reports/source_inventory.json")
                               .read_text(encoding="utf-8"))
        profile = json.loads((self.project_root /
                             "AssetFactory/AdmissionProfiles/ganzse_free_modular_character.json")
                             .read_text(encoding="utf-8"))
        self.assertEqual([], validate_admission_profile(profile, inventory, config))
        profile["source"]["provenance"]["license_status"] = "unknown"
        self.assertIn("licence_non_verifiee",
                      validate_admission_profile(profile, inventory, config))

    def test_generic_publication_is_dry_run_by_default_and_atomic_when_explicit(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            source = root / "AssetFactory/Workbench/Example/model.fbx"
            source.parent.mkdir(parents=True)
            source.write_bytes(b"generated-model")
            destination = root / "Assets/CityLabHost/Adapted/Factory/Example/model.fbx"
            manifest = root / "AssetFactory/Manifests/example.json"
            manifest.parent.mkdir(parents=True)
            manifest.write_text(json.dumps({
                "status": "generated",
                "status_after_publication": "published",
                "assets": [{
                    "generated_path": "AssetFactory/Workbench/Example/model.fbx",
                    "path": "Assets/CityLabHost/Adapted/Factory/Example/model.fbx",
                    "fbx_sha256": sha256_file(source),
                }]
            }), encoding="utf-8")
            config = {"adapted_output_root": "Assets/CityLabHost/Adapted/Factory"}

            self.assertEqual(0, publish_from_manifest(config, root, manifest, False))
            self.assertFalse(destination.exists())
            self.assertEqual(0, publish_from_manifest(config, root, manifest, True))
            self.assertEqual(source.read_bytes(), destination.read_bytes())
            self.assertFalse(destination.with_suffix(".fbx.tmp").exists())
            published_manifest = json.loads(manifest.read_text(encoding="utf-8"))
            self.assertEqual("published", published_manifest["status"])
            self.assertEqual("explicit_atomic_copy", published_manifest["publication"]["mode"])

    def test_pbr_trim_has_six_coherent_published_maps_and_three_review_scales(self) -> None:
        recipe = json.loads((self.project_root / "AssetFactory/Recipes/texture_citylab_trim_v1.json")
                            .read_text(encoding="utf-8"))
        manifest = json.loads((self.project_root / "AssetFactory/Manifests/citylab_trim_v1.json")
                              .read_text(encoding="utf-8"))
        report = json.loads((self.project_root / "AssetFactory/Reports/Textures/citylab_trim_v1.json")
                            .read_text(encoding="utf-8"))
        self.assertEqual([], validate_texture_recipe(
            recipe, "Assets/CityLabHost/Adapted/Factory"))
        self.assertEqual(
            {"BaseColor", "Normal", "AO", "Roughness", "Metallic", "VariationMask"},
            set(recipe["maps"]),
        )
        self.assertEqual(6, len(manifest["assets"]))
        self.assertTrue(manifest["gates"]["determinism"])
        self.assertEqual("published_pending_unity_material_validation", manifest["status"])
        self.assertEqual({"512", "256", "128"}, set(report["contrast_by_resolution"]))
        for resolution in report["contrast_by_resolution"].values():
            self.assertTrue(all(value >= recipe["budgets"]["contrast_min"]
                                for value in resolution.values()))
        for asset in manifest["assets"]:
            generated = self.project_root / asset["generated_path"]
            published = self.project_root / asset["path"]
            self.assertEqual(asset["sha256"], sha256_file(generated))
            self.assertEqual(asset["sha256"], sha256_file(published))
            self.assertEqual([2048, 2048], asset["resolution"])

        invalid_recipe = json.loads(json.dumps(recipe))
        invalid_recipe["maps"].remove("AO")
        invalid_recipe["outputs"]["published"] = "Assets/Vendor/Textures"
        errors = validate_texture_recipe(invalid_recipe, "Assets/CityLabHost/Adapted/Factory")
        self.assertIn("cartes_pbr_invalides", errors)
        self.assertIn("sortie_hors_adapted_factory", errors)

    def test_factory_qa_covers_every_published_fbx_and_texture(self) -> None:
        report_path = self.project_root / "AssetFactory/Reports/factory_qa.json"
        board_path = self.project_root / "AssetFactory/Reports/QA/factory_review_board.png"
        report = json.loads(report_path.read_text(encoding="utf-8"))
        manifest = json.loads((self.project_root / "AssetFactory/Manifests/factory_qa.json")
                              .read_text(encoding="utf-8"))

        self.assertEqual("technical_pass_artistic_and_unity_gates_pending", report["status"])
        self.assertEqual(24, report["summary"]["building_fbx"])
        self.assertEqual(32, report["summary"]["character_fbx"])
        self.assertEqual(6, report["summary"]["texture_maps"])
        self.assertEqual(report["summary"]["fbx_meshes"], report["summary"]["uv_meshes"])
        self.assertEqual(0, report["summary"]["embedded_colliders"])
        self.assertEqual("passed_all_fbx_meshes", report["gates"]["uv"])
        self.assertEqual("passed_none_embedded", report["gates"]["colliders"])
        self.assertEqual("pending_user_review", report["gates"]["artistic_approval"])
        self.assertFalse(report["unity_launched"])
        self.assertEqual(sha256_file(report_path), manifest["report_sha256"])
        self.assertEqual(sha256_file(board_path), manifest["review_board_sha256"])

    def test_legacy_save_fixture_has_valid_checksum_and_complete_state_axes(self) -> None:
        fixture = json.loads((self.project_root /
                             "Packages/com.victoria.citymode/Tests/Fixtures/city_save_v0.json")
                            .read_text(encoding="utf-8"))
        payload = fixture["payload"]
        self.assertEqual(
            fixture["payloadSha256"],
            hashlib.sha256(payload.encode("utf-8")).hexdigest(),
        )
        snapshot = json.loads(payload)
        self.assertEqual(0, snapshot["schemaVersion"])
        self.assertEqual(1001, snapshot["cityId"])
        self.assertIn("elapsedSeconds", snapshot)
        self.assertIn("stockWood", snapshot)
        self.assertIn("reservedWood", snapshot)
        for collection in ("households", "roads", "parcels", "buildings",
                           "villagers", "productionSites"):
            self.assertIn(collection, snapshot)


if __name__ == "__main__":
    unittest.main()
