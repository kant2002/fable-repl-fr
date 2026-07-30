// Fractal playground by Mark Pattison (Twitter @mark_pattison)
// Source code available dans Github: https://github.com/markpattison/FableFractal

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

module WebGLHelper =

  ouvrir Browser.Types
  ouvrir Fable.Core.JsInterop

  // Shorthand
  taper GL = WebGLRenderingContext

  laisser getWebGLContext (canvas: HTMLCanvasElement) =
      laisser getContext ctxString =
          canvas.getContext(ctxString, createObj [ "premultipliedAlpha" ==> faux ]) |> unbox<WebGLRenderingContext>

      laisser webgl = getContext "webgl"

      // If we have webgl = nulle dans JS alors essayer à get experimental-webgl
      // Edge et webkit utiliser experimental-webgl
      si not (unbox webgl) alors
          getContext "experimental-webgl"
      autre
          webgl

  laisser createShaderProgram (gl:GL) vertex fragment =
      laisser vertexShader = gl.createShader(gl.VERTEX_SHADER)
      gl.shaderSource(vertexShader, vertex)
      gl.compileShader(vertexShader)

      laisser fragShader = gl.createShader(gl.FRAGMENT_SHADER)
      gl.shaderSource(fragShader, fragment)
      gl.compileShader(fragShader)

      laisser program = gl.createProgram()
      gl.attachShader(program, vertexShader)
      gl.attachShader(program, fragShader)
      gl.linkProgram(program)

      program

  laisser createUniformLocation (gl:GL) program name =
      laisser uniformLocation = gl.getUniformLocation(program, name)
      uniformLocation

  laisser createAttributeLocation (gl : GL) program name =
      laisser attributeLocation = gl.getAttribLocation(program, name)
      gl.enableVertexAttribArray(attributeLocation)

      attributeLocation

  laisser createBuffer (items : float[]) (gl:GL) =
      laisser buffer = gl.createBuffer()

      gl.bindBuffer(gl.ARRAY_BUFFER, buffer)
      gl.bufferData(gl.ARRAY_BUFFER, (createNew Fable.Core.JS.Constructors.Float32Array items) |> unbox, gl.STATIC_DRAW)

      buffer

  laisser clear (gl:GL) (width, height) =
      gl.clearColor(1.0, 1.0, 1.0, 1.0)

      gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA)
      //gl.enable(gl.DEPTH_TEST)
      gl.enable(gl.BLEND)

      gl.viewport(0., 0., width, height)
      gl.clear(float (int gl.COLOR_BUFFER_BIT ||| int gl.DEPTH_BUFFER_BIT))

module Types =

  ouvrir Browser.Types

  taper Msg =
      | MandelbrotClick
      | JuliaClick
      | JuliaMoveClick
      | JuliaChangeSeedClick
      | MouseDownMsg de MouseEvent
      | MouseUpMsg de MouseEvent
      | MouseMoveMsg de MouseEvent
      | MouseLeaveMsg de MouseEvent
    //   | WheelMsg de WheelEvent
    //   | TouchStartMsg de TouchEvent
    //   | TouchEndMsg de TouchEvent
    //   | TouchMoveMsg de TouchEvent
      | RenderMsg

  taper JuliaSeed = { SeedX: float; SeedY: float }
  taper JuliaScrolling = Move | ChangeSeed

  taper FractalType =
      | Mandelbrot
      | Julia de JuliaSeed * JuliaScrolling

  taper Transform =
      | Scrolling de float * float
      | Pinching de float
      | NoTransform

  taper Model =
      {
          CanvasHeight: float
          Zoom: float
          FractalType: FractalType
          X: float
          Y: float
          Now: System.DateTime
          Render: (Model -> unit) option
          Transform: Transform
      }

module FractalRenderer =

  ouvrir System
  ouvrir Browser
  ouvrir Browser.Types
  ouvrir WebGLHelper
  ouvrir Types

  laisser myVertex = """
      precision highp float;
      precision highp int;

      attribute vec4 aVertexPosition;
      attribute vec2 aTextureCoord;
      varying vec2 vTextureCoord;
      vide main() {
        gl_Position = aVertexPosition;
        vTextureCoord = aTextureCoord;
      }
  """

  laisser myFragment = """
      precision highp float;
      precision highp int;
      uniform float uWidthOverHeight;
      uniform float uZoom;
      uniform vec2 uOffset, uJuliaSeed;
      uniform bool uIsJulia;
      varying vec2 vTextureCoord;
      vec2 calculatePosition(vec2 inputCoords, float zoom, float widthOverHeight, vec2 offset)
      {
          retour (inputCoords - 0.5) * vec2(widthOverHeight, 1.0) / zoom + offset;
      }
      vec4 applyColourMap(float x)
      {
          retour vec4(sin(x * 4.0), sin (x * 5.0), sin (x * 6.0), 1.0);
      }
      vec2 cConj(vec2 z)
      {
          retour vec2(z.x, -z.y);
      }
      vec2 cMul(vec2 a, vec2 b)
      {
          retour vec2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
      }
      vec2 cSq(vec2 z)
      {
          retour cMul(z, z);
      }
      vec2 cCube(vec2 z)
      {
          retour cMul(z, cMul(z, z));
      }
      vec2 cPow4(vec2 z)
      {
          retour cSq(cSq(z));
      }
      vec2 cDiv(vec2 a, vec2 b)
      {
          retour cMul(a, cConj(b));
      }
      vec2 cRecip(vec2 z)
      {
          retour cDiv(vec2(1.0, 0.0), z);
      }
      vec2 f(vec2 z, vec2 offset)
      {
          retour cSq(z) + offset;
      }
      float pixelResult(vec2 z, vec2 offset)
      {
          float result = 0.0;
          vec2 zsq = z * z;
          int iterations = 0;
          pour (int i = 0; i < 128; i++)
          {
              iterations = i;
              si (zsq.x + zsq.y > 49.0)
              {
                  break;
              }
              z = f(z, offset);
              zsq = z * z;
          }
          si (iterations == 127)
          {
              result = 0.0;
          }
          autre
          {
              result = float(iterations) + (log(2.0 * log(7.0)) - log(log(zsq.x + zsq.y))) / log(2.0);
              result = log(result * 0.4) / log(128.0);
          }
          retour result;
      }
      vide main(vide)
      {
          vec2 z = calculatePosition(vTextureCoord, uZoom, uWidthOverHeight, uOffset);
          float result = pixelResult(z, uIsJulia ? uJuliaSeed : z);
          gl_FragColor = applyColourMap(result);
      }
  """

  laisser initBuffers gl =
      laisser positions =
          createBuffer
              [|
                  -1.0; -1.0;
                    1.0; -1.0;
                  -1.0;  1.0;
                    1.0;  1.0
              |] gl
      laisser textureCoords =
          createBuffer
              [|
                  0.0; 0.0;
                  1.0; 0.0;
                  0.0; 1.0;
                  1.0; 1.0
              |] gl
      positions, textureCoords

  laisser create (holder : Element) =

      laisser canvas = document.createElement "canvas" :?> HTMLCanvasElement
      laisser width = 640
      laisser height = 480

      canvas.width <- float width
      canvas.height <- float height

      holder.appendChild(canvas) |> ignore

      laisser context = getWebGLContext canvas

      laisser program = createShaderProgram context myVertex myFragment

      laisser positionBuffer, colourBuffer = initBuffers context
      laisser vertexPositionAttribute = createAttributeLocation context program "aVertexPosition"
      laisser textureCoordAttribute = createAttributeLocation context program "aTextureCoord"
      laisser widthOverHeightUniform = createUniformLocation context program "uWidthOverHeight"
      laisser zoomUniform = createUniformLocation context program "uZoom"
      laisser offsetUniform = createUniformLocation context program "uOffset"
      laisser juliaSeedUniform = createUniformLocation context program "uJuliaSeed"
      laisser isJuliaUniform = createUniformLocation context program "uIsJulia"

      laisser draw widthOverHeight zoom x y jx jy isJulia =
          context.useProgram(program)

          context.bindBuffer(context.ARRAY_BUFFER, positionBuffer)
          context.vertexAttribPointer(vertexPositionAttribute, 2.0, context.FLOAT, faux, 0.0, 0.0)
          context.bindBuffer(context.ARRAY_BUFFER, colourBuffer)
          context.vertexAttribPointer(textureCoordAttribute, 2.0, context.FLOAT, faux, 0.0, 0.0)

          context.uniform1f(widthOverHeightUniform, widthOverHeight)
          context.uniform1f(zoomUniform, zoom)
          context.uniform2f(offsetUniform, x, y)
          context.uniform2f(juliaSeedUniform, jx, jy)
          context.uniform1i(isJuliaUniform, si isJulia alors 1.0 autre 0.0)

          context.drawArrays (context.TRIANGLE_STRIP, 0., 4.0)

      laisser clear = clear context

      // Try not à utiliser "context" after this point, bind a fonction above.

      laisser imageLoadCanvas = document.createElement "canvas" :?> HTMLCanvasElement
      laisser imageLoadCanvasContext = imageLoadCanvas.getContext_2d()

      laisser mutable last = DateTime.Now

      laisser render model =
          correspondre model avec
          | model quand model.Now <> last ->
              last <- model.Now

              laisser resolution = canvas.width, canvas.height
              laisser widthOverHeight = si canvas.height = 0.0 alors 1.0 autre canvas.width / canvas.height
              clear resolution

              correspondre model.FractalType avec
              | Mandelbrot ->
                  draw widthOverHeight model.Zoom model.X model.Y 0.0 0.0 faux
              | Julia ({ SeedX = seedX; SeedY = seedY }, _) ->
                  draw widthOverHeight model.Zoom model.X model.Y seedX seedY vraie

          | _ -> ignore()

      render, height

module State =

    ouvrir Browser
    ouvrir Browser.Types
    ouvrir Fable.Core.JsInterop
    ouvrir Types

    // taper INormalizedWheel =
    //     abstraite membre pixelX: float
    //     abstraite membre pixelY: float
    //     abstraite membre spinX: float
    //     abstraite membre spinY: float

    // laisser normalizeWheel : WheelEvent -> INormalizedWheel = importDefault "normalize-wheel"

    laisser renderCommand =
        laisser sub dispatch =
            window.requestAnimationFrame(fon _ -> dispatch RenderMsg) |> ignore
        [sub]

    laisser initMandelbrot =
        {
            CanvasHeight = 1.0
            Zoom = 0.314
            FractalType = Mandelbrot
            X = -0.5
            Y = 0.0
            Now = System.DateTime.Now
            Render = None
            Transform = NoTransform
        }

    laisser initJulia =
        {
            CanvasHeight = 1.0
            Zoom = 0.314
            FractalType = Julia ({ SeedX = 0.0; SeedY = 0.0 }, ChangeSeed)
            X = 0.0
            Y = 0.0
            Now = System.DateTime.Now
            Render = None
            Transform = NoTransform
        }

    laisser init() =
        document.addEventListener("gesturestart", (fon e -> e.preventDefault()), vraie)
        document.addEventListener("gesturechange", (fon e -> e.preventDefault()), vraie)
        document.addEventListener("gestureend", (fon e -> e.preventDefault()), vraie)
        document.addEventListener("scroll", (fon e -> e.preventDefault()), vraie)
        initMandelbrot, renderCommand

    laisser updateForMove x y model =
        correspondre model.Transform avec
        | Scrolling (lastScreenX, lastScreenY) ->
            { model avec
                X = model.X - (x - lastScreenX) / (model.Zoom * model.CanvasHeight)
                Y = model.Y + (y - lastScreenY) / (model.Zoom * model.CanvasHeight)
                Transform = Scrolling (x, y)
            }, []
        | _ -> model, []

    laisser updateForSeedChange seed x y model =
        correspondre model.Transform avec
        | Scrolling (lastScreenX, lastScreenY) ->
            { model avec
                FractalType = Julia ( {
                                        SeedX = seed.SeedX - (x - lastScreenX) / (model.Zoom * model.CanvasHeight)
                                        SeedY = seed.SeedY - (y - lastScreenY) / (model.Zoom * model.CanvasHeight)}, ChangeSeed)
                Transform = Scrolling (x, y)
            }, []
        | _ -> model, []

    laisser update msg model =
        correspondre model.FractalType, msg avec
        | Julia _, MandelbrotClick _ ->
            { model avec
                Zoom = 0.314; FractalType = Mandelbrot; X = -0.5; Y = 0.0
            }, []

        | Mandelbrot, JuliaClick ->
            { model avec
                Zoom = 0.314; FractalType = Julia ({ SeedX = 0.0; SeedY = 0.0 }, ChangeSeed); X = 0.0; Y = 0.0
            }, []

        | Julia (seed, _), JuliaMoveClick ->
            { model avec FractalType = Julia (seed, Move) }, []

        | Julia (seed, _), JuliaChangeSeedClick ->
            { model avec FractalType = Julia (seed, ChangeSeed) }, []

        | _, MouseDownMsg me quand me.button = 0.0 ->
            { model avec
                Transform = Scrolling (me.screenX, me.screenY)
            }, []

        | _, MouseUpMsg me quand me.button = 0.0 -> { model avec Transform = NoTransform }, []

        | _, MouseLeaveMsg _ -> { model avec Transform = NoTransform }, []

        | Mandelbrot, MouseMoveMsg me
        | Julia (_, Move), MouseMoveMsg me ->
            updateForMove me.screenX me.screenY model

        | Julia (seed, ChangeSeed), MouseMoveMsg me ->
            updateForSeedChange seed me.screenX me.screenY model

        // | _, WheelMsg we ->
        //     laisser zoom = (normalizeWheel we).pixelY / 100.0
        //     { model avec Zoom = model.Zoom * 0.99 ** zoom }, []

        // | _, TouchEndMsg _ -> { model avec Transform = NoTransform }, []

        // | _, TouchStartMsg te quand te.touches.Length = 1 ->
        //     { model avec
        //         Transform = Scrolling (te.touches.[0].clientX, te.touches.[0].clientY)
        //     }, []

        // | _, TouchStartMsg te quand te.touches.Length = 2 ->
        //     laisser dx = te.touches.[1].clientX - te.touches.[0].clientX
        //     laisser dy = te.touches.[1].clientY - te.touches.[0].clientY
        //     laisser distance = sqrt (dx * dx + dy * dy)
        //     { model avec
        //         Transform = Pinching distance
        //     }, []

        // | Mandelbrot, TouchMoveMsg te
        // | Julia (_, Move), TouchMoveMsg te quand te.touches.Length = 1 ->
        //     updateForMove te.touches.[0].screenX te.touches.[0].screenY model

        // | Julia (seed, ChangeSeed), TouchMoveMsg te quand te.touches.Length = 1 ->
        //     updateForSeedChange seed te.touches.[0].screenX te.touches.[0].screenY model

        // | Mandelbrot, TouchMoveMsg te
        // | Julia _, TouchMoveMsg te quand te.touches.Length = 2 ->
        //     correspondre model.Transform avec
        //     | Pinching lastDistance ->
        //         laisser dx = te.touches.[1].clientX - te.touches.[0].clientX
        //         laisser dy = te.touches.[1].clientY - te.touches.[0].clientY
        //         laisser distance = sqrt (dx * dx + dy * dy)
        //         { model avec
        //             Zoom = model.Zoom * 0.99 ** (lastDistance - distance)
        //             Transform = Pinching distance
        //         }, []
        //     | _ -> model, []

        | _, RenderMsg ->
            correspondre model.Render avec
            | None ->
                laisser holder = document.getElementById("Fractal")
                correspondre holder avec
                | nulle -> model, renderCommand
                | h ->
                    laisser renderer, height = FractalRenderer.create h
                    { model avec Render = Some renderer; CanvasHeight = float height }, renderCommand
            | Some render ->
                render model
                { model avec Now = System.DateTime.Now }, renderCommand

        | _ -> model, []

module View =

    ouvrir Elmish
    ouvrir Types
    ouvrir State

    laisser showParams model =
        correspondre model.FractalType avec
        | Julia (seed, _) ->
            [
                h?p [] [ Text $"X = %.6f{model.X}" ]
                h?p [] [ Text $"Y = %.6f{model.Y}" ]
                h?p [] [ Text $"Zoom = %.6f{model.Zoom}" ]
                h?p [] [ Text $"Seed X = %.6f{seed.SeedX}" ]
                h?p [] [ Text $"Seed Y = %.6f{seed.SeedY}" ]
            ]
        | Mandelbrot ->
            [
                h?p [] [ Text $"X = %.6f{model.X}" ]
                h?p [] [ Text $"Y = %.6f{model.Y}" ]
                h?p [] [ Text $"Zoom = %.6f{model.Zoom}" ]
            ]

    laisser showButtons model dispatch =
        h?div [] [
            h?div [ "classe" => "field has-addons" ] [
                h?button [
                    (correspondre model.FractalType avec
                        | Mandelbrot -> "classe" => "button is-primary is-selected"
                        | Julia _ -> "classe" => "button")
                    "onclick" =!> (fon _ -> MandelbrotClick |> dispatch)
                ] [ Text "Mandelbrot" ]
                h?button [
                    (correspondre model.FractalType avec
                        | Mandelbrot -> "classe" => "button"
                        | Julia _ -> "classe" => "button is-primary is-selected")
                    "onclick" =!> (fon _ -> JuliaClick |> dispatch)
                ] [ Text "Julia" ]
            ]
            h?div [] [
                correspondre model.FractalType avec
                | Julia (_, scrollType) ->
                    rendement h?button [
                        (correspondre scrollType avec
                            | Move -> "classe" => "button is-primary is-selected"
                            | ChangeSeed -> "classe" => "button")
                        "onclick" =!> (fon _ -> JuliaMoveClick |> dispatch)
                    ] [ Text "Move" ]
                    rendement h?button [
                        (correspondre scrollType avec
                            | Move -> "classe" => "button"
                            | ChangeSeed -> "classe" => "button is-primary is-selected")
                        "onclick" =!> (fon _ -> JuliaChangeSeedClick |> dispatch)
                    ] [ Text "ChangeSeed" ]
                | _ -> ()
            ]
        ]

    laisser hud model dispatch =
        h?div [ "classe" => "columns" ] [
            h?div [ "classe" => "column" ] (showParams model)
            h?div [ "classe" => "column" ] [ showButtons model dispatch ]
        ]

    laisser fractalCanvas dispatch =
        laisser dispatch (msg: 'Event -> Msg) (e: Browser.Types.Event) =
            e.preventDefault()
            msg (e :?> 'Event) |> dispatch

        h?div [
            "id" => "Fractal"
            "onmousedown" =!> dispatch MouseDownMsg
            "onmouseup" =!> dispatch MouseUpMsg
            "onmousemove" =!> dispatch MouseMoveMsg
            "onmouseleave" =!> dispatch MouseLeaveMsg
            // "onwheel" =!> dispatch WheelMsg
            // "ontouchstart" =!> dispatch TouchStartMsg
            // "ontouchmove" =!> dispatch TouchMoveMsg
            // "ontouchend" =!> dispatch TouchEndMsg
            // "ontouchcancel" =!> dispatch TouchEndMsg
        ] []

    laisser root model dispatch =
        h?div [] [
            hud model dispatch
            fractalCanvas dispatch
        ]

    app "FableFractal" init update root
