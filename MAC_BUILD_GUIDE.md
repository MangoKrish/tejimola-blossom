# Tejimola: The Blossom From Clay — Mac Build Guide

> **Build the Windows .exe game directly from your Mac laptop**

---

## ✅ Yes, You Can Build on Mac!

Unity allows you to build Windows executables **directly from macOS** using **IL2CPP** backend, even though you can't *run* the Windows game on Mac.

### Why This Works
- Unity supports cross-platform building
- The project uses IL2CPP scripting backend (already configured in ProjectSettings)
- All source code is platform-agnostic C#
- Build process generates Windows x64 binary on Mac

---

## 📋 Prerequisites

### On Your Mac (Required)

1. **Unity 2022 LTS** (already have it)
   ```
   ✓ Installed via Unity Hub
   ✓ Version: 2022 LTS or newer
   ```

2. **Project Folder**
   ```
   ✓ TejimolaBlossom folder accessible
   ✓ All assets present (58 MB)
   ```

3. **Disk Space**
   ```
   ✓ 5-10 GB free space (for build cache + output)
   ✓ SSD recommended (faster build process)
   ```

**That's it!** No Windows software needed on Mac.

---

## 🔨 Step-by-Step Mac Build Process

### Step 1: Open Project in Unity

```
1. Launch Unity Hub on Mac
2. Click "Open" → Select TejimolaBlossom folder
3. Wait for project to import (5-10 minutes)
4. Verify no red errors in Console (Ctrl+Shift+C)
```

### Step 2: Configure Build Settings

```
1. In Unity Editor: File → Build Settings
   (or press Ctrl+Shift+B)

2. Add Scenes to Build:
   - Click "Add Open Scenes" button
   - This adds currently open scene
   - Or manually add each scene:
     ✓ Assets/_Project/Scenes/MainMenu.unity
     ✓ Assets/_Project/Scenes/Act1_HappyHome.unity
     ✓ Assets/_Project/Scenes/Act1_Funeral.unity
     ✓ Assets/_Project/Scenes/Act2_Descent.unity
     ✓ Assets/_Project/Scenes/Act2_Dheki.unity
     ✓ Assets/_Project/Scenes/Act2_Burial.unity
     ✓ Assets/_Project/Scenes/Act3_DomArrival.unity
     ✓ Assets/_Project/Scenes/Act3_DualTimeline.unity
     ✓ Assets/_Project/Scenes/Act4_Confrontation.unity
     ✓ Assets/_Project/Scenes/Epilogue.unity

3. Verify Scene List Order:
   - MainMenu should be at index 0
   - Acts should follow in order
   - Scenes set to "Enabled" (checkbox on)

4. Select Target Platform:
   - Left sidebar: Select "PC, Mac & Linux Standalone"
   - Bottom right: Select "Windows"

5. Select Target Architecture:
   - Architecture dropdown: Select "x64" (64-bit)
   - ✓ Not x86 (32-bit is outdated)

6. Build Settings Configuration:
   ┌─────────────────────────────────────────┐
   │ Scenes In Build:        10 scenes       │
   │ Platform:               Windows         │
   │ Architecture:           x64             │
   │ Development Build:      ✓ Unchecked     │
   │ Autoconnect Profiler:   ✓ Unchecked     │
   │ Deep Profile:           ✓ Unchecked     │
   │ Script Debugging:       ✓ Unchecked     │
   └─────────────────────────────────────────┘
```

### Step 3: Choose Build Output Location

```
1. Click "Build" button in Build Settings window

2. macOS File Dialog appears:
   - Navigate to desired location
   - Suggested: ~/Desktop/TejimolaBuild/
   - Or: ~/Documents/Games/Tejimola/

3. Name the build folder: "Tejimola"
   (Unity creates .exe inside this folder)

4. Click "Save"
   (macOS will ask for location confirmation)
```

### Step 4: Build Process

```
Build begins (takes 5-15 minutes depending on Mac):

Progress indicators:
├─ "Building player..." → Scripts compiling
├─ "Compiling assemblies..." → C# compilation
├─ "Stripping assemblies..." → Removing unused code
├─ "Building scenes..." → Scene graph generation
├─ "Copying engine files..." → Runtime libs
└─ "Finalizing output..." → Finishing touches

Status bar shows: [████████░░░░░░] 45%

When complete:
✓ Build completed successfully!
✓ Output folder opens in Finder

Build output location:
~/Desktop/TejimolaBuild/Tejimola/  (example)
```

### Step 5: Verify Build Output

```
Build folder structure created:

Tejimola/
├── Tejimola.exe              ← Windows executable (main game)
├── Tejimola_Data/            ← Game assets & data
│   ├── resources.assets      ← Compiled resources
│   ├── level0                ← MainMenu scene
│   ├── level1                ← Act1_HappyHome
│   └── [8 more scenes...]
├── MonoBleedingEdge/         ← .NET runtime libraries
│   ├── mono.dll
│   ├── mscorlib.dll
│   └── [other .dlls...]
└── UnityCrashHandler64.exe   ← Error reporting (optional)
```

**Total Size**: ~250-400 MB (including all assets & runtime)

---

## 📊 Build Size Breakdown

| Component | Size |
|-----------|------|
| Tejimola.exe | 5-10 MB |
| Tejimola_Data/ (assets) | 200-250 MB |
| MonoBleedingEdge/ (runtime) | 30-50 MB |
| **Total** | **250-400 MB** |

*Exact size depends on Unity version & IL2CPP settings*

---

## 🔍 Verify the Build Works on Mac (Optional)

You can't *run* the Windows .exe on Mac, but you can verify its integrity:

```bash
# In Terminal, navigate to build folder:
cd ~/Desktop/TejimolaBuild/Tejimola/

# Check file exists and is executable:
file Tejimola.exe
# Should output: PE32+ executable (Windows)

# Verify size:
ls -lh Tejimola.exe
# Should show file size (5-10 MB)

# List all contents:
ls -lhR
# Shows complete folder structure
```

---

## 🚀 Quick Build Checklist (Mac)

- [ ] Unity 2022 LTS open with TejimolaBlossom project
- [ ] Project imported (no pink textures)
- [ ] Console shows no red errors
- [ ] 10 scenes added to Build Settings
- [ ] Platform: Windows
- [ ] Architecture: x64
- [ ] Output location chosen
- [ ] Build starts and shows progress
- [ ] Build completes successfully
- [ ] Output folder visible in Finder
- [ ] Tejimola.exe file present
- [ ] Tejimola_Data folder present (200+ MB)

---

## ⏱️ Build Times by Mac Model

| Mac Model | SSD | Build Time |
|-----------|-----|------------|
| M1/M2 Ultra (2023+) | NVMe | 5-8 min |
| M1/M2 Pro/Max | SSD | 8-12 min |
| M1/M2 Air/Base | SSD | 12-15 min |
| Intel i7 16" | SSD | 15-20 min |
| Intel i5 13" | HDD | 30-45 min |

**Tip**: Build on external SSD for faster results.

---

## 🐛 Troubleshooting Mac Builds

### Issue: "Build Settings don't show Windows option"
**Solution**:
```
1. File → Build Settings
2. Left sidebar: "PC, Mac & Linux Standalone"
3. Click the "Windows" button (bottom right)
4. If still missing: restart Unity
```

### Issue: "Platform not available"
**Solution**:
```
1. Check Unity version: Help → About
2. Requires: 2022 LTS or newer
3. Older versions: may not include Windows build support
4. Download latest 2022 LTS from unity.com/download
```

### Issue: "Build takes too long or freezes"
**Solution**:
```
1. Close all other applications
2. Ensure 5+ GB free disk space
3. Try Development Build first (faster):
   - File → Build Settings → checkmark "Development Build"
   - Then do final release build without it
4. Check Mac isn't thermal throttling (fans loud?)
```

### Issue: "Out of disk space during build"
**Solution**:
```
1. Build requires ~5-10 GB free space
2. Free up space: Trash → Empty Trash
3. Clear Unity cache:
   - ~/Library/Caches/Unity/
4. Try building to external drive
```

### Issue: "Build succeeds but .exe seems small/incomplete"
**Solution**:
```
1. Check Tejimola_Data folder exists (should be 200+ MB)
2. Verify total folder size:
   ls -lhR ~/path/to/build/
3. If small: build may have failed silently
4. Check Console for warnings/errors
5. Try building again
```

---

## 💾 Distributing Your Build

Once build is complete:

### Option 1: GitHub Release (Recommended)
```
1. Create GitHub repository
2. Add build folder to repo
3. Create Release with .zip of build folder
4. Users download & unzip
5. Users double-click Tejimola.exe to play

See: GITHUB_DISTRIBUTION_GUIDE.md (next section)
```

### Option 2: Direct Folder Share
```
1. Zip entire Tejimola/ folder:
   Right-click → Compress → "Tejimola.zip"

2. Share .zip file (250-400 MB):
   - Google Drive
   - Dropbox
   - OneDrive
   - WeTransfer

3. Users unzip and run Tejimola.exe
```

### Option 3: Installer (Advanced)
```
Create Windows .msi installer:
1. Use tools like Inno Setup
2. Bundle build folder + installer wizard
3. Users download .msi file
4. Click to install (adds to Programs/Apps)

See: INSTALLER_CREATION_GUIDE.md (future)
```

---

## ✨ You're Ready to Build!

1. **Open Unity** with TejimolaBlossom project
2. **File → Build Settings**
3. **Add 10 scenes, select Windows x64**
4. **Click Build**
5. **Wait 5-15 minutes**
6. **Share Tejimola.exe with world!**

The build process is handled entirely by Unity on your Mac.

---

## 📚 Related Guides

- `GITHUB_DISTRIBUTION_GUIDE.md` - Host & distribute via GitHub
- `WINDOWS_SETUP_GUIDE.md` - Setup on Windows PCs
- `QUICK_START_CHECKLIST.md` - Game setup guide

---

## 🎯 Key Takeaway

**Yes, you can build the Windows .exe game directly from your Mac.**

Unity's cross-platform building handles all the heavy lifting. Your Mac acts as a compiler and linker to generate Windows binaries. No Windows PC required!

After building, your Tejimola.exe works on any Windows 10/11 PC.

Happy building! 🎮
