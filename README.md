# Rock Precision Fix Diag

**How this was made.** Written with Claude, Anthropic's AI assistant, and reviewed line by line by a
human — me. I am saying so before anything else, because contributions made with an AI deserve a closer
look than others, and because some people would rather stop reading here. This mod measures and fixes
nothing, so what there is to check is the reading itself: the source is public, and the protocol below
runs on a stock install, on your own craft, against the figures on this page.

A diagnostic mod for KSP 1.12. It shows that stock KSP does not draw terrain scatter (rocks, and around
the KSC grass and trees) on the ground: it draws it above or below the ground, by several centimetres on
Kerbin, and by a different amount at every load.

## What it measures

**Heights.** Every height is a distance from the centre of the body, in double precision, from the
exact position of that centre that KSP keeps (`CelestialBody.position`). Stock builds the scatter of a
terrain quad as one mesh per kind of scatter, each held by an object of its own
(`PQSMod_LandClassScatterQuad`). For every quad carrying scatter around the craft, and for every holder
of that quad, the mod reads two heights: one from the transform position, one from the translation of the
local to world matrix. Unity gives a transform both, computes them separately, and draws with the matrix.

**The matrix against the quad.** For the quad and for each of its holders, the translation of the
matrix minus the transform position of the quad, split in two: *up*, along the vertical of the quad, and
*across*, the length of what is left. For a holder, this is the gap that moves the scatter against the
ground, since the scatter is drawn from the holder's matrix and the ground is placed by its quad. For the
quad, it tells whether the ground itself is drawn where the quad stands. *Across* matters on a slope:
an object moved sideways by *d* on a slope of angle *θ* stands above a ground higher or lower by up to
*d* · tan *θ*.

**The scatter against the ground.** For every object held by the quad nearest to the craft, the mod
reads the object's shape from its mesh and takes its lowest point as drawn, the vertex nearest to the
centre of the body. It then casts a ray straight down from 100 m above that point, so that the ground
is found even under an object sunk into it, and reads the height of the terrain collision surface it
hits: the surface a craft rests on. Stock sinks scatter partly into the ground on purpose, so the gap
between the two heights says little on its own. What matters is whether it stays the same from one load
to the next.

**Precision.** The positions read from transforms are single precision world coordinates. The world
origin stays near the craft, so close to it they resolve a fraction of a millimetre, but the step of a
`float` is 0.5 mm at 4 km from that origin and 1 mm at 8 km. The nearest quad is not affected; the
quads further away can be.

## What is wrong in stock

Stock places the holder of a quad's scatter under an object that is itself a child of the terrain
sphere, whose origin is the centre of the body (`PQSMod_LandClassScatterQuad.Setup`):

```csharp
base.transform.localPosition = quad.positionPlanet;
```

`quad.positionPlanet` is hundreds of kilometres long. Each object is then placed between two vertices of
the ground mesh, in the quad's own coordinates (`PQSLandControl.LandClassScatter.CreateScatterMesh`):

```csharp
scatterPos = Vector3.Lerp(q.quad.verts[num3], q.quad.verts[num2], UnityEngine.Random.value);
```

So the scatter lies on the ground only if the holder is drawn exactly where the quad is.

The figures below were taken with an earlier version of this mod, which showed gaps rather than
heights; the names in italics are its columns. Its *Rocks* averaged every object of the nearest quad,
all holders together. Its *Matrix* was the translation of a holder's matrix minus the holder's own
transform position: wherever the holder has the same position as its quad, as it does here, that is the
holder's matrix height minus its quad's height in the current version.

**The positions agree.** On Gilly, in one reading over 128 quads carrying scatter, the holder and its
quad have the same transform position to the bit: 0.000 mm on every quad. On Kerbin, 0.000 mm on every quad as well,
over 64 quads, on each of six loads.

**The matrix does not.** Kerbin, stock, the same save loaded six times near the KSC. Nearest quad
`Kerbin Zn3010000130`, 218 objects measured: 200 `Grass00` and 18 `Tree00`.

| load | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| *Rocks* (mm) | −324.378 | −283.626 | −283.712 | −389.836 | −304.302 | −323.870 |
| *Matrix* (mm) | 0.000 | +40.707 | +40.791 | −64.503 | +20.497 | 0.000 |
| *Rocks − Matrix* (mm) | −324.378 | −324.335 | −324.505 | −325.332 | −324.799 | −323.870 |

*Rocks* spans 106 mm over the six loads; *Rocks − Matrix* stays within 1.5 mm. Object by object, the
median range over the loads is 106 mm, and 3.9 mm once the holder's *Matrix* is taken off. An earlier
series of six loads on the same quad gave a 119 mm range.

Over the 118 holders of 64 quads and the six loads, the translation of the holder's matrix minus its
transform position, vertical part, runs from −85.5 to +64.7 mm, with a standard deviation of 29.7 mm. It
is made of whole single precision steps along the world axes, projected on the vertical: 62.5 mm is the
step of a `float` at 600 km. It differs from one holder to the next and from one load to the next. For
the quads, the same measurement is 0.000 mm everywhere.

In plain words: the holder's transform position matches its quad, but the matrix Unity draws it with does
not, by whole float steps. So the scatter of each quad is drawn above or below the ground by its own
amount, and that amount changes at every load. Stock scatter has no collider: the defect is visual only.

About fifty of the 218 objects, those whose lowest point is 0.5 to 1.8 m away from the ground, keep a
residue of a few centimetres from one load to the next once *Matrix* is taken off. It is not explained.

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
- **Every** quad carrying scatter around the craft, with the height of its transform position, the height
  of its matrix, *up* and *across* in millimetres.
- Under each quad, each of its holders, one per kind of scatter, ordered by name: whether its objects are
  built yet, then the same heights, *up* and *across*, still against the transform position of the quad.
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
tells whether its transform position left its quad too. The quad's own *up* and *across* tell whether the
ground is drawn away from the quad. The height of the quad itself may change from one load to the next as
well: comparing everything to it keeps that from blurring the rest.

## Get it

Source: <https://github.com/lhervier/KSP-RockPrecisionFixDiag>

**Build.** It needs the .NET SDK and reads the KSP assemblies from your install. Set `KSPDIR` to your
KSP install folder and run `build.bat`. It compiles `GameData/RockPrecisionFixDiagMod/RockPrecisionFixDiagMod.dll`
and never touches your KSP install.

**Install.** Copy `GameData/RockPrecisionFixDiagMod` into the `GameData` of KSP. It runs on a stock
install, and needs neither Harmony nor any other mod: it only reads the scene.

## License

MIT
