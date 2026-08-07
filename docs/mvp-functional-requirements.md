# Functional Requirements — UltimateGGx (MVP)

## Backend

### Data ingestion (Riot API)
- **FR-B01**: Search for a summoner and retrieve their recent match history.
- **FR-B02**: Download the full data for a specific match by its ID.
- **FR-B03**: Download the match timeline.
- **FR-B04**: Handle Riot API rate limits.
- **FR-B05**: Cache already-downloaded match data to avoid unnecessary API calls.
- **FR-B06**: Return clear, handled errors.

### Match state reconstruction
- **FR-B07**: Parse the timeline and reconstruct the match state for every available timestamp (gold, XP, levels, objectives and approximate champion positions).
- **FR-B08**: Extract and classify discrete game events (kills, objectives, towers, inhibitors).

### Game evaluation
- **FR-B09**: Compute match metrics for any point in time (gold difference, XP difference, objectives, towers, etc.).
- **FR-B10**: Evaluate the contextual impact of game events based on factors such as champion role, gold share, shutdown bounty, level and game state.
- **FR-B11**: Compute the dynamic win probability using the defined heuristic evaluation model.
- **FR-B12**: Expose the reconstructed state, metrics and evaluation through the API.

### Counterfactual simulation
- **FR-B13**: Allow cancelling or reversing a kill or objective event at a specific point in the timeline.
- **FR-B14**: Recompute the match state and evaluation from that point onward.
- **FR-B15**: Return both the original and simulated timelines so the frontend can compare them.

## Frontend

- **FR-F01**: User search form and selection of a match from the history.
- **FR-F02**: Main view with a timeline scrubber to navigate through the match.
- **FR-F03**: Time-series charts for gold advantage and win probability synchronized with the selected timestamp.
- **FR-F04**: Timeline markers for relevant game events.
- **FR-F05**: Simulation panel allowing the user to modify an event.
- **FR-F06**: Side-by-side comparison of the original and simulated evaluations.
- **FR-F07**: Visible loading and error states.

## Out of scope for the MVP (post-MVP backlog)

- Interactive minimap with champion positions.
- Simulation of positioning changes during teamfights.
- Team composition analysis.
- Vision and map-control metrics.
- ML-based win probability model.
- User accounts and saved analyses.