// Clean Architecture Solution Structure for Haven
// This README describes the purpose of each layer.

# API
- Entry point for HTTP requests (controllers, SignalR hubs)
- Handles authentication, routing, and response formatting

# Application
- Contains business logic, use cases, CQRS handlers, and service interfaces
- Coordinates between API and Domain layers

# Domain
- Core business models, entities, value objects, and domain services
- Contains business rules and logic

# Infrastructure
- Data access (repositories for PostgreSQL/MongoDB), external service integrations (RabbitMQ, Azure, etc.)
- Implements interfaces defined in Application layer

---

## Getting Started
- Place controllers and SignalR hubs in API
- Define use cases, commands, queries, and interfaces in Application
- Model entities and business logic in Domain
- Implement repositories, messaging, and integrations in Infrastructure

Follow Clean Architecture principles for maintainability and scalability.
