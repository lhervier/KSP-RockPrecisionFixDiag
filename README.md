# Rock Precision Fix Diag

A measuring instrument for KSP 1.12. It lets you check, on your own install, a claim about the terrain
scatter drawn around your craft — the rocks, and around the KSC the grass and the trees:

> **KSP never draws terrain scatter at the same height against the ground twice.** Load the same save
> several times, and every rock, tuft of grass or tree comes back drawn a little higher or a little
> lower against the ground each time — several centimetres apart on Kerbin.

**How this was made.** Written with Claude, Anthropic's AI assistant, and reviewed line by line by a
human — me. I am saying so up front, because contributions made with an AI deserve a closer
look than others, and because some people would rather stop reading here. This mod measures and fixes
nothing, so what there is to check is the reading itself: the source is public, and the protocol below
runs on a stock install, wherever you land.

## Why it matters

It barely does. Stock scatter has no collider: nothing rests on it and nothing hits it, so a rock drawn a
few centimetres higher or lower than at the last load changes nothing for your craft. Scatter is also
sunk partly into the ground on purpose, so the shift is hard to see, and most of the time you will not
see it at all.

This instrument exists for another reason. [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix)
changes the height at which KSP builds the ground, and checking a change like that means checking
everything placed against that ground. Scatter is one of those things, so this instrument is meant to be
run on stock and with Terrain Precision Fix installed. The readings taken both ways, on the same save, are
kept under [perfs](perfs/README.md) and summed up under [What the readings show](#what-the-readings-show). Why stock draws scatter off the ground is on
[Rock Precision Fix's page](https://github.com/lhervier/KSP-RockPrecisionFix/blob/main/README.md#the-problem).
This page sticks to how to measure it.

## What it measures

The instrument compares a scene with itself. You load the same save several times, and at each load it
reads the same things at the same place. Nothing in the scene changed between two loads, so every one of
these readings should come out the same every time.

Stock builds the scatter of a terrain quad as one mesh per kind of scatter, each held by an object of its
own, a *holder* (`PQSMod_LandClassScatterQuad`). Every position is read as a *height*: its distance from
the centre of the body, in double precision, from the exact position of that centre that KSP keeps
(`CelestialBody.position`).

At each load, for every quad carrying scatter around the craft, the mod reads:

1. **the centre of the quad** (the position of its transform, in Unity's terms);
2. **the matrix of the quad**: the translation of its local to world matrix. Unity keeps both the centre
   of an object and its matrix, computes them separately, and draws with the matrix;
3. **the centre of each holder of the quad**, one per kind of scatter;
4. **the matrix of each of these holders**.

These four heights should be exactly the same from one load to the next. In stock, they are not.

Two heights alone do not tell whether two points are shifted against each other: at the same distance
from the centre of the body, they can still stand apart sideways. So for each matrix, the quad's and each
holder's, the mod also measures how far it stands from the centre of the quad, in two parts: *up*, the
gap in height, and *across*, the gap sideways.

5. Then, for every object of scatter held by the quad nearest to the craft:
   - **its lowest point**: the mod reads the object's shape from its mesh and takes its lowest point as
     drawn, the vertex nearest to the centre of the body;
   - **the ground under that point**: the height of the terrain collision surface, the surface a craft
     rests on, hit by a ray cast straight down from 100 m above that point, so that the ground is found
     even under an object sunk into it.

Here again, both heights should stay the same over the loads, and neither does: the lowest point moves at
every load, and so does the ground found under it. Stock sinks scatter partly into the ground on purpose,
so the gap between the two, which the log gives as well, says little on its own. What matters is whether
it stays the same from one load to the next.

**Precision.** Unity gives centres, matrices and vertices as single precision world coordinates. The world
origin stays near the craft, so close to it they resolve a fraction of a millimetre, but the step of a
`float` is 0.5 mm at 4 km from that origin and 1 mm at 8 km. The nearest quad is not affected; the
quads further away can be.

## The protocol

1. **Terrain scatter must be on**: *Settings → Graphics → Terrain Scatters*.
2. **Have KSP write its log at once**: in the debug menu (`Alt+F12 → Debugging`), tick the option that
   flushes the log instantly (`LOG_INSTANT_FLUSH` in `settings.cfg`). Otherwise KSP writes `KSP.log` in
   batches, and a record can wait there until more lines come. Follow the file as it grows, for
   instance with `tail -f KSP.log`.
3. **Land a craft**, any craft, on any body, preferably where there is scatter around it. Scatter only
   exists on the most detailed terrain, and is only built below 200 m/s (the stock default). The
   simplest way is the debug menu: launch any craft, then `Alt+F12 → Cheats → Set Position`, either on
   another body or, with *Use middle click to set position* ticked, by middle-clicking a spot on the
   ground.
4. **Save.**
5. **Load that save and press `Alt+F6`** (`Mod+F6`: the modifier key of the game). The reading is
   written to `KSP.log`, and its last line tells what it holds: how many quads carry scatter, how many
   holders are not built yet, and how many objects were measured on the nearest quad. Quads and
   their scatter are built over several frames after loading: if holders are not built yet, or the
   counts still grow from one press to the next, wait a few seconds and press again.
   Only the last record of each load is needed.
6. **Load the same save again, and press `Alt+F6` again.** As many times as needed: five or six loads
   are enough.

KSP overwrites `KSP.log` each time it starts: copy it before relaunching. Reloading the save from within
the game does not overwrite it, and the record numbers keep counting from one load to the next.

## The log

Every record is written on lines starting with `[RockPrecisionFixDiag]`, every height in metres, from
the centre of the body, to the micrometre: subtracting two of them gives millimetres. A record reads:

```
Record … on …: scatter on. Heights are …
  quad '…': height …, matrix …, up … mm, across … mm
    holder '…': built, height …, matrix …, up … mm, across … mm
    …
  …
  rock '…' '…' #0: ground …, lowest point …, … mm above the ground
  …
End of record …: … quads with rocks, … holders (… not built yet); nearest quad '…': … rocks, … without ground under them
```

- An opening line, with the body and whether scatter is on.
- **Every** quad carrying scatter around the craft, with the height of its centre, the height
  of its matrix, *up* and *across* in millimetres.
- Under each quad, each of its holders, one per kind of scatter, ordered by name: whether its objects are
  built yet, then the same heights, *up* and *across*, still against the centre of the quad.
- Then, for the nearest quad only, one line per object, holder after holder: the quad, the kind of
  scatter, the object's number, the height of the ground under it and of its lowest point, and the gap
  between the two in millimetres. An object without ground under it is one where the ray found no
  terrain: its ground and its gap read `--`. An object keeps its number from one load to the next, since
  stock places the scatter from a seed.
- A closing line, with the counts: it is the one left in sight when following the file as it grows.

A holder's matrix height minus its quad's height and its *up* are nearly the same number, by two
different routes: a difference of heights, and a projection on the vertical.

**Reading the records.** Compare the records of the successive loads on the nearest quad. Its name has to
be the same in every record for them to compare anything. Object by object, the same kind of scatter and
the same number, the height above the ground answers the question on its own: constant over the loads,
that object is drawn at the same height against the ground every time; changing, it is not. The heights
above it tell where a change comes from: the object's height above the ground minus its holder's *up*
stays constant when the change is the holder's matrix, and the holder's height minus its quad's height
tells whether its centre left its quad's too. The quad's own *up* and *across* tell whether the
ground is drawn away from the quad. The height of the quad itself may change from one load to the next as
well: comparing everything to it keeps that from blurring the rest.

## What the readings show

The readings kept under [perfs](perfs/README.md) follow the protocol above on a single save,
[`reference-kerbin.sfs`](perfs/reference-kerbin.sfs), loaded six times on stock and six times with
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) installed. All twelve records
hold the same 64 quads and 118 holders, and name the same nearest quad, `Kerbin Zn3010000130`, with the
same 218 objects: 200 `Grass00` and 18 `Tree00`. Below, the *range* of a reading is its largest value
minus its smallest over the six loads of a series.

| | stock | with Terrain Precision Fix |
|---|---|---|
| centre of the nearest quad, range | 126 mm | 0.001 mm |
| matrix of each quad against its centre | *up* and *across* 0.000 mm everywhere | the same |
| centre of each holder against its quad's | the same height, to the micrometre | range 137 mm (median over the holders), up to 201 mm |
| *up* of the holders' matrices | −84 to +85 mm, 0.000 mm a third of the time | −60 to +176 mm, never 0 |
| each object above the ground, range | 44 mm (median over the objects), from 27 to 116 mm | 129 mm (median), from 125 to 134 mm |
| the same minus its holder's *up*, range | 2 mm (median), but over 10 mm for 60 objects, up to 82 mm | 1.8 mm (median), 6.9 mm at most |

**Stock.** Every object of the nearest quad is drawn at a different height against the ground at every
load: 44 mm apart over the six loads for half of them. Most of it comes from its holder: the holder's
centre stays on its quad's, but the matrix it is drawn with stands above or below it by an amount that
changes at every load, and taking that *up* off leaves 2 mm for most objects. Not for all of them: the
ground itself moves against the centre of the quad from one load to the next, by 54 mm for half of the
objects, the objects mostly move along with it, and 60 of them keep more than 10 mm once their holder's
*up* is taken off.

**With Terrain Precision Fix.** The quads stop moving: the centre of the nearest quad comes back at the
same height to the micrometre, and the ground under each object moves by 7 mm at most against it. The
objects are still drawn at a different height against the ground at every load, and further apart than
in stock: 129 mm for half of them, about three times as much. All of it now comes from the holders: their
centres no longer stay on their quads', the *up* of their matrices spans a wider range, and once it is
taken off, no object moves by more than 7 mm.

**In short.** Stock is far from perfect, and Terrain Precision Fix makes it worse. Neither is a real
problem in play, though. Scatter has no collider: no craft rests on it and nothing hits it, so an object
drawn a few centimetres higher or lower than at the last load changes nothing for the game. And scatter is
sunk into the ground on purpose: on the nearest quad, the objects sit between 23 and 36 cm below the
ground on average, depending on the load, so a shift of a few centimetres mostly moves them within the
ground, where nobody sees it.

## Get it

Source: <https://github.com/lhervier/KSP-RockPrecisionFixDiag>

**Build.** It needs the .NET SDK and reads the KSP assemblies from your install. Set `KSPDIR` to your
KSP install folder and run `build.bat`. It compiles `GameData/RockPrecisionFixDiagMod/RockPrecisionFixDiagMod.dll`
and never touches your KSP install.

**Install.** Copy `GameData/RockPrecisionFixDiagMod` into the `GameData` of KSP. It runs on a stock
install, and needs neither Harmony nor any other mod: it only reads the scene.

## License

MIT
