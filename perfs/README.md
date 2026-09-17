# Measurement runs

Readings of this mod, kept as they were logged: one file per load of the same save, each holding the
last record taken after that load, copied out of `KSP.log`.

The procedure is [the protocol](../README.md#the-protocol) on the main page, and what a record holds is
described under [the log](../README.md#the-log).

## The save

[`reference-kerbin.sfs`](reference-kerbin.sfs), a sandbox game of KSP 1.12.5. Its active craft is a
single Mk1 command pod landed on Kerbin, about 8 km north-west of the KSC (latitude 0.2696°, longitude
−75.2783°), where the scatter is grass and trees. To use it, copy it into the folder of a sandbox game and
load it from that game.

## Stock

KSP 1.12.5 on Windows. `GameData` holding Harmony, ModuleManager, KSP Community Fixes 1.41.1
and this mod: nothing else. Terrain scatter on, the log flushed at once. KSP
was started once, and the save loaded six times in that session, with `Alt+F6` pressed after each load
once the scene had settled.

| load | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| log | [`load1`](runs/kerbin-stock-load1.log) | [`load2`](runs/kerbin-stock-load2.log) | [`load3`](runs/kerbin-stock-load3.log) | [`load4`](runs/kerbin-stock-load4.log) | [`load5`](runs/kerbin-stock-load5.log) | [`load6`](runs/kerbin-stock-load6.log) |

Every record ends on the same line, but for its number:

```
End of record 1: 64 quads with rocks, 118 holders (0 not built yet); nearest quad 'Kerbin Zn3010000130': 218 rocks, 0 without ground under them
```

So every record was taken once all the holders were built, and all of them name the same nearest quad:
its 218 objects, 200 `Grass00` and 18 `Tree00`, compare one by one from one load to the next.

## With Terrain Precision Fix

The same install and the same build of this mod as above, with
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) 0.1.0 added to `GameData`,
at its default settings. Terrain scatter on. The save was loaded six times, with `Alt+F6` pressed after
each load once the scene had settled. KSP was started once for the first load and once more for the five
others, which is why the first two records both carry the number 1.

| load | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| log | [`load1`](runs/kerbin-tpf-load1.log) | [`load2`](runs/kerbin-tpf-load2.log) | [`load3`](runs/kerbin-tpf-load3.log) | [`load4`](runs/kerbin-tpf-load4.log) | [`load5`](runs/kerbin-tpf-load5.log) | [`load6`](runs/kerbin-tpf-load6.log) |

Here too every record ends on the same line as in stock, but for its number: all the holders built, and
the same nearest quad, `Kerbin Zn3010000130`, with the same 218 objects. The records of both series
therefore compare with each other as well, object by object.
