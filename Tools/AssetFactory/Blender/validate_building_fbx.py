"""Valide le contrat modulaire 4 phases x 3 LOD d'un FBX CityLab."""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

import bpy


PHASES = {1: "FOUNDATION", 2: "FRAME", 3: "ROOF", 4: "DETAILS"}
NAME_PATTERN = re.compile(r"__P(0[1-4])_([A-Z]+)_LOD([0-2])$")


def arguments() -> argparse.Namespace:
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--fbx", required=True)
    return parser.parse_args(argv)


def triangle_count(obj: bpy.types.Object) -> int:
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def main() -> int:
    path = Path(arguments().fbx).resolve()
    if not path.is_file():
        raise FileNotFoundError(path)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(path))
    found: dict[tuple[int, int], bpy.types.Object] = {}
    errors: list[str] = []
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        match = NAME_PATTERN.search(obj.name.upper())
        if match is None:
            errors.append("mesh_name_invalid:" + obj.name)
            continue
        phase = int(match.group(1))
        label = match.group(2)
        lod = int(match.group(3))
        if label != PHASES[phase]:
            errors.append("phase_label_invalid:" + obj.name)
        key = (phase, lod)
        if key in found:
            errors.append("phase_lod_duplicate:" + obj.name)
        found[key] = obj
    expected = {(phase, lod) for phase in PHASES for lod in range(3)}
    for missing in sorted(expected - set(found)):
        errors.append(f"phase_lod_missing:P{missing[0]:02d}_LOD{missing[1]}")
    if errors:
        print("CITYLAB_BUILDING_FBX_ERROR " + " ".join(errors))
        return 1
    counts = {
        f"p{phase:02d}_lod{lod}": triangle_count(found[(phase, lod)])
        for phase, lod in sorted(expected)
    }
    summary = " ".join(f"{key}={value}" for key, value in counts.items())
    print(f"CITYLAB_BUILDING_FBX_OK file={path.name} meshes=12 {summary}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
