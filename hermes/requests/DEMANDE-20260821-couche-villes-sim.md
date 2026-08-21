---
author: cursor-executant
kind: demande
created_at: 2026-08-21T00:00:00Z
concerns: ForgeHistory sim/ couche 2 Villes
status: OPEN
---

# Demande Hermes à recopier dans ForgeHistory

Cette demande vit dans CityLab. Elle n'est **pas** un patch ForgeHistory.
La recopier sous `PLiagre/ForgeHistory/hermes/requests/` si le
propriétaire l'accepte.

## Contexte

CityLab a un contrat v1 (`CityLaunchContext`, snapshot révisionné,
intentions idempotentes) et des packages de présentation portables. La
roadmap CityLab `M3-FH-05` / `M3-FH-07` reste bloquée : il n'existe pas
encore de couche villes dans `sim/`.

Le propriétaire a tranché : les villes deviennent des éléments de la
simulation ForgeHistory (moteur `sim/`, couche 2), pas une économie
parallèle Unity.

## Demande

1. Ouvrir la couche 2 « Villes » dans `sim/` comme unique autorité
   économique urbaine.
2. Exposer lecture de snapshot et application d'intention alignées sur
   le contrat CityLab v1 déjà publié (révision, `intentId`, refus
   explicite).
3. Ne pas réactiver `unity/` comme seconde simulation.
4. Garder CityLab comme laboratoire / vue : scènes, rendu, HUD, assets,
   EditMode.

## Hors périmètre CityLab

Aucun fichier de `PLiagre/ForgeHistory` n'est modifié par le lot qui
dépose cette demande.
