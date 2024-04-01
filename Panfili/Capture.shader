Shader "Panfili/Capture" {
    Properties {
        _MainTex        ("Camera Input", 2D) = "red" {}
    }
    SubShader {
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            
            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D_half _CameraDepthNormalsTexture;
            sampler2D_half _CameraMotionVectorsTexture;

            float4 _MainTex_ST;

            v2f vert (appdata v) { 
                v2f o;

                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.uv        = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }
            
            void frag (v2f i, 
            out half4 rgba : SV_Target0,
            out half4 dn : SV_Target1, 
            out half4 m : SV_Target2 ) 
            { 
                rgba = tex2D(_MainTex, i.uv); 
                dn = tex2D(_CameraDepthNormalsTexture, i.uv);
                m = tex2D(_CameraMotionVectorsTexture, i.uv);
            }
            ENDCG
        }
    }
}