"""Génère la scierie dark-fantasy CityLab depuis une recette déterministe."""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import random
import struct
import sys
from pathlib import Path

import bpy
from mathutils import Vector


ASSET_TAG = "citylab_asset"
PHASE_TAG = "citylab_construction_phase"
PHASES = ("foundation", "frame", "roof", "details")
CURRENT_PHASE = "details"


def arguments() -> argparse.Namespace:
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", required=True)
    parser.add_argument("--recipe", required=True)
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--variant", default="a")
    return parser.parse_args(argv)


def principled_material(
    name: str,
    color: tuple[float, float, float, float],
    roughness: float,
    metallic: float = 0.0,
    noise_scale: float = 0.0,
    second_color: tuple[float, float, float, float] | None = None,
    emission: tuple[float, float, float, float] | None = None,
    emission_strength: float = 0.0,
) -> bpy.types.Material:
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    shader = nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = color
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Metallic"].default_value = metallic
    if emission is not None:
        shader.inputs["Emission Color"].default_value = emission
        shader.inputs["Emission Strength"].default_value = emission_strength
    if noise_scale > 0.0 and second_color is not None:
        texture = nodes.new("ShaderNodeTexNoise")
        texture.inputs["Scale"].default_value = noise_scale
        texture.inputs["Detail"].default_value = 3.0
        texture.inputs["Roughness"].default_value = 0.72
        ramp = nodes.new("ShaderNodeValToRGB")
        ramp.color_ramp.elements[0].color = color
        ramp.color_ramp.elements[1].color = second_color
        bump = nodes.new("ShaderNodeBump")
        bump.inputs["Strength"].default_value = 0.18
        bump.inputs["Distance"].default_value = 0.08
        links.new(texture.outputs["Fac"], ramp.inputs["Fac"])
        links.new(ramp.outputs["Color"], shader.inputs["Base Color"])
        links.new(texture.outputs["Fac"], bump.inputs["Height"])
        links.new(bump.outputs["Normal"], shader.inputs["Normal"])
    return material


def tag(obj: bpy.types.Object) -> bpy.types.Object:
    obj[ASSET_TAG] = True
    obj[PHASE_TAG] = CURRENT_PHASE
    return obj


def set_phase(phase: str) -> None:
    global CURRENT_PHASE
    if phase not in PHASES:
        raise ValueError(f"Unknown construction phase: {phase}")
    CURRENT_PHASE = phase


def apply_bevel(obj: bpy.types.Object, width: float, segments: int = 2) -> None:
    if width <= 0.0:
        return
    modifier = obj.modifiers.new("edge_softening", "BEVEL")
    modifier.width = width
    modifier.segments = segments
    modifier.limit_method = "ANGLE"
    modifier.angle_limit = 0.35
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=modifier.name)


def box(
    name: str,
    location: tuple[float, float, float],
    dimensions: tuple[float, float, float],
    material: bpy.types.Material,
    rotation: tuple[float, float, float] = (0.0, 0.0, 0.0),
    bevel: float = 0.045,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(location=location, rotation=rotation)
    obj = tag(bpy.context.object)
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    apply_bevel(obj, bevel)
    return obj


def cylinder(
    name: str,
    location: tuple[float, float, float],
    radius: float,
    depth: float,
    material: bpy.types.Material,
    rotation: tuple[float, float, float] = (0.0, 0.0, 0.0),
    vertices: int = 16,
    bevel: float = 0.025,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=location, rotation=rotation
    )
    obj = tag(bpy.context.object)
    obj.name = name
    obj.data.materials.append(material)
    apply_bevel(obj, bevel)
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    return obj


def beam_between(
    name: str,
    start: tuple[float, float, float],
    end: tuple[float, float, float],
    thickness: float,
    material: bpy.types.Material,
    bevel: float = 0.035,
) -> bpy.types.Object:
    a, b = Vector(start), Vector(end)
    delta = b - a
    obj = box(name, tuple((a + b) * 0.5), (thickness, thickness, delta.length), material, bevel=bevel)
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = delta.to_track_quat("Z", "Y")
    return obj


def triangular_prism(
    name: str,
    y: float,
    width: float,
    bottom: float,
    peak: float,
    depth: float,
    material: bpy.types.Material,
) -> bpy.types.Object:
    half = width * 0.5
    y0, y1 = y - depth * 0.5, y + depth * 0.5
    vertices = [
        (-half, y0, bottom), (half, y0, bottom), (0.0, y0, peak),
        (-half, y1, bottom), (half, y1, bottom), (0.0, y1, peak),
    ]
    faces = [(0, 2, 1), (3, 4, 5), (0, 1, 4, 3), (1, 2, 5, 4), (2, 0, 3, 5)]
    mesh = bpy.data.meshes.new(name + "_mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = tag(bpy.data.objects.new(name, mesh))
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    apply_bevel(obj, 0.035)
    return obj


def import_vendor_component(
    path: Path,
    name: str,
    location: tuple[float, float, float],
    target_width: float,
    material: bpy.types.Material,
    rotation_z: float = 0.0,
) -> list[bpy.types.Object]:
    before = set(bpy.context.scene.objects)
    bpy.ops.import_scene.fbx(filepath=str(path))
    imported = [obj for obj in bpy.context.scene.objects if obj not in before]
    meshes = [obj for obj in imported if obj.type == "MESH"]
    lod0 = [obj for obj in meshes if "LOD0" in obj.name.upper()] or meshes
    for obj in imported:
        if obj.type != "MESH" or obj not in lod0:
            bpy.data.objects.remove(obj, do_unlink=True)
    corners = [obj.matrix_world @ Vector(corner) for obj in lod0 for corner in obj.bound_box]
    minimum = Vector((min(v.x for v in corners), min(v.y for v in corners), min(v.z for v in corners)))
    maximum = Vector((max(v.x for v in corners), max(v.y for v in corners), max(v.z for v in corners)))
    center = (minimum + maximum) * 0.5
    scale = target_width / max(0.001, maximum.x - minimum.x)
    result: list[bpy.types.Object] = []
    for index, obj in enumerate(lod0):
        obj.name = f"{name}_{index:02d}"
        obj.data = obj.data.copy()
        obj.data.materials.clear()
        obj.data.materials.append(material)
        obj.scale *= scale
        obj.location = (obj.location - Vector((center.x, center.y, minimum.z))) * scale
        obj.rotation_euler.z += rotation_z
        obj.location += Vector(location)
        tag(obj)
        result.append(obj)
    return result


def add_stone_foundation(stone: bpy.types.Material, rng: random.Random) -> None:
    box("foundation_core", (0.0, 0.15, 0.28), (9.6, 7.1, 0.55), stone, bevel=0.12)
    for side in (-1, 1):
        for index in range(9):
            x = -4.25 + index * 1.06
            box(
                f"stone_front_{side}_{index}",
                (x, side * 3.58, 0.42 + rng.uniform(-0.04, 0.04)),
                (0.92 + rng.uniform(-0.08, 0.08), 0.42, 0.5 + rng.uniform(-0.05, 0.07)),
                stone,
                rotation=(0.0, 0.0, rng.uniform(-0.035, 0.035)),
                bevel=0.11,
            )
    for side in (-1, 1):
        for index in range(6):
            y = -2.75 + index * 1.1
            box(
                f"stone_side_{side}_{index}",
                (side * 4.82, y, 0.42),
                (0.42, 0.94, 0.5 + rng.uniform(-0.04, 0.06)),
                stone,
                bevel=0.1,
            )


def add_frame(wood: bpy.types.Material, plaster: bpy.types.Material, brace_style: str) -> None:
    for x in (-4.15, 0.0, 4.15):
        for y in (-3.15, 3.1):
            box(f"post_{x}_{y}", (x, y, 3.25), (0.38, 0.38, 5.75), wood, bevel=0.07)
            box(f"post_foot_{x}_{y}", (x, y, 0.8), (0.62, 0.62, 0.48), wood, bevel=0.08)
    for y in (-3.15, 3.1):
        beam_between(f"top_beam_{y}", (-4.45, y, 5.75), (4.45, y, 5.75), 0.4, wood, 0.06)
    for x in (-4.15, 4.15):
        beam_between(f"side_top_{x}", (x, -3.4, 5.7), (x, 3.4, 5.7), 0.38, wood, 0.055)
    box("rear_wall", (0.0, 3.28, 3.1), (8.0, 0.22, 4.7), plaster, bevel=0.05)
    box("left_half_wall", (-4.0, 0.9, 2.0), (0.22, 4.2, 2.9), plaster, bevel=0.05)
    box("right_half_wall", (4.0, 1.75, 2.0), (0.22, 2.5, 2.9), plaster, bevel=0.05)
    for x in (-3.9, -1.95, 0.0, 1.95, 3.9):
        box(f"rear_stud_{x}", (x, 3.12, 3.2), (0.22, 0.24, 4.8), wood, bevel=0.035)
    for a, b in [((-3.9, 3.0, 1.0), (-1.95, 3.0, 4.9)), ((3.9, 3.0, 1.0), (1.95, 3.0, 4.9))]:
        beam_between("rear_brace", a, b, 0.22, wood, 0.03)
    if brace_style == "crossed":
        beam_between("rear_brace_cross_left", (-1.85, 3.0, 1.0), (0.0, 3.0, 4.9), 0.19, wood, 0.028)
        beam_between("rear_brace_cross_right", (1.85, 3.0, 1.0), (0.0, 3.0, 4.9), 0.19, wood, 0.028)
    elif brace_style == "dense":
        for x in (-2.9, -0.95, 0.95, 2.9):
            box(f"rear_extra_stud_{x}", (x, 3.0, 3.0), (0.15, 0.20, 3.85), wood, bevel=0.025)
    triangular_prism("front_gable", -3.16, 8.3, 5.55, 9.0, 0.18, plaster)
    triangular_prism("rear_gable", 3.18, 8.3, 5.55, 9.0, 0.18, plaster)
    for y in (-3.3, 3.3):
        beam_between(f"gable_left_{y}", (-4.1, y, 5.65), (0.0, y, 9.0), 0.25, wood, 0.04)
        beam_between(f"gable_right_{y}", (4.1, y, 5.65), (0.0, y, 9.0), 0.25, wood, 0.04)
        beam_between(f"gable_center_{y}", (0.0, y, 5.6), (0.0, y, 8.9), 0.24, wood, 0.04)
        beam_between(f"gable_cross_{y}", (-2.7, y, 6.8), (2.7, y, 6.8), 0.22, wood, 0.035)


def add_roof(
    roof: bpy.types.Material,
    roof_alt: bpy.types.Material,
    iron: bpy.types.Material,
    rng: random.Random,
) -> None:
    half_span, rise, depth = 4.85, 3.45, 7.55
    angle = math.atan2(rise, half_span)
    slope = math.sqrt(half_span * half_span + rise * rise)
    for side in (-1, 1):
        x = side * half_span * 0.5
        box(
            f"roof_slope_{side}",
            (x, 0.0, 5.62 + rise * 0.5),
            (slope, depth, 0.24),
            roof,
            rotation=(0.0, side * angle, 0.0),
            bevel=0.045,
        )
    for side in (-1, 1):
        for slope_row in range(9):
            distance = 0.38 + slope_row * 0.53
            x = side * distance
            z = 9.0 - distance * rise / half_span + 0.17
            stagger = 0.25 if slope_row % 2 else 0.0
            for depth_column in range(9):
                y = -3.25 + depth_column * 0.82 + stagger + rng.uniform(-0.035, 0.035)
                if y > 3.35:
                    y -= 7.1
                tile_material = roof_alt if rng.random() < 0.24 else roof
                box(
                    f"roof_shingle_{side}_{slope_row}_{depth_column}",
                    (x + side * rng.uniform(-0.018, 0.018), y, z + rng.uniform(-0.014, 0.014)),
                    (0.62 + rng.uniform(-0.025, 0.025), 0.92 + rng.uniform(-0.035, 0.025), 0.09),
                    tile_material,
                    rotation=(0.0, side * angle, rng.uniform(-0.008, 0.008)),
                    bevel=0.022,
                )
    cylinder("ridge_cap", (0.0, 0.0, 9.08), 0.13, 7.8, iron, rotation=(math.pi / 2, 0.0, 0.0), vertices=12)
    for y in (-3.45, 3.45):
        cylinder(f"gable_finial_{y}", (0.0, y, 9.55), 0.11, 1.05, iron, vertices=10)
        cylinder(f"finial_spike_{y}", (0.0, y, 10.18), 0.22, 0.48, iron, vertices=4)


def add_saw_mechanism(
    wood: bpy.types.Material,
    pale_wood: bpy.types.Material,
    iron: bpy.types.Material,
    blade: bpy.types.Material,
    bronze: bpy.types.Material,
) -> None:
    for x in (-0.72, 0.72):
        box(f"carriage_rail_{x}", (x, -0.15, 1.2), (0.24, 5.6, 0.28), iron, bevel=0.035)
    for y in (-2.2, -0.8, 0.8, 2.2):
        box(f"carriage_cross_{y}", (0.0, y, 1.32), (1.85, 0.28, 0.22), wood, bevel=0.04)
    cylinder("carriage_log", (0.0, -0.15, 1.82), 0.43, 5.35, pale_wood, rotation=(math.pi / 2, 0.0, 0.0), vertices=20, bevel=0.035)
    for x in (-1.32, 1.32):
        box(f"saw_frame_post_{x}", (x, 0.45, 3.2), (0.34, 0.42, 4.0), wood, bevel=0.06)
    box("saw_frame_top", (0.0, 0.45, 5.16), (3.0, 0.45, 0.38), wood, bevel=0.06)
    box("saw_frame_bottom", (0.0, 0.45, 1.46), (2.8, 0.42, 0.28), wood, bevel=0.05)
    box("saw_blade", (0.0, 0.43, 3.25), (0.085, 0.12, 3.35), blade, bevel=0.015)
    for index in range(13):
        z = 1.72 + index * 0.245
        box(f"saw_tooth_{index}", (0.12, 0.43, z), (0.22, 0.13, 0.13), blade, rotation=(0.0, math.pi / 4, 0.0), bevel=0.008)
    bpy.ops.mesh.primitive_torus_add(
        major_radius=1.42,
        minor_radius=0.12,
        major_segments=32,
        minor_segments=8,
        location=(3.28, 0.5, 2.95),
        rotation=(math.pi / 2, 0.0, 0.0),
    )
    wheel = tag(bpy.context.object)
    wheel.name = "drive_wheel_rim"
    wheel.data.materials.append(bronze)
    cylinder("drive_hub", (3.28, 0.5, 2.95), 0.28, 0.5, bronze, rotation=(math.pi / 2, 0.0, 0.0), vertices=16)
    for index in range(12):
        angle = index * math.tau / 12.0
        start = (3.28, 0.48, 2.95)
        end = (3.28 + math.cos(angle) * 1.32, 0.48, 2.95 + math.sin(angle) * 1.32)
        beam_between(f"wheel_spoke_{index}", start, end, 0.10, bronze, 0.018)
    beam_between("wheel_crank", (3.28, 0.25, 2.95), (1.30, 0.43, 4.75), 0.13, iron, 0.022)
    cylinder("pulley_top", (1.25, 0.43, 4.82), 0.24, 0.42, bronze, rotation=(math.pi / 2, 0.0, 0.0), vertices=16)


def add_logs(
    bark: bpy.types.Material,
    pale_wood: bpy.types.Material,
    vendor_path: Path,
    rng: random.Random,
    stack_side: int,
) -> None:
    for layer, count in enumerate((4, 3, 2)):
        for index in range(count):
            x = stack_side * (4.95 + index * 0.72 + (0.34 if layer else 0.0))
            z = 0.88 + layer * 0.58
            cylinder(
                f"log_stack_{layer}_{index}",
                (x, 1.75, z),
                0.29 + rng.uniform(-0.025, 0.03),
                4.6 + rng.uniform(-0.18, 0.18),
                bark,
                rotation=(math.pi / 2, 0.0, 0.0),
                vertices=14,
                bevel=0.025,
            )
            cylinder(
                f"log_end_{layer}_{index}",
                (x, -0.56, z),
                0.235,
                0.025,
                pale_wood,
                rotation=(math.pi / 2, 0.0, 0.0),
                vertices=14,
                bevel=0.0,
            )
    other_side = -stack_side
    import_vendor_component(vendor_path, "vendor_cut_wood", (other_side * 4.9, -2.35, 0.35), 2.7, pale_wood, -0.22 * other_side)
    import_vendor_component(vendor_path, "vendor_cut_wood_secondary", (other_side * 3.8, -2.65, 0.28), 2.2, pale_wood, 0.48 * other_side)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=1.0, location=(stack_side * 1.85, -2.12, 0.68))
    sawdust = tag(bpy.context.object)
    sawdust.name = "sawdust_mound"
    sawdust.scale = (1.15, 0.72, 0.25)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    for vertex in sawdust.data.vertices:
        vertex.co.x *= 1.0 + rng.uniform(-0.14, 0.14)
        vertex.co.y *= 1.0 + rng.uniform(-0.16, 0.16)
    sawdust.data.materials.append(pale_wood)


def add_annex(
    vendor_path: Path,
    wood: bpy.types.Material,
    roof: bpy.types.Material,
    side: int,
) -> None:
    objects = import_vendor_component(vendor_path, "vendor_annex", (side * 5.15, 1.0, 0.5), 5.1, wood, side * 0.05)
    for obj in objects:
        if "roof" in obj.name.lower():
            obj.data.materials.clear()
            obj.data.materials.append(roof)


def add_chimney(stone: bpy.types.Material, iron: bpy.types.Material, side: int) -> None:
    x = side * 2.45
    box("chimney_stack", (x, 1.35, 7.45), (1.0, 1.0, 4.25), stone, bevel=0.11)
    box("chimney_collar", (x, 1.35, 9.38), (1.28, 1.28, 0.28), stone, bevel=0.10)
    box("chimney_cap", (x, 1.35, 9.72), (1.38, 1.38, 0.20), iron, bevel=0.07)
    for corner_x in (-0.47, 0.47):
        for corner_y in (-0.47, 0.47):
            cylinder(
                "chimney_cap_support",
                (x + corner_x, 1.35 + corner_y, 9.53),
                0.045,
                0.32,
                iron,
                vertices=8,
                bevel=0.008,
            )


def add_lantern(
    name: str,
    location: tuple[float, float, float],
    iron: bpy.types.Material,
    ember: bpy.types.Material,
) -> None:
    x, y, z = location
    box(name + "_cap", (x, y, z + 0.34), (0.42, 0.38, 0.12), iron, bevel=0.03)
    box(name + "_base", (x, y, z - 0.34), (0.42, 0.38, 0.12), iron, bevel=0.03)
    for sx in (-0.18, 0.18):
        for sy in (-0.15, 0.15):
            beam_between(name + "_bar", (x + sx, y + sy, z - 0.3), (x + sx, y + sy, z + 0.3), 0.045, iron, 0.008)
    cylinder(name + "_glow", (x, y, z), 0.16, 0.5, ember, vertices=12, bevel=0.02)
    bpy.ops.object.light_add(type="POINT", location=location)
    light = tag(bpy.context.object)
    light.name = name + "_light"
    light.data.energy = 125.0
    light.data.color = (1.0, 0.22, 0.035)
    light.data.shadow_soft_size = 1.2


def add_sign(
    wood: bpy.types.Material,
    bronze: bpy.types.Material,
    style: str,
) -> None:
    if style == "round":
        cylinder("sign_board_round", (0.0, -3.65, 7.25), 0.76, 0.16, wood,
                 rotation=(math.pi / 2, 0.0, 0.0), vertices=16, bevel=0.07)
    else:
        box("sign_board", (0.0, -3.65, 7.25), (2.05, 0.16, 0.92), wood, bevel=0.11)
    if style == "saw":
        beam_between("sign_saw", (-0.62, -3.78, 7.05), (0.62, -3.78, 7.45), 0.12, bronze, 0.018)
        for index in range(6):
            x = -0.48 + index * 0.19
            box("sign_saw_tooth", (x, -3.79, 7.12 + (x + 0.48) * 0.32),
                (0.10, 0.08, 0.10), bronze, rotation=(0.0, math.pi / 4, 0.0), bevel=0.01)
    else:
        beam_between("sign_axe_1", (-0.55, -3.78, 6.95), (0.55, -3.78, 7.52), 0.10, bronze, 0.018)
        beam_between("sign_axe_2", (0.55, -3.80, 6.95), (-0.55, -3.80, 7.52), 0.10, bronze, 0.018)


def setup_review(materials: dict[str, bpy.types.Material]) -> None:
    ground = box("review_ground", (0.0, 0.0, -0.16), (24.0, 21.0, 0.25), materials["ground"], bevel=0.18)
    ground[ASSET_TAG] = False
    del ground[PHASE_TAG]
    world = bpy.data.worlds.new("CityLab Dark World")
    bpy.context.scene.world = world
    world.use_nodes = True
    background = world.node_tree.nodes["Background"]
    background.inputs["Color"].default_value = (0.006, 0.012, 0.021, 1.0)
    background.inputs["Strength"].default_value = 0.22
    lights = [
        ("key", "AREA", (8.0, -11.0, 16.0), 2100.0, (1.0, 0.62, 0.34), 8.0),
        ("fill", "AREA", (-11.0, -2.0, 10.0), 1650.0, (0.24, 0.42, 0.74), 9.0),
        ("rim", "AREA", (2.0, 10.0, 13.0), 1550.0, (0.38, 0.52, 0.82), 7.0),
    ]
    for name, light_type, location, energy, color, size in lights:
        bpy.ops.object.light_add(type=light_type, location=location)
        light = bpy.context.object
        light.name = name
        light.data.energy = energy
        light.data.color = color
        light.data.shape = "DISK"
        light.data.size = size
        light.rotation_euler = (Vector((0.0, 0.0, 3.5)) - light.location).to_track_quat("-Z", "Y").to_euler()


def render_view(output: Path, location: tuple[float, float, float], target: tuple[float, float, float], ortho: float, resolution: int) -> None:
    scene = bpy.context.scene
    if scene.camera is None:
        bpy.ops.object.camera_add()
        scene.camera = bpy.context.object
        scene.camera.name = "review_camera"
    camera = scene.camera
    camera.location = location
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = ortho
    camera.rotation_euler = (Vector(target) - camera.location).to_track_quat("-Z", "Y").to_euler()
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = resolution
    scene.render.resolution_y = resolution
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.filepath = str(output)
    scene.render.film_transparent = False
    scene.view_settings.look = "AgX - Medium High Contrast"
    bpy.ops.render.render(write_still=True)


def asset_objects() -> list[bpy.types.Object]:
    return [obj for obj in bpy.context.scene.objects if obj.type == "MESH" and obj.get(ASSET_TAG, False)]


def phase_objects(phase: str) -> list[bpy.types.Object]:
    return [obj for obj in asset_objects() if obj.get(PHASE_TAG) == phase]


def show_construction_through(stage_index: int) -> None:
    for obj in bpy.context.scene.objects:
        phase = obj.get(PHASE_TAG)
        if phase not in PHASES:
            continue
        visible = PHASES.index(phase) <= stage_index
        obj.hide_render = not visible
        obj.hide_viewport = not visible


def joined_copy(name: str, sources: list[bpy.types.Object] | None = None) -> bpy.types.Object:
    copies: list[bpy.types.Object] = []
    for source in sources or asset_objects():
        copy = source.copy()
        copy.data = source.data.copy()
        bpy.context.collection.objects.link(copy)
        copy.matrix_world = source.matrix_world.copy()
        copy[ASSET_TAG] = False
        copies.append(copy)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in copies:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = copies[0]
    bpy.ops.object.join()
    joined = bpy.context.object
    joined.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for layer in joined.data.uv_layers:
        for loop in layer.data:
            # Les importeurs FBX peuvent dériver d'environ 1e-5 entre deux
            # processus Blender. Une grille UV 1/512 rend le master binaire
            # reproductible ; ces matériaux utilisent une palette/trim partagé,
            # donc ce snap sous-pixel ne dégrade pas la lecture du bâtiment.
            loop.uv = (
                round(loop.uv.x * 512.0) / 512.0,
                round(loop.uv.y * 512.0) / 512.0,
            )
    joined.data.calc_loop_triangles()
    return joined


def decimated_copy(source: bpy.types.Object, name: str, ratio: float) -> bpy.types.Object:
    obj = source.copy()
    obj.data = source.data.copy()
    bpy.context.collection.objects.link(obj)
    obj.name = name
    modifier = obj.modifiers.new("lod_decimate", "DECIMATE")
    modifier.ratio = ratio
    modifier.use_collapse_triangulate = True
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.data.calc_loop_triangles()
    return obj


def triangle_count(obj: bpy.types.Object) -> int:
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def canonical_hash(obj: bpy.types.Object) -> str:
    digest = hashlib.sha256()
    obj.data.calc_loop_triangles()
    for vertex in obj.data.vertices:
        digest.update(struct.pack("<3f", *vertex.co))
    for triangle in obj.data.loop_triangles:
        digest.update(struct.pack("<3I", *triangle.vertices))
        digest.update(struct.pack("<I", triangle.material_index))
    return digest.hexdigest()


def export_assets(
    asset_id: str,
    raw_dir: Path,
    model_dir: Path,
) -> tuple[Path, Path, dict[str, list[int]], list[int], str]:
    master = joined_copy(asset_id + "_MASTER_LOD0")
    mesh_hash = canonical_hash(master)
    phase_lods: dict[str, list[bpy.types.Object]] = {}
    phase_triangles: dict[str, list[int]] = {}
    export_lods: list[bpy.types.Object] = []
    for phase_index, phase in enumerate(PHASES, start=1):
        source = phase_objects(phase)
        if not source:
            raise RuntimeError(f"Construction phase has no geometry: {phase}")
        prefix = f"{asset_id}__P{phase_index:02d}_{phase.upper()}"
        lod0 = joined_copy(prefix + "_LOD0", source)
        lod1 = decimated_copy(lod0, prefix + "_LOD1", 0.50)
        lod2 = decimated_copy(lod0, prefix + "_LOD2", 0.20)
        phase_lods[phase] = [lod0, lod1, lod2]
        phase_triangles[phase] = [triangle_count(obj) for obj in phase_lods[phase]]
        export_lods.extend(phase_lods[phase])
    triangles = [sum(phase_triangles[phase][lod] for phase in PHASES) for lod in range(3)]
    for original in asset_objects():
        original.hide_viewport = True
        original.hide_render = True
    raw_dir.mkdir(parents=True, exist_ok=True)
    model_dir.mkdir(parents=True, exist_ok=True)
    glb = raw_dir / f"{asset_id}.glb"
    fbx = model_dir / f"{asset_id}.fbx"
    bpy.ops.object.select_all(action="DESELECT")
    master.select_set(True)
    bpy.context.view_layer.objects.active = master
    bpy.ops.export_scene.gltf(filepath=str(glb), export_format="GLB", use_selection=True)
    bpy.ops.object.select_all(action="DESELECT")
    master.hide_viewport = True
    master.hide_render = True
    for obj in export_lods:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = export_lods[0]
    bpy.ops.export_scene.fbx(
        filepath=str(fbx),
        use_selection=True,
        axis_forward="-Z",
        axis_up="Y",
        apply_scale_options="FBX_SCALE_ALL",
        bake_space_transform=True,
        add_leaf_bones=False,
        mesh_smooth_type="FACE",
    )
    return glb, fbx, phase_triangles, triangles, mesh_hash


def color_from_hex(value: str) -> tuple[float, float, float, float]:
    cleaned = value.removeprefix("#")
    if len(cleaned) != 6:
        raise ValueError(f"Expected #RRGGBB color, got {value}")
    return tuple(int(cleaned[index:index + 2], 16) / 255.0 for index in (0, 2, 4)) + (1.0,)


def build_materials(variant: dict) -> dict[str, bpy.types.Material]:
    palette = variant["palette"]
    wood = color_from_hex(palette["wood"])
    wood_highlight = color_from_hex(palette["wood_highlight"])
    roof = color_from_hex(palette["roof"])
    roof_alt = color_from_hex(palette["roof_accent"])
    return {
        "wood": principled_material("timber_" + variant["id"], wood, 0.78, noise_scale=5.0, second_color=wood_highlight),
        "pale_wood": principled_material("fresh_cut_wood", (0.36, 0.14, 0.035, 1.0), 0.68, noise_scale=7.0, second_color=(0.72, 0.36, 0.10, 1.0)),
        "bark": principled_material("dark_bark", (0.045, 0.016, 0.007, 1.0), 0.92, noise_scale=9.0, second_color=(0.13, 0.038, 0.012, 1.0)),
        "stone": principled_material("cold_stone", (0.075, 0.085, 0.085, 1.0), 0.9, noise_scale=4.0, second_color=(0.19, 0.21, 0.20, 1.0)),
        "plaster": principled_material("soot_plaster", (0.15, 0.105, 0.062, 1.0), 0.88, noise_scale=3.2, second_color=(0.29, 0.19, 0.09, 1.0)),
        "roof": principled_material("roof_" + variant["id"], roof, 0.88, noise_scale=6.0, second_color=roof_alt),
        "roof_alt": principled_material("roof_accent_" + variant["id"], roof_alt, 0.91, noise_scale=7.0, second_color=roof),
        "iron": principled_material("black_iron", (0.018, 0.021, 0.024, 1.0), 0.38, metallic=0.82),
        "blade": principled_material("saw_steel", (0.19, 0.22, 0.22, 1.0), 0.23, metallic=0.92),
        "bronze": principled_material("aged_bronze", (0.30, 0.105, 0.025, 1.0), 0.31, metallic=0.76),
        "ember": principled_material("ember_glass", (0.22, 0.012, 0.001, 1.0), 0.25, emission=(1.0, 0.025, 0.001, 1.0), emission_strength=3.0),
        "ground": principled_material("review_ground", (0.012, 0.020, 0.019, 1.0), 0.94, noise_scale=2.5, second_color=(0.028, 0.045, 0.036, 1.0)),
    }


def main() -> int:
    args = arguments()
    root = Path(args.project_root).resolve()
    recipe_path = (root / args.recipe).resolve()
    output_root = (root / args.output_root).resolve()
    recipe = json.loads(recipe_path.read_text(encoding="utf-8"))
    variants = {entry["id"]: entry for entry in recipe["variants"]}
    if args.variant not in variants:
        raise ValueError(f"Unknown variant {args.variant}; expected one of {sorted(variants)}")
    variant = variants[args.variant]
    asset_id = recipe["id"] + "_" + variant["id"]
    input_paths = {entry["role"]: root / entry["path"] for entry in recipe["inputs"]}
    seed = recipe["seed"] + variant["seed_offset"]
    rng = random.Random(seed)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    materials = build_materials(variant)

    set_phase("foundation")
    add_stone_foundation(materials["stone"], rng)

    set_phase("frame")
    add_frame(materials["wood"], materials["plaster"], variant["brace_style"])

    set_phase("roof")
    add_roof(materials["roof"], materials["roof_alt"], materials["iron"], rng)

    set_phase("details")
    add_saw_mechanism(materials["wood"], materials["pale_wood"], materials["iron"], materials["blade"], materials["bronze"])
    add_logs(materials["bark"], materials["pale_wood"], input_paths["vendor_cut_wood"], rng, variant["log_stack_side"])
    add_annex(input_paths["vendor_annex_structure"], materials["wood"], materials["roof"], variant["annex_side"])
    if variant["chimney"]:
        add_chimney(materials["stone"], materials["iron"], variant["chimney_side"])
    add_lantern("lantern_left", (-3.65, -3.42, 4.2), materials["iron"], materials["ember"])
    add_lantern("lantern_right", (3.65, -3.42, 4.2), materials["iron"], materials["ember"])
    add_sign(materials["wood"], materials["bronze"], variant["sign_style"])
    setup_review(materials)

    previews = output_root / "Workbench" / "Previews"
    previews.mkdir(parents=True, exist_ok=True)
    for stage_index, phase in enumerate(PHASES):
        show_construction_through(stage_index)
        render_view(
            previews / f"{asset_id}_stage_{stage_index + 1:02d}_{phase}.png",
            (16.5, -19.5, 13.5),
            (0.0, 0.0, 3.8),
            17.8,
            512,
        )
    show_construction_through(len(PHASES) - 1)
    render_view(previews / f"{asset_id}_hero.png", (16.5, -19.5, 13.5), (0.0, 0.0, 3.8), 17.8, 768)
    render_view(previews / f"{asset_id}_rts.png", (18.0, -21.0, 23.0), (0.0, 0.0, 2.7), 19.5, 512)
    render_view(previews / f"{asset_id}_detail.png", (7.5, -12.0, 7.2), (0.0, 0.0, 3.0), 10.5, 640)

    blend_path = output_root / "Workbench" / f"{asset_id}_review.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    glb, fbx, phase_triangles, triangles, mesh_hash = export_assets(
        asset_id,
        output_root / "Raw",
        output_root / "Workbench" / "Models",
    )
    metrics = {
        "schema": 1,
        "id": asset_id,
        "recipe": recipe_path.relative_to(root).as_posix(),
        "variant": variant["id"],
        "seed": seed,
        "blender": bpy.app.version_string,
        "vendor_inputs": [entry["path"] for entry in recipe["inputs"]],
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
            "rts": (previews / f"{asset_id}_rts.png").relative_to(root).as_posix(),
            "detail": (previews / f"{asset_id}_detail.png").relative_to(root).as_posix(),
            "construction_stages": [
                (previews / f"{asset_id}_stage_{index + 1:02d}_{phase}.png").relative_to(root).as_posix()
                for index, phase in enumerate(PHASES)
            ],
        },
    }
    report = output_root / "Reports" / f"{asset_id}_metrics.json"
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text(json.dumps(metrics, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"CITYLAB_ASSET_GENERATED id={asset_id} triangles={triangles} hash={mesh_hash}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
