# FlowTrack

Proyecto de aprendizaje de .NET y C# centrado en arquitectura por bounded contexts, DDD, CQRS y TDD.

## Objetivo

Esta solution esta pensada para practicar:

- Fundamentos de C# aplicados en .NET (tipos, records, interfaces, async/await y DI).
- Modelado de dominio con DDD (Entities, Repositories, Exceptions de dominio).
- Separacion de responsabilidades con CQRS (Queries/Commands y sus handlers).
- Desarrollo guiado por pruebas con TDD (unit tests + integration tests).
- Composicion por contextos para evitar acoplamiento entre modulos.

## Stack

- .NET `10.0` + ASP.NET Core
- C#
- xUnit + Moq
- Entity Framework Core + PostgreSQL (Npgsql)
- Testcontainers para pruebas de integracion
- JWT + BCrypt
- Docker Compose para PostgreSQL local

## Arquitectura

La solution esta organizada por bounded context y por capas internas.

```text
src/
  apps/
    FlowtrackApi/                    # API HTTP (controllers, middleware, auth)
  contexts/
    Shared/
      Domain/
        bus/
          command/                   # Command Bus contracts
          query/                     # Query Bus contracts
          Event/                     # Domain Events contracts
        Dic/                         # Service Discovery attributes
        Mailer/                      # Mailer abstraction
        ValueObject/                 # Value Objects base
      Infrastructure/
        Bus/
          command/                   # InMemoryCommandBus + discover
          query/                     # InMemoryQueryBus + discover
          Event/                     # InMemoryDomainEventBus + dispatcher
        Mailer/                      # DummyMailer
    Iam/
      Auth/
        Application/
        Domain/
      User/
        Domain/
        Infrastructure/
      Shared/
        Domain/
        Infrastructure/

test/
  apps/
    FlowtrackApi/                    # E2E tests
  contexts/
    Shared/
    Iam/
```

### Principios aplicados

- DDD:
  - El dominio vive en `Domain`.
  - `AggregatedRoot` como clase base para entidades que publican eventos de dominio.
  - Value Objects (`Email`, `Password`) como tipos sellados con validacion propia.
  - Domain Events: bus, dispatcher y suscripcion automatica via atributo.
  - Abstracciones de infraestructura (`IMailer`, `IUserRepository`) desacoplan el dominio.
- CQRS:
  - Command Bus in-memory con descubrimiento automatico de handlers.
  - Query Bus in-memory con descubrimiento automatico de handlers.
  - Separacion completa: commands para escritura, queries para lectura.
- Service Discovery:
  - `ServiceAttribute` / `ProviderAttribute` para auto-registro en DI.
  - Escaneo de ensamblados al startup, eliminando registro manual.
- API Layer:
  - Controladores REST que orquestan queries y commands.
  - Mapeo automatico de excepciones de dominio a respuestas HTTP estandarizadas.
  - Esquema de autenticacion custom mediante cookie.
- TDD:
  - Tests unitarios orientados al comportamiento de handlers.
  - Tests de integracion contra PostgreSQL real con contenedores.
  - Tests E2E contra la API real con `WebApplicationFactory`.

## Estructura de proyectos

- `src/apps/FlowtrackApi/FlowtrackApi.csproj`
  - Capa de entrada HTTP: controllers, middleware, esquemas de autenticacion.
- `src/contexts/Shared/FlowTrack.Shared.csproj`
  - Codigo compartido transversal (contratos de bus, service discovery, mailer, value objects, utilidades de infraestructura).
- `src/contexts/Iam/FlowTrack.Iam.csproj`
  - Bounded context de identidad/autenticacion.
- `test/apps/FlowtrackApi/FlowtrackApiTest.csproj`
  - Tests E2E contra la API real.
- `test/contexts/Shared/FlowTrack.Shared.Test.csproj`
  - Utilidades base para tests (fixture compartida, carga de `.env`, object mother).
- `test/contexts/Iam/FlowTrack.Iam.Test.csproj`
  - Tests unitarios e integracion del contexto IAM.

## Variables de entorno

Copia el template y completa las variables necesarias (JWT, base de datos, URLs, etc.):

```bash
cp .env.template .env
```

Consulta `.env.template` para ver la lista completa de variables requeridas y sus valores por defecto.

## Requisitos

- SDK de .NET 10 instalado.
- Docker activo para ejecutar integration tests con Testcontainers.

## Comandos utiles

Levantar PostgreSQL local:

```bash
docker compose -f src/apps/FlowtrackApi/docker-compose.local.yml up -d
```

Desde la raiz de la solution:

```bash
dotnet restore FlowTrack.slnx
dotnet build FlowTrack.slnx
dotnet test FlowTrack.slnx
```

Ejecutar solo tests IAM:

```bash
dotnet test test/contexts/Iam/FlowTrack.Iam.Test.csproj
```

Ejecutar solo tests Shared:

```bash
dotnet test test/contexts/Shared/FlowTrack.Shared.Test.csproj
```

Ejecutar solo tests E2E:

```bash
dotnet test test/apps/FlowtrackApi/FlowtrackApiTest.csproj
```

## Ejemplos de flujo

**Signin (lectura/query):**

1. `SigninQryHandler` recibe email/password.
2. Busca usuario en `IUserRepository`.
3. Valida password con `IBcrypt`.
4. Genera access y refresh token con `AuthTokenGenerator`.
5. Devuelve `SigninSuccess`.

**Signup (escritura/command):**

1. `SignupCmdHandler` recibe datos de registro.
2. Valida reglas de dominio (email, password).
3. Crea la entidad raiz, que emite un evento de dominio.
4. El evento es despachado a suscriptores (ej: envio de email de activacion).

Ambos flujos muestran como se mantiene separada la logica de dominio de los detalles de infraestructura.

## Estado actual

Este repositorio esta en evolucion y enfocado en aprendizaje deliberado.

Ideas para siguientes iteraciones:

- Agregar pipeline CI para build + test automatico.
- Introducir proyecciones / read models.
- Añadir mas bounded contexts.
- Implementar event sourcing.
