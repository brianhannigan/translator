# Translator Main Window

## Overview
The **Translator Main Window** is the primary WPF page that orchestrates translation workflows across text, image/OCR, and PDF modules. It exposes global settings, runtime status indicators, language selection, and a multi-tab workspace for translation tools, while also providing a consolidated error panel for diagnostics. The layout and behaviors are defined in `MainWindow.xaml`, with supporting logic in `MainWindow.xaml.cs`.【F:src/Translator/MainWindow.xaml†L1-L260】【F:src/Translator/MainWindow.xaml.cs†L1-L200】

## Layout at a Glance
The page uses a three-row grid to organize top-level UI elements:

1. **Menu + Status Row**
   - **Settings menu** for server configuration, translation settings editor access, and server start/stop control. It also includes inline fields for translation and OCR server IP/port values. These bindings are powered by `NetworkManager`.【F:src/Translator/MainWindow.xaml†L27-L118】【F:src/Translator/MainWindow.xaml.cs†L70-L123】
   - **Palette menu** for editing, importing, exporting, and resetting the UI palette via `PaletteManager` commands.【F:src/Translator/MainWindow.xaml†L119-L146】【F:src/Translator/MainWindow.xaml.cs†L54-L89】
   - **Status indicators** for translation and OCR server availability, using `ServerManager` status fields to drive the indicator control states.【F:src/Translator/MainWindow.xaml†L147-L173】【F:src/Translator/MainWindow.xaml.cs†L110-L133】
   - **Error toggle** button that appears when errors are available; it uses the `ErrorDisplayCommand` to show or hide the error panel.【F:src/Translator/MainWindow.xaml†L174-L202】【F:src/Translator/MainWindow.xaml.cs†L23-L68】

2. **Language Selection Row**
   - **Source Language** and **Target Language** selectors are bound to `LanguageManager` and update the translation context for all tabs.【F:src/Translator/MainWindow.xaml†L203-L221】【F:src/Translator/MainWindow.xaml.cs†L62-L83】

3. **Workspace Row**
   - A **tabbed workspace** (`TabControlExt`) hosts translation tools. It supports drag/drop and displays tab checkboxes, enabling modular add-on experiences from `TextTranslatorControl`, `ImageTranslatorControl`, and `PdfTranslatorControl` managed in the code-behind.【F:src/Translator/MainWindow.xaml†L222-L228】【F:src/Translator/MainWindow.xaml.cs†L132-L176】

## Error Handling Panel
The right-side **error panel** is collapsed by default and can be toggled using the error icon in the header row. It includes:
- A read-only error log area for aggregated messages.
- A **Clear** button wired to `ErrorManager.ClearCommand` to reset error state.【F:src/Translator/MainWindow.xaml†L230-L259】【F:src/Translator/MainWindow.xaml.cs†L96-L123】

## Key Responsibilities (Code-Behind)
The `MainWindow` class wires together the core services that the UI binds to:
- **PaletteManager** for visual theming and palette commands.
- **LanguageManager** for language selection and available languages.
- **NetworkManager** for server endpoints and connectivity.
- **ErrorManager** and **ServerManager** for status, logging, and error visibility.
- **Tab controls** for text, image, and PDF translation modules, created during initialization and hosted in the tab control area.【F:src/Translator/MainWindow.xaml.cs†L54-L176】

## Related Files
- **Layout & bindings:** `MainWindow.xaml`【F:src/Translator/MainWindow.xaml†L1-L260】
- **Interaction logic:** `MainWindow.xaml.cs`【F:src/Translator/MainWindow.xaml.cs†L1-L200】

