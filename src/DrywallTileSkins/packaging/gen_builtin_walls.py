"""Generate seamless Klei-style cartoon wallpaper tiles for Drywall Tile Skins."""
from __future__ import annotations

import json
import math
from pathlib import Path

from PIL import Image, ImageDraw

SIZE = 512
OUT = Path(__file__).resolve().parent / "builtin_walls"

# Klei-ish colony palette
INK = (28, 24, 22, 255)
WHITE = (248, 246, 240, 255)
CREAM = (255, 243, 214, 255)
PINK = (255, 158, 186, 255)
LEMON = (255, 221, 86, 255)
KELLY = (126, 196, 74, 255)
MINT = (186, 228, 164, 255)
COBALT = (86, 156, 224, 255)
SKY = (186, 220, 246, 255)
SATSUMA = (255, 138, 58, 255)
ROSE = (226, 92, 118, 255)
MUSH = (110, 168, 96, 255)
CHARCOAL = (62, 64, 70, 255)
GRAPE = (168, 124, 214, 255)
LIME = (198, 220, 82, 255)
NAVY = (48, 86, 140, 255)


def new_img(color):
    return Image.new("RGBA", (SIZE, SIZE), color)


def wrap_xy(x, y):
    pts = []
    for dx in (0, SIZE, -SIZE):
        for dy in (0, SIZE, -SIZE):
            pts.append((x + dx, y + dy))
    return pts


def ellipse(draw, cx, cy, rx, ry, fill, outline=INK, width=6):
    for x, y in wrap_xy(cx, cy):
        box = [x - rx, y - ry, x + rx, y + ry]
        if outline and width:
            draw.ellipse(box, fill=outline)
            inner = [x - rx + width, y - ry + width, x + rx - width, y + ry - width]
            if inner[2] > inner[0] and inner[3] > inner[1]:
                draw.ellipse(inner, fill=fill)
        else:
            draw.ellipse(box, fill=fill)


def circle(draw, cx, cy, r, fill, outline=INK, width=6):
    ellipse(draw, cx, cy, r, r, fill, outline, width)


def polygon(draw, pts, fill, outline=INK, width=6):
    for dx in (0, SIZE, -SIZE):
        for dy in (0, SIZE, -SIZE):
            shifted = [(x + dx, y + dy) for x, y in pts]
            draw.polygon(shifted, fill=outline)
            if width <= 0:
                continue
            cx = sum(p[0] for p in pts) / len(pts)
            cy = sum(p[1] for p in pts) / len(pts)
            inset = []
            for x, y in pts:
                vx, vy = cx - x, cy - y
                n = math.hypot(vx, vy) or 1
                inset.append((x + vx / n * width, y + vy / n * width))
            draw.polygon([(p[0] + dx, p[1] + dy) for p in inset], fill=fill)


def line(draw, x0, y0, x1, y1, fill=INK, width=6):
    for dx in (0, SIZE, -SIZE):
        for dy in (0, SIZE, -SIZE):
            draw.line((x0 + dx, y0 + dy, x1 + dx, y1 + dy), fill=fill, width=width)


def rounded_rect(draw, x, y, w, h, r, fill, outline=INK, width=6):
    for px, py in wrap_xy(x, y):
        box = [px, py, px + w, py + h]
        draw.rounded_rectangle(box, radius=r, fill=outline)
        inner = [px + width, py + width, px + w - width, py + h - width]
        if inner[2] > inner[0] and inner[3] > inner[1]:
            draw.rounded_rectangle(inner, radius=max(1, r - width), fill=fill)


def star_pts(cx, cy, r_out, r_in, n=5, rot=-math.pi / 2):
    pts = []
    for i in range(n * 2):
        ang = rot + i * math.pi / n
        r = r_out if i % 2 == 0 else r_in
        pts.append((cx + math.cos(ang) * r, cy + math.sin(ang) * r))
    return pts


def heart_pts(cx, cy, s):
    pts = []
    for i in range(24):
        t = i / 24 * math.pi * 2
        x = s * 16 * math.sin(t) ** 3
        y = -s * (13 * math.cos(t) - 5 * math.cos(2 * t) - 2 * math.cos(3 * t) - math.cos(4 * t))
        pts.append((cx + x, cy + y))
    return pts


def save(img: Image.Image, stem: str, name: str):
    OUT.mkdir(parents=True, exist_ok=True)
    img.convert("RGB").save(OUT / f"{stem}.png", optimize=True)
    (OUT / f"{stem}.metadata.json").write_text(
        json.dumps({"Name": name}, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print("wrote", stem, name)


def wallpaper_dots():
    img = new_img(CREAM)
    d = ImageDraw.Draw(img)
    colors = [PINK, COBALT, KELLY, SATSUMA]
    i = 0
    for y in (48, 128, 208):
        for x in (48, 128, 208):
            circle(d, x, y, 28, colors[i % 4], width=7)
            i += 1
    save(img, "pastel_dots", "奶油波点")


def wallpaper_lemons():
    img = new_img(MINT)
    d = ImageDraw.Draw(img)
    for cx, cy in ((64, 64), (192, 64), (64, 192), (192, 192)):
        circle(d, cx, cy, 46, LEMON, width=7)
        circle(d, cx, cy, 14, CREAM, width=5)
        for k in range(8):
            a = k * math.pi / 4
            line(d, cx + math.cos(a) * 16, cy + math.sin(a) * 16, cx + math.cos(a) * 40, cy + math.sin(a) * 40, width=4)
    save(img, "citrus_slices", "柠檬片")


def wallpaper_stars():
    img = new_img(NAVY)
    d = ImageDraw.Draw(img)
    spots = [(64, 64, 34), (192, 64, 26), (64, 192, 26), (192, 192, 34), (128, 128, 22)]
    for x, y, r in spots:
        polygon(d, star_pts(x, y, r, r * 0.42), LEMON, width=6)
    save(img, "colony_stars", "星空")


def wallpaper_mushrooms():
    img = new_img(CREAM)
    d = ImageDraw.Draw(img)
    for cx, cy in ((64, 80), (192, 80), (64, 208), (192, 208)):
        rounded_rect(d, cx - 14, cy, 28, 36, 8, WHITE, width=5)
        ellipse(d, cx, cy, 42, 28, ROSE, width=6)
        circle(d, cx - 14, cy - 4, 7, WHITE, width=0)
        circle(d, cx + 12, cy + 6, 6, WHITE, width=0)
    save(img, "mush_caps", "蘑菇点")


def wallpaper_rain():
    img = new_img(SKY)
    d = ImageDraw.Draw(img)
    drops = [(40, 48), (112, 32), (184, 56), (72, 128), (160, 112), (232, 140), (48, 200), (128, 184), (208, 216)]
    for x, y in drops:
        polygon(d, [(x, y - 22), (x + 14, y + 10), (x - 14, y + 10)], COBALT, width=5)
        ellipse(d, x, y + 12, 14, 10, COBALT, width=5)
    save(img, "rain_drops", "雨滴")


def wallpaper_clouds():
    img = new_img(SKY)
    d = ImageDraw.Draw(img)
    for cx, cy in ((72, 80), (200, 72), (56, 200), (184, 192)):
        ellipse(d, cx, cy, 38, 22, WHITE, width=6)
        circle(d, cx - 22, cy + 4, 18, WHITE, width=6)
        circle(d, cx + 24, cy + 6, 16, WHITE, width=6)
        circle(d, cx + 4, cy - 14, 16, WHITE, width=6)
    save(img, "puffy_clouds", "云朵")


def wallpaper_leaves():
    img = new_img(CREAM)
    d = ImageDraw.Draw(img)
    for cx, cy, rot in ((64, 64, 0.4), (192, 64, -0.5), (64, 192, 1.1), (192, 192, -1.2)):
        pts = []
        for i in range(16):
            t = i / 16 * math.pi * 2
            rx = 18 + 22 * (1 + math.cos(t)) * 0.5
            ry = 32
            x = cx + math.cos(t + rot) * rx
            y = cy + math.sin(t + rot) * ry
            pts.append((x, y))
        polygon(d, pts, KELLY if (cx + cy) % 256 < 200 else MUSH, width=6)
        line(d, cx, cy - 20, cx, cy + 24, width=4)
    save(img, "leaf_scatter", "绿叶")


def wallpaper_honey():
    img = new_img(CREAM)
    d = ImageDraw.Draw(img)
    r = 36

    def hex_pts(cx, cy):
        return [(cx + r * math.cos(a), cy + r * math.sin(a)) for a in [i * math.pi / 3 for i in range(6)]]

    rows = [(64, 74), (192, 74), (128, 128), (64, 182), (192, 182)]
    for i, (x, y) in enumerate(rows):
        polygon(d, hex_pts(x, y), LEMON if i % 2 == 0 else SATSUMA, width=7)
    save(img, "honeycomb", "蜂巢")


def wallpaper_waves():
    img = new_img(SKY)
    d = ImageDraw.Draw(img)
    for band, col in ((40, COBALT), (100, NAVY), (160, COBALT), (220, NAVY)):
        pts = []
        for x in range(0, SIZE + 8, 8):
            y = band + math.sin(x / 28) * 14
            pts.append((x, y))
        for i in range(len(pts) - 1):
            line(d, pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], fill=col, width=10)
        # wrap the last-to-first seam
        line(d, pts[-1][0], pts[-1][1], pts[0][0] + SIZE, pts[0][1], fill=col, width=10)
    save(img, "wavy_sea", "海浪")


def wallpaper_hearts():
    img = new_img(PINK)
    d = ImageDraw.Draw(img)
    for cx, cy, s in ((64, 64, 1.15), (192, 64, 0.95), (64, 192, 0.95), (192, 192, 1.15), (128, 128, 0.7)):
        polygon(d, heart_pts(cx, cy, s), ROSE, width=6)
    save(img, "candy_hearts", "爱心")


def wallpaper_bricks():
    img = new_img(CREAM)
    d = ImageDraw.Draw(img)
    bw, bh = 84, 48
    colors = [SATSUMA, ROSE, PINK, (210, 126, 86, 255)]
    i = 0
    for row, y in enumerate((-8, 40, 88, 136, 184, 232)):
        ox = 0 if row % 2 == 0 else -42
        x = ox
        while x < SIZE + bw:
            rounded_rect(d, x + 4, y + 4, bw - 8, bh - 8, 8, colors[i % 4], width=5)
            i += 1
            x += bw
    save(img, "cartoon_bricks", "卡通砖")


def wallpaper_plus():
    img = new_img(WHITE)
    d = ImageDraw.Draw(img)
    for cx, cy, col in ((64, 64, COBALT), (192, 64, KELLY), (64, 192, SATSUMA), (192, 192, ROSE), (128, 128, GRAPE)):
        rounded_rect(d, cx - 12, cy - 32, 24, 64, 8, col, width=5)
        rounded_rect(d, cx - 32, cy - 12, 64, 24, 8, col, width=5)
    save(img, "plus_grid", "十字格")


def wallpaper_fish():
    img = new_img(SKY)
    d = ImageDraw.Draw(img)
    for cx, cy, col, flip in ((70, 70, SATSUMA, 1), (198, 70, COBALT, -1), (70, 198, KELLY, 1), (198, 198, ROSE, -1)):
        ellipse(d, cx, cy, 34, 22, col, width=6)
        polygon(d, [(cx - 34 * flip, cy), (cx - 58 * flip, cy - 16), (cx - 58 * flip, cy + 16)], col, width=5)
        circle(d, cx + 16 * flip, cy - 4, 5, WHITE, width=3)
        circle(d, cx + 17 * flip, cy - 4, 2, INK, width=0)
    save(img, "little_fish", "小鱼")


def wallpaper_flowers():
    img = new_img(CREAM)
    d = ImageDraw.Draw(img)
    petal = [PINK, COBALT, SATSUMA, GRAPE]
    for i, (cx, cy) in enumerate(((64, 64), (192, 64), (64, 192), (192, 192))):
        col = petal[i]
        for k in range(6):
            a = k * math.pi / 3
            circle(d, cx + math.cos(a) * 22, cy + math.sin(a) * 22, 14, col, width=5)
        circle(d, cx, cy, 12, LEMON, width=5)
    save(img, "tiny_flowers", "小花")


def wallpaper_bubbles():
    img = new_img((70, 170, 190, 255))
    d = ImageDraw.Draw(img)
    for cx, cy, r in ((56, 60, 28), (140, 44, 16), (210, 88, 24), (48, 150, 18), (128, 140, 32), (212, 176, 20), (80, 220, 22), (176, 228, 16)):
        circle(d, cx, cy, r, SKY, width=6)
        circle(d, cx - r * 0.28, cy - r * 0.28, max(4, r * 0.18), WHITE, width=0)
    save(img, "aqua_bubbles", "气泡")


def wallpaper_checkers():
    img = new_img(CREAM)
    d = ImageDraw.Draw(img)
    step = 64
    for y in range(0, SIZE, step):
        for x in range(0, SIZE, step):
            if ((x + y) // step) % 2 == 0:
                rounded_rect(d, x + 4, y + 4, step - 8, step - 8, 10, CHARCOAL, width=5)
            else:
                rounded_rect(d, x + 4, y + 4, step - 8, step - 8, 10, WHITE, width=5)
    save(img, "soft_checkers", "软格子")


def wallpaper_suns():
    img = new_img(SKY)
    d = ImageDraw.Draw(img)
    for cx, cy in ((64, 64), (192, 64), (64, 192), (192, 192)):
        for k in range(8):
            a = k * math.pi / 4 + 0.2
            line(d, cx + math.cos(a) * 28, cy + math.sin(a) * 28, cx + math.cos(a) * 46, cy + math.sin(a) * 46, fill=SATSUMA, width=8)
        circle(d, cx, cy, 26, LEMON, width=6)
    save(img, "little_suns", "小太阳")


def wallpaper_diamonds():
    img = new_img(GRAPE)
    d = ImageDraw.Draw(img)
    for cx, cy in ((64, 64), (192, 64), (128, 128), (64, 192), (192, 192)):
        polygon(d, [(cx, cy - 36), (cx + 28, cy), (cx, cy + 36), (cx - 28, cy)], LEMON if cx == 128 else PINK, width=6)
    save(img, "candy_diamonds", "菱形糖")


def main():
    wallpaper_dots()
    wallpaper_lemons()
    wallpaper_stars()
    wallpaper_mushrooms()
    wallpaper_rain()
    wallpaper_clouds()
    wallpaper_leaves()
    wallpaper_honey()
    wallpaper_waves()
    wallpaper_hearts()
    wallpaper_bricks()
    wallpaper_plus()
    wallpaper_fish()
    wallpaper_flowers()
    wallpaper_bubbles()
    wallpaper_checkers()
    wallpaper_suns()
    wallpaper_diamonds()


if __name__ == "__main__":
    main()
