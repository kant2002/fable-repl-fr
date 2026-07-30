module Tour.Fonctions

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

module FonctionsDeBase =

    /// Vous utilisez "laisser" pour définir une fonction. Celle-ci accepte un argument entier et renvoie un entier.
    /// Les parenthèses sont facultatives pour les arguments de fonction, sauf lorsque vous utilisez une annotation de taper explicite.
    laisser fonctionD'Example1 x = x*x + 3

    /// Appliquez la fonction, en nommant le résultat avec "laisser".
    /// Le taper de la variable est déduit du taper de retour de la fonction.
    laisser résultat1 = fonctionD'Example1 4573

    // Cette ligne utilise '%d' pour afficher le résultat en tant qu'entier. Ceci est sécurisé.
    // Si 'résultat1' n'était pas de taper 'int', alors la ligne échouerait à la compilation.
    printfn "Résultat de l'élévation au carré de l'entier 4573 et de l'ajout de 3 est %d" résultat1

    /// Lorsque nécessaire, annotez le taper d'un nom de paramètre en utilisant `(argument:taper)`. Les parenthèses sont obligatoires.
    laisser fonctionD'Example2 (x:int) = 2*x*x - x/5 + 3

    laisser résultat2 = fonctionD'Example2 (7 + 4)
    printfn "Le résultat de l'application de la 2ème fonction d'exemple à (7 + 4) est : %d" résultat2

    /// Les conditions utilisent si/alors/autsi/autre.
    ///
    /// Remarque : F# utilise une syntaxe basée sur l'indentation de l'espace blanc, similaire aux langages comme Python.
    laisser fonctionD'Example3 x =
        si x < 100.0 alors
            2.0*x*x - x/5.0 + 3.0
        autre
            2.0*x*x + x/5.0 - 37.0

    laisser résultat3 = fonctionD'Example3 (6.5 + 4.5)

    // Cette ligne utilise `%f` pour afficher le résultat en tant que nombre à virgule flottante. Comme pour `%d` ci-dessus, ceci est sécurisé.
    printfn "Le résultat de l'application de la 3ème fonction d'exemple à (6,5 + 4,5) est %f" résultat3


module Immuabilité =

    /// Lier une valeur à un nom via "laisser" la rend immuable.
    /// 
    /// La deuxième ligne de code ne se compile pas car "nombre" est immuable et lié.
    /// Il n'est pas possible de redéfinir "nombre" pour qu'il ait une autre valeur dans F#.
    laisser nombre = 2
    // laisser nombre = 3

    /// Une liaison mutable. Ceci est nécessaire pour pouvoir modifier la valeur de "autreNombre".
    laisser mutable autreNombre = 2

    printfn "'autreNombre' est %d" autreNombre

    // Lorsque vous modifiez une valeur, utilisez "<-" pour attribuer une nouvelle valeur.
    //
    // Remarque : "=" n'est pas le même que "<-". "=" est utilisé pour tester l'égalité.
    autreNombre <- autreNombre + 1

    printfn "'autreNombre' a été modifié pour être %d" autreNombre


module PipelinesEtComposition =

    /// Élève un nombre au carré
    laisser carré x = x * x

    /// Ajoute 1 à une valeur
    laisser ajouteUn x = x + 1

    /// Vérifie si une valeur entière est impaire en utilisant le modulo.
    laisser estImpair x = x % 2 <> 0

    /// Une liste de 5 nombres. Plus d'informations sur les listes plus tard.
    laisser nombres = [ 1; 2; 3; 4; 5 ]

    /// Étant donné une liste d'entiers, elle filtre les nombres pairs,
    /// élève au carré les impairs résultants et ajoute 1 aux impairs carrés.
    laisser éleverAuCarréLesValeursImpairesEtAjouterUn valeurs =
        laisser impairs = List.filter estImpair valeurs
        laisser carrés = List.map carré odds
        laisser résultat = List.map ajouteUn carrés
        résultat

    printfn "Le traitement de %A via 'éleverAuCarréLesValeursImpairesEtAjouterUn' produit: %A"
        nombres (éleverAuCarréLesValeursImpairesEtAjouterUn nombres)

    /// Une manière plus courte d'écrire "éleverAuCarréLesValeursImpairesEtAjouterUn" 
    /// consiste à imbriquer chaque résultat dans les appels de fonction eux-mêmes.
    ///
    /// Cela rend la fonction beaucoup plus courte, mais il est difficile de voir l'ordre
    /// dans lequel les données sont traitées.
    laisser éleverAuCarréLesValeursImpairesEtAjouterUnImbriqué valeurs =
        List.map ajouteUn (List.map carré (List.filter estImpair valeurs))

    printfn "Le traitement %A via 'éleverAuCarréLesValeursImpairesEtAjouterUnImbriqué' produit: %A"
        nombres (éleverAuCarréLesValeursImpairesEtAjouterUnImbriqué nombres)

    /// Une manière préférée d'écrire "éleverAuCarréLesValeursImpairesEtAjouterUn" 
    /// consiste à utiliser les opérateurs de canalisation F#.
    /// Cela vous permet d'éviter de créer des résultats intermédiaires, 
    /// mais c'est beaucoup plus lisible que le fait d'imbriquer des appels 
    /// de fonction comme "éleverAuCarréLesValeursImpairesEtAjouterUnImbriqué".
    laisser éleverAuCarréLesValeursImpairesEtAjouterPipeline valeurs =
        valeurs
        |> List.filter estImpair
        |> List.map carré
        |> List.map ajouteUn

    printfn "Le traitement %A via 'éleverAuCarréLesValeursImpairesEtAjouterPipeline' produit: %A"
        nombres (éleverAuCarréLesValeursImpairesEtAjouterPipeline nombres)

    /// Vous pouvez raccourcir "éleverAuCarréLesValeursImpairesEtAjouterPipeline" 
    /// en déplaçant la deuxième invocation de `List.map` dans la première, à l'aide d'une fonction lambda.
    ///
    /// Remarque : les pipelines sont également utilisés à l'intérieur de la fonction lambda. Les opérateurs de canalisation F# peuvent être
    /// utilisés pour des valeurs uniques. Cela les rend très puissants pour le traitement des données.
    laisser squareOddValuesAndAddOneShorterPipeline valeurs =
        valeurs
        |> List.filter estImpair
        |> List.map(fon x -> x |> carré |> ajouteUn)

    printfn "Le traitment %A via 'squareOddValuesAndAddOneShorterPipeline' produit: %A"
        nombres (squareOddValuesAndAddOneShorterPipeline nombres)


module FonctionsRécursives =

    /// Cet exemple montre une fonction récursive qui calcule la factorielle d'un
    /// entier. Elle utilise "laisser réc" pour définir une fonction récursive.
    laisser réc factorielle n =
        si n = 0 alors 1 autre n * factorielle (n-1)

    printfn "La factorielle de 6 est: %d" (factorielle 6)

    /// Calcule le plus grand commun diviseur de deux entiers.
    ///
    /// Puisque toutes les appels récursifs sont des appels de queue,
    /// le compilateur transformera la fonction en boucle,
    /// ce qui améliore les performances et réduit la consommation de mémoire.
    laisser réc grandCommunDiviseur a b =
        si a = 0 alors b
        autsi a < b alors grandCommunDiviseur a (b - a)
        autre grandCommunDiviseur (a - b) b

    printfn "Le plus grand commun diviseur de 300 et 620 est %d" (grandCommunDiviseur 300 620)

    /// Cet exemple calcule la somme d'une liste d'entiers en utilisant la récursion.
    laisser réc sommeDeLaList xs =
        correspondre xs avec
        | []    -> 0
        | y::ys -> y + sommeDeLaList ys

    /// Cela rend "sommeDeLaList" récursif de manière de queue, en utilisant une fonction auxiliaire avec un accumulateur de résultat.
    laisser réc privée sommeDeLaListRécAux accumulator xs =
        correspondre xs avec
        | []    -> accumulator
        | y::ys -> sommeDeLaListRécAux (accumulator+y) ys

    /// Cela invoque la fonction auxiliaire récursive de queue, en fournissant "0" comme accumulateur initial.
    /// Une approche comme celle-ci est courante en F#.
    laisser sommeDeLaListRéc xs = sommeDeLaListRécAux 0 xs

    laisser deUnÀDix = [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]

    printfn "Somme de 1 à 10 est %d" (sommeDeLaListRéc deUnÀDix)
