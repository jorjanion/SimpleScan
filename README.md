# 🖨️ SimpleScan

**English** | [Español](README.es.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform: Windows 10 / 11](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)](https://microsoft.com/windows)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/)

**SimpleScan** is a clean, lightweight, native Windows desktop application for scanning documents without bloatware, advertisements, background telemetry, or mandatory cloud logins. It works with any scanner or multifunction printer compatible with **WIA 2.0 / eSCL** (HP, Canon, Epson, Brother, and others).

---

## Features

- **Fast and Lightweight**: Instant startup with minimal memory and CPU usage.
- **Auto-Detection**: Automatically detects scanners connected via USB or local network via WIA.
- **Multi-page Management**: Scan consecutive pages, preview thumbnails, reorder pages, rotate 90°, and delete unwanted pages before saving.
- **Configurable Quality**: Adjust scan resolution (75, 150, 300, 600 DPI) and color modes (Color, Grayscale, Black & White).
- **Native Windows OCR**: Integrated optical character recognition using the hardware-accelerated Windows OCR engine (`Windows.Media.Ocr`). Completely local and private, with no cloud communication required.
- **Searchable PDF Export**: Creates PDF files where recognized text can be selected, searched (Ctrl+F), and copied in any standard PDF reader.
- **Copy Text to Clipboard**: Instantly copy OCR text from scanned pages with a single click.
- **Flexible Export Formats**: Save as a combined multi-page PDF or individual image files (PNG, JPEG).
- **Zero Bloatware**: No account requirements, no background services, no subscriptions, and no telemetry.

---

## System Requirements

- **Operating System**: Windows 10 (version 19041 or higher) or Windows 11 (64-bit).
- **Scanner**: Any WIA 2.0 or eSCL compatible scanner or multifunction printer supported by Windows.
- **Runtime**: Portable and installer releases are self-contained and require no additional runtime installation.

---

## Downloads

Download pre-built packages from the [GitHub Releases](https://github.com/jorjanion/SimpleScan/releases) section:

- **`SimpleScan-Setup.exe`**: Standard interactive installer (Inno Setup) with setup wizard, desktop and start menu shortcuts, and clean uninstaller.
- **`SimpleScan-Installer.msi`**: Windows Installer (MSI) package for system installation or enterprise deployment.
- **`SimpleScan-Portable.exe`**: Standalone portable single-file executable. Runs without installation.
- **`SimpleScan-Portable.zip`**: Compressed archive containing the portable executable.

---

## Building and Development

Building from source requires the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0):

```powershell
# Clone the repository
git clone https://github.com/jorjanion/SimpleScan.git
cd SimpleScan

# Restore dependencies
dotnet restore SimpleScan.sln

# Run automated tests
dotnet test SimpleScan.sln -c Release

# Run the UI project in development mode
dotnet run --project src/SimpleScan.UI

# Publish self-contained portable executable
dotnet publish src/SimpleScan.UI/SimpleScan.UI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o dist/portable
```

---

## License

This project is licensed under the [MIT License](LICENSE).
