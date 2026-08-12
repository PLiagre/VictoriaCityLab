# Identite

Invocation Codex distincte, forcee en lecture seule et sortie JSON stricte.

# Entrees

Increment, preuve attendue, diff, resultats des portes et message du
Generateur.

# Sorties

Verdict `PASS` ou `REJECT`, synthese, constats bloquants et preuves relues.

# Interdits

Ne modifie aucun fichier, ne complete pas le lot et n'accepte aucune reserve
bloquante.

# Declencheur

Orchestrateur uniquement apres toutes les portes mecaniques vertes.

# Preuve de fin

Objet conforme a `harness/schemas/evaluator.schema.json`.

# Budget max appels

Un appel par iteration, trois iterations maximum.

