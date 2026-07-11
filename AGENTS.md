# Interchained — Project Overview

## Game Ruleset

### Board
- Hexagonal grid, radius 3 (37 tiles). Center hex is excluded — acts as a pivot.
- 4 regions: North, South (mirrored `InvCone` shapes), East, West (mirrored `BiasCone` shapes). Each region has 9 cells.

### Sudoku Constraints
- No repeating numbers within a region.
- No repeating numbers along any hex axis (all 6 axial lines).
- Values: `0`=empty, `1–6`=domino numbers, `7`=solo, `8–9`=gears, `10`=blocker.

### Domino Pair Constraint
- Values 1–6 form domino pairs: (1,2), (3,4), (5,6). Adjacent tiles with matching pair values auto-pair.
- Pair assignment is dynamic (greedy scoring: prefers loop tiles, penalizes invalid tiles, won't break equal-score existing pairs).
- Value 7 is a single (no pair needed).
- Domino pairs form the segments of the loop.

### Loop Constraint
- A closed loop must be formed around the board using domino-paired tiles.
- Each loop tile must have exactly 2 adjacent loop neighbors (degree 2).
- Loop tiles with ≥3 adjacent loop tiles are invalid.
- Loop tiles with 0–1 adjacent loop tiles are incomplete (not yet invalid pending connection).
- The loop is drawn as a panning-texture LineRenderer (conveyor belt effect).

### Gear Constraints
- Gears follow Sudoku rules (no duplicate in region/axis).
- Gears cannot be adjacent to another gear of the same value (8↔9 adjacent is allowed).
- **Gear-Loop Meshing (conveyor belt rule):** Any gear adjacent to a loop tile must be on a consistent side of the loop flow:
  - All CW gears (8) must be on one side of the loop; all CCW gears (9) must be on the opposite side.
  - If any gear is on the wrong side → invalid.
  - The loop may run clockwise or counterclockwise — discovering the direction from gear clues is part of the puzzle.
  - Non-adjacent gears are unconstrained.

### Input
| Input | Action |
|---|---|
| Left click | Increment tile |
| Right click | Decrement tile |
| Long press / Enter / L | Toggle loop mark |
| Space | Swap domino pair values / toggle gear (8↔9) |
| 1–9 / A / B / X / Y | Set specific value |
| Delete / Backspace | Clear tile |

## Current Project Status

### Implemented ✅
- Hex grid with axial coordinates (`Hex.cs`)
- Board generation, region storage, tile instantiation (`HexGrid.cs`)
- Sudoku axis + region validation (`Validator.cs`)
- Gear adjacency and duplication checks
- Domino pair auto-assignment with scoring
- Loop tile degree validation
- Loop graph structure (`LoopGraph.cs`, `LoopNode.cs`)
- Loop drawing with LineRenderer (`LoopDrawer.cs`)
- Input handling, evaluation cycle (`Interchained.cs`)
- Label palettes (numeric and alpha), tile color schemes
- Gear CW/CCW visual rotators
- Save/load with bit-packed board state
- Create mode (lock tiles, place blockers)
- Clipboard copy/paste for level sharing

### Partially Implemented ⚠️
- `LoopGraph.FindPathDFS/BFS`, `HasCycle` — throw `NotImplementedException`
- `LoopDrawer.SortLoopTiles` builds graph but `TryToDrawLoop` is the full walk — `SortLoopTiles` is called but not fully integrated with drawing
- `LoopDrawer` assigns `LoopIn`/`LoopOut` but directionality can be unreliable mid-puzzle
- `BiasCone` has an unimplemented `width` parameter

### Not Yet Implemented ❌
- **Gear-loop meshing validation** (conveyor belt rule — rule designed, not coded)
- Win/solved state detection beyond counting invalid tiles
- Level progression UI integration
- Any tutorial or onboarding
- Sound effects / music

## Advisable Next Steps

### 1. Implement Gear-Loop Meshing Validation (High Priority)
This is the core remaining rule. Per the design plan:
- Add `GetDirectionBetween()` and `GetLoopSide()` helpers to `Validator.cs`
- Add `InvalidGearLoopMeshing(TileData gear)` to check consistency across all adjacent gears
- Integrate into `EvaluateCycle` (run after loop direction is established)

### 2. Fix LoopDrawer Direction Assignment (Medium Priority)
`TryToDrawLoop` assigns `LoopIn`/`LoopOut` but relies on `FindAdjacentLoopTiles` returning tiles in a specific order. This should be:
- More deterministic (sort by consistent hex direction order)
- Handle incomplete loops gracefully (partial direction assignment)
- This is a prerequisite for reliable gear-loop validation

**Recommended development order:** stabilise the loop graph in `LoopDrawer.OnDrawGizmos` first (the yellow-points-and-arrows debug view in the Scene window). The debug view is the right iteration surface for validating `FindAdjacentLoopTiles` ordering, `LoopGraph` link construction, and direction assignment — it's already wired up via `SortLoopTiles` and survives independent of the runtime LineRenderer. Only re-enable `TryToDrawLoop` (currently commented out in `Interchained.RedrawLoop`) once the gizmo representation is correct and deterministic across mid-puzzle states.

### 3. Implement LoopGraph Pathfinding (Medium Priority)
The DFS/BFS and `HasCycle` stubs would enable:
- Verifying the loop is a single closed cycle (no branches)
- Detecting valid loop completion even without a contiguous walk
- Could simplify `TryToDrawLoop` by using graph algorithms instead of manual stepping

### 4. Polished Win Detection (Low Priority)
Currently `IsSolved()` just checks `invalidTiles.Count == 0`. At minimum, also verify:
- The loop is closed (last tile connects back to first)
- All tiles are non-empty
- All 4 regions are complete (9 tiles each with unique values 1–9)

### 5. Level Design & Progression (Ongoing)
- More puzzle levels with varying gear/loop complexity
- A level-select UI
- Possibly a hint system for the "confusing" letter labels

### 6. Polish (Low Priority)
- Animated conveyor belt texture on loop LineRenderer
- Particle effects on gear meshing
- Sound design
