#!/usr/bin/env python3
"""Generate placeholder PNG sprites for Rifthold.

tModLoader refuses to load an item, NPC, or tile whose texture is missing, so
the mod needs a PNG beside every content class before it will even start. These
are deliberately ugly flat-colour blocks with frame separators drawn in, so it
is obvious at a glance which art has not been made yet.

Run from the repository root:

    python3 tools/make_placeholder_sprites.py

Re-running overwrites everything, so delete this script's output (or just stop
running it) once real sprites exist. Pass --force to overwrite files that have
already been replaced with real art.
"""

from __future__ import annotations

import argparse
import os
import struct
import zlib

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), os.pardir)
MOD = os.path.join(ROOT, "Rifthold")


def write_png(path: str, width: int, height: int, pixels: bytearray) -> None:
    """Write RGBA pixel data as a PNG. pixels is width*height*4 bytes."""
    stride = width * 4
    raw = bytearray()
    for y in range(height):
        raw.append(0)  # filter type 0 (None) per scanline
        raw.extend(pixels[y * stride:(y + 1) * stride])

    def chunk(tag: bytes, data: bytes) -> bytes:
        return (
            struct.pack(">I", len(data))
            + tag
            + data
            + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)
        )

    header = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)
    blob = (
        b"\x89PNG\r\n\x1a\n"
        + chunk(b"IHDR", header)
        + chunk(b"IDAT", zlib.compress(bytes(raw), 9))
        + chunk(b"IEND", b"")
    )

    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as handle:
        handle.write(blob)


def placeholder(
    width: int,
    height: int,
    colour: tuple[int, int, int],
    frames: int = 1,
    margin: int = 0,
) -> bytearray:
    """Flat colour with a darker border, a diagonal stripe, and frame splits."""
    r, g, b = colour
    dark = (max(r - 70, 0), max(g - 70, 0), max(b - 70, 0))
    light = (min(r + 60, 255), min(g + 60, 255), min(b + 60, 255))

    pixels = bytearray(width * height * 4)
    frame_height = height // frames

    for y in range(height):
        for x in range(width):
            in_margin = (
                x < margin
                or y % frame_height < margin
                or x >= width - margin
                or y % frame_height >= frame_height - margin
            )
            if in_margin:
                continue  # leave fully transparent

            local_y = y % frame_height
            on_border = (
                x == margin
                or local_y == margin
                or x == width - 1 - margin
                or local_y == frame_height - 1 - margin
            )
            on_stripe = (x + local_y) % 8 == 0

            if on_border:
                colour_out = dark
            elif on_stripe:
                colour_out = light
            else:
                colour_out = (r, g, b)

            index = (y * width + x) * 4
            pixels[index] = colour_out[0]
            pixels[index + 1] = colour_out[1]
            pixels[index + 2] = colour_out[2]
            pixels[index + 3] = 255

    return pixels


# path relative to the Rifthold mod folder -> (width, height, rgb, frames, margin)
SPRITES = {
    "icon.png": (80, 80, (120, 80, 190), 1, 0),
    "icon_small.png": (30, 30, (120, 80, 190), 1, 0),
    "Content/Items/RiftSigil.png": (32, 32, (150, 90, 220), 1, 2),
    "Content/Items/RiftEssence.png": (24, 24, (90, 190, 210), 1, 2),
    "Content/Items/RiftwovenBlade.png": (40, 40, (200, 170, 90), 1, 2),
    "Content/Items/Placeable/MapDeviceItem.png": (32, 32, (90, 140, 200), 1, 2),
    # Tile sheets are 16px tiles on an 18px pitch, so a 3x3 tile is 54x54.
    "Content/Tiles/MapDeviceTile.png": (54, 54, (90, 140, 200), 1, 0),
    # NPC sheets are one frame stacked above the next; frame height = height / frames.
    "Content/NPCs/RiftHusk.png": (40, 120, (110, 150, 110), 3, 2),
    "Content/NPCs/RiftStalker.png": (40, 120, (180, 110, 110), 3, 2),
    "Content/NPCs/RiftWarden.png": (80, 320, (170, 90, 200), 4, 2),
    "Content/NPCs/RiftWarden_Head_Boss.png": (30, 30, (170, 90, 200), 1, 2),
}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--force",
        action="store_true",
        help="overwrite sprites that already exist",
    )
    args = parser.parse_args()

    written, skipped = 0, 0
    for relative, (width, height, colour, frames, margin) in sorted(SPRITES.items()):
        target = os.path.join(MOD, relative.replace("/", os.sep))
        if os.path.exists(target) and not args.force:
            skipped += 1
            continue
        write_png(target, width, height, placeholder(width, height, colour, frames, margin))
        written += 1
        print(f"wrote {relative} ({width}x{height}, {frames} frame(s))")

    print(f"\n{written} written, {skipped} left alone (use --force to overwrite).")


if __name__ == "__main__":
    main()
