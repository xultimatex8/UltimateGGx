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
- [Match Metrics](docs/metrics.md)
- [Data Model](docs/data-model.md)
- [Roadmap](docs/roadmap.md)
- [Tech Stack and Rationale](docs/tech-stack.md)

## Running the project

### Prerequisites

- [Git](https://git-scm.com/downloads)
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- A [Riot Games API key](https://developer.riotgames.com/)

> There is no need to install PostgreSQL locally — it runs via Docker.

### 1. Clone the repository

```bash
git clone <REPOSITORY_URL>
cd UltimateGGx
```

### 2. Set up environment variables

Create the `.env` file at the project root:

```bash
cp .env.example .env
```

Edit `.env` and fill in the values:

```env
POSTGRES_USER=your_user
POSTGRES_PASSWORD=your_password
POSTGRES_DB=ultimateggx

RIOT_API_KEY=your_riot_api_key
```

### 3. Start PostgreSQL

From the project root:

```bash
docker compose up postgres -d
```

Verify it's running:

```bash
docker compose ps
```

### 4. Set up the backend

```bash
cd backend
dotnet restore
```

Apply database migrations:

```bash
dotnet ef database update
```

> If you don't have `dotnet-ef` installed yet:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

Start the backend:

```bash
dotnet watch run
```

The API will be available at `http://localhost:5037`.

### 5. Set up the frontend

```bash
cd frontend
npm install
```

Start the frontend:

```bash
ng serve
```

The app will be available at `http://localhost:4200`.

### Day-to-day development

Once everything is set up, three things need to be running simultaneously:

**1 — Database (Docker):**
```bash
docker compose up postgres -d
```

**2 — Backend:**
```bash
cd backend
dotnet watch run
```

**3 — Frontend:**
```bash
cd frontend
ng serve
```

### Running everything with Docker (alternative)

Instead of running the backend and frontend natively, the full stack (database, backend, and frontend) can be built and run in containers:

```bash
docker compose up --build
```

The app will be available at `http://localhost`.

## Legal notice

This project isn't affiliated with Riot Games. League of Legends and Riot Games are trademarks of Riot Games, Inc. This application only consumes Riot Games' public API in accordance with their [developer policies](https://developer.riotgames.com/policies/general).
