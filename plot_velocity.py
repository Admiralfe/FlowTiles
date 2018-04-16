import matplotlib.pyplot as plt
import numpy as np

filename = 'velocity_full.txt'
flow_tile_size = 15
tiling_size = 15

file = open(filename, 'r')

def get_flowtile_data(fd, tile_size, ):
    x = np.zeros(tile_size*tile_size)
    y = np.zeros(tile_size*tile_size)
    vx = np.zeros(tile_size*tile_size)
    vy = np.zeros(tile_size*tile_size)
    for i in range(0, tile_size):
        row = fd.readline().split(';')
        for j in range(0, tile_size):
            x[i*tile_size + j] = j
            y[i*tile_size + j] = i
            vx[i*tile_size + j] = (float((row[j]).split(',')[0]))
            vy[i*tile_size + j] = (float((row[j]).split(',')[1]))
    return (x, y, vx, vy)

size = (flow_tile_size*tiling_size)**2)
x = np.zeros(size)
y = np.zeros(size)
vx = np.zeros(size)
vy = np.zeros(size)

for i in range(0, tiling_size): #row index
    for j in range(0, tiling_size): #col index
        data = get_flowtile_data(file, flow_tile_size)
        index = j*flow_tile_size**2 + i*tiling_size*(flow_tile_size**2)
        x[index:index+tile_size**2] = (tiling_size-1-j) + data[0]/flow_tile_size
        y[index:index+tile_size**2] = (tiling_size-1-i) + data[1]/flow_tile_size
        vx[index:index+tile_size**2] = data[2]
        vy[index:index+tile_size**2] = data[3]


plt.quiver(x, y, vx, vy)

file.close()
plt.show()
