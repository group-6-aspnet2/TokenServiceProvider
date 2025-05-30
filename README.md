# TokenServiceProvider

## Overview

**TokenServiceProvider** is a gRPC-based microservice responsible for generating and validating JWT tokens. It exposes endpoints for token operations and integrates with external services (e.g., AccountService and AuthService) via HTTP calls.

## Features

- **GenerateTokenAsync**: Creates JWT tokens enriched with custom claims (userId, email, role).
- **ValidateTokenAsync**: Validates existing JWT tokens and verifies user authenticity.
- gRPC endpoints defined in `Protos/TokenManager.proto`.
- Asynchronous HTTP calls to external services for additional validation.
- Comprehensive **XUnit** test suite covering business logic and gRPC service.
- PlantUML sequence diagrams illustrating each method in TokenService.

## Technologies Used

- [.NET 8](https://dotnet.microsoft.com/download)
- ASP.NET Core gRPC
- `Microsoft.IdentityModel.Tokens` for JWT creation and validation
- `Grpc.Net.Client` and `Grpc.AspNetCore` for gRPC communication
- `HttpClient` for external HTTP requests
- [XUnit](https://xunit.net/) for unit testing
- [PlantUML](https://plantuml.com/) for sequence diagrams

## Architecture Overview

├── Presentation                  # gRPC service implementation (TokenManagerService)
│   ├── Protos/TokenManager.proto # Protobuf definitions
│   └── Services/TokenManagerService.cs
├── Business                      # Core business logic (TokenService)
│   ├── Interfaces/ITokenService.cs
│   └── Services/TokenService.cs
├── Tests                         # XUnit test project
│   └── TokenServiceProvider.Tests
├── docs
│   └── diagrams
│       ├── GenerateTokenSequence.png
│       └── ValidateTokenSequence.png
└── README.md                     # This file

## Prerequisites

- .NET SDK 8.0 or later
- Environment variables:
  - `Issuer` – token issuer
  - `Audience` – token audience
  - `SecretKey` – secret key used for signing
  - `GenerateTokenUri` – URL for external token generation service
  - `ValidateTokenUri` – URL for external token validation service
- (Optional) PlantUML renderer to view sequence diagrams

## Sequence Diagrams

Below are the sequence diagrams for the methods in **TokenService**:

### GenerateTokenAsync
![GenerateTokenAsync](https://github.com/user-attachments/assets/28744c4b-e15a-4ce8-8a0f-e7bfd964093e)

### ValidateTokenAsync 
![ValidateTokenAsync](https://github.com/user-attachments/assets/3210df11-5f51-4334-9ad0-eec57121a879)

## Testing

Tested using xUnit testing.
