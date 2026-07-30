module Tour.Collections

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

module Listes =

    /// Les listes sont définies à l'aide de [ ... ]. Il s'agit d'une liste vide.
    laisser liste1 = [ ]

    /// Il s'agit d'une liste de 3 éléments. Le point-virgule (';') est utilisé pour séparer les éléments sur une même ligne.
    laisser liste2 = [ 1; 2; 3 ]

    /// Vous pouvez également séparer les éléments en les plaçant sur des lignes séparées.
    laisser liste3 = [
        1
        2
        3
    ]

    /// Ceci est une liste d'entiers de 1 à 1000
    laisser listeNombre = [ 1 .. 1000 ]

    /// Les listes peuvent également être générées par des calculs. Cette liste contient
    /// tous les jours de l'année.
    laisser listeJour =
        [ pour month dans 1 .. 12 faire
              pour day dans 1 .. System.DateTime.DaysInMonth(2017, month) faire
                  rendement System.DateTime(2017, month, day) ]

    // Imprime les 5 premiers éléments de 'listeJour' en utilisant 'List.take'.
    printfn "Les 5 premiers jours de 2017 sont: %A" (listeJour |> List.take 5)

    /// Les calculs peuvent inclure des conditions. Il s'agit d'une liste contenant les tuples
    /// correspondant aux coordonnées des cases noires d'un échiquier.
    laisser listeNoir =
        [ pour i dans 0 .. 7 faire
              pour j dans 0 .. 7 faire
                  si (i+j) % 2 = 1 alors
                      rendement (i, j) ]

    /// Les listes peuvent être transformées à l'aide de List.map et d'autres combinateurs de programmation fonctionnelle. 
    /// Cette définition produit une nouvelle liste en élevant au carré les nombres de listeNombre, en utilisant l'opérateur de pipeline
    /// pour transmettre un argument à List.map.
    laisser squares =
        listeNombre
        |> List.map (fon x -> x*x)

    /// Il existe de nombreuses autres combinaisons de listes. La définition suivante calcule la somme des carrés des
    /// nombres divisibles par 3.
    laisser sommeDesCarrés =
        listeNombre
        |> List.filter (fon x -> x % 3 = 0)
        |> List.sumBy (fon x -> x * x)

    printfn "La somme des carrés des nombres jusqu'à 1000 divisibles par 3 est: %d" sommeDesCarrés


module Tableaux =

    /// Il s'agit du tableau vide. Notez que la syntaxe est similaire à celle des listes, mais utilise `[| ... |]` à la place.
    laisser tableau1 = [| |]

    /// Les tableaux sont définis à l'aide des mêmes types de constructions que les listes.
    laisser tableau2 = [| "bonjour"; "monde"; "et"; "bonjour"; "monde"; "encore" |]

    /// Il s'agit d'un tableau de nombres de 1 à 1000.
    laisser tableau3 = [| 1 .. 1000 |]

    /// Il s'agit d'un tableau contenant uniquement les mots "bonjour" et "monde".
    laisser tableau4 =
        [| pour mot dans tableau2 faire
               si mot.Contains("n") alors
                   rendement mot |]

    /// Il s'agit d'un tableau initialisé par index et contenant les nombres pairs de 0 à 2000.
    laisser nombresPairs = Array.init 1001 (fon n -> n * 2)

    /// Les sous-tableaux sont extraits à l'aide de la notation de découpage (slicing).
    laisser nombresPairsSlice = nombresPairs[0..500]

    // Vous pouvez parcourir des tableaux et des listes à l'aide de boucles 'pour'.
    pour mot dans tableau4 faire
        printfn "mot: %s" mot

    // Vous pouvez modifier le contenu d'un élément de tableau à l'aide de l'opérateur d'affectation à gauche.
    //
    // Pour en savoir plus sur cet opérateur, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/values/index#mutable-variables
    tableau2[1] <- "MONDE!"

    /// Vous pouvez transformer des tableaux à l'aide de 'Array.map' et d'autres opérations de programmation fonctionnelle.
    /// La définition suivante calcule la somme des longueurs des mots qui commencent par 'b'.
    laisser sommeDesLongueursDesMots =
        tableau2
        |> Array.filter (fon x -> x.StartsWith "b")
        |> Array.sumBy (fon x -> x.Length)

    printfn "La somme des longueurs des mots dans le tableau 2 est: %d" sommeDesLongueursDesMots


module Séquences =

    /// C'est la séquence vide.
    laisser séq1 = Seq.empty

    /// Il s'agit d'une séquence de valeurs.
    laisser séq2 = seq { rendement "bonjour"; rendement "monde"; rendement "et"; rendement "bonjour"; rendement "monde"; rendement "encore" }

    /// C'est une séquence demandée de 1 à 1000.
    laisser séqDeNombres = seq { 1 .. 1000 }

    /// C'est une séquence produisant les mots "bonjour" et "monde".
    laisser séq3 =
        seq { pour mot dans séq2 faire
                  si mot.Contains("n") alors
                      rendement mot }

    /// C'est une séquence produisant les nombres pairs de 0 à 2000.
    laisser nombresPairs = Seq.init 1001 (fon n -> n * 2)

    laisser rnd = System.Random()

    /// Il s'agit d'une séquence infinie correspondant à une marche aléatoire. 
    /// Cet exemple utilise rendement! pour renvoyer chaque élément d'une sous-séquence.
    laisser réc marcheAléatoire x =
        seq { rendement x
              rendement! marcheAléatoire (x + rnd.NextDouble() - 0.5) }

    /// Cet exemple montre les 100 premiers éléments de la marche aléatoire.
    laisser les100PremièresValeursDeLaMarcheAléatoire =
        marcheAléatoire 5.0
        |> Seq.truncate 100
        |> Seq.toList

    printfn "Les 100 premiers éléments d'une marche aléatoire: %A" les100PremièresValeursDeLaMarcheAléatoire
