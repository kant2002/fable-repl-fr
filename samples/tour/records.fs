module Visite.TuplesEtEnregistrements

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

module Tuples =

    /// Un tuple simple d'entiers.
    let tuple1 = (1, 2, 3)

    /// Une fonction qui inverse l'ordre de deux valeurs dans un tuple.
    ///
    /// La déduction de type F# généralisera automatiquement la fonction pour qu'elle ait un type générique,
    /// ce qui signifie qu'elle fonctionnera avec n'importe quel type.
    let échangerLesÉléments (a, b) = (b, a)

    printfn "Le résultat de l'échange (1, 2) est %A" (échangerLesÉléments (1,2))

    /// Un tuple comprenant un entier, une chaîne de caractères
    /// et un nombre à virgule flottante de précision double.
    let tuple2 = (1, "fred", 3.1415)

    printfn "tuple1: %A\ttuple2: %A" tuple1 tuple2


module TypesEnregistrements =

    /// Cet exemple montre comment définir un nouveau type d'enregistrement.
    type CarteDeContact =
        { Nom     : string
          Phone    : string
          Vérifié : bool }

    /// Cet exemple montre comment instancier un type d'enregistrement.
    let contact1 =
        { Nom = "Alf"
          Phone = "(206) 555-0157"
          Vérifié = false }

    /// Vous pouvez également le faire en une seule ligne avec des séparateurs ';'.
    let contactSurLaMêmeLigne = { Nom = "Alf"; Phone = "(206) 555-0157"; Vérifié = false }

    /// Cet exemple montre comment utiliser l'"opération copie et mise à jour" sur les valeurs d'enregistrement. Il crée une nouvelle valeur
    /// d'enregistrement qui est une copie de contact1, mais avec des valeurs différentes pour les champs "Téléphone" et "Vérifié".
    ///
    /// Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/copy-and-update-record-expressions
    let contact2 =
        { contact1 with
            Phone = "(206) 555-0112"
            Vérifié = true }

    /// Cet exemple montre comment écrire une fonction qui traite une valeur d'enregistrement.
    /// Il convertit un objet "ContactCard" en chaîne de caractères.
    let afficherLaCarteDeContact (c: CarteDeContact) =
        c.Nom + " Phone: " + c.Phone + (if not c.Vérifié then " (non vérifié)" else "")

    printfn "Carte de contact d'Alf: %s" (afficherLaCarteDeContact contact1)

    /// Voici un exemple d'un Enregistrement avec un membre.
    type CarteDeContactAlternate =
        { Nom     : string
          Phone    : string
          Adresse  : string
          Vérifié : bool }

        /// Les membres peuvent implémenter des membres orientés objet.
        member this.CarteDeContactImprimeé =
            this.Nom + " Phone: " + this.Phone + (if not this.Vérifié then " (non vérifié)" else "") + this.Adresse

    let contactAlternate =
        { Nom = "Alf"
          Phone = "(206) 555-0157"
          Vérifié = false
          Adresse = "111 Alf Street" }

    // Les membres sont accessibles via l'opérateur "." sur un type instancié.
    printfn "La carte de contact alternative d'Alf est %s" contactAlternate.CarteDeContactImprimeé
