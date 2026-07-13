# W/L Metrics — UltimateGGx

**Cumulative objective metrics** (straightforward to compute directly from the timeline) with an **aggregate win probability metric** (heuristic, weighting several signals) are combined.

## Objective metrics (per frame/time)

| Metric | What it measures | How it changes with match events |
|---|---|---|
| **Gold advantage (Gold Diff)** | Accumulated total gold difference between teams | Increases with kills, assists, objectives, creeps, destroyed towers. The most direct and standard signal in LoL. |
| **XP advantage (XP Diff)** | XP/level difference between teams | Increases with kills, farming, and neutral objectives that grant shared XP. Determines, for some champions, their "power spikes". |
| **Objective control (Objectives Diff)** | Number of dragons, heralds, barons, towers and inhibitors per team | Each objective grants a discrete advantage jump (gold/buffs/map). Baron Nashor, Dragon Soul (4th dragon for a team) and Ancient Dragon have an especially high impact. |

## Aggregate metric: dynamic win probability

The project's core output is the estimated win probability for each team.

Rather than relying directly on raw metrics, the evaluation engine interprets the current game state by combining objective metrics with contextual information.

The heuristic model considers factors such as:

- Gold advantage.
- XP/level advantage.
- Objective control.
- Match time.
- Champion importance within each team (gold share, level, role and shutdown bounty).

The result is a single win probability value for each point in the match timeline.

### Future extension (post-MVP)

Train a statistical model (logistic regression or gradient boosting) on a historical match dataset, using the game state at each frame as features and the final result as the target. This is the more robust approach, but requires a data-collection and training phase.

## How metrics change with specific events (examples)

- **A kill** → immediate jump in gold diff in favor of the killing team; can accelerate securing an objective (chain effect).
- **Neutral objective (dragon/herald/baron)** → discrete jump in objective control and team gold/buffs; baron in particular causes a strong jump in win probability due to its impact on tower pushes.
- **Losing a tower** → gold impact (tower bounty for the rival team) and objective/map control impact (loss of pressure in that area).
