using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Panfili;
using Panfili.Observer;

public class Main : MonoBehaviour
{
    public bool interpolate = false;
    public int displayMode = 0;

    public GameObject marker_prefab;

    public String path = new String("Z:\\KarlBackup\\data_drive");
    public String run_name = new String("s3_7");

    public static string environment_path, data_path, marker_path;
    public static GameObject environment;

    public Filter firstPersonCamera;

    public static Panfili.Playback.Player   player;
    public static Panfili.Object.Source     shadow;

    public Panfili.Python.Server    server;
    public Panfili.Data.Loader      loader = new Panfili.Data.Loader();

    public double[] test1, test2;

    public System.Action[] guis;

    void Awake(){ 
        player = new Panfili.Playback.Player();
        server = new Panfili.Python.Server();
        environment = new GameObject();

        UpdatePaths();
        path.OnChange( UpdatePaths );
        run_name.OnChange( UpdatePaths );

        shadow = new Panfili.Object.Source( _name: "Shadow", _path: marker_path, _prefab: marker_prefab );
        LoadCamera();
        
        server.Init();

        guis = new System.Action[] {
            () => firstPersonCamera.ImageGUI(),
            () => firstPersonCamera.OptionsGUI(),
            () => player.ControlGUI(),
            () => EnvironmentGui()
        };

        var db = new Database(@"Z:\database\raw\BerkeleyOutdoorWalk.db");
        db.Select("BerkeleyOutdoorWalk.Subject03.Binocular");
        test1 = db.data["BerkeleyOutdoorWalk.Subject03.Binocular.Pupil.pupil_positions.Posx"];
        // test2 = db.data["BerkeleyOutdoorWalk.Subject03.Binocular.Shadow.take0002_stream.Hips.Global.Posx"];
    }

    void UpdatePaths(){
        environment_path = $"{path.Value}\\allMeshes\\{run_name.Value}_out\\texturedMesh.obj";
        data_path = $"{path.Value}\\pupilShadowMesh\\{run_name.Value}_pupilShadowMesh.mat";
        marker_path = @"Z:\rawdata\VRPrism\Subject08\05172023_150952\data.csv";

        // environment_path = $"{path.Value}\\berkAlign\\obj\\{run_name.Value}_out\\texturedMesh.obj";
        // data_path = $"{path.Value}\\berkAlign\\pupilShadowMesh\\{run_name.Value}.mat";
    }
    
    void Update(){ 
        player.Update(); 
        player.interpolate = interpolate; 
        
        // GameObject.Find("Camera").transform.LookAt(GameObject.Find("GazeTarget").transform.position);
        GameObject.Find("Camera").transform.LookAt(GameObject.Find("LookHere").transform.position);
    }

    void OnDestroy(){ server.Close(); firstPersonCamera.OnDestroy();}

    public void LoadEnvironment(){
        Debug.Log($"Loading Environment: {data_path}");
        loader.Environment(ref environment, environment_path, loader.Rotation(server, data_path));
    }

    public void LoadCamera(){
        firstPersonCamera = new Filter( shadow.markers["Camera"].game_object );
        // player.time.OnChange( () => StartCoroutine( firstPersonCamera.Capture( player.time.delta ) ) );
        player.time.OnChange( () => StartCoroutine( firstPersonCamera.OldUpdate( player.time.delta ) ) );
    }

    void OnGUI() { foreach(var gui in guis) gui();}

    void EnvironmentGui(){
        // Define GUI elements
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.border = new RectOffset(10, 10, 10, 10);

        float boxWidth = 300;
        float boxHeight = 120;
        float margin = 20;

        Rect boxRect = new Rect(Screen.width - boxWidth - margin, margin, boxWidth, boxHeight);
        Rect pathLabelRect = new Rect(boxRect.x + 10, boxRect.y + 20, 80, 20);
        Rect pathRect = new Rect(pathLabelRect.x + pathLabelRect.width, pathLabelRect.y, boxWidth - pathLabelRect.width - 20, 20);
        Rect runLabelRect = new Rect(boxRect.x + 10, boxRect.y + 40, 80, 20);
        Rect runNameRect = new Rect(runLabelRect.x + runLabelRect.width, runLabelRect.y, boxWidth - runLabelRect.width - 20, 20);
        Rect buttonRect = new Rect(pathLabelRect.x, pathLabelRect.y + 60, boxWidth - 20, 30);

        // Draw GUI elements
        GUI.Box(boxRect, "", boxStyle);
        
        GUI.Label(pathLabelRect, "Path:");
        path.Value = GUI.TextField(pathRect, path.Value);

        GUI.Label(runLabelRect, "Run Name:");
        run_name.Value = GUI.TextField(runNameRect, run_name.Value);

        if (GUI.Button(buttonRect, "Load Environment")) { LoadEnvironment(); }
    }

    // private int luminosity_option_index, flow_option_index;
    // void FlowGui(){
    //     // Define GUI elements
    //     GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
    //     boxStyle.border = new RectOffset(10, 10, 10, 10);

    //     float boxWidth = 300;
    //     float boxHeight = 200;
    //     float margin = 20;

    //     GUI.Box(new Rect(500, 10, boxWidth, boxHeight), "Flow options");
    //     Rect dropdownRect = new Rect(500, margin*2, boxWidth, boxHeight - margin*2);
    //     string[] displayMode_name  = {"Color", "Depth", "Normal", "Motion", "Curl", "Divergence"};
    //     displayMode     = GUI.SelectionGrid(dropdownRect, displayMode, displayMode_name, 2);
    //     // flow.displayMode.Value  = displayMode;
    // }
}