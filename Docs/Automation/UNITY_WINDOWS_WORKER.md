# Worker Unity Windows — Victoria CityLab

Implémentation CityLab du contrat ForgeHistory
`docs/operations/unity-windows-worker.md`. Cette page ne vit que dans ce
dépôt. ForgeHistory reste en lecture seule.

## Faits

- dépôt public `PLiagre/VictoriaCityLab`, branche par défaut `main` ;
- Unity `6000.0.43f1` ;
- Unity Test Framework `1.4.6` ;
- assets lourds via Git LFS ;
- Cursor n'écrit jamais la compatibilité Unity : le worker la prononce.

## Déclenchement

Pendant le pilote : **`workflow_dispatch` seulement**, jamais
`pull_request` ni `pull_request_target`. Le SHA doit appartenir à une
branche contrôlée par `PLiagre` : `main`, `agent/*` ou `cursor/*`.

```bash
gh workflow run unity-windows.yml \
  --repo PLiagre/VictoriaCityLab \
  --ref main \
  -f sha=<40-hex> \
  -f ref_name=agent/exemple
```

Le fichier de workflow est lu depuis `--ref` (en pilote : `main` une fois
fusionné, ou la branche `cursor/*` / `agent/*` qui porte le script). Le
job checkout ensuite le SHA demandé.

## Runner

Labels exigés : `self-hosted`, `windows`, `x64`, `unity`. Un runner déjà
nommé `citylab-full-auto` doit recevoir le label `unity` ; sans ce label
le job reste en file, jamais marqué succès.

## Séquence

1. Vérifier le SHA (40 hex, commit du dépôt, contenu dans une branche
   autorisée).
2. Checkout + `git lfs pull`.
3. Restaurer un cache `Library/` borné (clé : OS + `ProjectVersion` +
   `packages-lock.json`).
4. Vérifier Unity `6000.0.43f1` et `ProjectSettings/ProjectVersion.txt`.
5. Import / compilation + tests **EditMode** via
   `Tools/run_unity_windows_worker.ps1` (pas de `-quit` avec `-runTests`).
6. Parser le XML NUnit : refus si fichier absent, suite vide, échec ou
   test manquant.
7. Publier `unity.log`, le XML et `unity-windows-summary.json`.

Le check s'appelle `unity-windows`. Une indisponibilité du PC Windows
produit un état en attente ou bloqué.

## Hors périmètre de cette première livraison

PlayMode graphique, Wake-on-LAN, Unity Build Automation, auto-fusion, et
statut GitHub requis. Une scène visuellement correcte reste une revue
humaine.
