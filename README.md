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

- .NET `10.0`
- C#
- xUnit + Moq
- Entity Framework Core + PostgreSQL (Npgsql)
- Testcontainers para pruebas de integracion
- JWT + BCrypt

## Arquitectura

La solution esta organizada por bounded context y por capas internas.

```text
src/
  contexts/
    Shared/
      Domain/
      Infrastructure/
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
  contexts/
    Shared/
    Iam/
```

### Principios aplicados

- DDD:
  - El dominio vive en `Domain`.
  - Contratos como `IUserRepository` y objetos de dominio (`User`, `SigninSuccess`, `SigninFailed`) quedan fuera de infraestructura.
- CQRS:
  - Contratos base en `FlowTrack.Shared.Domain.Bus`:
    - `IQuery<T>`
    - `IQueryHandler<Q, R>`
    - `ICommand`
  - Caso de uso de ejemplo: `SigninQry` + `SigninQryHandler`.
- TDD:
  - Tests unitarios orientados al comportamiento del handler (`SigninQryHandlerTests`).
  - Tests de integracion contra PostgreSQL real con contenedor (`SigninQryHandlerIT`).

## Estructura de proyectos

- `src/contexts/Shared/FlowTrack.Shared.csproj`
  - Codigo compartido transversal (interfaces y utilidades de infraestructura comunes).
- `src/contexts/Iam/FlowTrack.Iam.csproj`
  - Bounded context de identidad/autenticacion.
- `test/contexts/Shared/FlowTrack.Shared.Test.csproj`
  - Utilidades base para tests (fixture compartida, carga de `.env`, object mother).
- `test/contexts/Iam/FlowTrack.Iam.Test.csproj`
  - Tests unitarios e integracion del contexto IAM.

## Variables de entorno

Se usan variables para la generacion de tokens JWT.

1. Copia el template:

```bash
cp .env.template .env
```

2. Completa al menos:

```env
ACCESS_TOKEN_SECRET=tu_access_secret
REFRESH_TOKEN_SECRET=tu_refresh_secret
```

Notas:

- `ACCESS_TOKEN_EXPIRE_MINUTES` y `REFRESH_TOKEN_EXPIRE_MINUTES` son opcionales.
- Si no se definen, se aplican valores por defecto en codigo.

## Requisitos

- SDK de .NET 10 instalado.
- Docker activo para ejecutar integration tests con Testcontainers.

## Comandos utiles

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

## Ejemplo de flujo (Signin)

1. `SigninQryHandler` recibe email/password.
2. Busca usuario en `IUserRepository`.
3. Valida password con `IBcrypt`.
4. Genera access y refresh token con `AuthTokenGenerator`.
5. Devuelve `SigninSuccess`.

Este flujo muestra como se mantiene separada la logica de dominio de los detalles de infraestructura.

## Estado actual

Este repositorio esta en evolucion y enfocado en aprendizaje deliberado.

Ideas para siguientes iteraciones:

- Agregar command bus y casos de escritura completos.
- Introducir eventos de dominio.
- Definir una API de entrada (HTTP) para exponer casos de uso.
- Agregar pipeline CI para build + test automatico.
