
# Auth Service

This repository contains a standalone authentication and authorization service built with .NET and designed to be deployed as a microservice. It provides a robust and secure foundation for managing user identities, including local authentication (email/password), OAuth 2.0 logins with external providers (e.g., Google, GitHub), and a complete password management flow.

The service exposes both a RESTful API for traditional clients and a high-performance gRPC API for service-to-service communication.

## Features

- **Dual API**: Provides both RESTful and gRPC APIs for flexible integration.
- **Asymmetric JWT Signing**: Uses the RS256 (RSA) algorithm, a security best practice. Consumer services only need the public key to validate tokens, not a shared secret.
- **OpenID & JWKS Endpoints**: Exposes public keys and configuration via standard `/.well-known/openid-configuration` and `/.well-known/jwks.json` endpoints for automatic key discovery and rotation.
- **Local Authentication**: Secure registration and login using email and a hashed password.
- **External Provider Login**: Easily link and authenticate with OAuth 2.0 providers.
- **Password Management**: Full flow for "forgot password" and "reset password" using secure, expiring tokens.
- **JWT & Refresh Tokens**: Issues short-lived JWTs for accessing resources and long-lived refresh tokens for maintaining sessions, with automatic token rotation.
- **Event-Driven Architecture**: Publishes events for key actions (e.g., `UserRegistered`, `UserLoggedIn`) to a message bus (NATS), allowing other services to react in a decoupled manner.
- **Clean Architecture**: Organized into distinct layers (Domain, Application, Infrastructure, API) for separation of concerns, testability, and maintainability.
- **Containerized**: Fully configured to run in Docker containers using `Dockerfile` and `docker-compose`.

## Technology Stack

- **Backend**: .NET, ASP.NET Core
- **APIs**: REST, gRPC
- **Database**: PostgreSQL (with Entity Framework Core)
- **Messaging**: NATS
- **Testing**: xUnit, Moq, Node.js (for integration tests)
- **Containerization**: Docker, Docker Compose

## Getting Started

### Prerequisites

- .NET SDK
- Docker Desktop
- Node.js and npm

### Running the Service

1.  **Clone the repository**:
    ```sh
    git clone <repository-url>
    cd <repository-folder>
    ```

2.  **Start the services using Docker Compose**:
    This command will build the Auth Service image and start the necessary containers, including the PostgreSQL database and NATS message broker.
    ```sh
    docker-compose up --build
    ```

## Development Environment

### Accessing the API (Swagger)

Once the service is running, you can explore and interact with the REST API using the built-in Swagger UI.

-   **Swagger UI URL**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

The Swagger interface provides detailed documentation for each endpoint, allows you to execute requests directly from the browser, and view the responses.

## API Reference

### REST API

| Endpoint                    | Method | Description                                                                 |
| --------------------------- | ------ | --------------------------------------------------------------------------- |
| `/api/auth/register`        | POST   | Registers a new user with an email and password.                            |
| `/api/auth/login`           | POST   | Authenticates a user and returns a JWT and refresh token.                   |
| `/api/auth/refresh`         | POST   | Issues a new JWT and refresh token in exchange for a valid refresh token.   |
| `/api/auth/validate`        | GET    | Validates the current user's token and returns their claims. (Requires Auth) |
| `/api/auth/forgot-password` | POST   | Initiates the password reset process for a user.                            |
| `/api/auth/reset-password`  | POST   | Completes the password reset process using a valid token.                   |
| `/.well-known/jwks.json`    | GET    | Exposes the public keys for validating JWTs.                                |
| `/.well-known/openid-configuration` | GET | Provides OpenID discovery information, including the JWKS URI. |

### gRPC API

For high-performance, strongly-typed service-to-service communication, the service exposes a gRPC API on port `5001`.

- **Protobuf Definition**: The service contract is defined in `src/Auth/Api/Protos/auth.proto`.

| RPC Method          | Description                                                    |
| ------------------- | -------------------------------------------------------------- |
| `Register`          | Registers a new user.                                          |
| `Login`             | Authenticates a user and returns tokens.                       |
| `RefreshToken`      | Issues a new set of tokens.                                    |
| `ForgotPassword`    | Initiates the password reset flow.                             |
| `ResetPassword`     | Completes the password reset flow.                             |
| `ValidateToken`     | Confirms if a given access token is valid.                     |
| `GetJwks`           | Returns the JSON Web Key Set for token signature validation.   |

## JWT Validation for Downstream Services

This service signs JWTs using an **asymmetric RS256 algorithm**. To validate a token, a downstream service (like an API Gateway or another microservice) **must** fetch the public key from the OIDC discovery document.

### Automatic Validation using OpenID Connect Discovery

The recommended approach for token validation is to use a JWT library that supports OpenID Connect (OIDC) discovery. The library will automatically handle the entire validation process.

**The validation flow works as follows:**
1.  The downstream service is configured with the **Authority URL** of this `auth-service`.
2.  On startup, the JWT middleware in the downstream service will query the `/.well-known/openid-configuration` endpoint.
3.  This configuration document tells the middleware where to find the JSON Web Key Set (JWKS) via the `jwks_uri` property.
4.  The middleware fetches the public keys from the JWKS endpoint and caches them.
5.  On every incoming request, the middleware validates the JWT's signature against the cached public keys, as well as the `iss` (issuer) and `aud` (audience) claims.

### Example: Configuring a .NET API Gateway

To configure a downstream .NET service (like an Ocelot or YARP gateway) to validate tokens from this Auth Service, you would add the following to its service configuration:

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // The public-facing address of the Auth Service.
        // The middleware will automatically append "/.well-known/openid-configuration" to this URL.
        options.Authority = "http://localhost:5000"; 

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 1. Validate the issuer claim.
            ValidateIssuer = true,
            ValidIssuer = "ck-auth-service", // Must match the "iss" claim in the JWT.

            // 2. Validate the audience claim.
            ValidateAudience = true,
            ValidAudience = "ck-gateways", // Must match the "aud" claim in the JWT.

            // 3. Validate the token's signature.
            ValidateIssuerSigningKey = true,

            // 4. Validate the token's lifetime.
            ValidateLifetime = true
        };
    });
```
This configuration correctly sets up the automatic discovery and validation process. The middleware handles all the complexity of fetching, caching, and using the public keys.

## Production Readiness and Security

The default configuration in this repository is designed for development and testing. Before deploying to a production environment, you **must** implement the following security best practices:

### 1. Set the Environment to Production

Set the `ASPNETCORE_ENVIRONMENT` variable to `Production` in your deployment configuration. This disables detailed error pages and enables other performance and security optimizations.

```yaml
# In your production docker-compose.yml or container configuration
environment:
  - ASPNETCORE_ENVIRONMENT=Production
```

### 2. Secure Secret Management

Do not store secrets directly in `docker-compose.yml` or configuration files. Use a secure secret management solution:

-   **Docker Secrets**: For Docker Swarm or single-node Docker deployments.
-   **Kubernetes Secrets**: For Kubernetes deployments.
-   **Cloud Provider Secret Managers**: Such as AWS Secrets Manager, Azure Key Vault, or Google Secret Manager.
-   **HashiCorp Vault**: A dedicated secrets management tool.

**Critical secrets to manage:**
-   `DataProtection__CertificatePassword`
-   `ConnectionStrings__DefaultConnection`
-   Any OAuth client secrets you add.

### 3. Use a Production-Grade Database

The included PostgreSQL container is for development convenience. For production, you should:
-   Use a managed database service (e.g., Amazon RDS, Azure Database for PostgreSQL, Google Cloud SQL).
-   Ensure the database is properly secured, backed up, and monitored.

### 4. Enable HTTPS

Never run a production authentication service over plain HTTP.
-   **Configure a Reverse Proxy**: Place the service behind a reverse proxy (like NGINX, YARP, or a cloud load balancer) that terminates TLS/SSL.
-   **Update the Issuer URL**: Ensure the `Jwt:Issuer` configuration and the OpenID discovery document reflect the public-facing HTTPS URL.

### 5. Review and Harden Configuration

-   **JWT Settings**: Review the token expiration times in `JwtTokenGenerator.cs` to match your security requirements.
-   **Data Protection**: Ensure the data protection keys volume (`dp-keys`) is persisted to a reliable, backed-up storage location.

## Testing

### Running Integration Tests

The repository includes a Node.js-based integration test suite that covers the full authentication lifecycle, including NATS event verification.

1.  **Install dependencies**:
    ```sh
    npm install --prefix test/integration/api
    ```

2.  **Run the tests**:
    ```sh
    npm test --prefix test/integration/api
    ```
