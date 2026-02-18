# Translator

WPF desktop application for translating text, OCR image content, and PDF text by using external OCR and translation services.

## What this repository contains

- `src/Translator` — main WPF desktop app (`net472`).
- `src/Translator Backend` — backend/service integration library (`net472`).
- `src/Translator Installer` — Visual Studio installer project (`.vdproj`).
- `src/Translator.sln` — solution file containing all projects.

## Features

- Text translation workflow.
- Image OCR extraction + translation.
- PDF translation workflow.
- Configurable OCR/translation endpoints (IP + port).
- Runtime server status monitoring.
- Language code mapping and selection.

## Requirements

### Development

- Windows
- Visual Studio 2019 or later (with .NET desktop development workload)
- .NET Framework 4.7.2 targeting pack

### Runtime

- Reachable OCR HTTP service
- Reachable translation HTTP service
- Valid app settings in `src/Translator/App.config`

## Build

Open `src/Translator.sln` in Visual Studio and build the solution using `Debug|Any CPU` or `Release|Any CPU`.

## Run

1. Start your OCR and translation services.
2. Launch the `Translator` project.
3. Configure endpoint host/port values from the application settings UI.
4. Use text, image, or PDF tabs to run translation workflows.

## Notes

- This repository currently does **not** include the image/gif assets that were previously referenced from `docs/...` paths.
- The projects target **.NET Framework 4.7.2** (not .NET 8).
