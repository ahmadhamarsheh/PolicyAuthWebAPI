# PolicyAuthWebAPI

## Overview

`PolicyAuthWebAPI` is an ASP.NET Core Web API project designed to manage user accounts with JWT-based authentication and custom authorization policies. It includes features for user registration, login, and role-based access control.

![image](https://github.com/user-attachments/assets/2792e6d0-c40e-47d3-b6fd-2e9341d65b46)


## Features

- **User Registration:** Allows new users to register with email, password, role, and date of birth.
- **User Login:** Authenticates users and generates JWT tokens.
- **Role-Based Authorization:** Custom policies for different user roles.
- **Password Management:** Password complexity requirements enforced by ASP.NET Core Identity.

## Prerequisites

- .NET 8 SDK or later
- SQL Server or another supported database
- Postman or Curl for testing API endpoints

## Setup

1. **Clone the Repository**

    ```bash
    git clone https://github.com/yourusername/PolicyAuthWebAPI.git
    cd PolicyAuthWebAPI
    ```

2. **Install Dependencies**

    Make sure you have the necessary packages installed by running:

    ```bash
    dotnet restore
    ```

3. **Configure the Database**

    Update the connection string in `appsettings.json` to match your database configuration:

    ```json
    "ConnectionStrings": {
      "Default": "Server=your_server;Database=your_db;User Id=your_user;Password=your_password;"
    }
    ```

4. **Run Migrations**

    Apply database migrations:

    ```bash
    dotnet ef database update
    ```

5. **Run the Application**

    Start the API:

    ```bash
    dotnet run
    ```

## API Endpoints

### User Registration

- **Endpoint:** `POST /account/create`
- **Description:** Registers a new user.
- **Request Body:**

    ```json
    {
      "Email": "user@example.com",
      "Password": "Password123!",
      "Role": "admin",
      "DateOfBirth": "2000-01-01T00:00:00"
    }
    ```

- **Responses:**
  - `200 OK`: User registered successfully.
  - `400 Bad Request`: Invalid request or failed registration.

### User Login

- **Endpoint:** `POST /account/login`
- **Description:** Authenticates a user and returns a JWT token.
- **Request Body:**

    ```json
    {
      "email": "user@example.com",
      "password": "Password123!"
    }
    ```

- **Responses:**
  - `200 OK`: JWT token returned.
  - `404 Not Found`: User not found.
  - `400 Bad Request`: Invalid credentials.

### Authorized Endpoints

- **Endpoint:** `GET /list`
  - **Description:** Accessible to users with `admin` or `manager` roles.
- **Endpoint:** `GET /single`
  - **Description:** Accessible to users with `admin` or `user` roles.
- **Endpoint:** `GET /home`
  - **Description:** Accessible to users with `admin`, `manager`, or `user` roles.

## Testing

Use Postman or Curl to test the API endpoints. Ensure that you have a valid JWT token for protected endpoints.

## Contributing

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Commit your changes and push to your fork.
4. Create a pull request to the main repository.

## Copyright

© 2024 Ahmad Hamarsheh. All rights reserved.

