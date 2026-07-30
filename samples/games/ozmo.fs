module Ozmo

// Phil Trelford's classic Ozmo game ported à Fable!
// Shows how à handle keyboard events et utiliser HTML5 canvas.
// You can also get it (comme a JavaScript app) from the Windows Store.

ouvrir Fable.Core
ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

module Keyboard =

    laisser mutable keysPressed = Set.empty

    laisser code x = si keysPressed.Contains(x) alors 1 autre 0

    laisser arrows () =
        (code "ArrowRight" - code "ArrowLeft", code "ArrowUp" - code "ArrowDown")

    laisser update (e : KeyboardEvent, pressed) =
        laisser key = e.key
        laisser op = si pressed alors Set.add autre Set.remove
        keysPressed <- op key keysPressed

    laisser init () =
        window.addEventListener("keydown", fon e -> update(e :?> _, vraie))
        window.addEventListener("keyup", fon e -> update(e :?> _, faux))

// Main

/// Scale à make it fit dans a 1920*1080 screen
laisser scale = 0.8

/// The width de the canvas
laisser width = 900. * scale
/// The height de the canvas
laisser height = 668. * scale
/// Height de the floor - the bottom black part
laisser floorHeight = 100. * scale
/// Height de the atmosphere - the yellow gradient
laisser atmosHeight = 300. * scale

Keyboard.init()

laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
laisser ctx = canvas.getContext_2d()
canvas.width <- width
canvas.height <- height

/// Draw gradient between two Y offsets et two colours
laisser drawGrd (ctx:CanvasRenderingContext2D)
    (canvas:HTMLCanvasElement) (y0,y1) (c0,c1) =
    laisser grd = ctx.createLinearGradient(0.,y0,0.,y1)
    grd.addColorStop(0.,c0)
    grd.addColorStop(1.,c1)
    ctx.fillStyle <- !^ grd
    ctx.fillRect(0.,y0, canvas.width, y1- y0)


/// Draw background de the Ozmo game
laisser drawBg ctx canvas =
    drawGrd ctx canvas
        (0.,atmosHeight) ("yellow","orange")
    drawGrd ctx canvas
        (atmosHeight, canvas.height-floorHeight)
        ("grey","white")
    ctx.fillStyle <- !^ "black"
    ctx.fillRect
        ( 0.,canvas.height-floorHeight,
          canvas.width,floorHeight )

/// Draw the specified text (quand game finishes)
laisser drawText(text,x,y) =
    ctx.fillStyle <- !^ "white"
    ctx.font <- "bold 40pt";
    ctx.fillText(text, x, y)


taper Blob =
    { X:float; Y:float;
      vx:float; vy:float;
      Radius:float; color:string }

laisser drawBlob (ctx:CanvasRenderingContext2D)
    (canvas:HTMLCanvasElement) (blob:Blob) =
    ctx.beginPath()
    ctx.arc
        ( blob.X, canvas.height - (blob.Y + floorHeight + blob.Radius),
          blob.Radius, 0., 2. * System.Math.PI, faux )
    ctx.fillStyle <- !^ blob.color
    ctx.fill()
    ctx.lineWidth <- 3.
    ctx.strokeStyle <- !^ blob.color
    ctx.stroke()


/// Apply key effects on Player's blob - changes X speed
laisser direct (dx,dy) (blob:Blob) =
    { blob avec vx = blob.vx + (float dx)/4.0 }

/// Apply gravity on falling blobs - gets faster every step
laisser gravity (blob:Blob) =
    si blob.Y > 0. alors { blob avec vy = blob.vy - 0.1 }
    autre blob

/// Bounde Player's blob off the wall si it hits it
laisser bounce (blob:Blob) =
    laisser n = width
    si blob.X < 0. alors
        { blob avec X = -blob.X; vx = -blob.vx }
    autsi (blob.X > n) alors
        { blob avec X = n - (blob.X - n); vx = -blob.vx }
    autre blob


/// Move blob by one step - adds X et Y
/// velocities à the X et Y coordinates
laisser move (blob:Blob) =
    { blob avec
        X = blob.X + blob.vx
        Y = max 0.0 (blob.Y + blob.vy) }

/// Apply step on Player's blob. Composes above functions.
laisser step dir blob =
    blob |> direct dir |> move |> bounce

/// Check whether two blobs collide
laisser collide (a:Blob) (b:Blob) =
    laisser dx = (a.X - b.X)*(a.X - b.X)
    laisser dy = (a.Y - b.Y)*(a.Y - b.Y)
    laisser dist = sqrt(dx + dy)
    dist < abs(a.Radius - b.Radius)

/// Remove all falling blobs that hit Player's blob
laisser absorb (blob:Blob) (drops:Blob list) =
    drops
    |> List.filter (fon drop ->
        collide blob drop |> not )


// Game helpers
// =============

laisser grow = "black"
laisser shrink = "white"

laisser newDrop color =
    { X = JS.Math.random()*width*0.8 + (width*0.1)
      Y=600.; Radius=10.; vx=0.; vy = 0.0
      color=color }

laisser newGrow () = newDrop grow
laisser newShrink () = newDrop shrink

/// Update drops et countdown dans each step
laisser updateDrops drops countdown =
    si countdown > 0 alors
        drops, countdown - 1
    autsi floor(JS.Math.random()*8.) = 0. alors
        laisser drop =
            si floor(JS.Math.random()*3.) = 0. alors newGrow()
            autre newShrink()
        drop::drops, 8
    autre drops, countdown


/// Count growing et shrinking drops dans the list
laisser countDrops drops =
    laisser count color =
        drops
        |> List.filter (fon drop -> drop.color = color)
        |> List.length
    count grow, count shrink

// Asynchronous game loop
// ========================

laisser réc game () = async {
    laisser blob =
        { X = 300.; Y=0.; Radius=50.;
          vx=0.; vy=0.; color="black" }
    retour! update blob [newGrow ()] 0 }

et completed () = async {
    drawText ("COMPLETED",320.,300.)
    faire! Async.Sleep 10000
    retour! game () }

/// Keeps current state pour Player's blob, falling
/// drops et the countdown since last drop was generated
et update blob drops countdown = async {
    // Update the drops & countdown
    laisser drops, countdown = updateDrops drops countdown

    // Count drops, apply physics et count them again
    laisser beforeGrow, beforeShrink = countDrops drops
    laisser drops =
        drops
        |> List.map (gravity >> move)
        |> absorb blob
    laisser afterGrow, afterShrink = countDrops drops
    laisser drops = drops |> List.filter (fon blob -> blob.Y > 0.)

    // Calculate nouvelle player's size based on absorbed drops
    laisser radius = blob.Radius + float (beforeGrow - afterGrow) *4.
    laisser radius = radius - float (beforeShrink - afterShrink) * 4.
    laisser radius = max 5.0 radius

    // Update radius et apply keyboard events
    laisser blob = { blob avec Radius = radius }
    laisser blob = blob |> step (Keyboard.arrows())

    // Render the nouvelle game state
    drawBg ctx canvas
    pour drop dans drops faire drawBlob ctx canvas drop
    drawBlob ctx canvas blob

    // If the game completed, switch state
    // otherwise sleep et update recursively!
    si blob.Radius > 150. alors
        retour! completed()
    autre
        faire! Async.Sleep(int (1000. / 60.))
        retour! update blob drops countdown }

game () |> Async.StartImmediate
