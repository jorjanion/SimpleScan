"""
SimpleScan Icon Generator
Generates high-resolution PNG (512x512) and multi-resolution ICO (16, 24, 32, 48, 64, 128, 256)
"""

from PIL import Image, ImageDraw, ImageFilter
import math
import os

def create_icon():
    size = 512
    im = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    
    # 1. Base squircle background with modern vibrant blue gradient
    bg = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw_bg = ImageDraw.Draw(bg)
    
    # Gradient: Royal Blue (#0052D4) -> Bright Cerulean (#4364F7) -> Electric Azure (#6FB1FC)
    # Vertical gradient map
    for y in range(size):
        ratio = y / size
        # Smooth interpolation
        r = int(0 + ratio * 20)
        g = int(100 + ratio * 30)
        b = int(220 + ratio * 35)
        # Saturated vibrant Windows blue
        draw_bg.line([(0, y), (size, y)], fill=(r, g, b, 255))
        
    # Mask rounded squircle
    mask = Image.new("L", (size, size), 0)
    draw_mask = ImageDraw.Draw(mask)
    margin = 24
    corner_radius = 110
    draw_mask.rounded_rectangle([margin, margin, size - margin, size - margin], radius=corner_radius, fill=255)
    
    # Apply mask
    bg_masked = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    bg_masked.paste(bg, (0, 0), mask=mask)
    im = Image.alpha_composite(im, bg_masked)
    
    # 2. Scanner Flatbed Base
    scanner_w = 340
    scanner_h = 240
    scanner_x = (size - scanner_w) // 2
    scanner_y = 170
    
    # Scanner body (Dark slate base)
    body_layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    body_draw = ImageDraw.Draw(body_layer)
    body_draw.rounded_rectangle(
        [scanner_x, scanner_y + 190, scanner_x + scanner_w, scanner_y + 250],
        radius=14,
        fill=(40, 48, 60, 255)
    )
    
    # Scanner glass / bed frame (Light glossy plate)
    body_draw.rounded_rectangle(
        [scanner_x + 10, scanner_y, scanner_x + scanner_w - 10, scanner_y + 205],
        radius=16,
        fill=(240, 244, 250, 245),
        outline=(210, 220, 235, 255),
        width=3
    )
    im = Image.alpha_composite(im, body_layer)
    
    # 3. Document Paper Sheet resting on the bed
    doc_w = 250
    doc_h = 230
    doc_x = (size - doc_w) // 2
    doc_y = 110
    
    # Document drop shadow
    shadow_layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    shadow_draw = ImageDraw.Draw(shadow_layer)
    shadow_draw.rounded_rectangle([doc_x, doc_y + 6, doc_x + doc_w, doc_y + doc_h + 6], radius=10, fill=(0, 0, 0, 80))
    shadow_layer = shadow_layer.filter(ImageFilter.GaussianBlur(10))
    im = Image.alpha_composite(im, shadow_layer)
    
    # Document white paper
    doc_layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    doc_draw = ImageDraw.Draw(doc_layer)
    doc_draw.rounded_rectangle([doc_x, doc_y, doc_x + doc_w, doc_y + doc_h], radius=10, fill=(255, 255, 255, 255))
    
    # Dog-ear / folded corner top right
    fold_size = 32
    doc_draw.polygon(
        [(doc_x + doc_w - fold_size, doc_y), (doc_x + doc_w, doc_y + fold_size), (doc_x + doc_w, doc_y)],
        fill=(235, 240, 248, 255)
    )
    
    # Document lines (simulating clean scanned text)
    # Header bar (blue accent)
    doc_draw.rounded_rectangle([doc_x + 30, doc_y + 30, doc_x + 120, doc_y + 42], radius=6, fill=(59, 130, 246, 255))
    
    # Text lines (subtle slate grey)
    lines_y = [60, 90, 120, 150]
    widths = [190, 190, 190, 115]
    for ly, lw in zip(lines_y, widths):
        doc_draw.rounded_rectangle([doc_x + 30, doc_y + ly, doc_x + 30 + lw, doc_y + ly + 10], radius=5, fill=(160, 175, 195, 255))
        
    im = Image.alpha_composite(im, doc_layer)
    
    # 4. Glowing Cyan Laser Scan Beam
    laser_y = 220
    beam_layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    beam_draw = ImageDraw.Draw(beam_layer)
    
    # Outer glow
    beam_draw.line([(scanner_x + 2, laser_y), (scanner_x + scanner_w - 2, laser_y)], fill=(0, 229, 255, 255), width=18)
    beam_layer = beam_layer.filter(ImageFilter.GaussianBlur(8))
    
    # Mid glow
    beam_mid = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    beam_mid_draw = ImageDraw.Draw(beam_mid)
    beam_mid_draw.line([(scanner_x + 2, laser_y), (scanner_x + scanner_w - 2, laser_y)], fill=(0, 240, 255, 255), width=8)
    beam_mid = beam_mid.filter(ImageFilter.GaussianBlur(3))
    
    # Core sharp bright white-cyan laser line
    beam_core = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    beam_core_draw = ImageDraw.Draw(beam_core)
    beam_core_draw.line([(scanner_x - 8, laser_y), (scanner_x + scanner_w + 8, laser_y)], fill=(255, 255, 255, 255), width=4)
    
    im = Image.alpha_composite(im, beam_layer)
    im = Image.alpha_composite(im, beam_mid)
    im = Image.alpha_composite(im, beam_core)
    
    # Save PNG
    os.makedirs("assets", exist_ok=True)
    im.save("assets/icon.png", format="PNG")
    print("Saved assets/icon.png (512x512)")
    
    # Save multi-size ICO
    icon_sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (24, 24), (16, 16)]
    im.save("assets/icon.ico", format="ICO", sizes=icon_sizes)
    print("Saved assets/icon.ico (all resolutions)")

if __name__ == "__main__":
    create_icon()
