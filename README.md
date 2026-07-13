# UltimateGGx

A web application for advanced analysis of **League of Legends** matches, built on the Riot Games API and on a reconstruction of each match's state from timeline data.

It lets you explore how a match unfolded and **simulate counterfactual scenarios** — modifying key events (a kill, securing an objective) at specific points in time — to estimate how the game state would have evolved under alternative conditions, comparing the real match to the hypothetical version and helping identify the decisive moments that shaped the final outcome.

## Main features (MVP)

- Match and user search and full timeline reconstruction.
- Navigable timeline with key events (kills, objectives) marked.
- Dynamic charts for gold advantage, XP advantage, and estimated win probability.
- Counterfactual simulation: cancel out an event (kill or objective) and compare the real curve vs. the hypothetical one.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET) + C# |
| ORM  | Entity Framework Core |
| Database | PostgreSQL |
| Frontend | Angular + TypeScript |
| Styling | Tailwind CSS |
| Containerization | Docker |

More detail and rationale behind these decisions in [`docs/tech-stack.md`](docs/tech-stack.md).

## Documentation

- [MVP Functional requirements](docs/mvp-functional-requirements.md)
- [Roadmap](docs/roadmap.md)
- [Tech Stack and Rationale](docs/tech-stack.md)

## Running the project

> Section to be completed during development (backend/frontend installation instructions and environment variables, including the Riot Games API key).

## Legal notice

This project isn't affiliated with Riot Games. League of Legends and Riot Games are trademarks of Riot Games, Inc. This application only consumes Riot Games' public API in accordance with their [developer policies](https://developer.riotgames.com/policies/general).
