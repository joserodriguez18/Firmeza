# Firmeza

Sistema administrativo para gestión de productos, clientes y ventas construido con ASP.NET Core Razor Pages, Entity Framework Core y PostgreSQL.

## Alcance de esta entrega

- Carga masiva desde Excel desnormalizado.
- Exportación de productos a Excel y clientes a PDF.
- Generación automática de recibo PDF al registrar una venta.
- Interfaz administrativa con navegación lateral y estilo unificado.
- Base inicial de documentación técnica y despliegue con Docker.

## Estructura

- `src/Firmeza.Core`: entidades e interfaces del dominio.
- `src/Firmeza.Infrastructure`: acceso a datos y servicios de negocio.
- `src/Firmeza.AdminWeb`: interfaz web Razor Pages.
- `tests/Firmeza.Tests`: pruebas unitarias con xUnit.

## Ejecución local

Requisitos:

- .NET 10 SDK
- Docker Desktop

Arranque de base de datos:

```bash
docker compose up -d
```

Migraciones:

```bash
dotnet ef database update --project src/Firmeza.Infrastructure --startup-project src/Firmeza.AdminWeb
```

Ejecución:

```bash
dotnet run --project src/Firmeza.AdminWeb
```

## Documentos y exportaciones

La sección administrativa `Documentos` permite:

- importar archivos `.xlsx` con datos mezclados;
- exportar productos en Excel;
- exportar clientes en PDF;
- descargar recibos desde `wwwroot/recibos`.

## Pruebas

```bash
dotnet test
```

## Despliegue

La raíz del repositorio incluye `Dockerfile` para la app y `docker-compose.yml` para levantar la API y PostgreSQL.

## Notas técnicas

- Los recibos PDF se generan con QuestPDF.
- La importación usa EPPlus.
- La solución ya trabaja con Identity para usuarios y roles.
- Diagramas: [ER](docs/erd.md) y [clases](docs/class-diagram.md).
