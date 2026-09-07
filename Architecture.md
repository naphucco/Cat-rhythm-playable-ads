
# Duet Cats - Game Architecture

## 1. Overview

A WebGL rhythm-based casual game developed in Unity (URP 2D) where players control two cats (left and right) via a drag/slide mechanism to catch falling candies synchronized with background music. Designed specifically for Playable Ads with an optimized single-scene architecture and minimal asset overhead.

---

## 2. Core Systems & Architecture

### A. GameManager

* **Responsibility**: Manages overall game lifecycle states (`Tutorial`, `Playing`, `Win`, `PickNextSong`, `Lose`).
* **Key Functions**: Score tracking, combo multipliers, and level timer synchronization.

### B. Note Spawning & Movement System (Data-Driven & Logic-Based)

* **Data Source**: External JSON file converted from MIDI data.
* **Data Parsing & Logic**:
* **`ta` (Timing Arrival)**: Absolute song timestamp used to synchronize note arrival.
* **Constant Speed Trajectory**: Notes are spawned at a calculated top viewport position and travel down at a fixed constant speed derived from a global `noteTravelTime`, ensuring visual-timing synchronization without mid-air speed jitters.


* **Decoupled Hit Detection (No Physics Colliders)**:
* **Why Physics Colliders Are Omitted**: Traditional Unity 2D physics (`OnTriggerEnter2D`) introduces performance overhead, frame-rate dependency, and potential hit registration jitter or tunneling when items fall at high speeds in a lightweight WebGL container.
* **Pure Logic-Based Resolution**: Hit detection relies entirely on a deterministic spatial-temporal check. When a falling note's timestamp (`targetArrivalTime`) is reached, `CandyMover` queries a static registry in `CatMoveController` (`IsLaneCaught(laneIndex)`) to verify if a cat occupies that specific lane index. This guarantees frame-rate-independent, mathematically precise rhythm judgment.

### C. Player Mechanics & Input (Responsive Layout)

* **CatMoveController**: Manages cat lane assignments and smooth horizontal snapping based on input.
* **Input Solutions**:
* **Touch / Drag-Slide Control (Primary)**: Allows players to touch and drag/slide across the screen or use mouse dragging on desktop. Cats dynamically follow the horizontal coordinate of the input.
* **Known Trade-off**: the current implementation reads a single global pointer position (`Input.mousePosition`), so both cats currently resolve their target lane from the *same* pointer rather than two fully independent touch points. On a single-cursor desktop this is not noticeable, but on a real multi-touch device two fingers dragging simultaneously would not yet be tracked independently. Given the time constraints, this was accepted as-is; a proper fix would track `Input.touches[]` by touch `fingerId` and assign each active touch to whichever cat's screen half it falls into.


* **Resolution Independence & Layout Management (`LaneManager`)**:
* **Single Source of Truth**: `LaneManager` acts as the central hub calculating lane X-slices and the global judgment line (`HitLineY`) mapped dynamically from screen Viewport coordinates.
* **Background & UI Harmony**: Both the cats' vertical tracking position (`initialY`) and the note hit line are derived from viewport percentages rather than hardcoded world coordinates. This ensures that regardless of the device aspect ratio (e.g., standard 9:16 vs. ultra-tall mobile viewports), the cats stay perfectly aligned with the visual background art (such as the wooden benches) and the rhythm judgment line remains completely synchronized.

### D. Event Flow & Dependency Layering

A deliberate one-way dependency chain is enforced across the codebase to keep gameplay logic decoupled from presentation:

```text
RhythmController (core/domain layer)
   │   owns gameplay state (songTimer, note spawning, hit/miss resolution)
   │   fires: OnSongPlayRequested, OnSongStopRequested,
   │          OnNoteHitEvent, OnNoteMissEvent, OnGameWin, OnGameLose
   ▼
GameManager (state-machine layer)
   │   listens to RhythmController's raw gameplay events and translates
   │   them into high-level game states (Tutorial / Playing / Win / Lose...)
   │   fires: OnTutorialStateEntered, OnPlayingStateEntered,
   │          OnWinStateEntered, OnLoseStateEntered, ...
   ▼
Presentation layer (UI, CatAnimationController, TutorialController, ScoreManager...)
   listens to whichever layer's event best matches what it actually needs

```

* **Dependencies only flow downward** (RhythmController → GameManager → Presentation). `RhythmController` remains fully independent and reusable without referencing `GameManager`.
* **Semantic event binding**:
* `AudioManager` listens to `RhythmController` audio lifecycle events (`OnSongPlayRequested` / `OnSongStopRequested`) and has zero coupling to `GameManager`.
* `CandyMover` listens to `RhythmController.OnSongStopRequested` to clear active candies on both win and lose outcomes, avoiding reverse-dependencies.
* `CatAnimationController` listens to `GameManager.OnLoseStateEntered` for game-over animations, preventing double-firing bugs in single-life designs.
* **Modular architecture**: Core systems (`AudioManager`, `CandyMover`) operate independently and remain compatible with alternative state machines.

---

## 3. Project Structure

```
Assets/
├── Art/
├── Audio/
├── Congfigs/
├── Data/
├── Editor/
├── Fonts/
├── Materials/
├── Plugins/
├── Prefabs/
│   ├── Candies/
│   ├── Cats/
│   ├── Manager/
│   └── UI/
├── Resources/
├── Scenes/
├── Scripts/
│   ├── Audio/
│   ├── Core/
│   ├── Data/
│   ├── Gameplay/
│   └── Utility/
├── Settings/
└── TextMeshPro/

```

## 4. Playable Ads Optimization Strategies

* **Single-Scene Architecture**: All game states (`Tutorial`, `Playing`, `Win`, `PickNextSong`, `Lose`) are contained within a single scene, toggling UI Canvas Groups dynamically to eliminate loading screens and transition lags critical for instant-play web environments.

* **Lightweight Codebase (No Heavy Frameworks)**: Avoids heavy reactive programming libraries (like UniRx) to maintain a minimal build size and ultra-fast WebGL initialization times. Instead, we implemented a custom **ObservableSystem** (~5KB) that provides a UniRx-like fluent API (`WhenReady().Subscribe().AddTo()`) for safe singleton subscription while avoiding third-party dependencies.

* **Asset Optimization**:
  * Utilizes **Sprite Atlas V2** for texture packing to reduce draw calls, memory overhead, and file size footprint.
  * All textures are compressed using **ASTC 12x12 block format** for WebGL builds, balancing visual quality with aggressive size reduction. ASTC was chosen over ETC2 because ETC2 lacks hardware support on desktop browsers (Chrome/Firefox on Windows/macOS) and iOS Safari, which would cause black textures or rendering failures on a significant portion of ad viewers.
  * Background textures are limited to **1024x1024 Max Size**, while UI elements and sprites are kept at **512x512** or lower, ensuring visual clarity without unnecessary memory bloat.

* **Audio Optimization**:
  * All audio clips are set to **Force To Mono**, reducing channel count by 50%.
  * Compression format: **Vorbis** with **Quality set to 50-60**, balancing audio fidelity against file size.
  * Load Type: **Compressed In Memory** for short SFX (cry sounds, hit feedback), preventing full decompression into RAM.

* **Object Pooling**: Utilizes a multi-type object pool (`Pooler`) with automated lifecycle management and queue recycling to handle candy instances efficiently without runtime performance spikes.

* **Code Stripping & Build Configuration**:
  * **Managed Stripping Level**: Set to **High** to strip unused C# code.
  * **Strip Engine Code**: Enabled to remove unused Unity engine modules.
  * **IL2CPP Code Generation**: Set to **Optimize for code size and build time**.
  * **Compression Format**: **Brotli** for optimal WebGL asset compression.
  * **Shader Stripping**: Unused HDRP shaders (e.g., `TMP_SDF-HDRP Lit/Unlit`) were removed, retaining only `TMP_SDF-URP Lit/Unlit` to avoid unnecessary shader variants.
  * **DOTween & Spine**: Retained as they are essential for gameplay animations; removing them would require rewriting extensive animation logic and introduce high risk of regression.

* **Final Build Metrics**:
  - **Total Build Size (uncompressed)**: ~11.3 MB
  - **Build.data**: ~5.2 MB (assets, audio, textures)
  - **Build.wasm**: ~6.1 MB (IL2CPP compiled code)
  - **After Brotli Compression (actual download size)**: ~4-5 MB
  - This size is well within the limits of major ad networks (Meta: 10-15 MB, Google: 10-15 MB, Unity Ads: 10 MB) and ensures fast loading even on mobile 3G/4G connections.

* **Known Trade-offs & Future Improvements**:
  * The current build uses **ASTC 12x12**, which is the most aggressive compression available. Upgrading to **ASTC 6x6** or **4x4** would improve visual quality but increase build size by ~1-2 MB.
  * **`wasm-opt`** (Binaryen) could reduce `.wasm` size by an additional 0.2-0.5 MB, but was omitted due to time constraints and the risk of runtime instability on WebGL.
  * **`link.xml`** manual stripping could further reduce code size, but carries a high risk of `NullReferenceException` due to IL2CPP stripping reflection-based code. This is not recommended for production Playable Ads where stability is paramount.
  * **DOTween** is a convenience library; if future size constraints require more aggressive reduction, replacing it with manual `Mathf.Lerp` or custom coroutines could save ~0.3-0.5 MB, but at the cost of development time and potential animation jitter.
