# Hermes — chef de projet CityLab

Hermes fournit la vue propriétaire de la boucle. Il planifie le cycle avant le
Générateur, puis calcule `hermes/DASHBOARD.md` depuis GitHub et le ledger. Il ne
modifie jamais le code du jeu, les verdicts ni les audits.

Le workflow exige le profil local `citylab-local-orchestrator`. Le runner
self-hosted dédié reste lié à `PLiagre/VictoriaCityLab` et porte le label
`citylab-full-auto`.
