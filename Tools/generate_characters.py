#!/usr/bin/env python3
"""
Generate character sprite sheets in Assamese Puthi painting style.
Bold outlines, flat color fills, ornamental patterns.
Each character: 512x512 sprite sheets with multiple animation frames.
"""
from PIL import Image, ImageDraw, ImageFont
import os
import math

OUTPUT_DIR = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project/Art/Sprites/Characters"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Color palette from game design doc
COLORS = {
    'gold': (255, 215, 0),
    'sky_blue': (135, 206, 235),
    'earth_brown': (139, 69, 19),
    'warm_brown': (101, 67, 33),
    'dark_slate': (47, 79, 79),
    'dark_magenta': (139, 0, 88),
    'hope_blue': (100, 149, 237),
    'silver': (192, 192, 192),
    'forest_green': (34, 139, 34),
    'spirit_purple': (75, 0, 130),
    'triumph_red': (220, 20, 60),
    'pure_white': (255, 255, 255),
    'black': (0, 0, 0),
    'skin_warm': (222, 184, 135),
    'skin_light': (245, 222, 179),
    'mekhela_red': (178, 34, 34),
    'mekhela_gold': (218, 165, 32),
    'sador_white': (255, 248, 240),
    'hair_black': (25, 25, 25),
}

OUTLINE_WIDTH = 3

def draw_outlined_ellipse(draw, bbox, fill, outline=(0,0,0), width=OUTLINE_WIDTH):
    """Draw an ellipse with bold outline in Puthi style."""
    draw.ellipse(bbox, fill=fill, outline=outline, width=width)

def draw_outlined_rect(draw, bbox, fill, outline=(0,0,0), width=OUTLINE_WIDTH):
    """Draw a rectangle with bold outline."""
    draw.rectangle(bbox, fill=fill, outline=outline, width=width)

def draw_outlined_polygon(draw, points, fill, outline=(0,0,0), width=OUTLINE_WIDTH):
    """Draw a polygon with bold outline."""
    draw.polygon(points, fill=fill, outline=outline)
    # Draw thick outline
    for i in range(len(points)):
        p1 = points[i]
        p2 = points[(i+1) % len(points)]
        draw.line([p1, p2], fill=outline, width=width)

def draw_ornamental_border(draw, x, y, w, h, color):
    """Draw Assamese manuscript-style ornamental border pattern."""
    step = 8
    for i in range(0, w, step):
        # Top border dots
        draw.ellipse([x+i-2, y-4, x+i+2, y], fill=color)
        # Bottom border dots
        draw.ellipse([x+i-2, y+h, x+i+2, y+h+4], fill=color)

def add_puthi_details(draw, x, y, size, color):
    """Add Assamese Puthi manuscript-style decorative details."""
    # Small lotus-like motifs
    for angle in range(0, 360, 45):
        rad = math.radians(angle)
        px = x + int(size * 0.3 * math.cos(rad))
        py = y + int(size * 0.3 * math.sin(rad))
        draw.ellipse([px-2, py-2, px+2, py+2], fill=color)


def generate_tejimola_child(frame=0):
    """Generate Tejimola as a child - Act I & II character."""
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = 64, 64  # Center

    # Slight animation offset
    sway = int(3 * math.sin(frame * 0.8))
    bob = int(2 * math.sin(frame * 1.2))

    # Mekhela-Sador (traditional Assamese dress) - Body
    # Lower garment (mekhela) - deep red with gold border
    body_points = [
        (cx-18, cy-5+bob),   # Left shoulder
        (cx+18, cy-5+bob),   # Right shoulder
        (cx+22, cy+45),      # Right hem
        (cx-22, cy+45),      # Left hem
    ]
    draw_outlined_polygon(draw, body_points, COLORS['mekhela_red'])

    # Sador (upper drape) - white with gold
    sador_points = [
        (cx-16+sway, cy-5+bob),
        (cx+8+sway, cy-5+bob),
        (cx+12+sway, cy+15+bob),
        (cx-20+sway, cy+18+bob),
    ]
    draw_outlined_polygon(draw, sador_points, COLORS['sador_white'])

    # Gold border on mekhela
    draw.line([(cx-22, cy+43), (cx+22, cy+43)], fill=COLORS['mekhela_gold'], width=3)
    draw.line([(cx-21, cy+40), (cx+21, cy+40)], fill=COLORS['gold'], width=1)

    # Arms
    # Left arm
    draw.line([(cx-18, cy+bob), (cx-25+sway, cy+20+bob)], fill=COLORS['skin_warm'], width=6)
    draw_outlined_ellipse(draw, [cx-28+sway, cy+18+bob, cx-22+sway, cy+24+bob], COLORS['skin_warm'])

    # Right arm
    draw.line([(cx+18, cy+bob), (cx+25+sway, cy+18+bob)], fill=COLORS['skin_warm'], width=6)
    draw_outlined_ellipse(draw, [cx+22+sway, cy+16+bob, cx+28+sway, cy+22+bob], COLORS['skin_warm'])

    # Legs/feet
    draw.line([(cx-8, cy+45), (cx-10, cy+55)], fill=COLORS['skin_warm'], width=5)
    draw.line([(cx+8, cy+45), (cx+10, cy+55)], fill=COLORS['skin_warm'], width=5)

    # Head
    draw_outlined_ellipse(draw, [cx-14, cy-32+bob, cx+14, cy-4+bob], COLORS['skin_warm'])

    # Hair - long black hair
    draw.arc([cx-16, cy-36+bob, cx+16, cy-10+bob], 180, 360, fill=COLORS['hair_black'], width=4)
    # Hair strands down sides
    draw.line([(cx-14, cy-20+bob), (cx-18, cy+5+bob)], fill=COLORS['hair_black'], width=3)
    draw.line([(cx+14, cy-20+bob), (cx+18, cy+5+bob)], fill=COLORS['hair_black'], width=3)
    # Hair bun with flower
    draw_outlined_ellipse(draw, [cx-8, cy-38+bob, cx+8, cy-30+bob], COLORS['hair_black'])

    # Flower in hair (kopou phool - Assamese orchid)
    draw_outlined_ellipse(draw, [cx+6, cy-36+bob, cx+14, cy-28+bob], COLORS['gold'])
    draw.ellipse([cx+8, cy-34+bob, cx+12, cy-30+bob], fill=COLORS['triumph_red'])

    # Face details
    # Eyes - large, expressive (Puthi style)
    draw_outlined_ellipse(draw, [cx-9, cy-22+bob, cx-3, cy-16+bob], COLORS['pure_white'])
    draw.ellipse([cx-7, cy-20+bob, cx-4, cy-17+bob], fill=COLORS['black'])

    draw_outlined_ellipse(draw, [cx+3, cy-22+bob, cx+9, cy-16+bob], COLORS['pure_white'])
    draw.ellipse([cx+4, cy-20+bob, cx+7, cy-17+bob], fill=COLORS['black'])

    # Smile
    draw.arc([cx-6, cy-16+bob, cx+6, cy-8+bob], 0, 180, fill=COLORS['black'], width=2)

    # Bindi (small)
    draw.ellipse([cx-2, cy-24+bob, cx+2, cy-20+bob], fill=COLORS['triumph_red'])

    # Gold jewelry - small necklace
    draw.arc([cx-10, cy-6+bob, cx+10, cy+2+bob], 0, 180, fill=COLORS['gold'], width=2)

    return img


def generate_tejimola_spirit(frame=0):
    """Generate Tejimola as spirit/memory - translucent, ethereal."""
    img = generate_tejimola_child(frame)

    # Make semi-transparent and add blue/white glow
    pixels = img.load()
    for x in range(img.width):
        for y in range(img.height):
            r, g, b, a = pixels[x, y]
            if a > 0:
                # Shift toward blue/white, increase transparency
                r = min(255, r + 60)
                g = min(255, g + 60)
                b = min(255, b + 80)
                a = int(a * 0.6)
                pixels[x, y] = (r, g, b, a)

    # Add glow effect around edges
    draw = ImageDraw.Draw(img)
    for angle in range(0, 360, 30):
        rad = math.radians(angle)
        gx = 64 + int(30 * math.cos(rad))
        gy = 40 + int(20 * math.sin(rad))
        draw.ellipse([gx-3, gy-3, gx+3, gy+3], fill=(200, 200, 255, 40))

    return img


def generate_dom(frame=0):
    """Generate Dom - the spirit-sensitive drummer. Adult male, weathered but gentle."""
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = 64, 60
    sway = int(2 * math.sin(frame * 0.6))
    bob = int(1 * math.sin(frame * 1.0))

    # Body - simple dhoti and gamosa
    # Dhoti (lower garment) - white/off-white
    body_points = [
        (cx-20, cy-8+bob),
        (cx+20, cy-8+bob),
        (cx+18, cy+48),
        (cx-18, cy+48),
    ]
    draw_outlined_polygon(draw, body_points, COLORS['sador_white'])

    # Upper body - bare-chested with gamosa (Assamese towel) draped
    draw_outlined_rect(draw, [cx-18, cy-20+bob, cx+18, cy-5+bob], COLORS['skin_warm'])

    # Gamosa (red and white checkered towel) over shoulder
    gamosa_points = [
        (cx-16+sway, cy-18+bob),
        (cx+5+sway, cy-20+bob),
        (cx+8+sway, cy+5+bob),
        (cx-12+sway, cy+8+bob),
    ]
    draw_outlined_polygon(draw, gamosa_points, COLORS['pure_white'])
    # Gamosa red pattern
    draw.line([(cx-14+sway, cy-14+bob), (cx+3+sway, cy-16+bob)], fill=COLORS['triumph_red'], width=2)
    draw.line([(cx-13+sway, cy-8+bob), (cx+4+sway, cy-10+bob)], fill=COLORS['triumph_red'], width=2)

    # Arms - stronger build
    draw.line([(cx-20, cy-12+bob), (cx-28+sway, cy+8+bob)], fill=COLORS['skin_warm'], width=7)
    draw_outlined_ellipse(draw, [cx-31+sway, cy+6+bob, cx-25+sway, cy+12+bob], COLORS['skin_warm'])

    draw.line([(cx+20, cy-12+bob), (cx+28+sway, cy+8+bob)], fill=COLORS['skin_warm'], width=7)
    draw_outlined_ellipse(draw, [cx+25+sway, cy+6+bob, cx+31+sway, cy+12+bob], COLORS['skin_warm'])

    # Dhol (drum) hanging from neck - when idle
    if frame % 8 < 4:  # Alternate with/without drum visible
        drum_cx = cx + 15
        drum_cy = cy + 10 + bob
        # Drum body
        draw_outlined_ellipse(draw, [drum_cx-12, drum_cy-8, drum_cx+12, drum_cy+8], COLORS['earth_brown'])
        # Drum heads
        draw.ellipse([drum_cx-12, drum_cy-6, drum_cx-6, drum_cy+6], fill=COLORS['skin_light'], outline=COLORS['black'], width=2)
        draw.ellipse([drum_cx+6, drum_cy-6, drum_cx+12, drum_cy+6], fill=COLORS['skin_light'], outline=COLORS['black'], width=2)
        # Strap
        draw.line([(drum_cx-5, drum_cy-8), (cx+5, cy-18+bob)], fill=COLORS['warm_brown'], width=2)

    # Head
    draw_outlined_ellipse(draw, [cx-15, cy-42+bob, cx+15, cy-14+bob], COLORS['skin_warm'])

    # Hair - messy, spiritual
    for angle in range(160, 380, 15):
        rad = math.radians(angle)
        hx = cx + int(16 * math.cos(rad))
        hy = cy - 28 + bob + int(14 * math.sin(rad))
        draw.line([(cx + int(10 * math.cos(rad)), cy-28+bob + int(8 * math.sin(rad))),
                   (hx, hy)], fill=COLORS['hair_black'], width=2)

    # Face
    # Eyes - deeper set, knowing
    draw_outlined_ellipse(draw, [cx-10, cy-32+bob, cx-4, cy-26+bob], COLORS['pure_white'])
    draw.ellipse([cx-8, cy-30+bob, cx-5, cy-27+bob], fill=COLORS['warm_brown'])

    draw_outlined_ellipse(draw, [cx+4, cy-32+bob, cx+10, cy-26+bob], COLORS['pure_white'])
    draw.ellipse([cx+5, cy-30+bob, cx+8, cy-27+bob], fill=COLORS['warm_brown'])

    # Gentle expression - slight smile
    draw.arc([cx-5, cy-24+bob, cx+5, cy-18+bob], 10, 170, fill=COLORS['black'], width=2)

    # Stubble/beard suggestion
    for i in range(0, 8):
        bx = cx - 4 + i
        by = cy - 18 + bob
        draw.line([(bx, by), (bx, by+3)], fill=(80, 80, 80, 120), width=1)

    # Spirit marks - subtle purple dots on forehead (third eye area)
    draw.ellipse([cx-2, cy-36+bob, cx+2, cy-32+bob], fill=COLORS['spirit_purple'])

    # Legs
    draw.line([(cx-8, cy+48), (cx-10, cy+58)], fill=COLORS['skin_warm'], width=5)
    draw.line([(cx+8, cy+48), (cx+10, cy+58)], fill=COLORS['skin_warm'], width=5)

    return img


def generate_ranima(frame=0):
    """Generate Ranima (stepmother) - imposing, dark colors, sharp features."""
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = 64, 58
    sway = int(1 * math.sin(frame * 0.4))

    # Body - dark mekhela-sador, imposing posture
    body_points = [
        (cx-22, cy-10),
        (cx+22, cy-10),
        (cx+25, cy+50),
        (cx-25, cy+50),
    ]
    draw_outlined_polygon(draw, body_points, COLORS['dark_magenta'])

    # Sador - dark
    sador_points = [
        (cx-20+sway, cy-10),
        (cx+10+sway, cy-12),
        (cx+14+sway, cy+20),
        (cx-24+sway, cy+22),
    ]
    draw_outlined_polygon(draw, sador_points, COLORS['dark_slate'])

    # Dark gold border
    draw.line([(cx-25, cy+48), (cx+25, cy+48)], fill=(139, 119, 42), width=3)

    # Arms - commanding gesture
    draw.line([(cx-22, cy-4), (cx-32+sway, cy+12)], fill=COLORS['skin_light'], width=6)
    draw.line([(cx+22, cy-4), (cx+30+sway, cy+15)], fill=COLORS['skin_light'], width=6)

    # Head - sharper features
    draw_outlined_ellipse(draw, [cx-15, cy-42, cx+15, cy-14], COLORS['skin_light'])

    # Hair - pulled back tight, severe
    draw.arc([cx-17, cy-46, cx+17, cy-20], 180, 360, fill=COLORS['hair_black'], width=5)
    # Hair bun (tight)
    draw_outlined_ellipse(draw, [cx-6, cy-48, cx+6, cy-40], COLORS['hair_black'])

    # Face - sharp, angular eyes
    # Eyes - narrow, piercing
    draw.line([(cx-11, cy-30), (cx-3, cy-28)], fill=COLORS['black'], width=2)
    draw.ellipse([cx-9, cy-30, cx-5, cy-27], fill=COLORS['pure_white'])
    draw.ellipse([cx-8, cy-29, cx-6, cy-27], fill=COLORS['black'])

    draw.line([(cx+3, cy-28), (cx+11, cy-30)], fill=COLORS['black'], width=2)
    draw.ellipse([cx+5, cy-30, cx+9, cy-27], fill=COLORS['pure_white'])
    draw.ellipse([cx+6, cy-29, cx+8, cy-27], fill=COLORS['black'])

    # Thin, cruel mouth
    draw.line([(cx-5, cy-20), (cx+5, cy-20)], fill=COLORS['black'], width=2)
    draw.line([(cx-5, cy-20), (cx-7, cy-22)], fill=COLORS['black'], width=1)
    draw.line([(cx+5, cy-20), (cx+7, cy-22)], fill=COLORS['black'], width=1)

    # Dark bindi
    draw.ellipse([cx-2, cy-34, cx+2, cy-30], fill=COLORS['dark_magenta'])

    # Heavy jewelry - oppressive gold
    draw.arc([cx-12, cy-14, cx+12, cy-4], 0, 180, fill=(139, 119, 42), width=3)

    # Feet
    draw.line([(cx-8, cy+50), (cx-10, cy+58)], fill=COLORS['skin_light'], width=5)
    draw.line([(cx+8, cy+50), (cx+10, cy+58)], fill=COLORS['skin_light'], width=5)

    return img


def generate_father(frame=0):
    """Generate Father - kind merchant, warm colors."""
    img = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = 64, 58
    bob = int(1 * math.sin(frame * 0.8))

    # Body - warm brown kurta
    body_points = [
        (cx-22, cy-12+bob),
        (cx+22, cy-12+bob),
        (cx+20, cy+48),
        (cx-20, cy+48),
    ]
    draw_outlined_polygon(draw, body_points, COLORS['earth_brown'])

    # Dhoti
    draw_outlined_rect(draw, [cx-18, cy+20, cx+18, cy+48], COLORS['sador_white'])

    # Gold trim on kurta
    draw.line([(cx-2, cy-12+bob), (cx-2, cy+20)], fill=COLORS['gold'], width=2)
    draw.line([(cx+2, cy-12+bob), (cx+2, cy+20)], fill=COLORS['gold'], width=2)

    # Arms
    draw.line([(cx-22, cy-6+bob), (cx-28, cy+12+bob)], fill=COLORS['skin_warm'], width=6)
    draw.line([(cx+22, cy-6+bob), (cx+28, cy+12+bob)], fill=COLORS['skin_warm'], width=6)

    # Head - round, kind face
    draw_outlined_ellipse(draw, [cx-16, cy-44+bob, cx+16, cy-14+bob], COLORS['skin_warm'])

    # Hair - neat
    draw.arc([cx-18, cy-48+bob, cx+18, cy-22+bob], 180, 360, fill=COLORS['hair_black'], width=5)

    # Mustache
    draw.arc([cx-8, cy-22+bob, cx, cy-16+bob], 0, 180, fill=COLORS['hair_black'], width=2)
    draw.arc([cx, cy-22+bob, cx+8, cy-16+bob], 0, 180, fill=COLORS['hair_black'], width=2)

    # Kind eyes
    draw_outlined_ellipse(draw, [cx-10, cy-34+bob, cx-4, cy-28+bob], COLORS['pure_white'])
    draw.ellipse([cx-8, cy-32+bob, cx-5, cy-29+bob], fill=COLORS['warm_brown'])

    draw_outlined_ellipse(draw, [cx+4, cy-34+bob, cx+10, cy-28+bob], COLORS['pure_white'])
    draw.ellipse([cx+5, cy-32+bob, cx+8, cy-29+bob], fill=COLORS['warm_brown'])

    # Warm smile
    draw.arc([cx-6, cy-24+bob, cx+6, cy-16+bob], 10, 170, fill=COLORS['black'], width=2)

    return img


def generate_sprite_sheet(generator, name, frames=8, sheet_size=512):
    """Generate a sprite sheet with multiple frames."""
    frame_size = 128
    cols = sheet_size // frame_size
    rows = (frames + cols - 1) // cols

    sheet = Image.new('RGBA', (sheet_size, rows * frame_size), (0, 0, 0, 0))

    for i in range(frames):
        frame_img = generator(frame=i)
        col = i % cols
        row = i // cols
        sheet.paste(frame_img, (col * frame_size, row * frame_size))

    output_path = os.path.join(OUTPUT_DIR, f"{name}_spritesheet.png")
    sheet.save(output_path)
    print(f"Generated: {output_path} ({frames} frames)")

    # Also save individual idle frame as portrait
    portrait = generator(frame=0)
    portrait_path = os.path.join(OUTPUT_DIR, f"{name}_portrait.png")
    portrait_resized = portrait.resize((256, 256), Image.NEAREST)
    portrait_resized.save(portrait_path)
    print(f"Generated: {portrait_path}")


def generate_ranima_corrupted(frame=0):
    """Generate Ranima's corrupted boss form - Act IV."""
    base = generate_ranima(frame)
    pixels = base.load()

    # Shift colors toward dark magenta/purple
    for x in range(base.width):
        for y in range(base.height):
            r, g, b, a = pixels[x, y]
            if a > 0:
                r = min(255, int(r * 0.6 + 80))
                g = int(g * 0.3)
                b = min(255, int(b * 0.5 + 60))
                pixels[x, y] = (r, g, b, a)

    # Add corruption tendrils
    draw = ImageDraw.Draw(base)
    for i in range(8):
        angle = i * 45 + frame * 10
        rad = math.radians(angle)
        sx, sy = 64, 40
        ex = sx + int(35 * math.cos(rad))
        ey = sy + int(35 * math.sin(rad))
        draw.line([(sx, sy), (ex, ey)], fill=(139, 0, 88, 150), width=2)
        # Tendril tips
        draw.ellipse([ex-3, ey-3, ex+3, ey+3], fill=(75, 0, 130, 120))

    return base


if __name__ == "__main__":
    print("Generating character sprite sheets...")
    print("Style: Assamese Puthi manuscript painting")
    print()

    generate_sprite_sheet(generate_tejimola_child, "tejimola_child", frames=8)
    generate_sprite_sheet(generate_tejimola_spirit, "tejimola_spirit", frames=8)
    generate_sprite_sheet(generate_dom, "dom", frames=8)
    generate_sprite_sheet(generate_ranima, "ranima", frames=8)
    generate_sprite_sheet(generate_father, "father", frames=8)
    generate_sprite_sheet(generate_ranima_corrupted, "ranima_corrupted", frames=8)

    print("\nAll character sprites generated!")
