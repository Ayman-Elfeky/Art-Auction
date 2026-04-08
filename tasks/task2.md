Responsibility:
  - Register command (Artist & Buyer)
  - Login command
  - JWT token generation flow
  - Password hashing with BCrypt
  - AuthController

Files you own:
  ├── Application/Features/Auth/
  │   ├── Commands/
  │   │   ├── Register/
  │   │   │   ├── RegisterCommand.cs
  │   │   │   ├── RegisterCommandHandler.cs
  │   │   │   └── RegisterCommandValidator.cs
  │   │   └── Login/
  │   │       ├── LoginCommand.cs
  │   │       ├── LoginCommandHandler.cs
  │   │       └── LoginCommandValidator.cs
  │   └── DTOs/
  │       ├── AuthResponseDto.cs
  │       ├── RegisterRequestDto.cs
  │       └── LoginRequestDto.cs
  │
  └── API/Controllers/
      └── AuthController.cs

Endpoints to implement:
  POST /api/auth/register
  POST /api/auth/login

Business Rules:
  ✅ Buyers are auto approved on register
  ✅ Artists need admin approval before they can post
  ✅ Password must be hashed with BCrypt
  ✅ Returns JWT token on success

Estimated time: 1–2 days