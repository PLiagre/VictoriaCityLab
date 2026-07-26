"""Lance une commande Unity CityLab avec un verrou propre au projet.

Ce verrou vit dans le Library de CityLab et ne consulte jamais
VictoriaProject/runtime_bridge/locks/unity.lock.

Usage:
    py Tools/run_unity_locked.py -- <Unity.exe> -batchmode -projectPath <CityLab> ...
"""

from __future__ import annotations

import json
import os
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
LOCK = ROOT / "Library" / "CityLabAutomation.lock"


def pid_alive(pid: int) -> bool:
    if pid <= 0:
        return False
    try:
        os.kill(pid, 0)
    except OSError:
        return False
    return True


def acquire() -> None:
    LOCK.parent.mkdir(parents=True, exist_ok=True)
    if LOCK.exists():
        try:
            holder = json.loads(LOCK.read_text(encoding="utf-8"))
        except (OSError, ValueError):
            holder = {}
        if pid_alive(int(holder.get("pid", -1))):
            raise SystemExit(
                f"CityLab est deja utilise par l'automatisation pid={holder.get('pid')}"
            )
        LOCK.unlink(missing_ok=True)
    descriptor = os.open(LOCK, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
    with os.fdopen(descriptor, "w", encoding="utf-8") as stream:
        json.dump({"pid": os.getpid(), "project": str(ROOT)}, stream)


def main() -> int:
    try:
        separator = sys.argv.index("--")
    except ValueError as exc:
        raise SystemExit("Commande Unity attendue apres --") from exc
    command = sys.argv[separator + 1 :]
    if not command:
        raise SystemExit("Commande Unity vide")
    acquire()
    try:
        return subprocess.call(command, cwd=ROOT)
    finally:
        try:
            holder = json.loads(LOCK.read_text(encoding="utf-8"))
            if int(holder.get("pid", -1)) == os.getpid():
                LOCK.unlink()
        except (OSError, ValueError):
            pass


if __name__ == "__main__":
    raise SystemExit(main())

