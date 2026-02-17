# Tejimola: The Blossom From Clay — Quick Start Checklist

> **TL;DR**: Copy folder → Open in Unity → Press Play → Enjoy 90-minute game
>
> **Total setup time: 15-30 minutes** (including Unity import)

---

## ⚡ Super Quick Start (5 minutes to first demo)

If you just want to see the game running immediately without full setup:

1. **Copy the TejimolaBlossom folder to your Windows PC**
   - Location: `C:\Users\YourUsername\Documents\TejimolaBlossom`

2. **Open Unity Hub** (must have Unity 2022 LTS installed)
   - Click **"Add"** → **"Add project from disk"**
   - Navigate to your TejimolaBlossom folder
   - Click to open

3. **Wait for import** (5-10 minutes)
   - Unity will compile scripts and import assets
   - Watch the progress bar in bottom right

4. **Create ONE simple scene** (takes 2 minutes)
   - In Unity: **File** → **New Scene**
   - Attach `MainMenuSetup.cs` script to an empty GameObject
   - Add a Canvas (right-click in hierarchy → UI → Canvas)
   - **Press Play** (Ctrl+P)
   - You'll see the main menu!

---

## 📋 Complete Setup Checklist

### Pre-Flight (Before Starting)
- [ ] Windows 10 or Windows 11 (64-bit) installed
- [ ] At least 2 GB free hard drive space
- [ ] Internet connection available (for Unity download)
- [ ] Administrator access to install software

### Step 1: Install Prerequisites (10 minutes)
- [ ] **Unity Hub** installed from https://unity.com/download
- [ ] **Unity 2022 LTS** (or newer) installed via Hub
  - Estimated install: 3 GB, takes 10-15 minutes
- [ ] *Optional*: Visual Studio Community (for debugging)

### Step 2: Copy Project (1 minute)
- [ ] TejimolaBlossom folder copied to Windows PC
  - Suggested location: `C:\Users\YourUsername\Documents\TejimolaBlossom`
  - Folder size: ~500 MB
- [ ] Verified folder contains `Assets/`, `Packages/`, `ProjectSettings/`

### Step 3: Open in Unity (5 minutes)
- [ ] Opened Unity Hub
- [ ] Clicked **"Add"** → **"Add project from disk"**
- [ ] Selected TejimolaBlossom folder
- [ ] Project opened in Unity Editor
- [ ] Progress bar reached 100% (import complete)

### Step 4: Import Assets (10 minutes)
- [ ] No pink/purple textures visible (if they appear, right-click → Reimport All)
- [ ] Console shows NO red errors (Window → General → Console)
- [ ] All C# scripts compiled successfully

### Step 5: Create Scenes (Optional but Recommended)
**Option A: Quick Test** (2 minutes - minimal setup)
- [ ] Created new scene: **File** → **New Scene**
- [ ] Attached `MainMenuSetup.cs` to empty GameObject
- [ ] Added Canvas for UI
- [ ] Pressed **Play** — Main menu appears ✓

**Option B: Full Setup** (30 minutes - complete experience)
- [ ] Created scene "MainMenu"
- [ ] Added Camera, Canvas, AudioListener
- [ ] Attached `MainMenuSetup.cs` to empty GameObject
- [ ] Repeated for each act:
  - Act1_HappyHome
  - Act1_Funeral
  - Act2_Descent
  - Act2_Dheki
  - Act2_Burial
  - Act3_DomArrival
  - Act3_DualTimeline
  - Act4_Confrontation
  - Epilogue

### Step 6: Test the Game
- [ ] Pressed **Play** in Unity Editor
- [ ] Main menu displays correctly
- [ ] Audio plays when music should play
- [ ] No crashes or errors in Console

### Step 7: Build Standalone Windows .exe (10 minutes)
- [ ] Went to **File** → **Build Settings**
- [ ] Added all scenes via **"Add Open Scenes"**
- [ ] Set Platform: **Windows**
- [ ] Set Architecture: **x64**
- [ ] Clicked **"Build"**
- [ ] Chose output folder: `C:\Games\Tejimola\` (or preferred location)
- [ ] Unity generated `Tejimola.exe`
- [ ] Tested by double-clicking `Tejimola.exe`
- [ ] Game launched successfully ✓

---

## 🎮 Controls (Keep This Handy!)

| Key | Action |
|-----|--------|
| **Arrow Keys** or **A/D** | Move left/right |
| **Space** | Jump / Spirit Pulse |
| **E** | Interact with objects / Dialogue continue |
| **C** or **Ctrl** | Crouch (stealth mode) |
| **Q / E** | Rhythm game inputs (beat matching) |
| **F** | Use spirit orbs (boss fight) |
| **Esc** | Pause menu |
| **Mouse** | Menu navigation |

---

## ⏱️ Game Duration at a Glance

```
Act 1: Happy Home              15 minutes
├─ Explore family courtyard, interact with father
└─ Witness father's death - turning point

Act 2: The Descent             20 minutes
├─ Stealth sequences (hide from Ranima)
├─ Rhythm-based dheki minigame (90-150 BPM progression)
└─ Witness Tejimola's fate

Act 3: Spirit Awakens          25 minutes
├─ Play as Dom (spirit-sensitive drummer)
├─ Explore ruined house 20 years later
└─ Recover 5 memories via puzzles (split-screen)

Act 4: Confrontation           15 minutes
├─ 3-phase boss fight vs corrupted Ranima
├─ Navigate obstacles → Use spirit orbs → Barrel pursuit
└─ Epic conclusion

Epilogue                       5 minutes
└─ Nahor tree blooms, peaceful resolution

TOTAL: ~90 minutes (non-stop playthrough)
```

---

## 🚨 Troubleshooting Quick Links

| Problem | Solution |
|---------|----------|
| Pink/purple textures | Right-click → **Reimport All** |
| Script errors in Console | **Window** → **General** → **Console** (view errors) |
| No audio playing | Check **Edit** → **Preferences** → **Audio** |
| Game runs slowly | Lower resolution in **Edit** → **Project Settings** → **Player** |
| "DLL not found" error | Install Visual C++ Redistributable (see full guide) |

> For detailed troubleshooting, see **WINDOWS_SETUP_GUIDE.md**

---

## 📂 Important Locations

| Item | Location |
|------|----------|
| Project folder | `C:\Users\YourUsername\Documents\TejimolaBlossom` |
| Save files | `C:\Users\YourUsername\AppData\LocalLow\Tejimola Games\` |
| Build output | `C:\Games\Tejimola\` (or your chosen folder) |
| Console errors | Window → General → Console |

---

## 🎯 Next Steps

**After setup:**
1. **Play the game** in Unity Editor first (Edit → Play or Ctrl+P)
2. **Test controls** (movement, dialogue, interactions)
3. **Build standalone** executable for distribution
4. **Share with others** - just send them the `Tejimola.exe` folder!

---

## 📞 Still Stuck?

1. **Check Console for errors**: Window → General → Console (Ctrl+Shift+C)
2. **Reimport assets**: Right-click Project → Reimport All
3. **Restart Unity**: Close and reopen the editor
4. **Last resort**: Delete `Library/` folder in project, let Unity regenerate
5. **See full guide**: Open **WINDOWS_SETUP_GUIDE.md** for comprehensive help

---

## ✨ What You're About to Experience

A **beautiful, intimate narrative game** exploring an Assamese folktale through:
- **Atmospheric exploration** of a family home
- **Stealth sequences** with tension and consequence
- **Rhythm gameplay** combining music and memory
- **Dual-timeline puzzles** revealing hidden truths
- **Epic boss confrontation** in a surreal world
- **Emotional resolution** celebrating storytelling and cultural heritage

**Playtime: 90 minutes of continuous, emotionally-driven narrative experience**

---

**Enjoy Tejimola: The Blossom From Clay!** 🌸

A game celebrating Assamese traditions through interactive storytelling.
