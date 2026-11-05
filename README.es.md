<p align="center">
  <img src="assets/icon.png" width="100" height="100" alt="Icono de SimpleScan" />
</p>

<h1 align="center">SimpleScan</h1>

<p align="center">
  <strong>Escaneo de documentos rápido, nativo y sin bloatware para Windows</strong>
</p>

<p align="center">
  <a href="README.md">English</a> | <b>Español</b>
</p>

<p align="center">
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="License: MIT" /></a>
  <a href="https://microsoft.com/windows"><img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg" alt="Platform: Windows 10 / 11" /></a>
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-9.0-512BD4.svg" alt=".NET 9.0" /></a>
</p>

---

**SimpleScan** es una aplicación de escritorio nativa para Windows, rápida y ligera, diseñada para escanear documentos sin publicidad, bloatware ni cuentas en la nube. Funciona con cualquier escáner o impresora multifunción compatible con el estándar **WIA 2.0 / eSCL** (HP, Canon, Epson, Brother, etc.).

---

## Características

- **Rápida y ligera**: Inicio inmediato y bajo consumo de recursos.
- **Detección automática**: Detecta dispositivos conectados por USB o red local mediante WIA.
- **Gestión multipágina**: Escaneo continuo de páginas, vista previa, reordenación, rotación en pasos de 90° y eliminación de páginas individuales.
- **Ajustes de escaneo**: Selección de resolución (75, 150, 300, 600 DPI) y modos de color (Color, Escala de grises, Blanco y negro).
- **OCR nativo**: Reconocimiento de texto integrado mediante la API de Windows (`Windows.Media.Ocr`), local, sin conexión a internet y respetuoso con la privacidad.
- **PDF con búsqueda (Searchable PDF)**: Genera documentos PDF donde el texto reconocido se puede buscar y seleccionar directamente.
- **Copia de texto al portapapeles**: Permite copiar el texto reconocido de las páginas escaneadas con un solo clic.
- **Exportación flexible**: Guarda como PDF de varias páginas o como imágenes independientes (PNG, JPG).
- **Sin bloatware**: Sin cuentas de usuario, sin servicios en segundo plano y sin telemetría intrusiva.

---

## Requisitos del Sistema

- **Sistema operativo**: Windows 10 (versión 19041 o superior) o Windows 11 (64 bits).
- **Dispositivo**: Escáner o impresora multifunción compatible con WIA o eSCL reconocido por Windows.
- **Tiempo de ejecución**: La versión portable y los instaladores son autocontenidos y no requieren instalar paquetes adicionales.

---

## Descargas

En la sección de [Releases de GitHub](https://github.com/jorjanion/SimpleScan/releases) se ofrecen las siguientes opciones:

- **`SimpleScan-Setup.exe`**: Instalador interactivo estándar (Inno Setup) con asistente guiado, accesos directos en el Escritorio y Menú Inicio y desinstalación limpia.
- **`SimpleScan-Installer.msi`**: Paquete de Windows Installer (MSI) para instalación en el sistema o despliegue automatizado.
- **`SimpleScan-Portable.exe`**: Ejecutable único y portable. No requiere instalación.
- **`SimpleScan-Portable.zip`**: Archivo comprimido con la versión portable.

---

## Compilación y Desarrollo

Para compilar el proyecto desde el código fuente se requiere el SDK de .NET 9:

```powershell
# Clonar el repositorio
git clone https://github.com/jorjanion/SimpleScan.git
cd SimpleScan

# Restaurar dependencias
dotnet restore SimpleScan.sln

# Ejecutar las pruebas unitarias
dotnet test SimpleScan.sln -c Release

# Ejecutar la aplicación en modo desarrollo
dotnet run --project src/SimpleScan.UI

# Publicar el ejecutable portable autocontenido
dotnet publish src/SimpleScan.UI/SimpleScan.UI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o dist/portable
```

---

## Licencia

Distribuido bajo la licencia [MIT](LICENSE).
