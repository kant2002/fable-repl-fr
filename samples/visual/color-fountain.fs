module Color.Fountain

// Color Fountain by Erik Novales: https://github.com/enovales

ouvrir Fable.Core
ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

laisser canvas = document.getElementsByTagName("canvas").[0] :?> HTMLCanvasElement
canvas.width <- 1000.
canvas.height <- 800.
laisser ctx = canvas.getContext_2d()

laisser rng (): float = JS.Math.random()

laisser particleLimit = 200

taper Particle = {
    x: double
    y: double
    xvel: double
    yvel: double
    c: (int * int * int)
    rot: double
    rotVel: double
}
avec
    passeroutre this.ToString() =
        laisser (r,g,b) = this.c
        sprintf "Particle(x = %O, y = %O, xvel = %O, yvel = %O, c = (%O, %O, %O))"
            this.x this.y this.xvel this.yvel r g b


laisser updateParticle(dt: double)(p: Particle) =
    {
        p avec
            x = p.x + p.xvel * dt
            y = p.y + p.yvel * dt
            yvel = p.yvel + 1. * dt
            rot = (p.rot + p.rotVel * dt) % (2. * 3.14159)
    }

laisser refillParticles(p: Particle array, dt: double) =
    laisser stillValid =
        p |> Array.filter(fon pt -> (pt.y < 1000.))
    //System.Console.WriteLine("stillValid.Length = " + stillValid.Length.ToString())
    laisser updatedPos =
        stillValid
        |> Array.map(updateParticle(dt))

    //System.Console.WriteLine("updatedPos = " + updatedPos |> Array.map(fon p -> p.ToString()).ToString())
    laisser toCreate = particleLimit - stillValid.Length
    //System.Console.WriteLine("going à create " + toCreate.ToString() + " particles")
    laisser newParticles =
        seq {
            pour i dans 0..toCreate faire
                rendement {
                    Particle.x = 200.
                    y = 300.
                    xvel = (rng() - 0.5) * (rng() * 30.)
                    yvel = -(rng() * 25.)
                    c = (int (rng() * 255.), int (rng() * 255.), int (rng() * 255.))
                    rot = (rng() * 2. * 3.14159)
                    rotVel = (rng() * 1.5)
                }
        }
        |> Seq.toArray

    updatedPos |> Array.append(newParticles)

laisser mutable particles = [||]
laisser timestep = 0.8

laisser réc loop last t =
    // Comment out this line à make sure the animation runs
    // avec same speed on different frame rates
    // laisser timestep = (t - last) / 20.
    particles <- refillParticles(particles, timestep)

    ctx.clearRect(0., 0., 10000., 10000.)
    laisser drawParticle(p: Particle) =
        laisser (r,g,b) = p.c
        laisser fs = "rgb(" + r.ToString() + ", " + g.ToString() + ", " + b.ToString() + ")"
        ctx.fillStyle <- !^fs

        laisser x1 = (p.x - 5.)
        laisser x2 = (p.x + 5.)
        laisser y1 = (p.y - 5.)
        laisser y2 = (p.y + 5.)

        // laisser x1 = (p.x - (10. * System.Math.Cos(p.rot)))
        // laisser x2 = (p.x + (10. * System.Math.Cos(p.rot)))
        // laisser y1 = (p.y - (10. * System.Math.Sin(p.rot)))
        // laisser y2 = (p.y + (10. * System.Math.Sin(p.rot)))

        // ctx.fillRect(x1, y1, 10., 10.)
        ctx.beginPath()
        ctx.moveTo(x1, y1)
        ctx.lineTo(x2, y1)
        ctx.lineTo(x2, y2)
        ctx.lineTo(x1, y2)
        ctx.lineTo(x1, y1)
        ctx.closePath()
        ctx.fill()

    particles
    |> Array.iter drawParticle

    window.requestAnimationFrame(loop t) |> ignore

// start the loop
loop 0. 0.