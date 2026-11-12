# Arquitectura actual

## Enfoque

El objetivo es organizar Cazador de Ofertas como un **monolito modular**: una sola aplicación desplegable, dividida en módulos con responsabilidades claras. No es una arquitectura de microservicios y no requiere desplegar procesos separados.

La reorganización es progresiva. La existencia de carpetas por módulo no significa, por sí sola, que todas las dependencias estén encapsuladas.

## Módulos y responsabilidades

| Módulo | Responsabilidad actual | Componentes principales |
|---|---|---|
| `CapturaOfertas` | Consultar fuentes oficiales, extraer contenido, programar capturas y recibir archivos para OCR | `Controllers`, `Extractors`, `Services`, `CapturaOfertasModule` |
| `GestionOfertas` | Procesar el contenido capturado mediante OCR y clasificación por reglas o IA | `Services`, `ProcesamientoOfertasModule` |
| `ConsultaOfertas` | Mostrar el catálogo público, aplicar filtros y mostrar el detalle de una oferta | `Controllers/OfertasController` y vistas asociadas |
| `ReportesOfertas` | Recibir reportes de usuarios y permitir su consulta desde administración | `Controllers/ReportesController` y vistas asociadas |
| `Administracion` | Autenticación administrativa, revisión/publicación de capturas y resolución de reportes | `Controllers/AdminController`, `ViewModels` |

## Elementos compartidos actuales

- **Modelos:** `ProgIIICazador.Web/Models` contiene `Empresa`, `Oferta`, `Beneficio`, `CapturaFuente` y `Reporte`. Se mantienen juntos por ahora porque sus relaciones atraviesan responsabilidades: una oferta pertenece a una empresa, contiene beneficios y puede tener reportes; una captura pertenece a una empresa y puede vincularse con una oferta.
- **Persistencia:** `ProgIIICazador.Web/Data/OfertasDbContext.cs` centraliza los DbSet, índices, relaciones y datos iniciales. Las migraciones y el snapshot de EF Core están en `ProgIIICazador.Web/Migrations`.
- **Vistas MVC:** permanecen en `ProgIIICazador.Web/Views`. La ubicación física de los controladores bajo `Modules` no cambia sus nombres ni las convenciones de descubrimiento de vistas.
- **Composición de la aplicación:** `Program.cs` configura persistencia, autenticación, autorización, limitación de intentos y registra los módulos.

## Registro de dependencias

Cada módulo que tiene servicios propios expone un método de extensión para registrar sus componentes:

- `AddCapturaOfertasModule`: extractor HTTP, servicio de captura y tarea programada opcional.
- `AddProcesamientoOfertasModule`: OCR y clasificación, con alternativas locales cuando no hay credenciales externas configuradas.

La aplicación sigue usando un contenedor de inyección de dependencias común. Los controladores de administración todavía consumen directamente el DbContext y coordinan más de un caso de uso; esa responsabilidad debe extraerse gradualmente a servicios/casos de uso antes de afirmar que Administración está completamente desacoplado.

## Compilación de los módulos

Los archivos C# de la carpeta raíz `Modules/` se incorporan al proyecto web mediante esta regla de `ProgIIICazador.Web.csproj`:

```xml
<Compile Include="../Modules/**/*.cs" Link="Modules/%(RecursiveDir)%(Filename)%(Extension)" />
```

Al agregar un archivo C# nuevo bajo `Modules/`, debe verificarse que quede incluido en la compilación y que sus dependencias estén registradas.

## Próximos pasos de la reorganización

1. Extraer de `AdminController` los casos de uso de revisión/publicación de capturas y resolución de reportes, conservando autenticación y rutas MVC.
2. Definir contratos entre módulos para reducir el acceso directo a persistencia compartida.
3. Decidir la ubicación definitiva de los modelos después de definir sus propietarios y relaciones; no mover entidades solo para que coincidan con el nombre de una carpeta.
4. Agregar pruebas de integración para los flujos de captura, publicación, consulta y reportes.
5. Validar las migraciones contra una instancia real de SQL Server/LocalDB antes de considerar comprobada la persistencia en ese entorno.

## Criterio de seguridad para los cambios

Los movimientos de entidades deben conservar nombres de tablas, claves, relaciones, índices, datos semilla y el modelo del snapshot de EF Core. Las migraciones existentes no deben reescribirse para reflejar simples cambios de namespace o ubicación de archivos. Cualquier modificación del modelo persistido requiere una migración nueva y pruebas sobre una base descartable.
