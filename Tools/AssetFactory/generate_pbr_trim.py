"""Generate a deterministic CityLab wood/stone/roof PBR trim sheet."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def save_rgb(path: Path, array: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(np.clip(array, 0, 255).astype(np.uint8), "RGB").save(path, optimize=True)


def save_gray(path: Path, array: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(np.clip(array, 0, 255).astype(np.uint8), "L").save(path, optimize=True)


def smooth(array: np.ndarray, radius: float) -> np.ndarray:
    image = Image.fromarray(np.clip(array * 255.0, 0, 255).astype(np.uint8), "L")
    return np.asarray(image.filter(ImageFilter.GaussianBlur(radius=radius)), dtype=np.float32) / 255.0


def multiscale_noise(rng: np.random.Generator, size: int) -> tuple[np.ndarray, np.ndarray]:
    micro = rng.random((size, size), dtype=np.float32)
    coarse_size = max(16, size // 32)
    coarse = rng.random((coarse_size, coarse_size), dtype=np.float32)
    macro = np.asarray(Image.fromarray((coarse * 255).astype(np.uint8), "L").resize(
        (size, size), Image.Resampling.BICUBIC), dtype=np.float32) / 255.0
    return smooth(micro, 1.2), smooth(macro, 3.0)


def build_maps(recipe: dict) -> dict[str, np.ndarray]:
    size = recipe["resolution"]
    rng = np.random.default_rng(recipe["seed"])
    micro, macro = multiscale_noise(rng, size)
    yy, xx = np.mgrid[0:size, 0:size]
    base = np.zeros((size, size, 3), dtype=np.float32)
    height = np.zeros((size, size), dtype=np.float32)
    roughness = np.zeros((size, size), dtype=np.float32)
    metallic = np.zeros((size, size), dtype=np.float32)
    edges = np.zeros((size, size), dtype=np.float32)
    material_id = np.zeros((size, size), dtype=np.float32)
    palette = {key: np.asarray(value, dtype=np.float32) for key, value in recipe["palette"].items()}

    one_third = size // 3
    two_thirds = size * 2 // 3

    # Structural wood: broad vertical planks, fine longitudinal grain and iron nails.
    wood = yy < one_third
    plank_width = max(64, size // 8)
    seam = wood & (((xx % plank_width) < max(5, size // 256)) |
                   ((xx % plank_width) > plank_width - max(5, size // 256)))
    grain = 0.5 + 0.5 * np.sin(yy * 0.105 + np.sin(xx * 0.022) * 2.2)
    wood_mix = np.clip(0.20 + 0.48 * macro + 0.16 * grain + 0.10 * micro, 0, 1)
    base[wood] = palette["wood"] + (palette["wood_highlight"] - palette["wood"]) * wood_mix[wood, None]
    height[wood] = 0.55 + (grain[wood] - 0.5) * 0.075 + (macro[wood] - 0.5) * 0.10
    height[seam] = 0.16
    base[seam] *= 0.38
    edges[seam] = 1.0
    roughness[wood] = 0.69 + (micro[wood] - 0.5) * 0.13
    material_id[wood] = 0.18
    nail_radius = max(4, size // 300)
    for x in range(plank_width // 2, size, plank_width):
        for y in (one_third // 5, one_third * 4 // 5):
            nail = wood & ((xx - x) ** 2 + (yy - y) ** 2 <= nail_radius ** 2)
            base[nail] = palette["iron"]
            height[nail] = 0.68
            metallic[nail] = 0.88
            roughness[nail] = 0.34

    # Dressed stone: alternating courses with deterministic block variation and deep mortar.
    stone = (yy >= one_third) & (yy < two_thirds)
    stone_y = yy - one_third
    course_h = max(72, size // 18)
    block_w = max(150, size // 9)
    row = stone_y // course_h
    shifted_x = (xx + (row % 2) * (block_w // 2)) % block_w
    mortar_w = max(5, size // 300)
    mortar = stone & (((stone_y % course_h) < mortar_w) |
                      (shifted_x < mortar_w) | (shifted_x > block_w - mortar_w))
    cell = ((row * 17 + (xx + (row % 2) * block_w // 2) // block_w * 31) % 13) / 12.0
    stone_tone = np.clip(0.64 + (cell - 0.5) * 0.24 + (macro - 0.5) * 0.22, 0.28, 0.94)
    base[stone] = palette["stone"] * stone_tone[stone, None]
    base[mortar] = palette["mortar"] * (0.78 + micro[mortar, None] * 0.12)
    height[stone] = 0.61 + (macro[stone] - 0.5) * 0.16 + (micro[stone] - 0.5) * 0.05
    height[mortar] = 0.12
    edges[mortar] = 1.0
    roughness[stone] = 0.84 + (micro[stone] - 0.5) * 0.10
    roughness[mortar] = 0.95
    material_id[stone] = 0.52

    # Roof: offset overlapping clay tiles with readable grooves at RTS distance.
    roof = yy >= two_thirds
    roof_y = yy - two_thirds
    tile_h = max(72, size // 18)
    tile_w = max(96, size // 14)
    tile_row = roof_y // tile_h
    roof_x = (xx + (tile_row % 2) * (tile_w // 2)) % tile_w
    groove_w = max(5, size // 300)
    overlap = roof & ((roof_y % tile_h) < groove_w)
    groove = roof & ((roof_x < groove_w) | (roof_x > tile_w - groove_w) | overlap)
    ramp = (roof_y % tile_h) / float(tile_h)
    roof_mix = np.clip(0.25 + macro * 0.48 + micro * 0.13 + ramp * 0.14, 0, 1)
    base[roof] = palette["roof"] + (palette["roof_highlight"] - palette["roof"]) * roof_mix[roof, None]
    height[roof] = 0.43 + ramp[roof] * 0.24 + (macro[roof] - 0.5) * 0.07
    height[groove] = 0.14
    base[groove] *= 0.52
    edges[groove] = 1.0
    roughness[roof] = 0.74 + (micro[roof] - 0.5) * 0.12
    material_id[roof] = 0.86

    height = np.clip(height, 0, 1)
    gy, gx = np.gradient(height)
    strength = 7.5
    nx, ny, nz = -gx * strength, -gy * strength, np.ones_like(height)
    length = np.sqrt(nx * nx + ny * ny + nz * nz)
    normal = np.stack(((nx / length * 0.5 + 0.5) * 255,
                       (ny / length * 0.5 + 0.5) * 255,
                       (nz / length * 0.5 + 0.5) * 255), axis=2)
    blurred = smooth(height, max(2.0, size / 512.0))
    concavity = np.clip(blurred - height, 0, 0.5)
    ao = np.clip(1.0 - concavity * 1.7 - edges * 0.24, 0.22, 1.0) * 255
    variation = np.stack((np.clip(macro * 0.72 + micro * 0.28, 0, 1) * 255,
                          edges * 255, material_id * 255), axis=2)
    return {
        "BaseColor": base,
        "Normal": normal,
        "AO": ao,
        "Roughness": np.clip(roughness, 0, 1) * 255,
        "Metallic": np.clip(metallic, 0, 1) * 255,
        "VariationMask": variation,
    }


def review_sheet(basecolor_path: Path, resolutions: list[int], output: Path) -> dict[str, dict[str, float]]:
    source = Image.open(basecolor_path).convert("RGB")
    panel = 512
    font_path = Path("C:/Windows/Fonts/arial.ttf")
    font = ImageFont.truetype(font_path, 20) if font_path.is_file() else ImageFont.load_default(size=20)
    sheet = Image.new("RGB", (panel * len(resolutions), panel + 42), "#101820")
    draw = ImageDraw.Draw(sheet)
    metrics = {}
    for index, resolution in enumerate(resolutions):
        reduced = source.resize((resolution, resolution), Image.Resampling.LANCZOS)
        array = np.asarray(reduced, dtype=np.float32)
        thirds = np.array_split(array, 3, axis=0)
        metrics[str(resolution)] = {
            material: round(float(np.std(np.mean(region, axis=2))), 4)
            for material, region in zip(("wood", "stone", "roof"), thirds)
        }
        enlarged = reduced.resize((panel, panel), Image.Resampling.NEAREST)
        sheet.paste(enlarged, (index * panel, 0))
        label = f"{resolution} × {resolution}"
        bounds = draw.textbbox((0, 0), label, font=font)
        draw.text((index * panel + (panel - bounds[2] + bounds[0]) / 2, panel + 9),
                  label, font=font, fill="#F0E5D1")
    output.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output, optimize=True)
    return metrics


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", default=".")
    parser.add_argument("--recipe", default="AssetFactory/Recipes/texture_citylab_trim_v1.json")
    parser.add_argument("--verify-determinism", action="store_true")
    args = parser.parse_args()
    root = Path(args.project_root).resolve()
    recipe = json.loads((root / args.recipe).read_text(encoding="utf-8"))
    manifest_path = root / recipe["outputs"]["manifest"]
    previous_hashes = None
    if args.verify_determinism and manifest_path.is_file():
        previous = json.loads(manifest_path.read_text(encoding="utf-8"))
        previous_hashes = {asset["id"]: asset["sha256"] for asset in previous.get("assets", [])}
    maps = build_maps(recipe)
    workbench = root / recipe["outputs"]["workbench"]
    names = {
        "BaseColor": "CityLabTrim_BaseColor.png",
        "Normal": "CityLabTrim_Normal.png",
        "AO": "CityLabTrim_AO.png",
        "Roughness": "CityLabTrim_Roughness.png",
        "Metallic": "CityLabTrim_Metallic.png",
        "VariationMask": "CityLabTrim_VariationMask.png",
    }
    for map_name, filename in names.items():
        path = workbench / filename
        if maps[map_name].ndim == 3:
            save_rgb(path, maps[map_name])
        else:
            save_gray(path, maps[map_name])
    contrast = review_sheet(workbench / names["BaseColor"], recipe["review_resolutions"],
                            workbench / "CityLabTrim_ResolutionReview.png")
    if any(value < recipe["budgets"]["contrast_min"]
           for resolution in contrast.values() for value in resolution.values()):
        raise RuntimeError(f"Trim readability below budget: {contrast}")

    assets = []
    total_bytes = 0
    for map_name, filename in names.items():
        source = workbench / filename
        total_bytes += source.stat().st_size
        assets.append({
            "id": map_name,
            "generated_path": source.relative_to(root).as_posix(),
            "path": (root / recipe["outputs"]["published"] / filename).relative_to(root).as_posix(),
            "sha256": sha256(source),
            "bytes": source.stat().st_size,
            "resolution": [recipe["resolution"], recipe["resolution"]],
        })
    if total_bytes > recipe["budgets"]["total_bytes_max"]:
        raise RuntimeError(f"Texture budget exceeded {total_bytes}")
    report = {
        "schema": 1,
        "id": recipe["id"],
        "status": "generated_validated_pending_publication",
        "seed": recipe["seed"],
        "resolution": recipe["resolution"],
        "maps": list(names),
        "total_bytes": total_bytes,
        "budget_bytes": recipe["budgets"]["total_bytes_max"],
        "contrast_by_resolution": contrast,
        "review": (workbench / "CityLabTrim_ResolutionReview.png").relative_to(root).as_posix(),
        "unity_launched": False,
    }
    report_path = root / recipe["outputs"]["report"]
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    manifest = {
        "schema": 1,
        "id": recipe["id"],
        "status": "generated_pending_explicit_publication",
        "status_after_publication": "published_pending_unity_material_validation",
        "recipe": Path(args.recipe).as_posix(),
        "graph": "AssetFactory/Graphs/citylab_trim_pbr_graph.json",
        "unity_launched": False,
        "gates": {
            "maps_generated": True,
            "resolution_review": True,
            "determinism": False,
            "unity_material_import": False,
            "building_application_review": False,
        },
        "assets": assets,
    }
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    if args.verify_determinism:
        current_hashes = {asset["id"]: asset["sha256"] for asset in assets}
        if previous_hashes is None or previous_hashes != current_hashes:
            raise RuntimeError("PBR trim determinism verification failed")
        manifest["gates"]["determinism"] = True
    temporary = manifest_path.with_suffix(".json.tmp")
    temporary.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    temporary.replace(manifest_path)
    print(
        "CITYLAB_PBR_TRIM_OK "
        f"maps={len(assets)} resolution={recipe['resolution']} bytes={total_bytes} "
        f"reviews=512,256,128 determinism={str(manifest['gates']['determinism']).lower()} "
        "unity_launched=false"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
