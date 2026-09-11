# Rock Offset Probe

A measuring instrument for KSP 1.12, companion to
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix). It tests one prediction:
**the terrain fix moves the ground, but not the rocks standing on it.**

## What it measures

Two things, independently of each other.

**The rocks against the ground.** For every rock of the terrain quad nearest to the craft, the probe
reads the shape of the rock from the game, takes its lowest point, and measures how high that point is
above the ground right under it. The ground is the terrain collision surface, the one a craft rests on,
found by a ray cast straight down from 100 m above the point, so that it is found even under a rock
sunk into it. Negative means the lowest point is below the ground. Stock sinks rocks partly into the
ground on purpose, so the value itself says little. What matters is whether it stays the same from one
loading to the next.

**The holders against the quads.** Stock KSP builds the rocks of a terrain quad as a single mesh, held
by an object of its own (`PQSMod_LandClassScatterQuad`). Each rock is placed between two vertices of the
quad, in the quad's own coordinates (`PQSLandControl.LandClassScatter.CreateScatterMesh`):

```csharp
scatterPos = Vector3.Lerp(q.quad.verts[num3], q.quad.verts[num2], UnityEngine.Random.value);
```

Those coordinates are then used as they are, relative to the holder. So the rocks follow the ground
exactly when the holder has the same origin as the quad. For every quad carrying rocks, the probe
measures the gap between the two origins: the position of the holder minus the position of the quad,
split into a vertical part, positive when the holder is above the quad, and the rest.

The first measurement is the symptom. The second is what the code says causes it, and the two can be
checked against each other: if the code is read right, a change in the gap of a quad moves its rocks
by the same amount against the ground.

## Why the fix should change them

Stock places the quad and the holder with the same line of code:

```csharp
// PQ.SetupQuad
quadTransform.localPosition = positionPlanet;
// PQSMod_LandClassScatterQuad.Setup
base.transform.localPosition = quad.positionPlanet;
```

Both hang from the terrain sphere, whose origin is the centre of the body, so both store the same
double precision vector, hundreds of kilometres long, in the same single precision field. The rounding
is the same for both. The rocks are off exactly as much as the ground is, so they stay right on it.

Terrain Precision Fix moves the quad to the position its double precision coordinates give, and leaves
the holder where stock put it. Hence two predictions, read from the code and not measured yet:

- **stock**: every gap close to zero, and the rocks at the same height against the ground on every
  loading;
- **with Terrain Precision Fix**: gaps the size of the stock error on the quad origins, centimetres on
  Kerbin, different from one quad to the next and from one loading to the next, and rocks whose height
  against the ground changes from one loading to the next by the gap of their quad.

In stock, rocks have no collider. So if the prediction holds, the fix leaves rocks floating above or
sunk into the ground, without any physical effect.

## Get it

Clone this repository, set `KSPDIR` to your KSP install folder and run `build.bat`. It needs the .NET
SDK and reads the KSP assemblies from your install. Then copy `GameData/RockOffsetProbeMod` into the
`GameData` of KSP. It runs on a stock install, with or without Terrain Precision Fix.

## The window

In flight, a window shows one line per recorded reading. The **bottom line is the reading in
progress**, refreshed twice a second, and its *Record* button freezes it into the table. The table
survives scene changes, so the lines pile up as you reload. All distances are in millimetres.

| column | meaning |
|---|---|
| **Quads** | number of terrain quads carrying rocks around the craft |
| **Nearest** | vertical gap between the holder and the quad whose centre is nearest to the craft |
| **Rocks** | on that same quad, the height of the lowest point of each rock above the ground under it, averaged over its rocks |
| **Matrix** | for the holder of that quad, the translation of its local to world matrix minus its transform position, vertical part. Unity computes the two separately; the rocks are drawn with the matrix |
| **Rocks-Matrix** | **Rocks**, with each rock's share of **Matrix** taken off: where the rocks would stand if they were drawn at the holder's transform position |
| **Lowest** | the most negative vertical gap between a holder and its quad, over all quads |
| **Highest** | the most positive vertical gap between a holder and its quad, over all quads |

The longest gap between a holder and its quad, all directions included, is in `KSP.log` only, along
with the same matrix measurement for every holder and every quad.

Under the table: the name of the nearest quad, the distance from the craft to its centre, and how many
of its rocks were measured. The distance is not the distance to the nearest rock: a quad is a couple of
hundred metres wide or more, so the craft can stand on it with its centre over a hundred metres away.
The quad has to be the same on every line for **Nearest** and **Rocks** to compare anything. A rock
counted as without ground under it is one where the ray found no terrain, and it is left out.

Every recorded reading is also written to `KSP.log`, quad by quad, then rock by rock for the nearest
quad, on lines starting with `[RockOffsetProbe]`. KSP overwrites that file each time it starts: copy it
before relaunching.

## The protocol

1. **Terrain scatter must be on**: *Settings → Graphics → Terrain Scatters*. The probe says so when it
   is off.
2. **Land a craft where there are rocks.** Any craft, anywhere, as long as **Quads** is not zero.
   Rocks only exist on the most detailed terrain, and are only built below 200 m/s. The simplest way
   is the debug menu: launch any craft, then `Alt+F12 → Cheats → Set Position`, either on another
   body or, with *Use middle click to set position* ticked, by middle-clicking a spot on the ground.
3. **Save once.**
4. **Load that save, wait until Quads stops changing, and press *Record*.** Quads are built over
   several frames after loading.
5. **Load the same save again**, and record again. Five or six lines.
6. **Install Terrain Precision Fix and do it all again**, with the same save.

Compare the two tables. **Rocks** answers the question on its own: stable over the lines of the stock
table, the rocks follow the ground; changing over the lines of the other, they do not. **Rocks** minus
**Nearest** checks the explanation: if the rocks are off by the gap of their holder and nothing else, it
is the same on every line of both tables. Exactly the same on flat ground; on a slope, the horizontal
part of the gap also moves each rock over slightly higher or lower ground.
