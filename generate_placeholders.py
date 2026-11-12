import zlib
import struct
import math
import os

def write_png(filename, width, height, pixels):
    raw_data = bytearray()
    for row in pixels:
        raw_data.append(0)  # filter type 0 (None)
        for r, g, b in row:
            raw_data.extend((int(r), int(g), int(b)))
    
    compressed = zlib.compress(bytes(raw_data), 9)
    
    png = bytearray(b'\x89PNG\r\n\x1a\n')
    ihdr_data = struct.pack('>IIBBBBB', width, height, 8, 2, 0, 0, 0)
    ihdr_crc = zlib.crc32(b'IHDR' + ihdr_data)
    png.extend(struct.pack('>I', 13) + b'IHDR' + ihdr_data + struct.pack('>I', ihdr_crc))
    idat_crc = zlib.crc32(b'IDAT' + compressed)
    png.extend(struct.pack('>I', len(compressed)) + b'IDAT' + compressed + struct.pack('>I', idat_crc))
    iend_crc = zlib.crc32(b'IEND')
    png.extend(struct.pack('>I', 0) + b'IEND' + struct.pack('>I', iend_crc))
    
    os.makedirs(os.path.dirname(filename), exist_ok=True)
    with open(filename, 'wb') as f:
        f.write(png)

def hex_to_rgb(hex_str):
    hex_str = hex_str.lstrip('#')
    return tuple(int(hex_str[i:i+2], 16) for i in (0, 2, 4))

def make_gradient(filename, width, height, color1_hex, color2_hex):
    c1 = hex_to_rgb(color1_hex)
    c2 = hex_to_rgb(color2_hex)
    rows = []
    for y in range(height):
        t = y / max(1, height - 1)
        r = c1[0] * (1 - t) + c2[0] * t
        g = c1[1] * (1 - t) + c2[1] * t
        b = c1[2] * (1 - t) + c2[2] * t
        row = [(r, g, b)] * width
        rows.append(row)
    write_png(filename, width, height, rows)
    print(f"Created gradient {filename} ({width}x{height})")

def make_styled_asset(filename, width, height, bg_hex, border_hex, accent_hex, pattern='frame'):
    bg = hex_to_rgb(bg_hex)
    border = hex_to_rgb(border_hex)
    accent = hex_to_rgb(accent_hex)
    rows = []
    bw = 4
    for y in range(height):
        row = []
        for x in range(width):
            if x < bw or x >= width - bw or y < bw or y >= height - bw:
                row.append(border)
            elif (x == bw + 6 or x == width - bw - 7 or y == bw + 6 or y == height - bw - 7) and (x > bw + 5 and x < width - bw - 6 and y > bw + 5 and y < height - bw - 6):
                row.append(accent)
            else:
                dx = (x - width/2) / (width/2)
                dy = (y - height/2) / (height/2)
                dist = math.sqrt(dx*dx + dy*dy)
                factor = 1.0 - 0.15 * min(1.0, dist)
                
                is_grid = False
                if pattern == 'vines' and (y % 16 in (0, 1) or (x + y) % 32 in (0, 1)):
                    is_grid = True
                elif pattern == 'hearth' and (y > height * 0.75 and ((x * 7 + y * 3) % 43 < 3)):
                    is_grid = True

                if is_grid:
                    r = min(255, max(0, int(accent[0] * 0.7 + bg[0] * 0.3)))
                    g = min(255, max(0, int(accent[1] * 0.7 + bg[1] * 0.3)))
                    b = min(255, max(0, int(accent[2] * 0.7 + bg[2] * 0.3)))
                else:
                    r = min(255, max(0, int(bg[0] * factor)))
                    g = min(255, max(0, int(bg[1] * factor)))
                    b = min(255, max(0, int(bg[2] * factor)))
                row.append((r, g, b))
        rows.append(row)
    write_png(filename, width, height, rows)
    print(f"Created styled asset {filename} ({width}x{height})")

if __name__ == '__main__':
    # Palette colors:
    # 1. #9C2F2F (Spiced Cranberry)
    # 2. #B6562A (Warm Terracotta Rust)
    # 3. #DBBA33 (Harvest Gold / Amber)
    # 4. #633F21 (Warm Roasted Chestnut)
    # 5. #511F16 (Deep Espresso Mahogany)
    # Parchment canvas: #EADBC8 (Linen) & #DFCBB2 (Wheat)

    # Color Transition Fades
    make_gradient('assets/fade-linen-hearth.jpg', 600, 90, '#EADBC8', '#511F16')
    make_gradient('assets/fade-hearth-linen.jpg', 600, 90, '#511F16', '#EADBC8')
    make_gradient('assets/fade-linen-wheat.jpg', 600, 50, '#EADBC8', '#DFCBB2')
    make_gradient('assets/fade-wheat-linen.jpg', 600, 50, '#DFCBB2', '#EADBC8')
    make_gradient('assets/fade-wheat-hearth.jpg', 600, 90, '#DFCBB2', '#511F16')
    make_gradient('assets/fade-sand-night.jpg', 600, 90, '#EADBC8', '#511F16')
    make_gradient('assets/fade-night-sand.jpg', 600, 90, '#511F16', '#EADBC8')
    make_gradient('assets/fade-sand-tan.jpg', 600, 50, '#EADBC8', '#DFCBB2')
    make_gradient('assets/fade-tan-sand.jpg', 600, 50, '#DFCBB2', '#EADBC8')
    make_gradient('assets/fade-sand-foot.jpg', 600, 90, '#DFCBB2', '#511F16')

    # Banners and cards
    make_styled_asset('assets/banner-autumn-vines.png', 600, 52, '#EADBC8', '#9C2F2F', '#DBBA33', pattern='vines')
    make_styled_asset('assets/hero-thanksgiving.jpg', 600, 700, '#DFCBB2', '#9C2F2F', '#B6562A')
    make_styled_asset('assets/dinner-bell-clock.jpg', 552, 310, '#511F16', '#DBBA33', '#B6562A', pattern='hearth')
    make_styled_asset('assets/p-host.jpg', 260, 320, '#EADBC8', '#9C2F2F', '#DBBA33')
    make_styled_asset('assets/p-carry.jpg', 260, 320, '#EADBC8', '#B6562A', '#DBBA33')
    make_styled_asset('assets/p-keeper.jpg', 260, 320, '#EADBC8', '#9C2F2F', '#B6562A')
    make_styled_asset('assets/p-loaf.jpg', 260, 320, '#EADBC8', '#B6562A', '#DBBA33')
    make_styled_asset('assets/t-harvest.png', 552, 125, '#EADBC8', '#9C2F2F', '#DBBA33')
    make_styled_asset('assets/t-loaf.png', 552, 150, '#DFCBB2', '#B6562A', '#DBBA33')
    make_styled_asset('assets/pantry-thanksgiving.jpg', 552, 700, '#DFCBB2', '#9C2F2F', '#DBBA33')
    make_styled_asset('assets/harvest-footer.jpg', 600, 210, '#511F16', '#DBBA33', '#B6562A', pattern='hearth')

    print("All assets updated with the new 5-color palette!")
