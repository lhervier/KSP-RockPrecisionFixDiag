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

**The holder against its quad.** The translation of the holder's matrix minus the transform position of
its quad, split in two: *up*, along the vertical of the quad, and *across*, the length of what is left.
This is the gap that moves the scatter against the ground, since the scatter is drawn from the holder's
matrix and the ground is placed by its quad. *Across* matters on a slope: an object moved sideways by
*d* on a slope of angle *θ* stands above a ground higher or lower by up to *d* · tan *θ*.

**The scatter against the ground.** For every object held by the quad nearest to the craft, the mod
reads the object's shape from its mesh and takes its lowest point as drawn, the vertex nearest to the
centre of the body. It then casts a ray straight down from 100 m above that point, so that the ground
is found even under an object sunk into it, and reads the height of the terrain collision surface it
hits: the surface a craft rests on. Stock sinks scatter partly into the ground on purpose, so the gap
between the two heights says little on its own. What matters is whether it stays the same from one load
to the next.

**Where each holder hangs** in the scene hierarchy: under the terrain sphere, directly under its own
quad, or elsewhere.

**Precision.** The positions read from transforms are single precision world coordinates. The world
origin stays near the craft, so close to it they resolve a fraction of a millimetre, but the step of a
`float` is 0.5 mm at 4 km from that origin and 1 mm at 8 km. The nearest quad is not affected; the
extremes over every quad around can be.

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
holder's **Matrix** in the current version.

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

## The window

In flight, a window shows one block of lines per recorded reading. The **bottom block is the reading in
progress**, refreshed twice a second, and its *Record* button freezes it into the table. The table
survives scene changes, so the blocks pile up as you reload. *Delete* removes a block, *Clear table* all
of them. `--` means there is nothing to show.

A block reads:

| line | Where | Height | Matrix | Up | Across |
|---|---|---|---|---|---|
| the nearest quad, by name | distance from the craft to the quad's origin | height of the quad's transform position, in metres | height of the quad's matrix, in mm above the quad | | |
| each holder of that quad, by kind of scatter | where it hangs | height of the holder's transform position, in mm above the quad | height of the holder's matrix, in mm above the quad | *up*, in mm | *across*, in mm |
| under each holder, its objects: how many were measured, how many had no ground under them | | height of their lowest point above the ground under each, averaged, in mm | | | |
| all the quads around, and their holders | | | | lowest and highest *up*, in mm | largest *across*, in mm |

Everything but the height of the quad is measured against that height, so that millimetres can be read
next to hundreds of kilometres. A holder's **Matrix** and its **Up** are nearly the same number, by two
different routes: a difference of heights, and a projection on the vertical. The distance to the quad is
not the distance to the nearest object: a quad is a couple of hundred metres wide or more. The quad has
to be the same in every block for the blocks to compare anything. An object without ground under it is
one where the ray found no terrain, and it is left out of the average. Objects are only read once KSP has
built them.

Under the table, the **Holders** line:

```
Holders: <n> under the terrain sphere, <n> under their own quad, <n> elsewhere, <n> on pooled quads
```

The first three counts cover every holder measured. The last one counts holders found under quads that
KSP has put back into its pool of unused quads: they belong to no terrain and are not measured. It is
left out when the pool cannot be found.

Every recorded reading is also written to `KSP.log`, on lines starting with `[RockPrecisionFixDiag]`,
with every height in metres, from the centre of the body, to the micrometre: subtract two of them and
you get back the millimetres of the window. First a summary line with the extremes, and the **Holders**
line. Then **every** quad around, nearest first, with its two heights; under each quad, each of its
holders, tagged `[sphere]`, `[own quad]` or `[elsewhere]`, with its two heights, *up* and *across*;
and, for the nearest quad only, under each holder the average of its objects, then one line per object
with the height of the ground and of its lowest point. An object keeps its number from one load to the
next: stock places the scatter from a seed. KSP overwrites that file each time it starts: copy it before
relaunching.

## The protocol

1. **Terrain scatter must be on**: *Settings → Graphics → Terrain Scatters*. The window says so when it
   is off.
2. **Land a craft where there is scatter.** Any craft, anywhere, as long as the window shows quads.
   Scatter only exists on the most detailed terrain, and is only built below 200 m/s (the stock
   default). The simplest way is the debug menu: launch any craft, then `Alt+F12 → Cheats → Set
   Position`, either on another body or, with *Use middle click to set position* ticked, by
   middle-clicking a spot on the ground.
3. **Save once.**
4. **Load that save, wait until the number of quads stops changing and the objects are read, and press
   *Record*.** Quads and their scatter are built over several frames after loading.
5. **Load the same save again**, and record again. Five or six blocks.

Then compare the blocks, holder by holder. The line of objects under a holder answers the question on
its own: constant over the loads, that scatter is drawn at the same height against the ground every
time; changing, it is not. The heights above it tell where a change comes from: the objects' line minus
the holder's **Matrix** stays constant when the change is the holder's matrix, and the holder's
**Height** tells whether its transform position left its quad too. The height of the quad itself may
change from one load to the next as well: every other value is measured against it, so that does not
blur them.

## Get it

Source: <https://github.com/lhervier/KSP-RockPrecisionFixDiag>

**Build.** It needs the .NET SDK and reads the KSP assemblies from your install. Set `KSPDIR` to your
KSP install folder and run `build.bat`. It compiles `GameData/RockPrecisionFixDiagMod/RockPrecisionFixDiagMod.dll`
and never touches your KSP install.

**Install.** Copy `GameData/RockPrecisionFixDiagMod` into the `GameData` of KSP. It runs on a stock
install, and needs neither Harmony nor any other mod: it only reads the scene.

## License

MIT
