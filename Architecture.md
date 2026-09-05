# Duet Cats - Game Architecture

## 1. Overview

A WebGL rhythm-based casual game developed in Unity (URP 2D) where players control two cats (left and right) via a drag/slide mechanism to catch falling candies synchronized with background music. Designed specifically for Playable Ads with an optimized single-scene architecture and minimal asset overhead.

---

## 2. Core Systems & Architecture

### A. GameManager

* **Responsibility**: Manages overall game lifecycle states (`MainMenu`, `Playing`, `Paused`, `GameOver`, `Victory`).
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


* **Resolution Independence & Layout Management (`LaneManager`)**:
* **Single Source of Truth**: `LaneManager` acts as the central hub calculating lane X-slices and the global judgment line (`HitLineY`) mapped dynamically from screen Viewport coordinates.
* **Background & UI Harmony**: Both the cats' vertical tracking position (`initialY`) and the note hit line are derived from viewport percentages rather than hardcoded world coordinates. This ensures that regardless of the device aspect ratio (e.g., standard 9:16 vs. ultra-tall mobile viewports), the cats stay perfectly aligned with the visual background art (such as the wooden benches) and the rhythm judgment line remains completely synchronized.



---

## 3. Project Structure

```text
Assets/
├── Animations/
├── Art/
│   ├── Cats/
│   └── Candies/
├── Audio/
├── Data/
│   └── JsonMidi_BabyMonster.json
├── Materials/
├── Prefabs/
│   ├── Candies/
│   └── UI/
├── Scenes/
├── Scripts/
│   ├── Core/         # GameManager, AudioMaster, LaneManager
│   ├── Gameplay/     # CatMoveController, CandyMover
│   ├── Spawner/      # RhythmController, ChartLoader
│   └── UI/           # HUDManager, EndCardUI
└── Settings/

```

## 4. Playable Ads Optimization Strategies

* **Single-Scene Architecture**: All game states (`MainMenu`, `Playing`, `GameOver`, `Victory`) are contained within a single scene, toggling UI Canvas Groups dynamically to eliminate loading screens and transition lags critical for instant-play web environments.
* **Lightweight Codebase (No Heavy Frameworks)**: Avoids heavy reactive programming libraries (like UniRx) to maintain a minimal build size and ultra-fast WebGL initialization times.
* **Asset Optimization**: Utilizes Sprite Atlas V2 for texture packing to reduce draw calls, memory overhead, and file size footprint for WebGL/Playable Ads deployment.