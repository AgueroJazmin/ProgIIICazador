# Cazador de Ofertas

Aplicación web ASP.NET Core MVC para centralizar promociones publicadas por McDonald's, Burger King y Mostaza en Argentina. El sistema no compara marcas, productos ni precios y no gestiona delivery.

## Tecnologías

- C# / ASP.NET Core MVC sobre .NET 10.
- Entity Framework Core 10 y SQL Server.
- Bootstrap 5.
- Captura web mediante HttpClient y lectura de datos estructurados Schema.org.
- OCR opcional mediante Azure AI Document Intelligence.
- Clasificación opcional mediante una API compatible con OpenAI Chat Completions.
- GitHub Actions para restauración, compilación y pruebas unitarias.

## Ejecutar en desarrollo

Requisitos: Visual Studio 2026 con la carga de trabajo **Desarrollo de ASP.NET y web**, SDK de .NET 10 y SQL Server LocalDB.

### Desde Visual Studio 2026

1. Cloná o descargá este repositorio.
2. Abrí el archivo **ProgIIICazador.sln**.
3. Si Visual Studio no selecciona el proyecto automáticamente, hacé clic derecho sobre **ProgIIICazador.Web** y elegí **Establecer como proyecto de inicio**.
4. Verificá que el perfil **ProgIIICazador.Web** esté seleccionado y ejecutá con **F5**.
5. La aplicación abre la página de inicio en el navegador. El perfil local desactiva la captura automática al iniciar para que puedas probar primero la interfaz sin depender de las páginas externas.

La base de datos local se crea en el primer inicio si no existe. Para que funcione la conexión predeterminada, necesitás tener SQL Server LocalDB instalado.

### Desde una terminal

```bash
dotnet restore ProgIIICazador.Web/ProgIIICazador.Web.csproj
dotnet run --project ProgIIICazador.Web/ProgIIICazador.Web.csproj
```

Para iniciar desde Visual Studio o desde la terminal, el proyecto web utiliza el perfil local configurado en `ProgIIICazador.Web/Properties/launchSettings.json`.

Para ejecutar las pruebas unitarias:

```bash
dotnet test ProgIIICazador.Tests/ProgIIICazador.Tests.csproj
```

En el primer arranque se crea la base de datos si no existe mediante EnsureCreated. Esta inicialización sirve para una instalación nueva de prueba; no reemplaza migraciones de EF Core para actualizar una base existente ni debe usarse como estrategia de despliegue productivo. Si ya existe una base creada con una versión anterior del modelo, respaldá los datos y recreá la base local antes de probar esta versión.

La cadena de conexión está en `ConnectionStrings:OfertasDb`. Para otro SQL Server, reemplazá el valor mediante configuración o variables de entorno.

## Configuración de administración

No existe una contraseña predeterminada. Para poder iniciar sesión al ejecutar con F5, configurá credenciales locales con User Secrets desde una terminal ubicada en la carpeta del repositorio:

```powershell
dotnet user-secrets set "Admin:Usuario" "admin" --project ProgIIICazador.Web
dotnet user-secrets set "Admin:Contrasena" "TU_CLAVE_SEGURA" --project ProgIIICazador.Web
```

User Secrets guarda estos valores fuera del repositorio y ASP.NET Core los carga automáticamente en el entorno Development. No subas contraseñas ni claves al repositorio.

El panel se abre desde Administración. Las rutas administrativas usan autenticación por cookie, HTTPS, autorización por rol y protección antifalsificación en los formularios.

## Captura automática

La captura periódica queda habilitada por defecto, con una consulta inicial al iniciar la aplicación y un intervalo de seis horas. Para desactivarla, definí `Captura__Habilitada=false`. Para cambiar el intervalo:

```powershell
$env:Captura__IntervaloMinutos = "360"
```

El intervalo permitido se limita a entre 30 minutos y 24 horas. Se consultan las URL oficiales configuradas en cada empresa. El extractor lee metadatos y bloques JSON-LD Schema.org; si no encuentra ofertas estructuradas, guarda texto de la página como candidato. No ejecuta JavaScript, no evade bloqueos y no garantiza detectar contenido que solo exista dentro de una app o detrás de autenticación.

Cada candidato se guarda como pendiente de revisión, con huella para evitar duplicados. El administrador debe verificar la publicación original y completar beneficio, condiciones y vigencia antes de publicarla. Las ofertas vencidas se excluyen del catálogo y pasan a estado vencido durante la siguiente ejecución de captura.

## OCR de imágenes y documentos

Para habilitar OCR con Azure AI Document Intelligence, configurá:

- `Ocr__Endpoint`
- `Ocr__Key`
- Opcional: `Ocr__ModelId` (por defecto `prebuilt-read`).

El panel permite subir PDF, PNG o JPG de hasta 10 MB y asociarlo a una URL oficial. El texto extraído queda pendiente de revisión; el OCR no valida por sí mismo la vigencia ni la veracidad de una promoción.

## Clasificación con IA

La integración es opcional y usa un endpoint compatible con OpenAI Chat Completions:

- `IA__Endpoint`: URL base de la API compatible.
- `IA__ApiKey`: clave de acceso.
- `IA__Modelo`: identificador del modelo.

Sin estas variables se usa un clasificador local conservador basado en reglas. La clasificación es orientativa y siempre requiere revisión humana. No se deben cargar claves en el repositorio.

## Funcionalidades implementadas

- Catálogo público de ofertas publicadas, filtros por empresa y búsqueda.
- Ficha de oferta con beneficio, condiciones, vigencia y enlace a la fuente original.
- Modelo de beneficios estructurados.
- Captura automática de páginas oficiales, lectura de JSON-LD/metadatos y deduplicación por huella.
- Ejecución periódica configurable y detección de ofertas vencidas.
- OCR opcional de documentos e imágenes.
- Clasificación preliminar por reglas o IA configurable.
- Panel administrativo con login, revisión, publicación o descarte de capturas.
- Recepción y resolución de reportes de usuarios.
- Validaciones de formularios y enlaces HTTPS para fuentes.

## Alcance y limitaciones conocidas

La extracción genérica no reemplaza conectores específicos por marca. Las páginas oficiales pueden cambiar su estructura, cargar promociones dinámicamente o reservar información para sus aplicaciones. Para lograr cobertura completa y estable habrá que verificar y mantener los endpoints o selectores específicos de cada marca, respetando sus términos y límites de acceso. No se inventan ofertas ni se publica información sin aprobación.

La app está en desarrollo. Antes de producción faltan migraciones versionadas de EF Core, pruebas automatizadas de integración y seguridad, configuración real de credenciales/servicios externos, revisión legal de la captura automatizada y pruebas de despliegue. El flujo OCR/IA necesita sus credenciales para operar con proveedores externos.

## Estructura

Monolito modular organizado en CapturaOfertas, GestionOfertas, ConsultaOfertas y ReportesOfertas, con vertical slices para incorporación/procesamiento/actualización, consulta y reportes.
