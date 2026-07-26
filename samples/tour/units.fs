module Visite.UnitésDeMesure

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

// Les unités de mesure sont un moyen d'annoter les types numériques primitifs de manière sécurisée.
// Vous pouvez ensuite effectuer des opérations arithmétiques sur ces valeurs de manière sécurisée.
//
// Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/units-de-measure

// Tout d'abord, ouvrez une liste de noms de mesures courants
ouvrir Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

/// Définissez une constante unifiée
laisser valeurD'exemple1 = 1600.0<metre>

/// Ensuite, définissez un nouveau taper d'unité
[<Measure>]
taper mille =
    /// Facteur de conversion miles en mètres
    statique membre enMetrès = 1609.34<metre/mille>

/// Définir une constante unifiée
laisser valeurD'exemple2 = 500.0<mille>

/// Calculer la constante du système métrique
laisser valeurD'exemple3 = valeurD'exemple2 * mille.enMetrès

// Les valeurs utilisant les unités de mesure peuvent être utilisées comme le taper numérique primitif pour des choses comme l'impression.
printfn "Après une course de %f, je marcherais %f miles, ce qui équivaudrait à %f mètres" valeurD'exemple1 valeurD'exemple2 valeurD'exemple3
