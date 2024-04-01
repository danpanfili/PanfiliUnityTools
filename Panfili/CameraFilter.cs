using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panfili.Observer;

namespace Panfili {
    public class Filter{
        public GameObject cam;
        public Material capture, extract;

        public List<RenderTexture> preRT = new List<RenderTexture>();
        public List<RenderTexture> postRT = new List<RenderTexture>();

        public List<RenderBuffer> preRB = new List<RenderBuffer>();
        public List<RenderBuffer> postRB = new List<RenderBuffer>();

        public float deltaTime;

        public Float edgeThreshold = new Float(0.01f);
        public Int edgeDepth = new Int(2);

        public Int displayMode = new Int(0);
        public Vector2 window_pos = new Vector2(500, 10);
        public Vector2 window_size = new Vector2(200, 150);

        public Filter(GameObject _cam){
            cam         = _cam;

            capture    = new Material(Shader.Find("Panfili/Capture"));
            extract    = new Material(Shader.Find("Panfili/Extract"));

            for(int i = 0; i < 3; i++) { 
                preRT.Add( RT_Init() ); 
                preRB.Add( preRT[i].colorBuffer );
            }

            for(int i = 0; i < 4; i++) { 
                postRT.Add( RT_Init() ); 
                postRB.Add( postRT[i].colorBuffer );
            }
            
            Cam_Init();
        }

        void Cam_Init(){
            cam.GetComponent<Camera>().targetTexture = preRT[0];
            cam.GetComponent<Camera>().depthTextureMode    = DepthTextureMode.DepthNormals | DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
            
            // displayMode.OnChange( () => Extract() );
            // edgeThreshold.OnChange( () => Extract() );
            // edgeDepth.OnChange( () => Extract() );
        }

        RenderTexture RT_Init(int depth = 16, RenderTextureFormat rtf = RenderTextureFormat.ARGBHalf){
            var rt = new RenderTexture(Screen.width, Screen.height, depth, rtf);
            rt.useMipMap = true;
            rt.autoGenerateMips = true;
            rt.Create();
            return rt;
        }

        public IEnumerator OldUpdate(float _deltaTime){
            yield return new WaitForEndOfFrame();
            
            extract.SetFloat("_DeltaTime",         _deltaTime);
            extract.SetFloat("_DisplayMode",       displayMode.Value);
            extract.SetFloat("_EdgeThreshold",     edgeThreshold.Value);
            extract.SetFloat("_EdgeDepth",         edgeDepth.Value);
            
            Graphics.Blit(preRT[0], postRT[0], extract);

            yield return null;
        }

        public IEnumerator Capture(float _deltaTime){
            yield return new WaitForEndOfFrame();
            Graphics.SetRenderTarget(preRB.ToArray(), preRT[0].depthBuffer);
            Graphics.Blit(null, capture);

            deltaTime = _deltaTime;
            Extract();
            yield return null;
        }

        public void Extract(){
            extract.SetFloat("_DeltaTime",         deltaTime);
            extract.SetFloat("_DisplayMode",       displayMode.Value);
            extract.SetFloat("_EdgeThreshold",     edgeThreshold.Value);
            extract.SetFloat("_EdgeDepth",         edgeDepth.Value);

            extract.SetTexture("_LastTex", preRT[0]);
            extract.SetTexture("_DepthNormalsTexture", preRT[1]);
            extract.SetTexture("_MotionVectorsTexture", preRT[2]);

            Graphics.SetRenderTarget(postRB.ToArray(), postRT[0].depthBuffer);
            Graphics.Blit(null, extract);
        }

        // public IEnumerator Update( float deltaTime ) {
        //     yield return new WaitForEndOfFrame();

        //     material.SetFloat("_DeltaTime",         deltaTime);
        //     material.SetFloat("_DisplayMode",       displayMode.Value);
        //     material.SetFloat("_EdgeThreshold",     edgeThreshold.Value);
        //     material.SetFloat("_EdgeDepth",         edgeDepth.Value);
            
        //     Graphics.Blit(null, RT[0]);

        //     Graphics.SetRenderTarget(_mrt, rt1.depthBuffer);
        //     Graphics.Blit(RT[0], material);
        //     // Graphics.Blit(capture, postFilter, material);

        //     yield return null;
        // }

        public void ImageGUI() { 
            // GUI.depth = 666;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), postRT[0], ScaleMode.ScaleToFit, false); 
        }

        public void OptionsGUI(float margin = 20, int offset = 10){
            GUIStyle boxStyle =     new GUIStyle(GUI.skin.box);
            boxStyle.border =       new RectOffset(offset, offset, offset, offset);

            GUI.Box(                new Rect(window_pos.x, window_pos.y, window_size.x, window_size.y), "Display Mode" );
            Rect dropdownRect =     new Rect(window_pos.x, margin*2, window_size.x, window_size.y - margin*2);

            string[] displayMode_name   = {"Color", "Depth", "Luminance", "Normal", "Motion", "Curl", "Divergence", "Edges"};
            displayMode.Value           = GUI.SelectionGrid( dropdownRect, displayMode.Value, displayMode_name, 2 );

            edgeThreshold.Value = GUI.HorizontalSlider(new Rect(window_pos.x, window_pos.y+window_size.y, window_size.x, 30), edgeThreshold.Value, 0.0f, .5f);
            edgeDepth.Value = (int) GUI.HorizontalSlider(new Rect(window_pos.x, window_pos.y+window_size.y+30, window_size.x, 30), edgeDepth.Value, 0, 5);
        }

        public void OnDestroy(){
            foreach(var rt in preRT) {rt.Release();}
            foreach(var rt in postRT) {rt.Release();}
        }
    }
}