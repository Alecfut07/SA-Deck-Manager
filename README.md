# 🎮 SA Deck Manager

A custom **Mod Manager for Sonic Adventure DX and Sonic Adventure 2 (Steam)** designed to run on **Steam Deck (Linux)** using Flatpak.

---

## 🚀 Project Goal

This project aims to create a **fully functional, Linux-compatible Mod Manager** that allows users to:

- Run mods for:
  - Sonic Adventure DX (Steam)
  - Sonic Adventure 2 (Steam)

- Use the tool directly on **Steam Deck**
- Avoid Windows-only limitations of existing tools

---

## 🧩 Background

Currently:

- **HedgeModManager** supports multiple Sonic games and works on Linux/Steam Deck via Flatpak
- **SA-Mod-Manager** supports Sonic Adventure DX and Sonic Adventure 2, but is **Windows-only**

This project is inspired by the strengths of both:

| Project         | Used For                                            |
| --------------- | --------------------------------------------------- |
| HedgeModManager | Linux compatibility, Flatpak structure, UI concepts |
| SA-Mod-Manager  | Game-specific mod loading logic                     |

---

## ⚙️ Tech Stack

- **Language:** C#
- **UI Framework:** Avalonia
- **Platform Target:** Linux (Steam Deck)
- **Packaging:** Flatpak
- **Compatibility Layer:** Proton (Steam)

---

## 🎯 Features (Planned)

### ✅ Core Features (MVP)

- Detect Sonic Adventure DX and Sonic Adventure 2 installations
- Display installed mods
- Enable / disable mods
- Launch game through Steam

### 🔄 Mid-Term Features

- Load order management
- Profiles / presets
- Mod metadata display

### 🧪 Advanced Features (Future)

- File redirection systems
- DLL-based mod support (if feasible on Proton)
- Dependency handling

---

## 🐧 Steam Deck Support

This project is designed specifically for:

- Steam Deck (SteamOS / Linux)
- Flatpak distribution
- Proton-based game execution

---

## ⚠️ Challenges

- Windows-specific mod logic must be adapted to Linux
- File paths differ between Windows and Proton environments
- Some mods may rely on DLL injection or memory patching

---

## 📦 Flatpak Notes

Planned permissions:

- `--filesystem=home`
- `--filesystem=xdg-data/Steam`

---

## 📁 Third-Party Components

This project includes or depends on the following third-party components:

- HedgeModManager
- SA-Mod-Manager

All third-party software is used in compliance with their respective.
See `THIRD_PARTY_NOTICES.md` for details.

---

## 📌 Notes

- This project is unofficial and not affiliated with SEGA.
- All trademarks belong to their respective owners.

---

## 💡 Future Vision

A unified Mod Manager that:

- Works Sonic Adventure 1 & 2
- Is fully Linux-compatible

---
