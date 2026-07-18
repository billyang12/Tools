#!/usr/bin/env python3
"""
Convert SVG icon to ICO format with multiple sizes.
Requires: pip install pillow cairosvg
"""

import io
from PIL import Image
import cairosvg

def svg_to_ico(svg_path, ico_path):
    """Convert SVG to ICO with multiple sizes."""
    sizes = [16, 32, 48, 64, 128, 256]
    images = []

    with open(svg_path, 'rb') as f:
        svg_data = f.read()

    for size in sizes:
        # Convert SVG to PNG at specific size
        png_data = cairosvg.svg2png(
            bytestring=svg_data,
            output_width=size,
            output_height=size
        )

        # Load PNG into PIL Image
        img = Image.open(io.BytesIO(png_data))
        images.append(img)
        print(f"Generated {size}x{size} icon")

    # Save as ICO with all sizes
    images[0].save(
        ico_path,
        format='ICO',
        sizes=[(img.width, img.height) for img in images],
        append_images=images[1:]
    )
    print(f"\nSaved icon to: {ico_path}")

if __name__ == "__main__":
    svg_to_ico("icon.svg", "icon.ico")
