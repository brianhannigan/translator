# 🚀 LinguoAI  
### AI-Powered OCR & Translation Intelligence Platform  

<p align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge&logo=dotnet)  
![WPF](https://img.shields.io/badge/UI-WPF-blue?style=for-the-badge)  
![Architecture](https://img.shields.io/badge/Architecture-MVVM-darkgreen?style=for-the-badge)  
![Status](https://img.shields.io/badge/Status-Production_Ready-brightgreen?style=for-the-badge)  
![Platform](https://img.shields.io/badge/Platform-Windows-black?style=for-the-badge)  
![AI](https://img.shields.io/badge/AI-Document_Intelligence-orange?style=for-the-badge)

</p>

---

## 🔥 Overview

**LinguoAI** is a high-performance Windows desktop application built using **WPF + MVVM architecture** that transforms images and documents into structured, translated intelligence.

It integrates:

- 🖼 High-accuracy OCR extraction  
- 📊 Word-level bounding box metadata  
- 📄 TSV structured parsing  
- 🌎 Real-time translation  
- ⚡ Server health monitoring  
- ⚙ Configurable AI microservice endpoints  

Designed for **enterprise-grade document intelligence workflows**.

---

## 🎥 Demo Preview

<p align="center">
  <img src="docs/demo/linguoai-demo.gif" width="95%">
</p>

> End-to-end OCR → Structured TSV → Real-Time Translation pipeline in action.

---

# ✨ Core Capabilities

| Category | Capability |
|-----------|------------|
| OCR | Word-level bounding boxes + confidence scoring |
| Parsing | TSV structured metadata extraction |
| Translation | REST-based real-time translation engine |
| Monitoring | Live service health polling |
| Configuration | Dynamic IP/Port endpoint switching |
| Logging | Real-time diagnostic feed |
| Extensibility | Modular AI service abstraction |

---

# 🏗 System Architecture

## High-Level Flow

```mermaid
flowchart LR
    UI["WPF Client (MVVM)"] --> OCR["OCR Service"]
    OCR --> OCRAPI["OCR API Endpoint"]
    OCR --> TSV["TSV Parser Engine"]
    UI --> TRANS["Translation Service"]
    TRANS --> TRANSAPI["Translation API Endpoint"]
    UI --> HEALTH["Server Health Monitor"]
```

---

## Document Processing Pipeline

```mermaid
sequenceDiagram
    participant User
    participant UI
    participant OCRService
    participant TranslationService
    participant OCRAPI
    participant TranslationAPI

    User->>UI: Upload Image
    UI->>OCRService: Send Multipart Image
    OCRService->>OCRAPI: POST /ocr
    OCRAPI-->>OCRService: JSON Response
    OCRService-->>UI: Extracted Text

    UI->>TranslationService: Send Text
    TranslationService->>TranslationAPI: POST /translate
    TranslationAPI-->>TranslationService: translatedText
    TranslationService-->>UI: Display Result
```

---

# 📡 API Specifications

## 🔹 Translation Endpoint

```
POST /translate
Content-Type: application/json
```

### Request

```json
{
  "q": "Hello world",
  "source": "en",
  "target": "es"
}
```

### Response

```json
{
  "translatedText": "Hola mundo"
}
```

---

## 🔹 OCR Endpoint

```
POST /ocr
Content-Type: multipart/form-data
```

### Response

```json
{
  "text": "Hello world",
  "words_info": [
    {
      "word": "Hello",
      "confidence": 98,
      "bounding_box": {
        "x1": 10,
        "y1": 12,
        "x2": 50,
        "y2": 28
      }
    }
  ]
}
```

---

# 📊 OCR Metadata Model

| Property | Description |
|-----------|------------|
| Level | Hierarchical grouping level |
| PageNum | Page index |
| BlockNum | Block identifier |
| LineNum | Line grouping |
| WordNum | Word index |
| Left/Top | Bounding box position |
| Width/Height | Bounding box size |
| Conf | Confidence score |
| Text | Extracted word |

---

# 🖥 Product Screens

## Dashboard

<p align="center">
  <img src="docs/images/dashboard.png" width="90%">
</p>

---

## OCR with Bounding Boxes

<p align="center">
  <img src="docs/images/ocr-overlay.png" width="90%">
</p>

---

## TSV Structured Analytics View

<p align="center">
  <img src="docs/images/tsv-analytics.png" width="90%">
</p>

---

## Translation Workspace

<p align="center">
  <img src="docs/images/translation-panel.png" width="90%">
</p>

---

# 🛠 Technical Stack

| Layer | Technology |
|-------|------------|
| UI | WPF (.NET 8) |
| Pattern | MVVM |
| Networking | HttpClient (async) |
| Data Binding | ObservableCollection |
| Parsing | JSON + TSV |
| Config | JSON-based settings |
| Monitoring | Periodic health polling |

---

# 📦 Project Structure

```
src/
 ├── Translator (WPF UI)
 ├── Translator Backend
 ├── Installer
docs/
 ├── images/
 ├── demo/
```

---

# 🚀 Enterprise Benefits

| Benefit | Value |
|----------|-------|
| Modular Architecture | Swap OCR/translation engines easily |
| Offline Capable | Suitable for secure environments |
| Structured Output | Enables downstream analytics |
| Real-Time Monitoring | Operational reliability |
| Clean Separation | Scalable service abstraction |

---

# 📈 Performance Characteristics

- Async non-blocking API calls  
- UI-thread safe dispatcher updates  
- Efficient bitmap streaming  
- Structured data parsing  
- Real-time observable updates  

---

# 🔐 Security Considerations

- No hardcoded secrets  
- Endpoint configurability  
- Controlled HTTP request handling  
- Defensive exception management  
- Config-based environment separation  

---

# 📌 Requirements

## Development

- .NET SDK 8+
- Visual Studio / Rider
- Windows OS

## Runtime

- OCR API service running
- Translation API service running
- Configured endpoints in `appsettings.json`

---

# 🎯 Engineering Highlights

- Clean MVVM architecture  
- Dependency injection pattern  
- Async command implementation  
- Service abstraction layer  
- Language mapping logic  
- Structured error logging  
- Health monitoring service  

---

# 💼 Use Cases

- Government document processing  
- Legal translation workflows  
- Healthcare documentation  
- Multilingual research analysis  
- Offline secure deployments  

---

# 🧠 Built By

**Brian Hannigan**  
Software Engineer | AI Systems Architect  
14+ years in secure government systems  
Specializing in AI-powered document intelligence platforms  

---

# ⭐ Why This Project Matters

LinguoAI demonstrates:

- AI system orchestration  
- Microservice integration  
- Clean UI architecture  
- Real-world document intelligence workflows  
- Production-ready engineering patterns  

---

# 📬 Connect

- GitHub
- LinkedIn
- Portfolio

---

