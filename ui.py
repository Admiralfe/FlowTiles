import numpy as np
import matplotlib.pyplot as plt
from lxml import etree
import system

grid_size = sys.argv[1]
row_index = sys.argv[2]
col_index = sys.argv[3]
velocity_xml_file = sys.argv[4]
valid_tiles_xml_file = arg.argv[5]

velocity_root = etree.parse(velocity_xml_file).getroot()
valid_tiles_root = etree.parse(valid_tiles_xml_file).getroot()

grid_fig = plt.figure()
grid_ax = grid_fig.add_subbplot(111)

valid_tiles_fig = plt.figure()

grid_ax.axis([0, 0, grid_size, grid_size])

for i in range(1, grid_size):
    grid_ax.axhline(i, 0, grid_size, c = 'r')
    grid_ax.axvline(i, 0, grid_size, c = 'r')


for tile in velocity_root:
    xmin = int(tile.get("col"))
    ymin = grid_size - int(tile.get("row")) - 1
    for vel in tile:
        x = xmin + float(vel.get("relX"))
        y = ymin + float(vel.get("relY"))
        vx = float(vel.get("vx"))
        vy = float(vel.get("vy"))
        grid_ax.quiver(x, y, vx, vy)

no_valid_tiles = len(valid_tile_root)
valid_tiles_axes = valid_tiles_fig.subplots((no_valid_tiles - no_valid_tiles%3)/3 + 1), 3)

for i in range(0, no_valid_tiles):
    ax = valid_tiles_axes[i]
    ax.set_title("")
    for vel in root[i]:
        x = float(vel.get("relX"))
        y = float(vel.get("relY"))
        vx = float(vel.get("vx"))
        vy = float(vel.get("vy"))
        ax.quiver(x, y, vx, vy)

plt.show()




