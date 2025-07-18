# Hotel Reservation API

This project is a backend API for a hotel reservation system, built with ASP.NET Core. It provides a complete solution for managing rooms, users, and reservations, with a focus on clean architecture and testability.

## Features

- **User Authentication:** Secure user signup and login using JSON Web Tokens (JWT).
- **Room Management:** Full CRUD (Create, Read, Update, Delete) functionality for hotel rooms.
- **Reservation System:** Allows users to create and view reservations.
- **Pagination:** Efficiently lists rooms, users, and reservations with paginated results.
- **Role-Based Access Control:** (Future) Foundation for different user roles like Guest and Administrator.
- **Developer-Friendly:** Includes a devcontainer setup for a consistent development environment.
- **Code Quality:** Enforces consistent code style using CSharpier.

## Technologies Used

- **Framework:** .NET 8 & ASP.NET Core
- **Database:** Entity Framework Core (with migrations for a relational database like PostgreSQL or SQL Server)
- **Testing:** xUnit, Moq, and AutoFixture
- **Containerization:** Docker & Devcontainers
- **Code Formatting:** CSharpier

## Getting Started

Follow these instructions to get the project up and running on your local machine.

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop) (Recommended, for using the devcontainer or Docker Compose)

### Local Development

1.  **Clone the repository:**

    ```bash
    git clone <repository-url>
    cd dgmx_practic_case
    ```
2.  **Install Dependencies:**
    Restore the .NET packages for the solution.

    ```bash
    dotnet restore backend/backend.sln
    ```
3.  **Database Migrations:**
    Apply the Entity Framework migrations to set up the database schema.
    ```bash
    dotnet ef database update --project backend/api
    ```

4.  **Run the Application:**
    To start the API, run the following command. By default, it will be accessible at `https://localhost:7053` and `http://localhost:5139`.

    ```bash
    dotnet run --project backend/api
    ```

5.  **Running Tests:**
    To run the full suite of unit tests, execute the following command from the root directory:

    ```bash
    dotnet test backend/backend.sln
    ```

### Devcontainer (Recommended)

If you are using Visual Studio Code, you will be prompted to "Reopen in Container". This will build the development environment with all necessary dependencies, providing a consistent and isolated development environment.

## Running with Docker

This project is fully configured to run with Docker, providing a consistent environment for both development and production.

### Using Docker Compose (Recommended)

Docker Compose is the easiest way to get the application running, as it orchestrates the build process and service configuration for you.

#### Development Environment

The development setup uses `Dockerfile.dev` to build a container with the .NET SDK, enabling features like hot-reloading.

1.  **Build and run the container:**
    From the root directory, run the following command. The `UID` and `GID` variables ensure that the files created inside the container match your user's permissions on the host machine.

    ```bash
    UID=$(id -u) GID=$(id -g) docker-compose -f backend/docker-compose.yml up --build -d
    ```

2.  **Access the running application:**
    The API will be available at `http://localhost:5000` and `https://localhost:5001`.

3.  **Access the container shell:**
    If you need to run commands inside the container (e.g., `dotnet restore`), you can use:
    ```bash
    docker exec -it dgmx_practice bash
    ```

#### Production Environment

The production setup uses a multi-stage `Dockerfile.prod` to build a small, optimized, and secure image for deployment.

1.  **Build and run the container:**
    From the root directory, run:

    ```bash
    docker-compose -f backend/docker-compose.prod.yml up --build -d
    ```

2.  **Access the running application:**
    The API will be available at `http://localhost:8080`.

### Using Docker Manually (Without Compose)

You can also build and run the Docker images manually without using Docker Compose.

#### Development Image

1.  **Build the image:**

    ```bash
    docker buildx build -t hotel-api:dev -f backend/api/Dockerfile.dev --build-arg UID=$(id -u) --build-arg GID=$(id -g) . --load
    ```

2.  **Run the container:**
    ```bash
    docker run -d -p 5001:5001 -p 5000:5000 -v ./backend:/app/backend --name hotel-api-dev-container hotel-api:dev
    ```

#### Production Image

1.  **Build the image:**

    ```bash
    docker buildx build -t hotel-api:prod -f backend/api/Dockerfile.prod . --load
    ```

2.  **Run the container:**
    ```bash
    docker run -d -p 8080:80 --name hotel-api-prod-container hotel-api:prod
    ```

## API Endpoints

Here is a summary of the main API endpoints available.

### Authentication

- `POST /api/Auth/signup`: Register a new user.
- `POST /api/Auth/login`: Log in an existing user and receive a JWT.

### Rooms

- `GET /api/Rooms`: Get a paginated list of rooms.
- `POST /api/Rooms`: Create a new room (Admin only).
- `GET /api/Rooms/{IdRoom}`: Get a specific room by its ID.
- `PUT /api/Rooms/{IdRoom}`: Update an existing room (Admin only).
- `GET /getRoomType`: Get a list of all available room types.

### Users

- `GET /api/Users`: Get a paginated list of registered users (Admin only).

### Reservations

- `GET /api/Reservation`: Get a paginated list of reservations.
- `POST /api/Reservation`: Create a new reservation.
- `GET /api/Reservation/{IdRes}`: Get a specific reservation by its ID.

## Code Style

This project enforces consistent code style using **CSharpier** and **.editorconfig**.

- **.editorconfig**: Defines basic code style rules (indentation, line endings, etc.) that most IDEs and editors will respect.
- **CSharpier**: A code formatter that automatically formats C# code to a consistent style.

To format the entire codebase and ensure adherence to the defined style, run the following command from the `backend` directory:

```bash
dotnet csharpier .
```
## Project Structure

The solution is organized into two main projects:

- `backend/api/`: The main ASP.NET Core Web API project. It contains the controllers, services, repositories, data models, and business logic.
- `backend/apiTest/`: The unit testing project. It contains tests for the API controllers and services, following the structure of the main project.
