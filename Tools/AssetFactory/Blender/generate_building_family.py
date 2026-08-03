"""Génère les sept familles du pilote CityLab avec 4 phases et 3 LOD."""

from __future__ import annotations

import argparse
import json
import math
import random
import sys
from pathlib import Path

import bpy

sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_sawmill as kit


def arguments() -> argparse.Namespace:
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", required=True)
    parser.add_argument("--catalog", required=True)
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--family", required=True)
    parser.add_argument("--variant", required=True)
    parser.add_argument("--skip-previews", action="store_true")
    return parser.parse_args(argv)


def foundation(width: float, depth: float, stone: bpy.types.Material, rng: random.Random) -> None:
    kit.box("foundation_core", (0.0, 0.0, 0.30), (width, depth, 0.58), stone, bevel=0.10)
    spacing = 0.9
    for side in (-1, 1):
        count = max(3, round(width / spacing))
        for index in range(count):
            x = -width * 0.5 + (index + 0.5) * width / count
            kit.box(f"foundation_front_{side}_{index}",
                    (x, side * depth * 0.5, 0.42 + rng.uniform(-0.035, 0.035)),
                    (width / count * 0.88, 0.38, 0.48 + rng.uniform(-0.04, 0.05)),
                    stone, rotation=(0.0, 0.0, rng.uniform(-0.025, 0.025)), bevel=0.09)
        count = max(3, round(depth / spacing))
        for index in range(count):
            y = -depth * 0.5 + (index + 0.5) * depth / count
            kit.box(f"foundation_side_{side}_{index}",
                    (side * width * 0.5, y, 0.42),
                    (0.38, depth / count * 0.88, 0.48 + rng.uniform(-0.04, 0.05)),
                    stone, bevel=0.09)


def closed_shell(width: float, depth: float, wall: float,
                 material: bpy.types.Material) -> None:
    """Create four real wall planes; doors and windows are applied as architectural layers."""
    z = wall * 0.5 + 0.58
    kit.box("front_wall", (0.0, -depth * 0.5 + 0.24, z),
            (width - 0.54, 0.30, wall), material, bevel=0.035)
    kit.box("rear_wall", (0.0, depth * 0.5 - 0.24, z),
            (width - 0.54, 0.30, wall), material, bevel=0.035)
    kit.box("left_wall", (-width * 0.5 + 0.24, 0.0, z),
            (0.30, depth - 0.54, wall), material, bevel=0.035)
    kit.box("right_wall", (width * 0.5 - 0.24, 0.0, z),
            (0.30, depth - 0.54, wall), material, bevel=0.035)


def masonry_courses(width: float, depth: float, height: float,
                    material: bpy.types.Material, rng: random.Random,
                    brick: bool = False) -> None:
    """Add readable coursed masonry over the closed structural shell."""
    course_height = 0.34 if brick else 0.48
    unit_width = 0.72 if brick else 1.02
    courses = max(2, round(height / course_height))
    for course in range(courses):
        z = 0.68 + course * course_height
        offset = unit_width * 0.5 if course % 2 else 0.0
        count = max(4, math.ceil(width / unit_width) + 1)
        for index in range(count):
            x = -width * 0.5 + index * unit_width + offset
            if x > width * 0.5:
                continue
            kit.box(f"masonry_front_{course}_{index}",
                    (x, -depth * 0.5 - 0.025, z),
                    (unit_width * 0.91, 0.20, course_height * 0.82), material,
                    rotation=(0.0, 0.0, rng.uniform(-0.012, 0.012)),
                    bevel=0.0)
            kit.box(f"masonry_rear_{course}_{index}",
                    (x, depth * 0.5 + 0.025, z),
                    (unit_width * 0.91, 0.20, course_height * 0.82), material,
                    rotation=(0.0, 0.0, rng.uniform(-0.012, 0.012)),
                    bevel=0.0)
        count = max(4, math.ceil(depth / unit_width) + 1)
        for index in range(count):
            y = -depth * 0.5 + index * unit_width + offset
            if y > depth * 0.5:
                continue
            for side in (-1, 1):
                kit.box(f"masonry_side_{side}_{course}_{index}",
                        (side * (width * 0.5 + 0.025), y, z),
                        (0.20, unit_width * 0.91, course_height * 0.82), material,
                        bevel=0.0)


def timber_cladding(width: float, depth: float, wall: float,
                    material: bpy.types.Material) -> None:
    """Individual boards keep agricultural buildings readable at RTS distance."""
    board = 0.48
    z = wall * 0.5 + 0.58
    front_count = max(8, math.ceil(width / board))
    side_count = max(8, math.ceil(depth / board))
    for index in range(front_count):
        x = -width * 0.5 + (index + 0.5) * width / front_count
        for side in (-1, 1):
            kit.box(f"front_board_{side}_{index}", (x, side * (depth * 0.5 + 0.015), z),
                    (width / front_count * 0.92, 0.16, wall - 0.12), material, bevel=0.025)
    for index in range(side_count):
        y = -depth * 0.5 + (index + 0.5) * depth / side_count
        for side in (-1, 1):
            kit.box(f"side_board_{side}_{index}", (side * (width * 0.5 + 0.015), y, z),
                    (0.16, depth / side_count * 0.92, wall - 0.12), material, bevel=0.025)


def frame(width: float, depth: float, wall: float, function: str, wall_system: str,
          materials: dict[str, bpy.types.Material], style: str,
          rng: random.Random) -> None:
    wood = materials["wood"]
    plaster = materials["plaster"]
    half_w, half_d = width * 0.5 - 0.28, depth * 0.5 - 0.28
    shell_material = materials["stone"] if wall_system == "dressed_stone" else (
        materials["brick"] if wall_system == "brick_stone" else plaster)
    closed_shell(width, depth, wall, shell_material)
    if wall_system == "dressed_stone":
        masonry_courses(width, depth, wall - 0.12, materials["limestone"], rng)
    elif wall_system == "brick_stone":
        masonry_courses(width, depth, wall - 0.12, materials["brick"], rng, brick=True)
        masonry_courses(width, depth, 1.0, materials["stone"], rng)
    elif wall_system == "timber_plank":
        timber_cladding(width, depth, wall, materials["wood_highlight"])
    elif wall_system == "stone_timber":
        masonry_courses(width, depth, 1.35, materials["stone"], rng)
    positions = [(-half_w, -half_d), (half_w, -half_d), (-half_w, half_d), (half_w, half_d)]
    for index, (x, y) in enumerate(positions):
        kit.box(f"corner_post_{index}", (x, y, wall * 0.5 + 0.55),
                (0.34, 0.34, wall), wood, bevel=0.055)
    if wall_system in {"stone_timber", "timber_plank"}:
        for y in (-depth * 0.5 - 0.04, depth * 0.5 + 0.04):
            for x in (-width * 0.5 + 0.32, -width * 0.25, 0.0,
                      width * 0.25, width * 0.5 - 0.32):
                kit.box("exterior_facade_post", (x, y, wall * 0.5 + 0.55),
                        (0.20, 0.18, wall - 0.18), wood, bevel=0.03)
        for x in (-width * 0.5 - 0.04, width * 0.5 + 0.04):
            for y in (-depth * 0.25, 0.0, depth * 0.25):
                kit.box("exterior_side_post", (x, y, wall * 0.5 + 0.55),
                        (0.18, 0.20, wall - 0.18), wood, bevel=0.03)
        front_y = -depth * 0.5 - 0.14
        kit.beam_between("front_brace_left", (-width * 0.5 + 0.40, front_y, 0.95),
                         (-width * 0.25, front_y, wall - 0.05), 0.16, wood, 0.022)
        if style != "classic":
            kit.beam_between("front_brace_right", (width * 0.5 - 0.40, front_y, 0.95),
                             (width * 0.25, front_y, wall - 0.05), 0.16, wood, 0.022)
    for side in (-1, 1):
        kit.beam_between(f"eave_beam_{side}", (-half_w, side * half_d, wall + 0.5),
                         (half_w, side * half_d, wall + 0.5), 0.34, wood, 0.05)
    for side in (-1, 1):
        kit.beam_between(f"side_beam_{side}", (side * half_w, -half_d, wall + 0.5),
                         (side * half_w, half_d, wall + 0.5), 0.30, wood, 0.045)
    stud_count = 5 if style == "dense" else 3
    for index in range(stud_count):
        x = -half_w + (index + 1) * (2 * half_w) / (stud_count + 1)
        kit.box(f"rear_stud_{index}", (x, half_d - 0.13, wall * 0.5 + 0.55),
                (0.19, 0.20, wall - 0.15), wood, bevel=0.025)
    braces = [((-half_w, half_d - 0.15, 0.85), (0.0, half_d - 0.15, wall)),
              ((half_w, half_d - 0.15, 0.85), (0.0, half_d - 0.15, wall))]
    if style == "classic":
        braces = braces[:1]
    for index, (start, end) in enumerate(braces):
        kit.beam_between(f"rear_brace_{index}", start, end, 0.18, wood, 0.025)
    gable_material = materials["limestone"] if function == "chapel" else plaster
    kit.triangular_prism("front_gable", -half_d, width - 0.35, wall + 0.35,
                         wall + 3.0, 0.18, gable_material)
    kit.triangular_prism("rear_gable", half_d, width - 0.35, wall + 0.35,
                         wall + 3.0, 0.18, gable_material)
    for y in (-half_d - 0.02, half_d + 0.02):
        kit.beam_between("gable_left", (-half_w, y, wall + 0.5),
                         (0.0, y, wall + 3.0), 0.22, wood, 0.035)
        kit.beam_between("gable_right", (half_w, y, wall + 0.5),
                         (0.0, y, wall + 3.0), 0.22, wood, 0.035)
        kit.beam_between("gable_center", (0.0, y, wall + 0.45),
                         (0.0, y, wall + 2.9), 0.20, wood, 0.03)
    if function == "chapel":
        for y in (-depth * 0.34, 0.0, depth * 0.34):
            for side in (-1, 1):
                kit.box("chapel_buttress", (side * (width * 0.5 + 0.30), y, 1.75),
                        (0.72, 0.95, 3.0), materials["limestone"], bevel=0.09)
                kit.box("chapel_buttress_cap", (side * (width * 0.5 + 0.30), y, 3.26),
                        (0.88, 1.10, 0.22), materials["stone"], bevel=0.06)


def roof(width: float, depth: float, wall: float, rise: float,
         roof_mat: bpy.types.Material, accent: bpy.types.Material,
         iron: bpy.types.Material, rng: random.Random) -> None:
    half = width * 0.5 + 0.48
    slope = math.sqrt(half * half + rise * rise)
    angle = math.atan2(rise, half)
    for side in (-1, 1):
        kit.box(f"roof_plane_{side}", (side * half * 0.5, 0.0, wall + 0.48 + rise * 0.5),
                (slope, depth + 0.95, 0.22), roof_mat,
                rotation=(0.0, side * angle, 0.0), bevel=0.04)
        rows = max(7, round(half / 0.55))
        columns = max(7, round(depth / 0.78))
        for row in range(rows):
            distance = 0.30 + row * (half - 0.45) / max(1, rows - 1)
            x = side * distance
            z = wall + 0.48 + rise - distance * rise / half + 0.15
            for column in range(columns):
                y = -depth * 0.5 + 0.38 + column * depth / columns
                material = accent if rng.random() < 0.22 else roof_mat
                kit.box(f"shingle_{side}_{row}_{column}",
                        (x + side * rng.uniform(-0.018, 0.018), y + rng.uniform(-0.025, 0.025), z),
                        (0.58, depth / columns * 1.12, 0.08), material,
                        rotation=(0.0, side * angle, rng.uniform(-0.008, 0.008)), bevel=0.018)
    kit.cylinder("ridge", (0.0, 0.0, wall + rise + 0.60), 0.12, depth + 1.05,
                 iron, rotation=(math.pi / 2, 0.0, 0.0), vertices=12)


def door_and_windows(width: float, depth: float, wall: float,
                     wood: bpy.types.Material, iron: bpy.types.Material,
                     warm: bpy.types.Material, wide: bool = False,
                     windows: bool = True) -> None:
    front = -depth * 0.5 - 0.13
    door_width = 2.65 if wide else 1.35
    kit.box("front_door", (0.0, front, 1.85), (door_width, 0.18, 2.65), wood, bevel=0.07)
    kit.box("door_lintel", (0.0, front - 0.05, 3.28), (door_width + 0.55, 0.24, 0.28), wood, bevel=0.045)
    for side in (-1, 1):
        kit.box("door_jamb", (side * (door_width * 0.5 + 0.14), front - 0.05, 1.85),
                (0.24, 0.24, 2.85), wood, bevel=0.04)
    if wide:
        kit.box("double_door_seam", (0.0, front - 0.12, 1.85), (0.08, 0.08, 2.45), iron, bevel=0.012)
        for side in (-1, 1):
            kit.beam_between("door_cross_brace",
                             (side * 0.10, front - 0.15, 0.82),
                             (side * (door_width * 0.5 - 0.12), front - 0.15, 2.86),
                             0.12, wood, 0.018)
    for z in (1.25, 2.45):
        kit.box("door_hinge", (-door_width * 0.38, front - 0.14, z),
                (door_width * 0.34, 0.08, 0.10), iron, bevel=0.018)
    kit.cylinder("door_handle", (door_width * 0.28, front - 0.18, 1.85),
                 0.08, 0.16, iron, rotation=(math.pi / 2, 0.0, 0.0), vertices=10, bevel=0.012)
    for x in ((-width * 0.28, width * 0.28) if windows else ()):
        kit.box("window_glow", (x, front - 0.02, 2.75), (1.1, 0.10, 1.25), warm, bevel=0.04)
        kit.beam_between("window_bar_v", (x, front - 0.09, 2.15),
                         (x, front - 0.09, 3.35), 0.065, iron, 0.01)
        kit.beam_between("window_bar_h", (x - 0.5, front - 0.09, 2.75),
                         (x + 0.5, front - 0.09, 2.75), 0.065, iron, 0.01)
        for side in (-1, 1):
            kit.box("window_shutter", (x + side * 0.76, front - 0.08, 2.75),
                    (0.38, 0.16, 1.38), wood,
                    rotation=(0.0, 0.0, side * 0.055), bevel=0.035)
            kit.box("shutter_hinge", (x + side * 0.76, front - 0.18, 2.75),
                    (0.30, 0.06, 0.08), iron, bevel=0.012)


def hanging_emblem(name: str, location: tuple[float, float, float],
                   wood: bpy.types.Material, metal: bpy.types.Material,
                   symbol: str) -> None:
    x, y, z = location
    kit.beam_between(name + "_post", (x, y, z), (x, y, z + 2.5), 0.16, wood, 0.025)
    kit.beam_between(name + "_arm", (x, y, z + 2.35), (x + 1.15, y, z + 2.35), 0.14, wood, 0.02)
    kit.cylinder(name + "_ring", (x + 0.98, y, z + 1.60), 0.45, 0.10, metal,
                 rotation=(math.pi / 2, 0.0, 0.0), vertices=16, bevel=0.025)
    if symbol == "grain":
        kit.beam_between(name + "_stalk", (x + 0.98, y - 0.08, z + 1.20),
                         (x + 0.98, y - 0.08, z + 1.98), 0.07, metal, 0.012)
        for side in (-1, 1):
            for level in (1.38, 1.62, 1.84):
                kit.beam_between(name + "_grain", (x + 0.98, y - 0.08, z + level),
                                 (x + 0.98 + side * 0.22, y - 0.08, z + level + 0.15),
                                 0.065, metal, 0.01)
    elif symbol == "horseshoe":
        bpy.ops.mesh.primitive_torus_add(major_radius=0.27, minor_radius=0.065,
                                         major_segments=16, minor_segments=6,
                                         location=(x + 0.98, y - 0.08, z + 1.60),
                                         rotation=(math.pi / 2, 0.0, 0.0))
        emblem = kit.tag(bpy.context.object)
        emblem.name = name + "_horseshoe"
        emblem.data.materials.append(metal)
    elif symbol == "crate":
        kit.box(name + "_crate", (x + 0.98, y - 0.08, z + 1.60),
                (0.58, 0.12, 0.58), wood, rotation=(0.0, 0.0, math.pi / 4), bevel=0.035)


def chimney(height: float, x: float, stone: bpy.types.Material, iron: bpy.types.Material) -> None:
    kit.box("chimney", (x, 1.25, height - 0.6), (0.95, 0.95, 3.8), stone, bevel=0.10)
    kit.box("chimney_cap", (x, 1.25, height + 1.25), (1.28, 1.28, 0.22), iron, bevel=0.06)


def hay_bale(name: str, location: tuple[float, float, float], hay: bpy.types.Material) -> None:
    kit.cylinder(name, location, 0.62, 1.15, hay, rotation=(0.0, math.pi / 2, 0.0),
                 vertices=16, bevel=0.04)


def add_function_details(function: str, width: float, depth: float, wall: float, rise: float,
                         variant: dict, materials: dict[str, bpy.types.Material],
                         vendor_path: Path, rng: random.Random) -> None:
    wood, stone, iron = materials["wood"], materials["stone"], materials["iron"]
    pale, bronze = materials["pale_wood"], materials["bronze"]
    front = -depth * 0.5
    if function == "residence":
        door_and_windows(width, depth, wall, wood, iron, materials["ember"])
        kit.import_vendor_component(vendor_path, "vendor_porch", (0.0, front - 1.0, 0.55),
                                    width * 0.62, wood)
        if variant["chimney"]:
            chimney(wall + 3.0, width * 0.27, stone, iron)
        for x in (-width * 0.34, width * 0.34):
            kit.box("flower_box", (x, front - 0.30, 1.85), (1.25, 0.42, 0.34), wood, bevel=0.06)
        kit.box("house_bench", (-width * 0.28, front - 1.15, 0.72),
                (2.2, 0.60, 0.18), pale, bevel=0.05)
        for side in (-1, 1):
            kit.box("bench_leg", (-width * 0.28 + side * 0.72, front - 1.15, 0.43),
                    (0.16, 0.46, 0.58), wood, bevel=0.025)
        for index in range(5):
            kit.cylinder("house_firewood", (width * 0.38 + (index % 2) * 0.30,
                         front - 0.75 - (index // 2) * 0.28, 0.46),
                         0.14, 1.05, materials["bark"], rotation=(0.0, math.pi / 2, 0.0),
                         vertices=10, bevel=0.025)
    elif function == "granary":
        door_and_windows(width, depth, wall, wood, iron, materials["ember"], True)
        kit.import_vendor_component(vendor_path, "vendor_ladder", (width * 0.33, front - 0.45, 0.45),
                                    1.6, wood, 0.08)
        for x in (-1.65, 0.0, 1.65):
            kit.cylinder("grain_sack", (x, front - 0.75, 0.85), 0.48, 1.2,
                         materials["cloth"], vertices=14, bevel=0.08)
        kit.beam_between("hoist", (0.0, front - 0.30, wall - 0.2),
                         (0.0, front - 1.8, wall - 0.2), 0.18, wood, 0.025)
        kit.cylinder("hoist_pulley", (0.0, front - 1.82, wall - 0.2), 0.32, 0.16,
                     iron, rotation=(math.pi / 2, 0.0, 0.0), vertices=16, bevel=0.025)
        kit.beam_between("hoist_rope", (0.0, front - 1.90, wall - 0.2),
                         (0.0, front - 1.90, 1.0), 0.055, materials["rope"], 0.008)
        for x in (-width * 0.30, width * 0.30):
            for z in (3.75, 4.45):
                kit.box("grain_vent", (x, front - 0.18, z), (1.05, 0.12, 0.13), iron, bevel=0.02)
        hanging_emblem("granary_sign", (-width * 0.55, front - 0.42, 0.55), wood, bronze, "grain")
    elif function == "warehouse":
        door_and_windows(width, depth, wall, wood, iron, materials["ember"], True)
        kit.box("loading_dock", (0.0, front - 1.05, 0.75), (width * 0.72, 2.0, 0.65), stone, bevel=0.10)
        for index, x in enumerate((-2.3, 0.0, 2.3)):
            kit.import_vendor_component(vendor_path, f"vendor_crate_{index}",
                                        (x, front - 1.15, 1.05), 1.25, pale, index * 0.17)
        kit.box("loading_canopy", (0.0, front - 1.35, wall - 0.25),
                (width * 0.78, 2.6, 0.18), materials["roof"],
                rotation=(0.18, 0.0, 0.0), bevel=0.04)
        for index, x in enumerate((-width * 0.34, width * 0.34)):
            kit.cylinder("warehouse_barrel", (x, front - 1.40, 0.95), 0.52, 1.35,
                         wood, vertices=16, bevel=0.055)
            for z in (0.46, 0.95, 1.43):
                kit.cylinder("barrel_hoop", (x, front - 1.40, z), 0.55, 0.08,
                             iron, vertices=16, bevel=0.012)
        hanging_emblem("warehouse_sign", (-width * 0.54, front - 0.36, 0.55), wood, bronze, "crate")
        kit.cylinder("warehouse_pulley", (0.0, front - 1.58, wall - 0.35), 0.34, 0.16,
                     iron, rotation=(math.pi / 2, 0.0, 0.0), vertices=16, bevel=0.02)
    elif function == "market":
        door_and_windows(width, depth, wall, wood, iron, materials["ember"], True)
        for side in (-1, 1):
            x = side * width * 0.28
            kit.import_vendor_component(vendor_path, f"vendor_bench_{side}",
                                        (x, front - 1.3, 0.6), 2.4, wood, side * 0.08)
            kit.box("stall_counter", (x, front - 1.55, 1.25), (3.2, 1.0, 0.24), wood, bevel=0.06)
            kit.box("stall_awning", (x, front - 1.25, 3.45), (3.5, 2.2, 0.16),
                    materials["cloth"], rotation=(0.18, 0.0, 0.0), bevel=0.04)
        for x in (-2.8, -1.4, 0.0, 1.4, 2.8):
            kit.cylinder("market_basket", (x, front - 2.0, 0.65), 0.38, 0.52,
                         pale, vertices=14, bevel=0.04)
        for index, x in enumerate((-2.75, -2.48, -1.48, -1.22, 1.22, 1.48, 2.48, 2.75)):
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=0.20,
                                                  location=(x, front - 2.05, 0.98 + (index % 2) * 0.10))
            produce = kit.tag(bpy.context.object)
            produce.name = "market_produce"
            produce.data.materials.append(materials["produce_red"] if index % 3 else materials["produce_green"])
        hanging_emblem("market_sign", (-width * 0.55, front - 0.34, 0.55), wood, bronze, "crate")
    elif function == "blacksmith":
        door_and_windows(width, depth, wall, wood, iron, materials["ember"], True)
        kit.import_vendor_component(vendor_path, "vendor_stove", (-width * 0.28, 1.0, 0.55),
                                    2.2, stone)
        chimney(wall + 3.0, -width * 0.27, stone, iron)
        kit.box("forge_hearth", (width * 0.24, front - 0.65, 1.0), (2.5, 1.5, 1.25), stone, bevel=0.12)
        kit.box("forge_ember", (width * 0.24, front - 1.05, 1.55), (1.8, 0.75, 0.20), materials["ember"], bevel=0.04)
        kit.box("anvil_base", (-0.4, front - 1.7, 0.8), (0.75, 0.75, 1.1), wood, bevel=0.07)
        kit.box("anvil", (-0.4, front - 1.7, 1.45), (1.45, 0.55, 0.35), iron, bevel=0.06)
        for index in range(9):
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=0.22 + (index % 3) * 0.035,
                                                  location=(width * 0.38 + (index % 3) * 0.28,
                                                            front - 0.85 - (index // 3) * 0.26,
                                                            0.38 + (index % 2) * 0.16))
            coal = kit.tag(bpy.context.object)
            coal.name = "coal_heap"
            coal.data.materials.append(materials["coal"])
        hanging_emblem("blacksmith_sign", (-width * 0.54, front - 0.40, 0.55), wood, iron, "horseshoe")
        kit.box("tool_rack", (-width * 0.30, front - 0.28, 3.70), (2.2, 0.18, 0.18), wood, bevel=0.025)
        for index, x in enumerate((-width * 0.40, -width * 0.32, -width * 0.24)):
            kit.beam_between("smith_tool", (x, front - 0.40, 2.35),
                             (x + (index - 1) * 0.15, front - 0.40, 3.65), 0.075, iron, 0.012)
    elif function == "barn":
        door_and_windows(width, depth, wall, wood, iron, materials["ember"], True)
        kit.import_vendor_component(vendor_path, "vendor_hovel",
                                    (variant["annex_side"] * width * 0.55, 0.5, 0.5),
                                    width * 0.42, wood, variant["annex_side"] * 0.05)
        for index, x in enumerate((-2.4, -0.8, 0.8, 2.4)):
            hay_bale(f"hay_{index}", (x, front - 1.0, 0.9), materials["hay"])
        kit.box("loft_door", (0.0, front - 0.16, wall - 0.55), (2.4, 0.16, 1.8), wood, bevel=0.06)
        for side in (-1, 1):
            kit.beam_between("loft_cross_brace", (-1.02 * side, front - 0.26, wall - 1.20),
                             (1.02 * side, front - 0.26, wall + 0.10), 0.12, pale, 0.018)
        kit.box("animal_trough", (width * 0.34, front - 1.65, 0.65),
                (2.8, 0.85, 0.68), pale, bevel=0.08)
        for side in (-1, 1):
            kit.beam_between("pitchfork_tine", (-width * 0.38, front - 0.65, 0.45),
                             (-width * 0.38 + side * 0.18, front - 0.65, 1.15),
                             0.055, iron, 0.008)
        kit.beam_between("pitchfork_handle", (-width * 0.38, front - 0.65, 0.95),
                         (-width * 0.31, front - 0.65, 3.45), 0.075, pale, 0.012)
        for index in range(4):
            x = width * 0.52 + index * 0.85
            kit.box("pen_post", (x, 0.55, 0.80), (0.16, 0.16, 1.55), wood, bevel=0.025)
        for z in (0.55, 1.15):
            kit.beam_between("pen_rail", (width * 0.52, 0.55, z),
                             (width * 0.52 + 2.55, 0.55, z), 0.12, wood, 0.018)
    elif function == "chapel":
        door_and_windows(width, depth, wall, wood, iron, materials["ember"], windows=False)
        kit.import_vendor_component(vendor_path, "vendor_chapel_sign",
                                    (-width * 0.36, front - 0.65, 0.45), 1.7, pale)
        tower_base = wall + rise + 0.25
        tower_height = tower_base + 5.0
        kit.box("bell_tower", (0.0, 0.6, tower_base + 1.8), (2.7, 2.7, 3.6), stone, bevel=0.11)
        for side in (-1, 1):
            kit.box("bell_opening", (side * 0.72, -0.78, tower_base + 2.1),
                    (0.72, 0.12, 1.35), iron, bevel=0.08)
        bpy.ops.mesh.primitive_cone_add(vertices=4, radius1=2.05, radius2=0.0,
                                        depth=2.45, location=(0.0, 0.6, tower_base + 4.25),
                                        rotation=(0.0, 0.0, math.pi / 4))
        spire = kit.tag(bpy.context.object)
        spire.name = "tower_spire"
        spire.data.materials.append(materials["slate"])
        kit.cylinder("bell", (0.0, -0.82, tower_base + 2.05), 0.52, 0.72,
                     bronze, rotation=(math.pi / 2, 0.0, 0.0), vertices=16, bevel=0.05)
        kit.beam_between("cross_vertical", (0.0, 1.5, tower_height - 0.2),
                         (0.0, 1.5, tower_height + 1.45), 0.16, bronze, 0.025)
        kit.beam_between("cross_horizontal", (-0.62, 1.5, tower_height + 0.82),
                         (0.62, 1.5, tower_height + 0.82), 0.16, bronze, 0.025)
        kit.box("chapel_steps", (0.0, front - 0.80, 0.48), (3.2, 1.35, 0.40), materials["limestone"], bevel=0.08)
        for side in (-1, 1):
            x = side * width * 0.30
            kit.box("lancet_glow", (x, front - 0.25, 3.65), (0.72, 0.12, 2.15),
                    materials["ember"], bevel=0.22)
            kit.beam_between("lancet_frame_v", (x, front - 0.36, 2.65),
                             (x, front - 0.36, 4.70), 0.08, iron, 0.012)
            kit.beam_between("lancet_frame_h", (x - 0.31, front - 0.36, 3.55),
                             (x + 0.31, front - 0.36, 3.55), 0.08, iron, 0.012)
        for side in (-1, 1):
            x = side * (width * 0.58)
            kit.box("grave_marker", (x, front - 1.65, 0.75), (0.48, 0.24, 1.25),
                    materials["limestone"], bevel=0.08)
            kit.beam_between("grave_cross", (x - 0.30, front - 1.76, 1.00),
                             (x + 0.30, front - 1.76, 1.00), 0.09, materials["stone"], 0.012)

    kit.add_lantern("lantern_left", (-width * 0.38, front - 0.32, 3.8), iron, materials["ember"])
    kit.add_lantern("lantern_right", (width * 0.38, front - 0.32, 3.8), iron, materials["ember"])


def extra_materials(materials: dict[str, bpy.types.Material]) -> None:
    materials["wood_highlight"] = kit.principled_material(
        "sawn_timber_boards", (0.19, 0.075, 0.020, 1.0), 0.84,
        noise_scale=11.0, second_color=(0.42, 0.20, 0.055, 1.0))
    materials["limestone"] = kit.principled_material(
        "dressed_limestone", (0.29, 0.29, 0.25, 1.0), 0.92,
        noise_scale=7.5, second_color=(0.48, 0.44, 0.34, 1.0))
    materials["brick"] = kit.principled_material(
        "kiln_fired_brick", (0.24, 0.055, 0.022, 1.0), 0.91,
        noise_scale=12.0, second_color=(0.52, 0.17, 0.055, 1.0))
    materials["slate"] = kit.principled_material(
        "blue_black_slate", (0.035, 0.050, 0.058, 1.0), 0.82,
        noise_scale=9.0, second_color=(0.12, 0.17, 0.19, 1.0))
    materials["slate_alt"] = kit.principled_material(
        "weathered_slate", (0.065, 0.085, 0.090, 1.0), 0.88,
        noise_scale=11.0, second_color=(0.16, 0.19, 0.19, 1.0))
    materials["rope"] = kit.principled_material(
        "hemp_rope", (0.31, 0.20, 0.075, 1.0), 0.96,
        noise_scale=15.0, second_color=(0.55, 0.39, 0.16, 1.0))
    materials["coal"] = kit.principled_material(
        "forge_coal", (0.008, 0.010, 0.012, 1.0), 0.62,
        noise_scale=18.0, second_color=(0.05, 0.055, 0.06, 1.0))
    materials["produce_red"] = kit.principled_material(
        "market_red_produce", (0.42, 0.025, 0.012, 1.0), 0.64,
        noise_scale=8.0, second_color=(0.72, 0.09, 0.025, 1.0))
    materials["produce_green"] = kit.principled_material(
        "market_green_produce", (0.06, 0.18, 0.035, 1.0), 0.72,
        noise_scale=8.0, second_color=(0.19, 0.38, 0.08, 1.0))
    materials["cloth"] = kit.principled_material("woven_cloth", (0.25, 0.035, 0.018, 1.0), 0.88,
                                                  noise_scale=8.0, second_color=(0.58, 0.16, 0.04, 1.0))
    materials["hay"] = kit.principled_material("dry_hay", (0.34, 0.18, 0.035, 1.0), 0.94,
                                                noise_scale=10.0, second_color=(0.70, 0.44, 0.10, 1.0))


def main() -> int:
    args = arguments()
    root = Path(args.project_root).resolve()
    output = (root / args.output_root).resolve()
    catalog_path = (root / args.catalog).resolve()
    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    families = {item["id"]: item for item in catalog["families"]}
    variants = {item["id"]: item for item in catalog["variants"]}
    family = families[args.family]
    variant = variants[args.variant]
    width, depth, wall, rise = family["dimensions"]
    seed = family["seed"] + variant["seed_offset"]
    rng = random.Random(seed)
    asset_id = family["id"] + "_" + variant["id"]

    bpy.ops.wm.read_factory_settings(use_empty=True)
    materials = kit.build_materials(variant)
    extra_materials(materials)
    kit.set_phase("foundation")
    foundation(width, depth, materials["stone"], rng)
    kit.set_phase("frame")
    frame(width, depth, wall, family["function"], family["wall_system"],
          materials, variant["brace_style"], rng)
    kit.set_phase("roof")
    primary_roof = materials["slate"] if family["roof_system"] == "slate" else materials["roof"]
    accent_roof = materials["slate_alt"] if family["roof_system"] == "slate" else materials["roof_alt"]
    roof(width, depth, wall, rise, primary_roof, accent_roof, materials["iron"], rng)
    kit.set_phase("details")
    add_function_details(family["function"], width, depth, wall, rise, variant, materials,
                         root / family["input"]["path"], rng)
    kit.setup_review(materials)

    previews = output / "Workbench" / "Previews"
    previews.mkdir(parents=True, exist_ok=True)
    ortho = max(width, depth) * 1.55 + 4.0
    target_z = wall * 0.55
    if family["function"] == "chapel":
        ortho = max(width, depth) * 1.8 + 6.0
        target_z = wall + rise * 0.55
    if variant["id"] == "a" and not args.skip_previews:
        for stage_index, phase in enumerate(kit.PHASES):
            kit.show_construction_through(stage_index)
            kit.render_view(previews / f"{asset_id}_stage_{stage_index + 1:02d}_{phase}.png",
                            (16.5, -19.5, 14.0), (0.0, 0.0, target_z), ortho, 448)
    kit.show_construction_through(3)
    if not args.skip_previews:
        kit.render_view(previews / f"{asset_id}_hero.png", (16.5, -19.5, 14.0),
                        (0.0, 0.0, target_z), ortho, 640)
        kit.render_view(previews / f"{asset_id}_rts.png", (18.0, -21.0, 23.0),
                        (0.0, 0.0, target_z * 0.92), ortho * 1.12, 512)

    blend = output / "Workbench" / f"{asset_id}_review.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    glb, fbx, phase_triangles, triangles, mesh_hash = kit.export_assets(
        asset_id, output / "Raw", output / "Workbench" / "Models")
    metrics = {
        "schema": 1,
        "id": asset_id,
        "family": family["id"],
        "function": family["function"],
        "wall_system": family["wall_system"],
        "roof_system": family["roof_system"],
        "identity_markers": family["identity_markers"],
        "variant": variant["id"],
        "seed": seed,
        "blender": bpy.app.version_string,
        "vendor_input": family["input"],
        "triangles": {"lod0": triangles[0], "lod1": triangles[1], "lod2": triangles[2]},
        "construction_phase_triangles": {
            phase: {"lod0": counts[0], "lod1": counts[1], "lod2": counts[2]}
            for phase, counts in phase_triangles.items()
        },
        "canonical_mesh_sha256": mesh_hash,
        "outputs": {
            "glb": glb.relative_to(root).as_posix(),
            "fbx": fbx.relative_to(root).as_posix(),
            "hero": (previews / f"{asset_id}_hero.png").relative_to(root).as_posix(),
            "rts": (previews / f"{asset_id}_rts.png").relative_to(root).as_posix()
        }
    }
    report = output / "Reports" / f"{asset_id}_metrics.json"
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"CITYLAB_BUILDING_GENERATED id={asset_id} triangles={triangles} hash={mesh_hash}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
