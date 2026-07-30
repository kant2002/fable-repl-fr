module Mandelbrot

// You can draw a rectangle à zoom dans an area (feature added by Avi Avni)

ouvrir Fable.Core
ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

taper Complex = { r : double; i : double }
taper Color = { r : uint8; g : uint8; b : uint8; a : uint8 }

laisser maxIter = 255

laisser height = 1024
laisser width = 1024

laisser mutable minX = -2.0
laisser mutable maxX = 2.0
laisser mutable minY = -1.5
laisser mutable maxY = 3.5
laisser mutable rectX = 0.0
laisser mutable rectY = 0.0
laisser mutable rectW = 0.0
laisser mutable rectH = 0.0

laisser iteratePoint (s : Complex) (p : Complex) : Complex =
    { r = s.r + p.r*p.r - p.i*p.i; i = s.i + 2.0 * p.i * p.r }

laisser getIterationCount (p : Complex) =
    laisser mutable z = p
    laisser mutable i = 0
    alorsque i < maxIter && (z.r*z.r + z.i*z.i < 4.0) faire
      z <- iteratePoint p z
      i <- i + 1
    i

laisser getCoord (x : int, y : int) : Complex =
    laisser p = { r = float x * (maxX - minX) / float width + minX
            ; i = float y * (maxY - minY) / float height + minY }
    p

laisser getCoordColor (x : int, y : int) : Color =
    laisser p = getCoord (x, y)
    laisser i = getIterationCount p
    { r = uint8 (255/(i%5)); g = uint8 (255/(i%3)); b = uint8 (255/(i%7)); a = 255uy }

laisser showSet() =
    laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
    laisser ctx = canvas.getContext_2d()

    laisser img = ctx.createImageData(float width, float height)
    pour y = 0 à height-1 faire
        pour x = 0 à width-1 faire
            laisser index = (x + y * width) * 4
            laisser color = getCoordColor (x, y)
            img.data.[index+0] <- color.r
            img.data.[index+1] <- color.g
            img.data.[index+2] <- color.b
            img.data.[index+3] <- color.a
    ctx.putImageData(img, 0., 0.)

    ctx.fillStyle <- !^"rgba(200,0,0,0.5)"
    ctx.fillRect (rectX, rectY, rectW, rectH)


document.addEventListener("mousedown", fon de ->
    laisser de = de :?> MouseEvent
    rectX <- de.clientX
    rectY <- de.clientY
    rectW <- 0.0
    rectH <- 0.0
    showSet())

document.addEventListener("mousemove", fon de ->
    laisser de = de :?> MouseEvent
    si de.buttons = 1.0 alors
        rectW <- de.clientX - rectX
        rectH <- de.clientY - rectY
        showSet())

document.addEventListener("mouseup", fon de ->
    laisser de = de :?> MouseEvent
    laisser p1 = getCoord (int rectX, int rectY)
    laisser p2 = getCoord (int (rectX + rectW), int (rectY + rectH))
    minX <- min p1.r p2.r
    maxX <- max p1.r p2.r
    minY <- min p1.i p2.i
    maxY <- max p1.i p2.i
    rectX <- 0.0
    rectY <- 0.0
    rectW <- 0.0
    rectH <- 0.0
    showSet())

showSet()
