# Measurement runs

Readings of this mod, kept as they were logged: one file per load, each holding the last record taken
after that load, copied out of `KSP.log`.

The procedure is [the protocol](../README.md#the-protocol) on the main page, and what a record holds is
described under [the log](../README.md#the-log).

## The saves

Two saves of a sandbox game of KSP 1.12.5. To use one, copy it into the folder of a sandbox game and load
it from that game.

- [`reference-kerbin.sfs`](reference-kerbin.sfs): a single Mk1 command pod landed on Kerbin, about 8 km
  north-west of the KSC (latitude 0.2696°, longitude −75.2783°), where the scatter is grass and trees.
- [`reference-mune.sfs`](reference-mune.sfs): a single Mk1 command pod landed on the Mun (latitude
  −12.4124°, longitude 91.7314°), where the scatter is rocks.

## The install

KSP 1.12.5 on Windows. `GameData` holding Harmony, ModuleManager, KSP Community Fixes 1.41.1 and this
mod: nothing else on stock. For the series *with Terrain Precision Fix*,
[Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix) 0.1.0 is added to
`GameData`, at its default settings. The same build of this mod throughout. Terrain scatter on, the log
flushed at once.

Each series is one save loaded twelve times, with `Alt+F6` pressed after each load once the scene had
settled. A series may span two sessions of KSP: the record numbers keep counting within a session and
start again at 1 in the next, and a session may go on from one series to the other.

## Kerbin

| load | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| stock | [`1`](runs/kerbin-stock-load1.log) | [`2`](runs/kerbin-stock-load2.log) | [`3`](runs/kerbin-stock-load3.log) | [`4`](runs/kerbin-stock-load4.log) | [`5`](runs/kerbin-stock-load5.log) | [`6`](runs/kerbin-stock-load6.log) | [`7`](runs/kerbin-stock-load7.log) | [`8`](runs/kerbin-stock-load8.log) | [`9`](runs/kerbin-stock-load9.log) | [`10`](runs/kerbin-stock-load10.log) | [`11`](runs/kerbin-stock-load11.log) | [`12`](runs/kerbin-stock-load12.log) |
| with Terrain Precision Fix | [`1`](runs/kerbin-tpf-load1.log) | [`2`](runs/kerbin-tpf-load2.log) | [`3`](runs/kerbin-tpf-load3.log) | [`4`](runs/kerbin-tpf-load4.log) | [`5`](runs/kerbin-tpf-load5.log) | [`6`](runs/kerbin-tpf-load6.log) | [`7`](runs/kerbin-tpf-load7.log) | [`8`](runs/kerbin-tpf-load8.log) | [`9`](runs/kerbin-tpf-load9.log) | [`10`](runs/kerbin-tpf-load10.log) | [`11`](runs/kerbin-tpf-load11.log) | [`12`](runs/kerbin-tpf-load12.log) |

- **Stock**: loads 1 to 6 in one session of KSP, loads 7 to 12 in another.
- **With Terrain Precision Fix**: the twelve loads in a single session.

Every record of both series ends on the same line, but for its number:

```
End of record 1: 64 quads with rocks, 118 holders (0 not built yet); nearest quad 'Kerbin Zn3010000130': 218 rocks, 1780 vertices measured, 0 without ground under them
```

So every record was taken once all the holders were built, and all of them name the same nearest quad:
its 218 objects, 200 `Grass00` and 18 `Tree00`, and their 1,780 vertices compare one by one from one load
to the next, and from one series to the other.

## The Mun

| load | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| stock | [`1`](runs/mun-stock-load1.log) | [`2`](runs/mun-stock-load2.log) | [`3`](runs/mun-stock-load3.log) | [`4`](runs/mun-stock-load4.log) | [`5`](runs/mun-stock-load5.log) | [`6`](runs/mun-stock-load6.log) | [`7`](runs/mun-stock-load7.log) | [`8`](runs/mun-stock-load8.log) | [`9`](runs/mun-stock-load9.log) | [`10`](runs/mun-stock-load10.log) | [`11`](runs/mun-stock-load11.log) | [`12`](runs/mun-stock-load12.log) |
| with Terrain Precision Fix | [`1`](runs/mun-tpf-load1.log) | [`2`](runs/mun-tpf-load2.log) | [`3`](runs/mun-tpf-load3.log) | [`4`](runs/mun-tpf-load4.log) | [`5`](runs/mun-tpf-load5.log) | [`6`](runs/mun-tpf-load6.log) | [`7`](runs/mun-tpf-load7.log) | [`8`](runs/mun-tpf-load8.log) | [`9`](runs/mun-tpf-load9.log) | [`10`](runs/mun-tpf-load10.log) | [`11`](runs/mun-tpf-load11.log) | [`12`](runs/mun-tpf-load12.log) |

- **Stock**: the twelve loads in a single session, the one of Kerbin's stock loads 7 to 12.
- **With Terrain Precision Fix**: loads 1 to 6 in the session of Kerbin's series, loads 7 to 12 in
  another.

Every record of both series ends on the same line, but for its number:

```
End of record 1: 128 quads with rocks, 128 holders (0 not built yet); nearest quad 'Mun Zp211333000': 20 rocks, 200 vertices measured, 0 without ground under them
```

All the holders built, and the same nearest quad in every record: its 20 `Rock00` and their 200 vertices
compare one by one.
