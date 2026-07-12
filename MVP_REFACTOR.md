# Interchained MVP Refactor

Goal: bring Interchained to a Minimum Viable Product state where the loop mechanics are fully implemented, victory conditions with clear win states work, and the project is minimally refactored to prepare for further development.

Derived from `Web/Docs/code-review-notes.html` and `AGENTS.md` "Advisable Next Steps". Each item below is sized for a single developer and written to be copy-pasted into a Gitea issue.

## MVP scope definition

**In scope:**
- Loop subsystem working end-to-end (build graph → walk cycle → draw on board).
- Canonical win condition: "the loop is closed".
- Gear-loop meshing rule (gears must be on the correct side of the loop flow).
- Win-state UX (overlay, play-again).
- Minimal refactors that the loop work needs anyway.

**Out of scope (post-MVP):**
- Expanded level editor beyond current create-mode.
- Multiple game modes / `IWinCondition` strategy seam.
- Tutorial / onboarding.
- Sound, music, particle polish.
- Save format centralisation (`TileData.Encode/Decode`).
- Cosmetic renames (Refactor #2 from the review).

## Milestones

| # | Milestone | Goal | Effort |
|---|---|---|---|
| 0 | Unblocking fixes | Land the one-line bug fix and the trivial dedup refactor that clears the path. | ~0.5 day |
| 1 | Loop foundation | Encapsulate loop state, inject shared Validator, rename the loop entry points. | ~1 day |
| 2 | Loop graph | Implement `HasCycle`/pathfinding, stabilise the gizmo debug view. | ~2 days |
| 3 | Loop drawing | Re-enable `TryToDrawLoop` with deterministic direction assignment. | ~1.5 days |
| 4 | Win condition | Replace `IsSolved()`, route through `GameManager`, ship win overlay. | ~1 day |
| 5 | Gear-loop meshing | Implement the conveyor-belt rule (the last undefined gameplay mechanic). | ~2 days |
| 6 | MVP polish | Sweep, playtest fixes, ship. | ~1 day |

**Total:** ~9 developer-days of focused work. Buffer to ~12 with debugging.

## Dependency graph

```
M0 ── M1 ── M2 ── M3 ── M4 ── M6
                       │
                       └── M5 (gear meshing) ──┘
```

M5 can start once M3 (loop drawing + direction) lands — direction is a prerequisite for the meshing rule.

---

## Milestone 0 — Unblocking fixes

### MVP-001 · Fix `LoopNode.IsValid` operator bug

**Labels:** `bug`, `loop-subsystem`, `quick-win`

`LoopNode.cs:12` reads `Links.Count <= 2 || Links.Count >= 1` — true for every integer count, so the validity gate never trips. This silently undermines `LoopGraph.IsValid()` and everything that will depend on it (win detection, gear meshing).

**Acceptance criteria:**
- [ ] Operator changed to `Links.Count >= 1 && Links.Count <= 2`.
- [ ] Unit-style test or `GraphTester.cs` scenario demonstrating: 0 links → invalid, 1 or 2 → valid, 3+ → invalid.
- [ ] Project still compiles (`dotnet build Assembly-CSharp.csproj`).

**Depends on:** nothing.
**Effort:** S (15 minutes).

---

### MVP-002 · Extract `ApplyBoardState` + `FlagOffendingTiles` helpers

**Labels:** `refactor`, `tech-debt`, `quick-win`

`Interchained.cs:660` (`LoadBoardState`) and `:683` (`LoadClipboard`) are ~95% identical. The `IsNumber` and `IsGear` branches of `ValidatePlacement` (`Interchained.cs:312-339`) share an identical tail. Extract both, so the orchestrator is smaller going into the loop work.

**Acceptance criteria:**
- [ ] New `private void ApplyBoardState(BoardData1D<int>)` helper; both load methods reduce to one line each.
- [ ] New `private void FlagOffendingTiles(TileData source, IEnumerable<TileData> offenders)` helper; both `ValidatePlacement` branches end with a one-line call.
- [ ] No behaviour change — load paths still work, validation flags the same set of tiles.
- [ ] Build green.

**Depends on:** nothing.
**Effort:** S (1 hour).

---

## Milestone 1 — Loop foundation

### MVP-003 · Encapsulate `TileData.LoopIn` / `LoopOut` behind a setter

**Labels:** `refactor`, `loop-subsystem`

`TileData.cs:29-30` exposes `LoopIn` and `LoopOut` as public mutable fields, written directly by `LoopDrawer.TryToDrawLoop` (`LoopDrawer.cs:90-103`). Three problems: no invariant enforced, no `onLoopChanged` event fires, ownership ambiguous. Refactor #3 (Option A) from the review.

**Acceptance criteria:**
- [ ] `LoopIn` / `LoopOut` become `private` with public read-only properties.
- [ ] New `SetLoopEndpoints(TileData in, TileData @out)` method on `TileData` that sets both, fires `onValueChanged` and (if marked) `onLoopChanged`.
- [ ] New `ClearLoopEndpoints()` method.
- [ ] `LoopDrawer` and any other callers updated to use the setters.
- [ ] Build green; existing behaviour preserved.

**Depends on:** nothing (can land before MVP-001/002 if desired).
**Effort:** S (1 hour).

---

### MVP-004 · Inject shared `Validator` into `LoopDrawer`; clear stale `possibleLoopTiles`

**Labels:** `refactor`, `loop-subsystem`

`LoopDrawer.cs:21` constructs its own `new Validator(_board)` separate from `Interchained`'s. `LoopDrawer.cs:13` persists `possibleLoopTiles` across calls. Both are drift surfaces as the loop subsystem is re-enabled.

**Acceptance criteria:**
- [ ] `LoopDrawer.Initialize` accepts the orchestrator's `Validator` instead of constructing one.
- [ ] `possibleLoopTiles` cleared at the top of `SortLoopTiles` (or each call rebuilds it).
- [ ] Build green.

**Depends on:** nothing.
**Effort:** S (45 minutes).

---

### MVP-005 · Rename loop entry points to match behaviour

**Labels:** `refactor`, `readability`, `loop-subsystem`

Refactor #2 from the review, scoped to loop files only (skip the broader renames).

| Current | Rename to |
|---|---|
| `SortLoopTiles` | `BuildLoopGraph` |
| `EvaluateLoopTiles` | `TryCacheLoopTiles` |
| `msg` parameter | `showDebug` (or remove if always `true`) |
| `PassesPlacementEval` | `candidateTiles` |
| `FindInAdjacent` | `AdjacentOf` |

**Acceptance criteria:**
- [ ] All five renames applied across their call sites.
- [ ] `msg` parameter either renamed or removed (prefer remove if every call site passes `true`).
- [ ] Build green.

**Depends on:** MVP-004 (so we touch the same files once).
**Effort:** S (45 minutes).

---

## Milestone 2 — Loop graph

### MVP-006 · Implement `LoopGraph.HasCycle()`

**Labels:** `feature`, `loop-subsystem`, `blocked-wait`

Currently throws `NotImplementedException` (`LoopGraph.cs:129`). Required by the canonical win condition. Standard undirected-cycle detection: DFS with parent tracking, or union-find. For a graph this small (≤37 nodes), either is fine.

**Acceptance criteria:**
- [ ] `HasCycle()` returns `true` iff the graph contains at least one cycle.
- [ ] Hand-tested in `GraphTester.cs` with: open chain (false), single cycle (true), two disjoint cycles (true), T-junction (true).
- [ ] Build green.

**Depends on:** MVP-001 (so the `IsValid` gate works during testing).
**Effort:** M (half a day).

---

### MVP-007 · Stabilise `FindAdjacentLoopTiles` ordering

**Labels:** `loop-subsystem`

`LoopDrawer.FindAdjacentLoopTiles` (`LoopDrawer.cs:155`) returns adjacent loop tiles in the order `Validator.FindInAdjacent` yields them, which is the order of `Hex.Neighbours()` — currently the axial-direction enumeration order. `TryToDrawLoop` depends on this ordering for direction assignment. Make it deterministic by sorting candidates by a consistent hex direction index, and handle incomplete loops (1 neighbour) gracefully.

**Acceptance criteria:**
- [ ] Adjacent loop tiles sorted by axial direction index before being returned.
- [ ] 1-neighbour case (open loop end) doesn't break the walk — produces a partial direction assignment.
- [ ] 3+ neighbour case still flagged as invalid.
- [ ] OnDrawGizmos debug view shows consistent arrows across multiple runs of the same board state.

**Depends on:** MVP-005 (renames clarify what we're stabilising).
**Effort:** M (half a day).

---

### MVP-008 · (Optional) Implement `LoopGraph.FindPathDFS` / `FindPathBFS`

**Labels:** `feature`, `loop-system`, `optional`

Stubbed at `LoopGraph.cs:123`. Not strictly required if `HasCycle` + `IsValid` cover the win gate — but useful if `TryToDrawLoop` should walk the cycle in order without manual stepping. Decide during MVP-007/MVP-009 whether the manual walk or graph pathfinding is cleaner.

**Acceptance criteria:**
- [ ] Either both helpers implemented and used by `TryToDrawLoop`, OR the stubs are deleted with a comment explaining why manual stepping is preferred.
- [ ] No `NotImplementedException` left in the codebase for these.

**Depends on:** MVP-006.
**Effort:** M (half a day) if implemented, S (15 minutes) if deleted.

---

## Milestone 3 — Loop drawing

### MVP-009 · Re-enable `TryToDrawLoop` with deterministic direction

**Labels:** `feature`, `loop-system`, `user-facing`

`Interchained.RedrawLoop` (`Interchained.cs:567`) currently calls only `SortLoopTiles` (the debug graph build) — the LineRenderer walk is commented out. Per AGENTS.md: this should land only after the gizmo representation is correct and deterministic.

**Acceptance criteria:**
- [ ] `TryToDrawLoop` un-commented and called from `RedrawLoop`.
- [ ] LineRenderer draws a closed loop when the cycle is complete.
- [ ] Partial (open) loops draw as an open chain, no crash, no infinite loop.
- [ ] Direction assignment survives: place a tile, remove it, place it again — same arrows.
- [ ] Build green; playtest in editor for 10 minutes without weird state.

**Depends on:** MVP-003 (encapsulated fields), MVP-007 (deterministic ordering).
**Effort:** L (1-1.5 days, including debugging).

---

## Milestone 4 — Win condition

### MVP-010 · Replace `IsSolved()` with the canonical "loop closed" check

**Labels:** `feature`, `win-condition`

`Interchained.IsSolved()` (`Interchained.cs:604`) currently checks `invalidTiles.Count == 0` — necessary but not sufficient (a half-solved board with no conflicts passes). Per AGENTS.md: the canonical win condition is "the loop is closed".

**Acceptance criteria:**
- [ ] `IsSolved()` returns `true` iff `LoopGraph.HasCycle()` AND no tiles are tracked invalid AND all tiles marked for the loop have degree exactly 2.
- [ ] Half-solved board with no conflicts → `false`.
- [ ] T-junction on loop → `false` (degree violation).
- [ ] Two disjoint cycles → `false` (not one loop).
- [ ] Single closed cycle, no conflicts → `true`.
- [ ] Build green.

**Depends on:** MVP-006 (HasCycle), MVP-009 (so "closed" is observable), MVP-001 (IsValid works).
**Effort:** M (half a day).

---

### MVP-011 · Route win state through `GameManager`

**Labels:** `feature`, `win-condition`, `game-flow`

`GameManager` already has `WaitingToStart` / `GamePlaying` / `GameOver` states but `GameOver` is commented out. Wire the new `IsSolved()` result through `GameManager.OnStateChanged` so the state machine is the single owner of game flow.

**Acceptance criteria:**
- [ ] On `IsSolved()` returning true, `GameManager` transitions to `GameOver`.
- [ ] `GameOver` state disables input on the board.
- [ ] State transition reproducible: solve a board → state changes; reset → back to `GamePlaying`.
- [ ] Build green.

**Depends on:** MVP-010.
**Effort:** S (2 hours).

---

### MVP-012 · Win overlay UX

**Labels:** `feature`, `ux`, `user-facing`

No win celebration currently exists in-game. Add a simple overlay: "Puzzle Complete" + "Play Again" button (mirror the prototype's pattern from `Web/Prototype/interchained_zai_prototype.html`).

**Acceptance criteria:**
- [ ] Overlay panel appears on transition to `GameOver`.
- [ ] "Play Again" resets non-locked tiles, returns to `GamePlaying`.
- [ ] Overlay skinnable via existing `BoardColors` ScriptableObject (no hard-coded colours).
- [ ] Build green; functions in editor playmode.

**Depends on:** MVP-011.
**Effort:** M (half a day).

---

## Milestone 5 — Gear-loop meshing

### MVP-013 · Implement gear-loop meshing validation (conveyor belt rule)

**Labels:** `feature`, `gear-rule`, `core-mechanic`

The last undefined gameplay rule per AGENTS.md. Any gear adjacent to a loop tile must be on the correct side of the loop flow: all CW gears (8) on one side, all CCW gears (9) on the other. Loop direction (CW/CCW) is discovered from gear clues as part of the puzzle.

**Acceptance criteria:**
- [ ] `Validator.GetDirectionBetween(a, b)` helper added.
- [ ] `Validator.GetLoopSide(gear, loopTile)` helper added.
- [ ] `Validator.InvalidGearLoopMeshing(TileData gear)` returns true if the gear is on the wrong side of the flow.
- [ ] Integrated into `EvaluationCycle` (runs after loop direction is established by MVP-009).
- [ ] Loop-invalid gears surface via the `IsOnLoopIncorrectly` flag pattern (or a sibling flag).
- [ ] Hand-tested with at least one puzzle that requires meshing.
- [ ] Build green.

**Depends on:** MVP-009 (direction established), MVP-010 (so win reflects gear validity).
**Effort:** L (1.5-2 days).

---

## Milestone 6 — MVP polish

### MVP-014 · MVP playtest sweep

**Labels:** `qa`, `playtest`

Build a small set of test puzzles (3-5 levels of varying complexity) and playtest end-to-end. Capture any state where the loop draws incorrectly, the win overlay mis-fires, or gear meshing flags the wrong tiles.

**Acceptance criteria:**
- [ ] 3-5 test levels committed under `Assets/Resources/Levels/` (or wherever `Levels.cs` expects them).
- [ ] Each level played to a win and to at least one known-invalid state.
- [ ] Bugs filed as follow-up issues (post-MVP).
- [ ] Build green on `main` / `ai-staging`.

**Depends on:** all of M0-M5.
**Effort:** M (1 day).

---

## Out-of-scope follow-ups (post-MVP backlog)

Tracked here so they aren't lost; not part of this project.

- `IWinCondition` strategy seam + multiple game modes (architecture review §3.8).
- Tutorial / onboarding + `InputActions` abstraction (architecture review §3.10).
- Save format centralisation as `TileData.Encode/Decode` (architecture review §3.5).
- Stop deriving `hex` from transform (architecture review §3.2).
- Drop speculative generality: `BiasCone.width`, unused region fields, `Link.IsDirectional` (standards review).
- Cleanup of committed debug toggles (`bool msg = true`, `Debug.Log` calls).
- Remove dead commented-out code (~60-line block in `CheckForPair`, etc.).
- Card Pickups subsystem review (out of gameplay loop, untouched here).
