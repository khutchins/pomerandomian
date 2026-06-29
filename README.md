# Pomerandomian

An abstract class for randomness that makes it easy to get a flexible, repeatable random number source.

The name is combining the words Pomeranian and Random together. I know it's a reach.

## Overview

IRandom is an abstract class that allows you to pass around an abstract reference. There's currently two implementations: Xoshiro256PpRandom and SystemRandom. Xoshiro256PpRandom uses Xoshiro256++ for its underlying generation and SystemRandom uses System.Random (I know, not a big surprise). 

## Which Random Should I Use

Xoshiro256++ is recommended for all purposes.

C# doesn't guarantee that System.Random behaves the same cross-platform or across runtime/.NET versions, so it doesn't have guarantees that Xoshiro256++ has in regard to reproducibility across platforms.

Use `SystemRandom` only if you need to reproduce sequences you already generated with it. For everything else, `Xoshiro256PpRandom` is better on quality, speed, and reproducibility.

## Quick Start

```csharp
using Pomerandomian;

// Seed with an int, a string (MD5-hashed down to bits; don't rely on it for anything
// security-sensitive), or nothing (time-based).
var rand = new Xoshiro256PpRandom("my-seed");   // IRandom for generation; ISeeded for Split/RawSeed

int n      = rand.Next(1, 7);             // int in [1, 7)
double d   = rand.NextDouble();           // double in [0, 1)
bool coin  = rand.NextBool();             // 50/50
bool crit  = rand.WithPercentChance(0.05);// 5% chance

// Pick from a collection (optionally weighted).
Item item  = rand.From(items);
Item drop  = rand.FromWithOdds(items, new[] { 70, 25, 5 });

// LINQ helpers.
var shuffled = items.Shuffle(rand);
var sample   = items.ReservoirSample(rand, 3); // 3 uniform picks, single pass

// Dice.
Dice dice  = Dice.FromString("2d6+1");    // also "1d20a" (advantage), "4d6l" (disadvantage)
int total  = rand.Roll(dice);
DiceResult detailed = dice.RollDetailed(rand); // per-die breakdown

// Isolate a subsystem so its draws don't shift the rest of your randomness. ChildRandom advances
// the parent; Split(id) is a pure function of the seed + id, so it's order-independent (ideal for
// giving each entity, e.g. a spawn index, its own reproducible stream).
IRandom sparks = rand.ChildRandom();

// Pass randoms around as IRandom to just generate. Take ISeededRandom (IRandom + ISeeded) when a
// system also needs to Split off its own stream, so you don't have to cast.
void SpawnEnemy(ISeededRandom rng, int spawnIndex) {
    IRandom enemyRng = rng.Split(spawnIndex);
    // use enemyRng for this enemy
}
SpawnEnemy(rand, enemyIndex);

// Reproduce later: Seed is the typed numeric seed (ulong) and round-trips the sequence exactly.
// RawSeed (object) hands back the original value you constructed with, e.g. the string "my-seed".
var again = new Xoshiro256PpRandom(rand.Seed);
```

## Caveats

The `FromWithOdds` methods that take in floats are not guaranteed to give the same result across machines or targets due to differences in floating point behavior. If you want true reproducibility between runs, avoid these methods (and avoid SystemRandom).

Neither of these are cryptographically secure! Do not use these for any cases where being able to predict future outputs is undesireable.

## Installation

There are two options for installation. One involves manually editing a file, and the other involves adding a URL to package manager.

NOTE: You should always back up your project before installing a new package.

### Add to Package Manager

Open the package manager (Window -> Package Manager), and hit the plus button (+) in the top right, then "add package from git URL". In that field, enter `https://github.com/khutchins/pomerandomian.git` and click Add.

### Modify manifest.json

Open Packages/manifest.json and add this to the list of dependencies (omitting the comma if it's at the end):

```
"com.khutchins.pomerandomian": "https://github.com/khutchins/pomerandomian.git",
```
