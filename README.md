# Rock Precision Fix Diag

A measuring instrument for KSP 1.12. It lets you check, on your own install, a claim about the terrain
scatter drawn around your craft — the rocks, and around the KSC the grass and the trees:

> **KSP never draws terrain scatter at the same height against the ground twice.** Load the same save
> several times, and every rock, tuft of grass or tree comes back drawn a little higher or a little
> lower against the ground each time — several centimetres apart on Kerbin.

**How this was made.** Written with Claude, Anthropic's AI assistant, and reviewed line by line by a
human — me. I am saying so up front, because contributions made with an AI deserve a closer
look than others, and because some people would rather stop reading here. This mod measures and fixes
nothing, so what there is to check is the reading itself: the source is public, and the protocol below
runs on a stock install, wherever you land.

## Why it matters

It barely does. Stock scatter has no collider: nothing rests on it and nothing hits it, so a rock drawn a
few centimetres higher or lower than at the last load changes nothing for your craft. Scatter is also
sunk partly into the ground on purpose, so the shift is hard to see, and most of the time you will not
see it at all.

This instrument exists for another reason. [Terrain Precision Fix](https://github.com/lhervier/KSP-TerrainPrecisionFix)
changes the height at which KSP builds the ground, and checking a change like that means checking
everything placed against that ground. Scatter is one of those things, so this instrument is meant to be
run on stock and with Terrain Precision Fix installed. The readings taken both ways, on the same saves, are
kept under [diag](diag/README.md) and summed up under [What the readings show](#what-the-readings-show). Why stock draws scatter off the ground is on
[Rock Precision Fix's page](https://github.com/lhervier/KSP-RockPrecisionFix/blob/main/README.md#the-culprit).
This page sticks to how to measure it.

## What it measures

At each load of a save, for every quad carrying scatter around the craft, the heights of the quad and of
each of its holders, centre and matrix, and, on the nearest quad, 10 vertices of every object against the
ground under them: readings that should come out the same at every load. A sixth reading, during a
flight, checks the pools the holders are taken from and handed back to.

**→ Full chapter: [What it measures](docs/what-it-measures.md)**

## Measuring the rocks

Readings 1 to 5: scatter on and the log written at once, a craft landed and saved, then that save loaded a
dozen times, with `Alt+F6` pressed after each load. Each record ends on a line counting what it holds.

**→ Full chapter: [Measuring the rocks](docs/measuring-the-rocks.md)**

## Checking the holder pools

Reading 6: the provided save of a pod flying 5 km over the Mun, with `Alt+Shift+F6` pressed along the
flight and after the crash. Each record ends on the number of rules the pools break: 0 expected.

**→ Full chapter: [Checking the holder pools](docs/checking-the-holder-pools.md)**

## What the readings show

Loaded twelve times on Kerbin and on the Mun, on stock and with Terrain Precision Fix, no object changes
shape, but every one is drawn at a different height against the ground at each load: centimetres apart
either way. Over a whole flight low over the Mun, with Terrain Precision Fix, the holder pools break no
rule.

**→ Full chapter: [What the readings show](docs/what-the-readings-show.md)**

## Get it

Source: <https://github.com/lhervier/KSP-RockPrecisionFixDiag>

**Build.** It needs the .NET SDK and reads the KSP assemblies from your install. Set `KSPDIR` to your
KSP install folder and run `build.bat`. It compiles `GameData/RockPrecisionFixDiagMod/RockPrecisionFixDiagMod.dll`
and never touches your KSP install.

**Install.** Copy `GameData/RockPrecisionFixDiagMod` into the `GameData` of KSP. It runs on a stock
install, and needs neither Harmony nor any other mod: it only reads the scene.

## License

MIT
