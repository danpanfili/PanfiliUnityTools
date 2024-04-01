# objLoader
import pywavefront as pwf

path = r'Z:\KarlBackup\data_drive\allMeshes\s3_3_out\texturedMesh.obj'
obj = pwf.Wavefront(path, collect_faces=True, strict=True, cache=True)

print('done')