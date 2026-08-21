# ADR-0002 : Hermes pilote CityLab ; Cursor exécute ; le propriétaire fusionne

Date : 21 août 2026  
Statut : accepté par décision explicite du propriétaire  
Amende : ADR-0001 (full-auto Codex + auto-fusion)

## Contexte

Le 12 août 2026, CityLab a copié l'ancien pipeline ForgeHistory : `mode:
full_auto`, `auto_merge: true`, producteur Codex, juge Claude, audit Cursor
des lots critiques, source d'instruction = une ligne `EN_COURS` de la
roadmap, Hermes réduit à un tableau de bord, cron toutes les six heures,
merge bot et `pull_request_target` vers un runner Windows personnel.

ForgeHistory a depuis changé de modèle (ADR-0013, ADR-0014, ADR-0016) :
Hermes pilote, Claude Code écrit le brief et relit en lecture seule,
Cursor exécute dans un worktree `agent/*`, aucune auto-fusion, un cron
quotidien de lecture/mesure/proposition seulement. Le produit vivant de
ForgeHistory est `sim/` (`python -m sim`) ; `unity/` y est en veille. Les
villes doivent devenir la couche 2 de cette simulation unique. CityLab
reste le laboratoire Unity et la vue ville, jamais une seconde source de
vérité économique.

Le contrat du worker Unity Windows est déjà écrit dans ForgeHistory
(`docs/operations/unity-windows-worker.md`). Il doit être implémenté ici,
pas là-bas. CityLab est public : un runner personnel ne doit jamais
répondre à `pull_request` ou `pull_request_target`.

## Décision

1. **Hermes est le pilote** de CityLab, comme sur ForgeHistory. Il propose,
   cadance et lance les lots. Il n'écrit pas le code produit, ni un brief,
   ni un verdict, et il ne fusionne pas.
2. **Chaîne nominale** : propriétaire → Hermes → Claude Code (brief +
   critères, lecture seule) → Cursor (worktree `agent/*`) → portes
   mécaniques → worker Unity Windows si le lot touche le jeu → Claude Code
   (nouvelle invocation, revue lecture seule) → le propriétaire fusionne.
3. **Pas d'auto-fusion.** Pas de cron qui produit ou fusionne. Un cron
   quotidien de lecture / mesure / proposition est autorisé.
4. **`sim/` est la simulation de production.** Toute économie locale
   (`LocalCitySimulation`, horloge parallèle, sauvegarde parallèle) est un
   adaptateur de laboratoire. CityLab n'écrit pas dans ForgeHistory : une
   évolution de `sim/` ou du contrat d'intégration devient une demande
   sous `hermes/requests/`, à recopier côté ForgeHistory.
5. **Le worker Unity Windows** valide le SHA exact d'une branche contrôlée
   par `PLiagre` (`main`, `agent/*`, `cursor/*`) via `workflow_dispatch`
   seulement. Check `unity-windows` : import, compilation, EditMode,
   artefacts. PlayMode graphique, auto-merge et Wake-on-LAN sont hors
   périmètre.
6. L'ancien pipeline (`mode: full_auto`, merge bot, cadence 6 h, audit
   auto-fusionné) passe en **archive réversible** : `mode: manual`,
   workflows retirés visibles, preuves historiques conservées.

## Conséquences

- Un lot CityLab ne part plus tout seul toutes les six heures.
- Codex n'est plus le producteur ; Cursor l'est, dans un worktree isolé.
- Le brief sous `harness/queue/briefs/` devient la source d'instruction
  d'un lot ; la roadmap reste la mémoire produit.
- Tant que le runner `unity` est hors ligne, la PR reste non fusionnable
  si elle touche le jeu ; l'absence n'est jamais un succès.
- La couche 2 « Villes » de ForgeHistory n'est pas commencée ici.

## Ce que cet ADR ne décide pas

- le contenu métier de la couche villes dans `sim/` ;
- le passage du check `unity-windows` en statut GitHub requis ;
- le PlayMode graphique et Unity Build Automation.
