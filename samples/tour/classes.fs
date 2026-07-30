module Tour.Classes

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

/// Les classes permettent de définir de nouveaux types d'objets en F# et prennent en charge les constructions standard de la programmation orientée objet.
/// Elles peuvent comporter divers membres (méthodes, propriétés, événements, etc.).
///
/// Pour en savoir plus sur les classes, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/classes
///
/// Pour en savoir plus sur les membres, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/members

/// Une classe Vecteur2D simple.
///
/// Le constructeur de la classe se trouve sur la première ligne
/// et prend deux arguments: dx et dy, tous deux de taper 'double'.
taper Vecteur2D(dx : double, dy : double) =

    /// Ce champ interne stocke la longueur du vecteur, calculée lors de la
    /// construction de l'objet.
    laisser longueur = sqrt (dx*dx + dy*dy)

    // 'this' spécifie un nom pour l'identifiant interne de l'objet. 
    // Dans les méthodes d'instance, il doit précéder le nom du membre.
    membre this.DX = dx

    membre this.DY = dy

    membre this.Longueur = longueur

    /// Ce membre est une méthode. Les membres précédents étaient des propriétés.
    membre this.Échelle(k) = Vecteur2D(k * this.DX, k * this.DY)

/// Voici comment instancier la classe Vecteur2D.
laisser vecteur1 = Vecteur2D(3.0, 4.0)

/// Obtenez un nouvel objet vectoriel mis à l'échelle, sans modifier l'objet d'origine.
laisser vecteur2 = vecteur1.Échelle(10.0)

printfn "Longueur du vecteur1: %f\nLongueur du vecteur2: %f" vecteur1.Longueur vecteur2.Longueur


/// Les classes génériques permettent de définir des types en fonction d'un ensemble de paramètres de taper.
/// Dans l'exemple suivant, 'T est le paramètre de taper de la classe.
///
/// Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/generics/

taper SuiviD'État<'T>(élémentInitial: 'T) =

    /// Ce champ interne stocke les états dans une liste.
    laisser mutable états = [ élémentInitial ]

    /// Ajoutez un nouvel élément à la liste des états.
    membre this.MettreÀJourLÉtat nouvelÉtat =
        états <- nouvelÉtat :: états  // Utilisez l'opérateur '<-' pour modifier la valeur.

    /// Obtient la liste entière des états historiques.
    membre this.History = états

    /// Obtient l'état le plus récent.
    membre this.Current = états.Head

/// Une instance de taper 'int' de la classe de suivi d'état. Notez que le paramètre de taper est déduit.
laisser suivi = SuiviD'État 10

// Ajouter un état
suivi.MettreÀJourLÉtat 17


/// Les interfaces sont des types d'objets ne comportant que des membres « abstraits ».
/// Les types d'objets et les expressions d'objet peuvent implémenter des interfaces.
///
/// Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/interfaces

/// Il s'agit d'un taper qui implémente IDisposable.
taper LireLeFichier(path: string) =
    membre this.LireLaLigne() = printfn "En lisant %s..." path

    // Il s'agit de l'implémentation des membres de IDisposable.
    interface System.IDisposable avec
        membre this.Dispose() = printfn "Fermeture de %s..." path


/// Il s'agit d'un objet qui implémente IDisposable via une expression d'objet.
/// Contrairement à d'autres langages comme le C#, aucune nouvelle définition de taper n'est nécessaire
/// pour implémenter une interface.
laisser implémentationD'Interface =
    { nouvelle System.IDisposable avec
        membre this.Dispose() = printfn "disposé" }
