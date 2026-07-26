module Visite.UnitésDeMesure

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

// Les unités de mesure sont un moyen d'annoter les types numériques primitifs de manière sécurisée.
// Vous pouvez ensuite effectuer des opérations arithmétiques sur ces valeurs de manière sécurisée.
//
// Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/units-of-measure

// Tout d'abord, ouvrez une liste de noms de mesures courants
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

/// Définissez une constante unifiée
let valeurD'exemple1 = 1600.0<metre>

/// Ensuite, définissez un nouveau type d'unité
[<Measure>]
type mille =
    /// Facteur de conversion miles en mètres
    static member enMetrès = 1609.34<metre/mille>

/// Définir une constante unifiée
let valeurD'exemple2 = 500.0<mille>

/// Calculer la constante du système métrique
let valeurD'exemple3 = valeurD'exemple2 * mille.enMetrès

// Les valeurs utilisant les unités de mesure peuvent être utilisées comme le type numérique primitif pour des choses comme l'impression.
printfn "Après une course de %f, je marcherais %f miles, ce qui équivaudrait à %f mètres" valeurD'exemple1 valeurD'exemple2 valeurD'exemple3
