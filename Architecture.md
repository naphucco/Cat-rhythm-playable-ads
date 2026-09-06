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
   │  owns gameplay state (songTimer, note spawning, hit/miss resolution)
   │  fires: OnSongPlayRequested, OnSongStopRequested,
   │         OnNoteHitEvent, OnNoteMissEvent, OnGameWin, OnGameLose
   ▼
GameManager (state-machine layer)
   │  listens to RhythmController's raw gameplay events and translates
   │  them into high-level game states (Tutorial / Playing / Win / Lose...)
   │  fires: OnTutorialStateEntered, OnPlayingStateEntered,
   │         OnWinStateEntered, OnLoseStateEntered, ...
   ▼
Presentation layer (UI, CatAnimationController, TutorialController, ScoreManager...)
   listens to whichever layer's event best matches what it actually needs
```

* **Dependencies only flow downward** (RhythmController → GameManager → Presentation). `RhythmController` never references `GameManager`, keeping the core gameplay loop fully independent and reusable.
* **Each listener binds to the event that matches its own semantic layer, not just "whatever fires at a convenient time".** For example:
  * `AudioManager` listens to `RhythmController.OnSongPlayRequested` / `OnSongStopRequested` — it only cares about audio lifecycle, not about win/lose outcome, so it is never coupled to `GameManager` at all.
  * `CandyMover` listens to `RhythmController.OnSongStopRequested` (not `GameManager.OnLoseStateEntered`) to clear itself instantly when the game ends. `OnSongStopRequested` fires on **both** Win and Lose, so candies are cleaned up in either outcome with a single subscription — and since `CandyMover` is itself spawned/owned by `RhythmController`, listening to a layer above it (`GameManager`) would create a conceptual reverse-dependency.
  * `CatAnimationController` listens to `GameManager.OnLoseStateEntered` (not `RhythmController.OnNoteMissEvent`) for its "miss/game over" animation, since with the current single-life design a miss always immediately ends the game — binding to the higher-level state event avoids firing the same animation twice from two events that happen in the same frame.
* **Why this matters for a playable ad**: this separation means `AudioManager`, `CandyMover`, and other core systems could be dropped into a different game/scene with a different `GameManager` state machine and would keep working unmodified — none of them know `GameManager` exists.

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

* Single-Scene Architecture: All game states (`Tutorial`, `Playing`, `Win`, `PickNextSong`, `Lose`) are contained within a single scene, toggling UI Canvas Groups dynamically to eliminate loading screens and transition lags critical for instant-play web environments.
* Lightweight Codebase (No Heavy Frameworks): Avoids heavy reactive programming libraries (like UniRx) to maintain a minimal build size and ultra-fast WebGL initialization times.
* Asset Optimization: Utilizes Sprite Atlas V2 for texture packing to reduce draw calls, memory overhead, and file size footprint for WebGL/Playable Ads deployment.
* Object Pooling: Utilizes a multi-type object pool (Pooler) with automated lifecycle management and queue recycling to handle candy instances efficiently without runtime performance spikes.