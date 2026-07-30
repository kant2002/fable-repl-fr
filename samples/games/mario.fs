module Mario

ouvrir Fable.Core
ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

module Keyboard =

    laisser mutable keysPressed = Set.empty

    /// Returns 1 si key avec given code is pressed
    laisser code x =
        si keysPressed.Contains(x) alors 1 autre 0

    /// Update the state de the set pour given key event
    laisser update (e : KeyboardEvent, pressed) =
        laisser key = e.key
        laisser op =  si pressed alors Set.add autre Set.remove
        keysPressed <- op key keysPressed

    /// Returns pair avec -1 pour left ou down et +1
    /// pour right ou up (0 si no ou both keys are pressed)
    laisser arrows () =
        (code "ArrowRight" - code "ArrowLeft", code "ArrowUp" - code "ArrowDown")

    laisser initKeyboard () =
        document.addEventListener("keydown", fon e -> update(e :?> _, vraie))
        document.addEventListener("keyup", fon e -> update(e :?> _, faux))

module Physics =

    taper MarioModel =
        { x:float; y:float;
          vx:float; vy:float;
          dir:string }


    // If the Up key is pressed (y > 0) et Mario is on the ground,
    // alors create Mario avec the y velocity 'vy' set à 5.0
    laisser jump (_,y) m =
        si y > 0 && m.y = 0. alors { m avec vy = 5. } autre m

    // If Mario is dans the air, alors his "up" velocity is decreasing
    laisser gravity m =
        si m.y > 0. alors { m avec vy = m.vy - 0.1 } autre m

    // Apply physics - move Mario according à the current velocities
    laisser physics m =
        { m avec x = m.x + m.vx; y = max 0. (m.y + m.vy) }

    // When Left/Right keys are pressed, change 'vx' et direction
    laisser walk (x,_) m =
        laisser dir = si x < 0 alors "left" autsi x > 0 alors "right" autre m.dir
        { m avec vx = float x; dir = dir }


    laisser marioStep dir mario =
        mario
        |> physics
        |> walk dir
        |> gravity
        |> jump dir

module Canvas =

    // Get the canvas context pour drawing
    laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
    laisser context = canvas.getContext_2d()

    // Format RGB color comme "rgb(r,g,b)"
    laisser ($) s n = s + n.ToString()
    laisser rgb r g b = "rgb(" $ r $ "," $ g $ "," $ b $ ")"

    /// Fill rectangle avec given color
    laisser filled (color: string) rect =
        laisser ctx = context
        ctx.fillStyle <- !^ color
        ctx.fillRect rect

    /// Move element à a specified X Y position
    laisser position (x,y) (img : HTMLImageElement) =
        img?style?left <- x.ToString() + "px"
        img?style?top <- (canvas.offsetTop + y).ToString() + "px"

    laisser getWindowDimensions () =
        canvas.width, canvas.height

    /// Get the first <img /> element et set `src` (faire
    /// nothing si it is the right one à keep animation)
    laisser image (src:string) =
        laisser image = document.getElementsByTagName("img").[0] :?> HTMLImageElement
        si image.src.IndexOf(src) = -1 alors image.src <- src
        image

ouvrir Canvas
ouvrir Physics

laisser origin =
    // Sample is running dans an iframe, so get the location de parent
    laisser topLocation = window.top.location
    topLocation.origin + topLocation.pathname

laisser render (w,h) (mario: MarioModel) =
    (0., 0., w, h) |> filled (rgb 174 238 238)
    (0., h-50., w, 50.) |> filled (rgb 74 163 41)
    // Select et position Mario
    // (walking is represented comme an animated gif)
    laisser verb =
        si mario.y > 0. alors "jump"
        autsi mario.vx <> 0. alors "walk"
        autre "stand"
    origin + "img/mario/mario" + verb + mario.dir + ".gif"
    |> image
    |> position (w/2.-16.+mario.x,  h-50.-31.-mario.y)

Keyboard.initKeyboard()

laisser w, h = getWindowDimensions()

laisser réc update mario () =
    laisser mario = mario |> Physics.marioStep (Keyboard.arrows())
    render (w,h) mario
    window.setTimeout(update mario, 1000 / 60) |> ignore

laisser mario = { x=0.; y=0.; vx=0.; vy=0.; dir="right" }
update mario ()
