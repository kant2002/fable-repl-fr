module Hokusai

(*
Hokusai et Julia: rendering fractals using HTML5 canvas
This demo is based on Tomas Petricek's F# Advent Calendar post that explores
Japanese art et renders The Great Wave by Hokusai using the Julia fractal.
*)

ouvrir Fable.Core
ouvrir Browser.Types
ouvrir Browser

taper Complex =
  | Complex de float * float
  /// Calculate the absolute value de a complex number
  statique membre Abs(Complex(r, i)) =
    laisser num1, num2 = abs r, abs i
    si (num1 > num2) alors
      laisser num3 = num2 / num1
      num1 * sqrt(1.0 + num3 * num3)
    autsi num2 = 0.0 alors
      num1
    autre
      laisser num4 = num1 / num2
      num2 * sqrt(1.0 + num4 * num4)
  /// Add real et imaginary components pointwise
  statique membre (+) (Complex(r1, i1), Complex(r2, i2)) =
    Complex(r1+r2, i1+i2)

module ComplexModule =
  /// Calculates nth power de a complex number
  laisser Pow(Complex(r, i), power) =
    laisser num = Complex.Abs(Complex(r, i))
    laisser num2 = atan2 i r
    laisser num3 = power * num2
    laisser num4 = num ** power
    Complex(num4 * cos(num3), num4 * sin(num3))

/// Constant that generates nice fractal
laisser c = Complex(-0.70176, -0.3842)

/// Generates sequence pour given coordinates
laisser iterate x y =
  laisser réc loop current = seq {
    rendement current
    rendement! loop (ComplexModule.Pow(current, 2.0) + c) }
  loop (Complex(x, y))

laisser countIterations max x y =
  iterate x y
  |> Seq.take (max - 1)
  |> Seq.takeWhile (fon v -> Complex.Abs(v) < 2.0)
  |> Seq.length

// Transition between colors dans 'count' steps
laisser (--) clr count = clr, count
laisser (-->) ((r1, g1, b1), count) (r2, g2, b2) = [
  pour c dans 0 .. count - 1 ->
    laisser k = c / count |> byte
    laisser mid v1 v2 =
      (v1 + (v2 - v1) * k)
    (mid r1 r2, mid g1 g2, mid b1 b2) ]

// Palette avec colors used by Hokusai
laisser palette =
  [| // 3x sky color & transition à light blue
     rendement! (245uy, 219uy, 184uy) --3--> (245uy, 219uy, 184uy)
     rendement! (245uy, 219uy, 184uy) --4--> (138uy, 173uy, 179uy)
     // à dark blue et alors medium dark blue
     rendement! (138uy, 173uy, 179uy) --4--> (2uy, 12uy, 74uy)
     rendement! (2uy, 12uy, 74uy)     --4--> (61uy, 102uy, 130uy)
     // à wave coloruy,  alors light blue & back à wave
     rendement! (61uy, 102uy, 130uy)  -- 8--> (249uy, 243uy, 221uy)
     rendement! (249uy, 243uy, 221uy) --32--> (138uy, 173uy, 179uy)
     rendement! (138uy, 173uy, 179uy) --32--> (61uy, 102uy, 130uy)
  |]

// Specifies what range de the set à draw
laisser w = -0.4, 0.4
laisser h = -0.95, -0.35

// Create bitmap that matches the size de the canvas
laisser width = 400.0
laisser height = 300.0


/// Set pixel value dans ImageData à a given color
laisser setPixel (img:ImageData) x y width (r, g, b) =
  laisser index = (x + y * int width) * 4
  img.data.[index+0] <- r
  img.data.[index+1] <- g
  img.data.[index+2] <- b
  img.data.[index+3] <- 255uy

/// Dynamic operator that returns HTML element by ID
laisser (?) (doc:Document) name :'R =
  doc.getElementById(name) :?> 'R

/// Render fractal asynchronously avec sleep after every line
laisser render () = async {
  // Get <canvas> element & create image pour drawing
  laisser canv : HTMLCanvasElement = document?canvas
  laisser ctx = canv.getContext_2d()
  laisser img = ctx.createImageData(float width, float height)

  // For each pixel, transform à the specified range
  // et get color using countInterations et palette
  pour x dans 0 .. int width - 1 faire
    pour y dans 0 .. int height - 1 faire
      laisser x' = (float x / width * (snd w - fst w)) + fst w
      laisser y' = (float y / height * (snd h - fst h)) + fst h
      laisser it = countIterations palette.Length x' y'
      setPixel img x y width (palette.[it])

    // Insert non-blocking waiting & update the fractal
    faire! Async.Sleep(1)
    ctx.putImageData(img, 0.0, 0.0) }

/// Setup button event handler à start the rendering
laisser go : HTMLButtonElement = document?go
go.addEventListener("click", fon _ ->
  render() |> Async.StartImmediate)