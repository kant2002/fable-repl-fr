// Source: http://www.tryfsharp.org/create/cpoulain/shared/raytracer.fsx
// slightly modified à avoid some allocations

module RayTracer

[<Struct>]
taper Vector =
    { X: float; Y: float; Z: float }
    statique membre (*) (k, v: Vector) = { X = k * v.X; Y = k * v.Y; Z = k * v.Z }
    statique membre (-) (v1: Vector, v2: Vector) = { X = v1.X - v2.X; Y = v1.Y - v2.Y; Z = v1.Z - v2.Z }
    statique membre (+) (v1: Vector, v2: Vector) = { X = v1.X + v2.X; Y = v1.Y + v2.Y; Z = v1.Z + v2.Z }
    statique membre Dot (v1: Vector, v2: Vector) = v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z
    statique membre Mag (v: Vector) = sqrt (v.X * v.X + v.Y * v.Y + v.Z * v.Z)
    statique membre Norm (v: Vector) =
        laisser mag = Vector.Mag v
        laisser div = si mag = 0.0 alors infinity autre 1.0/mag
        div * v
    statique membre Cross (v1: Vector, v2: Vector) =
        { X = v1.Y * v2.Z - v1.Z * v2.Y
        ; Y = v1.Z * v2.X - v1.X * v2.Z
        ; Z = v1.X * v2.Y - v1.Y * v2.X }

[<Struct>]
taper Color =
    { R: float; G: float; B: float }
    statique membre Scale (k, v: Color) = { R = k * v.R; G = k * v.G; B = k * v.B }
    statique membre (+) (v1: Color, v2: Color) = { R = v1.R + v2.R; G = v1.G + v2.G; B = v1.B + v2.B }
    statique membre (*) (v1: Color, v2: Color) = { R = v1.R * v2.R; G = v1.G * v2.G; B = v1.B * v2.B }
    statique membre White = { R = 1.0; G = 1.0; B = 1.0 }
    statique membre Grey = { R = 0.5; G = 0.5; B = 0.5 }
    statique membre Black = { R = 0.0; G = 0.0; B = 0.0 }
    statique membre Background = Color.Black
    statique membre DefaultColor = Color.Black

taper Camera (pos: Vector, lookAt: Vector) =
    laisser forward = Vector.Norm (lookAt - pos)
    laisser down = { X = 0.0; Y = -1.0; Z = 0.0 }
    laisser right = 1.5 * Vector.Norm (Vector.Cross (forward, down))
    laisser up = 1.5 * Vector.Norm (Vector.Cross (forward, right))
    membre c.Pos     = pos
    membre c.Forward = forward
    membre c.Up      = up
    membre c.Right   = right

[<Struct>]
taper Ray =
    { Start: Vector;
      Dir: Vector }

taper Surface =
    abstraite Diffuse: Vector -> Color
    abstraite Specular: Vector -> Color
    abstraite Reflect: Vector -> float
    abstraite Roughness : float

[<Struct>]
taper Intersection =
    { Thing: SceneObject;
      Ray: Ray;
      Dist: float }

et SceneObject =
    abstraite Surface: Surface
    abstraite Intersect: Ray -> float
    abstraite Normal: Vector -> Vector

taper Light =
    { Pos : Vector;
      Color : Color }

taper Scene =
    { Things : SceneObject[];
      Lights : Light[];
      Camera : Camera }

module RayTracer =

    laisser maxDepth = 5

    laisser NearestIntersection ray scene =
        laisser mutable acc = None
        pour x dans scene.Things faire
            laisser dist = x.Intersect ray
            si acc.IsNone || dist < acc.Value.Dist alors
                acc <- Some { Thing = x; Ray = ray; Dist = dist }
        acc

    laisser TestRay ray scene =
        correspondre NearestIntersection ray scene avec
        | None -> None
        | Some isect ->
            si isect.Dist = infinity
            alors None
            autre Some isect.Dist

    laisser réc TraceRay ray scene (depth: int) =
        correspondre NearestIntersection ray scene avec
        | None -> Color.Background
        | Some isect ->
            si isect.Dist = infinity
            alors Color.Background
            autre Shade isect scene depth

    et Shade isect scene depth =
        laisser d = isect.Ray.Dir
        laisser pos = isect.Dist * d + isect.Ray.Start
        laisser normal = isect.Thing.Normal (pos)
        laisser reflectDir = d - 2.0 * Vector.Dot (normal, d) * normal
        laisser naturalcolor = Color.DefaultColor + (GetNaturalColor isect.Thing pos normal reflectDir scene)
        laisser reflectedColor =
            si depth >= maxDepth alors Color.Grey
            autre GetReflectionColor (isect.Thing, pos + (0.001*reflectDir), normal, reflectDir, scene, depth)
        naturalcolor + reflectedColor

    et GetReflectionColor (thing: SceneObject, pos, normal: Vector, rd: Vector, scene: Scene, depth: int) =
        Color.Scale (thing.Surface.Reflect (pos), TraceRay { Start = pos; Dir = rd } scene (depth + 1))

    et GetNaturalColor thing pos normal rd scene =
        laisser mutable color = Color.DefaultColor
        pour light dans scene.Lights faire
            color <- AddLight thing pos normal rd scene color light
        color

    et AddLight (thing: SceneObject) pos normal rd scene color light =
        laisser ldis = light.Pos - pos
        laisser livec = Vector.Norm (ldis)
        laisser neatIsect = TestRay { Start = pos; Dir = livec } scene
        laisser isInShadow =
            correspondre neatIsect avec
            | None -> faux
            | Some d -> not (d > Vector.Mag (ldis))
        si isInShadow alors color
        autre
            laisser illum = Vector.Dot (livec, normal)
            laisser lcolor =
                si illum > 0.0
                alors Color.Scale (illum, light.Color)
                autre Color.DefaultColor
            laisser specular = Vector.Dot (livec, Vector.Norm (rd))
            laisser scolor =
                si specular > 0.0
                alors Color.Scale (specular ** thing.Surface.Roughness, light.Color)
                autre Color.DefaultColor
            color + thing.Surface.Diffuse (pos) * lcolor +
                    thing.Surface.Specular (pos) * scolor

    laisser GetPoint x y width height (camera: Camera) =
        laisser RecenterX x =  (float x - (float width / 2.0))  / (2.0 * float width)
        laisser RecenterY y = -(float y - (float height / 2.0)) / (2.0 * float height)
        Vector.Norm (camera.Forward + RecenterX (x) * camera.Right + RecenterY (y) * camera.Up)

    laisser Render scene (data: byte[]) (x, y, width, height) =
        laisser clamp v = min (max (v * 255.0) 0.0) 255.0 |> byte
        pour y = y à height-1 faire
            laisser stride = y * width
            pour x = x à width-1 faire
                laisser index = (x + stride) * 4
                laisser dir = GetPoint x y width height scene.Camera
                laisser ray = { Start = scene.Camera.Pos; Dir = dir }
                laisser color = TraceRay ray scene 0
                data.[index+0] <- clamp color.R
                data.[index+1] <- clamp color.G
                data.[index+2] <- clamp color.B
                data.[index+3] <- 255uy

module SceneObjects =

    taper Sphere (center, radius, surface) =
        interface SceneObject avec
            membre this.Surface = surface
            membre this.Normal pos = Vector.Norm (pos - center)
            membre this.Intersect ray =
                laisser eo = center - ray.Start
                laisser v = Vector.Dot (eo, ray.Dir)
                laisser dist =
                    si (v < 0.0) alors infinity
                    autre
                        laisser disc = radius * radius - (Vector.Dot (eo,eo) - (v*v))
                        si disc < 0.0
                        alors infinity
                        autre v - (sqrt (disc))
                dist

    taper Plane (normal, offset, surface) =
        interface SceneObject avec
            membre this.Surface = surface
            membre this.Normal pos = normal
            membre this.Intersect ray =
                laisser denom = Vector.Dot (normal, ray.Dir)
                laisser dist =
                    si denom > 0.0
                    alors infinity
                    autre (Vector.Dot (normal, ray.Start) + offset) / (-denom)
                dist

module Surfaces =

    taper Shiny() =
        interface Surface avec
            membre s.Diffuse pos = Color.White
            membre s.Specular pos = Color.Grey
            membre s.Reflect pos = 0.7
            membre s.Roughness = 250.0

    taper Checkerboard() =
        interface Surface avec
            membre s.Diffuse pos =
                si (int (floor (pos.Z) + floor (pos.X))) % 2 <> 0
                alors Color.White
                autre Color.Black
            membre s.Specular pos = Color.White
            membre s.Reflect pos =
                si (int (floor (pos.Z) + floor (pos.X))) % 2 <> 0
                alors 0.1
                autre 0.7
            membre s.Roughness = 150.0

module Scenes =

    laisser TwoSpheresOnACheckerboard = {
        Things = [|
            SceneObjects.Plane ({ X = 0.0; Y = 1.0; Z = 0.0 }, 0.0, Surfaces.Checkerboard())
            SceneObjects.Sphere ({ X = 0.0; Y = 1.0; Z = -0.25 }, 1.0, Surfaces.Shiny())
            SceneObjects.Sphere ({ X = -1.0; Y = 0.5; Z = 1.5 }, 0.5, Surfaces.Shiny())
        |];
        Lights = [|
            { Pos = { X = -2.0; Y = 2.5; Z = 0.0 }; Color = { R = 0.49; G = 0.07; B = 0.07 } }
            { Pos = { X = 1.5; Y = 2.5; Z = 1.5 }; Color = { R = 0.07; G = 0.07; B = 0.49 } }
            { Pos = { X = 1.5; Y = 2.5; Z = -1.5 }; Color = { R = 0.07; G = 0.49; B = 0.071 } }
            { Pos = { X = 0.0; Y = 3.5; Z = 0.0 }; Color = { R = 0.21; G = 0.21; B = 0.35 } }
        |];
        Camera =
            Camera ({ X = 3.0; Y = 2.0; Z = 4.0 }, { X = -1.0; Y = 0.5; Z = 0.0 })
    }

ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

laisser renderScene scene (x, y, width, height) =
    laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
    laisser ctx = canvas.getContext_2d()
    laisser img = ctx.createImageData(float width, float height)
    RayTracer.Render scene img.data (x, y, width, height)
    ctx.putImageData(img, float -x, float -y)

laisser measure f x y =
    laisser dtStart = window?performance?now()
    laisser res = f x y
    laisser elapsed = window?performance?now() - dtStart
    res, elapsed

laisser x, y, w, h = (0, 0, 512, 512)
laisser _, elapsed = measure renderScene Scenes.TwoSpheresOnACheckerboard (x, y, w, h)
printfn "Ray tracing:\n - rendered image size: (%dx%d)\n - elapsed: %f ms" w h elapsed
