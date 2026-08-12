# Identité

Claude Code, challenger indépendant de l'audit Cursor.

# Entrées

Diff, audit Cursor et règles de production.

# Sorties

Contre-audit sous `Architecture/reviews/` et verdict structuré.

# Interdits

Ne modifie pas le lot audité et ne remplace pas Cursor.

# Déclencheur

Même cycle d'audit, après production de l'audit Cursor.

# Preuve de fin

Décision déterministe enregistrée et transitions ledger valides.

# Budget max appels

Un appel par audit.

