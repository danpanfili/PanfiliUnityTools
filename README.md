# Panfili Unity Tools
Panfili Unity Tools (PUT): various tools for performing data playback and analysis in Unity.

Tested on Unity 2021.3.27f1

# Dependencies
OBJImport by Dummiesman
Mono.Data.Sqlite Plugin

# Data Playback
Designed for the playback of body and eye tracking data in a 3D environment.

Playback timeseries data in one of 3 ways:
-  Exact frame
-  Fixed time interpolation
-  Free time interpolation

# Runtime Mesh Loading
Load in meshes from any local drive at runtime with Dummiesman's OBJImport (to be replaced with novel loader).

# Python Integration
Communicate with python using network tools.

# Database Access
Interact with SQLite databases like those in LabDAT package.

# Visual Feature Shaders
Easy export using MRT of various features of the current scene, including:
-  Luminance
-  Depth
-  Normals
-  Edges (Sobel)
-  Visual Motion
--  Curl
--  Divergence
