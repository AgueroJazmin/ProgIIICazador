# Cazador de Ofertas

Aplicación web ASP.NET Core MVC para centralizar ofertas publicadas por McDonald's, Burger King y Mostaza.

## Tecnologías

- C# y ASP.NET Core MVC sobre .NET 10.
- Entity Framework Core 10.
- SQL Server (LocalDB en desarrollo).
- Bootstrap 5.

## Ejecutar en desarrollo

Requisitos: SDK de .NET 10 y SQL Server LocalDB instalado.

Desde la raíz del repositorio:

```bash
dotnet restore ProgIIICazador.Web/ProgIIICazador.Web.csproj
dotnet run --project ProgIIICazador.Web/ProgIIICazador.Web.csproj
```

La aplicación crea la base de datos de desarrollo si todavía no existe. La cadena de conexión se configura en `ProgIIICazador.Web/appsettings.json`.

## Alcance funcional

- Consulta de ofertas publicadas y filtros por empresa o texto.
- Ficha de oferta con beneficio, condiciones, vigencia y enlace a la publicación original.
- Registro de reportes asociados a una oferta.

## Estado del desarrollo

Esta es la primera base ejecutable del sistema: proyecto MVC, persistencia inicial, catálogo de empresas y flujo público de consulta y reportes.

Todavía falta implementar el proceso automático de extracción por empresa, OCR de imágenes y documentos, clasificación y normalización asistidas por IA, actualización periódica y el panel autenticado de administración. Las ofertas no se inventan ni se cargan como reales: solo se publicarán cuando exista información verificable y una fuente original.

## Arquitectura prevista

Monolito modular con los módulos CapturaOfertas, GestionOfertas, ConsultaOfertas y ReportesOfertas. Los vertical slices organizan las nueve historias de usuario en incorporación/procesamiento/actualización, consulta/acceso a fuente original y reportes de problemas.
