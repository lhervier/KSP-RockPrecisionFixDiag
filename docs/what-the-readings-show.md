# What the readings show

## The rocks

The readings kept under [diag](../diag/README.md) follow [the protocol for the rocks, load after load](measuring-the-rocks.md#load-after-load) on two saves, each loaded twelve
times on stock and twelve times with
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) installed:

- [`reference-kerbin.sfs`](../diag/reference-kerbin.sfs), on Kerbin: every record holds the same 64 quads
  and 118 holders, and names the same nearest quad, `Kerbin Zn3010000130`, with the same 218 objects:
  200 `Grass00`, with 8 vertices each, all measured, and 18 `Tree00`, with 10 vertices measured each;
- [`reference-mune.sfs`](../diag/reference-mune.sfs), on the Mun: 128 quads and 128 holders, and the same
  nearest quad, `Mun Zp211333000`, with 20 `Rock00`, 10 vertices measured each.

Below, the *range* of a reading is its largest value minus its smallest over the twelve loads of a series,
and the *shape* of an object is the height of each of its vertices minus the height of its first vertex in
the log, the lowest of its model.

**Kerbin**

| | stock | with Terrain Precision Fix |
|---|---|---|
| centre of the nearest quad, range | 131 mm | 0.002 mm |
| matrix of each quad against its centre | *up* and *across* 0.000 mm everywhere | the same |
| centre of each holder against its quad's | the same height, to the micrometre | range 148 mm (median over the holders), from 88 to 227 mm |
| *up* of the holders' matrices | −92 to +92 mm, 0.000 mm 28% of the time | −107 to +110 mm, never 0 |
| each vertex above the ground, range | 94 mm (median over the vertices), from 66 to 167 mm | 130 mm (median), from 127 to 134 mm |
| the same minus its holder's *up*, range | 5.3 mm (median), but over 10 mm for 80 objects, up to 119 mm | 1.7 mm (median), 6.6 mm at most |
| shape of each object, range | 0.015 mm (median over the vertices), 0.066 mm at most | 0.015 mm (median), 0.063 mm at most |

**The Mun**

| | stock | with Terrain Precision Fix |
|---|---|---|
| centre of the nearest quad, range | 33 mm | 0.005 mm |
| matrix of each quad against its centre | *up* and *across* 0.000 mm everywhere | the same |
| centre of each holder against its quad's | the same height, to the micrometre | range 25 mm (median over the holders), from 15 to 42 mm |
| *up* of the holders' matrices | within 0.1 mm of 0 three times out of four, otherwise ±15.2 to ±15.4 mm, nothing in between | −27 to +29 mm, never 0 |
| each vertex above the ground, range | 31 mm (median over the vertices), from 12 to 37 mm | 31 mm (median), from 30 to 32 mm |
| the same minus its holder's *up*, range | 2.2 mm (median), but over 10 mm for 3 rocks out of 20, up to 18.5 mm | 0.55 mm (median), 1.9 mm at most |
| shape of each object, range | 0.002 mm (median over the vertices), 0.011 mm at most | 0.006 mm (median), 0.024 mm at most |

**In every series, no object changes shape from one load to the next.** Within an object, the heights of
its vertices against each other come back the same at every load to within 0.07 mm, trees 20 m tall
included, while the whole object moves by centimetres. Whatever moves an object moves all of it by the
same height: none of them comes back leaning another way, sunk deeper at one end or stretched.

**Stock.** Every object of the nearest quad is drawn at a different height against the ground at every
load: 94 mm apart over the twelve loads for half of the vertices on Kerbin, 31 mm on the Mun. Most of it
comes from its holder: the holder's centre stays on its quad's, but the matrix it is drawn with stands
above or below it by an amount that changes at every load, and taking that *up* off leaves a few
millimetres for most vertices. Not for all of them: the ground itself moves against the centre of the
quad from one load to the next, by 104 mm for half of the vertices on Kerbin and 27 mm on the Mun, the
objects mostly move along with it, and some of them keep more than 10 mm once their holder's *up* is taken
off: 80 objects out of 218 on Kerbin, 3 rocks out of 20 on the Mun.

**With Terrain Precision Fix.** The quads stop moving: the centre of the nearest quad comes back at the
same height to a few micrometres, and the ground under each vertex moves by 6.7 mm at most against it on
Kerbin, 1.8 mm on the Mun. The objects are still drawn at a different height against the ground at every
load: 130 mm apart for half of the vertices on Kerbin, 31 mm on the Mun. All of it now comes from the
holders: their centres no longer stay on their quads', the *up* of their matrices never comes back to 0,
and once it is taken off, no vertex moves by more than 6.6 mm on Kerbin and 1.9 mm on the Mun.

**How much the objects move is a draw.** The *up* of a holder changes at every load, and the range of a
dozen draws can come out small or large: split in two halves of six loads, the Kerbin series with
Terrain Precision Fix gives 43 mm for one half and 130 mm for the other. Over all the holders and all the
loads, the *up* of their matrices is 31 mm on average (root mean square) on stock and 40 mm with Terrain
Precision Fix on Kerbin, 7.5 mm and 9.6 mm on the Mun: the same order of size.

**In short.** Neither stock nor Terrain Precision Fix draws scatter at the same height against the ground
twice. Neither is a real problem in play, though. Scatter has no collider: no craft rests on it and
nothing hits it, so an object drawn a few centimetres higher or lower than at the last load changes
nothing for the game. And scatter is sunk into the ground on purpose: on the nearest quad, the lowest
vertex of each object's model sits between 27 and 40 cm below the ground on average on Kerbin, and about
2 m on the Mun, depending on the load, so a shift of a few centimetres mostly moves the objects within the
ground, where nobody sees it.

### Along a flight

The readings kept under [diag](../diag/README.md#the-rocks-over-a-flight) follow
[the protocol over a flight](measuring-the-rocks.md#over-a-flight), with Terrain Precision Fix installed:
six records, five along the flight and one after the crash, 1,600 holders in all.

| record | holders | *up* of the holders' matrices | largest *across* | largest gap between a holder's centre and its quad's |
|---|---|---|---|---|
| 1 | 344 | −25.9 to +13.3 mm | 10.0 mm | 25.2 mm |
| 2 | 168 | −17.7 to +17.5 mm | 10.0 mm | 22.6 mm |
| 3 | 144 | −26.5 to +9.4 mm | 9.9 mm | 22.5 mm |
| 4 | 224 | −20.1 to +18.6 mm | 8.2 mm | 20.1 mm |
| 5 | 152 | −13.8 to +21.1 mm | 10.0 mm | 20.1 mm |
| 6, after the crash | 568 | −25.7 to +17.4 mm | 10.0 mm | 24.0 mm |

The matrices of the quads stand on their centres, *up* and *across* 0.000 mm, in every record. **The
holders do not, at any record**: not one of the 1,600 has its centre at its quad's height, nor an *up* of
0, and the objects of the nearest quad are drawn off the ground by their holder's *up*. The offset does not
grow along the flight, though: 9.0 mm on average (root mean square) over all the holders, between 6.9 and
10.6 mm from one record to another, where the twelve loads of the Mun gave 9.6 mm. Quads built minutes
into the flight, and those still there after the crash, are drawn off the ground as much as those of a
load, and no more.

## The colliders

The readings kept under [diag](../diag/README.md#the-colliders) follow
[the protocol with colliders on the scatter](measuring-the-rocks.md#with-colliders-on-the-scatter):
[`ref-kerbin-scatter-collider-eva.sfs`](../diag/ref-kerbin-scatter-collider-eva.sfs), a kerbal standing on
a boulder in a desert of Kerbin, loaded six times on stock and six times with
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix), with a picture of the
kerbal's feet at each load. Every record names the same nearest quad and measures the same six objects
with a collider: one `boulder` and five `cactus`. *Up* is positive when the collider stands above the
object one sees.

**Stock**

| load | *up* of the six colliders | largest *across* | *up* of their holder's matrix |
|---|---|---|---|
| [1](../diag/runs/collider-stock-load1.log) | −59.1 to +15.3 mm | 42.6 mm | +51.2 mm |
| [2](../diag/runs/collider-stock-load2.log) | −68.7 to +2.3 mm | 83.3 mm | −9.4 mm |
| [3](../diag/runs/collider-stock-load3.log) | +39.2 to +104.2 mm | 38.7 mm | +10.1 mm |
| [4](../diag/runs/collider-stock-load4.log) | −2.3 to +73.3 mm | 23.4 mm | 0.000 mm |
| [5](../diag/runs/collider-stock-load5.log) | −50.8 to +5.0 mm | 37.8 mm | +11.2 mm |
| [6](../diag/runs/collider-stock-load6.log) | −2.0 to +87.7 mm | 52.5 mm | −69.3 mm |

**With Terrain Precision Fix**

| load | *up* of the six colliders | largest *across* | *up* of their holder's matrix |
|---|---|---|---|
| [1](../diag/runs/collider-tpf-load1.log) | −36.2 to +0.1 mm | 44.5 mm | +119.9 mm |
| [2](../diag/runs/collider-tpf-load2.log) | −55.5 to +37.7 mm | 27.7 mm | +1.1 mm |
| [3](../diag/runs/collider-tpf-load3.log) | −3.0 to +70.3 mm | 32.8 mm | +55.6 mm |
| [4](../diag/runs/collider-tpf-load4.log) | −4.4 to +32.2 mm | 24.5 mm | +10.9 mm |
| [5](../diag/runs/collider-tpf-load5.log) | −70.2 to −33.9 mm | 28.7 mm | +151.7 mm |
| [6](../diag/runs/collider-tpf-load6.log) | −48.2 to +44.1 mm | 38.7 mm | +75.1 mm |

**The collider of an object is not where that object is drawn**, in either series. Over the 36 readings of
each, the gap runs from −68.7 to +104.2 mm on stock, 48.1 mm on average (root mean square), and from
−70.2 to +70.3 mm with Terrain Precision Fix, 31.6 mm on average; sideways, up to 83.3 mm and 44.5 mm. Each
object is drawn afresh at every load: over its six loads, the boulder alone covers 102 mm on stock and
72 mm with Terrain Precision Fix, and the widest cactus 163 mm and 114 mm.

**It does not follow the holder.** On load 4 of the stock series, the holder of those six objects sits
exactly on its quad, *up* and *across* 0.000 mm, and its colliders still stand up to 73 mm from the
objects they belong to. Whatever separates the two happens below the holder, where each object carries its
own position under it.

**Seen from the ground.** On the boulder the kerbal stands on, the gap reads −4.3, −48.1, +48.2, +10.2,
−9.1 and +53.7 mm over the stock loads, and −22.5, +37.7, +29.9, +2.3, −33.9 and +14.9 mm with Terrain
Precision Fix. The pictures kept beside the logs sort themselves in the same order: boots sunk to the
ankles where the gap is most negative, boots clear of the rock where it is most positive. They are
illustrations, not measurements: the viewpoint is not exactly the same twice, and a kerbal sinks a little
into whatever it stands on.

**What it changes.** Everything else this mod measures leaves the game as it was: stock scatter has no
collider, so an object drawn a few centimetres off changes nothing for a craft. With colliders on the
scatter, it does: the rock a craft hits is not the rock its pilot sees, by several centimetres, drawn
afresh at every load. Terrain Precision Fix does not settle it. What
[Rock Precision Fix](https://github.com/lhervier/KSP-RockPrecisionFix) makes of it, and the third series,
with both fixes installed, are on
[its own page](https://github.com/lhervier/KSP-RockPrecisionFix/blob/main/docs/checking-the-culprit.md).

## The holder pools

Both series were taken with [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix)
installed.

### Over a flight

The readings kept under [diag](../diag/README.md#the-holder-pools-over-a-flight) follow
[the protocol for the holder pools](checking-the-holder-pools.md#over-a-flight) over one flight: six
records, five along the flight and one after the crash.

| record | holders in use | free | broken rules |
|---|---|---|---|
| 1 | 344 | 40 | 0 |
| 2 | 168 | 216 | 0 |
| 3 | 144 | 240 | 0 |
| 4 | 224 | 160 | 0 |
| 5 | 152 | 232 | 0 |
| 6, after the crash | 568 | 40 | 0 |

The only pool is the Mun's `Rock00`. Its holders keep moving between in use and free as the pod flies on,
and the pool grows from 384 holders to 608 after the crash, when stock makes new ones because it has none
free left. **No record breaks any rule**: every holder handed back hangs in the pool's container without a
quad, every holder in use stands on a live quad of the Mun, and every count agrees with its list.

### Across scene switches

The readings kept under [diag](../diag/README.md#the-holder-pools-across-scene-switches) follow
[the protocol across scene switches](checking-the-holder-pools.md#across-scene-switches): one record on the
Mun, one at the Space Center, one on Kerbin.

| record | scene | pools | holders in use | free | broken rules |
|---|---|---|---|---|---|
| 1 | the Mun | the Mun's `Rock00` | 128 | 32 | 0 |
| 2 | the Space Center | Kerbin's `Tree00`, `Grass00`, `boulder`, `Pine00`, `cactus` | 4 | 316 | 0 |
| 3 | Kerbin | the same five | 118 | 202 | 0 |

The Mun's pool is gone from the second record on: leaving the Mun, stock destroyed its holders along with
its terrain, and left none of them in the pool's lists. **No record breaks any rule**, and none finds a
holder in no pool: no holder of the Mun was left on a quad reused for Kerbin.
