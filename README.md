# 🖨️ SimpleScan

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform: Windows 10 / 11](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)](https://microsoft.com/windows)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/)

**SimpleScan** es una aplicación de escritorio nativa para Windows, ultrarrápida, limpia y ligera, diseñada para escanear documentos sin la lentitud, la publicidad, el bloatware ni las cuentas en la nube que imponen aplicaciones como *HP Smart*.

Nacida originalmente para hacer volar el escáner de una **HP Smart Tank Plus 550** en ordenadores modestos (como portátiles con Intel Core i3), **SimpleScan** funciona con cualquier escáner o impresora multifunción del mercado compatible con **WIA 2.0 / eSCL** (HP, Canon, Epson, Brother, etc.).

---

## ✨ Características Principales

- ⚡ **Rendimiento extremo**: Arranca en menos de 400 ms y consume menos de 45 MB de RAM.
- 🔍 **Detección automática**: Detecta de inmediato tu escáner (por Wi-Fi o USB) sin configurar nada.
- 📑 **Escaneo multipágina ágil**: Añade páginas consecutivas, previsualízalas, reordénalas, gíralas 90° si han salido torcidas o elimina las defectuosas antes de guardar.
- ⚙️ **Control de calidad**: Selecciona fácilmente la resolución (75, 150, 300, 600 DPI) y modo de color (Color, Escala de grises, Blanco y negro).
- 🔤 **OCR nativo en español (y otros idiomas)**: Utiliza el motor nativo acelerado por hardware de Windows (`Windows.Media.Ocr`). Sin descargas de 100 MB ni servicios en la nube: 100% privado y offline.
- 📄 **PDF Buscable (Searchable PDF)**: Genera PDFs donde el texto escaneado se puede seleccionar, copiar y buscar (Ctrl+F) en cualquier visor de PDF.
- 📋 **Extracción directa de texto**: Botón para copiar todo el texto reconocido al portapapeles con un solo clic.
- 🖼️ **Exportación flexible**: Guarda como un único PDF combinado o como imágenes individuales (PNG/JPG).
- 🛡️ **0% Bloatware**: Sin inicios de sesión obligatorios, sin suscripciones, sin telemetría intrusiva y sin procesos en segundo plano.

---

## 🚀 Requisitos del Sistema

- **Sistema Operativo**: Windows 10 (versión 19041 o superior) o Windows 11.
- **Hardware**: Cualquier procesador x64 (optimizado incluso para Intel Celeron / Core i3 y 4 GB de RAM).
- **Impresora / Escáner**: Cualquier dispositivo conectado por USB o red Wi-Fi reconocido por Windows (drivers WIA nativos o eSCL/Mopria).

---

## 📦 Compilación y Desarrollo

Para compilar el proyecto localmente:

```powershell
# Clonar el repositorio
git clone https://github.com/jorjanion/SimpleScan.git
cd SimpleScan

# Restaurar dependencias y compilar
dotnet build

# Ejecutar la aplicación
dotnet run --project src/SimpleScan.UI

# Publicar como ejecutable único autocontenido (sin dependencias)
dotnet publish src/SimpleScan.UI -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./dist
```

---

## 📄 Licencia

Este proyecto se distribuye bajo la licencia **MIT**. Consulta el archivo [LICENSE](LICENSE) para más detalles.
