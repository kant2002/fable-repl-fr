// F# Ant Colony Fable Edition

// Ported from: https://github.com/robertpi/F--Ant-Colony/ which is a folk de: https://github.com/Rickasaurus/F--Ant-Colony

// Original notice:

//
// This is Richard Minerich's F# Ant Colony Silverlight Ediiton
// Visit my Blog at http://RichardMinerich.com
// This code is free à be used pour anything you like comme long comme I am properly acknowledged.
//
// The basic Silverlight used here is based on Phillip Trelford's Missile Command Example
// http://www.trelford.com/blog/post/MissileCommand.aspx
//

module Ants

ouvrir Fable.Core
ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

module Types =

    laisser xSize = 50
    laisser ySize = 50
    laisser nestSize = 5
    laisser maxTotalFoodPerSquare = 200
    laisser minGeneratedFoodPerSquare = 20
    laisser maxGeneratedFoodPerSquare = 100
    laisser maxFoodAntCanCarry = 5
    laisser chanceOfFood = 0.04

    laisser maxCellPheromoneQuantity = 255
    laisser maxAntDropPheromoneQunatity = 50
    laisser pheromoneDispersalRate = 1

    laisser percentFoodToWin = 0.5
    laisser maxWorldCycles = 1500

    taper UID = { X: int; Y: int }

    laisser uid (x, y) = { X = x; Y = y}

    taper AntColor =
        | Black
        | Red

    taper WorldCellType =
            | FieldCell
            | NestCell de AntColor

    taper Ant =
        { Color : AntColor
          FoodCarried : int }
        avec
            membre x.IsFullOfFood = x.FoodCarried >= maxFoodAntCanCarry
            membre x.HasFood = x.FoodCarried > 0
            membre x.MaxPheromonesToDrop = maxAntDropPheromoneQunatity

    et WorldCell =
        { Id : UID
          Food : int
          Ant : option<Ant>
          CellType : WorldCellType
          Pheromones : Map<AntColor, int> }
        avec
            membre t.IsFullOfFood = t.Food >= maxTotalFoodPerSquare
            membre t.HasFood = t.Food > 0
            membre t.ContainsAnt = t.Ant.IsSome
            membre t.HasPheromone color = not (t.Pheromones.[color] = 0)
            membre t.MaxPheromones = maxCellPheromoneQuantity
            membre t.MaxFood = maxTotalFoodPerSquare

    et TheWorld = Map<UID, WorldCell>

    et AntAction =
        | Nothing
        | Move de WorldCell
        | TakeFood de WorldCell
        | DropFood de WorldCell
        | DropPheromone de WorldCell * int

    taper Nest(ix, iy, sizex, sizey) =
        membre interne t.MinX = ix
        membre interne t.MinY = iy
        membre interne t.MaxX = ix + sizex
        membre interne t.MaxY = iy + sizey
        membre interne t.IsInBounds x y = x >= t.MinX && x <= t.MaxX && y >= t.MinY && y <= t.MaxY
        membre t.Distance cell =
                laisser cx, cy = t.MinX + ((t.MaxX - t.MinX) / 2), t.MinY + ((t.MaxY - t.MinY) / 2)
                laisser x, y = cell.Id.X, cell.Id.Y
                laisser pow x = x * x
                sqrt (pow(double cx - double x) + pow(double cy - double y))
        membre t.CountFood (world: TheWorld) =
                Map.fold (fon s (k: UID) v -> si t.IsInBounds k.X k.Y alors s + v.Food autre s) 0 world


    taper IAntBehavior =
        abstraite membre Name : string
        abstraite membre Behave : Ant -> WorldCell -> WorldCell list -> Nest -> AntAction

    taper WorldChange = TheWorld -> TheWorld

module Helpers =

    ouvrir System
    ouvrir System.Reflection

    module Array =
        laisser randomPermute a =
            laisser n = Array.length a
            si n > 0 alors
                laisser rand = nouvelle Random()
                laisser réc aux = fonction
                    | 0 -> a
                    | k ->
                        laisser i = rand.Next(k+1)
                        laisser tmp = a.[i]
                        a.[i] <- a.[k]
                        a.[k] <- tmp
                        aux (k-1)
                aux (n-1)
            autre a

    module Seq =
        laisser randomPermute a =
            a |> Seq.toArray |> Array.randomPermute |> Array.toSeq

    module List =

        laisser privée r = Random(int DateTime.Now.Ticks)
        laisser random l =
            laisser index = r.Next(0, List.length l) dans
                l.[index]

module World =

    ouvrir System

    ouvrir Types
    ouvrir Helpers

    laisser BlackAntNest = nouvelle Nest( 0, 0, nestSize - 1, nestSize - 1 )
    laisser RedAntNest = nouvelle Nest( 1 + xSize - nestSize, 1 + ySize - nestSize, nestSize - 1, nestSize - 1)

    laisser (|InBlackNest|InRedNest|Neither|) (x,y) =
        si BlackAntNest.IsInBounds x y alors InBlackNest
        autsi RedAntNest.IsInBounds x y alors InRedNest
        autre Neither

    laisser getAntNest ant =
        correspondre ant.Color avec
        | AntColor.Black -> BlackAntNest
        | AntColor.Red -> RedAntNest

    laisser emptyPheromoneSet =
        seq { laisser colors = [| AntColor.Black; AntColor.Red |]
              pour color dans colors faire
                rendement color, 0 }
        |> Map.ofSeq

    laisser defaultCell id = {Id = id; Food = 0; Ant = None; CellType = FieldCell; Pheromones = emptyPheromoneSet }
    laisser defaultBlackAnt = Some { Color = AntColor.Black; FoodCarried = 0 }
    laisser defaultRedAnt = Some { Color = AntColor.Red; FoodCarried = 0 }

    laisser buildWorldInitialWorld () =
        laisser rnd = nouvelle System.Random() dans
            seq { pour x dans 0 .. xSize faire
                    pour y dans 0 .. ySize faire
                        laisser uid = uid (x, y)
                        laisser defaultcell = defaultCell uid
                        correspondre x, y avec
                        | InBlackNest -> rendement uid, { defaultcell avec Ant = defaultBlackAnt; CellType = NestCell(AntColor.Black) }
                        | InRedNest ->   rendement uid, { defaultcell avec Ant = defaultRedAnt; CellType = NestCell(AntColor.Red) }
                        | Neither ->     si chanceOfFood > rnd.NextDouble()
                                            alors rendement uid, { defaultcell avec Food = rnd.Next(minGeneratedFoodPerSquare, maxGeneratedFoodPerSquare) }
                                            autre rendement uid, defaultcell
                }
            |> Map.ofSeq

    laisser getAntViews (world: TheWorld) =
        laisser getWorldCell x y = Map.tryFind (uid (x,y)) world
        laisser worldFold state (uid: UID) cell =
                laisser x, y = (uid.X, uid.Y)
                correspondre cell.Ant avec
                | None -> state
                | Some(ant) ->
                    laisser visibleCells = [ getWorldCell x (y - 1); getWorldCell x (y + 1); getWorldCell (x - 1) y; getWorldCell (x + 1) y ]
                                        |> List.choose id
                    state @ [ant, cell, visibleCells, getAntNest ant]
        Map.fold worldFold [] world

    laisser getAntActions (bBehave: IAntBehavior) (rBehave: IAntBehavior) (views: (Ant * WorldCell * WorldCell list * Nest) list) =
        laisser getAntBehavior ant =
            correspondre ant.Color avec
            | AntColor.Black -> bBehave
            | AntColor.Red -> rBehave
        laisser transformView (ant, cell, antView, nest) =
            laisser behavior = getAntBehavior ant dans
            cell, behavior.Behave ant cell antView nest
        List.map transformView views

    laisser buildTransaction (expectedCells: WorldCell list) actions =
        laisser predicate (world: TheWorld) =
            List.forall (fon (cell: WorldCell) -> (Map.find cell.Id world) = cell) expectedCells
        laisser action (iworld: TheWorld) =
            List.fold (fon (cworld: TheWorld) (id, action) -> Map.add id (action cworld.[id]) cworld) iworld actions
        predicate, action

    laisser getWorldChangeTransactions actions =
        seq { pour source, action dans actions faire
                laisser ant = Option.get source.Ant
                correspondre action avec
                | Nothing -> ()
                | Move (target) ->
                    si Option.isSome target.Ant alors ()
                    autre rendement buildTransaction
                                    [ source; target ]
                                    [ source.Id, (fon oldcell -> { oldcell avec Ant = None });
                                        target.Id, (fon oldtarget -> { oldtarget avec Ant = source.Ant }) ]
                | TakeFood (target) ->
                    si target.Food <= 0 alors ()
                    autre
                        laisser foodToGet = min (target.Food) (maxFoodAntCanCarry - ant.FoodCarried)
                        rendement buildTransaction
                                    [ source; target ]
                                    [ target.Id, (fon oldtarget -> { oldtarget avec Food = oldtarget.Food - foodToGet });
                                        source.Id, (fon oldcell -> { oldcell avec Ant = Some { ant avec FoodCarried = ant.FoodCarried + foodToGet } } ) ]
                | DropFood (target) ->
                    si target.Food >= maxTotalFoodPerSquare alors ()
                    autre
                        laisser foodToDrop = min (maxTotalFoodPerSquare - target.Food) (ant.FoodCarried)
                        laisser transaction =
                            buildTransaction
                                    [ source; target ]
                                    [ target.Id, (fon oldtarget -> { oldtarget avec Food = oldtarget.Food + foodToDrop });
                                        source.Id, (fon oldcell -> { source avec Ant = Some { ant avec FoodCarried = ant.FoodCarried - foodToDrop } }) ]
                        rendement transaction
                | DropPheromone (target, quantity) ->
                    laisser newValue = max (target.Pheromones.[ant.Color] + quantity) maxCellPheromoneQuantity
                    rendement buildTransaction
                                [ target ]
                                [ target.Id, (fon oldtarget -> { oldtarget avec Pheromones = oldtarget.Pheromones.Add(ant.Color, newValue ) } ) ] }

    laisser degradePheromones (world: TheWorld) =
        world
        |> Map.map (fon uid cell -> { cell avec Pheromones = cell.Pheromones |> Map.map (fon key quantity -> max (quantity - 1) 0) } )

    laisser applyWorldTransactions (oldWorld: TheWorld) changes =
        laisser foldAction (world: TheWorld) (pred, action) =
            si pred world
            alors action world
            autre world
        Seq.fold foldAction oldWorld changes

    laisser uid2xy (uid: UID) = uid.X, uid.Y

    laisser worldCycle bPlayer rPlayer world : TheWorld =
        world
        |> getAntViews
        |> getAntActions bPlayer rPlayer
        |> Seq.randomPermute
        |> getWorldChangeTransactions
        |> applyWorldTransactions world
        |> degradePheromones

module Canvas =

    // Get the canvas context pour drawing
    laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
    laisser context = canvas.getContext_2d()

    // Format RGB color comme "rgb(r,g,b)"
    laisser ($) s n = s + n.ToString()
    laisser rgb r g b = "rgb(" $ r $ "," $ g $ "," $ b $ ")"

    // Fill rectangle avec given color
    laisser filled (color: string) rect =
        laisser ctx = context
        ctx.fillStyle <- !^ color
        ctx.fillRect rect

    laisser drawBlob (color: string) size (x, y) =
        context.beginPath()
        context.arc(x, y, size, 0., 2. * System.Math.PI, faux )
        context.fillStyle <- !^ color
        context.fill()

    laisser getWindowDimensions () =
        canvas.width, canvas.height


    laisser image (src:string) =
        laisser image = document.getElementsByTagName("img").[0] :?> HTMLImageElement
        si image.src.IndexOf(src) = -1 alors image.src <- src
        image

    laisser updateInput name text =
        laisser image = document.getElementsByName(name).[0] :?> HTMLDivElement
        image.innerHTML <- text
        image


module Simulation =
    ouvrir Types
    ouvrir World
    ouvrir Canvas

    laisser drawAnt x y antColor =
        laisser color =
            correspondre antColor avec
            | AntColor.Black -> rgb 0 0 0
            | AntColor.Red -> rgb 255 0 0
        drawBlob color 4. (x, y)

    laisser drawFood food x y =
        laisser radius = ((float food / float maxTotalFoodPerSquare) * 3.) + 1.
        laisser color = rgb 0 255 0
        drawBlob color radius (x, y)

    laisser makeGradiant quantity max =
        laisser inverseGrediant = 1. - (float quantity / float max)
        laisser levelDiff = 200. - 111. // difference between the "full pheromone color et background"
        levelDiff * inverseGrediant
    laisser drawPheromone x y antColor amount =
        laisser opacityFudge = makeGradiant amount maxCellPheromoneQuantity
        laisser level = int opacityFudge + 111
        // console.log(sprintf "level: %d" level)
        laisser color =
            correspondre antColor avec
            | AntColor.Black -> rgb level level level
            | AntColor.Red -> rgb level opacityFudge level
        drawBlob color 4. (x, y)

    laisser drawUpdates (width, height) (world: TheWorld) =
        laisser updateCell uid cell =
            laisser wm, hm = width / float (xSize + 1), height / float (ySize + 1)
            laisser offset x y = (x + 0.5) * wm, (y + 0.5) * hm
            laisser x, y = uid2xy uid
            laisser ox, oy = offset (float x) (float y)
            cell.Pheromones |> Map.iter (fon color amount -> si amount > 0 alors drawPheromone ox oy color amount)
            si cell.Food > 0 alors drawFood cell.Food ox oy
            si cell.Ant.IsSome alors drawAnt ox oy cell.Ant.Value.Color
        world
        |> Map.iter updateCell


module HardishAI =

    ouvrir Helpers
    ouvrir Types

    laisser rnd = System.Random(int System.DateTime.Now.Ticks)

    taper TestAntBehavior() =
        interface IAntBehavior avec
            membre x.Name = "Rick's Hardish"
            membre x.Behave me here locations nest =

                laisser isMyHome node = node.CellType = WorldCellType.NestCell(me.Color)
                laisser locationsWithoutAnts = locations |> List.filter  (fon node -> node.Ant = None)

                laisser (|HasFood|HasMaxFood|HasNoFood|) (ant: Ant) =
                    si ant.FoodCarried = 0 alors HasNoFood
                    autsi ant.FoodCarried = maxFoodAntCanCarry alors HasMaxFood
                    autre HasFood

                laisser (|NearHome|_|) (locations: WorldCell list) =
                    laisser homeNodes = locations |> List.filter (fon node -> isMyHome node)
                    si List.isEmpty homeNodes alors None
                    autre Some homeNodes

                laisser (|AwayFromHome|NearHome|) (locations: WorldCell list) =
                    laisser homeLocations, awayLocations = locations |> List.partition (fon node -> isMyHome node)
                    si List.isEmpty homeLocations alors AwayFromHome awayLocations
                    autre NearHome homeLocations

                laisser (|CanDrop|CantDrop|) (locations: WorldCell list) =
                    laisser dropFoodLocations = locations |> List.filter (fon node -> not (node.IsFullOfFood))
                    si List.isEmpty dropFoodLocations alors CantDrop
                    autre CanDrop dropFoodLocations

                laisser (|HasUnownedFood|_|) (locations: WorldCell list) =
                    laisser foodLocations = locations |> List.filter (fon node -> node.HasFood && not (isMyHome node))
                    si List.isEmpty foodLocations alors None
                    autre Some foodLocations

                laisser (|HasPheromonesAndNoAnt|_|) (locations: WorldCell list) =
                    laisser pheromoneLocations = locations |> List.filter (fon node -> node.Ant = None) |> List.filter (fon node -> node.HasPheromone me.Color)
                    si List.isEmpty pheromoneLocations alors None
                    autre Some pheromoneLocations

                laisser (|HasNoAnt|_|) (locations: WorldCell list) =
                    laisser emptyLocations = locations |> List.filter (fon node -> node.Ant = None)
                    si List.length emptyLocations > 0 alors
                        Some (emptyLocations)
                    autre None

                laisser (|ShortestDistanceWithNoAnt|_|)  (locations: WorldCell list) =
                    laisser noAnts = locations |> List.filter (fon node -> node.Ant = None)
                    si List.length noAnts > 0 alors Some (noAnts |> List.minBy (fon node -> nest.Distance node))
                    autre None

                laisser maxFood = List.maxBy (fon node -> node.Food)
                laisser minPhero = List.minBy (fon node -> node.Pheromones.[me.Color])
                laisser noAnts = List.filter (fon node -> node.Ant = None)

                // [snippet:Simple Pheromone-Using Ant Colony AI]
                correspondre me avec
                | HasFood
                | HasMaxFood ->
                    correspondre locations avec
                    | NearHome homeCells ->
                        correspondre homeCells avec
                        | CanDrop dropCells -> DropFood dropCells.Head
                        | HasNoAnt noAntCells -> Move (List.random noAntCells)
                        | _ -> Nothing
                    | AwayFromHome allCells ->
                        correspondre here.Pheromones.[me.Color] avec
                        | n quand n < 20 -> DropPheromone (here, 100 - n)
                        | _ ->
                            correspondre allCells avec
                            | HasNoAnt noAnts quand rnd.Next(0, 3) = 0 -> Move (List.random noAnts)
                            | ShortestDistanceWithNoAnt node -> Move node
                            | _ -> Nothing
                | HasNoFood ->
                    correspondre locations avec
                    | HasNoAnt noAnts quand rnd.Next(0, 3) = 0 -> Move (List.random noAnts)
                    | HasUnownedFood foodCells -> TakeFood (maxFood foodCells)
                    | HasPheromonesAndNoAnt pheroCells -> Move (minPhero pheroCells)
                    | HasNoAnt noAntCells -> Move (List.random noAntCells)
                    | _ -> Nothing


module AntsEverywhereExmampleAI =
    ouvrir Types

    laisser randomGen = nouvelle System.Random()

    laisser getRandomVal min max =
        lock randomGen (fon () -> randomGen.Next(min, max))

    taper TestAntBehavior() =
        interface IAntBehavior avec
            membre x.Name = "Frank_Levine"
            membre x.Behave me here locations nest =

                // This Ant's basic strategy is this:
                // If you have food et are near the nest
                //      drop the food
                // If you can't carry anymore food (bur are not near the nest)
                //      head back à the nest avec the following exception
                //          si the current cell (here) has <40 phereomones, replenish the supply back à 100
                // If you're not dropping off food ou heading home, you're foraging
                //      The logic pour foraging is:
                //      If you see food, take it (this applies even quand you have food but aren't full)
                //      If you see pheromones, move à the pheromone that is farthest from the nest
                //          si all pheromones are closer à the nest than you, alors make a random move
                //      Otherwise you'e dans the middle de nowhere, wanter randomly
                //
                // Special note on 'Traffic Control':  Inbound ants always rendement à outbound ants
                //                                     This seems reasonable since the inbound ants
                //                                     Know where they're going et the outbound ones
                //                                     Are dependent on the pheromone trail



                //
                // helper functions
                laisser isNest (cell: WorldCell) = cell.CellType = WorldCellType.NestCell(me.Color)

                // how faire I negate a fonction?!?  this seems a bit heavy-handed
                laisser isNotNest (cell: WorldCell) =
                    si isNest cell alors
                        faux
                    autre
                        vraie

                // nest cells that can receive food
                laisser nestCells = locations |> List.filter isNest
                                        |> List.filter (fon c -> c.IsFullOfFood = faux)

                // all empty neighbors, sorted so we can get at the closest et farthest ones from the nest
                // first = closest à nest
                // last = farthest from nest
                laisser emptyNeighbors = locations |> List.filter (fon c -> c.ContainsAnt = faux)
                                            |> List.sortBy (fon c -> nest.Distance(c))

                // all empty neighbors avec my pheromones
                laisser emptyNeighborsWithP = emptyNeighbors |> List.filter( fon c -> c.HasPheromone(me.Color))
                                                        |> List.sortBy( fon c -> nest.Distance(c))
                                                        |> List.toArray

                // all neighbors avec food, ordered by the amount de food decending
                laisser neighborsWithFood = locations |> List.filter (isNotNest)
                                                |> List.filter (fon c -> c.HasFood)
                                                |> List.sortBy (fon c -> c.Food)
                                                |> List.rev

                // functions à make the code below more readable
                // NullMove does nothing (like quand you're boxed dans)
                // RandomMove is... Random
                laisser NullMove = fon() -> Move here

                laisser RandomMove = fon () ->
                    laisser i = getRandomVal 0 emptyNeighbors.Length
                    Move (List.item i emptyNeighbors)


                // maximum amount de pheromone à leave on a cell
                laisser MAX_PHERO = 100;

                // quand returning à the nest, add more pheromones quand the cell
                // has less than this number
                laisser REFRESH_THRESHOLD = 50;



                // active pattern à determine the ant's high-level state
                laisser (|ShouldDropFood|Forage|ReturnToNest|) (ant: Ant) =
                    laisser haveAvailableNestCells = (nestCells.IsEmpty = faux)
                    correspondre ant avec
                        | a quand a.HasFood && haveAvailableNestCells -> ShouldDropFood
                        | a quand a.IsFullOfFood -> ReturnToNest
                        | _ -> Forage

                // active pattern à decide si we need à refresh pheromones
                laisser (|NeedsRefresh|NoRefresh|) (cell: WorldCell) =
                    correspondre cell.Pheromones.[me.Color] avec
                        | x quand x < REFRESH_THRESHOLD ->
                            laisser amt = MAX_PHERO - x     // amt is the number de pheromones required à bring this cell back à 100
                            NeedsRefresh amt
                        | _ -> NoRefresh    // there are enough pour now

                // gets the relative distance à the nest
                // relativeDist > 0 --> cell is farther from the nest than 'here'
                // relativeDist < 0 --> cell is closer à the nest than 'here'
                laisser relativeDist (cell: WorldCell) =
                    laisser dHere = nest.Distance(here)
                    laisser dCell = nest.Distance(cell)
                    dCell - dHere

                // fonction à get the last thing from an array
                laisser last (arr: 'a[]) =
                    arr.[arr.Length-1]

                // the ant parameter isn't used, but I don't know how à make a
                // parameterless active pattern
                laisser (|AdjacentToFood|AdjacentToPheromone|NoMansLand|) (ant: Ant) =
                    si neighborsWithFood.Length > 0 alors
                        AdjacentToFood
                    autsi emptyNeighborsWithP.Length > 0 && relativeDist (last emptyNeighborsWithP) > 0. alors
                        // remember emptyNeighborsWithP is sorted
                        AdjacentToPheromone (last emptyNeighborsWithP)
                    autre
                        NoMansLand

                // The Actual logic...

                si emptyNeighbors.IsEmpty alors
                    NullMove()
                autre
                    correspondre me avec
                    | ShouldDropFood -> DropFood nestCells.Head
                    | ReturnToNest ->
                        correspondre here avec
                        | NeedsRefresh amt -> DropPheromone (here, amt)
                        | NoRefresh -> Move emptyNeighbors.Head
                    | Forage ->
                        correspondre me avec
                        | AdjacentToFood -> TakeFood neighborsWithFood.Head
                        | AdjacentToPheromone pheroCell -> Move pheroCell
                        | NoMansLand -> RandomMove()

ouvrir Canvas
ouvrir Types
ouvrir World
ouvrir Simulation

laisser origin =
    // Sample is running dans an iframe, so get the location de parent
    laisser topLocation = window.top.location
    topLocation.origin + topLocation.pathname

laisser formatScoreCard bName bFood rName rFood =
    sprintf "Black (%s): %05d vs Red (%s): %05d" bName bFood rName rFood

laisser formatRemaining remaining =
    sprintf "Remaining Cycles: %05d" remaining


laisser maxCycles = 1000
laisser world = ref (buildWorldInitialWorld())
laisser foodToWin = int <| double (Map.fold (fon s k v -> s + v.Food) 0 world.Value) * percentFoodToWin
laisser cycles = ref 0

laisser blackAI = nouvelle HardishAI.TestAntBehavior() :> IAntBehavior
laisser redAI = nouvelle AntsEverywhereExmampleAI.TestAntBehavior() :> IAntBehavior

laisser render (w,h) =
    cycles.Value <- cycles.Value + 1

    laisser bScore = BlackAntNest.CountFood world.Value
    laisser rScore = RedAntNest.CountFood world.Value

    laisser remainig = maxCycles - cycles.Value

    laisser scoreString = formatScoreCard blackAI.Name bScore redAI.Name rScore
    updateInput "score" scoreString |> ignore

    laisser remainingString = formatRemaining remainig
    updateInput "secondline" remainingString |> ignore


    (0., 0., w, h) |> filled (rgb 200 200 200)
    drawUpdates (w,h) world.Value
    world.Value <- worldCycle blackAI redAI world.Value

    si bScore > foodToWin || rScore > foodToWin || cycles.Value > maxCycles alors
        si bScore > rScore alors Some blackAI.Name
        autsi rScore > bScore alors Some redAI.Name
        autre None
    autre None

laisser w, h = getWindowDimensions()

laisser réc update () =
    laisser result = render (w,h)
    correspondre result avec
    | None ->
        window.setTimeout(update, 1000 / 30) |> ignore
    | Some winner ->
        updateInput "secondline" (sprintf "The winner is: %s" winner) |> ignore

update ()
