# Theocracy Save & File Encoder / Decoder (GUI)

[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://microsoft.com/windows)
[![Framework](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.x-yellow.svg)](https://python.org)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

An unofficial graphical tool to decode and encode save game files (`.tsg`) and configuration files (`.txt`) for the classic real-time strategy game **Theocracy** (2000, developed by Philos Laboratories / Ubisoft).

---

## 📖 Overview

The game *Theocracy* stores its save games (`saveXX.tsg`) and certain configuration files (such as `selap.txt`) in an obfuscated format protected by a custom 7-byte header and a double XOR cipher.

This tool allows you to:
- **Decode** encrypted game saves and configuration files into plaintext / raw editable formats for modding and hex editing.
- **Encode** modified files back into the format required by the game engine.
- **Inspect** file contents with a live Hex and ASCII dump and automatic save metadata detection (e.g., in-game Aztec date and engine signature).
- **Batch Process** multiple save files or entire game directories at once.

---

## ✨ Features

- **🌐 Bilingual User Interface**: Switch between 🇵🇱 Polish and 🇬🇧 English at any time with a single click in both C# (.NET 9) and Python (Tkinter) applications.
- **Automatic Format Detection**: Instantly identifies whether a file is currently:
  - 🟢 **Encrypted** (prefixed with the `RSA4096` header).
  - 🔵 **Decrypted / Plaintext** (raw data ready for editing or encoding).
- **Savegame Metadata Inspector**:
  - Automatically detects Theocracy save game signatures (`theosg42`).
  - Reads in-game Aztec calendar dates (e.g. `1449/13/14`).
  - Identifies INI configuration sections (e.g., `[Building_sys]`, `[Buildings]`).
- **Live Hex & Text Preview**: View the first 256 bytes in both hexadecimal and ASCII representation before and after processing.
- **Drag & Drop Support**: Drag `.tsg` or `.txt` files directly onto the window or onto the executable icon.
- **Safe Operations**:
  - Automatic backup creation (`.bak`) before modifying any file (enabled by default).
  - *"Save As..."* feature to export to a new file without touching the original.
- **Batch Processing**:
  - Decode or encode multiple files or an entire directory of saves in one click with a live progress bar.
- **Zero-Dependency Standalone Executable**:
  - `TheocracyEncDecGUI.exe` is completely self-contained—runs immediately on Windows 10/11 without requiring any runtimes installed.
- **Cross-Platform Python Version**:
  - `TheocracyEncDecGUI.py` is included for Python users (Tkinter).

---

## ⚙️ How the Algorithm Works

The game engine utilizes a lightweight, symmetric obfuscation scheme:

1. **Header**:
   - Encrypted files always begin with the 7-byte ASCII string: `"RSA4096"` (`0x52, 0x53, 0x41, 0x34, 0x30, 0x39, 0x36`).
2. **Encryption Keys**:
   - `Key 1`: `"theocracy sux"` (13 bytes / `0x0D`)
   - `Key 2`: `"mutant technology"` (17 bytes / `0x11`)
3. **Byte Transformation**:
   - Every byte `i` is XORed symmetrically:
     $$\text{byte}'[i] = \text{byte}[i] \oplus \text{Key2}[i \pmod{17}] \oplus \text{Key1}[i \pmod{13}]$$
4. **Operation Logic**:
   - **Decoding**: If the file starts with `"RSA4096"`, the 7-byte header is stripped, and the payload is XORed.
   - **Encoding**: If the file does not have the header, the entire content is XORed, and `"RSA4096"` is prepended.

---

## 🚀 Getting Started

### Option 1: Standalone Windows Executable (Recommended)
1. Download **`TheocracyEncDecGUI.exe`** from the [Releases](https://github.com/) tab.
2. Double-click to launch (no installation needed).
3. Drag and drop your `.tsg` save file or `.txt` config file into the application window.
4. Click **`🔓 Odszyfruj (Decode)`** or **`🔒 Zaszyfruj (Encode)`**.

### Option 2: Lightweight .NET 9 Build
If you already have .NET 9 Desktop Runtime installed, you can use **`TheocracyEncDecGUI_Lite.exe`** (only ~200 KB).

### Option 3: Python Script
If you prefer Python:
```bash
python TheocracyEncDecGUI.py
```
*(Optionally pass a file path as an argument: `python TheocracyEncDecGUI.py save00.tsg`)*

---

## 🛠️ Building from Source

Prerequisites: [.NET 9.0 SDK](https://dotnet.microsoft.com/download)

```bash
# Clone the repository
git clone https://github.com/your-username/theocracy-encoder-decoder.git
cd theocracy-encoder-decoder

# Build Debug version
dotnet build TheocracyEncDecGUI

# Publish standalone single-file binary
dotnet publish TheocracyEncDecGUI -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
```

---

## 📜 Credits & Attribution

- **Reverse Engineering & Cryptographic Discovery**:
  The underlying XOR algorithm, the encryption keys (`"theocracy sux"` / `"mutant technology"`), and the `"RSA4096"` signature were reverse-engineered by an **unknown community developer** and originally posted on an archived gaming/modding forum. The author of this repository is **not** the discoverer of the cipher algorithm; full credit goes to the anonymous researcher who initially analyzed the game binary.
- **GUI Application & Modern Architecture**:
  The modern C# (.NET 9 WinForms) and Python (Tkinter) graphical user interface, batch processing engine, metadata inspector, and build infrastructure were developed with the assistance of **Artificial Intelligence** ([Google DeepMind](https://deepmind.google/)).

---

## ⚖️ Legal Disclaimer

- This is an **unofficial, fan-made community utility** created strictly for interoperability, preservation, and game modding purposes (*pursuant to EU Directive 2009/24/EC and fair use principles*).
- **Theocracy** is a registered trademark of its respective copyright owners (*Philos Laboratories / Ubisoft*).
- This project is **not** affiliated with, endorsed by, or connected to the original game developers or publishers.
- **No copyrighted game assets or proprietary game binaries are distributed with this software.** Users must provide their own legally acquired game files.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
