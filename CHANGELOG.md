# Changelog

NOTE: Assume that every version change will product different results unless otherwise
specified.

## [2.0.0]

### Breaking
* `IRandom` is now an interface rather than an abstract class. Custom implementations implement
  the interface (drop `: IRandom` base-class semantics and the `override` keyword); this also makes
  it composable with other interfaces (e.g. a combined `IRandom, ISeeded` type).
* `IRandom` gained a `NextLong()` primitive (full-range 64-bit). Custom implementations must add it.
* `SystemRandom` is now `sealed`.
* The convenience helpers (`NextBool`, `WithOdds`, `WithPercentChance`, `From`, `FromWithOdds`,
  `PickFromList`, `FromEnum`, `Next(max)`, `NextDouble(min,max)`, `Next(float,float)`, `Roll`, and
  the obsolete `From*` methods) moved off `IRandom` into extension methods in `RandomExtensions`
  and `RandomPickExtensions`. `IRandom` now declares only the primitives (`Next`, `NextDouble`,
  `ChildRandom`). Call sites are unchanged as long as `using Pomerandomian;` is in scope.
* The string-to-seed helpers moved off `IRandom` into a dedicated `Seeds` static class and were
  rename.
* Seeding is now a separate capability, `ISeeded`, rather than part of `IRandom`. `Seed`,
  `RawSeed`, and `Split` were removed from `IRandom` and the lossy `int Seed` is gone entirely.
  `RawSeed` (the faithful original seed) and `Split` now live on `ISeeded`.. Code that needs to
  both generate and `Split` can use `ISeededRandom`.
  A typed `ISeeded<TSeed>` adds a more accurately-typed seed value.

### Added
* `Xoshiro256PpRandom`: a new `IRandom` implementation backed by xoshiro256++ (seeded
  via SplitMix64). Unlike `SystemRandom`, its primitives should be deterministic across
  platforms and runtimes (Mono, IL2CPP, etc.), so the same seed reproduces the same 
  sequence everywhere. Use it when you need cross-platform determinism (replays, 
  deterministic generation, lockstep).
* `ISeeded` capability interface exposing `RawSeed` and `Split(int streamId)`. `Split`
  deterministically derives an independent substream as a pure function of the seed and stream
  id (order-independent, unlike `ChildRandom()`). `ISeeded<TSeed>` adds a typed `Seed`, and
  `ISeededRandom` (`IRandom, ISeeded`) is a convenience type for code that both generates and splits.
* `ReservoirSample` LINQ extension: single-pass uniform sampling of up to N items from a
  source of any (or unknown) size.
* `From<T>(IReadOnlyList<T>)` and `FromWithOdds` overloads taking `IReadOnlyList` for both
  `int` and `float` weights.
* `ObjectOddsFloat<T>` struct for specifying float weights (usable in the inspector).
* `NextLong()` for a full-range 64-bit value, with ranged `NextLong(maxExclusive)` and
  `NextLong(min, max)` extension helpers.
* `WeightedOdds<T>`: a precomputed weighted distribution. Build it once from int or float weights
  (or `ObjectOdds`/`ObjectOddsFloat`) and `Pick` in O(log n). Float weights are quantized to
  integers at construction, so picks are deterministic across platforms; tiny weights round to zero
  by default (pass `keepTinyWeightsSelectable: true` to floor them at one unit instead).
* `Seeds` static class for deriving deterministic seeds from strings: `Seeds.Bytes` (raw 16-byte
  hash primitive), `Seeds.Int` (32-bit), and `Seeds.ULong` (64-bit). `Xoshiro256PpRandom` seeds
  from 64 bits of a string seed; `SystemRandom` uses the 32-bit `Seeds.Int`.
* `Dice.MinRoll` property.

### Changed
* `NextDouble(double, double)` and `Next(float, float)` now split their multiply-add into
  separately-rounded steps so backends cannot fuse them into a single FMA. This makes the
  results deterministic across platforms (e.g. IL2CPP players now match the editor).
  Note: output for these two methods may differ from previous versions.
* `Dice.FromString` parsing is now culture-invariant (`ToLowerInvariant`), so the
  advantage/disadvantage suffix is matched regardless of locale.
* `FromWithOdds` (all overloads) now throws on invalid input instead of silently returning
  `default`: an `ArgumentNullException` for a null list/odds and an `ArgumentException` for an
  empty list, matching `WeightedOdds`. The length-mismatch message is aligned to match as well.
* The float `FromWithOdds` overloads (`IReadOnlyList<float>` and `ObjectOddsFloat<T>`) now quantize
  their weights to integers and pick against the integer total — the same deterministic recipe as
  `WeightedOdds` — instead of summing floats at pick time. Picks are now reproducible across
  platforms and runtimes. Note: output for these overloads may differ from previous versions, and a
  weight more than ~10^9x smaller than the largest now quantizes to zero (use `WeightedOdds` with
  `keepTinyWeightsSelectable: true` to keep such weights selectable).

### Fixed
* `Dice.FromString` read the advantage/disadvantage suffix from a non-existent regex
  group, so the modifier was never applied; rolls now parse `A`/`H`/`D`/`L` correctly.
* `FromWithOdds` performed its null/empty guard *after* dereferencing the list, which threw
  a `NullReferenceException` on null input; the guard now runs first.
* `FromWithOdds` float overloads short-circuited valid fractional weight totals to the
  first element (`< 1` / `<= 1` guard); the guard is now `<= 0`, so totals below 1.0 are
  weighted correctly.

### Deprecated
* `FromList`, `FromArray`, and the `FromArrayWithOdds` overloads. Use `From` /
  `FromWithOdds` with `IReadOnlyList` instead.

## [1.1.0]

### Added
* Dice support: `Dice`, `DiceResult`, `SingleRollResult`, and `RollType` (standard,
  advantage, disadvantage), with flat modifiers and `Dice.FromString` parsing of
  notation like `2d6`, `1d20A`, `4d6-1`.
* `IRandom.Roll(Dice)` helper for rolling a dice configuration.

## [1.0.1]

### Added
* `ObjectOdds<T>` struct and a `FromArrayWithOdds(ObjectOdds<T>[])` overload, providing a
  weighted-pick API that binds objects and odds together so it can be populated in the
  inspector.

## [1.0.0]

* Baseline release.

[2.0.0]: https://github.com/khutchins/Pomerandomian/releases/tag/v2.0.0
[1.1.0]: https://github.com/khutchins/Pomerandomian/releases/tag/v1.1.0
[1.0.1]: https://github.com/khutchins/Pomerandomian/releases/tag/v1.0.1
