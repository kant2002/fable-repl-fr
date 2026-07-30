module Visite.Primitives

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

module EntiersEtNombres =

    /// Ceci est un entier d'exemple.
    laisser unEntierDExemple = 176

    /// Ceci est un nombre à virgule flottante d'exemple.
    laisser unNombreAVirguleFlottanteDExemple = 4.1

    /// Ceci calcule un nouveau nombre par une arithmétique. Les types numériques sont convertis
    /// en utilisant les fonctions 'int', 'double' et ainsi de suite.
    laisser unEntierDExemple2 = (unEntierDExemple/4 + 5 - 7) * 4 + int unNombreAVirguleFlottanteDExemple

    /// Ceci est une liste des nombres de 0 à 99.
    laisser numérosDÉchantillon = [ 0 .. 99 ]

    /// Ceci est une liste de tous les tuples contenant tous les nombres de 0 à 99 et leurs carrés.
    laisser exempleDeTableDesCarrés = [ pour i dans 0 .. 99 -> (i, i*i) ]

    // La ligne suivante affiche une liste qui comprend des tuples, en utilisant '%A' pour l'impression générique.
    printfn $"La table des carrés de 0 à 99 est:\n{exempleDeTableDesCarrés}"


module Booléens =

    /// Les valeurs booléennes sont 'vraie' et 'faux'.
    laisser booléen1 = vraie
    laisser booléen2 = faux

    /// Opérateurs sur les booléens sont 'not', '&&' et '||'.
    laisser booléen3 = not booléen1 && (booléen2 || faux)

    // Cette ligne utilise '%b' pour afficher une valeur booléenne. Cela est taper-safe.
    printfn $"L'expression 'not booléen1 && (booléen2 || faux)' est %b{booléen3}"


module ManipulationDeChaînes =

    /// Les chaînes utilisent des guillemets doubles.
    laisser chaîne1 = "Bonjour"
    laisser chaîne2  = "le monde"

    /// Les chaînes peuvent également utiliser @ pour créer un littéral de chaîne verbeux.
    /// Cela ignore les caractères d'échappement tels que '\', '\n', '\t', etc.
    laisser chaîne3 = @"C:\Program Files\"

    /// Les littéraux de chaîne peuvent également utiliser des guillemets triples.
    laisser chaîne4 = """L'ordinateur a dit "Bonjour le monde" quand je le lui ai demandé !"""

    /// La concaténation de chaînes de caractères se fait généralement avec l'opérateur '+'.
    laisser bonjourLeMonde = chaîne1 + " " + chaîne2

    // Cette ligne utilise '%s' pour afficher une valeur de chaîne. Cela est taper-safe.
    printfn "%s" bonjourLeMonde

    /// Les sous-chaînes utilisent la notation d'index. Cette ligne extrait les 7 premiers caractères comme une sous-chaîne.
    /// Notez que comme de nombreux langages, les chaînes sont indexées à partir de zéro en F#.
    laisser souschaîne = bonjourLeMonde.[0..6]
    printfn "%s" souschaîne
