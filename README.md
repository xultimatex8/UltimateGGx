# UltimateGGx

A web application for advanced analysis of **League of Legends** matches, built on the Riot Games API and on a reconstruction of each match's state from timeline data.

It lets you explore how a match unfolded and **simulate counterfactual scenarios** — modifying key events (a kill, securing an objective) at specific points in time — to estimate how the game state would have evolved under alternative conditions, comparing the real match to the hypothetical version and helping identify the decisive moments that shaped the final outcome.

## Main Features (MVP)

- Match and user search and full timeline reconstruction.
- Navigable timeline with key events (kills, objectives) marked.
- Dynamic charts for gold advantage, XP advantage, and estimated win probability.
- Counterfactual simulation: cancel out an event (kill or objective) and compare the real curve vs. the hypothetical one.

## Tech Stack

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
- [Decision Log](docs/decisions.md)

## Running the Project

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

RIOT_RATE_LIMIT_PER_SECOND=20
RIOT_RATE_LIMIT_PER_SECOND_WINDOW=1
RIOT_RATE_LIMIT_PER_WINDOW=100
RIOT_RATE_LIMIT_WINDOW_MINUTES=2

RIOT_API_KEY=your_riot_api_key
```

The Riot rate limit variables should match the limits associated with your Riot API key.

- `RIOT_RATE_LIMIT_PER_SECOND`: Maximum number of requests allowed within the short time window.
- `RIOT_RATE_LIMIT_PER_SECOND_WINDOW`: Duration (in seconds) of the short time window.
- `RIOT_RATE_LIMIT_PER_WINDOW`: Maximum number of requests allowed within the longer time window.
- `RIOT_RATE_LIMIT_WINDOW_MINUTES`: Duration (in minutes) of the longer time window.

For a standard Riot development API key, the default values are:

- **20 requests every 1 second**
- **100 requests every 2 minutes**

If you use a personal or production API key with different limits, update these values accordingly.

## 3. Run the project

The project can be run in two different ways:

- **Development mode**: run the backend and frontend natively while using PostgreSQL in Docker. This is recommended when developing the application.
- **Production mode (Docker)**: run the entire stack (database, backend, and frontend) in Docker containers.

### Development mode

#### Start PostgreSQL

From the project root:

```bash
docker compose up postgres -d
```

Verify it's running:

```bash
docker compose ps
```

#### Start the backend

```bash
cd backend
dotnet restore
dotnet watch run
```

The API will be available at `http://localhost:5037`.

> Database migrations are applied automatically when the application starts.

#### Start the frontend

```bash
cd frontend
npm install
ng serve
```

The application will be available at `http://localhost:4200`.

#### Day-to-day development

During development, the following three processes should be running simultaneously:

1. **PostgreSQL**
```bash
docker compose up postgres -d
```

2. **Backend**
```bash
cd backend
dotnet watch run
```

3. **Frontend**
```bash
cd frontend
ng serve
```

### Production mode (Docker)

Build and start the entire application stack:

```bash
docker compose up --build
```

The application will be available at `http://localhost`.

To stop all containers, press `Ctrl + C` if `docker compose up` is running in the foreground. If the containers are running in detached mode, stop and remove them with:

```bash
docker compose down
```

This stops all services while preserving the PostgreSQL data stored in the Docker volume.

## Legal notice

This project uses Riot Games' public API in accordance with their [developer policies](https://developer.riotgames.com/policies/general).

UltimateGGx is not endorsed by Riot Games and does not reflect the views or opinions of Riot Games or anyone officially involved in producing or managing Riot Games properties. Riot Games and all associated properties are trademarks or registered trademarks of Riot Games, Inc