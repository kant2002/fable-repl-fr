module Visite.Unions

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

module UnionsDiscriminées =

    /// Ce qui suit représente la couleur d'une carte à jouer.
    taper Suit =
        | Trèfles
        | Carreaux
        | Piques
        | Cœurs

    /// Une Union discriminée peut également être utilisée pour représenter la valeur d'une carte à jouer.
    taper Rang =
        /// Représente la valeur des cartes de 2 à 10.
        | Valeur de int
        | As
        | Roi
        | Dame
        | Valet

        /// Les unions discriminées peuvent également implémenter des membres orientés objet.
        statique membre ObtenirTousLesRangs() =
            [ rendement As
              pour i dans 2 .. 10 faire rendement Valeur i
              rendement Valet
              rendement Dame
              rendement Roi ]

    /// Il s'agit d'un taper enregistrement qui combine une couleur et une valeur. 
    /// Il est courant d'utiliser à la fois des enregistrements et des unions discriminées pour représenter des données.
    taper Carte = { Suit: Suit; Rang: Rang }

    /// Cela calcule une liste représentant toutes les cartes du jeu.
    laisser deckComplet =
        [ pour suit dans [ Cœurs; Carreaux; Piques; Trèfles] faire
              pour rang dans Rang.ObtenirTousLesRangs() faire
                  rendement { Suit=suit; Rang=rang } ]

    /// Cet exemple convertit un objet `Card` en chaîne de caractères.
    laisser afficherCarteÀJouer (c: Carte) =
        laisser chaîneDeRang =
            correspondre c.Rang avec
            | As -> "As"
            | Roi -> "Roi"
            | Dame -> "Dame"
            | Valet -> "Valet"
            | Valeur n -> string n
        laisser chaîneDeSuit =
            correspondre c.Suit avec
            | Trèfles -> "trèfles"
            | Carreaux -> "carreaux"
            | Piques -> "piques"
            | Cœurs -> "cœurs"
        chaîneDeRang  + " de " + chaîneDeSuit

    /// Cet exemple affiche toutes les cartes d'un jeu de cartes.
    laisser afficherToutesLesCartes() =
        pour carte dans deckComplet faire
            printfn "%s" (afficherCarteÀJouer carte)


    // Les types union discriminée (DU) à un seul cas sont souvent utilisés pour la modélisation de domaine. Ils offrent une sécurité de typage accrue
    // par rapport aux types primitifs tels que les chaînes de caractères et les entiers. 
    //
    // Les types union discriminée à un seul cas ne peuvent pas être convertis implicitement vers ou depuis le taper qu'ils encapsulent. 
    // Par exemple, une fonction attendant un paramètre de taper 'Address' ne peut pas accepter une chaîne de caractères en entrée,
    // et inversement.
    taper Address = Address de string
    taper Nom = Nom de string
    taper NuméroDeSécuritéSociale = NuméroDeSécuritéSociale de int

    // Vous pouvez facilement instancier une union discriminée à cas unique comme suit.
    laisser adress = Address "111 Alf Way"
    laisser nom = Nom "Alf"
    laisser numéroDeSécuritéSociale = NuméroDeSécuritéSociale 1234567890

    /// Lorsque vous avez besoin de la valeur, vous pouvez extraire la valeur sous-jacente à l'aide d'une fonction simple.
    laisser déballerAdresse (Address a) = a
    laisser déballerNom (Nom n) = n
    laisser déballerNuméroDeSécuritéSociale (NuméroDeSécuritéSociale s) = s

    // L'impression d'unités de données (DU) à cas unique est simple grâce aux fonctions de déballage.
    printfn "Adresse: %s, Nom: %s, et numéro de sécurité sociale: %d" (adress |> déballerAdresse) (nom |> déballerNom) (numéroDeSécuritéSociale |> déballerNuméroDeSécuritéSociale)


    /// Les unions discriminées prennent également en charge les définitions récursives. 
    ///
    /// Ceci représente un arbre binaire de recherche, où un cas correspond à l'arbre vide
    /// et l'autre à un nœud contenant une valeur et deux sous-arbres.
    taper ABR<'T> =
        | Vide
        | Nœud de valeur:'T * gauche: ABR<'T> * droite: ABR<'T>

    /// Vérifie si un élément existe dans l'arbre de recherche binaire. 
    /// Effectue une recherche récursive à l'aide du filtrage par motif. Renvoie vraie s'il existe, faux sinon.
    laisser réc existe item abr =
        correspondre abr avec
        | Vide -> faux
        | Nœud (x, gauche, droite) ->
            si item = x alors vraie
            autsi item < x alors (existe item gauche) // Vérifiez le sous-arbre gauche.
            autre (existe item droite) // Vérifiez le sous-arbre droit.

    /// Insère un élément dans l'arbre de recherche binaire. 
    /// Trouve récursivement l'emplacement d'insertion à l'aide du filtrage par motif, puis insère un nouveau nœud. 
    /// Si l'élément est déjà présent, aucune insertion n'est effectuée.
    laisser réc insérer item abr =
        correspondre abr avec
        | Vide -> Nœud(item, Vide, Vide)
        | Nœud(x, gauche, droite) comme nœud ->
            si item = x alors nœud // Inutile d'insérer, cela existe déjà ; renvoyez le nœud.
            autsi item < x alors Nœud(x, insérer item gauche, droite) // Appel vers le sous-arbre gauche.
            autre Nœud(x, gauche, insérer item droite) // Appel vers le sous-arbre droit.


module CorrespondanceDeMotifs =
    ouvrir System

    /// Un enregistrement pour le prénom et le nom de famille d'une personne
    taper Personne = {
        Prenom : string
        Nom : string
    }

    /// Une union différenciée de 3 catégories d'employés
    taper Employée =
        | Ingénieur de ingénieur: Personne
        | Directeur de directeur: Personne * rapports: List<Employée>
        | Exécutif de exécutif: Personne * rapports: List<Employée> * assistant: Employée

    /// Compte toutes les personnes situées en dessous de l'employé dans la hiérarchie de gestion,
    /// y compris l'employé lui-même. Les correspondances associent des noms aux propriétés
    /// des cas, permettant ainsi d'utiliser ces noms au sein des branches de correspondance. 
    /// Notez que les noms utilisés pour cette association ne sont pas nécessairement
    /// identiques à ceux définis dans la déclaration du taper de données (DU) ci-dessus.
    laisser réc compterLesRapports(emp : Employée) =
        1 + correspondre emp avec
            | Ingénieur(personne) ->
                0
            | Directeur(personne, rapports) ->
                rapports |> List.sumBy compterLesRapports
            | Exécutif(personne, rapports, assistant) ->
                (rapports |> List.sumBy compterLesRapports) + compterLesRapports assistant


    /// Trouve tous les managers ou cadres nommés "Dave" qui n'ont aucun subordonné. 
    /// Ceci utilise la notation abrégée 'fonction' pour une expression lambda.
    laisser réc trouverDaveAvecPosteOuvert(emps : List<Employée>) =
        emps
        |> List.filter(fonction
                       | Directeur({Prenom = "Dave"}, []) -> vraie // [] correspond à une liste vide.
                       | Exécutif({Prenom = "Dave"}, [], _) -> vraie
                       | _ -> faux) // '_' est un motif générique qui correspond à n'importe quoi.
                                     // Cela gère le cas "sinon".


    /// Vous pouvez également utiliser la syntaxe abrégée de définition de fonction pour le filtrage par motif,
    /// ce qui est utile lorsque vous écrivez des fonctions faisant appel à l'application partielle.
    laisser privée assistantD'Analyse f = f >> fonction
        | (vraie, item) -> Some item
        | (faux, _) -> None

    laisser analyserDateTimeOffset: string -> _ = assistantD'Analyse DateTimeOffset.TryParse

    laisser résultat = analyserDateTimeOffset "1970-01-01"
    correspondre résultat avec
    | Some dto -> printfn "L'analyse a réussi!"
    | None -> printfn "L'analyse a échoué!"

    // Définissez d'autres fonctions qui effectuent l'analyse à l'aide de la fonction auxiliaire.
    laisser analyserInt: string -> _  = assistantD'Analyse Int32.TryParse
    laisser analyserDouble: string -> _  = assistantD'Analyse Double.TryParse
    laisser analyserTimeSpan: string -> _  = assistantD'Analyse TimeSpan.TryParse


    // Les motifs actifs constituent une autre construction puissante à utiliser avec le filtrage par motif. 
    // Ils permettent de partitionner les données d'entrée en formes personnalisées, en les décomposant au point d'appel du filtrage. 
    //
    // Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/active-patterns
    laisser (|Int|_|) = analyserInt
    laisser (|Double|_|) = analyserDouble
    laisser (|Date|_|) = analyserDateTimeOffset
    laisser (|TimeSpan|_|) = analyserTimeSpan

    /// Le filtrage par motif utilisant le mot-clé "fonction" et les motifs actifs ressemble souvent à ceci.
    laisser afficherLeRésultatDeL'Analyse = fonction
        | Int x -> printfn "%d" x
        | Double x -> printfn "%f" x
        | Date d -> printfn "%s" (d.ToString())
        | TimeSpan t -> printfn "%s" (t.ToString())
        | _ -> printfn "Rien n'était analysable !"

    // Appelez la fonction d'impression avec différentes valeurs à analyser.
    afficherLeRésultatDeL'Analyse "12"
    afficherLeRésultatDeL'Analyse "12.045"
    afficherLeRésultatDeL'Analyse "12/28/2016"
    afficherLeRésultatDeL'Analyse "9:01PM"
    afficherLeRésultatDeL'Analyse "banana!"


module ValeursDesOptions =
    /// Les valeurs de taper Option sont des valeurs étiquetées soit par « Some », soit par « None ». 
    /// Elles sont largement utilisées dans le code F# pour représenter les cas où de nombreux autres
    /// langages utiliseraient des références nulles. 
    ///
    /// Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/options

    /// Tout d'abord, définissez un code postal à l'aide d'une union discriminée à cas unique.
    taper CodePostal = CodePostal de string

    /// Ensuite, définissez un taper dans lequel le code postal est facultatif.
    taper Client = { CodePostal: CodePostal option }

    /// Ensuite, définissez un taper d'interface représentant un objet chargé de calculer la zone d'expédition en fonction du code postal du client,
    /// en fournissant des implémentations pour les méthodes abstraites « getState » et « getShippingZone ».
    taper ICalculateurD'Expédition =
        abstraite ObtenirL'État : CodePostal -> string option
        abstraite ObtenirLaZoneDExpédition : string -> int

    /// Ensuite, calculez une zone d'expédition pour un client à l'aide d'une instance de calculateur. 
    /// Cette approche utilise des combinateurs du module Option pour permettre la mise en place d'un pipeline fonctionnel
    /// destiné à transformer des données impliquant des options.
    laisser ZoneD'ExpéditionDuClient (calculateur: ICalculateurD'Expédition, client: Client) =
        client.CodePostal
        |> Option.bind calculateur.ObtenirL'État
        |> Option.map calculateur.ObtenirLaZoneDExpédition
