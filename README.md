# ApiBase - Backend Estandarizado (.NET)

ApiBase es una plantilla base para proyectos backend construida con **ASP.NET Core (.NET)** siguiendo los principios de **Clean-ish Architecture** (Arquitectura en Capas). Está diseñada para ser escalable, segura y mantener un estándar estricto en las respuestas HTTP y el manejo de errores.

## 🚀 Tecnologías Principales

- **Framework:** ASP.NET Core (.NET)
- **Base de Datos:** PostgreSQL
- **Micro ORM:** Dapper (Consultas SQL nativas de alto rendimiento)
- **Autenticación:** JWT (JSON Web Tokens) con soporte para Refresh Tokens
- **Validaciones:** FluentValidation
- **Logging:** Serilog (Consola y Archivos con rotación diaria)
- **Documentación de API:** Swagger / OpenAPI

---

## 📂 Arquitectura del Proyecto

El proyecto está dividido en las siguientes capas lógicas:

- **Domain:** Contiene las entidades principales del negocio (`User`, `Role`, `Permission`). No tiene dependencias de otras capas.
- **Application:** Contiene la lógica de negocio, interfaces de servicios y repositorios, los `DTOs` (Data Transfer Objects) y las reglas de validación (FluentValidation).
- **Infrastructure:** Implementación concreta de acceso a datos (Repositorios con Dapper), conexión a la base de datos PostgreSQL, y los Middlewares personalizados (como `ErrorHandlingMiddleware`).
- **Common:** Clases transversales a todo el proyecto como el wrapper `ApiResponse<T>`, clases de Paginación y las Excepciones de negocio (`ConflictException`, `NotFoundException`, etc.).
- **API (Controllers):** Punto de entrada de la aplicación. Define las rutas HTTP y delega la ejecución a los servicios de la capa *Application*.

---

## 🛠️ Configuración Inicial y Ejecución

### 1. Requisitos Previos
- [.NET SDK](https://dotnet.microsoft.com/download) (Compatible con la versión del proyecto)
- [PostgreSQL](https://www.postgresql.org/download/) instalado y en ejecución.

### 2. Configurar la Base de Datos
1. Abre el archivo `appsettings.Development.json` (o `appsettings.json`).
2. Localiza la sección `ConnectionStrings` y modifica la cadena `DefaultConnection` con tus credenciales de PostgreSQL:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=ApiBaseDb;Username=tu_usuario;Password=tu_password"
   }
   ```

### 3. Configurar JWT
En el mismo archivo `appsettings.json`, asegúrate de tener configurada la sección de JWT con una clave secreta fuerte:
```json
"Jwt": {
  "Key": "tu_super_clave_secreta_muy_larga_y_segura_aqui_12345!",
  "Issuer": "ApiBaseIssuer",
  "Audience": "ApiBaseAudience",
  "DurationInMinutes": 60,
  "RefreshTokenDurationInDays": 7
}
```

### 4. Compilar y Ejecutar
Abre una terminal en la raíz del proyecto (`ApiBase/`) y ejecuta los siguientes comandos:

```bash
# Restaurar los paquetes NuGet
dotnet restore

# Compilar el proyecto
dotnet build

# Ejecutar el proyecto
dotnet run
```
Una vez en ejecución, puedes acceder a la interfaz de **Swagger** navegando en tu explorador a:
👉 `https://localhost:<puerto>/swagger` o `http://localhost:<puerto>/swagger`

---

## 🔌 Endpoints Principales (Autenticación)

### Registro de Usuario
- **POST** `/api/auth/register`
- **Body (JSON):**
  ```json
  {
    "username": "johndoe",
    "email": "john@example.com",
    "password": "Password123!"
  }
  ```

### Inicio de Sesión
- **POST** `/api/auth/login`
- **Body (JSON):**
  ```json
  {
    "email": "john@example.com",
    "password": "Password123!"
  }
  ```

### Refrescar Token
- **POST** `/api/auth/refresh-token`
- **Body (JSON):**
  ```json
  {
    "token": "tu_access_token_actual",
    "refreshToken": "tu_refresh_token_actual"
  }
  ```

### Cerrar Sesión (Revocar Token)
- **POST** `/api/auth/logout`
- **Body (JSON):**
  ```json
  {
    "token": "tu_access_token",
    "refreshToken": "tu_refresh_token_a_revocar"
  }
  ```

---

## 📦 Estructura de Respuesta Estandarizada

Todos los endpoints (incluyendo los errores) siempre devuelven una estructura de respuesta JSON idéntica mediante el wrapper `ApiResponse<T>`.

**Ejemplo de respuesta exitosa (200 OK / 201 Created):**
```json
{
  "success": true,
  "message": "Operación realizada con éxito.",
  "data": {
    "id": 1,
    "username": "johndoe"
  },
  "errors": null,
  "pagination": null,
  "timestamp": "2024-01-15T12:00:00Z"
}
```

**Ejemplo de respuesta con errores de validación (400 Bad Request o 409 Conflict):**
```json
{
  "success": false,
  "message": "Error de validación",
  "data": null,
  "errors": {
    "email": ["El email ya está registrado."],
    "password": ["La contraseña debe contener al menos una mayúscula."]
  },
  "pagination": null,
  "timestamp": "2024-01-15T12:00:00Z"
}
```

---

## 🛡️ Manejo Global de Errores

El proyecto utiliza un **Middleware Global de Errores** (`ErrorHandlingMiddleware.cs`) que intercepta cualquier excepción (errores de negocio controlados, errores nativos de PostgreSQL como llaves foráneas o duplicidad, y excepciones no controladas de código 500).

Para lanzar un error controlado desde la capa de servicios, simplemente debes usar las excepciones personalizadas de `Common.Exceptions`:

```csharp
// Ejemplo de lanzar un error 404
throw new NotFoundException("El usuario no existe.");

// Ejemplo de lanzar un error 409 (Conflicto) especificando el campo
throw new ConflictException("email", "El correo ya está en uso.");
```
Estas excepciones son procesadas automáticamente, evitando que la aplicación se caiga y formateando el JSON limpio para el Frontend, mientras que **Serilog** registra la petición de forma elegante en la consola y en los archivos log.
