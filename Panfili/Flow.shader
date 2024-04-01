Shader "Panfili/VisualMotion" {
    Properties {
        _LuminosityOption ("Use Luminosity (0 off, 1 current, 2 delta, -1 white)", int) = 0
        _FlowOption ("Flow Option (0 off, 1 flow, 2 lines, 3 curl, 4 div)", int) = 0

        _MainTex ("Texture", 2D) = "white" {}
        _ThisTex ("This Frame Texture", 2D) = "white" {}
        _PrevTex ("Previous Frame Texture", 2D) = "white" {}

        _LuminosityDelta ("Change in Luminosity", 2D) = "white" {}
        _LuminosityChangeThreshold ("Luminosity Change Threshold", Range(0, 1)) = 0.01

        _OpticFlow ("Optic Flow Field", 2D) = "white" {}
        _Gain ("Gain of Flow Magnitude", Range(0, 1000)) = 50
        _Saturation ("Flow Color Saturation", Range(0, 1)) = 1
        _FlowAngleIncrement ("Incremenet of Flow Angle to Draw", Range(0, 90)) = 10
        _FlowAngleRange ("Flow Angle Bandpass Range", Range(0, 90)) = 1

        _MaxValue ("Max Value of the Field", Range(0, 1)) = .001
        _PosColor ("Positive Color", Color) = (1.0, 0.0, 0.0, 1)
        _NegColor ("Negative Color", Color) = (0.0, 0.0, 1.0, 1)
    }
    SubShader {
        // Tags { "RenderType"="Opaque" }
        LOD 100
        // Cull Off
        // ZTest Always
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            
            float4 _ThisTex_ST;
            
            float _LuminosityChangeThreshold;
            int _LuminosityOption;

            sampler2D _ThisTex;
            sampler2D _PrevTex;
            sampler2D _LuminosityDelta;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) { 
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ThisTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target0 {
                if(_LuminosityOption == -1) {return float4(1.0, 1.0, 1.0, 1.0);}

                float4 currentColor = tex2D(_ThisTex, i.uv);
                if(_LuminosityOption == 0) {return currentColor;}
                
                float currentLuminosity = dot(currentColor.rgb, fixed3(0.2126, 0.7152, 0.0722));
                if(_LuminosityOption == 1) {return currentLuminosity;}

                float4 prevColor = tex2D(_PrevTex, i.uv);

                float prevLuminosity = dot(prevColor.rgb, fixed3(0.2126, 0.7152, 0.0722));

                float luminosityDerivative = currentLuminosity - prevLuminosity;
                if (luminosityDerivative >= _LuminosityChangeThreshold) 
                {
                    luminosityDerivative = 1.0;
                    float4 luminosityDelta = float4(luminosityDerivative, luminosityDerivative, luminosityDerivative, 1.0);
                    return float4(luminosityDerivative, luminosityDerivative, luminosityDerivative, 1.0);
                }
                else {return float4(0.0, 0.0, 0.0, 0.0);}
            }
            ENDCG
        }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
             
            #include "UnityCG.cginc"
 
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv_flow : TEXCOORD1;
            };
 
            struct v2f {
                float2 uv : TEXCOORD0;
                float2 uv_flow : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };
 
            float4 _MainTex_ST;
            float4 _CameraMotionVectorsTexture_ST;
            float _Gain;
            float _Saturation;
            float _FlowAngleIncrement;
            float _FlowAngleRange;
            float _MaxValue;
            float4 _PosColor;
            float4 _NegColor;
            int _FlowOption;
                
            v2f vert (appdata v) { 
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_flow = TRANSFORM_TEX(v.uv, _CameraMotionVectorsTexture);
                return o;
            }

            sampler2D _MainTex;
            sampler2D_float _CameraMotionVectorsTexture;

            float4 HSVToRGB(float h, float s, float v) {
                float c = v * s;
                float x = c * (1.0 - abs(fmod(h * 6.0, 2.0) - 1.0));
                float m = v - c;
                    
                float3 rgb;
                if (h < 1.0 / 6.0)
                    rgb = float3(c, x, 0.0);
                else if (h < 2.0 / 6.0)
                    rgb = float3(x, c, 0.0);
                else if (h < 3.0 / 6.0)
                    rgb = float3(0.0, c, x);
                else if (h < 4.0 / 6.0)
                    rgb = float3(0.0, x, c);
                else if (h < 5.0 / 6.0)
                    rgb = float3(x, 0.0, c);
                else
                    rgb = float3(c, 0.0, x);
                    
                return float4(rgb + m, 1.0);
            }

            float4 FlowToRGB(float2 motion) {
                float magnitude = length(motion);
                if(magnitude < 0.0005 * _Gain) {return float4(1.0, 1.0, 1.0, 1.0);}

                float angle = atan2(motion.y, motion.x);
                float hue = (angle + 3.14159) / (2.0 * 3.14159);
                float value = magnitude;

                float4 rgb = HSVToRGB(hue, _Saturation, value);

                return rgb;
            }

            float4 FlowToLine(float2 motion)
            {
                float magnitude = length(motion);
                if(magnitude < 0.0005 * _Gain) {return float4(1.0, 1.0, 1.0, 1.0);}

                float angle = atan2(motion.y, motion.x) * 180.0 / 3.14159;

                if (angle < 0.0){angle += 360.0;}

                float angleNearest = round(angle / _FlowAngleIncrement) * _FlowAngleIncrement;

                float angleMin = angleNearest - _FlowAngleRange/2;
                float angleMax = angleNearest + _FlowAngleRange/2;

                if (angle < angleMin || angle > angleMax){return float4(0.0, 0.0, 0.0, 0.0);}

                float hue = angle / 360.0;
                float value = magnitude;

                

                return HSVToRGB(hue, _Saturation, value);
            }

            // float4 InterpolateColor(float value) //, float minVal, float maxVal
            // {
            //     float4 white = float4(1.0,1.0,1.0,1.0);

            //     if (value > 0.0) {return lerp(_PosColor, white, 1-saturate(value/_MaxValue));}
            //     else if (value < 0.0) {return lerp(_NegColor, white, 1-saturate(-value/_MaxValue));}
            //     else {return white;}
            // }

            float4 InterpolateColor(float value) //, float minVal, float maxVal
            {
                float4 white = float4(1.0,1.0,1.0,1.0);
                float4 red = float4(1.0, 0.0, 0.0, 1.0);
                float4 yellow = float4(1.0, 1.0, 0.0, 1.0);
                float4 blue = float4(0.0, 0.0, 1.0, 1.0);
                float4 cyan = float4(0.0, 1.0, 1.0, 1.0);

                float t = saturate(value/_MaxValue);

                if (value > 0.0)
                {
                    if (t > 0.7) {return lerp(red, yellow, 1-saturate(value/2/_MaxValue));}
                    else if (t > 0.2) {return lerp(yellow, white, 0.5-saturate(value/2/_MaxValue));}
                }
                else if(value < 0.0)
                {
                    float t = saturate(-value/_MaxValue);
                    if (t > 0.7) {return lerp(blue, cyan, 1-saturate(-value/2/_MaxValue));}
                    else if (t > 0.2 ) {return lerp(cyan, white, 0.5-saturate(-value/2/_MaxValue));}
                }
                    
                return white;
            }

            float CalculateCurl(float2 motion)
            {
                float2 dU = float2(ddx(motion.x), ddy(motion.x));
                float2 dV = float2(ddx(motion.y), ddy(motion.y));
                float curl = dV.x - dU.y;
                return curl;
            }

            float CalculateDivergence(float2 motion)
            {
                float2 dU = float2(ddx(motion.x), ddy(motion.x));
                float2 dV = float2(ddx(motion.y), ddy(motion.y));
                float divergence = dU.x + dV.y;
                return divergence;
            }

            float4 frag(v2f i) : SV_Target0 {
                float4 rgba = tex2D(_MainTex, i.uv);
                if(_FlowOption == 0) {return rgba;}

                float2 motion = tex2D(_CameraMotionVectorsTexture, i.uv_flow).rg * _Gain * -1;

                if(_FlowOption == 1) {return rgba * FlowToRGB(motion);}
                if(_FlowOption == 2) {return rgba * FlowToLine(motion);}
                if(_FlowOption == 3) {return rgba * InterpolateColor(CalculateCurl(motion));}
                if(_FlowOption == 4) {return rgba * InterpolateColor(CalculateDivergence(motion));}
                else {return rgba;}
            }

            ENDCG
        }
        
    }
    FallBack "Diffuse"
}
