<div align="center">

![KAYZE Converter Tool Banner](KAYZE%20Converter%20Tool/ConverterTool/banner.svg)

# 🚀 KAYZE Converter Tool 🚀
## The All-in-One Creator Toolkit

[![English](https://img.shields.io/badge/Language-English-blue)](#english-version)
[![Italiano](https://img.shields.io/badge/Lingua-Italiano-green)](#versione-italiana)
[![Releases](https://img.shields.io/github/v/release/kayzedevx0/KAYZE-Converter-Tool?color=orange&label=Latest%20Release)](../../releases)

</div>

---

<h2 id="english-version">🇬🇧 English Version</h2>

### 🌟 Introduction

Welcome to **KAYZE Converter Tool**!

Built with **C#** and **.NET Windows Forms**, this application is a powerful, lightning-fast, and user-friendly desktop toolkit designed for creators, musicians, and everyday users. 

What makes it special is its true "All-in-One" nature: whether you need to convert images, process audio/video files, download media from YouTube, or even separate vocal and instrumental stems using Artificial Intelligence, this tool has got you covered in a single, modern interface!

### ✨ Features

- **Image Converter**: Lightning-fast batch processing for PNG, JPG, BMP, WEBP, ICO, TIFF, and more (Powered by `Magick.NET`).
- **Generic File Converter**: Convert media files (MP4, AVI, MKV, MP3, WAV) and documents (PDF, DOCX, TXT) seamlessly. 
- **YouTube Downloader**: Paste a link and download YouTube videos in Maximum Quality (MP4) or Audio Only (MP3).
- **AI Stem Separation**: Extract **Vocals, Drums, Bass, and Melody** from any audio track! The built-in setup will automatically install the required AI modules (Python, PyTorch, Demucs).
- **Drag & Drop Support**: Drop your files right into the app to start working.
- **Smart Optimization**: Auto-detects your CPU cores for multi-threaded processing.
- **Pixel-Perfect UI**: A fully custom dark/cyan modern interface, fully DPI-unaware to look perfect on any monitor scaling.

### 📥 Download (Ready to Use)

Don't want to compile it yourself? No problem! You can grab the ready-to-use portable version right now.

1. Go to the [Releases](../../releases) tab of this repository.
2. Download the latest `KAYZE Converter Tool.exe` file.
3. Run it and enjoy! *(Note: The app is standalone, but features like YouTube Download and AI Separation will automatically install required external modules on their first use).*

### 🛠️ How to Compile from Source

If you are a developer and want to build the tool yourself from the source code, follow these simple steps:

1. **Prerequisites**: Make sure you have [Visual Studio 2022](https://visualstudio.microsoft.com/) installed with the `.NET Desktop Development` workload.

2. **Clone the Repo**:
   
```bash
   git clone https://github.com/kayzedevx0/KAYZE-Converter-Tool.git
   ```

3. **Open the Project**: Navigate to the folder and double-click the `KAYZE Converter Tool.slnx` (or `.sln`) file to open the solution in Visual Studio.

4. **Restore NuGet Packages**: Right-click on the Solution in the Solution Explorer and select **Restore NuGet Packages**.

5. **Build & Publish**:
   - Right-click the Project in the Solution Explorer -> **Publish**.
   - Select **Folder** as the target.
   - Click on **Show all settings** and set:
     - **Target Runtime**: `win-x64`
     - **Deployment Mode**: `Self-contained`
   - Expand File Publish Options and check **Produce single file**.
   - Click **Publish**.
   - You will find your compiled single `.exe` file inside the `bin\\Release\\...\\publish` directory.

---

<h2 id="versione-italiana">🇮🇹 Versione Italiana</h2>

### 🌟 Introduzione

Benvenuto in **KAYZE Converter Tool**!

Sviluppato in **C#** e **.NET Windows Forms**, questa applicazione è un potente toolkit desktop, veloce e semplice da usare, pensato per creativi, musicisti e utenti di tutti i giorni.

Ciò che lo rende speciale è la sua natura "All-in-One": che tu debba convertire immagini, elaborare file audio/video, scaricare media da YouTube o persino separare le tracce vocali e strumentali usando l'Intelligenza Artificiale, questo tool fa tutto all'interno di un'unica interfaccia moderna!

### ✨ Funzionalità

- **Convertitore Immagini**: Elaborazione batch ultra-veloce per PNG, JPG, BMP, WEBP, ICO, TIFF e altri (Basato su `Magick.NET`).
- **Convertitore File Generici**: Converti file multimediali (MP4, AVI, MKV, MP3, WAV) e documenti (PDF, DOCX, TXT) senza sforzo.
- **YouTube Downloader**: Incolla un link e scarica video da YouTube alla Massima Qualità (MP4) o Solo Audio (MP3).
- **Separazione Stems con AI**: Estrai **Voce, Batteria, Basso e Melodia** da qualsiasi canzone! Il setup integrato installerà automaticamente i moduli AI necessari (Python, PyTorch, Demucs).
- **Supporto Drag & Drop**: Trascina i file direttamente nell'app per iniziare a lavorare.
- **Ottimizzazione Intelligente**: Rileva automaticamente i core della tua CPU per un'elaborazione multi-thread.
- **Interfaccia Pixel-Perfect**: UI completamente personalizzata in stile dark/cyan moderno, totalmente DPI-unaware per adattarsi perfettamente a qualsiasi ridimensionamento del monitor.

### 📥 Download (Pronto all'uso)

Non vuoi compilare il progetto da solo? Nessun problema. Puoi ottenere la versione portable pronta all'uso.

1. Vai nella sezione [Releases](../../releases) di questa repository.
2. Scarica l'ultima versione di `KAYZE Converter Tool.exe`.
3. Avvialo e divertiti! *(Nota: L'app è standalone, ma funzionalità come il Download di YouTube e la Separazione AI installeranno automaticamente i moduli esterni necessari al loro primo utilizzo).*

### 🛠️ Come compilare dal sorgente

Se sei uno sviluppatore e vuoi compilare il programma dal codice sorgente, segui questi passaggi:

1. **Requisiti**: Assicurati di avere [Visual Studio 2022](https://visualstudio.microsoft.com/) con il workload **Sviluppo desktop .NET**.

2. **Clona la repo**:

```bash
   git clone https://github.com/kayzedevx0/KAYZE-Converter-Tool.git
   ```

3. **Apri il progetto**: Entra nella cartella del progetto e apri il file `KAYZE Converter Tool.slnx` (oppure `.sln`) con Visual Studio.

4. **Ripristina i pacchetti NuGet**: Fai clic destro sulla soluzione e seleziona **Ripristina pacchetti NuGet**.

5. **Compila e pubblica**:
   - Fai clic destro sul progetto -> **Pubblica**.
   - Scegli **Cartella** come destinazione.
   - Apri **Mostra tutte le impostazioni** e imposta:
     - **Runtime di destinazione**: `win-x64`
     - **Modalità di distribuzione**: `Autocontenuto`
   - Espandi le opzioni di pubblicazione e attiva **Produci file singolo**.
   - Clicca su **Pubblica**.
   - Troverai il file `.exe` finale nella cartella `bin\\Release\\...\\publish`.

---

<div align="center">

Made with ❤️ by <b>KAYZE</b>

</div>
