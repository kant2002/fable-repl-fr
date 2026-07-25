module VisiteGuidée.Primitives

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

module EntiersEtNombres =

    /// Ceci est un entier d'exemple.
    let unEntierDExemple = 176

    /// Ceci est un nombre à virgule flottante d'exemple.
    let unNombreAVirguleFlottanteDExemple = 4.1

    /// Ceci calcule un nouveau nombre par une arithmétique. Les types numériques sont convertis
    /// en utilisant les fonctions 'int', 'double' et ainsi de suite.
    let unEntierDExemple2 = (unEntierDExemple/4 + 5 - 7) * 4 + int unNombreAVirguleFlottanteDExemple

    /// Ceci est une liste des nombres de 0 à 99.
    let numérosDÉchantillon = [ 0 .. 99 ]

    /// Ceci est une liste de tous les tuples contenant tous les nombres de 0 à 99 et leurs carrés.
    let exempleDeTableDesCarrés = [ for i in 0 .. 99 -> (i, i*i) ]

    // La ligne suivante affiche une liste qui comprend des tuples, en utilisant '%A' pour l'impression générique.
    printfn $"La table des carrés de 0 à 99 est:\n{exempleDeTableDesCarrés}"


module Booléens =

    /// Les valeurs booléennes sont 'true' et 'false'.
    let booléen1 = true
    let booléen2 = false

    /// Opérateurs sur les booléens sont 'not', '&&' et '||'.
    let booléen3 = not booléen1 && (booléen2 || false)

    // Cette ligne utilise '%b' pour afficher une valeur booléenne. Cela est type-safe.
    printfn $"L'expression 'not booléen1 && (booléen2 || false)' est %b{booléen3}"


module ManipulationDeChaînes =

    /// Les chaînes utilisent des guillemets doubles.
    let chaîne1 = "Bonjour"
    let chaîne2  = "le monde"

    /// Les chaînes peuvent également utiliser @ pour créer un littéral de chaîne verbeux.
    /// Cela ignore les caractères d'échappement tels que '\', '\n', '\t', etc.
    let chaîne3 = @"C:\Program Files\"

    /// Les littéraux de chaîne peuvent également utiliser des guillemets triples.
    let chaîne4 = """L'ordinateur a dit "Bonjour le monde" quand je le lui ai demandé !"""

    /// La concaténation de chaînes de caractères se fait généralement avec l'opérateur '+'.
    let bonjourLeMonde = chaîne1 + " " + chaîne2

    // Cette ligne utilise '%s' pour afficher une valeur de chaîne. Cela est type-safe.
    printfn "%s" bonjourLeMonde

    /// Les sous-chaînes utilisent la notation d'index. Cette ligne extrait les 7 premiers caractères comme une sous-chaîne.
    /// Notez que comme de nombreux langages, les chaînes sont indexées à partir de zéro en F#.
    let souschaîne = bonjourLeMonde.[0..6]
    printfn "%s" souschaîne
