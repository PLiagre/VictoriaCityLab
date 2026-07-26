# Gouvernance CityLab

CityLab est un troisieme perimetre, separe de :

- `C:/Users/liagr/VictoriaProject` (`main`, simulation et jeu principal) ;
- `C:/Users/liagr/VictoriaProject-assets` (`assets/main`, AssetFactory).

Le depot CityLab ne modifie aucun fichier de ces deux arbres. Son code metier
reutilisable reste dans le package `com.victoria.citymode`; ses fixtures,
showrooms, assets Vendor et scenes hote restent propres a CityLab.

Cette decision doit etre recopiee par le CTO dans la gouvernance centrale de
Victoria lors de la prochaine mise a jour documentaire. CityLab ne doit pas
etre integre a `main` avant cette validation et avant la migration URP.

