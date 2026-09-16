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

**The scatter against the ground.** For every object of the terrain quad nearest to the craft, the mod
reads the object's shape from its mesh, takes its lowest point, and measures how high that point is
above the ground right under it. The ground is the terrain collision surface, the one a craft rests on,
found by a ray cast straight down from 100 m above the point, so that it is found even under an object
sunk into it. Stock sinks scatter partly into the ground on purpose, so the value itself says little.
What matters is whether it stays the same from one load to the next.

**The holders against their quads.** Stock builds the scatter of a terrain quad as one mesh, held by an
object of its own (`PQSMod_LandClassScatterQuad`). The mod measures, for every holder, its position
minus the position of its quad, split into a vertical part and the rest.

**The holder's matrix against its position.** Unity gives a transform both a position and a local to
world matrix, and draws with the matrix. The mod measures the translation of the holder's matrix minus
its position, and the translation of the holder's matrix minus that of its quad's matrix: where the
scatter is drawn against where the ground is drawn.

**Where each holder hangs** in the scene hierarchy: under the terrain sphere, directly under its own
quad, or elsewhere.

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

In flight, a window shows one line per recorded reading. The **bottom line is the reading in
progress**, refreshed twice a second, and its *Record* button freezes it into the table. The table
survives scene changes, so the lines pile up as you reload. *Delete* removes a line, *Clear table* all
of them. All distances are in millimetres; `--` means there is nothing to show.

| column | meaning |
|---|---|
| **Quads** | number of terrain quads carrying scatter around the craft |
| **Nearest** | vertical gap between the holder and the quad whose centre is nearest to the craft, from their transform positions. *on quad* when that holder hangs directly under its quad: the gap then says nothing |
| **Rocks** | on that same quad, the height of the lowest point of each object above the ground under it, averaged over its objects |
| **Matrix** | for the holder of that quad (the first by name of scatter kind, when the quad carries several kinds), the translation of its local to world matrix minus its transform position, vertical part |
| **Rocks-Matrix** | **Rocks**, with each object's share of **Matrix** taken off: where the objects would stand if they were drawn at the holder's transform position |
| **Drawn** | for that same holder, the translation of its matrix minus the translation of its quad's matrix, vertical part: where the scatter is drawn against where the ground is drawn |
| **Lowest** | the most negative vertical gap between a holder and its quad, over all quads, holders hanging directly under their quad left out |
| **Highest** | the same, most positive |

Under the table: the name of the nearest quad, the distance from the craft to its centre, and how many
of its objects were measured. The distance is not the distance to the nearest object: a quad is a couple
of hundred metres wide or more. The quad has to be the same on every line for the lines to compare
anything. An object counted as without ground under it is one where the ray found no terrain, and it is
left out.

Then the **Holders** line:

```
Holders: <n> under the terrain sphere, <n> under their own quad, <n> elsewhere, <n> on pooled quads
```

The first three counts cover every holder measured. The last one counts holders found under quads that
KSP has put back into its pool of unused quads: they belong to no terrain and are not measured. It is
left out when the pool cannot be found.

Every recorded reading is also written to `KSP.log`, on lines starting with `[RockPrecisionFixDiag]`:
a summary line with every column, the **Holders** line, one line per holder (nearest first, then by name of scatter kind, tagged
`[sphere]`, `[own quad]` or `[elsewhere]`, with its gap to its quad, its matrix, its quad's matrix and
*Drawn*), then one line per object of the nearest quad. The longest gap between a holder and its quad,
all directions included, is in the log only. KSP overwrites that file each time it starts: copy it
before relaunching.

## The protocol

1. **Terrain scatter must be on**: *Settings → Graphics → Terrain Scatters*. The window says so when it
   is off.
2. **Land a craft where there is scatter.** Any craft, anywhere, as long as **Quads** is not zero.
   Scatter only exists on the most detailed terrain, and is only built below 200 m/s (the stock
   default). The simplest way is the debug menu: launch any craft, then `Alt+F12 → Cheats → Set
   Position`, either on another body or, with *Use middle click to set position* ticked, by
   middle-clicking a spot on the ground.
3. **Save once.**
4. **Load that save, wait until Quads stops changing, and press *Record*.** Quads are built over several
   frames after loading.
5. **Load the same save again**, and record again. Five or six lines.

Then compare the lines. **Rocks** answers the question on its own: constant over the loads, the scatter
is drawn at the same height against the ground every time; changing, it is not. **Rocks-Matrix** and
**Drawn** tell where a change comes from.

## Get it

Source: <https://github.com/lhervier/KSP-RockPrecisionFixDiag>

**Build.** It needs the .NET SDK and reads the KSP assemblies from your install. Set `KSPDIR` to your
KSP install folder and run `build.bat`. It compiles `GameData/RockPrecisionFixDiagMod/RockPrecisionFixDiagMod.dll`
and never touches your KSP install.

**Install.** Copy `GameData/RockPrecisionFixDiagMod` into the `GameData` of KSP. It runs on a stock
install, and needs neither Harmony nor any other mod: it only reads the scene.

## License

MIT
