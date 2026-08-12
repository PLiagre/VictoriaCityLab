# ADR-0001 : adaptation de l'architecture full-auto ForgeHistory

Date : 12 aout 2026  
Statut : accepte par demande explicite du proprietaire

## Contexte

Victoria CityLab disposait d'une roadmap tres detaillee et de portes Unity,
mais l'execution restait conversationnelle. ForgeHistory fournit une
architecture multi-roles avec file autoritaire, producteur separe du juge,
portes mecaniques, budgets, kill switch et publication conditionnelle.

## Decision

Adopter ces proprietes sous une forme native au depot CityLab : la roadmap
existante remplace les briefs, Codex est le Generateur, une seconde invocation
Codex en lecture seule est l'Evaluateur, et un orchestrateur Python standard
library gere les retries, preuves et operations GitHub. La CI execute aussi les
tests structurels du mecanisme.

Le pipeline est active (`mode: full_auto`) mais echoue ferme si le secret Codex,
les outils, les preuves ou l'auto-merge manquent. Les quatre coupe-circuits
restent disponibles.

## Consequences

- Un increment peut avancer sans presence humaine jusqu'a sa PR fusionnee.
- Le producteur ne peut pas s'auto-publier : seul le verdict structure d'une
  invocation distincte ouvre la porte.
- Les changements de gouvernance, de CI, d'orchestration et Vendor restent
  volontairement manuels.
- Le cout et les boucles sont bornes a trois iterations par increment.

