#!/usr/bin/env python3
"""
Tejimola: The Blossom From Clay — Complete Asset Generator
Generates all sprites, backgrounds, UI, and audio using Python/PIL/numpy.
All art is original creative work generated programmatically.
"""

from PIL import Image, ImageDraw, ImageFilter, ImageFont
import numpy as np
import wave, struct, os, math, random, colorsys

ROOT = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project"
ART  = f"{ROOT}/Art"
AUD  = f"{ROOT}/Audio"

# ─────────────────────────────────────────────────────────────────────────────
# DIRECTORY SETUP
# ─────────────────────────────────────────────────────────────────────────────

def mkdirs():
    for d in [
        f"{ART}/Backgrounds/Act1", f"{ART}/Backgrounds/Act2",
        f"{ART}/Backgrounds/Act3", f"{ART}/Backgrounds/Act4",
        f"{ART}/Sprites/Characters", f"{ART}/Sprites/Props",
        f"{ART}/UI/Menu", f"{ART}/UI/DialogueBox", f"{ART}/UI/HUD",
        f"{ART}/VFX",
        f"{AUD}/Music", f"{AUD}/SFX",
    ]:
        os.makedirs(d, exist_ok=True)

# ─────────────────────────────────────────────────────────────────────────────
# COLOR PALETTE
# ─────────────────────────────────────────────────────────────────────────────

C = {
    # Skin
    'skin':      (240, 195, 140, 255),
    'skin_d':    (200, 150, 100, 255),
    'skin_dd':   (160, 110, 70,  255),
    # Hair
    'hair':      (25,  15,  10,  255),
    'hair_h':    (50,  35,  20,  255),
    # Eyes / features
    'eye':       (35,  25,  15,  255),
    'bindi':     (200, 50,  50,  255),
    'lip':       (200, 110, 90,  255),
    # Clothing – Tejimola (gold/white)
    'teji_top':  (248, 244, 228, 255),
    'teji_skirt':(240, 185, 35,  255),
    'teji_sk_d': (190, 140, 20,  255),
    'teji_border':(195, 45, 45,  255),
    # Clothing – Dom (blue/dhoti)
    'dom_dhoti': (235, 232, 218, 255),
    'dom_dhoti_d':(195,190,175, 255),
    'dom_sash':  (185, 40,  40,  255),
    'dom_sash_w':(245,240,230,  255),
    'dom_skin':  (180, 120, 75,  255),
    'dom_skin_d':(140, 90,  55,  255),
    # Clothing – Ranima (dark purple)
    'rani_top':  (90,  40,  110, 255),
    'rani_sk':   (70,  25,  90,  255),
    'rani_sk_d': (50,  15,  65,  255),
    'rani_acc':  (170, 90,  200, 255),
    # Corruption (boss)
    'boss':      (110, 0,   85,  255),
    'boss_d':    (70,  0,   55,  255),
    'boss_glow': (200, 80,  255, 255),
    # Spirit (translucent blue-white)
    'spirit':    (200, 220, 255, 180),
    'spirit_g':  (230, 245, 255, 140),
    # Props
    'wood':      (120, 75,  40,  255),
    'wood_d':    (85,  50,  25,  255),
    'wood_h':    (160, 110, 65,  255),
    'gold':      (245, 195, 40,  255),
    'gold_d':    (200, 150, 20,  255),
    'clay':      (175, 100, 55,  255),
    'clay_d':    (140, 75,  35,  255),
    'gamosa_r':  (195, 45,  45,  255),
    'gamosa_w':  (245, 240, 228, 255),
    'leaf':      (65,  145, 50,  255),
    'leaf_d':    (45,  105, 35,  255),
    'leaf_l':    (100, 190, 75,  255),
    'nahor_w':   (248, 242, 218, 255),
    'nahor_y':   (255, 195, 75,  255),
    'nahor_o':   (235, 150, 50,  255),
    'orb_p':     (160, 90,  240, 255),
    'orb_g':     (210, 180, 255, 255),
    'barrel_d':  (85,  50,  25,  255),
    'barrel_s':  (180, 30,  30,  255),
    'vine':      (55,  120, 40,  255),
    'spirit_pulse':(130, 180, 255, 220),
    # Outline
    'outline':   (20,  12,  8,   255),
    'T':         (0,   0,   0,   0),   # transparent
}

def px(img, x, y, col): img.putpixel((x, y), col)
def rect(draw, x1,y1,x2,y2, fill): draw.rectangle([x1,y1,x2,y2], fill=fill)
def ell(draw, x1,y1,x2,y2, fill): draw.ellipse([x1,y1,x2,y2], fill=fill)
def tri(draw, pts, fill): draw.polygon(pts, fill=fill)

# ─────────────────────────────────────────────────────────────────────────────
# CHARACTER SPRITES  (64 × 96 px per frame, 4 frames → 256 × 96 sheet)
# ─────────────────────────────────────────────────────────────────────────────

def draw_outline_ellipse(draw, x1,y1,x2,y2, fill, outline):
    draw.ellipse([x1,y1,x2,y2], fill=fill, outline=outline, width=1)

def draw_outline_rect(draw, x1,y1,x2,y2, fill, outline):
    draw.rectangle([x1,y1,x2,y2], fill=fill, outline=outline, width=1)

# ── Tejimola ─────────────────────────────────────────────────────────────────

def draw_tejimola_frame(img, ox, oy, walk_phase=0, crouching=False, hiding=False):
    """Draw Tejimola at offset (ox, oy). Returns drawn Image."""
    d = ImageDraw.Draw(img)
    O = C['outline']

    leg_offset = int(math.sin(walk_phase * math.pi) * 4) if walk_phase else 0
    body_y = oy + (12 if crouching else 0)

    if hiding:
        # Hiding: curl up, very small
        ell(d, ox+20, oy+70, ox+44, oy+90, C['teji_skirt'])
        ell(d, ox+22, oy+65, ox+42, oy+82, C['skin'])
        draw_outline_ellipse(d, ox+26, oy+60, ox+38, oy+72, C['skin'], O)  # head
        return

    # ── Hair ──
    ell(d, ox+18, body_y+2, ox+46, body_y+24, C['hair'])
    # braid
    for i in range(5):
        ell(d, ox+40+i, body_y+14+i*3, ox+44+i, body_y+18+i*3, C['hair'])

    # ── Head ──
    draw_outline_ellipse(d, ox+20, body_y+4, ox+44, body_y+26, C['skin'], O)

    # ── Face details ──
    # eyes
    ell(d, ox+25, body_y+11, ox+30, body_y+16, (255,255,255,255))
    ell(d, ox+34, body_y+11, ox+39, body_y+16, (255,255,255,255))
    ell(d, ox+26, body_y+12, ox+29, body_y+15, C['eye'])
    ell(d, ox+35, body_y+12, ox+38, body_y+15, C['eye'])
    # bindi
    ell(d, ox+31, body_y+9, ox+33, body_y+11, C['bindi'])
    # mouth
    d.arc([ox+27, body_y+18, ox+37, body_y+24], 0, 180, fill=C['lip'], width=1)

    # ── Neck / Collar ──
    rect(d, ox+28, body_y+25, ox+36, body_y+30, C['skin'])

    # ── Riha (top garment) ──
    rect(d, ox+20, body_y+30, ox+44, body_y+46, C['teji_top'])
    draw_outline_rect(d, ox+20, body_y+30, ox+44, body_y+46, C['teji_top'], O)
    # border trim
    d.line([ox+20, body_y+30, ox+44, body_y+30], fill=C['teji_border'], width=1)

    # ── Mekhela (skirt) ──
    if crouching:
        tri(d, [(ox+16, body_y+46),(ox+48, body_y+46),(ox+50, body_y+75),(ox+14, body_y+75)], C['teji_skirt'])
    else:
        tri(d, [(ox+16, body_y+46),(ox+48, body_y+46),(ox+52, body_y+82),(ox+12, body_y+82)], C['teji_skirt'])
    # skirt pattern / border
    d.line([ox+13, body_y+76, ox+51, body_y+76], fill=C['teji_border'], width=2)
    d.line([ox+14, body_y+79, ox+50, body_y+79], fill=C['nahor_y'], width=1)

    # ── Arms ──
    if walk_phase:
        arm_angle = math.sin(walk_phase * math.pi) * 15
        # Left arm
        ell(d, ox+14, body_y+32+int(arm_angle), ox+22, body_y+50+int(arm_angle), C['skin'])
        # Right arm
        ell(d, ox+42, body_y+32-int(arm_angle), ox+50, body_y+50-int(arm_angle), C['skin'])
    else:
        ell(d, ox+14, body_y+32, ox+22, body_y+50, C['skin'])
        ell(d, ox+42, body_y+32, ox+50, body_y+50, C['skin'])

    # ── Legs / Feet ──
    if not crouching:
        rect(d, ox+22, body_y+82, ox+30, body_y+92+leg_offset, C['skin'])
        rect(d, ox+34, body_y+82, ox+42, body_y+92-leg_offset, C['skin'])
        # feet
        ell(d, ox+20, body_y+90+leg_offset, ox+32, body_y+95+leg_offset, C['skin_d'])
        ell(d, ox+32, body_y+90-leg_offset, ox+44, body_y+95-leg_offset, C['skin_d'])

def make_tejimola_spritesheet(spirit=False):
    W, H = 256, 96
    img = Image.new('RGBA', (W, H), (0,0,0,0))
    phases = [0, 0.4, 0, 0.8]  # idle, walk1, idle, walk2
    for i, phase in enumerate(phases):
        draw_tejimola_frame(img, i*64, 0, walk_phase=phase)
    if spirit:
        # Tint blue-white and reduce alpha
        arr = np.array(img)
        mask = arr[:,:,3] > 0
        arr[mask, 0] = np.clip(arr[mask, 0].astype(int) * 0.6 + 100, 0, 255).astype(np.uint8)
        arr[mask, 1] = np.clip(arr[mask, 1].astype(int) * 0.6 + 120, 0, 255).astype(np.uint8)
        arr[mask, 2] = np.clip(arr[mask, 2].astype(int) * 0.5 + 180, 0, 255).astype(np.uint8)
        arr[mask, 3] = (arr[mask, 3] * 0.65).astype(np.uint8)
        img = Image.fromarray(arr)
    return img

# ── Dom ───────────────────────────────────────────────────────────────────────

def draw_dom_frame(img, ox, oy, walk_phase=0):
    d = ImageDraw.Draw(img)
    O = C['outline']
    leg_offset = int(math.sin(walk_phase * math.pi) * 4) if walk_phase else 0

    # ── Hair (short, slightly wavy) ──
    ell(d, ox+16, oy+2, ox+48, oy+22, C['hair'])

    # ── Head (adult man, slightly wider) ──
    draw_outline_ellipse(d, ox+18, oy+4, ox+46, oy+28, C['dom_skin'], O)

    # ── Stubble / jaw ──
    for i in range(4):
        ell(d, ox+23+i*4, oy+22, ox+25+i*4, oy+25, C['hair'])

    # ── Eyes ──
    ell(d, ox+23, oy+12, ox+29, oy+17, (255,255,255,255))
    ell(d, ox+35, oy+12, ox+41, oy+17, (255,255,255,255))
    ell(d, ox+24, oy+13, ox+28, oy+16, C['eye'])
    ell(d, ox+36, oy+13, ox+40, oy+16, C['eye'])

    # ── Gamosa around neck ──
    rect(d, ox+16, oy+27, ox+48, oy+32, C['gamosa_r'])
    for i in range(4):
        rect(d, ox+16+i*8, oy+27, ox+22+i*8, oy+32, C['gamosa_w'])

    # ── Upper body (kurta / vest) ──
    rect(d, ox+16, oy+32, ox+48, oy+54, C['dom_dhoti'])
    draw_outline_rect(d, ox+16, oy+32, ox+48, oy+54, C['dom_dhoti'], O)

    # ── Drum (dhol) — held in front ──
    ell(d, ox+18, oy+44, ox+46, oy+62, C['wood'])
    ell(d, ox+20, oy+45, ox+44, oy+55, C['wood_h'])
    # drum skin ends
    ell(d, ox+18, oy+44, ox+26, oy+62, C['gamosa_w'])
    ell(d, ox+38, oy+44, ox+46, oy+62, C['gamosa_w'])
    # drum strap
    rect(d, ox+28, oy+32, ox+36, oy+44, C['dom_sash'])

    # ── Dhoti (lower body) ──
    tri(d, [(ox+14, oy+62),(ox+50, oy+62),(ox+52, oy+88),(ox+12, oy+88)], C['dom_dhoti'])
    d.line([ox+12, oy+82, ox+52, oy+82], fill=C['gamosa_r'], width=2)

    # ── Arms ──
    arm_l = int(math.sin(walk_phase * math.pi) * 8) if walk_phase else 0
    ell(d, ox+8, oy+36+arm_l, ox+18, oy+54+arm_l, C['dom_skin'])
    ell(d, ox+46, oy+36-arm_l, ox+56, oy+54-arm_l, C['dom_skin'])

    # ── Legs ──
    rect(d, ox+20, oy+88, ox+29, oy+94+leg_offset, C['dom_skin'])
    rect(d, ox+35, oy+88, ox+44, oy+94-leg_offset, C['dom_skin'])

def make_dom_spritesheet():
    W, H = 256, 96
    img = Image.new('RGBA', (W, H), (0,0,0,0))
    for i, phase in enumerate([0, 0.4, 0, 0.8]):
        draw_dom_frame(img, i*64, 0, walk_phase=phase)
    return img

# ── Ranima ───────────────────────────────────────────────────────────────────

def draw_ranima_frame(img, ox, oy, walk_phase=0, corrupted=False):
    d = ImageDraw.Draw(img)
    O = C['outline']
    leg_offset = int(math.sin(walk_phase * math.pi) * 4) if walk_phase else 0

    skirt_c = C['boss'] if corrupted else C['rani_sk']
    top_c   = C['boss_d'] if corrupted else C['rani_top']
    skin_c  = (C['dom_skin_d'] if not corrupted else (140, 20, 110, 255))
    glow_c  = C['boss_glow'] if corrupted else C['rani_acc']

    # ── Hair (tight bun) ──
    ell(d, ox+17, oy+2, ox+47, oy+20, C['hair'])
    ell(d, ox+34, oy+2, ox+46, oy+12, C['hair'])  # bun

    # ── Head (angular) ──
    d.polygon([(ox+20, oy+6),(ox+44, oy+6),(ox+46, oy+26),(ox+18, oy+26)], fill=skin_c, outline=O)

    # ── Eyes (narrow, severe) ──
    d.line([ox+23, oy+13, ox+31, oy+13], fill=C['eye'], width=2)
    d.line([ox+33, oy+13, ox+41, oy+13], fill=C['eye'], width=2)
    if corrupted:
        d.line([ox+23, oy+13, ox+31, oy+13], fill=glow_c, width=1)
        d.line([ox+33, oy+13, ox+41, oy+13], fill=glow_c, width=1)

    # ── Frown ──
    d.arc([ox+27, oy+19, ox+37, oy+25], 180, 0, fill=C['outline'], width=1)

    # ── Neck ──
    rect(d, ox+27, oy+26, ox+37, oy+32, skin_c)

    # ── Top ──
    rect(d, ox+17, oy+32, ox+47, oy+52, top_c)
    if corrupted:
        for i in range(4):
            d.line([ox+17+i*8, oy+32, ox+17+i*8, oy+52], fill=glow_c, width=1)
    draw_outline_rect(d, ox+17, oy+32, ox+47, oy+52, top_c, O)

    # ── Skirt ──
    tri(d, [(ox+14, oy+52),(ox+50, oy+52),(ox+52, oy+86),(ox+12, oy+86)], skirt_c)
    d.line([ox+12, oy+79, ox+52, oy+79], fill=glow_c, width=2)

    # ── Arms ──
    arm_a = int(math.sin(walk_phase * math.pi) * 10) if walk_phase else 0
    ell(d, ox+9, oy+34+arm_a, ox+18, oy+54+arm_a, skin_c)
    ell(d, ox+46, oy+34-arm_a, ox+55, oy+54-arm_a, skin_c)

    # ── Corruption aura ──
    if corrupted:
        for radius in [2, 4]:
            for angle in range(0, 360, 45):
                ax = ox + 32 + int(math.cos(math.radians(angle)) * (20+radius))
                ay = oy + 44 + int(math.sin(math.radians(angle)) * (20+radius))
                if 0 <= ax < img.width and 0 <= ay < img.height:
                    img.putpixel((ax, ay), glow_c)

    # ── Legs ──
    rect(d, ox+21, oy+86, ox+30, oy+93+leg_offset, skin_c)
    rect(d, ox+34, oy+86, ox+43, oy+93-leg_offset, skin_c)

def make_ranima_spritesheet(corrupted=False):
    W, H = 256, 96
    img = Image.new('RGBA', (W, H), (0,0,0,0))
    for i, phase in enumerate([0, 0.4, 0, 0.8]):
        draw_ranima_frame(img, i*64, 0, walk_phase=phase, corrupted=corrupted)
    return img

# ── Father ───────────────────────────────────────────────────────────────────

def make_father_portrait():
    img = Image.new('RGBA', (128, 128), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # background oval
    ell(d, 10, 10, 118, 118, (200, 170, 120, 60))
    # hair
    ell(d, 24, 14, 104, 58, C['hair'])
    # head
    draw_outline_ellipse(d, 26, 18, 102, 72, C['skin'], C['outline'])
    # moustache
    for x in range(38, 58): d.line([x, 52, x, 55], fill=C['hair'], width=1)
    for x in range(70, 90): d.line([x, 52, x, 55], fill=C['hair'], width=1)
    # eyes
    ell(d, 36, 36, 50, 48, (255,255,255,255))
    ell(d, 78, 36, 92, 48, (255,255,255,255))
    ell(d, 38, 38, 48, 46, C['eye'])
    ell(d, 80, 38, 90, 46, C['eye'])
    # warm smile
    d.arc([44, 55, 84, 68], 0, 180, fill=C['lip'], width=2)
    # body (dhoti)
    rect(d, 30, 72, 98, 110, C['dom_dhoti'])
    rect(d, 30, 72, 98, 78, C['gamosa_r'])
    # arms
    ell(d, 12, 72, 32, 108, C['skin'])
    ell(d, 96, 72, 116, 108, C['skin'])
    return img

# ── Portraits ─────────────────────────────────────────────────────────────────

def make_portrait(char='tejimola'):
    img = Image.new('RGBA', (128, 128), (0,0,0,0))
    d = ImageDraw.Draw(img)

    bgs = {
        'tejimola': (240, 210, 130, 80),
        'dom':      (80,  120, 180, 80),
        'ranima':   (80,  20,  90,  80),
        'ranima_c': (50,  0,   70,  80),
    }
    ell(d, 5, 5, 123, 123, bgs.get(char, (100,100,100,80)))

    if char == 'tejimola':
        ell(d, 24, 14, 104, 50, C['hair'])
        draw_outline_ellipse(d, 26, 18, 102, 72, C['skin'], C['outline'])
        ell(d, 34, 34, 46, 46, (255,255,255,255)); ell(d, 36, 36, 44, 44, C['eye'])
        ell(d, 82, 34, 94, 46, (255,255,255,255)); ell(d, 84, 36, 92, 44, C['eye'])
        ell(d, 63, 28, 66, 31, C['bindi'])
        d.arc([44, 54, 80, 66], 0, 180, fill=C['lip'], width=2)
        rect(d, 34, 72, 94, 110, C['teji_top'])
        rect(d, 34, 72, 94, 76, C['teji_border'])

    elif char == 'dom':
        ell(d, 22, 14, 106, 50, C['hair'])
        draw_outline_ellipse(d, 24, 18, 104, 72, C['dom_skin'], C['outline'])
        ell(d, 34, 34, 46, 46, (255,255,255,255)); ell(d, 36, 36, 44, 44, C['eye'])
        ell(d, 82, 34, 94, 46, (255,255,255,255)); ell(d, 84, 36, 92, 44, C['eye'])
        for i in range(5): ell(d, 36+i*8, 54, 40+i*8, 58, C['hair'])
        rect(d, 26, 72, 102, 80, C['gamosa_r'])
        for i in range(4): rect(d, 26+i*18, 72, 34+i*18, 80, C['gamosa_w'])
        rect(d, 26, 80, 102, 116, C['dom_dhoti'])

    elif char in ('ranima', 'ranima_c'):
        corrupted = char == 'ranima_c'
        sc = (140,20,110,255) if corrupted else C['dom_skin_d']
        ell(d, 22, 12, 106, 50, C['hair'])
        ell(d, 72, 12, 106, 34, C['hair'])
        d.polygon([(26,18),(102,18),(104,72),(24,72)], fill=sc, outline=C['outline'])
        d.line([36,38,52,38], fill=C['eye'], width=2)
        d.line([76,38,92,38], fill=C['eye'], width=2)
        if corrupted:
            d.line([36,38,52,38], fill=C['boss_glow'], width=1)
            d.line([76,38,92,38], fill=C['boss_glow'], width=1)
        d.arc([44,56,82,68], 180, 0, fill=C['outline'], width=2)
        top_c = C['boss_d'] if corrupted else C['rani_top']
        sk_c  = C['boss']   if corrupted else C['rani_sk']
        rect(d, 26, 72, 102, 96, top_c)
        rect(d, 26, 96, 102, 120, sk_c)
        if corrupted:
            for ang in range(0,360,30):
                ax = 64 + int(math.cos(math.radians(ang))*55)
                ay = 64 + int(math.sin(math.radians(ang))*55)
                if 0<=ax<128 and 0<=ay<128: img.putpixel((ax,ay), C['boss_glow'])

    return img

# ─────────────────────────────────────────────────────────────────────────────
# PROPS
# ─────────────────────────────────────────────────────────────────────────────

def make_nahor_flower():
    img = Image.new('RGBA', (128, 128), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # trunk
    tri(d, [(56,128),(72,128),(68,60),(60,60)], C['wood_d'])
    # branches
    d.line([64,60,40,35], fill=C['wood'], width=3)
    d.line([64,60,88,35], fill=C['wood'], width=3)
    d.line([64,80,30,50], fill=C['wood'], width=2)
    d.line([64,80,98,50], fill=C['wood'], width=2)
    # leaf clusters
    for cx,cy,r in [(40,30,18),(88,30,18),(25,46,14),(103,46,14),(64,20,20)]:
        ell(d, cx-r, cy-r, cx+r, cy+r, C['leaf'])
    for cx,cy,r in [(40,30,10),(88,30,10),(25,46,8),(103,46,8),(64,20,12)]:
        ell(d, cx-r, cy-r, cx+r, cy+r, C['leaf_l'])
    # nahor flowers (white/cream)
    for fx,fy in [(38,22),(52,28),(76,22),(90,28),(62,12),(36,38),(92,38)]:
        ell(d, fx-6, fy-6, fx+6, fy+6, C['nahor_w'])
        ell(d, fx-3, fy-3, fx+3, fy+3, C['nahor_y'])
    return img

def make_dheki():
    img = Image.new('RGBA', (128, 64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Base/trough
    rect(d, 10, 40, 118, 64, C['wood_d'])
    rect(d, 12, 42, 116, 62, C['wood'])
    # Beam (horizontal lever)
    rect(d, 8, 28, 120, 36, C['wood_h'])
    # Pivot post
    rect(d, 58, 20, 70, 64, C['wood_d'])
    ell(d, 56, 18, 72, 30, C['wood'])
    # Foot pedal end
    ell(d, 8, 24, 24, 40, C['wood_d'])
    # Pestle end (goes into mortar)
    rect(d, 102, 36, 114, 60, C['wood_d'])
    ell(d, 100, 56, 116, 64, C['clay'])
    # Rice/grain
    for i in range(6):
        ell(d, 30+i*10, 46, 36+i*10, 52, C['nahor_w'])
    return img

def make_hairpin():
    img = Image.new('RGBA', (64, 64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Pin shaft
    d.line([10, 54, 54, 10], fill=C['gold'], width=3)
    d.line([10, 54, 54, 10], fill=C['gold_d'], width=1)
    # Flower top
    for ang in range(0, 360, 60):
        px2 = int(54 + math.cos(math.radians(ang)) * 8)
        py2 = int(10 + math.sin(math.radians(ang)) * 8)
        ell(d, px2-5, py2-5, px2+5, py2+5, C['gold'])
    ell(d, 48, 4, 60, 16, C['nahor_y'])
    # Sparkles
    for sx, sy in [(20,44),(32,32),(44,20)]:
        ell(d, sx-2, sy-2, sx+2, sy+2, (255,240,180,255))
    return img

def make_pot():
    img = Image.new('RGBA', (64, 64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Clay pot body
    ell(d, 8, 20, 56, 60, C['clay'])
    ell(d, 12, 24, 52, 56, C['clay_d'])
    # Rim / opening
    ell(d, 16, 16, 48, 28, C['clay_d'])
    ell(d, 18, 18, 46, 26, C['wood_h'])
    # Handle
    d.arc([4, 28, 18, 48], 90, 270, fill=C['clay_d'], width=3)
    d.arc([46, 28, 60, 48], 270, 90, fill=C['clay_d'], width=3)
    # Decoration line
    d.arc([14, 36, 50, 52], 0, 180, fill=C['gamosa_r'], width=2)
    return img

def make_gamosa():
    """Assamese traditional cloth — white with red border pattern."""
    img = Image.new('RGBA', (96, 64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Main cloth
    rect(d, 4, 16, 92, 48, C['gamosa_w'])
    # Red borders
    rect(d, 4, 16, 92, 22, C['gamosa_r'])
    rect(d, 4, 42, 92, 48, C['gamosa_r'])
    # Woven pattern in red
    for i in range(8):
        rect(d, 12+i*10, 24, 18+i*10, 40, C['gamosa_r'])
    # Fringe
    for i in range(0, 88, 6):
        d.line([i+4, 16, i+2, 8], fill=C['gamosa_w'], width=1)
        d.line([i+4, 48, i+2, 56], fill=C['gamosa_w'], width=1)
    return img

def make_spirit_orb():
    img = Image.new('RGBA', (64, 64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Outer glow
    for r, alpha in [(32,40),(28,80),(24,140),(20,200)]:
        col = (160, 90, 240, alpha)
        ell(d, 32-r, 32-r, 32+r, 32+r, col)
    # Core
    ell(d, 22, 22, 42, 42, C['orb_p'])
    ell(d, 26, 26, 38, 38, C['orb_g'])
    ell(d, 29, 27, 35, 33, (240, 220, 255, 255))
    # Sparkle trails
    for ang in range(0, 360, 45):
        ex = 32 + int(math.cos(math.radians(ang)) * 28)
        ey = 32 + int(math.sin(math.radians(ang)) * 28)
        d.line([32,32,ex,ey], fill=(200,160,255,60), width=1)
    return img

def make_spiked_barrel():
    img = Image.new('RGBA', (64, 64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Barrel body
    ell(d, 8, 8, 56, 56, C['barrel_d'])
    ell(d, 10, 10, 54, 54, C['wood'])
    # Metal bands
    rect(d, 8, 22, 56, 28, (80,80,80,255))
    rect(d, 8, 36, 56, 42, (80,80,80,255))
    # Wood grain
    for i in range(4):
        d.arc([12+i*2, 14-i, 52-i*2, 50+i], 60, 120, fill=C['wood_d'], width=1)
    # Spikes
    for ang in range(0, 360, 40):
        sx = 32 + int(math.cos(math.radians(ang)) * 28)
        sy = 32 + int(math.sin(math.radians(ang)) * 28)
        ex = 32 + int(math.cos(math.radians(ang)) * 38)
        ey = 32 + int(math.sin(math.radians(ang)) * 38)
        d.line([sx,sy,ex,ey], fill=C['barrel_s'], width=2)
    return img

def make_dhol_drum():
    img = Image.new('RGBA', (64, 64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Drum cylinder (horizontal)
    rect(d, 6, 18, 58, 46, C['wood'])
    # Drum ends
    ell(d, 4, 14, 22, 50, C['dom_dhoti'])
    ell(d, 42, 14, 60, 50, C['dom_dhoti'])
    d.ellipse([4,14,22,50], outline=C['outline'], width=1)
    d.ellipse([42,14,60,50], outline=C['outline'], width=1)
    # Lacing
    for y in range(20, 44, 4):
        d.line([22, y, 42, y+2 if y%8==0 else y-2], fill=C['gamosa_r'], width=1)
    # Band
    rect(d, 6, 29, 58, 33, (80,60,30,255))
    # Drum stick
    d.line([50, 8, 62, 48], fill=C['wood_h'], width=2)
    ell(d, 60, 44, 64, 52, C['wood_d'])
    return img

def make_gourd():
    img = Image.new('RGBA', (64, 64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Gourd lower body
    ell(d, 12, 28, 52, 60, C['leaf_l'])
    ell(d, 14, 30, 50, 58, C['leaf'])
    # Gourd upper body (smaller)
    ell(d, 20, 10, 44, 36, C['leaf_l'])
    ell(d, 22, 12, 42, 34, C['leaf'])
    # Neck
    rect(d, 28, 4, 36, 14, (80,120,50,255))
    # Stem
    d.line([32, 4, 32, 0], fill=C['wood'], width=2)
    # Highlight
    ell(d, 24, 14, 32, 22, (150,210,110,200))
    ell(d, 18, 36, 26, 46, (150,210,110,200))
    return img

def make_footprint():
    img = Image.new('RGBA', (32, 48), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # Heel
    ell(d, 8, 30, 24, 44, (120,80,50,160))
    # Ball
    ell(d, 8, 16, 24, 32, (120,80,50,140))
    # Toes
    for i, (tx, ty) in enumerate([(9,10),(13,8),(17,8),(21,10),(24,13)]):
        r = 4-i//2
        ell(d, tx-r, ty-r, tx+r, ty+r, (120,80,50,120))
    return img

def make_spirit_pulse_ring():
    img = Image.new('RGBA', (128, 128), (0,0,0,0))
    d = ImageDraw.Draw(img)
    for r, a in [(60,255),(54,200),(48,150),(42,100),(36,60)]:
        ell(d, 64-r, 64-r, 64+r, 64+r, (130,180,255,a))
    ell(d, 30, 30, 98, 98, (0,0,0,0))  # hollow center
    for ang in range(0, 360, 15):
        ex = 64 + int(math.cos(math.radians(ang))*58)
        ey = 64 + int(math.sin(math.radians(ang))*58)
        img.putpixel((ex,ey), (200,230,255,255))
    return img

def make_vine_obstacle():
    img = Image.new('RGBA', (32, 96), (0,0,0,0))
    d = ImageDraw.Draw(img)
    for y in range(0, 96, 6):
        xoff = int(math.sin(y/10)*4)
        rect(d, 12+xoff, y, 20+xoff, y+8, C['vine'])
        if y % 18 == 0:
            ell(d, 4+xoff, y-2, 16+xoff, y+10, C['leaf'])
        if y % 18 == 9:
            ell(d, 16+xoff, y-2, 28+xoff, y+10, C['leaf_d'])
    return img

# ─────────────────────────────────────────────────────────────────────────────
# BACKGROUNDS  (1920 × 1080 per layer)
# ─────────────────────────────────────────────────────────────────────────────

def gradient(img, top_col, bot_col):
    """Fill image with vertical gradient."""
    arr = np.zeros((img.height, img.width, 4), dtype=np.uint8)
    for y in range(img.height):
        t = y / (img.height - 1)
        for c in range(4):
            arr[y, :, c] = int(top_col[c] + (bot_col[c] - top_col[c]) * t)
    return Image.fromarray(arr)

def draw_sun(draw, cx, cy, r, color):
    for radius, alpha in [(r+20, 40),(r+12, 80),(r+6, 140),(r, 255)]:
        c = (*color[:3], alpha)
        draw.ellipse([cx-radius, cy-radius, cx+radius, cy+radius], fill=c)

def draw_mountains(draw, w, h, num, color, seed=42):
    rng = random.Random(seed)
    pts = [(0, h)]
    x = 0
    while x < w:
        x += rng.randint(80, 200)
        y = h - rng.randint(h//3, h*3//4)
        pts.append((x, y))
    pts.append((w, h))
    draw.polygon(pts, fill=color)

def draw_trees(draw, w, h, num, trunk_col, leaf_col, seed=0):
    rng = random.Random(seed)
    for _ in range(num):
        x = rng.randint(0, w)
        trunk_h = rng.randint(40, 80)
        tree_w = rng.randint(20, 45)
        draw.rectangle([x-4, h-trunk_h, x+4, h], fill=trunk_col)
        draw.ellipse([x-tree_w, h-trunk_h-tree_w, x+tree_w, h-trunk_h+tree_w//2], fill=leaf_col)

def draw_buildings(draw, w, h, num, color, seed=0):
    rng = random.Random(seed)
    for _ in range(num):
        bx = rng.randint(0, w-60)
        bw = rng.randint(30, 80)
        bh = rng.randint(50, 120)
        draw.rectangle([bx, h-bh, bx+bw, h], fill=color)
        # Roof
        draw.polygon([(bx-4, h-bh),(bx+bw+4, h-bh),(bx+bw//2, h-bh-30)], fill=(*color[:3], color[3]-30))

def make_background_act1():
    """Act 1 – Happy Home: warm golden dawn, lush green, homely."""
    W, H = 1920, 1080

    # Layer 4 – Sky (dawn gold gradient)
    sky = gradient(Image.new('RGBA',(W,H)), (255,200,120,255), (255,160,60,255))
    d = ImageDraw.Draw(sky)
    draw_sun(d, 300, 200, 60, (255,240,180,255))
    # Clouds
    for cx, cy in [(500,150),(900,100),(1400,180),(1700,130)]:
        for dx, dy, r in [(-30,0,40),(0,-15,50),(30,0,40),(60,5,35)]:
            d.ellipse([cx+dx-r, cy+dy-r, cx+dx+r, cy+dy+r], fill=(255,240,220,180))
    sky.save(f"{ART}/Backgrounds/Act1/layer4_sky.png")

    # Layer 3 – Distant hills + village silhouette
    bg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(bg)
    draw_mountains(d, W, H, 6, (180,140,90,200), seed=1)
    draw_buildings(d, W, 880, 8, (150,110,70,220), seed=2)
    bg.save(f"{ART}/Backgrounds/Act1/layer3_background.png")

    # Layer 2 – Midground: trees and courtyard wall
    mg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(mg)
    # Courtyard ground
    rect(d, 0, 900, W, H, (170, 130, 80, 255))
    draw_trees(d, W, 900, 14, (100,65,30,255), (65,145,50,255), seed=3)
    # Nahor tree (larger, central feature)
    d.rectangle([900,750,930,900], fill=(100,65,30,255))
    d.ellipse([820,640,1010,820], fill=(65,145,50,255))
    d.ellipse([840,650,990,800], fill=(90,170,65,255))
    # Flowers on nahor
    for fx, fy in [(860,660),(900,650),(950,660),(880,690),(930,685)]:
        d.ellipse([fx-8,fy-8,fx+8,fy+8], fill=(248,242,218,255))
        d.ellipse([fx-4,fy-4,fx+4,fy+4], fill=(255,195,75,255))
    mg.save(f"{ART}/Backgrounds/Act1/layer2_midground.png")

    # Layer 1 – Foreground: flowers, path
    fg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(fg)
    # Dirt path
    d.polygon([(760,1080),(1160,1080),(1100,900),(820,900)], fill=(150,110,70,180))
    # Foreground flowers
    rng = random.Random(7)
    for _ in range(40):
        fx = rng.randint(0,W); fy = rng.randint(920, 1060)
        fc = rng.choice([(240,185,35,255),(200,80,80,255),(255,255,180,255)])
        d.ellipse([fx-6,fy-6,fx+6,fy+6], fill=fc)
        d.line([fx,fy,fx,fy+20], fill=(65,145,50,255), width=2)
    fg.save(f"{ART}/Backgrounds/Act1/layer1_foreground.png")

def make_background_act2():
    """Act 2 – Descent: dark, desaturated, oppressive."""
    W, H = 1920, 1080

    # Sky – dark stormy
    sky = gradient(Image.new('RGBA',(W,H)), (50,55,70,255), (30,35,50,255))
    d = ImageDraw.Draw(sky)
    # Clouds (heavy, dark)
    for cx, cy in [(300,120),(700,80),(1200,150),(1700,100)]:
        for dx, dy, r in [(-40,0,55),(0,-20,65),(40,0,55),(80,10,45)]:
            d.ellipse([cx+dx-r, cy+dy-r, cx+dx+r, cy+dy+r], fill=(40,42,55,200))
    sky.save(f"{ART}/Backgrounds/Act2/layer4_sky.png")

    # Background – dark hills
    bg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(bg)
    draw_mountains(d, W, H, 5, (60,55,70,200), seed=10)
    draw_buildings(d, W, 880, 6, (50,45,60,220), seed=11)
    bg.save(f"{ART}/Backgrounds/Act2/layer3_background.png")

    # Midground – bare trees, dark house
    mg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(mg)
    rect(d, 0, 880, W, H, (80, 70, 55, 255))
    draw_trees(d, W, 880, 10, (50,35,20,255), (40,55,35,255), seed=12)
    # Dark house
    d.rectangle([800,700,1100,880], fill=(55,45,55,255))
    d.polygon([(790,700),(1110,700),(950,600)], fill=(45,35,45,255))
    # Window (lit)
    d.rectangle([840,740,880,780], fill=(200,160,80,200))
    d.rectangle([1000,740,1040,780], fill=(200,160,80,200))
    mg.save(f"{ART}/Backgrounds/Act2/layer2_midground.png")

    # Foreground
    fg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(fg)
    # Dead leaves
    rng = random.Random(13)
    for _ in range(30):
        fx = rng.randint(0,W); fy = rng.randint(900,1060)
        d.ellipse([fx-4,fy-3,fx+4,fy+3], fill=(80,60,40,200))
    fg.save(f"{ART}/Backgrounds/Act2/layer1_foreground.png")

def make_background_act3():
    """Act 3 – Spirit World: twilight purple, mystical."""
    W, H = 1920, 1080

    # Sky – deep twilight
    sky = gradient(Image.new('RGBA',(W,H)), (60,20,100,255), (20,10,60,255))
    d = ImageDraw.Draw(sky)
    # Moon
    draw_sun(d, 1600, 150, 45, (220,220,255,255))
    d.ellipse([1620,120,1660,160], fill=(60,20,100,255))  # crescent shadow
    # Stars
    rng = random.Random(20)
    for _ in range(120):
        sx = rng.randint(0,W); sy = rng.randint(0,400)
        sa = rng.randint(100,255)
        d.ellipse([sx-1,sy-1,sx+1,sy+1], fill=(220,220,255,sa))
    sky.save(f"{ART}/Backgrounds/Act3/layer4_sky.png")

    # Background – ruined estate silhouettes
    bg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(bg)
    draw_mountains(d, W, H, 4, (40,20,60,180), seed=20)
    # Ruined walls
    for bx, bh in [(200,180),(500,120),(800,200),(1400,160),(1700,140)]:
        d.rectangle([bx,H-bh,bx+60,H], fill=(50,25,70,220))
        # broken top
        d.polygon([(bx,H-bh),(bx+20,H-bh-20),(bx+40,H-bh),(bx+60,H-bh-10),(bx+60,H-bh)], fill=(50,25,70,220))
    bg.save(f"{ART}/Backgrounds/Act3/layer3_background.png")

    # Midground – spirit glows, nahor tree (ethereal)
    mg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(mg)
    rect(d, 0, 880, W, H, (40,30,55,255))
    # Glowing nahor tree
    for r, a in [(100,30),(80,60),(60,100),(40,160)]:
        d.ellipse([880-r,640-r,880+r,640+r], fill=(150,100,255,a))
    d.rectangle([874,740,886,880], fill=(70,50,90,255))
    d.ellipse([820,600,940,740], fill=(100,60,180,180))
    # Spirit wisps
    for wx, wy in [(400,800),(700,750),(1200,820),(1600,770)]:
        d.ellipse([wx-20,wy-20,wx+20,wy+20], fill=(150,180,255,80))
    mg.save(f"{ART}/Backgrounds/Act3/layer2_midground.png")

    # Foreground
    fg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(fg)
    rng = random.Random(21)
    for _ in range(20):
        fx = rng.randint(0,W); fy = rng.randint(920,1060)
        d.ellipse([fx-8,fy-8,fx+8,fy+8], fill=(100,80,140,150))
    fg.save(f"{ART}/Backgrounds/Act3/layer1_foreground.png")

def make_background_act4():
    """Act 4 – Boss Fight: surreal corruption, twisted reality."""
    W, H = 1920, 1080

    # Sky – blood red/dark magenta
    sky = gradient(Image.new('RGBA',(W,H)), (100,0,70,255), (50,0,40,255))
    d = ImageDraw.Draw(sky)
    # Corruption tendrils
    rng = random.Random(30)
    for _ in range(8):
        x1,y1 = rng.randint(0,W), rng.randint(200,600)
        for i in range(30):
            x2 = x1 + rng.randint(-20,20)
            y2 = y1 + rng.randint(10,30)
            d.line([x1,y1,x2,y2], fill=(150,0,120,100), width=2)
            x1,y1 = x2,y2
    sky.save(f"{ART}/Backgrounds/Act4/layer4_sky.png")

    # Background – twisted household
    bg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(bg)
    # Warped floor
    pts = [(0,H-200)]
    for x in range(0, W+100, 100):
        y = H-200 + int(math.sin(x/200)*40) + rng.randint(-10,10)
        pts.append((x,y))
    pts.append((W,H)); pts.append((0,H))
    d.polygon(pts, fill=(70,0,55,220))
    # Twisted pillars
    for px_pos in [200,600,1000,1400,1800]:
        d.rectangle([px_pos-15, H-400, px_pos+15, H-200], fill=(80,0,60,255))
        d.ellipse([px_pos-25, H-410, px_pos+25, H-390], fill=(100,0,80,255))
    bg.save(f"{ART}/Backgrounds/Act4/layer3_background.png")

    mg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(mg)
    rect(d, 0, 880, W, H, (60, 0, 45, 255))
    mg.save(f"{ART}/Backgrounds/Act4/layer2_midground.png")

    fg = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(fg)
    rng2 = random.Random(31)
    for _ in range(25):
        fx = rng2.randint(0,W); fy = rng2.randint(900,1060)
        d.ellipse([fx-5,fy-5,fx+5,fy+5], fill=(180,0,140,120))
    fg.save(f"{ART}/Backgrounds/Act4/layer1_foreground.png")

# ─────────────────────────────────────────────────────────────────────────────
# UI ASSETS
# ─────────────────────────────────────────────────────────────────────────────

def make_menu_background():
    W, H = 1920, 1080
    img = gradient(Image.new('RGBA',(W,H)), (30,15,8,255), (12,6,3,255))
    d = ImageDraw.Draw(img)
    # Nahor tree (large, right side)
    # Trunk
    for i in range(8):
        d.rectangle([1500+i*2, 400+i*5, 1510+i*2, 1080], fill=(80+i*3,50+i*2,25+i,255))
    # Canopy branches
    for bx,by,br,bl,bd in [
        (1504,400,120,16,4),(1504,500,100,14,3),(1504,350,90,12,3),
        (1400,380,80,12,3),(1620,400,80,12,3),
    ]:
        ell(d, bx-br,by-br,bx+br,by+br, (50,120,40,220))
        ell(d, bx-br+10,by-br+10,bx+br-10,by+br-10, (70,155,55,230))
    # Nahor flowers
    for fx,fy in [(1420,340),(1460,310),(1500,290),(1540,310),(1580,340),
                   (1400,390),(1610,380),(1440,420),(1560,415)]:
        d.ellipse([fx-12,fy-12,fx+12,fy+12], fill=(248,242,218,230))
        d.ellipse([fx-6,fy-6,fx+6,fy+6], fill=(255,195,75,255))

    # Decorative border (left panel area)
    d.rectangle([80, 80, 600, 1000], fill=(20,10,5,180))
    d.rectangle([80, 80, 600, 1000], outline=(180,140,50,200), width=3)
    # Corner ornaments
    for cx, cy in [(80,80),(600,80),(80,1000),(600,1000)]:
        d.ellipse([cx-10,cy-10,cx+10,cy+10], fill=(200,160,50,255))

    # River at bottom
    for y in range(1040, 1080):
        t = (y-1040)/40
        col = (int(40+t*20), int(60+t*30), int(120+t*40), int(180+t*75))
        d.line([0,y,W,y], fill=col)

    img.save(f"{ART}/UI/Menu/menu_background.png")

def make_button(label, filename, width=220, height=55,
                bg=(40,20,10,230), border=(180,140,50,255)):
    img = Image.new('RGBA',(width,height),(0,0,0,0))
    d = ImageDraw.Draw(img)
    # Rounded rectangle (simulated)
    r = 8
    d.rectangle([r,0,width-r,height], fill=bg)
    d.rectangle([0,r,width,height-r], fill=bg)
    for cx,cy in [(r,r),(width-r,r),(r,height-r),(width-r,height-r)]:
        d.ellipse([cx-r,cy-r,cx+r,cy+r], fill=bg)
    # Border
    d.rectangle([r,0,width-r,height], outline=border, width=2)
    d.rectangle([0,r,width,height-r], outline=border, width=2)
    # Highlight top
    d.line([r,1,width-r,1], fill=(*border[:3],120), width=1)
    img.save(f"{ART}/UI/Menu/{filename}")

def make_dialogue_box():
    W, H = 1400, 220
    img = Image.new('RGBA',(W,H),(0,0,0,0))
    d = ImageDraw.Draw(img)
    # Main panel
    d.rectangle([4,4,W-4,H-4], fill=(12,6,3,235))
    # Ornate border
    d.rectangle([4,4,W-4,H-4], outline=(180,140,50,255), width=3)
    d.rectangle([8,8,W-8,H-8], outline=(120,90,30,180), width=1)
    # Corner ornaments
    for cx,cy in [(20,20),(W-20,20),(20,H-20),(W-20,H-20)]:
        d.ellipse([cx-10,cy-10,cx+10,cy+10], fill=(180,140,50,255))
        d.ellipse([cx-5,cy-5,cx+5,cy+5], fill=(220,180,80,255))
    # Portrait frame area
    d.rectangle([12,12,196,H-12], fill=(20,12,6,255))
    d.rectangle([12,12,196,H-12], outline=(180,140,50,255), width=2)
    # Nahor motif at corners
    for fx,fy in [(186,12),(186,H-28)]:
        d.ellipse([fx-6,fy,fx+6,fy+12], fill=(248,242,218,200))
    img.save(f"{ART}/UI/DialogueBox/dialogue_box.png")

def make_hud_icon(filename, color, symbol='•'):
    img = Image.new('RGBA',(32,32),(0,0,0,0))
    d = ImageDraw.Draw(img)
    d.ellipse([2,2,30,30], fill=color)
    d.ellipse([5,5,27,27], fill=(*color[:3], color[3]-50 if len(color)>3 and color[3]>50 else 50))
    img.save(f"{ART}/UI/HUD/{filename}")

# ─────────────────────────────────────────────────────────────────────────────
# AUDIO GENERATION
# ─────────────────────────────────────────────────────────────────────────────

SAMPLE_RATE = 44100

def save_wav(filename, samples, rate=SAMPLE_RATE):
    """Save numpy float array [-1,1] as 16-bit WAV."""
    samples = np.clip(samples, -1.0, 1.0)
    data = (samples * 32767).astype(np.int16)
    with wave.open(filename, 'w') as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(rate)
        wf.writeframes(data.tobytes())

def tone(freq, dur, rate=SAMPLE_RATE, vol=0.3, env='harp'):
    """Generate a single tone with envelope."""
    t = np.linspace(0, dur, int(rate*dur), False)
    wave_data = np.sin(2*np.pi*freq*t)
    # Add harmonics for richness
    wave_data += 0.3 * np.sin(4*np.pi*freq*t)
    wave_data += 0.15 * np.sin(6*np.pi*freq*t)
    wave_data += 0.08 * np.sin(8*np.pi*freq*t)
    # Envelope
    attack = int(rate*0.02)
    release = int(rate*min(0.3, dur*0.4))
    env_arr = np.ones(len(t))
    env_arr[:attack] = np.linspace(0, 1, attack)
    env_arr[-release:] = np.linspace(1, 0, release)
    return wave_data * env_arr * vol

def make_melody(notes_hz, durations, rate=SAMPLE_RATE, vol=0.3):
    """Compose a melody from note frequencies and durations."""
    samples = []
    for freq, dur in zip(notes_hz, durations):
        if freq == 0:
            samples.append(np.zeros(int(rate*dur)))
        else:
            samples.append(tone(freq, dur, rate, vol))
    return np.concatenate(samples)

# Assamese Borgeet-inspired pentatonic scale notes (Bhairav raga flavor)
# Using Sa Re Ga Pa Dha (pentatonic) centered around D
NOTES = {
    'Sa': 293.66,  # D4
    'Re': 329.63,  # E4
    'Ga': 369.99,  # F#4
    'Pa': 440.00,  # A4
    'Dha': 493.88, # B4
    'Ni': 523.25,  # C5
    'Sa2': 587.33, # D5
    'Re2': 659.25, # E5
    'Ga_l': 146.83,# D3 low
    'Pa_l': 220.00,# A3 low
    'Sa_l': 196.00,# G3
    'R':    0,     # rest
}

def make_act1_music():
    """Happy home theme — gentle, warm, folksy."""
    N = NOTES
    melody = [
        N['Sa'],N['Re'],N['Ga'],N['Pa'],N['Dha'],N['Pa'],N['Ga'],N['Re'],
        N['Sa'],N['Ga'],N['Pa'],N['Sa2'],N['Dha'],N['Pa'],N['Ga'],N['Sa'],
        N['Re'],N['Ga'],N['Pa'],N['Dha'],N['Sa2'],N['Dha'],N['Pa'],N['Ga'],
        N['Re'],N['Sa'],N['Re'],N['Ga'],N['Pa'],N['Sa'],N['R'],N['Sa'],
    ]
    durs = [0.25]*32
    base = make_melody(melody, durs, vol=0.35)
    # Add a simple bass line
    bass_notes = [N['Ga_l'],N['Ga_l'],N['Pa_l'],N['Pa_l'],N['Sa_l'],N['Sa_l'],N['Ga_l'],N['Ga_l']]
    bass_durs  = [1.0]*8
    bass = make_melody(bass_notes, bass_durs, vol=0.15)
    # Loop to match length
    loops = math.ceil(len(base)/len(bass))
    bass_full = np.tile(bass, loops)[:len(base)]
    combined = base + bass_full
    # Loop the whole thing 4x for a longer track
    track = np.tile(combined, 4)
    # Fade out last 2 seconds
    fade = min(SAMPLE_RATE*2, len(track))
    track[-fade:] *= np.linspace(1,0,fade)
    save_wav(f"{AUD}/Music/act1_theme.wav", track)

def make_act2_descent_music():
    """Descent theme — slow, minor, oppressive."""
    N = NOTES
    melody = [
        N['Sa'],N['R'],N['Re'],N['R'],N['Ga'],N['Re'],N['Sa'],N['R'],
        N['Sa'],N['R'],N['Sa'],N['R'],N['Re'],N['Sa'],N['Ga_l'],N['R'],
    ]
    durs = [0.5,0.25,0.5,0.25,0.5,0.25,0.75,0.5,
            0.5,0.25,0.5,0.25,0.5,0.5, 0.75,0.5]
    base = make_melody(melody, durs, vol=0.25)
    # Dark drone
    t = np.linspace(0, len(base)/SAMPLE_RATE, len(base))
    drone = 0.12 * np.sin(2*np.pi*N['Ga_l']*t)
    # Slow tremolo on drone
    tremolo = 1 + 0.3*np.sin(2*np.pi*3*t)
    combined = base + drone*tremolo
    track = np.tile(combined, 4)
    track[-SAMPLE_RATE*2:] *= np.linspace(1,0,SAMPLE_RATE*2)
    save_wav(f"{AUD}/Music/act2_descent.wav", track)

def make_act2_dheki_music():
    """Dheki rhythm theme — driving, repetitive, intensifying."""
    N = NOTES
    # Fast rhythmic pattern
    melody = [
        N['Sa'],N['Ga'],N['Pa'],N['Ga'],N['Sa'],N['Re'],N['Ga'],N['Re'],
        N['Sa'],N['Ga'],N['Pa'],N['Dha'],N['Pa'],N['Ga'],N['Pa'],N['Ga'],
    ]
    durs = [0.2]*16
    base = make_melody(melody, durs, vol=0.3)
    # Drum-like beat using noise bursts
    beat_dur = len(base)/SAMPLE_RATE
    t_total = int(SAMPLE_RATE * beat_dur)
    beat = np.zeros(t_total)
    beat_interval = int(SAMPLE_RATE * 0.4)
    for i in range(0, t_total, beat_interval):
        end = min(i + int(SAMPLE_RATE*0.05), t_total)
        noise = np.random.Random(i).uniform(-1,1,end-i) if False else np.random.uniform(-0.4,0.4,end-i)
        env = np.linspace(0.6,0,end-i)
        beat[i:end] += noise*env
    combined = base + beat*0.4
    # Tile with increasing volume
    tracks = []
    for i in range(6):
        vol_scale = 0.7 + i * 0.06
        tracks.append(combined * vol_scale)
    track = np.concatenate(tracks)
    track[-SAMPLE_RATE*2:] *= np.linspace(1,0,SAMPLE_RATE*2)
    save_wav(f"{AUD}/Music/act2_dheki.wav", track)

def make_boss_music():
    """Boss fight — intense, dissonant, dark."""
    N = NOTES
    melody = [
        N['Sa'],N['Sa'],N['Re'],N['Sa'],N['Re'],N['Sa'],N['Ga'],N['Sa'],
        N['Sa'],N['Re'],N['Sa'],N['Ni'],N['Sa'],N['Ni'],N['Dha'],N['Sa'],
    ]
    durs = [0.15]*16
    base = make_melody(melody, durs, vol=0.3)
    t = np.linspace(0, len(base)/SAMPLE_RATE, len(base))
    # Dissonant layer
    dissonant = 0.15 * np.sin(2*np.pi*311*t) + 0.1*np.sin(2*np.pi*370*t)
    # Pulse
    pulse = 0.2 * np.sign(np.sin(2*np.pi*4*t))
    combined = base + dissonant + pulse * 0.1
    track = np.tile(combined, 6)
    track[-SAMPLE_RATE*2:] *= np.linspace(1,0,SAMPLE_RATE*2)
    save_wav(f"{AUD}/Music/boss_music.wav", track)

def make_epilogue_music():
    """Epilogue — peaceful, resolution, hopeful."""
    N = NOTES
    melody = [
        N['Sa'],N['Re'],N['Ga'],N['Pa'],N['Dha'],N['Sa2'],N['Dha'],N['Pa'],
        N['Ga'],N['Pa'],N['Dha'],N['Sa2'],N['Re2'],N['Sa2'],N['Dha'],N['Pa'],
        N['Sa'],N['Ga'],N['Pa'],N['Sa2'],N['Re2'],N['Sa2'],N['Pa'],N['Ga'],
        N['Re'],N['Sa'],N['Re'],N['Ga'],N['Pa'],N['Ga'],N['Re'],N['Sa'],
    ]
    durs = [0.3,0.3,0.3,0.3, 0.4,0.4,0.3,0.3,
            0.3,0.3,0.3,0.3, 0.4,0.4,0.3,0.3,
            0.3,0.3,0.3,0.3, 0.4,0.4,0.3,0.3,
            0.3,0.3,0.3,0.3, 0.5,0.3,0.3,0.6]
    base = make_melody(melody, durs, vol=0.3)
    t = np.linspace(0, len(base)/SAMPLE_RATE, len(base))
    # Gentle pad
    pad = 0.08 * np.sin(2*np.pi*N['Sa']*t) + 0.06*np.sin(2*np.pi*N['Pa_l']*t)
    combined = base + pad
    track = np.tile(combined, 3)
    track[-SAMPLE_RATE*3:] *= np.linspace(1,0,SAMPLE_RATE*3)
    save_wav(f"{AUD}/Music/epilogue.wav", track)

def make_sfx():
    """Generate all SFX."""
    rate = SAMPLE_RATE

    # Footstep — short thud
    t = np.linspace(0, 0.08, int(rate*0.08))
    noise = np.random.uniform(-1,1,len(t))
    env = np.exp(-t*40)
    save_wav(f"{AUD}/SFX/footstep.wav", noise*env*0.5)

    # Hide — soft whoosh
    t = np.linspace(0, 0.3, int(rate*0.3))
    noise = np.random.uniform(-0.5,0.5,len(t))
    env = np.exp(-t*8) * (1-np.exp(-t*30))
    lpf = np.convolve(noise*env, np.ones(60)/60, mode='same')
    save_wav(f"{AUD}/SFX/hide.wav", lpf*0.4)

    # Dialogue click — short bright tap
    t = np.linspace(0,0.05,int(rate*0.05))
    click = np.sin(2*np.pi*800*t)*np.exp(-t*80)
    save_wav(f"{AUD}/SFX/dialogue_click.wav", click*0.5)

    # Beat hit (perfect) — satisfying thud+chime
    t = np.linspace(0,0.2,int(rate*0.2))
    hit = np.sin(2*np.pi*440*t)*np.exp(-t*20)
    hit += np.sin(2*np.pi*880*t)*np.exp(-t*30)*0.4
    save_wav(f"{AUD}/SFX/beat_hit_perfect.wav", hit*0.6)

    # Beat hit (good)
    t = np.linspace(0,0.15,int(rate*0.15))
    hit = np.sin(2*np.pi*350*t)*np.exp(-t*25)
    save_wav(f"{AUD}/SFX/beat_hit_good.wav", hit*0.5)

    # Beat miss — dull thud
    t = np.linspace(0,0.3,int(rate*0.3))
    noise = np.random.uniform(-0.3,0.3,len(t))
    miss = np.sin(2*np.pi*120*t)*np.exp(-t*8) + noise*np.exp(-t*10)*0.2
    save_wav(f"{AUD}/SFX/beat_miss.wav", miss*0.5)

    # Boss hit — heavy impact
    t = np.linspace(0,0.4,int(rate*0.4))
    noise = np.random.uniform(-1,1,len(t))
    impact = np.sin(2*np.pi*60*t)*np.exp(-t*6) + noise*np.exp(-t*15)*0.4
    save_wav(f"{AUD}/SFX/boss_hit.wav", impact*0.6)

    # Spirit pulse — mystical whoosh
    t = np.linspace(0,0.6,int(rate*0.6))
    sweep_freq = 300 + 400*t
    pulse = np.sin(2*np.pi*sweep_freq*t)*np.exp(-t*4)*(1-np.exp(-t*10))
    pulse += 0.2*np.sin(2*np.pi*sweep_freq*2*t)*np.exp(-t*6)
    save_wav(f"{AUD}/SFX/spirit_pulse.wav", pulse*0.5)

    # Phase transition — cinematic impact
    t = np.linspace(0,1.0,int(rate*1.0))
    boom = np.sin(2*np.pi*80*t)*np.exp(-t*4)
    noise = np.random.uniform(-0.3,0.3,len(t))*np.exp(-t*6)
    chime = np.sin(2*np.pi*523*t)*np.exp(-t*10)*0.3
    combo = boom + noise + chime
    save_wav(f"{AUD}/SFX/phase_transition.wav", combo*0.6)

    # Defeat sound — descending sweep
    t = np.linspace(0,2.0,int(rate*2.0))
    freq_sweep = 500 * np.exp(-t*1.5)
    defeat = np.sin(2*np.pi*freq_sweep*t)*np.exp(-t*0.8)
    save_wav(f"{AUD}/SFX/boss_defeat.wav", defeat*0.5)

    # Catch — stinger
    t = np.linspace(0,0.5,int(rate*0.5))
    catch = np.sin(2*np.pi*200*t)*np.exp(-t*5)
    catch += np.sin(2*np.pi*150*t)*np.exp(-t*4)*0.5
    save_wav(f"{AUD}/SFX/caught.wav", catch*0.5)

    # Collect orb — sparkle
    t = np.linspace(0,0.4,int(rate*0.4))
    sparkle = sum(
        np.sin(2*np.pi*f*t)*np.exp(-t*(8+i*3))*0.2
        for i, f in enumerate([880,1100,1320,1760])
    )
    save_wav(f"{AUD}/SFX/collect_orb.wav", sparkle*0.5)

# ─────────────────────────────────────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────────────────────────────────────

def main():
    print("Creating directories...")
    mkdirs()

    print("Generating character sprites...")
    make_tejimola_spritesheet().save(f"{ART}/Sprites/Characters/tejimola_child_spritesheet.png")
    make_tejimola_spritesheet(spirit=True).save(f"{ART}/Sprites/Characters/tejimola_spirit_spritesheet.png")
    make_dom_spritesheet().save(f"{ART}/Sprites/Characters/dom_spritesheet.png")
    make_ranima_spritesheet(corrupted=False).save(f"{ART}/Sprites/Characters/ranima_spritesheet.png")
    make_ranima_spritesheet(corrupted=True).save(f"{ART}/Sprites/Characters/ranima_corrupted_spritesheet.png")

    print("Generating portraits...")
    make_portrait('tejimola').save(f"{ART}/Sprites/Characters/tejimola_child_portrait.png")
    make_portrait('dom').save(f"{ART}/Sprites/Characters/dom_portrait.png")
    make_father_portrait().save(f"{ART}/Sprites/Characters/father_portrait.png")
    make_portrait('ranima').save(f"{ART}/Sprites/Characters/ranima_portrait.png")
    make_portrait('ranima_c').save(f"{ART}/Sprites/Characters/ranima_corrupted_portrait.png")

    print("Generating props...")
    make_nahor_flower().save(f"{ART}/Sprites/Props/nahor_flower.png")
    make_dheki().save(f"{ART}/Sprites/Props/dheki.png")
    make_hairpin().save(f"{ART}/Sprites/Props/hairpin.png")
    make_pot().save(f"{ART}/Sprites/Props/pot.png")
    make_gamosa().save(f"{ART}/Sprites/Props/gamosa.png")
    make_spirit_orb().save(f"{ART}/Sprites/Props/spirit_orb.png")
    make_spiked_barrel().save(f"{ART}/Sprites/Props/spiked_barrel.png")
    make_dhol_drum().save(f"{ART}/Sprites/Props/dhol_drum.png")
    make_gourd().save(f"{ART}/Sprites/Props/gourd.png")

    print("Generating VFX...")
    make_footprint().save(f"{ART}/VFX/footprint.png")
    make_spirit_pulse_ring().save(f"{ART}/VFX/spirit_pulse_ring.png")
    make_vine_obstacle().save(f"{ART}/VFX/vine_obstacle.png")

    print("Generating backgrounds...")
    make_background_act1()
    make_background_act2()
    make_background_act3()
    make_background_act4()

    print("Generating UI...")
    make_menu_background()
    make_dialogue_box()
    for name, fname in [
        ("New Game","btn_new_game.png"),("Continue","btn_continue.png"),
        ("Extras","btn_extras.png"),("Quit","btn_quit.png"),
        ("Back","btn_back.png"),("Resume","btn_resume.png"),
        ("Save","btn_save.png"),("Load","btn_load.png"),
        ("Settings","btn_settings.png"),
    ]:
        make_button(name, fname)
    make_hud_icon("catch_icon_inactive.png", (80,40,40,220))
    make_hud_icon("spirit_pulse_icon.png", (130,80,230,240))

    print("Generating music...")
    make_act1_music()
    make_act2_descent_music()
    make_act2_dheki_music()
    make_boss_music()
    make_epilogue_music()

    print("Generating SFX...")
    make_sfx()

    print("\n✓ All assets generated successfully!")
    print(f"  Art:   {ART}")
    print(f"  Audio: {AUD}")

if __name__ == '__main__':
    main()
