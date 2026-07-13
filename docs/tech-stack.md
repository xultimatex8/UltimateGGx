# Tech Stack — UltimateGGx

## Final Decision

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET) + C# |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Frontend | Angular + TypeScript |
| Styling | Tailwind CSS |
| Containerization | Docker |

## Rationale

### Backend: ASP.NET Core

- Chosen as a primary learning goal, as .NET is widely used in enterprise software development (banking, insurance, ERP systems, and large consulting firms).
- Although the project combines API development with data processing, the MVP only requires business logic over structured data. This is well suited to **LINQ** and the .NET ecosystem, without requiring data science or machine learning tools.
- Python or a dedicated microservice would only become justified if the project later incorporates a machine learning model for win probability prediction, which is intentionally deferred until after the MVP.

### Architecture: Modular Monolith

A microservices architecture (for example, separating ingestion, analytics, or simulation into independent services) was considered but intentionally rejected.

- The application does not require independent scaling of its components.
- It is a single-developer project, so microservices would not provide the team-level development benefits they are designed for.
- A microservices approach would introduce unnecessary complexity without delivering meaningful benefits for this project's scope.

**Conclusion:** the application will be implemented as a modular monolith, with responsibilities separated into modules. If machine learning is added in the future, it would be the most suitable candidate to be extracted into an independent service.

### Frontend: Angular

- Selected as a learning objective.
- Angular pairs naturally with ASP.NET Core in many enterprise environments.
- Its TypeScript-first approach, dependency injection, services, and modular architecture share many concepts with ASP.NET Core, making both technologies complementary to learn together.

### ORM: Entity Framework Core

- The standard ORM in the .NET ecosystem.
- Simplifies data access while providing experience with one of the most widely used persistence technologies in modern ASP.NET Core applications.
- Integrates seamlessly with PostgreSQL through the Npgsql provider.

### Database: PostgreSQL

- Open-source, free, and widely adopted in production environments.
- A robust relational database suitable for storing cached matches, timelines, and application data.

### Containerization: Docker

- Docker is not strictly required for the MVP but significantly improves reproducibility and deployment.
- Containerizing the backend, frontend, and PostgreSQL database demonstrates familiarity with modern development workflows.