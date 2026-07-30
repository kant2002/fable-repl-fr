module Pacman

// Another great F# game by Phil Trelford! The code involves rendering the maze,
// AI pour the ghosts, user interaction et even playing sound effects. There is
// some brief commentary, but si you're a beginner look at the other examples first.

ouvrir Fable.Core
ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

module Sound =
    laisser [<Global>] Audio: obj = jsNative

    laisser origin =
        // Sample is running dans an iframe, so get the location de parent
        laisser topLocation = window.top.location
        topLocation.origin + topLocation.pathname

    laisser play (fileName: string) =
        laisser audio = createNew Audio (origin + "img/pacman/" + fileName + ".wav")
        audio?play()

module Images =
    (**
    The following block embeds the ghosts et other parts de graphics comme Base64 encoded strings.
    This way, we can load them without making additional server requests:
    *)
    laisser cyand = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAiUlEQVQoU8WSURKAIAhE8Sh6Fc/tVfQoJdqiMDTVV4wfufAAmw3kxEHUz4pA1I8OJVjAKZZ6+XiC0ATTB/gW2mEFtlpHLqaktrQ6TxUQSRCAPX2AWPMLyM0VmPOcV8palxt6uoAMpDjfWJt+o6cr0DPDnfYjyL94NwIcYjXcR/FuYklcxrZ3OO0Ep4dJ/3dR5jcAAAAASUVORK5CYII="
    laisser oranged = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAgklEQVQoU8WS0RGAIAxDZRRYhblZBUZBsBSaUk/9kj9CXlru4g7r1FxBdsFpGwoa2NwrYIFPEIeM6QS+hQQMYC70EjzuuOlt6gT5kRGGTf0Cx5qfwJYOYIw0L6W1bg+09Al2wAcCS8Y/WjqAZhluxD/B3ghZBO6n1sadzLLEbNSg8pzXIVLvbNvPwAAAAABJRU5ErkJggg=="
    laisser pinkd = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAj0lEQVQoU8WSsRWAIAxEZRQpXITGVZzIVWxYxAJHwRfwMInxqZV0XPIvgXeuM05eUuayG73TbULQwKWZGTTwCYIJphfwLcRhAW5DLfWrXFLrNLWBKAIBbOkFxJpfQDIXYAh1XoznumRo6Q0kwE8VTLN8o6UL0ArDnfYjSF/Mg4CEaA330sxD3ApHLvUdSdsBdgNkr9L8gxYAAAAASUVORK5CYII="
    laisser redd = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAkklEQVQoU8WSvRWAIAyEZRQtXIRCV3EiVtGCRSx0FHxBD5MYn1pJl0u+/PDOVcZLY5e47PrJ6TIhaOBSzBoU8AlCE0zP4FuIwwJc25Bz9TyILbVOUwuIJAjAlp5BrPkFpOYC9H6fF+O5LjW09AIS0Az7jUuQN1q6AC0z3Gk/gvTF3AhwiNYQ52Ju4pI4fKljOG0DA3tp97vN6C8AAAAASUVORK5CYII="
    laisser pu1 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAANCAYAAABy6+R8AAAAWElEQVQoU62SUQoAIAhD9f6HNiYYolYi9VfzuXIxDRYbI0LCTHsfe3ldi3BgRRUY9Rnku1Rupf4NgiPeVjVU7STckphBceSvrHHtNPI21HWz4NO3eUUAgwVpmjX/zwK8KQAAAABJRU5ErkJggg=="
    laisser pu2 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAANCAYAAABy6+R8AAAAW0lEQVQoU8WSwQoAIAhD9f8/2lIwdKRIl7o1e010THBESJiJXca76qnoDxFC3SD9LRpWkLnsLt4gdImtlLX/EK4iDapqr4VuI2+BauQjaOrmSz8xillDp5gQrS054jv/0fkNVAAAAABJRU5ErkJggg=="
    laisser pd1 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAANCAYAAABy6+R8AAAAXElEQVQoU62SUQoAIAhD9f6HNgyMWpMs6k/XU5mqwDMTw5yq6JwbAfucwR2qAFHAu75BN11Gt6+Qz54VpMJsMV3BaS9UR8txkUzfLC9DUY0BYbOPGfpyU3g2WdwAOvU1/9KZsT4AAAAASUVORK5CYII="
    laisser pd2 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAANCAYAAABy6+R8AAAAU0lEQVQoU62SUQoAIAhD9f6HNgwUGw4s6q/pc6KqwDMTQ01VtGr56ZIZvKEJEAXc9Q26cUm3r5D3zgrywHeoG3ldJrZIRz6C0I1BoR83FTBCeHsLIlw7/wOkQycAAAAASUVORK5CYII="
    laisser pl1 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAANCAYAAABy6+R8AAAAVUlEQVQoU62S2woAIAhD9f8/2jAwvGRMyDfF49iQKZUISZ4xE/vZaW7LHbwhBLADqjpSUjBAdglRDQa9hxfcQi+vf5RGnpDlkB4KlMgR0N6pBIH83gIPFCb/N+MLCwAAAABJRU5ErkJggg=="
    laisser pl2 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAANCAYAAABy6+R8AAAAUklEQVQoU52SUQoAIAhD3f0PbRQoZgnT/hyttYeQdFRFswYIoubD73JlPibGYA/s1Jmpk+JpDIinWxbiXP3iQslCwbhTxzhHbsWZNFsnCkTevQW2bCb/VRTuVwAAAABJRU5ErkJggg=="
    laisser pr1 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAANCAYAAABy6+R8AAAAWElEQVQoU52S4Q4AIASE3fs/tKalSTHyL/O5CyAXzMQ+BxBsbj9exRE8oQqgDUS1BalNVFSuP2WQL94WIygCBEzttZWOvbz2VBnGtLXg1sgV/L8I679yewN9sScO5wcxLQAAAABJRU5ErkJggg=="
    laisser pr2 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAANCAYAAABy6+R8AAAAVElEQVQoU62SWwoAIAgE9f6HNgqU3BK2R3+J48KoCjwzMaypis61+OyaK3hADOADeuoddJISaQy0iKggbEz2viah7mVPTNq7cp/ApLmcdFPVdaDJBnWdJwjk629HAAAAAElFTkSuQmCC"
    laisser blue = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAeklEQVQoU62S0Q3AIAhEyyi6UcfoRB2jG+koNkeCoVcaTaw/huMeEkS24KTUmpdrFWHbQ2CAzb5AB0eQFTFYwVnIw/+B5by0cD52vTmGhnaF25wBAb/A6HsibR0ctch5fRHi1zCigvCut4oR+wnbhrBmsZr9DlqCQfbcnfZjDyiZqCEAAAAASUVORK5CYII="
    laisser eyed = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAUElEQVQoU2NkIBMwkqmPYYA13rt37z/I6UpKSiguwSYOVwCThPkZphmXOHU0OjtD7Nu7F+FckI3YxFH8oqgI8eP9+6h+xCY+wNFBSiqiv1MBDgYsD185vj8AAAAASUVORK5CYII="
    laisser _200 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAS0lEQVQoU2NkIBMwkqmPYYA0vpVR+Q9zsvCTO4yE+CC1KE4FaYBpxEfDNWKzgWiNIIUw5xKyGa+N+PyM4UdS4nSA4pEUJ8LUku1UAMC0VA8iscBNAAAAAElFTkSuQmCC"
    laisser _400 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAASElEQVQoU2NkIBMwkqmPYYA0vpVR+S/85A4jMg3zAkwcmQ9ig52KTSO6Qch8FI3oNhClEaaJWJvhNmLTSJQfyYnLAYpHujoVAChTXA9pVJi5AAAAAElFTkSuQmCC"
    laisser _800 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAQElEQVQoU2NkIBMwkqmPYYA0vpVR+Q9zsvCTO4yE+CC1YKeCFMI0EEOjaES3EZ8BtLERn5/hNpITlwMUj3R1KgCe5lwPHtUmcwAAAABJRU5ErkJggg=="
    laisser _1600 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAYAAAAfSC3RAAAAQ0lEQVQoU2NkIBMwkqmPYQA0vpVR+S/85A4jiIY5mxg+WANMIYiGaUYXR+ejaES3EdlAvBrxKSTJRnx+HoDoGDopBwDHLGwPAhDgRQAAAABJRU5ErkJggg=="

    // Create image using the specified data
    laisser createImage data =
      laisser img = document.createElement("img") :?> HTMLImageElement
      img.src <- data
      img

    // Load the different Pacman images
    laisser privée pu1Img, pu2Img =
      createImage pu1, createImage  pu2
    laisser privée pd1Img, pd2Img =
      createImage pd1, createImage  pd2
    laisser privée pl1Img, pl2Img =
      createImage pl1, createImage pl2
    laisser privée pr1Img, pr2Img =
      createImage pr1, createImage pr2

    // Represent Pacman's mouth state
    laisser privée lastp = ref pr1Img

    (**
    This fonction returns the pacman image pour the specified X et Y location, taking into account the
    direction dans which Pacman is going. It keeps a mutable state avec current step de Pacman's
    mouth.
    *)
    laisser imageAt(x: _ ref, y: _ ref, v: _ ref) =
      laisser p1, p2 =
        correspondre v.Value avec
        | -1,  0 -> pl1Img, pl2Img
        |  1,  0 -> pr1Img, pr2Img
        |  0, -1 -> pu1Img, pu2Img
        |  0,  1 -> pd1Img, pd2Img
        |  _,  _ -> lastp.Value, lastp.Value
      laisser x' = int (floor(float (x.Value/6)))
      laisser y' = int (floor(float (y.Value/6)))
      laisser p = si (x' + y') % 2 = 0 alors p1 autre p2
      lastp.Value <- p
      p

module Keyboard =

    /// Set de currently pressed keys
    laisser mutable keysPressed = Set.empty
    /// Update the keys comme requested
    laisser reset () = keysPressed <- Set.empty
    laisser isPressed keyCode = Set.contains keyCode keysPressed

    /// Triggered quand key is pressed/released
    laisser update (e : KeyboardEvent, pressed) =
      laisser key = e.key
      laisser op =  si pressed alors Set.add autre Set.remove
      keysPressed <- op key keysPressed

    /// Register DOM event handlers
    laisser init () =
      window.addEventListener("keydown", fon e -> update(e :?> _, vraie))
      window.addEventListener("keyup", fon e -> update(e :?> _, faux))

module Types =
    (**
    Creating ghosts
    ===============
    Ghosts are represented by a simple F# classe taper that contains the image de the ghost,
    current X, Y positions et a velocity dans both directions. In Pacman, ghosts are mutable
    et expose `Move` et `Reset` methods that change their properties.
    *)

    /// Wrap around the sides de the Maze
    laisser wrap (x,y) (dx,dy) =
      laisser x =
        si dx = -1 && x = 0 alors 30 * 8
        autsi dx = 1  && x = 30 *8 alors 0
        autre x
      x + dx, y + dy

    /// Mutable representation de a ghost
    taper Ghost(image: HTMLImageElement,x,y,v) =
      laisser mutable x' = x
      laisser mutable y' = y
      laisser mutable v' = v
      membre val Image = image
      membre val IsReturning = faux avec get, set
      membre __.X = x'
      membre __.Y = y'
      membre __.V = v'
      /// Move back à initial location
      membre ghost.Reset() =
        x' <- x
        y' <- y
      /// Move dans the current direction
      membre ghost.Move(v) =
        v' <- v
        laisser dx,dy = v
        laisser x,y = wrap (x',y') (dx,dy)
        x' <- x
        y' <- y

ouvrir Images
ouvrir Types

(**
Here we define the maze, tile bits et blank block. The maze is defined comme one big string
using ASCII-art encoding. Where `/`, `7`, `L` et `J` represent corners (upper-left, upper-right,
lower-left et lower-right), `!`, `|`, `-` et `_` represent walls (left, right, top, bottom) alorsque
`o` et `.` represent two kinds de pills dans the maze.
*)

laisser maze =
 [| "##/------------7/------------7##"
    "##|............|!............|##"
    "##|./__7./___7.|!./___7./__7.|##"
    "##|o|  !.|   !.|!.|   !.|  !o|##"
    "##|.L--J.L---J.LJ.L---J.L--J.|##"
    "##|..........................|##"
    "##|./__7./7./______7./7./__7.|##"
    "##|.L--J.|!.L--7/--J.|!.L--J.|##"
    "##|......|!....|!....|!......|##"
    "##L____7.|L__7 |! /__J!./____J##"
    "#######!.|/--J LJ L--7!.|#######"
    "#######!.|!          |!.|#######"
    "#######!.|! /__==__7 |!.|#######"
    "-------J.LJ |      ! LJ.L-------"
    "########.   | **** !   .########"
    "_______7./7 |      ! /7./_______"
    "#######!.|! L______J |!.|#######"
    "#######!.|!          |!.|#######"
    "#######!.|! /______7 |!.|#######"
    "##/----J.LJ L--7/--J LJ.L----7##"
    "##|............|!............|##"
    "##|./__7./___7.|!./___7./__7.|##"
    "##|.L-7!.L---J.LJ.L---J.|/-J.|##"
    "##|o..|!.......<>.......|!..o|##"
    "##L_7.|!./7./______7./7.|!./_J##"
    "##/-J.LJ.|!.L--7/--J.|!.LJ.L-7##"
    "##|......|!....|!....|!......|##"
    "##|./____JL__7.|!./__JL____7.|##"
    "##|.L--------J.LJ.L--------J.|##"
    "##|..........................|##"
    "##L--------------------------J##" |]

laisser tileBits =
 [| [|0b00000000;0b00000000;0b00000000;
      0b00000000;0b00000011;0b00000100;
      0b00001000;0b00001000|]

    [|0b00000000;0b00000000;0b00000000;0b00000000;0b11111111;0b00000000;0b00000000;0b00000000|] // top
    [|0b00000000;0b00000000;0b00000000;0b00000000;0b11000000;0b00100000;0b00010000;0b00010000|] // tr
    [|0b00001000;0b00001000;0b00001000;0b00001000;0b00001000;0b00001000;0b00001000;0b00001000|] // left
    [|0b00010000;0b00010000;0b00010000;0b00010000;0b00010000;0b00010000;0b00010000;0b00010000|] // right
    [|0b00001000;0b00001000;0b00000100;0b00000011;0b00000000;0b00000000;0b00000000;0b00000000|] // bl
    [|0b00000000;0b00000000;0b00000000;0b11111111;0b00000000;0b00000000;0b00000000;0b00000000|] // bottom
    [|0b00010000;0b00010000;0b00100000;0b11000000;0b00000000;0b00000000;0b00000000;0b00000000|] // br
    [|0b00000000;0b00000000;0b00000000;0b00000000;0b11111111;0b00000000;0b00000000;0b00000000|] // door
    [|0b00000000;0b00000000;0b00000000;0b00011000;0b00011000;0b00000000;0b00000000;0b00000000|] // pill
    [|0b00000000;0b00011000;0b00111100;0b01111110;0b01111110;0b00111100;0b00011000;0b00000000|] // power
 |]

laisser blank =
  [| 0b00000000;0b00000000;0b00000000; 0b00000000;0b00000000;0b00000000;0b00000000;0b00000000 |]

(**
Check pour walls:
The following functions parse the maze representation et check various properties de the maze.
Those are used pour rendering, but also pour checking whether Pacman can go dans a given direction.
Characters _|!/7LJ represent different walls
*)

laisser isWall (c:char) =
  "_|!/7LJ-".IndexOf(c) <> -1

/// Returns ' ' pour positions outside de range
laisser tileAt (x,y) =
  si x < 0 || x > 30 alors ' ' autre maze.[y].[x]

/// Is the maze tile at x,y a wall?
laisser isWallAt (x,y) =
  tileAt(x,y) |> isWall

// Is Pacman at a point where it can turn?
laisser verticallyAligned (x,y) =  (x % 8) = 5
laisser horizontallyAligned (x,y) = (y % 8) = 5
laisser isAligned n = (n % 8) = 5

// Check whether Pacman can go dans given direction
laisser noWall (x,y) (ex,ey) =
  laisser bx, by = (x+6+ex) >>> 3, (y+6+ey) >>> 3
  isWallAt (bx,by) |> not

laisser canGoUp (x,y) = isAligned x && noWall (x,y) (0,-4)
laisser canGoDown (x,y) = isAligned x && noWall (x,y) (0,5)
laisser canGoLeft (x,y) = isAligned y && noWall (x,y) (-4,0)
laisser canGoRight (x,y) = isAligned y && noWall (x,y) (5,0)

(**
Background rendering
================================
To render the background, we first fill the background
et alors iterate over the string lines that represent the maze et we draw images de
walls specified dans the `tileBits` value earlier (ou utiliser `blank` tile pour all other characters).

The following is used à map from tile characters à the `tileBits` values et à draw individual lines:
*)
laisser tileColors = "BBBBBBBBBYY"
laisser tileChars =  "/_7|!L-J=.o"

/// Returns tile pour a given Maze character
laisser toTile (c:char) =
  laisser i = tileChars.IndexOf(c)
  si i = -1 alors blank, 'B'
  autre tileBits.[i], tileColors.[i]

/// Draw the lines specified by a wall tile
laisser draw f (lines:int[]) =
  laisser width = 8
  lines |> Array.iteri (fon y line ->
    pour x = 0 à width-1 faire
      laisser bit = (1 <<< (width - 1 - x))
      laisser pattern = line &&& bit
      si pattern <> 0 alors f (x,y) )

/// Creates a brush pour rendering the given RGBA color
laisser createBrush (context:CanvasRenderingContext2D) (r,g,b,a) =
  laisser id = context.createImageData(1.0, 1.0)
  laisser d = id.data
  d.[0] <- r; d.[1] <- g
  d.[2] <- b; d.[3] <- a
  id

(**
The main fonction pour rendering background just fills the canvas avec a black color et
alors iterates over the maze tiles et renders individual walls:
*)
laisser createBackground () =
  // Fill background avec black
  laisser background = document.createElement("canvas") :?> HTMLCanvasElement
  background.width <- 256.
  background.height <- 256.
  laisser context = background.getContext_2d()
  context.fillStyle <- !^ "rgb(0,0,0)"
  context.fillRect (0., 0. , 256., 256.);

  // Render individual tiles de the maze
  laisser blue = createBrush context (63uy, 63uy, 255uy, 255uy)
  laisser yellow = createBrush context (255uy, 255uy, 0uy, 255uy)
  laisser lines = maze
  pour y = 0 à lines.Length-1 faire
    laisser line = lines.[y]
    pour x = 0 à line.Length-1 faire
      laisser c = line.[x]
      laisser tile, color = toTile c
      laisser brush = correspondre color avec 'Y' -> yellow | _ -> blue
      laisser f (x',y') =
        context.putImageData
          (brush, float (x*8 + x'), float (y*8 + y'))
      draw f tile
  background

/// Clear whatever is rendered dans the specified Maze cell
laisser clearCell (background : HTMLCanvasElement) (x,y) =
  laisser context = background.getContext_2d()
  context.fillStyle <- !^ "rgb(0,0,0)"
  context.fillRect (float (x*8), float (y*8), 8., 8.)

laisser createGhosts context =
  [| Images.redd, (16, 11), (1,0)
     Images.cyand, (14, 15), (1,0)
     Images.pinkd, (16, 13), (0,-1)
     Images.oranged, (18, 15), (-1,0) |]
  |> Array.map (fon (data,(x,y),v) ->
        Ghost(Images.createImage data, (x*8)-7, (y*8)-3, v) )

(**
Generating Ghost movement
=========================
For generating Ghost movements, we need an implementation de the [Flood fill algorithm](https://en.wikipedia.org/wiki/Flood_fill),
which we utiliser à generate the shortest path home quand Ghosts are returning. The `fillValue` fonction does this, by starting
at a specified location (which can be one de the directions dans which ghosts can go).
*)

/// Recursive flood fill fonction
laisser flood canFill fill (x,y) =
  laisser réc f n = fonction
    | [] -> ()
    | ps ->
        laisser ps = ps |> List.filter (fon (x,y) -> canFill (x,y))
        ps |> List.iter (fon (x,y) -> fill (x,y,n))
        ps |> List.collect (fon (x,y) ->
            [(x-1,y);(x+1,y);(x,y-1);(x,y+1)]) |> f (n+1)
  f 0 [(x,y)]

/// Possible routes that take the ghost home
laisser homeRoute =
  laisser numbers =
    maze |> Array.map (fon line ->
      line.ToCharArray()
      |> Array.map (fon c -> si isWall c alors 999 autre -1) )
  laisser canFill (x:int,y:int) =
    y>=0 && y < (numbers.Length-1) &&
    x>=0 && x < (numbers.[y].Length-1) &&
    numbers.[y].[x] = -1
  laisser fill (x,y,n) = numbers.[y].[x] <- n
  flood canFill fill (16,15)
  numbers

/// Find the shortest way home from specified location
/// (adjusted by offset dans which ghosts start)
laisser fillValue (x,y) (ex,ey) =
  laisser bx = int (floor(float ((x+6+ex)/8)))
  laisser by = int (floor(float ((y+6+ey)/8)))
  homeRoute.[by].[bx]

laisser fillUp (x,y) = fillValue (x,y) (0,-4)
laisser fillDown (x,y) = fillValue (x,y) (0,5)
laisser fillLeft (x,y) = fillValue (x,y) (-4,0)
laisser fillRight (x,y) = fillValue (x,y) (5,0)

(**
When choosing a direction, ghosts that are returning will go dans the direction
that leads them home. Other ghosts generate a list de possible directions (the `directions` array)
et alors filter those that are dans the direction de Pacman et choose one de the options. If they
are stuck et cannot go dans any way, they stay where they are.
*)
laisser chooseDirection (ghost:Ghost) =
  laisser x,y = ghost.X, ghost.Y
  laisser dx,dy = ghost.V
  // Are we facing towards the given point?
  laisser isBackwards (a,b) =
    (a <> 0 && a = -dx) || (b <> 0 && b = -dy)
  // Generate array avec possible directions
  laisser directions =
    [|si canGoLeft(x,y) alors rendement (-1,0), fillLeft(x,y)
      si canGoDown(x,y) alors rendement (0,1), fillDown(x,y)
      si canGoRight(x,y) alors rendement (1,0), fillRight(x,y)
      si canGoUp(x,y) alors rendement (0,-1), fillUp(x,y) |]

  si ghost.IsReturning alors
    // Returning ghosts find the shortest way home
    laisser xs = directions |> Array.sortBy snd
    laisser v, n = xs.[0]
    si n = 0 alors ghost.IsReturning <- faux
    v
  autre
    // Other ghosts pick one direction twoards Pacman
    laisser xs =
      directions
      |> Array.map fst
      |> Array.filter (not << isBackwards)
    si xs.Length = 0 alors 0, 0
    autre
      laisser randomNum = System.Random().NextDouble()
      laisser i = randomNum * float xs.Length
      xs.[int (floor i)]

/// Count number de dots dans the maze
laisser countDots () =
  maze |> Array.sumBy (fon line ->
    line.ToCharArray()
    |> Array.sumBy (fonction '.' -> 1 | 'o' -> 1 | _ -> 0))

(**
## The game play fonction

Most de the Pacman game logic is wrapped dans the top-level `playLevel` fonction. This takes two functions - that are called
quand the game completes - et alors it initializes the world state et runs dans a loop until the fin de the game.
The following outlines the structure de the fonction:

    laisser playLevel (onLevelCompleted, onGameOver) =
      // (Create canvas, background et ghosts)
      // (Define the Pacman state)
      // (Move ghosts et Pacman)
      // (Detect pills et collisiions)
      // (Rendering everything dans the game)
      laisser réc update () =
        logic ()
        render ()
        si dotsLeft.Value = 0 alors onLevelCompleted()
        autsi energy.Value <= 0 alors onGameOver()
        autre window.setTimeout(update, 1000. / 60.) |> ignore

      update()

After defining all the helpers, the `update` fonction runs dans a loop (via a timer) until there are no dots
left ou until the Pacman is out de energy et alors it calls one de the continuations.

In the following 5 sections, we'll look at the 5 blocks de code that define the body de the fonction.
*)

laisser playLevel (onLevelCompleted, onGameOver) =
  (**
  ### Create canvas, background et ghosts
  In the first part, the fonction finds the `<canvas>` element, paints it avec black background et
  creates other graphical elements - namely the game background, ghosts et eyes:
  *)
  // Fill the canvas element
  laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
  canvas.width <- 256.
  canvas.height <- 256.
  laisser context = canvas.getContext_2d()
  context.fillStyle <- !^ "rgb(0,0,0)"
  context.fillRect (0., 0. , 256., 256.);
  laisser bonusImages =
    [| createImage Images._200; createImage Images._400;
       createImage Images._800; createImage Images._1600 |]

  // Load images pour rendering
  laisser background = createBackground()
  laisser ghosts = createGhosts(context)
  laisser blue,eyed = createImage Images.blue, createImage Images.eyed

  (**
  ### Define the Pacman state
  Next, we define the game state. Pacman game uses mutable state, so the following uses
  F# reference cells; `ref 0` creates a mutable cell containing `0`. Later, we will access
  the value by writing `score.Value` et mutate it by writing `score.Value <- score.Value + 1`.
  *)
  laisser pills = maze |> Array.map (fon line ->
    line.ToCharArray() |> Array.map id)
  laisser dotsLeft = ref (countDots())
  laisser score = ref 0
  laisser bonus = ref 0
  laisser bonuses = ref []
  laisser energy = ref 128
  laisser flashCountdown = ref 0
  laisser powerCountdown = ref 0
  laisser x, y = ref (16 * 8 - 7), ref (23 * 8 - 3)
  laisser v = ref (0,0)

  laisser moveGhosts () =
    ghosts |> Array.iter (fon ghost ->
      ghost.Move(chooseDirection ghost)
    )

  laisser movePacman () =
    // In which directions should pacman go?
    laisser inputs =
       [| si Keyboard.isPressed "ArrowUp" alors
            rendement canGoUp (x.Value,y.Value), (0,-1)
          si Keyboard.isPressed "ArrowDown" alors
            rendement canGoDown (x.Value,y.Value), (0,1)
          si Keyboard.isPressed "ArrowLeft" alors
            rendement canGoLeft (x.Value,y.Value), (-1,0)
          si Keyboard.isPressed "ArrowRight" alors
            rendement canGoRight (x.Value,y.Value), (1,0) |]
    // Can we continue dans the same direction?
    laisser canGoForward =
      correspondre v.Value avec
      | 0,-1 -> canGoUp(x.Value,y.Value)
      | 0,1  -> canGoDown(x.Value,y.Value)
      | -1,0 -> canGoLeft(x.Value,y.Value)
      | 1, 0 -> canGoRight(x.Value,y.Value)
      | _ -> faux
    // What nouvelle directions can we take?
    laisser availableDirections =
      inputs
      |> Array.filter fst
      |> Array.map snd
      |> Array.sortBy (fon v' -> v' = v.Value)
    si availableDirections.Length > 0 alors
      // Choose the first one, prefers no change
      v.Value <- availableDirections.[0]
    autsi inputs.Length = 0 || not canGoForward alors
      // There are no options - stop
      v.Value <- 0,0

    // Update X et Y accordingly
    laisser x',y' = wrap (x.Value,y.Value) v.Value
    x.Value <- x'
    y.Value <- y'

  // Check si Pacman eats a pill at current cell
  laisser eatPills () =
    laisser tx = int (floor(float ((x.Value+6)/8)))
    laisser ty = int (floor(float ((y.Value+6)/8)))
    laisser c = pills.[ty].[tx]
    si c = '.' alors
      // Eating a small pill increments the score
      pills.[ty].[tx] <- ' '
      clearCell background (tx,ty)
      score.Value <- score.Value + 10
      dotsLeft.Value <- dotsLeft.Value - 1
      Sound.play "Dot5"
    si c = 'o' alors
      // Eating a large pill turns on the power mode
      pills.[ty].[tx] <- ' '
      clearCell background (tx,ty)
      bonus.Value <- 0
      score.Value <- score.Value + 50
      powerCountdown.Value <- 250
      dotsLeft.Value <- dotsLeft.Value - 1
      Sound.play "Powerup"

  /// Are there any ghosts that collide avec Pacman?
  laisser touchingGhosts () =
    laisser px, py = x.Value, y.Value
    ghosts |> Array.filter (fon ghost ->
      laisser x,y = ghost.X, ghost.Y
      ((px >= x && px < x + 13) ||
       (x < px + 13 && x >= px)) &&
      ((py >= y && py < y + 13) ||
       (y < py + 13 && y >= py)) )

(**
The `collisionDetection` fonction implements the right response à collision avec a ghost:
*)
  /// Handle collision detections between Pacman et ghosts
  laisser collisionDetection () =
    laisser touched = touchingGhosts ()
    si touched.Length > 0 alors
      si powerCountdown.Value > 0 alors
        // Pacman is eating ghosts!
        touched |> Array.iter (fon ghost ->
          si not ghost.IsReturning alors
            Sound.play "EatGhost"
            ghost.IsReturning <- vraie
            laisser added = int (2. ** (float bonus.Value))
            score.Value <- score.Value + added * 200
            laisser image = bonusImages.[bonus.Value]
            bonuses.Value <- (100, (image, ghost.X, ghost.Y)) :: bonuses.Value
            bonus.Value <-  min 3 (bonus.Value + 1) )
      autre
        // Pacman loses energy quand hitting ghosts
        energy.Value <- energy.Value - 1
        si flashCountdown.Value = 0 alors Sound.play "Hurt"
        flashCountdown.Value <- 30
    si flashCountdown.Value > 0 alors flashCountdown.Value <- flashCountdown.Value - 1

  /// Updates bonus points
  laisser updateBonus () =
    laisser removals,remainders =
      bonuses.Value
      |> List.map (fon (count,x) -> count-1,x)
      |> List.partition (fst >> (=) 0)
    bonuses.Value <- remainders

(**
The logic is called from the following single `logic` fonction that includes all the checks:
*)
  laisser logic () =
    moveGhosts()
    movePacman()
    eatPills ()
    si powerCountdown.Value > 0 alors
        powerCountdown.Value <- powerCountdown.Value - 1
    collisionDetection()
    updateBonus ()

(**
### Rendering everything dans the game

When rendering everything dans the game, we first draw the background et alors we render
individual components. Those include the score, remaining energy, pacman, ghosts et bonuses.
Each de those is handled by a single nested fonction that are put together dans `render`.
We start avec Pacman et remaining energy:
*)
  laisser renderPacman () =
    laisser p = Images.imageAt(x,y,v)
    si (flashCountdown.Value >>> 1) % 2 = 0
    alors context.drawImage(!^ p, float x.Value, float y.Value)

  laisser renderEnergy () =
    context.fillStyle <- !^ "yellow"
    context.fillRect(120., 250., float energy.Value, 2.)
(**
The next three functions render ghosts, current score et bonuses:
*)
  laisser renderGhosts () =
    ghosts |> Array.iter (fon ghost ->
      laisser image =
        si ghost.IsReturning alors eyed
        autre
          si powerCountdown.Value = 0 alors ghost.Image
          autsi powerCountdown.Value > 100 ||
                ((powerCountdown.Value >>> 3) % 2) <> 0 alors blue
          autre ghost.Image
      context.drawImage(!^ image, float ghost.X, float ghost.Y) )

  laisser renderScore () =
    context.fillStyle <- !^ "white"
    context.font <- "bold 8px";
    context.fillText("Score " + (score.Value).ToString(), 0., 255.)

  laisser renderBonus () =
    bonuses.Value |> List.iter (fon (_,(image,x,y)) ->
      context.drawImage(!^ image, float x, float y))

  laisser render () =
    context.drawImage(!^ background, 0., 0.)
    renderScore ()
    renderEnergy ()
    renderPacman()
    renderGhosts ()
    renderBonus ()

  laisser réc update () =
    logic ()
    render ()
    si dotsLeft.Value = 0 alors onLevelCompleted()
    autsi energy.Value <= 0 alors onGameOver()
    autre window.setTimeout(update, 1000 / 60) |> ignore

  update()

(**
Game entry point
================
Now we have everything we need à start the game, so the last step is à define the
`levelCompleted` et `gameOver` functions (that are called quand the game ends), render
the starting state de the game (avec "CLICK TO START" text) et start the game!
*)
laisser réc game () =
  // Initialize keyboard et canvas
  Keyboard.reset()
  laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
  laisser context = canvas.getContext_2d()

  // A helper fonction à draw text
  laisser drawText(text,x,y) =
    context.fillStyle <- !^ "white"
    context.font <- "bold 8px";
    context.fillText(text, x, y)

  // Called quand level is completed
  laisser levelCompleted () =
    drawText("COMPLETED",96.,96.)
    window.setTimeout(game, 5000) |> ignore

  // Called quand the game ends
  laisser gameOver () =
    drawText("GAME OVER",96.,96.)
    window.setTimeout(game, 5000) |> ignore

  // Start a nouvelle game after click!
  laisser start () =
    laisser background = createBackground()
    context.drawImage(!^ background, 0., 0.)
    context.fillStyle <- !^ "white"
    context.font <- "bold 8px";
    drawText("CLICK TO START", 88., 96.)
    laisser mutable playing = faux
    canvas.addEventListener("click", fon _ ->
        si not playing alors
            playing <- vraie
            playLevel (levelCompleted, gameOver))

  // Resize canvas et get ready pour a game
  laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
  canvas.width <- 256.
  canvas.height <- 256.
  start()

// At the beginning, initialize keyboard & start the first game.
Keyboard.init ()
game ()