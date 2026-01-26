# Modifiers

What is a Behaviour?
A behaviour is a pure capability. It knows HOW to do exactly one thing. It has stats that affect that thing. It does NOT know:

- When it executes
- What triggers it
- What happens after it executes
- Anything about events

What is a Skill?
A skill is a container of behaviours. Nothing more. It doesn't orchestrate. It doesn't have triggers. It's just a bag of capabilities with combined tags.

Who orchestrates?
The GAME orchestrates. Not the library. The library cannot possibly know all the trigger types your game will have.



# Library Architecture Overview

## Overview
This document outlines the architectural philosophy and structural patterns used across all Spellbound Studio libraries.
Understanding these patterns will help you navigate, extend, contribute, and reason about any system we build.

---

## Table of Contents
- [Overview](#overview)
- [The Layer Model](#the-layer-model)
    - [Layer 0 - "The Data Layer"](#layer-0---the-data-layer)
    - [Layer 1 - "The Engine Layer"](#layer-1---the-engine-layer)
    - [Layer 2 - "The Convenience Layer"](#layer-2---the-convenience-layer)
    - [Layer 3 - "The Educational Layer"](#layer-3---the-educational-layer)
- [Dependency Rules](#dependency-rules)
- [The Two User Types](#the-two-user-types)
- [Library Directory Layout](#library-directory-layout)
- [Summary](#summary)

---

## The Layer Model

### Layer 0 - "The Data Layer"
Pure data structures with no behavior. These are the inputs and outputs that flow through the system.

**Contains:**
- Configuration files (.yaml, .json)
- ScriptableObjects
- Structs, enums, readonly records
- Data Transfer Objects (DTO's)

**Dependencies:** None.

**Change Impact:** Catastrophic. Everything in the library depends on these.

---

### Layer 1 - "The Engine Layer"
The engine transforms inputs into outputs. Users can ship an entire game without knowing the inner workings of this layer—and most will. If you find that the majority of users are trying to access this layer then that should act as a code smell. Power users live here.

**Contains:**
- Interfaces defining contracts
- Containers and managers
- Core algorithms and calculations

**Dependencies:** Layer 0 only.

**Change Impact:** Severe. Changes here require significant effort and refactoring.

---

### Layer 2 - "The Convenience Layer"
The majority of users live here. They should be able to make powerful behavioral changes to their game with ease by referencing this layer. This is the layer that allows users to feel powerful and say "I did X" even when they don't know the details of what's happening in the Engine Layer. This is not a jab at the user... this is a core idea on the <i> feeling </i> the user gets when using the library.

**Contains:**
- API classes with readable methods
- Base classes users can inherit
- Inspector tools and attributes
- Extension methods and helpers

**Dependencies:** Layers 0 and 1.

**Change Impact:** Moderate. Changes are localized and rarely require refactoring lower layers. Engineers can enhance tooling, visuals, and wrapper methods *usually* in one location.

---

### Layer 3 - "The Educational Layer"
Thorough and complete examples demonstrating usage. This layer is separate from the shipped library.

**Contains:**
- Samples and demos
- Concrete implementations
- Integration examples

**Dependencies:** No restrictions.

**Change Impact:** None. This layer is not used by the library.

---
## Dependency Rules

Dependencies flow downward only. Never upward. Never sideways.

| Layer | Can Depend On |
|-------|---------------|
| 0 | Nothing |
| 1 | Layer 0 |
| 2 | Layers 0 and 1 |
| 3 | Anything |

Violations of this rule indicate architectural problems.

---

## The Two User Types

### The 80% User
- Wants to ship a game, not study a library
- Lives in Layer 2
- Follows Layer 3 examples
- Entry point: base classes and helpers

### The 20% Power User
- Has constraints the library doesn't anticipate
- Implements Layer 1 interfaces directly
- May bypass Layer 2 entirely
- Entry point: contracts and interfaces

A well-designed library serves both.

---

## Library Directory Layout

The directory layout follows Unity's recommended package structure:
https://docs.unity3d.com/6000.3/Documentation/Manual/cus-layout.html
```
<package-root>/
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
├── Editor/
│   ├── Spellbound.[PackageName].Editor.asmdef
│   └── ...
├── Runtime/
│   ├── Spellbound.[PackageName].asmdef
│   ├── Data/           ← Layer 0
│   ├── Core/           ← Layer 1
│   └── API/            ← Layer 2
├── Tests/
│   ├── Editor/
│   └── Runtime/
├── Samples~/           ← Layer 3
│   └── ExampleName/
└── Documentation~/
```

**Notes:**
- The `~` suffix excludes folders from Unity's compilation (Samples are opt-in imports)
- Layer boundaries are directory boundaries where possible
- Assembly definitions enforce dependency rules at compile time

---

## Summary

| Layer | Name | Purpose | Users |
|-------|------|---------|-------|
| 0 | Data | Shapes that flow through the system | Everyone (indirectly) |
| 1 | Engine | Powers the system | Power users (20%) |
| 2 | Convenience | Makes the common case trivial | Most users (80%) |
| 3 | Educational | Demonstrates usage | New users |

When navigating any Spellbound Studio library:
- **Quick results?** Start at Layer 2.
- **Full control?** Drop to Layer 1.
- **Learning?** Read Layer 3.