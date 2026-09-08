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
* **Multi-Touch Support (Primary)**: Each cat is controlled independently by its own touch finger.
* The screen is divided into left and right halves.
* The left cat responds only to touches on the left half; the right cat responds only to touches on the right half.
* Each cat tracks its assigned `fingerId` independently, allowing simultaneous two-finger control on mobile devices.


* **Mouse Support (Desktop Fallback)**: On desktop, mouse drag is supported with the same screen-half logic: dragging on the left side controls the left cat, and dragging on the right side controls the right cat.
* **Keyboard Controls (Desktop Fallback)**: To provide an alternative for desktop environments, keyboard controls are also supported:
* **Left Cat**: **A** (move left) and **D** (move right) to switch between its assigned lanes.
* **Right Cat**: **←** (move left) and **→** (move right) to switch between its assigned lanes.


* **Implementation Detail**: `CatMoveController` separates touch, mouse, and keyboard input into dedicated handlers. Touch input uses `Input.touches` with `fingerId` tracking, while mouse and keyboard provide fallback support for desktop.


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
* `CatAnimationController` listens to `GameManager.OnLoseStateEntered` and `GameManager.OnWinStateEntered` to trigger appropriate game-over animations (miss or victory), ensuring animation states are locked once the game ends.


* **Modular architecture**: Core systems (`AudioManager`, `CandyMover`) operate independently and remain compatible with alternative state machines.

### E. Lives & Failure System

* **Lives Management**: The game implements a lives system to provide a forgiving gameplay experience rather than ending the game on a single miss.
* Players start with **2 lives** (hearts) at the beginning of each session.
* Each missed note deducts **1 life**.
* When lives reach **0**, the game transitions to the `Lose` state.
* This design reduces player frustration and encourages longer engagement, which is critical for Playable Ads where retention is a key metric.


* **Visual Feedback**:
* Lives are displayed as heart icons in the UI, updating in real-time as lives are lost.
* When a life is lost, a brief visual effect (e.g., heart shake or fade) provides immediate feedback to the player.


* **Implementation Note**: The lives system is integrated into the existing event flow via `OnNoteMissEvent`. `GameManager` listens to this event and manages life count transitions, ensuring decoupling from the core rhythm logic.

### F. Dual-Orientation Support (Portrait & Landscape)

* **Unified Canvas Approach**: Instead of maintaining two separate UI hierarchies, the game uses a **single Canvas with responsive layout components**.
* UI elements (score, lives, combo, etc.) are repositioned dynamically using **ResponsiveRectOffset** components, which adjust anchor positions and offsets based on the initial screen orientation.
* Background art is dynamically swapped between `BGOutro_PT` (portrait) and `BGOutro_LS` (landscape) based on the aspect ratio at startup.


* **Resolution Independence**:
* All core gameplay logic (`RhythmController`, `GameManager`, `Pooler`, etc.) is shared, and `LaneManager` calculates lane positions using viewport percentages that adapt to both portrait and landscape widths/heights.
* This ensures proper spacing and scaling across different ad network container dimensions.


* **Activation & Rotation Policy**:
* At startup, `Screen.width > Screen.height` is evaluated to determine whether to initialize in Landscape or Portrait layout mode.
* **Runtime Rotation Lock**: Device orientation changes *during active gameplay* are intentionally not supported. Once the session begins, the orientation locks to prevent complex UI reflows or layout glitches mid-song, matching standard Playable Ad practices where containers do not rotate dynamically.



### G. Special Note (Lollipop) & Ripple Effect

* **Special Note Configuration**:
* A single note can be designated as a "lollipop" by its **index in the JSON chart** (`specialNoteIndex` in `RhythmController`). This avoids modifying JSON or relying on imprecise timing values (`ta`).
* When hit, it triggers:
* Visual: **ripple distortion effect** on the background (custom shader `BackgroundDistortion` with Gaussian ring).
* Audio: **water drop SFX** (`AudioManager.PlayWaterDrop()`).




* **Implementation**:
* `RhythmController.SpawnNote()` checks `specialNoteIndex` and overrides candy type to `Lollipop_Long`.
* `RipplePulse` listens to `OnNoteHitEvent`, triggers ripple animation when `candyType == Lollipop_Long`.
* `AudioManager` listens to same event, plays water drop SFX.


* **Performance**: Shader runs on GPU, no grab pass or full-screen pass → lightweight and WebGL-friendly.

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
* **Compression Format**: **Disabled** (Brotli/Gzip were initially tested; however, due to static hosting Content-Encoding header mapping quirks on Vercel causing MIME/syntax errors, uncompressed build was chosen as the verified-working deployment strategy for this submission).
* **Shader Stripping**: Unused HDRP shaders (e.g., `TMP_SDF-HDRP Lit/Unlit`) were removed, retaining only `TMP_SDF-URP Lit/Unlit` to avoid unnecessary shader variants.
* **DOTween & Spine**: Retained as they are essential for gameplay animations; removing them would require rewriting extensive animation logic and introduce high risk of regression.


* **Lives System (Retention-Focused Design)**:
* Instead of a single-miss failure condition, the game uses a **2-lives system** to keep players engaged longer.
* This increases the average playtime per session, which is a key metric for Playable Ads effectiveness.
* The trade-off is a slightly larger UI footprint (heart icons and feedback animations), but the impact on build size is negligible (~0.1 MB).


* **Dual-Orientation Support**:
* The game natively supports **both Portrait and Landscape orientations**, initializing smoothly based on the container's starting aspect ratio via a **single Canvas with responsive UI components** (`ResponsiveRectOffset`).
* Background art and layouts adapt automatically at startup, avoiding the overhead of maintaining separate scene hierarchies.
* **Note**: Device rotation *during active gameplay* is locked out to keep the presentation stable and avoid complex mid-session UI reflows.


* **Final Build Metrics**:
* **Total Build Size (uncompressed/deployed)**: ~11.3 MB
* **Build.data**: ~5.2 MB (assets, audio, textures)
* **Build.wasm**: ~6.1 MB (IL2CPP compiled code)
* This size is well within the limits of major ad networks (Meta: 10-15 MB, Google: 10-15 MB, Unity Ads: 10 MB) and ensures fast loading even on mobile 3G/4G connections.


* **Known Trade-offs & Future Improvements**:
* The current build uses **ASTC 12x12**, which is the most aggressive compression available. Upgrading to **ASTC 6x6** or **4x4** would improve visual quality but increase build size by ~1-2 MB.
* **`wasm-opt`** (Binaryen) could reduce `.wasm` size by an additional 0.2-0.5 MB, but was omitted due to time constraints and the risk of runtime instability on WebGL.
* **`link.xml`** manual stripping could further reduce code size, but carries a high risk of `NullReferenceException` due to IL2CPP stripping reflection-based code. This is not recommended for production Playable Ads where stability is paramount.
* **DOTween** is a convenience library; if future size constraints require more aggressive reduction, replacing it with manual `Mathf.Lerp` or custom coroutines could save ~0.3-0.5 MB, but at the cost of development time and potential animation jitter.
* **Dynamic orientation switching** (runtime rotation mid-session) is disabled by design to ensure layout stability since ad networks lock orientation containers.