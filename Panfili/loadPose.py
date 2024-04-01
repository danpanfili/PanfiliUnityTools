import pickle, json
from collections import OrderedDict
import numpy as np
import matplotlib.pyplot as plt
from scipy.spatial.transform import Rotation as Rot
import pywavefront as pwf
import pyglet
from pywavefront import visualization

name = [f"s3_{n}" for n in range(7,9)]
ax = plt.axes(projection='3d')

allpos = {}

for i,n in enumerate(name):
    path = fr'Z:\KarlBackup\data_drive\allTraj\{n}_out\cameras.sfm'
    with open(path, 'r') as file:
        data = json.load(file)

    view = {v['poseId']:{
        'image':v['path'].split('/')[-1].split('.')[0],
    } for v in data['views']}

    pose = {int(view[p['poseId']]['image']):{
        'transform':    p['pose']['transform'],
        'id':           p['poseId']
    } for p in data['poses']}
    pose = OrderedDict(sorted(pose.items()))

    pos = [p['transform']['center'] for p in pose.values()]
    rot = [p['transform']['rotation'] for p in pose.values()]

    pos = np.array(pos).astype(np.float64)
    rot = np.array(rot).astype(np.float64).reshape(-1,3,3)
    # rot = [Rot.from_matrix(r.reshape(3,3)).as_euler() for r in rot]
    # dir = np.array([Rot.from_matrix(r).as_euler('xyz') for r in rot])
    dir = rot[:,:,0]
    dir -= dir[0,:]

    if(i > 0): pos = pos - pos[0,:] + allpos[name[i-1]][-1,:]

    path = fr'Z:\KarlBackup\data_drive\allMeshes\{n}_out\texturedMesh.obj'
    # scene = pwf.Wavefront(path, parse=True, cache=True)
    scene = pwf.Wavefront(path, cache = True)
    pwf.visualization.draw(scene)
    # ax.scatter3D(pos[:,0],pos[:,1],pos[:,2],'gray')
    # ax.quiver(pos[:,0],pos[:,1],pos[:,2], dir[:,0], dir[:,1], dir[:,2], length=1, normalize=True)

    allpos[n] = pos

ax.set_aspect('equal', 'box')
ax.view_init(elev=0, azim=90, roll=0)
plt.show()
print("Done")