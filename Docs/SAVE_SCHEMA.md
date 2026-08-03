# Schéma de sauvegarde CityLab

## Portée

`CitySaveService` persiste un `CitySnapshot` complet sans référence à un
`GameObject`. Le snapshot contient l'horloge, les stocks et réservations, les
foyers, routes, parcelles, bâtiments, habitants/emplois et sites de production.
Les prochains systèmes de simulation devront ajouter leur état au snapshot et
faire évoluer son `schemaVersion` avant d'être considérés sauvegardables.

## Enveloppe version 1

```json
{
  "format": "victoria-citylab-save",
  "formatVersion": 1,
  "snapshotSchemaVersion": 1,
  "payloadSha256": "sha256-du-payload-utf8",
  "payload": "{...CitySnapshot...}"
}
```

`formatVersion` versionne l'enveloppe et l'algorithme d'intégrité ;
`snapshotSchemaVersion` versionne les données métier. Le SHA-256 porte sur les
octets UTF-8 exacts de `payload`. Un format futur, un schéma futur, un hash
incorrect, un JSON invalide ou une ville sans identifiant sont refusés avec une
raison stable, sans remplacer la simulation courante.

## Écriture et commandes de jeu

L'écriture se fait dans le même dossier sous le suffixe `.tmp`, avec flush sur
disque, puis déplacement ou remplacement atomique. Le fichier temporaire est
nettoyé en cas d'échec. Les chemins runtime sont :

- `Application.persistentDataPath/CityLab/Saves/city_1001.save.json` pour F5 ;
- `Application.persistentDataPath/CityLab/Saves/city_1001.autosave.json` toutes
  les 120 secondes réelles ;
- F9 recharge la sauvegarde manuelle uniquement après validation complète de
  l'enveloppe, du checksum et du snapshot.

Les runs smoke/capture automatisés n'écrivent aucun autosave.

## Migration et validation

La migration initiale accepte le snapshot v0 vérifiable
`Packages/com.victoria.citymode/Tests/Fixtures/city_save_v0.json`, initialise
les collections absentes et produit un snapshot v1. Les tests Editor couvrent
aller-retour exact, remplacement manuel, autosave, nettoyage `.tmp`, corruption
et migration. Ils sont ajoutés mais restent à exécuter lors de la prochaine
session Unity CityLab autorisée ; cette session n'a pas lancé Unity.
