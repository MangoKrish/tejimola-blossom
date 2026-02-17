#!/usr/bin/env python3
"""
Generate parallax background layers for each act.
Assamese Puthi painting style with 4 layers per scene.
Layer 1: Foreground (foliage, decorative)
Layer 2: Mid-ground (buildings, structures)
Layer 3: Background (distant scenery)
Layer 4: Sky (atmospheric)
"""
from PIL import Image, ImageDraw, ImageFilter
import os
import math
import random

OUTPUT_BASE = "/Users/krishnas/Desktop/game/TejimolaBlossom/Assets/_Project/Art/Backgrounds"

# Consistent palette
PALETTE = {
    'act1': {
        'sky': (135, 206, 235),
        'sky_gradient': (255, 223, 140),
        'ground': (139, 119, 42),
        'foliage': (34, 139, 34),
        'foliage_light': (50, 205, 50),
        'building': (139, 69, 19),
        'building_light': (210, 180, 140),
        'water': (70, 130, 180),
        'gold_accent': (255, 215, 0),
    },
    'act2': {
        'sky': (105, 105, 105),
        'sky_gradient': (60, 60, 80),
        'ground': (80, 70, 60),
        'foliage': (40, 60, 40),
        'foliage_light': (60, 80, 50),
        'building': (90, 60, 40),
        'building_light': (140, 120, 100),
        'water': (50, 70, 90),
        'gold_accent': (139, 119, 42),
    },
    'act3': {
        'sky': (40, 30, 60),
        'sky_gradient': (75, 0, 130),
        'ground': (60, 50, 70),
        'foliage': (20, 40, 30),
        'foliage_light': (40, 60, 50),
        'building': (70, 50, 60),
        'building_light': (100, 80, 90),
        'water': (40, 50, 80),
        'gold_accent': (100, 149, 237),
    },
    'act4': {
        'sky': (30, 0, 40),
        'sky_gradient': (139, 0, 88),
        'ground': (40, 20, 30),
        'foliage': (30, 10, 20),
        'foliage_light': (60, 20, 40),
        'building': (50, 30, 40),
        'building_light': (80, 50, 60),
        'water': (30, 20, 50),
        'gold_accent': (220, 20, 60),
    },
    'epilogue': {
        'sky': (255, 200, 150),
        'sky_gradient': (255, 140, 100),
        'ground': (139, 119, 42),
        'foliage': (50, 180, 50),
        'foliage_light': (100, 220, 100),
        'building': (160, 120, 80),
        'building_light': (220, 200, 170),
        'water': (100, 180, 220),
        'gold_accent': (255, 215, 0),
    },
}

WIDTH = 1920
HEIGHT = 1080


def gradient_fill(draw, bbox, top_color, bottom_color):
    """Fill a rectangle with vertical gradient."""
    x0, y0, x1, y1 = bbox
    for y in range(y0, y1):
        t = (y - y0) / max(1, (y1 - y0))
        r = int(top_color[0] + (bottom_color[0] - top_color[0]) * t)
        g = int(top_color[1] + (bottom_color[1] - top_color[1]) * t)
        b = int(top_color[2] + (bottom_color[2] - top_color[2]) * t)
        draw.line([(x0, y), (x1, y)], fill=(r, g, b))


def draw_tree(draw, x, y, scale, foliage_color, trunk_color, style='normal'):
    """Draw a tree in Puthi painting style."""
    s = scale
    # Trunk
    draw.rectangle([x-int(3*s), y, x+int(3*s), y+int(30*s)], fill=trunk_color, outline=(0,0,0), width=2)

    # Foliage (layered circles)
    if style == 'nahor':
        # Nahor tree - large, spreading, with flowers
        for i in range(5):
            ox = int((i-2) * 12 * s)
            oy = int(-i * 3 * s)
            draw.ellipse([x+ox-int(18*s), y-int(20*s)+oy, x+ox+int(18*s), y+int(5*s)+oy],
                        fill=foliage_color, outline=(0,0,0), width=2)
        # Nahor blossoms
        for i in range(8):
            bx = x + random.randint(int(-15*s), int(15*s))
            by = y - random.randint(int(5*s), int(18*s))
            draw.ellipse([bx-3, by-3, bx+3, by+3], fill=(255, 255, 255))
            draw.ellipse([bx-1, by-1, bx+1, by+1], fill=(255, 215, 0))
    elif style == 'bamboo':
        # Bamboo cluster
        for i in range(3):
            bx = x + (i-1) * int(6*s)
            draw.rectangle([bx-int(2*s), y-int(40*s), bx+int(2*s), y+int(30*s)],
                          fill=(80, 140, 60), outline=(0,0,0), width=1)
            # Nodes
            for j in range(4):
                ny = y - int(j*10*s)
                draw.line([(bx-int(2*s), ny), (bx+int(2*s), ny)], fill=(40,80,30), width=2)
            # Leaves
            for j in range(3):
                ly = y - int(j*12*s)
                draw.line([(bx, ly), (bx+int(15*s), ly-int(5*s))], fill=foliage_color, width=2)
                draw.line([(bx, ly), (bx-int(12*s), ly-int(4*s))], fill=foliage_color, width=2)
    else:
        # Standard tree
        draw.ellipse([x-int(15*s), y-int(25*s), x+int(15*s), y+int(5*s)],
                    fill=foliage_color, outline=(0,0,0), width=2)
        draw.ellipse([x-int(10*s), y-int(30*s), x+int(10*s), y-int(5*s)],
                    fill=foliage_color, outline=(0,0,0), width=2)


def draw_house_assamese(draw, x, y, w, h, wall_color, roof_color, detail_color):
    """Draw an Assamese traditional house (Chang ghar style)."""
    # Stilts
    for i in range(3):
        sx = x + int(w * (i+1) / 4)
        draw.rectangle([sx-3, y+h, sx+3, y+h+30], fill=(80, 60, 40), outline=(0,0,0), width=1)

    # Main structure (raised)
    draw.rectangle([x, y, x+w, y+h], fill=wall_color, outline=(0,0,0), width=3)

    # Bamboo/wood texture lines
    for i in range(0, w, 15):
        draw.line([(x+i, y), (x+i, y+h)], fill=(wall_color[0]-20, wall_color[1]-20, wall_color[2]-20), width=1)

    # Thatched/tin roof (triangular)
    roof_points = [
        (x-10, y),
        (x+w+10, y),
        (x+w//2, y-h//2),
    ]
    draw.polygon(roof_points, fill=roof_color, outline=(0,0,0))
    draw.line([roof_points[0], roof_points[2]], fill=(0,0,0), width=3)
    draw.line([roof_points[1], roof_points[2]], fill=(0,0,0), width=3)
    draw.line([roof_points[0], roof_points[1]], fill=(0,0,0), width=3)

    # Window
    wx = x + w//3
    wy = y + h//4
    draw.rectangle([wx, wy, wx+w//5, wy+h//3], fill=detail_color, outline=(0,0,0), width=2)
    draw.line([(wx+w//10, wy), (wx+w//10, wy+h//3)], fill=(0,0,0), width=2)

    # Door
    dx = x + w*2//3 - 5
    dy = y + h//3
    draw.rectangle([dx, dy, dx+w//6, y+h], fill=detail_color, outline=(0,0,0), width=2)


def draw_river(draw, y, width, height, color):
    """Draw flowing river/water."""
    for i in range(0, width, 4):
        wave = int(5 * math.sin(i * 0.05))
        c = (color[0] + random.randint(-10, 10),
             color[1] + random.randint(-10, 10),
             color[2] + random.randint(-10, 10))
        draw.line([(i, y+wave), (i, y+height+wave)], fill=c)


def generate_sky_layer(act_name, palette):
    """Layer 4: Sky with clouds, sun/moon."""
    img = Image.new('RGBA', (WIDTH, HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gradient sky
    gradient_fill(draw, (0, 0, WIDTH, HEIGHT), palette['sky'], palette['sky_gradient'])

    # Sun/moon
    if act_name in ['act1', 'epilogue']:
        # Golden sun
        sx, sy = WIDTH - 300, 150
        draw.ellipse([sx-60, sy-60, sx+60, sy+60], fill=(255, 200, 50))
        # Rays
        for angle in range(0, 360, 20):
            rad = math.radians(angle)
            ex = sx + int(90 * math.cos(rad))
            ey = sy + int(90 * math.sin(rad))
            draw.line([(sx, sy), (ex, ey)], fill=(255, 215, 0, 100), width=2)
    elif act_name in ['act3', 'act4']:
        # Moon
        mx, my = WIDTH - 250, 120
        draw.ellipse([mx-40, my-40, mx+40, my+40], fill=(200, 200, 220))
        draw.ellipse([mx-30, my-35, mx+10, my+35], fill=palette['sky'])  # Crescent

    # Clouds
    num_clouds = 5 if act_name in ['act1', 'epilogue'] else 3
    for i in range(num_clouds):
        cx = random.randint(100, WIDTH-200)
        cy = random.randint(50, 250)
        cloud_color = (255, 255, 255, 120) if act_name in ['act1', 'epilogue'] else (100, 100, 120, 80)
        for j in range(4):
            ox = j * 25 - 35
            oy = random.randint(-10, 10)
            draw.ellipse([cx+ox-25, cy+oy-15, cx+ox+25, cy+oy+15], fill=cloud_color)

    return img


def generate_background_layer(act_name, palette):
    """Layer 3: Distant scenery - hills, mountains, river."""
    img = Image.new('RGBA', (WIDTH, HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Distant hills
    hill_color = tuple(int(c * 0.6) for c in palette['foliage'])
    for i in range(3):
        hx = i * 700
        hy = HEIGHT - 400 + i * 30
        points = []
        for x in range(hx - 100, hx + 800, 10):
            y = hy - int(80 * math.sin((x - hx) * 0.005)) - random.randint(0, 20)
            points.append((x, y))
        points.append((hx + 800, HEIGHT))
        points.append((hx - 100, HEIGHT))
        draw.polygon(points, fill=hill_color)

    # Brahmaputra river (distant)
    river_y = HEIGHT - 350
    draw_river(draw, river_y, WIDTH, 60, palette['water'])

    # Distant trees
    for i in range(15):
        tx = random.randint(0, WIDTH)
        ty = HEIGHT - random.randint(280, 380)
        draw_tree(draw, tx, ty, 0.4, hill_color, (60, 40, 20))

    return img


def generate_midground_layer(act_name, palette):
    """Layer 2: Buildings, main structures."""
    img = Image.new('RGBA', (WIDTH, HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Ground
    draw.rectangle([0, HEIGHT-200, WIDTH, HEIGHT], fill=palette['ground'])

    # Houses
    if act_name in ['act1', 'act2']:
        # Main house (Tejimola's home)
        draw_house_assamese(draw, 600, HEIGHT-380, 300, 150,
                           palette['building_light'], palette['building'],
                           palette['gold_accent'])

        # Neighboring house
        draw_house_assamese(draw, 1200, HEIGHT-340, 200, 120,
                           palette['building_light'], palette['building'],
                           palette['gold_accent'])

    elif act_name == 'act3':
        # Ruined house
        draw_house_assamese(draw, 600, HEIGHT-370, 300, 150,
                           palette['building_light'], palette['building'],
                           palette['gold_accent'])
        # Overgrown vines
        for i in range(10):
            vx = 600 + random.randint(0, 300)
            vy = HEIGHT - 370 + random.randint(0, 150)
            draw.line([(vx, vy), (vx + random.randint(-20, 20), vy + random.randint(10, 30))],
                     fill=palette['foliage'], width=2)

    elif act_name == 'act4':
        # Surreal inverted structures
        draw_house_assamese(draw, 600, HEIGHT-300, 300, 150,
                           palette['building_light'], palette['building'],
                           palette['gold_accent'])
        # Inverted house (upside down)
        inv_y = 100
        draw.rectangle([650, inv_y, 850, inv_y+120], fill=palette['building_light'], outline=(0,0,0), width=2)
        inv_roof = [(640, inv_y+120), (860, inv_y+120), (750, inv_y+180)]
        draw.polygon(inv_roof, fill=palette['building'], outline=(0,0,0))

    # Trees in midground
    for i in range(5):
        tx = random.randint(50, WIDTH-100)
        ty = HEIGHT - random.randint(200, 280)
        style = 'nahor' if i == 2 else ('bamboo' if i == 4 else 'normal')
        draw_tree(draw, tx, ty, 0.7, palette['foliage'], palette['building'], style)

    # Path/road
    for x in range(0, WIDTH, 3):
        py = HEIGHT - 180 + int(5 * math.sin(x * 0.01))
        draw.line([(x, py), (x, py+15)], fill=(palette['ground'][0]+30, palette['ground'][1]+20, palette['ground'][2]+10))

    return img


def generate_foreground_layer(act_name, palette):
    """Layer 1: Foreground foliage, decorative elements."""
    img = Image.new('RGBA', (WIDTH, HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Foreground foliage (bottom)
    for i in range(20):
        fx = random.randint(-50, WIDTH+50)
        fy = HEIGHT - random.randint(0, 100)
        # Grass/plants
        for j in range(5):
            gx = fx + random.randint(-15, 15)
            gy = fy
            gh = random.randint(20, 50)
            draw.line([(gx, gy), (gx + random.randint(-8, 8), gy - gh)],
                     fill=palette['foliage_light'], width=2)

    # Foreground flowers
    if act_name in ['act1', 'epilogue']:
        for i in range(12):
            fx = random.randint(0, WIDTH)
            fy = HEIGHT - random.randint(20, 80)
            # Flower
            flower_colors = [(255,255,255), (255,200,50), (255,150,150), (200,150,255)]
            fc = random.choice(flower_colors)
            for p in range(5):
                angle = p * 72
                rad = math.radians(angle)
                px = fx + int(5 * math.cos(rad))
                py = fy + int(5 * math.sin(rad))
                draw.ellipse([px-3, py-3, px+3, py+3], fill=fc)
            draw.ellipse([fx-2, fy-2, fx+2, fy+2], fill=(255, 215, 0))

    # Fireflies/particles for night scenes
    if act_name in ['act3', 'act4']:
        for i in range(20):
            px = random.randint(0, WIDTH)
            py = random.randint(100, HEIGHT-100)
            size = random.randint(1, 3)
            alpha = random.randint(80, 200)
            draw.ellipse([px-size, py-size, px+size, py+size],
                        fill=(200, 200, 255, alpha))

    # Ornamental border (Puthi style) at bottom
    border_y = HEIGHT - 10
    for x in range(0, WIDTH, 16):
        draw.ellipse([x-3, border_y-3, x+3, border_y+3], fill=palette['gold_accent'])
        draw.line([(x, border_y-5), (x+8, border_y-5)], fill=palette['gold_accent'], width=1)

    # Side foliage overlap
    for side in [0, WIDTH-80]:
        for i in range(3):
            ly = random.randint(200, HEIGHT-200)
            leaf_points = [
                (side, ly),
                (side + (40 if side == 0 else -40), ly-15),
                (side + (60 if side == 0 else -60), ly),
                (side + (40 if side == 0 else -40), ly+15),
            ]
            draw.polygon(leaf_points, fill=palette['foliage'], outline=(0,0,0,100))

    return img


def generate_all_layers(act_name):
    """Generate all 4 parallax layers for an act."""
    palette = PALETTE[act_name]
    act_dir = os.path.join(OUTPUT_BASE, act_name.replace('act', 'Act').replace('epilogue', 'Epilogue'))
    os.makedirs(act_dir, exist_ok=True)

    random.seed(42 + hash(act_name))  # Consistent random per act

    layers = {
        'layer4_sky': generate_sky_layer(act_name, palette),
        'layer3_background': generate_background_layer(act_name, palette),
        'layer2_midground': generate_midground_layer(act_name, palette),
        'layer1_foreground': generate_foreground_layer(act_name, palette),
    }

    for name, img in layers.items():
        path = os.path.join(act_dir, f"{name}.png")
        img.save(path)
        print(f"Generated: {path}")

    # Also generate a composite preview
    composite = Image.new('RGBA', (WIDTH, HEIGHT), (0, 0, 0, 255))
    for name in ['layer4_sky', 'layer3_background', 'layer2_midground', 'layer1_foreground']:
        composite = Image.alpha_composite(composite, layers[name])
    preview_path = os.path.join(act_dir, "preview_composite.png")
    composite.save(preview_path)
    print(f"Generated preview: {preview_path}")


if __name__ == "__main__":
    print("Generating parallax backgrounds for all acts...")
    print("Style: Assamese Puthi manuscript with modern execution")
    print()

    for act in ['act1', 'act2', 'act3', 'act4', 'epilogue']:
        print(f"\n--- {act.upper()} ---")
        generate_all_layers(act)

    print("\nAll backgrounds generated!")
