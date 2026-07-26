module Visite.TuplesEtEnregistrements

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

module Tuples =

    /// Un tuple simple d'entiers.
    laisser tuple1 = (1, 2, 3)

    /// Une fonction qui inverse l'ordre de deux valeurs dans un tuple.
    ///
    /// La déduction de taper F# généralisera automatiquement la fonction pour qu'elle ait un taper générique,
    /// ce qui signifie qu'elle fonctionnera avec n'importe quel taper.
    laisser échangerLesÉléments (a, b) = (b, a)

    printfn "Le résultat de l'échange (1, 2) est %A" (échangerLesÉléments (1,2))

    /// Un tuple comprenant un entier, une chaîne de caractères
    /// et un nombre à virgule flottante de précision double.
    laisser tuple2 = (1, "fred", 3.1415)

    printfn "tuple1: %A\ttuple2: %A" tuple1 tuple2


module TypesEnregistrements =

    /// Cet exemple montre comment définir un nouveau taper d'enregistrement.
    taper CarteDeContact =
        { Nom     : string
          Phone    : string
          Vérifié : bool }

    /// Cet exemple montre comment instancier un taper d'enregistrement.
    laisser contact1 =
        { Nom = "Alf"
          Phone = "(206) 555-0157"
          Vérifié = faux }

    /// Vous pouvez également le faire en une seule ligne avec des séparateurs ';'.
    laisser contactSurLaMêmeLigne = { Nom = "Alf"; Phone = "(206) 555-0157"; Vérifié = faux }

    /// Cet exemple montre comment utiliser l'"opération copie et mise à jour" sur les valeurs d'enregistrement. Il crée une nouvelle valeur
    /// d'enregistrement qui est une copie de contact1, mais avec des valeurs différentes pour les champs "Téléphone" et "Vérifié".
    ///
    /// Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/copy-et-update-record-expressions
    laisser contact2 =
        { contact1 avec
            Phone = "(206) 555-0112"
            Vérifié = vraie }

    /// Cet exemple montre comment écrire une fonction qui traite une valeur d'enregistrement.
    /// Il convertit un objet "ContactCard" en chaîne de caractères.
    laisser afficherLaCarteDeContact (c: CarteDeContact) =
        c.Nom + " Phone: " + c.Phone + (si not c.Vérifié alors " (non vérifié)" autre "")

    printfn "Carte de contact d'Alf: %s" (afficherLaCarteDeContact contact1)

    /// Voici un exemple d'un Enregistrement avec un membre.
    taper CarteDeContactAlternate =
        { Nom     : string
          Phone    : string
          Adresse  : string
          Vérifié : bool }

        /// Les membres peuvent implémenter des membres orientés objet.
        membre this.CarteDeContactImprimeé =
            this.Nom + " Phone: " + this.Phone + (si not this.Vérifié alors " (non vérifié)" autre "") + this.Adresse

    laisser contactAlternate =
        { Nom = "Alf"
          Phone = "(206) 555-0157"
          Vérifié = faux
          Adresse = "111 Alf Street" }

    // Les membres sont accessibles via l'opérateur "." sur un taper instancié.
    printfn "La carte de contact alternative d'Alf est %s" contactAlternate.CarteDeContactImprimeé
