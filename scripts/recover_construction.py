#!/usr/bin/env python3
"""Recover missing original bitmaps from the two pinned Deuteros disk images.

No ROMs/disks are distributed. See docs/original-construction-artwork-evidence.md.
Uses the existing remake metal palette; preserves original indexed pixels.
"""
import argparse
import hashlib
from pathlib import Path
import struct
import zlib

ROOT = Path(__file__).resolve().parents[1]
ITEMS = [('pulse_blaster_laser', 9), ('g_chassis', 12), ('star_drive', 13),
         ('prejudice_torpedo_launcher', 26), ('star_drone', 29), ('prison_pod', 30), ('sonic_blaster', 31),
         ('s__d__m', 18), ('m__t__x', 23)]
SMALL = [('pulse_blaster_laser', 10), ('m__f__l', 25), ('prejudice_torpedo_launcher', 27),
         ('prison_pod', 31), ('sonic_blaster', 32)]


def decode(data, start, end):
    """Original $41C72 / $41D44, bounded to one independently sized bitmap."""
    total_width, header_height = struct.unpack_from('>HH', data, start)
    plane_major = header_height >= 200
    height = header_height & 255 if plane_major else header_height
    assert total_width % 4 == 0 and 0 < total_width <= 80 and 0 < height <= 200
    word_width = total_width // 4
    expected = total_width * height
    pos = start + 4
    words = []
    while len(words) < expected:
        assert pos < end
        command = data[pos]; pos += 1
        mode, count = command >> 6, command & 63
        if mode == 0:
            count = count or (256 if plane_major else 65536)
            count = min(count, expected - len(words))
            assert pos + count * 2 <= end
            words.extend(struct.unpack_from('>' + 'H' * count, data, pos)); pos += count * 2
        else:
            if mode == 1:
                assert pos < end
                value = data[pos] * 257; pos += 1
            else:
                if mode == 2:
                    assert pos < end
                    count = count * 256 + data[pos]; pos += 1
                assert pos + 2 <= end
                value = int.from_bytes(data[pos:pos + 2], 'little'); pos += 2
            count = count or 65536
            # The original returns as soon as the final row is full, even mid-run.
            words.extend([value] * min(count, expected - len(words)))
    assert 0 <= end - pos <= 1  # At most the independently indexed alignment byte.
    width = word_width * 16
    pixels = bytearray(width * height)
    for y in range(height):
        for xword in range(word_width):
            for plane in range(4):
                at = (plane * height + y) * word_width + xword if plane_major else y * total_width + xword * 4 + plane
                for bit in range(16):
                    pixels[y * width + xword * 16 + bit] |= ((words[at] >> (15 - bit)) & 1) << plane
    return width, height, pixels


def png(width, height, rgba):
    def chunk(kind, body):
        return struct.pack('>I', len(body)) + kind + body + struct.pack('>I', zlib.crc32(kind + body))
    scanlines = b''.join(b'\0' + rgba[y * width * 4:(y + 1) * width * 4] for y in range(height))
    return (b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0))
            + chunk(b'IDAT', zlib.compress(scanlines, 9)) + chunk(b'IEND', b''))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('disk1', type=Path)
    parser.add_argument('disk2', type=Path)
    parser.add_argument('--check', action='store_true', help='compare regenerated assets without writing')
    args = parser.parse_args()
    disk1, disk2 = args.disk1.read_bytes(), args.disk2.read_bytes()
    assert hashlib.sha256(disk1).hexdigest() == '6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38'
    assert hashlib.sha256(disk2).hexdigest() == '99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a'
    offset = lambda address: 0x6e000 + address - 0x13000
    word = lambda data, address: struct.unpack_from('>H', data, address)[0]
    palette = [tuple(((word(disk1, offset(0x1ed24) + i * 2) >> shift) & 15) * 16 for shift in (8, 4, 0)) for i in range(16)]
    # Match the established remake/source-sheet metal colours, verified against derrick RGB pixels.
    palette[:5] = [(0, 0, 0), (160, 160, 96), (128, 128, 64), (96, 96, 32), (64, 64, 0)]
    palette[5:8] = [tuple(((word(disk1, offset(0x410dc) + 6 + i * 2) >> shift) & 15) * 16 for shift in (8, 4, 0)) for i in range(3)]
    sheet = bytearray(192 * len(ITEMS) * 48 * 4)
    outputs = {}
    for row, (name, index) in enumerate(ITEMS):
        record = 0x29400 + word(disk2, 0x29400 + index * 2)
        record_end = 0x29400 + (word(disk2, 0x29400 + (index + 1) * 2) if index < 31 else 0xf200)
        assert disk2[record] == index
        pos = record + word(disk2, record + 2)
        for stage in range(3):
            size = word(disk2, pos)
            assert size > 0 and pos + 2 + size <= record_end
            width, height, pixels = decode(disk2, pos + 2, pos + 2 + size)
            assert (width, height) == (64, 48)
            for y in range(height):
                for x in range(width):
                    target = ((row * 48 + y) * 192 + stage * 64 + x) * 4
                    sheet[target:target + 4] = bytes((*palette[pixels[y * width + x]], 255))
            outputs[f'Production/{name}_{stage + 1}.tres'] = f'''[gd_resource type="AtlasTexture" load_steps=2 format=3]

[ext_resource type="Texture2D" path="res://Sprites/Items/Sheets/RecoveredConstruction.png" id="1"]

[resource]
atlas = ExtResource("1")
region = Rect2({stage * 64}, {row * 48}, 64, 48)
filter_clip = true
'''.encode()
            pos += 2 + size + size % 2
    outputs['Sheets/RecoveredConstruction.png'] = png(192, len(ITEMS) * 48, sheet)
    for name, item in SMALL:
        index = 77 + disk1[offset(0x4190a) + item]
        start = offset(0x422fa) + struct.unpack_from('>I', disk1, offset(0x41faa) + index * 4)[0]
        end = offset(0x422fa) + struct.unpack_from('>I', disk1, offset(0x41faa) + (index + 1) * 4)[0]
        width, height, pixels = decode(disk1, start, end)
        assert width == 48 and height <= 44
        rgba = bytearray(48 * 48 * 4)
        for y in range(height):
            for x in range(width):
                index = pixels[y * width + x]
                target = ((y + 4) * 48 + x) * 4  # Original y54 within the remake's y50 control.
                rgba[target:target + 4] = bytes((*palette[index], 0 if index == 0 else 255))
        outputs[f'Research/{name}.png'] = png(48, 48, rgba)
    for name, data in outputs.items():
        path = ROOT / 'Godot/Sprites/Items' / name
        if args.check:
            assert path.read_bytes() == data, f'Recovery mismatch: {path}'
        else:
            path.write_bytes(data)
    print(f'{"Verified" if args.check else "Recovered"} {len(outputs)} assets: 21 visible construction stages, six blank stages and five research illustrations.')


if __name__ == '__main__':
    main()
