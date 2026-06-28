# Handoff — Gear-Loop Meshing & Project Overview

## What Was Accomplished

### Conversation Summary
- Explored the full codebase: `Hex.cs`, `HexGrid.cs`, `TileData.cs`, `TileLabels.cs`, `Interchained.cs`, `Validator.cs`, `LoopDrawer.cs`, `LoopGraph.cs`, `LoopNode.cs`, `TileVisuals.cs`, `Rotator.cs`
- Documented the complete ruleset in `AGENTS.md`
- Designed the **gear-loop meshing (conveyor belt) rule** — the last major unimplemented mechanic

### Key Design Decisions (Gear-Loop Meshing)
| Decision | Choice |
|---|---|
| Rule type | Hard constraint (invalid tiles, not soft scoring) |
| Loop direction | Can be CW or CCW — discovered from gear clues |
| Side consistency | All CW (8) must be on one side of loop, all CCW (9) on the opposite |
| Non-adjacent gears | Unconstrained |
| Gear-on-gear (8↔9) | No additional rule needed — 8↔9 adjacent is already allowed |
| When to validate | Continuously, but only for loop segments with defined direction |
| Who checks | Add to `Validator.cs`, call from `Interchained.EvaluationCycle` |

### How Side Detection Works
Given a gear adjacent to a loop tile:
- Compute `entryDir` = direction from `LoopIn` to the loop tile
- Compute `gearDir` = direction from the loop tile to the gear
- `diff = (gearDir - entryDir + 6) % 6`
  - `diff = 1 or 2` → side A
  - `diff = 4 or 5` → side B
  - `diff = 0 or 3` → on the loop path (shouldn't occur)
- Collect all (gear, loopTile, side) tuples, then verify: same side must have same gear type; different sides must have opposite gear types.

## Files to Modify

| File | What to Do |
|---|---|
| `Assets/Scripts/Validator.cs` | Add `GetDirectionBetween()`, `GetLoopSide()`, `InvalidGearLoopMeshing()` Modify `InvalidGear()` to include the new check |
| `Assets/Scripts/Interchained.cs` | Call gear-loop validation in `EvaluateCycle` after loop direction is established |
| `Assets/Scripts/LoopDrawer.cs` *(maybe)* | Fix `TryToDrawLoop` to assign `LoopIn`/`LoopOut` more reliably — this is a prerequisite for gear-loop validation to work correctly |

## Next Steps Queue (from AGENTS.md)

1. **Implement Gear-Loop Meshing Validation** — the rule we just designed; highest priority
2. **Fix LoopDrawer Direction Assignment** — needed for reliable gear-loop checks
3. **Implement LoopGraph Pathfinding** — DFS/BFS and HasCycle stubs
4. **Polished Win Detection** — beyond just `invalidTiles.Count == 0`
5. **Level Design & Progression**
6. **Polish** (conveyor texture, particles, sound)

## Quick Reference — Key Types

| Type | Location | Role |
|---|---|---|
| `Hex` | `Hex.cs:26` | Axial coordinate (q, r) |
| `HexGrid` | `HexGrid.cs:19` | Board manager, region storage |
| `TileData` | `TileData.cs:13` | Per-tile state (value, loop, pair) |
| `LoopNode` | `Graph Structure/LoopNode.cs:6` | Graph node wrapping a TileData |
| `LoopGraph` | `Graph Structure/LoopGraph.cs:6` | Undirected/directed graph for loop |
| `Validator` | `Validator.cs:5` | All constraint checks |
| `LoopDrawer` | `LoopDrawer.cs:9` | Loop visual rendering (LineRenderer) |
| `Interchained` | `Interchained.cs:14` | Main game controller, input, evaluation cycle |
| `Constants` | `TileData.cs:7` | `MAX_CELL_VALUE = 9` |
| `BoardRegions` | `HexGrid.cs:10` | None, North, East, South, West |

## ScorePair Logic (for pair assignment)
Located in `Interchained.cs:422`:
- Tile on loop: +2
- Tile invalid: -1
- Already paired together: +3
- Both on loop: +2
- A new pair is accepted only if `replacingScore > existingScore`, or if equal and the existing tile is unpaired.
