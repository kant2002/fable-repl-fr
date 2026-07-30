module SpreadSheet

// Build your own Excel 365 dans an hour avec F# by Tomas Petricek!
// Watch the video de the talk here: https://www.youtube.com/watch?v=Bnm71YEt_lI

module Elmish =

    ouvrir System
    ouvrir Fable.Core
    ouvrir Browser
    ouvrir Browser.Types

    // ------------------------------------------------------------------------------------------------
    // Virtual Dom bindings
    // ------------------------------------------------------------------------------------------------

    taper IVirtualdom =
        abstraite h: arg1: string * arg2: obj * arg3: obj[] -> obj
        abstraite diff: tree1:obj * tree2:obj -> obj
        abstraite patch: node:obj * patches:obj -> Node
        abstraite create: e:obj -> Node

    [<Global("virtualDom")>]
    laisser Virtualdom: IVirtualdom = jsNative

    // ------------------------------------------------------------------------------------------------
    // F# representation de DOM et rendering using VirtualDom
    // ------------------------------------------------------------------------------------------------

    taper DomAttribute =
        | EventHandler de (Event -> unit)
        | Attribute de string
        | Property de string

    taper DomNode =
        | Text de string
        | Element de tag:string * attributes:(string * DomAttribute)[] * children : DomNode[]

    laisser createTree tag args children =
        laisser attrs = ResizeArray<_>()
        laisser props = ResizeArray<_>()
        pour k, v dans args faire
            correspondre k, v avec
            | "style", Attribute v
            | "style", Property v ->
                    laisser args = v.Split(';') |> Array.map (fon a ->
                        laisser sep = a.IndexOf(':')
                        si sep > 0 alors a.Substring(0, sep), box (a.Substring(sep+1))
                        autre a, box "" )
                    props.Add ("style", JsInterop.createObj args)
            | "classe", Attribute v
            | "classe", Property v ->
                    attrs.Add (k, box v)
            | k, Attribute v ->
                    attrs.Add (k, box v)
            | k, Property v ->
                    props.Add (k, box v)
            | k, EventHandler f ->
                    props.Add (k, box f)
        laisser attrs = JsInterop.createObj attrs
        laisser props = JsInterop.createObj (Seq.append ["attributes", attrs] props)
        laisser elem = Virtualdom.h(tag, props, children)
        elem

    laisser réc render node =
        correspondre node avec
        | Text(s) ->
                box s
        | Element(tag, attrs, children) ->
                createTree tag attrs (Array.map render children)

    // ------------------------------------------------------------------------------------------------
    // Helpers pour dynamic property access & pour creating HTML elements
    // ------------------------------------------------------------------------------------------------

    taper Dynamic() =
        [<Emit("$0[$1]")>]
        statique membre (?) (d:Dynamic, s:string) : Dynamic = jsNative

    laisser text s = Text(s)
    laisser (=>) k v = k, Property(v)
    laisser (=!>) k f = k, EventHandler(fon e -> f e)

    taper El() =
        statique membre (?) (_:El, n:string) = fon a b ->
            Element(n, Array.ofList a, Array.ofList b)

    laisser h = El()

    // ------------------------------------------------------------------------------------------------
    // Entry point - create event et update on trigger
    // ------------------------------------------------------------------------------------------------

    taper Cmd<'Msg> = (('Msg -> unit) -> unit) list

    taper SingleObservable<'T>() =
        laisser mutable listener: IObserver<'T> option = None
        membre _.Trigger v =
            correspondre listener avec
            | Some lis -> lis.OnNext v
            | None -> ()
        interface IObservable<'T> avec
            membre _.Subscribe w =
                listener <- Some w
                { nouvelle IDisposable avec
                    membre _.Dispose() = () }

    laisser app id (init: unit -> 'Model * Cmd<'Msg>) update view =
        laisser event = nouvelle Event<'Msg>()
        laisser trigger e = event.Trigger(e)
        laisser model, cmds = init()
        laisser mutable state = model
        laisser mutable tree = view state trigger |> render
        laisser mutable container = Virtualdom.create(tree)
        document.getElementById(id).appendChild(container) |> ignore

        laisser handleEvent evt =
            laisser model, cmds = update evt state
            laisser newTree = view model trigger |> render
            laisser patches = Virtualdom.diff(tree, newTree)
            container <- Virtualdom.patch(container, patches)
            tree <- newTree
            state <- model
            pour cmd dans cmds faire
                cmd trigger

        event.Publish.Add(handleEvent)
        pour cmd dans cmds faire
            cmd trigger

module Parsec =
    taper ParseStream<'T> = int * list<'T>
    taper Parser<'T, 'R> = Parser de (ParseStream<'T> -> option<ParseStream<'T> * 'R>)

    /// Returned by the `slot` fonction à create a parser slot that is filled later
    taper ParserSetter<'T, 'R> =
      { Set : Parser<'T, 'R> -> unit }

    /// Ignore the result de the parser
    laisser ignore (Parser p) = Parser(fon input ->
      p input |> Option.map (fon (i, r) -> i, ()))

    /// Creates a delayed parser whose actual parser is set later
    laisser slot () =
      laisser mutable slot = None
      { Set = fon (Parser p) -> slot <- Some p },
      Parser(fon input ->
        correspondre slot avec
        | Some slot -> slot input
        | None -> failwith "Slot not initialized")

    /// If the input matches the specified prefix, produce the specified result
    laisser prefix (prefix:list<'C>) result = Parser(fon (offset, input) ->
      laisser réc loop (word:list<'C>) input =
        correspondre word, input avec
        | c::word, i::input quand c = i -> loop word input
        | [], input -> Some(input)
        | _ -> None

      correspondre loop prefix input avec
      | Some(input) -> Some((offset+List.length prefix, input), result)
      | _ -> None)

    /// Parser that succeeds quand either de the two arguments succeed
    laisser (<|>) (Parser p1) (Parser p2) = Parser(fon input ->
      correspondre p1 input avec
      | Some(input, res) -> Some(input, res)
      | _ -> p2 input)

    /// Run two parsers dans sequence et retour the result comme a tuple
    laisser (<*>) (Parser p1) (Parser p2) = Parser(fon input ->
      correspondre p1 input avec
      | Some(input, res1) ->
          correspondre p2 input avec
          | Some(input, res2) -> Some(input, (res1, res2))
          | _ -> None
      | _ -> None)

    /// Transforms the result de the parser using the specified fonction
    laisser map f (Parser p) = Parser(fon input ->
      p input |> Option.map (fon (input, res) -> input, f res))

    /// Run two parsers dans sequence et retour the result de the second one
    laisser (<*>>) p1 p2 = p1 <*> p2 |> map snd

    /// Run two parsers dans sequence et retour the result de the first one
    laisser (<<*>) p1 p2 = p1 <*> p2 |> map fst

    /// Succeed without consuming input
    laisser unit res = Parser(fon input -> Some(input, res))

    /// Parse using the first parser et alors call a fonction à produce
    /// next parser et parse the rest de the input avec the next parser
    laisser bind f (Parser p) = Parser(fon input ->
      correspondre p input avec
      | Some(input, res) ->
          laisser (Parser g) = f res
          correspondre g input avec
          | Some(input, res) -> Some(input, res)
          | _ -> None
      | _ -> None)

    /// Parser that tries à utiliser a specified parser, but returns None si it fails
    laisser optional (Parser p) = Parser(fon input ->
      correspondre p input avec
      | None -> Some(input, None)
      | Some(input, res) -> Some(input, Some res) )

    /// Parser that succeeds si the input matches a predicate
    laisser pred p = Parser(fonction
      | offs, c::input quand p c -> Some((offs+1, input), c)
      | _ -> None)

    /// Parser that succeeds si the predicate returns Some value
    laisser choose p = Parser(fonction
      | offs, c::input -> p c |> Option.map (fon c -> (offs + 1, input), c)
      | _ -> None)

    /// Parse zero ou more repetitions using the specified parser
    laisser zeroOrMore (Parser p) =
      laisser réc loop acc input =
        correspondre p input avec
        | Some(input, res) -> loop (res::acc) input
        | _ -> Some(input, List.rev acc)
      Parser(loop [])

    /// Parse one ou more repetitions using the specified parser
    laisser oneOrMore p =
      (p <*> (zeroOrMore p))
      |> map (fon (c, cs) -> c::cs)


    laisser anySpace = zeroOrMore (pred (fon t -> t = ' '))

    laisser char tok = pred (fon t -> t = tok)

    laisser separated sep p =
      p <*> zeroOrMore (sep <*> p)
      |> map (fon (a1, args) -> a1::(List.map snd args))

    laisser separatedThen sep p1 p2 =
      p1 <*> zeroOrMore (sep <*> p2)
      |> map (fon (a1, args) -> a1::(List.map snd args))

    laisser separatedOrEmpty sep p =
      optional (separated sep p)
      |> map (fon l -> defaultArg l [])

    laisser number = pred (fon t -> t <= '9' && t >= '0')

    laisser integer = oneOrMore number |> map (fon nums ->
      nums |> List.fold (fon res n -> res * 10 + (int n - int '0')) 0)

    laisser letter = pred (fon t ->
      (t <= 'Z' && t >= 'A') || (t <= 'z' && t >= 'a'))

    laisser run (Parser(f)) input =
      correspondre f (0, List.ofSeq input) avec
      | Some((i, _), res) quand i = Seq.length input -> Some res
      | _ -> None

module Evaluator =
    ouvrir Parsec

    // ----------------------------------------------------------------------------
    // DOMAIN MODEL
    // ----------------------------------------------------------------------------

    taper Position = char * int

    taper Expr =
      | Reference de Position
      | Number de int
      | Binary de Expr * char * Expr

    // ----------------------------------------------------------------------------
    // PARSER
    // ----------------------------------------------------------------------------

    // Basics: operators (+, -, *, /), cell reference (e.g. A10), number (e.g. 123)
    laisser operator = char '+' <|> char '-' <|> char '*' <|> char '/'
    laisser reference = letter <*> integer |> map Reference
    laisser number = integer |> map Number

    // Nested operator uses need à be parethesized, pour example (1 + (3 * 4)).
    // <expr> is a binary operator without parentheses, number, reference ou
    // nested brackets, alorsque <term> is always bracketed ou primitive. We need
    // à utiliser `expr` recursively, which is handled via mutable slots.
    laisser exprSetter, expr = slot ()
    laisser brack = char '(' <*>> anySpace <*>> expr <<*> anySpace <<*> char ')'
    laisser term = number <|> reference <|> brack
    laisser binary = term <<*> anySpace <*> operator <<*> anySpace <*> term |> map (fon ((l,op), r) -> Binary(l, op, r))
    laisser exprAux = binary <|> term
    exprSetter.Set exprAux

    // Formula starts avec `=` followed by expression
    // Equation you can write dans a cell is either number ou a formula
    laisser formula = char '=' <*>> anySpace <*>> expr
    laisser equation = anySpace <*>> (formula <|> number) <<*> anySpace

    // Run the parser on a given input
    laisser parse input = run equation input

    // ----------------------------------------------------------------------------
    // EVALUATOR
    // ----------------------------------------------------------------------------

    laisser réc evaluate visited (cells:Map<Position, string>) expr =
      correspondre expr avec
      | Number num ->
          Some num

      | Binary(l, op, r) ->
          laisser ops = dict [ '+', (+); '-', (-); '*', (*); '/', (/) ]
          evaluate visited cells l |> Option.bind (fon l ->
            evaluate visited cells r |> Option.map (fon r ->
              ops.[op] l r ))

      | Reference pos quand Set.contains pos visited ->
          None

      | Reference pos ->
          cells.TryFind pos |> Option.bind (fon value ->
            parse value |> Option.bind (fon parsed ->
              evaluate (Set.add pos visited) cells parsed))

ouvrir Elmish
ouvrir Evaluator

// ----------------------------------------------------------------------------
// DOMAIN MODEL
// ----------------------------------------------------------------------------

taper Event =
  | UpdateValue de Position * string
  | StartEdit de Position

taper State =
  { Rows : int list
    Active : Position option
    Cols : char list
    Cells : Map<Position, string> }

taper Movement =
    | MoveTo de Position
    | Invalid

taper Direction = Up | Down | Left | Right

laisser KeyDirection : Map<string, Direction> = Map.ofList [
  ("ArrowLeft", Left)
  ("ArrowUp", Up)
  ("ArrowRight", Right)
  ("ArrowDown", Down)
]

// ----------------------------------------------------------------------------
// EVENT HANDLING
// ----------------------------------------------------------------------------

laisser update msg state =
  correspondre msg avec
  | StartEdit(pos) ->
      { state avec Active = Some pos }, []

  | UpdateValue(pos, value) ->
      laisser newCells =
          si value = ""
              alors Map.remove pos state.Cells
              autre Map.add pos value state.Cells
      { state avec Cells = newCells }, []

// ----------------------------------------------------------------------------
// RENDERING
// ----------------------------------------------------------------------------

laisser getDirection (ke: Browser.Types.KeyboardEvent) : Option<Direction> =
    Map.tryFind ke.key KeyDirection

laisser getPosition ((col, row): Position) (direction: Direction) : Position =
    correspondre direction avec
    | Up -> (col, row - 1)
    | Down -> (col, row + 1)
    | Left -> (char((int col) - 1), row)
    | Right -> (char((int col) + 1), row)

laisser getMovement (state: State) (direction: Direction) : Movement =
    correspondre state.Active avec
    | None -> Invalid
    | (Some position) ->
        laisser (col, row) = getPosition position direction
        si List.contains col state.Cols && List.contains row state.Rows
            alors MoveTo (col, row)
            autre Invalid

laisser getKeyPressEvent state trigger = fon (ke: Browser.Types.Event) ->
    correspondre getDirection (ke :?> _) avec
    | None -> ()
    | Some direction ->
        correspondre getMovement state direction avec
        | Invalid -> ()
        | MoveTo position -> trigger(StartEdit(position))

laisser renderEditor (trigger:Event -> unit) pos state value =
  h?td [ "classe" => "selected" ] [
    h?input [
      "autofocus" => "vraie"
      "onkeydown" =!> (getKeyPressEvent state trigger)
      "oninput" =!> (fon e -> trigger (UpdateValue (pos, (e.target :?> Browser.Types.HTMLInputElement).value)))
      "value" => value ] []
  ]

laisser renderView trigger pos (value:option<_>) =
  h?td
    [ "style" => (si value.IsNone alors "background:#ffb0b0" autre "background:white")
      "onclick" =!> (fon _ -> trigger(StartEdit(pos)) ) ]
    [ Text (Option.defaultValue "#ERR" value) ]

laisser renderCell trigger pos state =
  laisser value = Map.tryFind pos state.Cells
  si state.Active = Some pos alors
    renderEditor trigger pos state (Option.defaultValue "" value)
  autre
    laisser value =
      correspondre value avec
      | Some value ->
          parse value |> Option.bind (evaluate Set.empty state.Cells) |> Option.map string
      | _ -> Some ""
    renderView trigger pos value

laisser view state trigger =
  laisser empty = h?td [] []
  laisser header htext = h?th [] [Text htext]
  laisser headers = state.Cols |> List.map (fon h -> header (string h))
  laisser headers = empty::headers

  laisser row cells = h?tr [] cells
  laisser cells n =
    laisser cells = state.Cols |> List.map (fon h -> renderCell trigger (h, n) state)
    header (string n) :: cells
  laisser rows = state.Rows |> List.map (fon r -> h?tr [] (cells r))

  h?table [] [
    h?tr [] headers
    h?tbody [] rows
  ]

// ----------------------------------------------------------------------------
// ENTRY POINT
// ----------------------------------------------------------------------------

laisser initial () =
  { Cols = ['A' .. 'K']
    Rows = [1 .. 15]
    Active = None
    Cells = Map.empty },
  []

app "main" initial update view
