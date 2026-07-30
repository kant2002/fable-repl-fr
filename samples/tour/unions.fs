module Visite.Unions

// Extrait de https://docs.microsoft.com/en-us/dotnet/fsharp/tour
// Consultez le lien ci-dessus pour obtenir plus d'informations sur chaque sujet.
// Vous trouverez également d'autres ressources d'apprentissage sur https://fsharp.org/

module UnionsDiscriminées =

    /// Ce qui suit représente la couleur d'une carte à jouer.
    type Suit =
        | Trèfles
        | Carreaux
        | Piques
        | Cœurs

    /// Une Union discriminée peut également être utilisée pour représenter la valeur d'une carte à jouer.
    type Rang =
        /// Représente la valeur des cartes de 2 à 10.
        | Valeur of int
        | As
        | Roi
        | Dame
        | Valet

        /// Les unions discriminées peuvent également implémenter des membres orientés objet.
        static member ObtenirTousLesRangs() =
            [ yield As
              for i in 2 .. 10 do yield Valeur i
              yield Valet
              yield Dame
              yield Roi ]

    /// Il s'agit d'un type enregistrement qui combine une couleur et une valeur. 
    /// Il est courant d'utiliser à la fois des enregistrements et des unions discriminées pour représenter des données.
    type Carte = { Suit: Suit; Rang: Rang }

    /// Cela calcule une liste représentant toutes les cartes du jeu.
    let deckComplet =
        [ for suit in [ Cœurs; Carreaux; Piques; Trèfles] do
              for rang in Rang.ObtenirTousLesRangs() do
                  yield { Suit=suit; Rang=rang } ]

    /// Cet exemple convertit un objet `Card` en chaîne de caractères.
    let afficherCarteÀJouer (c: Carte) =
        let chaîneDeRang =
            match c.Rang with
            | As -> "As"
            | Roi -> "Roi"
            | Dame -> "Dame"
            | Valet -> "Valet"
            | Valeur n -> string n
        let chaîneDeSuit =
            match c.Suit with
            | Trèfles -> "trèfles"
            | Carreaux -> "carreaux"
            | Piques -> "piques"
            | Cœurs -> "cœurs"
        chaîneDeRang  + " de " + chaîneDeSuit

    /// Cet exemple affiche toutes les cartes d'un jeu de cartes.
    let afficherToutesLesCartes() =
        for carte in deckComplet do
            printfn "%s" (afficherCarteÀJouer carte)


    // Les types union discriminée (DU) à un seul cas sont souvent utilisés pour la modélisation de domaine. Ils offrent une sécurité de typage accrue
    // par rapport aux types primitifs tels que les chaînes de caractères et les entiers. 
    //
    // Les types union discriminée à un seul cas ne peuvent pas être convertis implicitement vers ou depuis le type qu'ils encapsulent. 
    // Par exemple, une fonction attendant un paramètre de type 'Address' ne peut pas accepter une chaîne de caractères en entrée,
    // et inversement.
    type Address = Address of string
    type Nom = Nom of string
    type NuméroDeSécuritéSociale = NuméroDeSécuritéSociale of int

    // Vous pouvez facilement instancier une union discriminée à cas unique comme suit.
    let adress = Address "111 Alf Way"
    let nom = Nom "Alf"
    let numéroDeSécuritéSociale = NuméroDeSécuritéSociale 1234567890

    /// Lorsque vous avez besoin de la valeur, vous pouvez extraire la valeur sous-jacente à l'aide d'une fonction simple.
    let déballerAdresse (Address a) = a
    let déballerNom (Nom n) = n
    let déballerNuméroDeSécuritéSociale (NuméroDeSécuritéSociale s) = s

    // L'impression d'unités de données (DU) à cas unique est simple grâce aux fonctions de déballage.
    printfn "Adresse: %s, Nom: %s, et numéro de sécurité sociale: %d" (adress |> déballerAdresse) (nom |> déballerNom) (numéroDeSécuritéSociale |> déballerNuméroDeSécuritéSociale)


    /// Les unions discriminées prennent également en charge les définitions récursives. 
    ///
    /// Ceci représente un arbre binaire de recherche, où un cas correspond à l'arbre vide
    /// et l'autre à un nœud contenant une valeur et deux sous-arbres.
    type ABR<'T> =
        | Vide
        | Nœud of valeur:'T * gauche: ABR<'T> * droite: ABR<'T>

    /// Vérifie si un élément existe dans l'arbre de recherche binaire. 
    /// Effectue une recherche récursive à l'aide du filtrage par motif. Renvoie true s'il existe, false sinon.
    let rec existe item abr =
        match abr with
        | Vide -> false
        | Nœud (x, gauche, droite) ->
            if item = x then true
            elif item < x then (existe item gauche) // Vérifiez le sous-arbre gauche.
            else (existe item droite) // Vérifiez le sous-arbre droit.

    /// Insère un élément dans l'arbre de recherche binaire. 
    /// Trouve récursivement l'emplacement d'insertion à l'aide du filtrage par motif, puis insère un nouveau nœud. 
    /// Si l'élément est déjà présent, aucune insertion n'est effectuée.
    let rec insérer item abr =
        match abr with
        | Vide -> Nœud(item, Vide, Vide)
        | Nœud(x, gauche, droite) as nœud ->
            if item = x then nœud // Inutile d'insérer, cela existe déjà ; renvoyez le nœud.
            elif item < x then Nœud(x, insérer item gauche, droite) // Appel vers le sous-arbre gauche.
            else Nœud(x, gauche, insérer item droite) // Appel vers le sous-arbre droit.


module CorrespondanceDeMotifs =
    open System

    /// Un enregistrement pour le prénom et le nom de famille d'une personne
    type Personne = {
        Prenom : string
        Nom : string
    }

    /// Une union différenciée de 3 catégories d'employés
    type Employée =
        | Ingénieur of ingénieur: Personne
        | Directeur of directeur: Personne * rapports: List<Employée>
        | Exécutif of exécutif: Personne * rapports: List<Employée> * assistant: Employée

    /// Compte toutes les personnes situées en dessous de l'employé dans la hiérarchie de gestion,
    /// y compris l'employé lui-même. Les correspondances associent des noms aux propriétés
    /// des cas, permettant ainsi d'utiliser ces noms au sein des branches de correspondance. 
    /// Notez que les noms utilisés pour cette association ne sont pas nécessairement
    /// identiques à ceux définis dans la déclaration du type de données (DU) ci-dessus.
    let rec compterLesRapports(emp : Employée) =
        1 + match emp with
            | Ingénieur(personne) ->
                0
            | Directeur(personne, rapports) ->
                rapports |> List.sumBy compterLesRapports
            | Exécutif(personne, rapports, assistant) ->
                (rapports |> List.sumBy compterLesRapports) + compterLesRapports assistant


    /// Trouve tous les managers ou cadres nommés "Dave" qui n'ont aucun subordonné. 
    /// Ceci utilise la notation abrégée 'function' pour une expression lambda.
    let rec trouverDaveAvecPosteOuvert(emps : List<Employée>) =
        emps
        |> List.filter(function
                       | Directeur({Prenom = "Dave"}, []) -> true // [] correspond à une liste vide.
                       | Exécutif({Prenom = "Dave"}, [], _) -> true
                       | _ -> false) // '_' est un motif générique qui correspond à n'importe quoi.
                                     // Cela gère le cas "sinon".


    /// Vous pouvez également utiliser la syntaxe abrégée de définition de fonction pour le filtrage par motif,
    /// ce qui est utile lorsque vous écrivez des fonctions faisant appel à l'application partielle.
    let private assistantD'Analyse f = f >> function
        | (true, item) -> Some item
        | (false, _) -> None

    let analyserDateTimeOffset: string -> _ = assistantD'Analyse DateTimeOffset.TryParse

    let résultat = analyserDateTimeOffset "1970-01-01"
    match résultat with
    | Some dto -> printfn "L'analyse a réussi!"
    | None -> printfn "L'analyse a échoué!"

    // Définissez d'autres fonctions qui effectuent l'analyse à l'aide de la fonction auxiliaire.
    let analyserInt: string -> _  = assistantD'Analyse Int32.TryParse
    let analyserDouble: string -> _  = assistantD'Analyse Double.TryParse
    let analyserTimeSpan: string -> _  = assistantD'Analyse TimeSpan.TryParse


    // Les motifs actifs constituent une autre construction puissante à utiliser avec le filtrage par motif. 
    // Ils permettent de partitionner les données d'entrée en formes personnalisées, en les décomposant au point d'appel du filtrage. 
    //
    // Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/active-patterns
    let (|Int|_|) = analyserInt
    let (|Double|_|) = analyserDouble
    let (|Date|_|) = analyserDateTimeOffset
    let (|TimeSpan|_|) = analyserTimeSpan

    /// Le filtrage par motif utilisant le mot-clé "function" et les motifs actifs ressemble souvent à ceci.
    let afficherLeRésultatDeL'Analyse = function
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
    /// Les valeurs de type Option sont des valeurs étiquetées soit par « Some », soit par « None ». 
    /// Elles sont largement utilisées dans le code F# pour représenter les cas où de nombreux autres
    /// langages utiliseraient des références nulles. 
    ///
    /// Pour en savoir plus, consultez : https://docs.microsoft.com/dotnet/fsharp/language-reference/options

    /// Tout d'abord, définissez un code postal à l'aide d'une union discriminée à cas unique.
    type CodePostal = CodePostal of string

    /// Ensuite, définissez un type dans lequel le code postal est facultatif.
    type Client = { CodePostal: CodePostal option }

    /// Ensuite, définissez un type d'interface représentant un objet chargé de calculer la zone d'expédition en fonction du code postal du client,
    /// en fournissant des implémentations pour les méthodes abstraites « getState » et « getShippingZone ».
    type ICalculateurD'Expédition =
        abstract ObtenirL'État : CodePostal -> string option
        abstract ObtenirLaZoneDExpédition : string -> int

    /// Ensuite, calculez une zone d'expédition pour un client à l'aide d'une instance de calculateur. 
    /// Cette approche utilise des combinateurs du module Option pour permettre la mise en place d'un pipeline fonctionnel
    /// destiné à transformer des données impliquant des options.
    let ZoneD'ExpéditionDuClient (calculateur: ICalculateurD'Expédition, client: Client) =
        client.CodePostal
        |> Option.bind calculateur.ObtenirL'État
        |> Option.map calculateur.ObtenirLaZoneDExpédition
