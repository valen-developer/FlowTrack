# FlowTrack

Proyecto de aprendizaje de .NET y C# centrado en arquitectura por bounded contexts, DDD, CQRS y TDD.

## Objetivo

Esta solution esta pensada para practicar:

- Fundamentos de C# aplicados en .NET (tipos, records, interfaces, async/await y DI).
- Modelado de dominio con DDD (Entities, Repositories, Exceptions de dominio, Value Objects).
- Separacion de responsabilidades con CQRS (Queries/Commands y sus handlers).
- Desarrollo guiado por pruebas con TDD (unit tests + integration tests + E2E).
- Composicion por contextos para evitar acoplamiento entre modulos.
- Integracion con infraestructura real: RabbitMQ, Elasticsearch, PostgreSQL.

## Stack

- .NET `10.0` + ASP.NET Core
- C#
- xUnit + Moq
- Entity Framework Core + PostgreSQL (Npgsql)
- Testcontainers para pruebas de integracion
- JWT + BCrypt
- RabbitMQ (transporte de eventos externos)
- Elasticsearch (indexacion y busqueda de workspaces)
- Husky + dotnet-format (hooks de pre-commit y pre-push)
- Docker Compose para PostgreSQL, RabbitMQ y Elasticsearch local

## Arquitectura

La solution esta organizada por bounded context y por capas internas.

```text
src/
  apps/
    FlowTrackIamApi/                 # API HTTP de IAM (auth, users)
    FlowTrackWorkManagementApi/      # API HTTP de Work Management (workspaces)
    FlowTrackApiGateway/             # API Gateway / Reverse Proxy (YARP)
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
          Event/                     # InMemoryDomainEventBus + ExternalEventBus (RabbitMQ)
        Mailer/                      # DummyMailer
        DotEnv/                      # DotEnvCharger para carga de .env
        Bcrypt.cs                    # BCrypt wrapper
        JWTService.cs                # JWT token generation
        RabbitMq/                    # Conexion, suscripcion y configuracion RabbitMQ
        Persistence/                 # DbContext, migraciones compartidas
        Transactions/                # Unit of Work / transaction management
        HttpErrorResponses/          # Mapeo de excepciones a HTTP
        ServiceDiscover.cs           # Escaneo de ensamblados para DI
        DateTimeProvider.cs          # Abstraccion de tiempo
        EnvStore.cs                  # Acceso centralizado a variables de entorno
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
    WorkManagement/
      Shared/
        Infrastructure/
      Workspaces/
        Application/
        Domain/
        Infrastructure/

test/
  apps/
    FlowTrackIamApi/                 # E2E tests contra FlowTrackIamApi
  contexts/
    Shared/                          # Test utilities (fixtures, object mothers)
    Iam/                             # Unit + integration tests
    WorkManagement/                  # Unit + integration tests
```

### Principios aplicados

- DDD:
  - El dominio vive en `Domain/`.
  - `AggregatedRoot` como clase base para entidades que publican eventos de dominio.
  - Value Objects (`Email`, `Password`, `UserId`) como tipos sellados con validacion propia.
  - Domain Events: bus, dispatcher y suscripcion automatica via atributo.
  - Abstracciones de infraestructura (`IMailer`, `IUserRepository`, `IWorkspaceRepository`) desacoplan el dominio.
  - External Events: eventos de dominio que se propagan a otros bounded contexts via RabbitMQ.
- CQRS:
  - Command Bus in-memory con descubrimiento automatico de handlers.
  - Query Bus in-memory con descubrimiento automatico de handlers.
  - Separacion completa: commands para escritura, queries para lectura.
- Event Bus hibrido:
  - **In-memory**: event bus interno para subscribers dentro del mismo contexto.
  - **RabbitMQ**: external event bus para notificar a otros contextos (ej: `UserCreated` → `CreateDefaultWorkspaceOnUserCreated`).
  - Infraestructura de reintentos y dead-letter para tolerancia a fallos en el consumo de eventos.
- Service Discovery:
  - `ServiceAttribute` / `ProviderAttribute` para auto-registro en DI.
  - Escaneo de ensamblados al startup, eliminando registro manual.
- API Layer:
  - Controladores REST que orquestan queries y commands.
  - Mapeo automatico de excepciones de dominio a respuestas HTTP estandarizadas.
  - Esquema de autenticacion custom mediante cookie.
  - Flujo de activacion de usuario por token via email.
- TDD:
  - Tests unitarios orientados al comportamiento de handlers.
  - Tests de integracion contra PostgreSQL real con contenedores.
  - Tests E2E contra la API real con `WebApplicationFactory`.
  - Patron Object Mother para simplificar creacion de entidades en tests.
- Husky + dotnet-format:
  - Pre-commit: formateo automatico de todo el codigo.
  - Pre-push: ejecucion de tests antes de permitir el push.
- Entorno:
  - Carga de variables de entorno via `DotEnvCharger` desde archivo `.env`.
  - Separacion de connection strings por contexto (`IAM_DB_CONNECTION_STRING`, `WORK_MANAGEMENT_DB_CONNECTION_STRING`).
  - Transacciones con `UnitOfWork` para garantizar consistencia entre operaciones.

## Convencion de namespaces

El namespace de cada archivo coincide con su ruta de carpeta dentro del proyecto.

**Reglas:**

- `namespace == ruta de carpeta` — ej: `src/contexts/Iam/Auth/Domain/Password.cs` → `FlowTrack.Iam.Auth.Domain`
- Nombres de carpeta en **PascalCase**.
- Si una carpeta tiene el mismo nombre que una clase que contiene (ej: `User/User.cs`), la carpeta se **pluraliza** (`Users/User.cs`) para evitar ambigüedad entre el namespace y el tipo.
- En la capa `Application/`, las carpetas de caso de uso (Signin, Signup, ActivationEmailSender, etc.) son organizativas y **no** forman parte del namespace — todos los archivos usan el namespace del directorio `Application` padre.

Ejemplos:

| Ruta                                                                               | Namespace                                            |
| ---------------------------------------------------------------------------------- | ---------------------------------------------------- |
| `src/contexts/Iam/Users/Domain/User.cs`                                            | `FlowTrack.Iam.Users.Domain`                         |
| `src/contexts/Iam/Auth/Application/ActivationEmailSender/ActivationEmailSender.cs` | `FlowTrack.Iam.Auth.Application`                     |
| `src/contexts/Shared/Domain/Bus/Event/DomainEvent.cs`                              | `FlowTrack.Shared.Domain.Bus.Event`                  |
| `src/contexts/Shared/Infrastructure/HttpErrorResponses/HttpErrorResponse.cs`       | `FlowTrack.Shared.Infrastructure.HttpErrorResponses` |

## Estructura de proyectos

- `src/apps/FlowTrackIamApi/FlowTrackIamApi.csproj`
  - API HTTP del contexto IAM: controllers, middleware, auth.
- `src/apps/FlowTrackWorkManagementApi/FlowTrackWorkManagementApi.csproj`
  - API HTTP del contexto Work Management: controllers, workspaces.
- `src/apps/FlowTrackApiGateway/FlowTrackApiGateway.csproj`
  - API Gateway con reverse proxy (YARP).
- `src/contexts/Shared/FlowTrack.Shared.csproj`
  - Codigo compartido transversal (contratos de bus, service discovery, mailer, value objects, utilidades de infraestructura, RabbitMQ, DotEnv).
- `src/contexts/Iam/FlowTrack.Iam.csproj`
  - Bounded context de identidad/autenticacion (signup, signin, activacion por email, JWT).
- `src/contexts/WorkManagement/FlowTrack.WorkManagement.csproj`
  - Bounded context de gestion de workspaces (creacion, indexacion con Elasticsearch, filtros).
- `test/apps/FlowTrackIamApi/FlowTrackIamApiTest.csproj`
  - Tests E2E contra la API real de FlowTrackIamApi.
- `test/contexts/Shared/FlowTrack.Shared.Test.csproj`
  - Utilidades base para tests (fixture compartida, carga de `.env`, object mother).
- `test/contexts/Iam/FlowTrack.Iam.Test.csproj`
  - Tests unitarios e integracion del contexto IAM.
- `test/contexts/WorkManagement/FlowTrack.WorkManagement.Test.csproj`
  - Tests unitarios e integracion del contexto WorkManagement.

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

Levantar servicios locales (infraestructura compartida + bases de datos):

```bash
docker compose -f src/apps/Shared/docker-compose.local.yml up -d
docker compose -f src/apps/FlowTrackIamApi/docker-compose.local.yml up -d
docker compose -f src/apps/FlowTrackWorkManagementApi/docker-compose.local.yml up -d
```

Desde la raiz de la solution:

```bash
dotnet restore FlowTrack.slnx
dotnet build FlowTrack.slnx
dotnet test FlowTrack.slnx
```

Ejecutar tests por proyecto:

```bash
dotnet test test/contexts/Iam/FlowTrack.Iam.Test.csproj
dotnet test test/contexts/Shared/FlowTrack.Shared.Test.csproj
dotnet test test/contexts/WorkManagement/FlowTrack.WorkManagement.Test.csproj
dotnet test test/apps/FlowTrackIamApi/FlowTrackIamApiTest.csproj
```

### Hooks de git (Husky)

El proyecto incluye hooks gestionados con Husky:

```bash
# Pre-commit: formatea automaticamente el codigo con dotnet-format
# Pre-push: ejecuta todos los tests
```

Los hooks se instalan automaticamente al ejecutar restore si Husky esta configurado.

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
3. Crea la entidad raiz (`User` con `UserId`, `Email`, `Password` como Value Objects).
4. La entidad emite un evento de dominio `UserCreated`.
5. El evento se despacha internamente (ej: envio de email de activacion).
6. El evento se publica como external event via RabbitMQ.
7. El contexto WorkManagement recibe `UserCreated` y crea un workspace por defecto.

**Activacion de usuario (escritura/command):**

1. El usuario recibe un email con un token de activacion.
2. `ActivateUserByTokenCmdHandler` valida el token.
3. Activa el usuario y emite `UserActivated`.

**Creacion de workspace (escritura/command + evento externo):**

1. `CreateDefaultWorkspaceOnUserCreated` suscribe `UserCreated` (external event).
2. Crea un workspace por defecto para el nuevo usuario.
3. El workspace se indexa automaticamente en Elasticsearch.

## Estado actual

Este repositorio esta en evolucion y enfocado en aprendizaje deliberado.

**Ultimas incorporaciones:**

- Contexto `WorkManagement` con workspaces, repositorios e indexacion en Elasticsearch.
- Event bus hibrido: in-memory + RabbitMQ con soporte de reintentos y dead-letter.
- Activacion de usuario por email con token.
- Value Objects (`UserId`, `Email`, `Password`) como tipos fuertes.
- Husky + dotnet-format para calidad de codigo automatizada.
- Carga de entorno via `DotEnvCharger`.
- Connection strings separados por bounded context.

Ideas para siguientes iteraciones:

- Agregar pipeline CI para build + test automatico.
- Introducir proyecciones / read models.
- Añadir mas bounded contexts.
- Implementar event sourcing.
- Mejorar la observabilidad (logging estructurado, tracing, metricas).
- Documentar decisiones arquitectonicas con ADRs.
