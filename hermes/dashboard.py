"""Dashboard Markdown deterministe de la boucle full-auto CityLab."""

from __future__ import annotations

import json
import subprocess
from pathlib import Path

from harness.audit_ledger import read_events


ROOT = Path(__file__).resolve().parents[1]


def gh_json(args: list[str]) -> object:
    result = subprocess.run(["gh", *args], cwd=ROOT, text=True, encoding="utf-8", errors="replace", stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False)
    return json.loads(result.stdout) if result.returncode == 0 and result.stdout.strip() else []


def render() -> str:
    events = read_events(ROOT / "Architecture" / "audit-ledger.jsonl")
    prs = gh_json(["pr", "list", "--repo", "PLiagre/VictoriaCityLab", "--state", "all", "--limit", "10", "--json", "number,title,state,mergedAt,url"])
    runs = gh_json(["run", "list", "--repo", "PLiagre/VictoriaCityLab", "--limit", "10", "--json", "name,status,conclusion,url"])
    lines = ["# Tableau de bord Hermes — Victoria CityLab", "", "## Pipeline", "", f"- Evenements d'audit: {len(events)}", f"- Pull requests recentes: {len(prs)}", f"- Workflows recents: {len(runs)}", "", "## Pull requests", ""]
    for pr in prs:
        lines.append(f"- [#{pr['number']} — {pr['title']}]({pr['url']}) — {pr['state']}")
    lines += ["", "## Workflows", ""]
    for run in runs:
        lines.append(f"- [{run['name']}]({run['url']}) — {run['status']} / {run.get('conclusion') or '-'}")
    lines += ["", "## Derniers evenements d'audit", ""]
    for event in events[-10:]:
        lines.append(f"- `{event['event']}` — `{event['audit_id']}` — {event['actor']}")
    return "\n".join(lines) + "\n"


def main() -> int:
    output = ROOT / "hermes" / "DASHBOARD.md"
    output.write_text(render(), encoding="utf-8")
    print(f"CITYLAB_HERMES_DASHBOARD_OK path={output.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

