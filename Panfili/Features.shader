Shader "Panfili/Extract" {
    Properties {
        _MainTex                ("Camera Input", 2D) = "red" {}
        _LastTex                ("Last Recorded Frame", 2D) = "red" {}
        _MotionVectorsTexture   ("Motion", 2D) = "red" {}
        _DepthNormalsTexture    ("Depth and Normal", 2D) = "red" {}


        _DisplayMode    ("Output Modes: 0 = Color", int) = 0
        _Saturation     ("Flow Color Saturation", Range(0, 1)) = 1
        _DeltaTime      ("Time Since Last Draw Call", float) = 0.01
        _EdgeThreshold  ("Edge detection threshold", float) = 1.0
        _EdgeDepth      ("Mipmap used for edge detection", int) = 2
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            
            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc"
            #include "ShaderTools.cginc"

            int     _DisplayMode;
            float   _DeltaTime;
            float   _EdgeThreshold;
            int     _EdgeDepth;

            
            sampler2D _MainTex;
            sampler2D _LastTex;
            // sampler2D_half _DepthNormalsTexture;
            // sampler2D_half _MotionVectorsTexture;
            sampler2D_half _CameraDepthNormalsTexture;
            sampler2D_half _CameraMotionVectorsTexture;
            
            float4 _MainTex_ST;
            float4 _LastTex_ST;

            struct appdata {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct output {
                half4   display     : SV_Target0;
                half4   rgbd        : SV_Target1;
                half4   normalEdge  : SV_Target2;
                half4   motion      : SV_Target3;
                half    lumin       : SV_Target4;
            };

            v2f vert (appdata v) { 
                v2f o;

                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.uv        = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            float4 DisplayMode(output o){
                switch (_DisplayMode) {
                    case 1:     return TripleChannel( half3( cos(o.rgbd.w * 128), sin(o.rgbd.w * 128), sin(o.rgbd.w * 64)) );
                    case 2:     return SingleChannel( o.lumin );
                    case 3:     return TripleChannel( o.normalEdge.xyz );
                    case 4:     return float4( MotionToRGB(o.motion.xy, _DeltaTime) * o.normalEdge.w, o.normalEdge.w * _EdgeThreshold);
                    case 5:     return DoubleChannel( half2(o.motion.z, -o.motion.z) );
                    case 6:     return DoubleChannel( half2(o.motion.w, -o.motion.w) );
                    case 7:     return SingleChannel( o.normalEdge.w );
                        
                    default:    return TripleChannel( o.rgbd.rgb );
                }
            }
            
            output frag(v2f i) {
                output o;

                o.rgbd      = tex2D(_MainTex, i.uv);
                DecodeDepthNormal( tex2D(_CameraDepthNormalsTexture, i.uv), o.rgbd.w, o.normalEdge.xyz );
                if( o.rgbd.w > .9 ) {
                    o.display = float4(0,0,0,1);
                    return o;
                }

                o.lumin     = Luminance(o.rgbd.rgb);

                // o.edge = Luminance(o.rgb - tex2Dlod(_LastTex, float4(i.uv, 0.0, _EdgeDepth)).rgb) > pow(_EdgeThreshold,3);
                o.normalEdge.w = Luminance(o.rgbd.rgb - tex2Dlod(_MainTex, float4(i.uv, 0.0, _EdgeDepth)).rgb) / _EdgeThreshold;

                o.motion.xy = tex2D(_CameraMotionVectorsTexture, i.uv).rg / _DeltaTime;

                float2 curlDiv = MotionComponents( o.motion ) * 100;
                o.motion.z  = curlDiv.x;
                o.motion.w  = curlDiv.y;

                o.display = DisplayMode(o);

                return o;
            }

            ENDCG
        }
    }
    // FallBack "Diffuse"
}


// Shader "Panfili/Features" {
//     Properties {
//         _MainTex        ("Camera Input", 2D) = "red" {}
//         _DisplayMode    ("Output Modes: 0 = Color", int) = 0
//         _Saturation     ("Flow Color Saturation", Range(0, 1)) = 1
//         _DeltaTime      ("Time Since Last Draw Call", float) = 0.01
//         _EdgeThreshold  ("Edge detection threshold", float) = 1.0
//         _EdgeDepth      ("Mipmap used for edge detection", int) = 2
//     }
//     SubShader {
//         Pass {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #pragma target 4.5
            
//             #include "UnityCG.cginc"
//             #include "UnityShaderVariables.cginc"
//             #include "ShaderTools.cginc"

//             int     _DisplayMode;
//             float   _DeltaTime;
//             float   _EdgeThreshold;
//             int   _EdgeDepth;

            
//             sampler2D _MainTex;
//             sampler2D_half _CameraDepthNormalsTexture;
//             sampler2D_half _CameraMotionVectorsTexture;
            
//             float4 _MainTex_ST;

//             struct appdata {
//                 float4 vertex   : POSITION;
//                 float2 uv       : TEXCOORD0;
//             };

//             struct v2f {
//                 float4 vertex   : SV_POSITION;
//                 float2 uv       : TEXCOORD0;
//             };

//             struct output {
//                 half4   display     : SV_Target0;
//                 half4   rgbd        : SV_Target1;
//                 half4   normalEdge  : SV_Target2;
//                 half4   motion      : SV_Target3;
//                 half    lumin       : SV_Target4;
//             };

//             v2f vert (appdata v) { 
//                 v2f o;

//                 o.vertex    = UnityObjectToClipPos(v.vertex);
//                 o.uv        = TRANSFORM_TEX(v.uv, _MainTex);

//                 return o;
//             }

//             float4 DisplayMode(output o){
//                 switch (_DisplayMode) {
//                     case 1:     return TripleChannel( half3( cos(o.rgbd.w * 128), sin(o.rgbd.w * 128), sin(o.rgbd.w * 64)) );
//                     case 2:     return SingleChannel( o.lumin );
//                     case 3:     return TripleChannel( o.normalEdge.xyz );
//                     case 4:     return float4( MotionToRGB(o.motion.xy, _DeltaTime) * o.normalEdge.w, o.normalEdge.w * _EdgeThreshold);
//                     case 5:     return DoubleChannel( half2(o.motion.z, -o.motion.z) );
//                     case 6:     return DoubleChannel( half2(o.motion.w, -o.motion.w) );
//                     case 7:     return SingleChannel( o.normalEdge.w );
                        
//                     default:    return TripleChannel( o.rgbd.rgb );
//                 }
//             }
            
//             output frag(v2f i) {
//                 output o;

//                 o.rgbd      = tex2D(_MainTex, i.uv);
//                 DecodeDepthNormal( tex2D(_CameraDepthNormalsTexture, i.uv), o.rgbd.w, o.normalEdge.xyz );
//                 if( o.rgbd.w > .9 ) {
//                     o.display = float4(0,0,0,1);
//                     return o;
//                 }

//                 o.lumin     = Luminance(o.rgbd.rgb);

//                 // o.edge = Luminance(o.rgb - tex2Dlod(_MainTex, float4(i.uv, 0.0, _EdgeDepth)).rgb) > pow(_EdgeThreshold,3);
//                 o.normalEdge.w = Luminance(o.rgbd.rgb - tex2Dlod(_MainTex, float4(i.uv, 0.0, _EdgeDepth)).rgb) / _EdgeThreshold;

//                 o.motion.xy = tex2D(_CameraMotionVectorsTexture, i.uv).rg / _DeltaTime;

//                 float2 curlDiv = MotionComponents( o.motion ) * 100;
//                 o.motion.z  = curlDiv.x;
//                 o.motion.w  = curlDiv.y;

//                 o.display = DisplayMode(o);

//                 return o;
//             }

//             ENDCG
//         }
//     }
//     // FallBack "Diffuse"
// }