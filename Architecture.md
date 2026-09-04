# Duet Cats - Game Architecture

## 1. Overview
A WebGL rhythm-based casual game developed in Unity (URP 2D) where players control two cats (left and right) via a drag/slide mechanism to catch falling candies synchronized with background music.

---

## 2. Core Systems & Architecture

### A. GameManager
- **Responsibility**: Manages overall game lifecycle states (`MainMenu`, `Playing`, `Paused`, `GameOver`, `Victory`).
- **Key Functions**: Score tracking, combo multipliers, and level timer synchronization.

### B. Note Spawning System (Data-Driven)
- **Data Source**: External JSON file (`JsonMidi_BabyMonster.json`) converted from MIDI data.
- **Data Parsing & Logic**:
  - **`ta` (Timing Arrival)**: Used as the absolute time reference to spawn notes compared against `Time.time`.
  - **`pid` (Lane ID)**: Determines target lanes and cat assignments:
    - `pid` 0 & 2: Left Cat / Candy Group 1.
    - `pid` 3 & 5: Right Cat / Candy Group 2.
  - **Unused/Ignored Fields**: `ts` (global delta time) and `d` (duration) are legacy artifacts from MIDI export and are bypassed during runtime execution.

### C. Player Mechanics & Input
- **CatController**: Handles the movement logic for both the left and right cats within their respective screen boundaries.
- **Input Solutions**:
  1. **Touch / Drag-Slide Control (Primary)**: 
     - Allows players to touch and drag/slide across the screen or use mouse dragging on desktop. 
     - Cats dynamically follow the horizontal coordinate of the input, offering a natural and immersive rhythm-game feel.
  2. **Button / Key Input (Secondary / Alternative)**: 
     - On-screen touch buttons or keyboard inputs (e.g., Left/Right arrows) for players who prefer tapping specific zones.
     - Ensures accessibility and smooth gameplay on devices where dragging might obscure visibility.
- **Collision Detection**: Uses Unity's 2D Physics system (`OnTriggerEnter2D`) between cat colliders and falling candy objects.

//TODO: update this

---

## 3. Project Structure (Unity URP)
```text
Assets/
├── Animations/
├── Art/
│   ├── Cats/
│   └── Candies/
├── Audio/
├── Data/
│   └── JsonMidi_BabyMonster.json
├── Scenes/
├── Scripts/
│   ├── Core/
│   ├── Gameplay/
│   └── Spawner/
└── Settings/
```

## 4. Optimize
Playable Ads: Instead of splitting the content into multiple separate scenes—which wastes time on transitions or loading when running on the web—you can implement all these screens within a single scene by toggling the corresponding UI Canvas Groups based on the GameManager's current state.