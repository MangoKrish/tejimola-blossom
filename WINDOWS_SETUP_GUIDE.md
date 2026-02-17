# Tejimola: The Blossom From Clay — Windows Setup Guide

## ⚙️ Prerequisites (Install First)

### 1. **Unity Editor** (Required)
- **Download**: https://unity.com/download
- **Version Required**: 2022 LTS or newer (latest recommended)
- **Download Size**: ~3 GB
- **Installation**: Use Unity Hub for easy management

### 2. **Visual Studio Community** (Recommended for debugging)
- **Download**: https://visualstudio.microsoft.com/downloads/
- **Choose**: "Desktop development with C++" workload
- **Size**: ~5 GB
- **Optional**: You can use VS Code instead if you prefer

### 3. **System Requirements (Minimum)**
- **OS**: Windows 10 or Windows 11 (64-bit)
- **Processor**: Intel i3 or AMD equivalent (game runs at 90 FPS on much older hardware)
- **RAM**: 4 GB (8 GB recommended)
- **GPU**: Intel HD 520 or NVIDIA GT 730 equivalent
- **Storage**: 2 GB free space
- **Internet**: Required for initial Unity setup

---

## 📥 Installation Steps

### Step 1: Copy Project Folder to Windows PC
```
Copy the entire "TejimolaBlossom" folder from your Mac to your Windows laptop
Example location: C:\Users\YourUsername\Documents\TejimolaBlossom\
```

### Step 2: Open Project in Unity Hub
1. Open **Unity Hub**
2. Click **"Add"** → **"Add project from disk"**
3. Navigate to and select the **TejimolaBlossom** folder
4. Unity will automatically detect it as a project
5. Click to open with your installed Unity version (2022 LTS or newer)

### Step 3: Initial Setup (First Time Only)
- **Wait** for Unity to import assets (5-10 minutes first time)
- You'll see a progress bar in the bottom right
- Unity will compile all C# scripts
- Dialogue JSON files will be loaded from Resources folder
- Audio and image assets will be imported

### Step 4: Create Scenes in Unity Editor
The project has scripts but needs scene files to be created. Here's the quick way:

**Option A: Quick Play Mode Test** (Fastest - No setup needed)
1. In the Unity Editor, go to **File** → **New Scene**
2. Attach `MainMenuSetup.cs` to an empty GameObject
3. Add a Canvas for the UI
4. Press **Play** to test

**Option B: Full Scene Setup** (Recommended)
1. **File** → **New Scene** → Save as "MainMenu"
2. Add these GameObjects:
   - Canvas (UI)
   - Camera (with ParallaxCamera script)
   - Player (with Dom or Tejimola script)
   - Background Layers (with ParallaxLayer scripts)
   - Audio Manager (with AudioManager script)
   - Dialogue Manager (already a singleton)
   - Event Manager (already a singleton)
3. **Asset** → **Scenes** → Repeat for each Act

### Step 5: Assign Assets to Scene
1. Drag character sprites from `Assets/_Project/Art/Sprites/Characters/` into scene
2. Drag background layers from `Assets/_Project/Art/Backgrounds/Act1/` into scene
3. Attach scripts to GameObjects:
   - Add `DomBehaviour` or `TejimolaBehaviour` to player
   - Add `ParallaxLayer` to background layers
   - Add `SceneSetup` (act-specific) to empty GameObject

### Step 6: Play the Game
- Press **Play** (Ctrl+P) to test in Editor
- Or **Build** for standalone Windows .exe

---

## 🎮 Building for Windows

### Option 1: Editor Play Mode (Development)
- Simply press **Play** in the Unity Editor
- Takes 3-5 seconds to start
- Great for testing and debugging

### Option 2: Standalone Build (Distribution)
1. **File** → **Build Settings**
2. Click **"Add Open Scenes"** to add all scenes
3. Set target: **Windows**
4. Set architecture: **x64** (Windows 64-bit)
5. Click **"Build"**
6. Choose output folder (e.g., `C:\Games\Tejimola\`)
7. Unity generates:
   - `Tejimola.exe` (the game executable)
   - `Tejimola_Data/` folder (all assets)
   - `MonoBleedingEdge/` folder (runtime libraries)

### Run Standalone Build
- Double-click `Tejimola.exe`
- No installation needed
- Can be shared as a folder or packaged as installer

---

## 🛠️ Troubleshooting

### Issue: "Assets not loading" or pink/purple textures
**Solution**:
- Right-click in Project window → **Reimport All**
- This refreshes all asset references

### Issue: "Scripts have errors"
**Solution**:
- **Window** → **General** → **Console** to see errors
- Most errors are Unity version mismatches - usually auto-fix on reimport

### Issue: Audio not playing
**Solution**:
- Check **Edit** → **Preferences** → **Audio**
- Ensure your speakers are connected
- Try adjusting volume in **Edit** → **Preferences** → **Audio**

### Issue: Game runs slowly
**Solution**:
- Lower screen resolution in **Edit** → **Project Settings** → **Player**
- Reduce particle effects in **Scene** → **Lighting**
- Check GPU drivers are up to date

### Issue: "DLL not found" or runtime errors
**Solution**:
- Make sure Visual C++ Redistributable is installed:
  https://support.microsoft.com/en-us/help/2977003
- Install from Windows Update if needed

---

## 📊 Game Duration & Content

### Story Length: **90 minutes**
```
Act 1: The Happy Home (15 min)
  - Explore family home
  - Watch father's happiness and warmth

Act 1: The Funeral (10 min)
  - Witness father's death
  - Experience emotional turning point

Act 2: The Descent (20 min)
  - Stealth sequences avoiding Ranima
  - Rhythm-based dheki minigame with visions
  - Witness Tejimola's tragic fate

Act 3: Spirit Awakens (25 min)
  - Play as Dom (spirit-sensitive drummer)
  - Explore ruined house 20 years later
  - Recover 5 memories through puzzles
  - Dual-timeline gameplay with split-screen

Act 4: Confrontation (15 min)
  - 3-phase boss fight vs corrupted Ranima
  - Surreal inverted world
  - Navigate obstacles, use spirit orbs, dodge barrel

Epilogue (5 min)
  - Resolution and closure
  - Nahor tree blooms
```

### Gameplay Features
- **Exploration**: Walk, examine objects, interact
- **Stealth (Act II)**: Hide from Ranima, avoid detection
- **Rhythm (Act II)**: Match drum beats (Q/E keys), BPM progression
- **Puzzles (Act III)**: 5 memory recovery tasks with split-screen
- **Boss Fight (Act IV)**: 3-phase combat with dodge mechanics

---

## ⌨️ Controls

| Input | Action |
|-------|--------|
| **Arrow Keys** / **A/D** | Move left/right |
| **Space** | Jump (Dom: Spirit Pulse / Tejimola: Hide in spots) |
| **E** | Interact with objects/dialogue |
| **C** / **Ctrl** | Crouch (for stealth) |
| **Q / E** | Rhythm game inputs (dheki sequence) |
| **F** | Use spirit orbs (boss fight) |
| **Esc** | Pause menu |
| **Mouse** | Menu navigation |

---

## 📱 First Time Walkthrough

**When you start the game:**
1. Main Menu appears with Nahor tree art
2. Click **"NEW GAME"** to begin
3. **Act 1**: You're Tejimola, explore home, talk to father
4. **Funeral scene**: Watch story unfold
5. **Act 2**: Stealth phase, Ranima arrives
6. **Dheki sequence**: Match rhythm beats (visual guide will appear)
7. **Act 3**: Switch to Dom, explore and recover memories
8. **Puzzle rooms**: Each memory is a split-screen puzzle
9. **Act 4**: Boss fight - dodge, use orbs, survive
10. **Epilogue**: Watch the nahor tree bloom at sunrise

**Total playtime: 90 minutes non-stop**
(Can save and resume with SAVE button in pause menu)

---

## 🎬 Demo Mode (Quick Test)

To quickly demo the game without full setup:

1. In Unity Editor, create a new scene
2. Add these to the scene:
   - **Camera**
   - **Canvas** (for UI)
   - **Character sprite** (from Sprites/Characters/)
   - **Background** (from Backgrounds/)
3. Attach script: `MainMenuSetup.cs` to empty GameObject
4. Press **Play**
5. You should see menu with title and buttons

This will show you the core systems work without needing full scene setup.

---

## 💾 Save & Load

- **Save**: Pause menu (Esc) → Click "SAVE"
- **Load**: Main menu → Click "CONTINUE" (if save exists)
- Save files are stored in: `C:\Users\YourUsername\AppData\LocalLow\Tejimola Games\`
- You can have multiple saves if you rename the save file

---

## 🎨 Customization

Want to modify the game?

- **Change colors**: Edit `Constants.cs` in `Scripts/Utils/`
- **Adjust difficulty**: Edit gameplay values in `Constants.cs`
- **Modify dialogue**: Edit JSON files in `Resources/Dialogue/`
- **Add new music**: Add WAV files to `Audio/Music/` and reference in `AudioManager.cs`
- **Create new scenes**: Use existing scripts as templates

---

## ✅ Final Checklist Before Playing

- [ ] Windows 10/11 with 2 GB free space
- [ ] Unity 2022 LTS or newer installed
- [ ] TejimolaBlossom folder copied to PC
- [ ] Project opened in Unity Editor
- [ ] Assets reimported (no pink textures)
- [ ] No red errors in Console
- [ ] Scenes created/set up
- [ ] Ready to play!

---

## 📞 Support

If you encounter issues:
1. Check **Console** (Ctrl+Shift+C) for error messages
2. Try **File** → **Reimport All Assets**
3. Restart Unity Editor
4. As last resort: Delete `Library` folder in project, let Unity regenerate

---

**Enjoy Tejimola: The Blossom From Clay!** 🌸

This is a complete narrative experience celebrating Assamese folktale traditions through interactive game design.
