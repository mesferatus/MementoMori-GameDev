"""Normalize generated 6x4 pixel-art sheets without cutting poses at cell edges.

Uses connected alpha regions to assign artwork to its nearest source cell, then
applies one shared character scale. This keeps the supplied sheet as separate
Unity sprites and avoids flattening scene or animation layers.
"""

from __future__ import annotations

import argparse
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image


def components(mask: np.ndarray) -> tuple[np.ndarray, list[dict[str, int]]]:
    height, width = mask.shape
    labels = np.zeros((height, width), dtype=np.int32)
    found: list[dict[str, int]] = []
    next_label = 0
    for y, x in np.argwhere(mask):
        y, x = int(y), int(x)
        if labels[y, x]:
            continue
        next_label += 1
        labels[y, x] = next_label
        queue = deque([(y, x)])
        count = sx = sy = 0
        left = right = x
        top = bottom = y
        while queue:
            cy, cx = queue.popleft()
            count += 1
            sx += cx
            sy += cy
            left, right = min(left, cx), max(right, cx)
            top, bottom = min(top, cy), max(bottom, cy)
            for ny, nx in ((cy - 1, cx), (cy + 1, cx), (cy, cx - 1), (cy, cx + 1)):
                if 0 <= ny < height and 0 <= nx < width and mask[ny, nx] and not labels[ny, nx]:
                    labels[ny, nx] = next_label
                    queue.append((ny, nx))
        found.append({"label": next_label, "count": count, "cx": sx // count, "cy": sy // count,
                      "left": left, "right": right + 1, "top": top, "bottom": bottom + 1})
    return labels, found


def normalize(source: Path, output: Path, frame_size: int, scale: float, baseline: int) -> None:
    original = Image.open(source).convert("RGBA")
    if original.size != (1536, 1024):
        raise ValueError(f"Expected 1536x1024 source: {source}: {original.size}")
    pixels = np.asarray(original)
    labels, found = components(pixels[:, :, 3] > 64)
    by_cell: dict[tuple[int, int], list[dict[str, int]]] = {(r, c): [] for r in range(4) for c in range(6)}
    for item in found:
        row = max(0, min(3, item["cy"] // 256))
        col = max(0, min(5, item["cx"] // 256))
        by_cell[row, col].append(item)
    sheet = Image.new("RGBA", (frame_size * 6, frame_size * 4))
    sizes: list[tuple[int, int]] = []
    for row in range(4):
        for col in range(6):
            parts = [item for item in by_cell[row, col] if item["count"] >= 3]
            if not parts:
                raise ValueError(f"Empty frame {row},{col} in {source}")
            left = min(item["left"] for item in parts)
            top = min(item["top"] for item in parts)
            right = max(item["right"] for item in parts)
            bottom = max(item["bottom"] for item in parts)
            region_labels = labels[top:bottom, left:right]
            assigned = np.isin(region_labels, [item["label"] for item in parts])
            region = pixels[top:bottom, left:right].copy()
            region[~assigned] = 0
            crop = Image.fromarray(region, "RGBA")
            width = max(1, round(crop.width * scale))
            height = max(1, round(crop.height * scale))
            if width > frame_size - 4 or height > frame_size - 4:
                fit = min((frame_size - 4) / crop.width, (frame_size - 4) / crop.height)
                width, height = max(1, round(crop.width * fit)), max(1, round(crop.height * fit))
            sprite = crop.resize((width, height), Image.Resampling.NEAREST)
            x = col * frame_size + (frame_size - width) // 2
            y = row * frame_size + min(baseline - height, frame_size - height - 2)
            sheet.alpha_composite(sprite, (x, y))
            sizes.append((width, height))
    output.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output)
    print(f"{output}: 24 frames, {sheet.width}x{sheet.height}, frame={frame_size}, "
          f"scale={scale}, max_sprite={max(w for w, _ in sizes)}x{max(h for _, h in sizes)}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--frame", type=int, required=True)
    parser.add_argument("--scale", type=float, required=True)
    parser.add_argument("--baseline", type=int, required=True)
    arguments = parser.parse_args()
    normalize(arguments.source, arguments.output, arguments.frame, arguments.scale, arguments.baseline)
