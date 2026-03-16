<div align="center">

<!-- Sostituisci banner.png con il nome esatto della tua immagine se diverso -->
![KAYZE Converter Tool Banner](banner.svg)

# 🚀 A BRAND NEW TOOL FROM KAYZE 🚀
## KAYZE Converter Tool

[![English](https://img.shields.io/badge/Language-English-blue)](#english-version)
[![Italiano](https://img.shields.io/badge/Lingua-Italiano-green)](#versione-italiana)
[![Releases](https://img.shields.io/github/v/release/kayzedevx0/KAYZE-Converter-Tool?color=orange&label=Latest%20Release)](../../releases)

</div>

---

<h2 id="english-version">🇬🇧 English Version</h2>

### 🌟 Introduction
Welcome to **KAYZE Converter Tool**! This is a brand-new, lightning-fast, and user-friendly image conversion application built with C# and .NET Windows Forms. Whether you need to convert a single image or batch-process multiple files between PNG, JPG, ICO, WEBP, and more, this tool has got you covered!

### ✨ Features
- **Drag & Drop Support**: Just drop your images right into the app!
- **Multiple Formats**: Seamlessly convert between PNG, JPG, BMP, WEBP, ICO, and more.
- **Lightning Fast Processing**: Powered by `Magick.NET` for optimized speed without quality loss.
- **Standalone Executable**: True portable experience. No installation required!

### 📥 Download (Ready to Use)
Don't want to compile it yourself? No problem! You can grab the ready-to-use version right now.

1. Go to the [Releases](../../releases) tab of this repository.
2. Download the latest `KAYZE Converter Tool.exe` file.
3. Run it and start converting your images!

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
   - You will find your compiled single `.exe` file inside the `bin\Release\...\publish` directory.

---

<h2 id="versione-italiana">🇮🇹 Versione Italiana</h2>

### 🌟 Introduzione
Benvenuto in **KAYZE Converter Tool**! Un'applicazione nuovissima, veloce e semplice da usare per la conversione di immagini, creata in C# e .NET Windows Forms. Che tu debba convertire una singola immagine o elaborarne diverse tra formati come PNG, JPG, ICO, WEBP e altri, questo tool fa al caso tuo!

### ✨ Funzionalità
- **Supporto Drag & Drop**: Trascina le immagini direttamente nell'app.
- **Multi-formato**: Converti facilmente tra PNG, JPG, BMP, WEBP, ICO e altri formati.
- **Elaborazione veloce**: Basato su `Magick.NET` per ottime prestazioni e qualità.
- **Eseguibile standalone**: Esperienza portable, senza installazione.

### 📥 Download (Pronto all'uso)
Non vuoi compilare il progetto da solo? Nessun problema.

1. Vai nella sezione [Releases](../../releases) di questa repository.
2. Scarica l'ultima versione di `KAYZE Converter Tool.exe`.
3. Avvialo e inizia subito a convertire le immagini.

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
   - Troverai il file `.exe` finale nella cartella `bin\Release\...\publish`.

---

<div align="center">
Made with ❤️ by <b>KAYZE</b>
</div>
