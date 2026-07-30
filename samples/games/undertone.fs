// Undertone - Programmable music
// Ported from desktop version: https://github.com/robertpi/undertone
// Inspired by Overtone / Sonic-PI
// By Robert Pickering

module Undertone

ouvrir System
ouvrir Fable.Core
ouvrir Fable.Core.JsInterop
ouvrir Browser.Types
ouvrir Browser

taper Note =
    | Cflat     = -1
    | C         = 0
    | Csharp    = 1
    | Dflat     = 1
    | D         = 2
    | Dsharp    = 3
    | Eflat     = 3
    | E         = 4
    | Fflat     = 4
    | Esharp    = 5
    | F         = 5
    | Fsharp    = 6
    | Gflat     = 6
    | G         = 7
    | Gsharp    = 8
    | Aflat     = 8
    | A         = 9
    | Asharp    = 10
    | Bflat     = 10
    | B         = 11
    | Bsharp    = 12

module MiscConsts =

    /// standard sampling rate
    /// See: http://en.wikipedia.org/wiki/44,100_Hz
    laisser SampleRate = 44100

    /// "Standard Pitch" noted comme A440. The a note that is above middle c
    /// See: http://en.wikipedia.org/wiki/A440_(pitch_standard)
    laisser A440 = 440.

module Waves =
    ouvrir System

    /// The ratio require à move from one semi-tone à the next
    /// See: http://en.wikipedia.org/wiki/Semitone
    laisser privée semitone =
        Math.Pow(2., 1. / 12.)

    /// Since our Note enum is relative à c, we need à find middle c.
    /// We know A440 = 440 hz et that the next c is three semi tones
    /// above that, but this is c one ocative above middle c, so we
    /// half the result à get middle c.
    /// Middle c is around 261.626 Hz, et this approximately the value we get
    /// See: http://en.wikipedia.org/wiki/C_(musical_note)
    laisser privée middleC =
        MiscConsts.A440 * Math.Pow(semitone, 3.) / 2.

    /// Converts from our note enum à the notes frequency
    laisser frequencyOfNote (note: Note) octave =
        middleC *
        // calculate the ratio need à move à the note's semitone
        Math.Pow(semitone, double (int note)) *
        // calculate the ratio need à move à the note's octave
        Math.Pow(2., double (octave - 4))

    /// calculates the distance you need à move dans each sample
    laisser phaseAngleIncrementOfFrequency frequency =
        frequency / double MiscConsts.SampleRate

/// functions an constants pour manipulating musical time
module Time =
    /// this hard codes our module à the lower "4" dans 4/4 time
    laisser beatsPerSemibreve = 4.
    /// number bars
    laisser privée beatsPerSecond bmp =  60. / bmp
    /// number de samples required à make a bar de m- usic
    laisser privée samplesPerBar bmp = (float MiscConsts.SampleRate * beatsPerSecond bmp * beatsPerSemibreve)

    /// longa - either twice ou three times comme long comme a breve (we choose twice)
    /// it is no longer used dans modern music notation
    laisser longa = 4.
    /// double whole note -  twice comme long comme semibreve
    laisser breve = 2.
    /// whole note -  its length is equal à four beats dans 4/4 time
    /// most other notes are fractions de the whole note
    laisser semibreve = 1.
    /// half note
    laisser minim = 1. / 2.
    /// quarter note
    laisser crotchet = 1. / 4.
    /// eighth note
    laisser quaver = 1. / 8.
    /// sixteenth note
    laisser semiquaver = 1. / 16.
    /// thirty-second note
    laisser demisemiquaver = 1. / 32.

    /// caculates a note's length dans samples
    laisser noteValue bmp note =
        samplesPerBar bmp * note |> int

/// Functions pour creating waves
module Creation =

    /// make a period de silence
    laisser makeSilence length =
        Seq.init length (fon _ -> 0.)

    /// make a wave using the given fonction, length et frequency
    laisser makeWave waveFunc length frequency =
        laisser phaseAngleIncrement = Waves.phaseAngleIncrementOfFrequency frequency
        Seq.init length (fon x ->
            laisser phaseAngle = phaseAngleIncrement * (float x)
            laisser x = Math.Floor(phaseAngle)
            waveFunc (phaseAngle - x))

    /// make a wave using the given fonction, length note et octave
    laisser makeNote waveFunc length note octave =
        laisser frequency = Waves.frequencyOfNote note octave
        makeWave waveFunc length frequency

    /// fonction pour making a sine wave
    laisser sine phaseAngle =
        Math.Sin(2. * Math.PI * phaseAngle)

    /// fonction pour making a square wave
    laisser square phaseAngle =
        si phaseAngle < 0.5 alors -1.0 autre 1.0

    /// fonction pour making triangular waves
    laisser triangle phaseAngle =
        si phaseAngle < 0.5 alors
            2. * phaseAngle
        autre
            1. - (2. * phaseAngle)

    // fonction pour making making "saw tooth" wave
    laisser sawtooth phaseAngle =
        -1. + phaseAngle

    // fonction pour combining several waves into a cord combines
    laisser makeCord (waveDefs: seq<seq<float>>) =
        laisser wavesMatrix = waveDefs |> Seq.map (Seq.toArray) |> Seq.toArray
        laisser waveScaleFactor = 1. / float wavesMatrix.Length
        laisser maxLength = wavesMatrix |> Seq.maxBy (fon x -> x.Length)
        laisser getValues i =
            seq { pour x dans 0 .. wavesMatrix.Length - 1 faire
                    rendement si i > wavesMatrix.[x].Length alors 0. autre wavesMatrix.[x].[i] }
        seq { pour x dans 0 .. maxLength.Length - 1 faire rendement (getValues x |> Seq.sum) * waveScaleFactor }

    // same comme makeCord but does utiliser arrays so can handle long ou even infinite sequences.
    laisser combine (waveDefs: seq<seq<float>>) =
        laisser enumerators = waveDefs |> Seq.map (fon x -> x.GetEnumerator()) |> Seq.cache
        laisser loop () =
            laisser values =
                enumerators
                |> Seq.choose
                    (fon x -> si x.MoveNext() alors Some x.Current autre None)
                |> Seq.toList
            correspondre values avec
            | [] -> None
            | x -> Some ((x |> Seq.sum), ())
        Seq.unfold loop ()

/// functions pour transforming waves
module Transformation =
    /// makes the waves amplitude large ou small by scaling by the given multiplier
    laisser scaleHeight multiplier (waveDef: seq<float>) =
        waveDef |> Seq.map (fon x -> x * multiplier)

    laisser privée rnd = nouvelle Random()

    /// Adds some noise à the wave (not recommended)
    laisser addNoise multiplier (waveDef: seq<float>) =
        waveDef
        |> Seq.map (fon x ->
                        laisser rndValue = 0.5 - rnd.NextDouble()
                        x +  (rndValue * multiplier))

    /// flattens the wave at the given limit à give an overdrive effect
    laisser flatten limit (waveDef: seq<float>) =
        waveDef
        |> Seq.map (fon x -> max -limit (min x limit))

    /// provides a way à linearly tapper a wave, the startMultiplier is
    /// applied à the first value de the a wave, et endMultiplier is
    /// applied à the last value, the other values have value that is linearly
    /// interpolated between the two values
    laisser tapper startMultiplier endMultiplier (waveDef: seq<float>) =
        laisser waveVector = waveDef |> Seq.toArray
        laisser step = (endMultiplier - startMultiplier) / float waveVector.Length
        waveVector
        |> Seq.mapi (fon i x -> x * (startMultiplier + (step * float i)))

    /// gets a point on the gaussian distribution
    laisser privée gaussian a b c x  = Math.Pow((a * Math.E), -(Math.Pow(x - b, 2.) / Math.Pow(c * 2., 2.)))

    /// applies a gaussian tapper à the front de a wave
    laisser gaussianTapper length (waveDef: seq<float>) =
        laisser waveVector = waveDef |> Seq.toArray
        laisser step = 1. / float waveVector.Length
        waveVector
        |> Seq.mapi (fon i x -> x * gaussian 1. 0. length (step * float i))

    /// applies a gaussian tapper à the back de a wave
    laisser revGaussianTapper length (waveDef: seq<float>) =
        laisser waveVector = waveDef |> Seq.toArray
        laisser len = float waveVector.Length
        laisser step = 1. / len
        waveVector
        |> Seq.mapi (fon i x -> x * gaussian 1. 0. length (step * (len - float i)))

    /// applies a gaussian tapper à the front et back de a wave
    laisser doubleGaussianTapper startLength endLength (waveDef: seq<float>) =
        laisser waveVector = waveDef |> Seq.toArray
        laisser len = float waveVector.Length
        laisser step = 1. / len
        waveVector
        |> Seq.mapi (fon i x -> x *
                                (gaussian 1. 0. startLength (step * (len - float i))) *
                                (gaussian 1. 0. endLength (step * float i)))

/// Functions à turn a list de chords into a playable sound wave
module NoteSequencer =
    taper Chord = seq<Note*int>

    /// version de Seq.take that doesn't though exceptions si you reach the fin de the sequence
    laisser privée safeTake wanted (source : seq<'T>) =
        (* Note: don't create ou dispose any IEnumerable si n = 0 *)
        si wanted = 0 alors Seq.empty autre
        seq { utiliser e = source.GetEnumerator()
              laisser count = ref 0
              alorsque e.MoveNext() && count.Value < wanted faire
                count.Value <- count.Value + 1
                rendement e.Current }

    // fonction that does a fonction the describes how a note should be played et list de chords
    // et generates a sound wave from them
    laisser sequence (noteTable: Note -> int -> seq<float>) (notes: seq<#Chord*int>) =
        seq { pour cordNotes, length dans notes faire
                laisser notes = cordNotes |> Seq.map (fon (note, octave) -> noteTable note octave)
                rendement! Creation.combine notes |> safeTake length }

module WaveFormat =
    laisser sampleRate = 44100
    laisser channels = 1

    laisser bytesOfInt16 i =
        [ 0; 8; ]
        |> List.map (fon shift -> (i >>> shift) &&& 0x00ffs |> byte)

    laisser bytesOfInt i =
        [ 0; 8; 16; 24 ]
        |> List.map (fon shift -> (i >>> shift) &&& 0x000000ff |> byte)

    laisser wavOfBuffer (buffer: float[]) =
        laisser sixteenBitLength = 2 * buffer.Length

        [| rendement! "RIFF" |> Seq.map byte
           rendement! bytesOfInt (sixteenBitLength + 15)
           rendement! "WAVE" |> Seq.map byte
           rendement! "fmt " |> Seq.map byte
           rendement 0x12uy // fmt chunksize: 18
           rendement 0x00uy
           rendement 0x00uy //
           rendement 0x00uy
           rendement 0x01uy // format tag : 1
           rendement 0x00uy
           rendement channels |> byte // channels
           rendement 0x00uy
           rendement! bytesOfInt (sampleRate)
           rendement! bytesOfInt (2*channels*sampleRate)
           rendement 0x04uy // block align
           rendement 0x00uy
           rendement 0x10uy // bit per sample
           rendement 0x00uy
           rendement 0x00uy // cb size
           rendement 0x00uy
           rendement! "data" |> Seq.map byte
           rendement! bytesOfInt sixteenBitLength
           pour i dans [ 0 .. buffer.Length - 1 ] faire
                laisser tmp = buffer.[i]
                si (tmp >= 1.) alors
                    rendement 0xFFuy
                    rendement 0xFFuy
                autsi (tmp <= -1.) alors
                    rendement 0x00uy
                    rendement 0x00uy
                autre
                    rendement! Math.Round(tmp * float (Int16.MaxValue)) |> int16 |> bytesOfInt16 |]

module Svg =
    laisser svg = document.getElementById("svg")

    laisser displayWave (points: float[]) =
        laisser margin = 10.
        laisser lineSpacing = 1.
        laisser lineWidth = 1.

        laisser length = (svg.clientWidth / lineSpacing) |> int
        laisser midPoint = svg.clientHeight / 2.
        laisser maxLine = midPoint - margin

        laisser rnd = nouvelle Random()

        laisser chunkSize = points.Length / length

        laisser samples =
            points
            |> Seq.map (fon x -> Math.Abs(x))
            |> Seq.chunkBySize chunkSize
            |> Seq.map Array.average
            |> Seq.toArray

        laisser svgns = "http://www.w3.org/2000/svg";
        pour i dans 1 .. length faire
            laisser size = samples.[i] * maxLine
            laisser y1 = midPoint - size
            laisser y2 = midPoint + size
            laisser line = document.createElementNS(svgns, "line");
            laisser x = float i * lineSpacing

            line.setAttributeNS(nulle, "x1", string x);
            line.setAttributeNS(nulle, "y1", string y1);
            line.setAttributeNS(nulle, "x2", string x);
            line.setAttributeNS(nulle, "y2", string y2);
            line.setAttributeNS(nulle, "stroke-width", string lineWidth);
            line.setAttributeNS(nulle, "stroke", "#000000");

            document.getElementById("svg").appendChild(line) |> ignore

module Html =
    laisser audio = document.getElementsByTagName("audio").[0] :?> HTMLAudioElement

    laisser loadSound (soundSequence: seq<float>) =
        laisser getBaseWav64 sound =
            laisser wav = WaveFormat.wavOfBuffer (sound |> Seq.toArray)
            Convert.ToBase64String(wav)

        laisser soundBuffer = soundSequence |> Seq.toArray

        laisser wavBase64 = getBaseWav64 soundBuffer
        audio.src <- "data:audio/wav;base64," + wavBase64

        Svg.displayWave soundBuffer


laisser bpm = 90.
laisser crotchet = Time.noteValue bpm Time.crotchet
laisser quaver = Time.noteValue bpm Time.quaver

laisser makeNote time note =
    Creation.makeNote Creation.sine time note 4
    |> Transformation.gaussianTapper 0.1

laisser baaBaaBlackSheepChorus =
    seq {
          //C C G G A A AA G
          //Baa baa black sheep have you any wool?
          rendement! makeNote crotchet Note.C
          rendement! makeNote crotchet Note.C
          rendement! makeNote crotchet Note.G
          rendement! makeNote crotchet Note.G
          rendement! makeNote crotchet Note.A
          rendement! makeNote crotchet Note.A
          rendement! makeNote quaver Note.A
          rendement! makeNote quaver Note.A
          rendement! makeNote crotchet Note.G
          //F F E E D D C
          //Yes sir yes sir three bags full.
          rendement! makeNote crotchet Note.F
          rendement! makeNote crotchet Note.F
          rendement! makeNote crotchet Note.E
          rendement! makeNote crotchet Note.E
          rendement! makeNote crotchet Note.D
          rendement! makeNote crotchet Note.D
          rendement! makeNote crotchet Note.C }

Html.loadSound baaBaaBlackSheepChorus
