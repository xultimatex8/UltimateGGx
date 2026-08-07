# Roadmap — UltimateGGx (1-Month MVP)

The following phases are intended as guidance rather than fixed deadlines for an intensive solo development cycle. Since the chosen stack includes new technologies (.NET and Angular), dedicated learning time is intentionally scheduled before implementation begins.

| Phase | Approx. duration | Scope | Depends on |
|---|---:|---|---|
| **Phase 0 — Learning & Project Setup** | **3–4 days** | Crash course on ASP.NET Core, Entity Framework Core, PostgreSQL and Angular; obtain Riot API key; explore the API; initialize project structure and Docker environment | — |
| **Phase 1 — Data Ingestion** | **3 days** | FR-B01 to FR-B06: summoner search, match retrieval, timeline download, caching and error handling | Phase 0 |
| **Phase 2 — Match State Reconstruction** | **5–6 days** | FR-B07 and FR-B08: parse the timeline and reconstruct the complete match state at every timestamp | Phase 1 |
| **Phase 3 — Game Evaluation Engine** | **4–5 days** | FR-B09 to FR-B12: compute match metrics, evaluate contextual event impact and implement the heuristic win probability model | Phase 2 |
| **Phase 4 — Frontend MVP** | **4–5 days** *(can overlap with Phase 3)* | FR-F01 to FR-F04: match selection, timeline navigation, charts and event markers | Phase 3 |
| **Phase 5 — Counterfactual Simulation** | **4–5 days** | FR-B13 to FR-B15 and FR-F05 to FR-F06: modify an event, recompute the evaluation and compare the original and simulated timelines | Phases 2–4 |
| **Phase 6 — Polish & UX** | **2 days** | FR-F07, loading/error states, minor UI improvements and API refinements | Phases 4–5 |
| **Phase 7 — Validation & Heuristic Tuning** | **2–3 days** | Validate the evaluation model against real matches and fine-tune heuristic weights and impact rules | Entire project |

## Identified Risks

- **Combined learning curve (.NET + Angular):** the primary schedule risk. Mitigation: dedicate Phase 0 exclusively to learning and project setup.
- **Heuristic evaluation model:** designing a believable evaluation model requires experimentation. The initial version should remain intentionally simple and be refined during the validation phase.
- **Counterfactual simulation:** the most conceptually complex feature. If time becomes limited, reduce its scope.

## Post-MVP Backlog

See the "Out of scope for the MVP" section in [MVP Functional requirements](mvp-functional-requirements.md).