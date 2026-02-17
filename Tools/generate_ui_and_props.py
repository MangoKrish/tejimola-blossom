#!/usr/bin/env python3
"""
Generate UI elements, props, and VFX sprites.
All in Assamese Puthi painting aesthetic.
"""
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import os
import math
import random

UI_DIR = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project/Art/UI"
PROPS_DIR = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project/Art/Sprites/Props"
VFX_DIR = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project/Art/VFX"

for d in [UI_DIR, PROPS_DIR, VFX_DIR,
          os.path.join(UI_DIR, "Menu"), os.path.join(UI_DIR, "HUD"),
          os.path.join(UI_DIR, "DialogueBox")]:
    os.makedirs(d, exist_ok=True)

COLORS = {
    'gold': (255, 215, 0),
    'dark_gold': (184, 134, 11),
    'earth_brown': (139, 69, 19),
    'warm_brown': (101, 67, 33),
    'dark_slate': (47, 79, 79),
    'dark_magenta': (139, 0, 88),
    'spirit_purple': (75, 0, 130),
    'forest_green': (34, 139, 34),
    'cream': (255, 248, 240),
    'parchment': (245, 235, 220),
    'red': (178, 34, 34),
    'black': (0, 0, 0),
    'white': (255, 255, 255),
}


def draw_ornamental_frame(draw, x, y, w, h, color, thickness=3):
    """Draw Puthi-style ornamental frame."""
    # Main rectangle
    draw.rectangle([x, y, x+w, y+h], outline=color, width=thickness)

    # Corner ornaments
    corner_size = 12
    for cx, cy in [(x, y), (x+w, y), (x, y+h), (x+w, y+h)]:
        draw.ellipse([cx-corner_size//2, cy-corner_size//2,
                      cx+corner_size//2, cy+corner_size//2], fill=color)

    # Edge decorations
    for i in range(x+20, x+w-10, 20):
        draw.ellipse([i-2, y-3, i+2, y+3], fill=color)
        draw.ellipse([i-2, y+h-3, i+2, y+h+3], fill=color)
    for i in range(y+20, y+h-10, 20):
        draw.ellipse([x-3, i-2, x+3, i+2], fill=color)
        draw.ellipse([x+w-3, i-2, x+w+3, i+2], fill=color)


# ============ UI ELEMENTS ============

def generate_menu_background():
    """Main menu background with nahor tree silhouette."""
    img = Image.new('RGBA', (1920, 1080), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gradient background - warm parchment
    for y in range(1080):
        t = y / 1080
        r = int(45 + (139 - 45) * t)
        g = int(30 + (69 - 30) * t)
        b = int(50 + (19 - 50) * t * 0.5)
        draw.line([(0, y), (1920, y)], fill=(r, g, b))

    # Nahor tree silhouette (center)
    cx, cy = 960, 700
    # Trunk
    draw.rectangle([cx-15, cy-50, cx+15, cy+200], fill=COLORS['warm_brown'])
    draw.rectangle([cx-10, cy-50, cx+10, cy+200], fill=COLORS['earth_brown'])

    # Branches
    branches = [
        [(cx, cy-50), (cx-120, cy-200)],
        [(cx, cy-50), (cx+130, cy-180)],
        [(cx, cy-80), (cx-80, cy-250)],
        [(cx, cy-80), (cx+90, cy-240)],
        [(cx, cy-100), (cx-40, cy-280)],
        [(cx, cy-100), (cx+50, cy-270)],
    ]
    for b in branches:
        draw.line(b, fill=COLORS['warm_brown'], width=6)

    # Foliage canopy
    for i in range(25):
        fx = cx + random.randint(-180, 180)
        fy = cy - random.randint(150, 320)
        size = random.randint(25, 50)
        color = (34 + random.randint(-10, 10), 139 + random.randint(-20, 20), 34 + random.randint(-10, 10))
        draw.ellipse([fx-size, fy-size//2, fx+size, fy+size//2], fill=color)

    # Nahor blossoms on tree
    for i in range(30):
        bx = cx + random.randint(-160, 160)
        by = cy - random.randint(130, 300)
        draw.ellipse([bx-4, by-4, bx+4, by+4], fill=COLORS['white'])
        draw.ellipse([bx-2, by-2, bx+2, by+2], fill=COLORS['gold'])

    # Falling petals
    for i in range(15):
        px = random.randint(200, 1700)
        py = random.randint(100, 900)
        size = random.randint(2, 5)
        draw.ellipse([px-size, py-size//2, px+size, py+size//2],
                    fill=(255, 255, 255, 180))

    # Ornamental border
    draw_ornamental_frame(draw, 40, 40, 1840, 1000, COLORS['gold'], 3)

    # Ground with grass
    for x in range(0, 1920, 5):
        gy = 900 + random.randint(-5, 5)
        draw.line([(x, gy), (x + random.randint(-3, 3), gy - random.randint(5, 15))],
                 fill=COLORS['forest_green'], width=2)

    path = os.path.join(UI_DIR, "Menu", "menu_background.png")
    img.save(path)
    print(f"Generated: {path}")


def generate_button(text, width=300, height=60, style='normal'):
    """Generate styled menu button."""
    img = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    if style == 'normal':
        bg = COLORS['parchment']
        border = COLORS['gold']
        text_color = COLORS['earth_brown']
    elif style == 'highlight':
        bg = COLORS['gold']
        border = COLORS['dark_gold']
        text_color = COLORS['warm_brown']
    else:
        bg = (200, 200, 200)
        border = (150, 150, 150)
        text_color = (100, 100, 100)

    # Button shape with rounded feel
    draw.rounded_rectangle([2, 2, width-3, height-3], radius=8, fill=bg, outline=border, width=3)

    # Ornamental dots on sides
    draw.ellipse([8, height//2-4, 16, height//2+4], fill=border)
    draw.ellipse([width-16, height//2-4, width-8, height//2+4], fill=border)

    # Text (centered)
    try:
        font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 24)
    except:
        font = ImageFont.load_default()

    bbox = draw.textbbox((0, 0), text, font=font)
    tw = bbox[2] - bbox[0]
    th = bbox[3] - bbox[1]
    tx = (width - tw) // 2
    ty = (height - th) // 2 - 2
    draw.text((tx, ty), text, fill=text_color, font=font)

    return img


def generate_dialogue_box():
    """Generate dialogue box background."""
    w, h = 1200, 250
    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Semi-transparent parchment background
    draw.rounded_rectangle([0, 0, w-1, h-1], radius=12,
                          fill=(40, 30, 20, 200), outline=COLORS['gold'], width=3)

    # Inner border
    draw.rounded_rectangle([8, 8, w-9, h-9], radius=8,
                          outline=(COLORS['gold'][0], COLORS['gold'][1], COLORS['gold'][2], 120), width=1)

    # Portrait frame area (left side)
    draw.rounded_rectangle([15, 15, 175, h-15], radius=6,
                          outline=COLORS['gold'], width=2)

    # Name plate area
    draw.rounded_rectangle([190, 15, 500, 50], radius=4,
                          fill=(60, 40, 20, 180), outline=COLORS['dark_gold'], width=2)

    # Ornamental corners
    corner_size = 8
    for cx, cy in [(5, 5), (w-6, 5), (5, h-6), (w-6, h-6)]:
        draw.ellipse([cx-corner_size, cy-corner_size, cx+corner_size, cy+corner_size],
                    fill=COLORS['gold'])

    path = os.path.join(UI_DIR, "DialogueBox", "dialogue_box.png")
    img.save(path)
    print(f"Generated: {path}")


def generate_hud_elements():
    """Generate HUD sprites."""
    # Spirit Pulse icon
    size = 64
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    center = size // 2
    # Concentric rings
    for i in range(3):
        r = 12 + i * 8
        alpha = 255 - i * 60
        draw.ellipse([center-r, center-r, center+r, center+r],
                    outline=(75, 0, 130, alpha), width=2)
    # Center dot
    draw.ellipse([center-4, center-4, center+4, center+4], fill=COLORS['spirit_purple'])
    img.save(os.path.join(UI_DIR, "HUD", "spirit_pulse_icon.png"))
    print(f"Generated: spirit_pulse_icon.png")

    # Exhaustion bar background
    bar_w, bar_h = 300, 30
    img = Image.new('RGBA', (bar_w, bar_h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([0, 0, bar_w-1, bar_h-1], radius=4,
                          fill=(20, 20, 20, 180), outline=COLORS['gold'], width=2)
    img.save(os.path.join(UI_DIR, "HUD", "bar_background.png"))
    print(f"Generated: bar_background.png")

    # Bar fill (gradient)
    img = Image.new('RGBA', (bar_w-8, bar_h-8), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    for x in range(bar_w-8):
        t = x / (bar_w-8)
        r = int(220 * (1-t) + 34 * t)
        g = int(20 * (1-t) + 139 * t)
        b = int(60 * (1-t) + 34 * t)
        draw.line([(x, 0), (x, bar_h-9)], fill=(r, g, b))
    img.save(os.path.join(UI_DIR, "HUD", "bar_fill.png"))
    print(f"Generated: bar_fill.png")

    # Catch icons (eye)
    eye_size = 32
    for state in ['active', 'inactive']:
        img = Image.new('RGBA', (eye_size, eye_size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        c = eye_size // 2
        if state == 'active':
            draw.ellipse([c-10, c-6, c+10, c+6], fill=COLORS['red'], outline=COLORS['black'], width=2)
            draw.ellipse([c-3, c-3, c+3, c+3], fill=COLORS['black'])
        else:
            draw.ellipse([c-10, c-6, c+10, c+6], fill=(100, 100, 100), outline=(60, 60, 60), width=2)
        img.save(os.path.join(UI_DIR, "HUD", f"catch_icon_{state}.png"))
    print(f"Generated: catch icons")


# ============ PROPS ============

def generate_props():
    """Generate game props and items."""
    props = {
        'dheki': generate_dheki,
        'dhol_drum': generate_dhol,
        'nahor_flower': generate_nahor_flower,
        'hairpin': generate_hairpin,
        'oil_lamp': generate_oil_lamp,
        'pot': generate_pot,
        'spirit_orb': generate_spirit_orb,
        'gourd': generate_gourd,
        'gamosa': generate_gamosa,
        'spiked_barrel': generate_spiked_barrel,
    }

    for name, generator in props.items():
        img = generator()
        path = os.path.join(PROPS_DIR, f"{name}.png")
        img.save(path)
        print(f"Generated: {path}")


def generate_dheki():
    """Dheki - traditional rice husker. Central prop."""
    img = Image.new('RGBA', (256, 256), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Base/fulcrum
    draw.rectangle([80, 180, 180, 200], fill=COLORS['earth_brown'], outline=COLORS['black'], width=3)

    # Lever arm
    draw.polygon([(40, 170), (220, 140), (225, 150), (45, 180)],
                fill=COLORS['warm_brown'], outline=COLORS['black'])

    # Pestle end (right)
    draw.rectangle([210, 130, 230, 200], fill=COLORS['earth_brown'], outline=COLORS['black'], width=2)

    # Mortar (stone bowl)
    draw.ellipse([190, 190, 250, 230], fill=(120, 120, 120), outline=COLORS['black'], width=3)

    # Rice grains in mortar
    for i in range(5):
        rx = 210 + random.randint(-10, 10)
        ry = 200 + random.randint(-3, 3)
        draw.ellipse([rx-2, ry-1, rx+2, ry+1], fill=COLORS['cream'])

    # Foot pedal
    draw.polygon([(30, 175), (60, 175), (55, 210), (25, 210)],
                fill=COLORS['warm_brown'], outline=COLORS['black'], width=2)

    return img


def generate_dhol():
    """Dhol drum - Dom's instrument."""
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = 64, 64

    # Drum body (barrel shape)
    draw.ellipse([cx-35, cy-20, cx+35, cy+20], fill=COLORS['earth_brown'], outline=COLORS['black'], width=3)

    # Drum heads (leather)
    draw.ellipse([cx-35, cy-15, cx-20, cy+15], fill=COLORS['cream'], outline=COLORS['black'], width=2)
    draw.ellipse([cx+20, cy-15, cx+35, cy+15], fill=COLORS['cream'], outline=COLORS['black'], width=2)

    # Lacing pattern
    for i in range(8):
        y_off = int(12 * math.sin(i * 0.8))
        draw.line([(cx-20, cy-10+y_off), (cx+20, cy-10+y_off)], fill=COLORS['red'], width=1)

    # Decorative bands
    draw.line([(cx-20, cy-20), (cx-20, cy+20)], fill=COLORS['gold'], width=2)
    draw.line([(cx+20, cy-20), (cx+20, cy+20)], fill=COLORS['gold'], width=2)

    return img


def generate_nahor_flower():
    """Nahor (Mesua ferrea) flower - key symbol."""
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = 32, 32

    # 4 white petals
    for angle in [0, 90, 180, 270]:
        rad = math.radians(angle)
        px = cx + int(12 * math.cos(rad))
        py = cy + int(12 * math.sin(rad))
        draw.ellipse([px-8, py-8, px+8, py+8], fill=COLORS['white'], outline=COLORS['black'], width=1)

    # Golden center with stamens
    draw.ellipse([cx-6, cy-6, cx+6, cy+6], fill=COLORS['gold'])
    for angle in range(0, 360, 30):
        rad = math.radians(angle)
        sx = cx + int(8 * math.cos(rad))
        sy = cy + int(8 * math.sin(rad))
        draw.line([(cx, cy), (sx, sy)], fill=COLORS['dark_gold'], width=1)
        draw.ellipse([sx-1, sy-1, sx+1, sy+1], fill=COLORS['gold'])

    return img


def generate_hairpin():
    """Mother's hairpin - puzzle item."""
    img = Image.new('RGBA', (64, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Pin shaft
    draw.line([(32, 20), (32, 110)], fill=COLORS['gold'], width=3)

    # Decorative head (flower motif)
    draw.ellipse([20, 5, 44, 35], fill=COLORS['gold'], outline=COLORS['dark_gold'], width=2)
    # Flower detail
    for angle in range(0, 360, 60):
        rad = math.radians(angle)
        px = 32 + int(8 * math.cos(rad))
        py = 20 + int(8 * math.sin(rad))
        draw.ellipse([px-3, py-3, px+3, py+3], fill=COLORS['red'])
    draw.ellipse([29, 17, 35, 23], fill=COLORS['gold'])

    return img


def generate_oil_lamp():
    """Traditional oil lamp (saaki)."""
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Lamp body
    draw.polygon([(20, 50), (44, 50), (40, 35), (24, 35)],
                fill=(180, 140, 60), outline=COLORS['black'], width=2)

    # Oil
    draw.rectangle([26, 32, 38, 36], fill=(200, 160, 40))

    # Wick & flame
    draw.line([(32, 32), (32, 22)], fill=COLORS['black'], width=2)
    # Flame
    draw.polygon([(32, 10), (27, 22), (37, 22)], fill=(255, 200, 50))
    draw.polygon([(32, 14), (29, 22), (35, 22)], fill=(255, 150, 30))

    # Glow
    draw.ellipse([22, 5, 42, 28], fill=(255, 215, 0, 30))

    return img


def generate_pot():
    """Clay pot."""
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Pot body
    draw.ellipse([12, 20, 52, 55], fill=(180, 120, 60), outline=COLORS['black'], width=2)
    # Neck
    draw.rectangle([22, 12, 42, 22], fill=(180, 120, 60), outline=COLORS['black'], width=2)
    # Rim
    draw.ellipse([18, 8, 46, 16], fill=(160, 100, 40), outline=COLORS['black'], width=2)

    # Decoration band
    draw.arc([14, 30, 50, 45], 0, 180, fill=COLORS['red'], width=2)

    return img


def generate_spirit_orb():
    """Spirit orb collectible."""
    img = Image.new('RGBA', (48, 48), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = 24, 24

    # Outer glow
    draw.ellipse([cx-20, cy-20, cx+20, cy+20], fill=(75, 0, 130, 40))
    draw.ellipse([cx-15, cy-15, cx+15, cy+15], fill=(100, 50, 180, 80))
    draw.ellipse([cx-10, cy-10, cx+10, cy+10], fill=(150, 100, 220, 160))
    draw.ellipse([cx-5, cy-5, cx+5, cy+5], fill=(200, 180, 255, 220))
    draw.ellipse([cx-2, cy-2, cx+2, cy+2], fill=COLORS['white'])

    return img


def generate_gourd():
    """Gourd - story item."""
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gourd shape
    draw.ellipse([15, 25, 50, 55], fill=(180, 160, 80), outline=COLORS['black'], width=2)
    draw.ellipse([22, 10, 42, 30], fill=(170, 150, 70), outline=COLORS['black'], width=2)
    # Stem
    draw.rectangle([30, 5, 34, 12], fill=(80, 120, 40), outline=COLORS['black'], width=1)

    return img


def generate_gamosa():
    """Gamosa - Assamese towel, cultural symbol."""
    img = Image.new('RGBA', (128, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # White base
    draw.rectangle([5, 10, 123, 54], fill=COLORS['white'], outline=COLORS['black'], width=2)

    # Red border pattern
    for y in [12, 52]:
        draw.line([(7, y), (121, y)], fill=COLORS['red'], width=3)
    for x in range(10, 120, 8):
        draw.line([(x, 13), (x, 15)], fill=COLORS['red'], width=2)
        draw.line([(x, 50), (x, 52)], fill=COLORS['red'], width=2)

    # Red end section with pattern
    draw.rectangle([5, 10, 25, 54], fill=COLORS['red'], outline=COLORS['black'], width=1)
    draw.rectangle([103, 10, 123, 54], fill=COLORS['red'], outline=COLORS['black'], width=1)

    # Motifs in red sections
    for sx in [15, 113]:
        draw.ellipse([sx-4, 25, sx+4, 39], fill=COLORS['gold'])

    return img


def generate_spiked_barrel():
    """Spiked barrel - boss fight obstacle."""
    img = Image.new('RGBA', (96, 96), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = 48, 48

    # Barrel body
    draw.ellipse([cx-25, cy-25, cx+25, cy+25], fill=(100, 60, 30), outline=COLORS['black'], width=3)

    # Bands
    draw.ellipse([cx-22, cy-22, cx+22, cy+22], outline=(60, 40, 20), width=2)
    draw.ellipse([cx-15, cy-15, cx+15, cy+15], outline=(60, 40, 20), width=2)

    # Spikes
    for angle in range(0, 360, 30):
        rad = math.radians(angle)
        bx = cx + int(25 * math.cos(rad))
        by = cy + int(25 * math.sin(rad))
        ex = cx + int(35 * math.cos(rad))
        ey = cy + int(35 * math.sin(rad))
        draw.polygon([(bx-3, by), (ex, ey), (bx+3, by)], fill=(80, 80, 80), outline=COLORS['black'])

    return img


# ============ VFX ============

def generate_vfx():
    """Generate VFX sprites."""
    # Spirit pulse ring
    size = 256
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size//2, size//2
    for r in range(50, 120, 3):
        alpha = max(0, 255 - (r - 50) * 3)
        draw.ellipse([cx-r, cy-r, cx+r, cy+r],
                    outline=(75, 0, 130, alpha), width=2)
    img.save(os.path.join(VFX_DIR, "spirit_pulse_ring.png"))
    print(f"Generated: spirit_pulse_ring.png")

    # Memory flash
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    for r in range(60, 0, -2):
        alpha = int(200 * (r / 60))
        draw.ellipse([64-r, 64-r, 64+r, 64+r], fill=(255, 255, 255, alpha))
    img.save(os.path.join(VFX_DIR, "memory_flash.png"))
    print(f"Generated: memory_flash.png")

    # Beat indicator
    for state in ['perfect', 'good', 'miss']:
        img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        if state == 'perfect':
            color = COLORS['gold']
        elif state == 'good':
            color = (135, 206, 235)
        else:
            color = COLORS['dark_magenta']

        draw.ellipse([8, 8, 56, 56], fill=(*color, 200), outline=COLORS['black'], width=2)
        draw.ellipse([16, 16, 48, 48], fill=(*color, 255), outline=COLORS['black'], width=1)
        img.save(os.path.join(VFX_DIR, f"beat_{state}.png"))
    print(f"Generated: beat indicators")

    # Footprint
    img = Image.new('RGBA', (32, 48), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.ellipse([6, 5, 26, 35], fill=(100, 80, 60, 150))
    for i in range(5):
        tx = 10 + i * 4
        draw.ellipse([tx-2, 2, tx+2, 6], fill=(100, 80, 60, 120))
    img.save(os.path.join(VFX_DIR, "footprint.png"))
    print(f"Generated: footprint.png")

    # Corruption particle
    img = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    for r in range(14, 0, -1):
        alpha = int(180 * (r / 14))
        draw.ellipse([16-r, 16-r, 16+r, 16+r], fill=(139, 0, 88, alpha))
    img.save(os.path.join(VFX_DIR, "corruption_particle.png"))
    print(f"Generated: corruption_particle.png")

    # Vine obstacle
    img = Image.new('RGBA', (64, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    for i in range(5):
        sx = 32 + int(15 * math.sin(i * 1.2))
        sy = i * 25
        draw.line([(sx, sy), (sx + int(10*math.sin(i)), sy+25)],
                 fill=COLORS['forest_green'], width=4)
        # Leaves
        draw.ellipse([sx-8, sy+5, sx+2, sy+15], fill=(50, 160, 50))
        draw.ellipse([sx+2, sy+10, sx+12, sy+20], fill=(40, 140, 40))
    img.save(os.path.join(VFX_DIR, "vine_obstacle.png"))
    print(f"Generated: vine_obstacle.png")


if __name__ == "__main__":
    print("Generating UI elements, props, and VFX...")
    print()

    print("--- MENU UI ---")
    generate_menu_background()

    buttons = {
        'btn_new_game': 'NEW GAME',
        'btn_continue': 'CONTINUE',
        'btn_extras': 'EXTRAS',
        'btn_quit': 'QUIT',
        'btn_resume': 'RESUME',
        'btn_save': 'SAVE',
        'btn_load': 'LOAD',
        'btn_settings': 'SETTINGS',
        'btn_back': 'BACK',
    }
    for filename, text in buttons.items():
        img = generate_button(text)
        path = os.path.join(UI_DIR, "Menu", f"{filename}.png")
        img.save(path)
        print(f"Generated: {path}")

    print("\n--- DIALOGUE BOX ---")
    generate_dialogue_box()

    print("\n--- HUD ---")
    generate_hud_elements()

    print("\n--- PROPS ---")
    generate_props()

    print("\n--- VFX ---")
    generate_vfx()

    print("\nAll UI, props, and VFX generated!")
