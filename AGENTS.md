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
  - **Side-detection algorithm (for `Validator.GetLoopSide`, MVP-013):** Given a gear adjacent to a loop tile with established direction:
    - `entryDir = GetDirectionBetween(loopTile.LoopIn, loopTile)` — direction the loop enters from.
    - `gearDir   = GetDirectionBetween(loopTile, gear)` — direction from loop tile to gear.
    - `diff = (gearDir - entryDir + 6) % 6`:
      - `1 or 2` → side A
      - `4 or 5` → side B
      - `0 or 3` → gear is on the loop axis (should not occur for a valid puzzle).
    - Collect `(gear, loopTile, side)` tuples, then enforce: same side ⇒ same gear type; opposite sides ⇒ opposite gear types. Either CW (8) or CCW (9) can be on side A — the puzzle is solved by discovering which.

### Input
| Input | Action |
|---|---|
| Left click | Increment tile |
| Right click | Decrement tile |
| Long press / Enter / L | Toggle loop mark |
| Space | Swap domino pair values / toggle gear (8↔9) |
| 1–9 / A / B / X / Y | Set specific value |
| Delete / Backspace | Clear tile |

## Development Workflow

### Build verification
After any C# change, verify the project compiles from the CLI:
```bash
dotnet build Assembly-CSharp.csproj
```
Unity-generated `.csproj` files are MSBuild-compatible, so this catches type errors, missing references, and accidental regressions without needing to open the Unity editor. Run it after every change before considering work complete.

### Active project: MVP refactor
The current focus is bringing Interchained to a Minimum Viable Product state — loop mechanics fully working, canonical win condition enforced, gear-loop meshing rule implemented. Two artefacts track this work:
- **`MVP_REFACTOR.md`** (repo root) — 14 issues across 7 milestones, with acceptance criteria, dependencies, and a dependency graph. Read this before starting any of the next-steps work below.
- **Gitea project board** — http://192.168.50.10:3000/fearian/Interchained/projects/1 ("Interchained MVP build and refactor"). Note: the projects API is not exposed on this Gitea instance (swagger lists 300 paths, zero for `/projects`), so the project description must be edited through the web UI rather than programmatically. The general issues/milestones/repos APIs work normally with a personal access token.

### Gitea CLI access (tea)
Gitea issues, milestones, and repos are managed via the `tea` CLI (the project board API is unavailable — see above). Two environment quirks must be worked around or every `tea` invocation silently hangs forever in non-interactive shells (including this tool's bash environment):

1. **PATH resolves to a 0-byte WinGet reparse point** that non-interactive callers cannot dereference. Always call the real binary by full path: `C:\Users\Fearian\AppData\Local\Microsoft\WinGet\Packages\Gitea.tea_Microsoft.Winget.Source_8wekyb3d8bbwe\tea.exe`.
2. **`tea` blocks waiting on a TTY** even for `--version`. Redirect stdin from an empty file via `Start-Process`.

Use this wrapper helper for every call — the stored `fearian` login is auto-selected, so **no token paste is needed**:
```powershell
function Invoke-Tea([string[]]$TeaArgs) {
  $tea = "C:\Users\Fearian\AppData\Local\Microsoft\WinGet\Packages\Gitea.tea_Microsoft.Winget.Source_8wekyb3d8bbwe\tea.exe"
  $empty = "$env:TEMP\tea_empty_stdin"; Set-Content $empty -Value "" -NoNewline
  $argString = (($TeaArgs | ForEach-Object { if ($_ -match '\s') { "`"$_`"" } else { $_ } }) -join ' ')
  $p = Start-Process $tea -ArgumentList $argString -NoNewWindow -PassThru `
       -RedirectStandardOut "$env:TEMP\tea_out" -RedirectStandardError "$env:TEMP\tea_err" -RedirectStandardInput $empty
  if (-not $p.WaitForExit(20000)) { $p.Kill(); throw "tea timed out" }
  [pscustomobject]@{ Code=$p.ExitCode; Out=(Get-Content "$env:TEMP\tea_out" -Raw -EA SilentlyContinue); Err=(Get-Content "$env:TEMP\tea_err" -Raw -EA SilentlyContinue) }
}
```
The quote-on-spaces step is **critical**: `Start-Process -ArgumentList` joins array elements with spaces *without* re-quoting, so `@("-t","M0 - Unblocking fixes")` would otherwise land as title `M0` with the rest dropped. (Learned the hard way during the initial milestone import.)

Common operations:
```powershell
Invoke-Tea @("milestones","-r","fearian/Interchained","--state","all","-o","simple")                       # list milestones
Invoke-Tea @("milestones","create","-r","fearian/Interchained","-t","Title","-d","Desc","--state","open")   # create milestone
Invoke-Tea @("issues","-r","fearian/Interchained","--state","open","-o","simple")                          # list issues
Invoke-Tea @("issues","create","-r","fearian/Interchained","-t","Title","-d","Body","-m","M0 - Unblocking fixes")  # create issue in milestone
```
Always verify writes with a follow-up read. The API is public-readable, so unauthenticated GETs work for verification:
```powershell
Invoke-RestMethod -Uri 'http://192.168.50.10:3000/api/v1/repos/fearian/Interchained/milestones?state=all' | Select-Object id, title, state, description
```

#### Wiring issue dependencies (blocked-by)
Gitea exposes native blocked-by relationships via `POST /repos/{owner}/{repo}/issues/{index}/dependencies` — the path issue becomes blocked-by the body issue. `tea` has no subcommand for this, so use the REST API directly. **The body must be the full `IssueMeta` schema, not just `{issue_index}`** — sending only the index returns a misleading `404 repository does not exist [empty]`. Working pattern:
```powershell
$dep   = 6   # the dependent issue (blocked one)
$block = 5   # the blocking issue (prerequisite)
$body  = "{`"index`":$block,`"owner`":`"fearian`",`"repo`":`"Interchained`"}"
Invoke-RestMethod -Uri "$apiBase/repos/fearian/Interchained/issues/$dep/dependencies" -Method Post `
                  -Headers @{Authorization="token $token"} -Body $body -ContentType 'application/json'
# Verify: GET /issues/$dep/dependencies should now list issue #$block
```
(There's a parallel `/issues/{index}/blocks` endpoint with mirror semantics — path issue blocks body issue — but the `dependencies` form is less confusing to read.) The dependency is idempotent: re-POSTing an existing edge returns `issue dependency does already exist`, which is safe to ignore.

All 14 MVP issues (MVP-001 through MVP-014) are imported against the 7 milestones, with the dependency graph from `MVP_REFACTOR.md` wired up using this API. Browse them at http://192.168.50.10:3000/fearian/Interchained/issues — the current frontier (agent-grabbable, no open blockers) is **MVP-001, MVP-002, MVP-003, MVP-004** (all tagged `ready-for-agent`). `MVP_REFACTOR.md` remains the design source of truth; Gitea is the execution tracker.

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
The **canonical win condition is "the loop is closed"** — that single check is the central mechanic of the puzzle. The other apparent gates are *consequences*, not independent win conditions:
- A closed loop requires domino-paired segments, which implicitly requires values 1–6 placed.
- Sudoku validity is already enforced via `invalidTiles` (placement + loop-invalid as of the `IsOnLoopIncorrectly` work).
- "All tiles filled" and "regions complete with 1–9" fall out of the above naturally and should NOT be added as separate gates.

Currently `IsSolved()` checks `invalidTiles.Count == 0`, which is necessary but not sufficient — a half-solved board with no conflicts would pass. The real fix is to verify **the loop is closed** (last tile connects back to first via `LoopGraph.HasCycle()` or a successful `TryToDrawLoop` walk). This is blocked on the loop subsystem work (step #2 above and `LoopGraph.HasCycle` in step #3) and should be landed alongside it, not as a partial gate beforehand.

### 5. Level Design & Progression (Ongoing)
- More puzzle levels with varying gear/loop complexity
- A level-select UI
- Possibly a hint system for the "confusing" letter labels

### 6. Polish (Low Priority)
- Animated conveyor belt texture on loop LineRenderer
- Particle effects on gear meshing
- Sound design
