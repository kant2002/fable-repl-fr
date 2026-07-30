module BasicCanvas

ouvrir Fable.Core
ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

laisser init() =
    laisser canvas = document.querySelector(".view") :?> HTMLCanvasElement

    laisser ctx = canvas.getContext_2d()
    // The (!^) operator checks et casts a value à an Erased Union taper
    // See http://fable.io/docs/interacting.html#Erase-attribute
    ctx.fillStyle <- !^"rgb(200,0,0)"
    ctx.fillRect (10., 10., 55., 50.)
    ctx.fillStyle <- !^"rgba(0, 0, 200, 0.5)"
    ctx.fillRect (30., 30., 55., 50.)

init()
