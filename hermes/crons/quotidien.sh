#!/usr/bin/env bash
# Cron quotidien Hermes — lecture, mesure, proposition. Jamais de fusion.
# Contrat : hermes/crons/README.md  |  ADR-0002
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

if [[ ! -f Docs/ROADMAP.md ]] || [[ ! -d hermes/crons ]]; then
    echo "refus : ce n'est pas la racine VictoriaCityLab ($ROOT)" >&2
    exit 2
fi

PYTHON="$(command -v python3 || command -v py || true)"
if [[ -z "${PYTHON}" ]]; then
    echo "refus : python3 introuvable" >&2
    exit 3
fi

STAMP="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
OUT="hermes/propositions/DERNIERE-VEILLE.md"
mkdir -p hermes/propositions

{
    echo "---"
    echo "author: hermes"
    echo "kind: proposition"
    echo "created_at: ${STAMP}"
    echo "concerns: projet"
    echo "status: OPEN"
    echo "---"
    echo "# Veille quotidienne CityLab — ${STAMP}"
    echo
    echo "Run automatique. Pas une instruction. Pas une fusion."
    echo
    echo "## Git"
    echo
    echo "- branche : \`$(git branch --show-current)\`"
    echo "- HEAD : \`$(git log -1 --oneline)\`"
    echo "- porcelain : \`$(git status --porcelain | wc -l) fichier(s)\`"
    echo
    echo "## Roadmap"
    echo
    grep -E '^(last_updated|active_milestone|roadmap_status):' Docs/ROADMAP.md || true
    echo
    echo "## Pipeline"
    echo
    MODE="$(python3 -c 'import json; print(json.load(open("harness/pipeline/config.json"))["mode"])' 2>/dev/null || echo illisible)"
    MERGE="$(python3 -c 'import json; print(json.load(open("harness/pipeline/config.json"))["publish"]["auto_merge"])' 2>/dev/null || echo illisible)"
    echo "- mode harnais : \`${MODE}\`"
    echo "- auto_merge : \`${MERGE}\`"
    echo
    echo "## Tests portables"
    echo
    echo '```'
    if python3 -m unittest discover -s harness/tests -q && python3 -m unittest Tools.tests.test_unity_windows_worker -q; then
        echo "(harnais + parseur Unity : OK)"
    else
        echo "ECHEC tests portables"
    fi
    echo '```'
    echo
    echo "## Vue"
    echo
    if [[ -f hermes/DASHBOARD.md ]]; then
        echo "hermes/DASHBOARD.md présent ($(wc -l < hermes/DASHBOARD.md) lignes)"
    else
        echo "hermes/DASHBOARD.md absent"
    fi
    echo
    echo "Unity EditMode n'est pas lancé par ce cron. Le worker Windows"
    echo "reste un workflow_dispatch manuel."
    echo
    echo "Une proposition nommée \`PROPOSITION-*.md\` n'est ouverte que si un"
    echo "humain confirme un constat nouveau. Ce fichier est seulement la"
    echo "veille du jour."
} > "$OUT"

echo "veille écrite : $OUT"
