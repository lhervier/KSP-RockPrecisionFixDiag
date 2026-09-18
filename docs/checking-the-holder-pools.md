# Checking the holder pools

How to take reading 6 of [What it measures](what-it-measures.md): the pools the holders are taken from
and handed back to, over a flight.

## The protocol

1. **Scatter on, and the log written at once**: steps 1 and 2 of
   [the protocol for the rocks](measuring-the-rocks.md#the-protocol).
2. **Load [`ref-mune-5km.sfs`](../diag/ref-mune-5km.sfs)**, copied into the folder of a sandbox game: a
   Mk1 command pod in a circular equatorial orbit 5 km over the Mun. Flying that low, the terrain keeps
   building the quads ahead of the pod and destroying those behind it, sending their holders through the
   pool all the way. Some of the Mun's relief rises above the orbit, so the pod crashes after a while.
3. **Press `Alt+Shift+F6`** (`Mod+Shift+F6`) 30 s into the flight, then again about every two minutes,
   and once more after the crash. The moments do not need to be exact: each record is a check of its own,
   not a number to compare with another flight.
4. **Copy `KSP.log`** before relaunching KSP.

The key works in every scene, not only in flight.

## The log

```
Holder record …: every pool of holders of rocks, one per body and kind of scatter
  Mun 'Rock00': in use … (counter …): … destroyed, … without a quad, … on an inactive quad, … on a quad of another body; free … (counter …): … destroyed, … outside the pool's container, … still with a quad; … both in use and free
  holder '…' in no pool, under '…'
End of holder record …: … pools, … holders in use, … free, … in no pool; … broken rules
```

- An opening line.
- One line per pool that holds any holder, by body and kind of scatter: how many holders are in use and
  free, each next to the pool's own count, then how many break each rule. A pool that holds none is left
  out.
- One line per holder that belongs to no pool, with the object it hangs from.
- A closing line, with the totals and the number of broken rules: the holders counted on the pool lines,
  one more for each count that disagrees with its list, and the holders in no pool. Expected: 0.

Holder records are numbered on their own, apart from the rock records.
