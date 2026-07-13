# Wordania

A game made by an AGH Computer Science student in his free time. It features:
- **Procedural World Generation** based on Perlin Noise
- **Dynamic Tiled World** *(breaking, building)* Optimized with chunks
- **Dynamic Lighting System** based on BFS
- **Enemies** with abstract state machine
- **Complex bosses** (1) with unique attack movesets
- **Stats** health, *magic/physical/etc* damage, *fire/maigical/etc* defense
- **Projectile Factory** optimized with Data-Oriented Design
- **Saving System** working on Json-Soft
- **Clear separation of concerns** following SOLID principles
- **Interface-Driven Design** 
- **More** world map, ui, loadout, skill tree.

---

## ⚡ Core Tech Stack
* **Dependency Injection:** VContainer (Explicit lifetime management & IoC object graphs)
* **Asynchronous Pipeline:** UniTask (Allocation-free async/await task coordination)
* **Simulation Layer:** C# Job System & Burst Compiler (Data-Oriented Design for high-throughput math)
* **Data Persistency:** Newtonsoft JSON (Polymorphic graph reconstruction & async I/O)
* **Custom abstract structures** SFM, Data-Registries, Events

---

## 🏗️ Architectural Core Pillars

### 1. Inversion of Control & Scoped Lifetimes
Global Singletons and brittle static references are completely banned. System graphs are decoupled using localized lifecycle containers:
* **Project Scope:** Manages persistent global infrastructure (Save Engine, Input Readers, Scene Streaming).
* **Gameplay Scope:** Encapsulates transient mechanics unique to an active world instance (Generation Pass Engine, Entity Trackers, View Presenters).

### 2. Strict Type-Safe Identity (`AssetId`)
To eliminate the instability of magic strings and anonymous integer evaluations, all structural configurations (items, blocks, recipes, and entity templates) are bounded by an immutable, value-typed `AssetId`. Registries derive directly from an abstract `IAssetRegistry` pattern to guarantee compiler-enforced identity checking across serialization boundaries.

### 3. Hierarchical Maschine States (SFM)
Complex actor behaviors abandon heavy tick loops in favor of isolated, command-driven Finite State Machines:
* **Player FSM:** Encapsulates strict environmental transitions via atomic state contexts (`AirState`, `GroundState`, `MenuState`).
* **Multi-Part Boss Engine:** Utilizes hierarchical sub-FSM controllers, allowing individual components (e.g., Head vs. Hands) to evaluate independent sub-behaviors concurrently while maintaining synchronization with a master supervisor.

### 4. Multi-Pass Procedural Voxel Engine
World data transformation operates via a pipeline of discrete generation filters extending the `IWorldGenerationPass` contract. Voxel calculations process as raw data chunks independent of Unity's main thread, feeding an asynchronous mesh builder and a threaded collision generation routine.

---

## 🚀 Repository Engineering Guidelines

* **Zero Legacy Lookup Queries:** The use of `GameObject.Find`, `FindWithTag`, and un-cached runtime `GetComponent` operations is strictly banned. Dependencies must be resolved exclusively via VContainer constructor injection or pass-by-context signatures.
* **Hermetic Slice Boundaries:** Gameplay features are treated as strictly isolated modules. Direct cross-referencing between distinct slices is blocked; communication across feature borders must operate via abstract core interface models or type-safe message dispatches routed through the global `Event Bus`.
