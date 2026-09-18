# What it measures

Stock builds the scatter of a terrain quad as one mesh per kind of scatter, each held by an object of its
own, a *holder* (`PQSMod_LandClassScatterQuad`).

Readings 1 to 5 compare a scene with itself. You load the same save several times, and at each load the
mod reads the same things at the same place: readings 1 to 4 on every quad carrying scatter around the
craft, reading 5 on the quad nearest to it. Nothing in the scene changed between two loads, so every one
of these readings should come out the same every time. Every position is read as a *height*: its distance
from the centre of the body, in double precision, from the exact position of that centre that KSP keeps
(`CelestialBody.position`).

Reading 6 is of another kind: taken during a flight, on a key of its own, it measures no height and checks
the pools the holders come from.

## 1. The centre of the quad

The position of the quad's transform, in Unity's terms.

## 2. The matrix of the quad

The translation of the quad's local to world matrix. Unity keeps both the centre of an object and its
matrix, computes them separately, and draws with the matrix.

Two heights alone do not tell whether two points are shifted against each other: at the same distance
from the centre of the body, they can still stand apart sideways. So the mod also measures how far the
matrix stands from the centre of the quad, in two parts: *up*, the gap in height, and *across*, the gap
sideways.

## 3. The centre of each holder

The position of the transform of each holder of the quad, one per kind of scatter.

## 4. The matrix of each holder

The translation of each holder's local to world matrix, the one its objects are drawn with, with its *up*
and *across* against the centre of the quad, as for the quad's own matrix.

## 5. The objects against the ground

For every object of scatter held by the quad nearest to the craft, at 10 of its vertices spread over the
whole object:

- **the vertex**, as drawn: the mod reads the object's shape from its mesh;
- **the ground under that vertex**: the height of the terrain collision surface, the surface a craft rests
  on, hit by a ray cast straight down from 100 m above that vertex, so that the ground is found even under
  an object sunk into it.

Every object of a kind is a copy of the same model, moved, turned and scaled. The 10 vertices are chosen
on that model, so they are the same ones in every object of that kind and at every load: the lowest vertex
of the model first, then, one after the other, the vertex furthest from all those already chosen. A model
with 10 vertices or fewer has all of them measured. Heights alone would not show an object that turns
about a point without moving it; spread over the whole object, the vertices show any part of it that
rises or sinks against the ground.

Stock sinks scatter partly into the ground on purpose, so the gap between a vertex and the ground, which
the log gives as well, says little on its own. What matters is whether it stays the same from one load to
the next.

## 6. The pools of holders

Stock does not make a holder for each quad. Each kind of scatter of a body keeps a *pool* of holders
(`PQSLandControl.LandClassScatter`): when a quad gets scatter of that kind, a holder is taken out of the
pool, and when the quad is destroyed, the holder is handed back. Free holders hang from one container
object of the pool. A mod that changes how scatter is placed can break that cycle with nothing to see on
screen: a holder left behind on a quad that went back to the terrain's cache of quads, or destroyed along
with it.

The mod reads each pool through its private fields, and counts the holders that break a rule stock keeps:

- a holder **in use** that no longer exists, has no quad, stands on a quad that is not active (stock
  clears that flag when it hands a quad back to its cache, to reuse it elsewhere), or on a quad of another
  body;
- a **free** holder that no longer exists, does not hang from the pool's container, or still has a quad;
- a holder found both in use and free;
- a list of the pool that disagrees with the pool's own count of it;
- a holder of the scene that belongs to **no pool**.

Where a free holder stands inside its container is not one of them: stock hands a holder back without
moving it, so it keeps the offset its last quad gave it.

## What to expect

In stock, none of the heights of readings 1 to 5 stays the same from one load to the next: the quads,
their holders, the vertices of every object and the ground under them all move. Installing
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) is not enough to keep them
equal: the quads' heights then come back the same at every load, but the holders' still do not, and the
objects still move against the ground. Reading 6, on the other hand, should break no rule. See
[What the readings show](what-the-readings-show.md).

## Precision

Unity gives centres, matrices and vertices as single precision world coordinates. The world origin stays
near the craft, so close to it they resolve a fraction of a millimetre, but the step of a `float` is
0.5 mm at 4 km from that origin and 1 mm at 8 km. The nearest quad is not affected; the quads further away
can be.
