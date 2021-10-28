# Pomerandomian

An abstract class for randomness that makes it easy to get a flexible, repeatable random number source.

The name is combining the words Pomeranian and Random together. I know it's a reach.

## Overview

IRandom is an abstract class that allows you to pass around an abstract reference. There's currently only one implementation: SystemRandom. It usees System.Random for its underlying generation (I know, not a big surprise). 

**C# doesn't guarantee that System.Random behaves the same cross-platform or across versions of C#**, but it hasn't failed me yet. If this guarantee is desired, implement your own PRNG!

## Selected Features

* Strings are supported as seeds (they're MD5 hashed down to integers. Standard security warnings about MD5 apply, but if you're doing anything involving security, don't use this).
* Access the IRandom's seed (and string seed via raw seed, if created using one) whenever you want.
* Ability to create child random objects to sequester off a RNG that pulls a non-deterministic number of samples. Environment object sparking randomly when it bounces off a wall? No problem, create a child random for it without affecting the randomness of anything later.

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
