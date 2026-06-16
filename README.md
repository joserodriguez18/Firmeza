# Firmeza SAS - Módulo Administrativo Base (Fase 1) 🏗️

Sistema de gestión, distribución e inventario de materiales de construcción desarrollado con prácticas de Arquitectura Limpia y DDD (Domain-Driven Design) en **.NET 10**. Incluye un Panel Administrativo robusto, control de inventarios transaccional y un sistema perimetral de autenticación de usuarios.

## 🏗️ Arquitectura del Sistema

El proyecto está diseñado bajo una **Arquitectura en Capas Decoupled**, lo que garantiza que las reglas de negocio sean independientes de la base de datos y la interfaz visual:

*   **`Firmeza.Core`**: Biblioteca de clases que contiene las entidades puras del dominio (`Producto`, `Venta`, `Cliente`, `UsuarioSistema`) e interfaces de servicios.
*   **`Firmeza.Infrastructure`**: Implementación de accesos a datos mediante **Entity Framework Core**, repositorios y control transaccional de PostgreSQL.
*   **`Firmeza.AdminWeb`**: Panel visual desarrollado en **ASP.NET Core Razor Pages**, estilizado con Bootstrap 5 local y protegido por políticas de roles.
*   **`Firmeza.Tests`**: Proyecto de pruebas unitarias automatizadas con **xUnit** utilizando almacenamiento aislado en memoria.

---

## 🛠️ Requisitos Previos

Antes de ejecutar la aplicación, asegúrate de tener instalado en tu máquina:
1. [SDK de .NET 10.0](https://microsoft.com) o superior.
2. [Docker Desktop](https://docker.com) (Para despliegue rápido de Base de Datos).
3. Herramienta de línea de comandos de Entity Framework Core:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

---

## 🚀 Guía de Instalación y Ejecución Local

Sigue estos pasos para clonar, configurar y poner en marcha el proyecto desde cero:

### 1. Clonar el repositorio
```bash
git clone https://github.com
cd Firmeza
```

### 2. Levantar PostgreSQL con Docker 🐋
El proyecto incluye una receta para inicializar el motor de base de datos de manera automatizada. En la raíz de la solución, ejecuta:
```bash
docker compose up -d
```
*Esto encenderá un contenedor de PostgreSQL en el puerto `5432` con las credenciales por defecto configuradas en el sistema.*

### 3. Aplicar las Migraciones a la Base de Datos
Construye el andamiaje físico de las tablas ejecutando:
```bash
dotnet ef database update --project src/Firmeza.Infrastructure --startup-project src/Firmeza.AdminWeb
```

### 4. Ejecutar la Aplicación Web 💻
Inicia el servidor en modo de escucha dinámica:
```bash
dotnet run --project src/Firmeza.AdminWeb
```
Abre tu navegador e ingresa a la dirección local provista por la consola (ej. `http://localhost:5000` o `https://localhost:5001`).

---

## 🔑 Credenciales de Acceso Automáticas (Seed Data)

El sistema cuenta con un inicializador de datos automatizado (`SeedData`) que se ejecuta al arrancar el servidor. Puedes ingresar al panel con el siguiente usuario administrador de pruebas:

*   **Usuario (Email):** `admin@firmeza.com`
*   **Contraseña:** `Admin123*`

> ⚠️ **Nota de Seguridad (Task 4):** Si registras un usuario desde el módulo de Clientes, sus credenciales pertenecerán al rol `Cliente`. Si intentas iniciar sesión con una cuenta de cliente en este panel Razor, el sistema bloqueará la petición y te redirigirá a la pantalla de **Acceso Denegado**.

---

## 🧪 Ejecución de Pruebas Unitarias

Para validar que las reglas de negocio, el cálculo del IVA (19%) y la reducción transaccional de inventarios funcionen con total precisión matemática en el backend, ejecuta:

```bash
dotnet test
```

---

## 🛠️ Tecnologías Utilizadas

*   **Backend**: .NET 10 Core C#
*   **Base de Datos**: PostgreSQL + Entity Framework Core (Npgsql)
*   **Seguridad**: ASP.NET Core Identity (Autenticación basada en Cookies)
*   **Testing**: xUnit + EF Core InMemory Database Provider
*   **Frontend**: Razor Pages + Bootstrap 5 (Archivos locales en `wwwroot`)
*   **Contenedores**: Docker & Docker Compose
