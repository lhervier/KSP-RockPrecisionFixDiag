# Measuring the rocks

How to take readings 1 to 5 of [What it measures](what-it-measures.md): the quads, their holders and the
objects of scatter against the ground, load after load, or along a flight.

## Load after load

1. **Terrain scatter must be on**: *Settings → Graphics → Terrain Scatters*.
2. **Have KSP write its log at once**: in the debug menu (`Alt+F12 → Debugging`), tick the option that
   flushes the log instantly (`LOG_INSTANT_FLUSH` in `settings.cfg`). Otherwise KSP writes `KSP.log` in
   batches, and a record can wait there until more lines come. Follow the file as it grows, for
   instance with `tail -f KSP.log`.
3. **Land a craft**, any craft, on any body, preferably where there is scatter around it. Scatter only
   exists on the most detailed terrain. The simplest way is the debug menu: launch any craft, then
   `Alt+F12 → Cheats → Set Position`, either on another body or, with *Use middle click to set position*
   ticked, by middle-clicking a spot on the ground.
4. **Save.**
5. **Load that save and press `Alt+F6`** (`Mod+F6`: the modifier key of the game). The reading is
   written to `KSP.log`, and its last line tells what it holds: how many quads carry scatter, how many
   holders are not built yet, and how many objects and vertices were measured on the nearest quad. Quads and
   their scatter are built over several frames after loading: if holders are not built yet, or the
   counts still grow from one press to the next, wait a few seconds and press again.
   Only the last record of each load is needed.
6. **Load the same save again, and press `Alt+F6` again.** A dozen times: how far things move changes
   from one load to the next, and a handful of loads can come out small or large by chance.

KSP overwrites `KSP.log` each time it starts: copy it before relaunching. Reloading the save from within
the game does not overwrite it, and the record numbers keep counting from one load to the next.

## Over a flight

The protocol above reads scatter built right after a load, around a craft that does not move. Over a
flight, the world origin follows the craft, and the terrain keeps building the quads ahead of it and
destroying those behind it: reading the rocks along a flight shows where they are drawn once they have
come through that.

1. **Scatter on, and the log written at once**: steps 1 and 2 of [load after load](#load-after-load).
2. **Load [`ref-mune-5km.sfs`](../diag/ref-mune-5km.sfs)**, copied into the folder of a sandbox game: a
   Mk1 command pod in a circular equatorial orbit 5 km over the Mun. Some of the Mun's relief rises above
   the orbit, so the pod crashes after a while.
3. **Press `Alt+F6`** 30 s into the flight, then again every two minutes, and once more after the crash.
4. **Copy `KSP.log`** before relaunching KSP.

Each record reads on its own: the heights of every quad and of its holders, as after a load. To compare
two installs, fly the same save in each and press at the same moments after the load: the pod then passes
over the same ground, and each record names the same quads and the same nearest quad as its counterpart,
whose vertices compare one by one.

The pod flies 5 km above the ground, so every quad, the nearest included, stands 5 km or more from the
world origin: its heights carry the [precision](what-it-measures.md#precision) of that distance, about a
millimetre, where the landed saves resolve the nearest quad to a few micrometres.

## The log

Every record is written on lines starting with `[RockPrecisionFixDiag]`, every height in metres, from
the centre of the body, to the micrometre: subtracting two of them gives millimetres. A record reads:

```
Record … on …: scatter on. Heights are …
  quad '…': height …, matrix …, up … mm, across … mm
    holder '…': built, height …, matrix …, up … mm, across … mm
    …
  …
  rock '…' '…' #0 vertex …: ground …, vertex …, … mm above the ground
  …
End of record …: … quads with rocks, … holders (… not built yet); nearest quad '…': … rocks, … vertices measured, … without ground under them
```

- An opening line, with the body and whether scatter is on.
- **Every** quad carrying scatter around the craft, with the height of its centre, the height
  of its matrix, *up* and *across* in millimetres.
- Under each quad, each of its holders, one per kind of scatter, ordered by name: whether its objects are
  built yet, then the same heights, *up* and *across*, still against the centre of the quad.
- Then, for the nearest quad only, one line per measured vertex, object after object and holder after
  holder: the quad, the kind of scatter, the object's number, the vertex's number in the model, the
  height of the ground under the vertex and of the vertex itself, and the gap between the two in
  millimetres. A vertex without ground under it is one where the ray found no terrain: its ground and its
  gap read `--`. An object keeps its number from one load to the next, since stock places the scatter
  from a seed, and a vertex keeps its number since it is chosen on the model.
- A closing line, with the counts: it is the one left in sight when following the file as it grows.

A holder's matrix height minus its quad's height and its *up* are nearly the same number, by two
different routes: a difference of heights, and a projection on the vertical.

**Reading the records.** Compare the records of the successive loads on the nearest quad. Its name has to
be the same in every record for them to compare anything. Vertex by vertex, the same kind of scatter, the
same object and the same vertex, the height above the ground answers the question on its own: constant
over the loads for all the vertices of an object, that object is drawn at the same place against the
ground every time; changing, it is not. The heights above it tell where a change comes from: a vertex's
height above the ground minus its holder's *up*
stays constant when the change is the holder's matrix, and the holder's height minus its quad's height
tells whether its centre left its quad's too. The quad's own *up* and *across* tell whether the
ground is drawn away from the quad. The height of the quad itself may change from one load to the next as
well: comparing everything to it keeps that from blurring the rest.
