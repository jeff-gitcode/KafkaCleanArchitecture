# Kafka Clean Architecture

This project is a .NET Core application that implements a clean architecture pattern with a focus on CQRS (Command Query Responsibility Segregation) and integrates with Kafka for messaging.

## Project Structure

- **src**: Contains the main application code.
  - **Application**: Contains commands, queries, and interfaces.
    - **Commands**: Command implementations for modifying state.
    - **Queries**: Query implementations for retrieving data.
    - **Interfaces**: Interfaces for application services.
  - **Domain**: Contains the core domain entities and events.
    - **Entities**: Domain entities representing the business model.
    - **Events**: Domain events that signify changes in state.
  - **Infrastructure**: Contains implementations for external services.
    - **Kafka**: Kafka producer and consumer implementations.
    - **Persistence**: Database context for Entity Framework Core.
  - **WebApi**: Contains the Web API layer.
    - **Controllers**: API controllers for handling HTTP requests.

- **tests**: Contains unit tests for the application.
  - **Application.Tests**: Tests for application commands and queries.
  - **Domain.Tests**: Tests for domain entities.
  - **Infrastructure.Tests**: Tests for infrastructure components.

## Setup Instructions

1. Clone the repository:
   ```
   git clone <repository-url>
   cd KafkaCleanArchitecture
   ```

2. Restore the NuGet packages:
   ```
   dotnet restore
   ```

3. Update the configuration for Kafka in the `appsettings.json` file.

4. Start Kafka using Docker Compose: Ensure you have Docker installed, then run:
    ```
    {
      "Kafka": {
        "BootstrapServers": "localhost:9092"
      }
    }
    ```
5. Run the application:
   ```
   dotnet run --project src/WebApi/WebApi.csproj
   ```

## Usage

- Use the API endpoints defined in `EntityController` to create and retrieve entities.
- The application uses Kafka for messaging, allowing for asynchronous processing of commands and events.

## Architecture

This project follows the Clean Architecture principles, separating concerns into different layers. The CQRS pattern is implemented to handle commands and queries distinctly, promoting a clear separation of responsibilities.

## Contributing

Contributions are welcome! Please submit a pull request or open an issue for any suggestions or improvements.