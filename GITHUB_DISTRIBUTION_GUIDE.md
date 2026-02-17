# Tejimola: The Blossom From Clay — GitHub Distribution Guide

> **Host your game on GitHub & share playable .exe with the world**

---

## 🎯 Overview

You can host the built game on GitHub and share a direct download link with anyone. They can download and play in 1 click!

### What You'll Do

1. Create GitHub repository
2. Build Windows .exe on Mac
3. Upload build folder to GitHub
4. Create Release with download
5. Share link → Users play immediately

---

## 📋 Prerequisites

### Required

- [ ] GitHub account (free from github.com)
- [ ] Git installed on Mac (`git --version` in Terminal)
- [ ] Built game folder (Tejimola/ with .exe)
- [ ] GitHub Desktop (optional, easier UI)

### Optional

- [ ] GitHub CLI (`gh` command line tool)
- [ ] Release notes written

---

## 🚀 Step 1: Create GitHub Repository

### Option A: Via GitHub Website (Easier)

```
1. Go to github.com
2. Login to your account
3. Click "+" → New repository

4. Fill in:
   Repository name:     tejimola-blossom-game
   Description:         "90-minute narrative adventure
                         celebrating Assamese folktales"
   Visibility:          Public

5. Initialize with:
   ✓ Add a README.md
   ✓ Add .gitignore → Select "Unity"

6. Click "Create repository"

Result:
   https://github.com/YourUsername/tejimola-blossom-game
```

### Option B: Via Git Command (Faster)

```bash
# On your Mac, in terminal:

# 1. Navigate to your build location
cd ~/Desktop/TejimolaBuild/

# 2. Initialize git
git init

# 3. Add remote repository
git remote add origin https://github.com/YourUsername/tejimola-blossom-game.git

# 4. Create initial commit
git add .
git commit -m "Initial commit: Tejimola game v1.0.0"

# 5. Push to GitHub
git push -u origin main

# Done! Repository created with all files
```

---

## 📦 Step 2: Upload Build Folder to GitHub

### Recommended: Create Release Instead of Regular Commit

Why use Releases?
- Designed for game distributions
- Users see "Download" button prominently
- Can upload pre-built .zip file
- Version tracking (v1.0.0, v1.0.1, etc.)
- Automatically generates changelog

### Option A: Via GitHub Website

```
1. Go to your repository:
   github.com/YourUsername/tejimola-blossom-game

2. Click "Releases" (right sidebar, under About)

3. Click "Create a new release" or "Draft a new release"

4. Fill in release details:

   Tag version:     v1.0.0
   Release title:   "Tejimola: The Blossom From Clay v1.0.0"

   Description:
   ```
   # Tejimola: The Blossom From Clay

   A 90-minute narrative adventure celebrating Assamese folktales.

   ## Download & Play

   1. Download the game below
   2. Unzip the folder
   3. Double-click `Tejimola.exe` to play

   ## Requirements

   - Windows 10/11 (64-bit)
   - 2 GB disk space
   - No installation needed!

   ## Game Features

   - 4 acts + epilogue
   - Stealth mechanics
   - Rhythm minigame
   - Memory puzzles
   - Boss fight
   - 90 minutes playtime

   ## How to Play

   - Arrow Keys / A-D: Move
   - Space: Jump / Spirit Pulse
   - E: Interact
   - Esc: Pause menu

   See full controls in game menu.
   ```

5. Attach the build folder:
   - Compress build folder:
     Right-click Tejimola/ → Compress → "Tejimola.zip"
   - Drag & drop Tejimola.zip into release assets
   - Or click "Attach binaries" button

6. Publish release:
   ✓ "Publish release" button

   Result:
   https://github.com/YourUsername/tejimola-blossom-game/releases/v1.0.0
```

### Option B: Via Git Command Line

```bash
# First, compress your build folder
cd ~/Desktop/TejimolaBuild/
zip -r Tejimola.zip Tejimola/

# Check zip file size (should be ~250-400 MB)
ls -lh Tejimola.zip

# Add to git (if not already added)
cd ~/path/to/repository/
git add Tejimola.zip
git commit -m "Add game v1.0.0 build"
git push origin main

# Create release using GitHub CLI (if installed)
gh release create v1.0.0 Tejimola.zip \
  --title "Tejimola: The Blossom From Clay v1.0.0" \
  --notes "Download and unzip. Double-click Tejimola.exe to play!"
```

---

## 💾 Step 3: File Size Considerations

### GitHub File Limits

| Plan | Max File Size | Total Repo Size |
|------|---------------|-----------------|
| Free | 100 MB | Unlimited (soft cap 1 GB) |
| Pro | 100 MB | Unlimited |
| Enterprise | 100 MB | Unlimited |

### Your Game Size

```
Build folder:        ~300-400 MB
Tejimola.zip:        ~80-120 MB (compressed)
```

**Problem**: .zip might exceed GitHub's 100 MB limit!

**Solution**: Use GitHub Releases with asset upload (not limited to 100 MB file size limit)

### Handling Large Files

**Option 1: Use GitHub Releases (Recommended)**
- No 100 MB limit on release assets
- Designed for game distribution
- Users see clear download button

**Option 2: Git LFS (Large File Storage)**
```bash
# Install Git LFS
brew install git-lfs

# Track large files
git lfs track "*.zip"
git add .gitattributes
git add Tejimola.zip
git commit -m "Add large build file"
git push
```

**Option 3: Split into smaller zips**
```bash
# Split 300MB into 100MB chunks
cd ~/Desktop/TejimolaBuild/
split -b 100M Tejimola.zip part_

# Users download and combine:
cat part_aa part_ab part_ac > Tejimola.zip
unzip Tejimola.zip
```

**Option 4: Alternative Hosting**
- Google Drive (up to 15 GB free)
- OneDrive (1 TB free)
- Dropbox (2 GB free)
- itch.io (free game hosting platform)
- GitHub + links to external storage

---

## 🔗 Step 4: Create Download Instructions

Add to your repository README.md:

```markdown
# Tejimola: The Blossom From Clay

A 90-minute narrative adventure celebrating Assamese folktales.

## 🎮 Download & Play

### Option 1: Direct Download (Recommended)
1. Go to [Releases](https://github.com/YourUsername/tejimola-blossom-game/releases)
2. Download `Tejimola.zip` (latest version)
3. Unzip folder
4. Double-click `Tejimola.exe` to play!

### Option 2: Clone Repository
```bash
git clone https://github.com/YourUsername/tejimola-blossom-game.git
cd tejimola-blossom-game
# Then run Tejimola.exe
```

## 💻 System Requirements

- **OS**: Windows 10 or Windows 11 (64-bit)
- **RAM**: 4 GB (8 GB recommended)
- **Storage**: 2 GB free space
- **GPU**: Intel HD 520 or equivalent

## ⌨️ Controls

| Key | Action |
|-----|--------|
| Arrow Keys / A-D | Move |
| Space | Jump / Spirit Pulse |
| E | Interact |
| C | Crouch |
| Q/E | Rhythm inputs |
| F | Use orbs |
| Esc | Pause |

## ⏱️ Playtime

**90 minutes** of continuous gameplay including:
- 4 acts + epilogue
- Story exploration
- Stealth sequences
- Rhythm minigame
- Memory puzzles
- Boss fight

## 🎨 Features

- Assamese cultural celebration
- Procedurally generated art
- Original Indian classical soundtrack
- Branching narrative with choices
- Save/load system
- Multiple game mechanics

## 📖 Documentation

See files in repository:
- `README.md` - Overview
- `QUICK_START_CHECKLIST.md` - Setup guide
- `WINDOWS_SETUP_GUIDE.md` - Troubleshooting
- `GAME_DEMO_WALKTHROUGH.txt` - Gameplay preview

## 📝 Credits

Game: Tejimola: The Blossom From Clay
Engine: Unity 2022 LTS
Based on: Assamese folktale traditions

## 📄 License

All game content, code, and assets created for this project.
```

---

## 📤 Step 5: Share Your Game Link

### Direct Links

```
GitHub Repository:
https://github.com/YourUsername/tejimola-blossom-game

Latest Release:
https://github.com/YourUsername/tejimola-blossom-game/releases/latest

Direct Download Link:
https://github.com/YourUsername/tejimola-blossom-game/releases/download/v1.0.0/Tejimola.zip
```

### Share on Social Media

```
Twitter/X:
"🎮 Just released my game!
'Tejimola: The Blossom From Clay'
- 90-minute narrative adventure
- Celebrating Assamese folktales
- Free to download!
👉 github.com/YourUsername/tejimola-blossom-game"

Reddit:
r/gamedev - "Released: Tejimola narrative game"
r/indiegames - "Indie game based on Assamese folktale"

LinkedIn:
"Excited to share my latest project:
Tejimola: The Blossom From Clay
A narrative-driven game celebrating cultural heritage..."

itch.io:
Upload game there too (free hosting platform for indie games)
```

---

## 🔄 Updating the Game (Future Versions)

When you fix bugs or add features:

```bash
# 1. Build new version on Mac
#    (File → Build Settings → Build)

# 2. Compress new build
cd ~/Desktop/TejimolaBuild/
zip -r Tejimola_v1.0.1.zip Tejimola/

# 3. Create new release on GitHub
gh release create v1.0.1 Tejimola_v1.0.1.zip \
  --title "Tejimola v1.0.1 - Bug fixes" \
  --notes "- Fixed rhythm game timing issue
           - Improved boss AI
           - Added subtitle options"

# 4. Users download latest version from Releases page
```

---

## 📊 Version Numbers

Recommended versioning scheme:

```
v1.0.0 - Initial release
v1.0.1 - Bug fixes
v1.0.2 - More bug fixes
v1.1.0 - New features
v1.2.0 - Major feature update
v2.0.0 - Significant overhaul
```

---

## 🎯 GitHub Release Checklist

- [ ] Created GitHub repository
- [ ] Repository is public
- [ ] Added descriptive README.md
- [ ] Built game on Mac (Tejimola.exe)
- [ ] Compressed build folder to .zip
- [ ] Created Release (not just commit)
- [ ] Uploaded .zip to release assets
- [ ] Added release description with instructions
- [ ] Tested download link works
- [ ] Shared link on social media
- [ ] Added download instructions to README

---

## 💡 Pro Tips

### Tip 1: Create Badges
Add to README.md top:
```markdown
![Release](https://img.shields.io/github/v/release/YourUsername/tejimola-blossom-game)
![Downloads](https://img.shields.io/github/downloads/YourUsername/tejimola-blossom-game/total)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![Duration](https://img.shields.io/badge/Duration-90%20minutes-brightgreen)
```

### Tip 2: Add Screenshots
```markdown
## Screenshots

![Menu](screenshots/menu.png)
![Gameplay](screenshots/gameplay.png)
![Boss Fight](screenshots/boss.png)
```

### Tip 3: Add Changelog
```markdown
## Changelog

### v1.0.0 (Feb 2026)
- Initial release
- 4 acts + epilogue
- All game systems functional
- 90 minute playtime

### v1.0.1 (Coming Soon)
- Performance improvements
- Bug fixes
```

### Tip 4: GitHub Pages (Optional)
Create a landing page for your game:
```
Settings → Pages → Choose theme
Creates: yourname.github.io/tejimola-blossom-game
```

---

## 🔐 Privacy & Distribution Notes

### Open Source vs. Closed Source

**Current Setup (Recommended for Games)**:
```
├── Public repository
├── Source code included (good for transparency)
├── Game executable included (for easy download)
└── Users can see how game is built
```

**If you want to hide source code**:
```
1. Remove /Assets/Scripts from repository
2. Only upload built .exe (in Tejimola_Data)
3. Users get playable game, not source
4. Add LICENSE file: MIT or similar
```

### License Recommendations

Add LICENSE file to repository:

```
MIT License - Most permissive (allow modifications)
Apache 2.0 - Balanced permissions
GPL - Share-alike (modifications must stay open)
```

For open source game:
```
# LICENSE file content (MIT)

MIT License

Copyright (c) 2026 [Your Name]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction...
[full MIT text]
```

---

## 🌐 Alternative Hosting Options

If GitHub doesn't work for you:

### itch.io (Recommended for Games)
```
✓ Built for indie game distribution
✓ Free hosting (no storage limit)
✓ Nice game page with ratings
✓ Download counter
✓ Community exposure
✓ Zip/exe files supported

Setup:
1. Create account at itch.io
2. Create project
3. Upload Tejimola.zip
4. Set to Windows platform
5. Publish
```

### Google Drive
```
✓ Simple folder sharing
✓ 15 GB free storage
✓ No download limit
✗ No version control
✗ No community features

Setup:
1. Upload Tejimola.zip to Google Drive
2. Share folder → Get link
3. Share link publicly
```

### Your Own Website
```
✓ Full control
✓ Professional appearance
✗ Need web hosting
✗ Need to manage downloads

Setup:
1. Buy domain
2. Get web hosting
3. Upload files
4. Create download page
```

---

## ✅ You're Ready!

1. **Create GitHub repository**
2. **Build game on Mac** (File → Build → Windows x64)
3. **Create Release with .zip file**
4. **Share download link**
5. **Anyone can play in 1 click!**

---

## 📚 Quick Reference Links

- GitHub Docs: https://docs.github.com/
- Git LFS: https://git-lfs.github.com/
- itch.io: https://itch.io/
- Badges: https://shields.io/

---

## 🎯 Share Your Game!

Once your game is on GitHub Releases, you can:

- Share direct download link with friends
- Post on social media
- Submit to game jams
- List on indie game sites
- Get community feedback

**Your game is ready to be played by the world!** 🌍

---

## 🆘 Troubleshooting

### "File too large for GitHub"
→ Use Releases instead of commits (no 100MB limit)
→ Or use Git LFS to track large files

### "Download link doesn't work"
→ Check repository is Public (not Private)
→ Try different browser/incognito mode
→ Verify .zip file is attached to Release

### "Users report game won't run"
→ Ensure .zip extracts correctly with folder structure
→ Check Tejimola.exe is in root of extracted folder
→ Provide Windows requirements list

### "Want to add more files to Release"
→ Edit Release → Add more files
→ Or create new Release version

---

**Happy distributing!** Share your game with the world! 🚀
