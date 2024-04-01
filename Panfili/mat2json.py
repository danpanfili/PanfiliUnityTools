from scipy.spatial.transform import Rotation
import scipy.io as sio
import numpy as np
import json

debug = False
data = {}

def LoadMat(path): return sio.loadmat(path)
def LoadValue(mat, var): 
    if type(var) is list: 
        for v in var: mat = mat[v]
        return mat
    return mat[var]

def RotationMatrixToQuaternion(rotmat): return Rotation.from_matrix(rotmat).inv().as_quat()
def ArrToBytes(arr): return bytearray.fromhex( arr.real.data.hex() )

def LoadJson(path):
    with open(path, "r") as file: return json.loads( file.read() )

def CameraPose(path):
    pose = {}
    camera = LoadJson(path)
    
    pose['name'] = camera['views'][0]['path'].split('/')[5]

    frames = [int(c['path'].split('/')[6].replace('.png','')) for c in camera['views']]
    transform = [c['pose']['transform'] for c in camera['poses']]

    transform = [t for f,t in sorted(zip(frames,transform))]

    position = [ np.array(t['center']).astype(np.float64).tolist() for t in transform ]
    rotation = [ Rotation.from_matrix( np.array( t['rotation'] ).astype(np.float64).reshape(3,3) ).as_quat().tolist() for t in transform ]

    pose['transform'] = np.array([p+f for p,f in zip(position,rotation)]) # NOTE: These are 64 bit floats

    return pose

def TypeByte(val):
    if val.dtype.str == '<f8': return b'd'
    if val.dtype.str == '<U2': return b's'

def SocketRequest(request):
    param = request.split(',')
    param = [int(p.replace('(int)','')) if '(int)' in p else p for p in param] # Parse for ints

    response = ''
    # Sample request: "Get,C:/path/file.mat,cens,(int)0,(int)0"
    if param[0] == 'Get':
        mat = LoadMat(param[1])
        val = LoadValue(mat, param[2:])
        response = TypeByte(val) + val.tobytes()
    
    elif param[0] == 'GetRot':
        mat = LoadMat(param[1])
        val = LoadValue(mat, param[2:])
        val = RotationMatrixToQuaternion(val)
        response = TypeByte(val) + val.tobytes()

    elif param[0] == 'rotquat':
        mat = LoadMat(param[1])
        val = LoadValue(mat, 'orig2alignedMat')
        val = RotationMatrixToQuaternion(val)
        val = ','.join(val.astype(str))
        response = str.encode(val)

    if response == '': 
        response = b'e'
        print(f"Request not understood: {data.decode()}")

    return response


if(debug):
    path = r'Z:\KarlBackup\data_drive\allTraj\s3_7_out\cameras.sfm'
    poses = CameraPose(path)
    poseAtFrame2 = poses['transform'][2,:].tobytes()

print('done')