"""Read-only image measurements for the farm v4 art acceptance record."""
import hashlib
import json
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[4]
world = root / 'Assets/_Game/Art/Generated/World'
names = ['props/fountain_keyart_v3.png', 'props/well_keyart_v3.png',
         'props/bridge_keyart_v3.png', 'props/water_ripples_keyart_v4.png',
         'props/waterfall_keyart_v4.png', 'props/stall_keyart_v4.png',
         'foliage/tree_keyart_v4_01.png', 'props/boulder_keyart_v4_01.png',
         'animals/cow_keyart_v4_01.png', 'animals/sheep_keyart_v4_01.png',
         'building/farmhouse_keyart_v4.png', 'building/greenhouse_keyart_v4.png',
         'props/barrel_keyart_v4.png', 'tiles/ground_water_keyart_v4.png',
         'tiles/ground_path_aseprite_v1.png', 'building/coop_keyart_v4.png',
         'building/barn_keyart_v4.png', 'building/cheese_shed_keyart_v4.png',
         'building/wine_shed_keyart_v4.png', 'props/dock_keyart_v4.png',
         'props/boat_keyart_v4.png', 'props/noticeboard_keyart_v4.png',
         'props/tool_rack_keyart_v4.png', 'props/lilies_keyart_v4.png',
         'foliage/undergrowth_keyart_v4.png', 'foliage/wildflowers_keyart_v4.png',
         'props/river_cascade_keyart_v4.png', 'trees/tree_pine_keyart_v4.png',
         'props/fence_rail_keyart_v4.png', 'props/fence_rail_keyart_v4_diagonal.png']
rows = []
for name in names:
    path = world / name
    image = Image.open(path).convert('RGBA')
    alpha = image.getchannel('A')
    counts = alpha.histogram()
    row = dict(path=str(path.relative_to(root)).replace('\\', '/'),
               size=image.size, alpha_bounds_exclusive=alpha.getbbox(),
               transparent_pixels=counts[0], opaque_pixels=counts[255],
               partial_alpha_pixels=sum(counts[1:255]),
               sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    rows.append(row)
out = root / 'docs/validation/farm_keyart_v4/asset-measurements.json'
out.write_text(json.dumps(rows, indent=2), encoding='utf-8')
print(json.dumps(rows, indent=2))
