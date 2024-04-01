#ifndef PANFILI_SHADER_TOOLS_INCLUDED
#define PANFILI_SHADER_TOOLS_INCLUDED

#include "UnityShaderVariables.cginc"
#include "UnityCG.cginc"

const float Rad2Deg = 180 / UNITY_PI;
const float Deg2Rad = UNITY_PI / 180;
#define FOV ( atan(1.0f / UNITY_MATRIX_P._m11 ) * 2.0 * Rad2Deg )

half2 PixelsToDegrees(half2 pix, float2 texelSizeXY) { return pix * texelSizeXY * FOV; }

uint Pack8to16(uint2 arr)   { return arr.x | (arr.y >> 8); }
uint Pack16to132(uint2 arr) { return arr.x | (arr.y >> 16); }

half2 MotionComponents(half2 motion){
    half2 dU    = half2(ddx_fine(motion.x), ddy_fine(motion.x));
    half2 dV    = half2(ddx_fine(motion.y), ddy_fine(motion.y));

    half curl   = dV.x - dU.y;
    half div    = dU.x + dV.y; // Divergence

    return half2(curl, div);
}

///////////////////////////////////////////////////////////////////
// Color conversions from: https://www.chilliant.com/rgb2hsv.html
half3 HSVtoRGB(in half3 HSV) {
    half R = abs(HSV.x * 6 - 3) - 1;
    half G = 2 - abs(HSV.x * 6 - 2);
    half B = 2 - abs(HSV.x * 6 - 4);
    half3 RGB = saturate( half3(R,G,B) );

    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

float Epsilon = 1e-10;
float3 RGBtoHCV(in float3 RGB) {
    // Based on work by Sam Hocevar and Emil Persson
    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return float3(H, C, Q.x);
}

float3 RGBtoHSV(in float3 RGB) {
  float3 HCV = RGBtoHCV(RGB);
  float S = HCV.y / (HCV.z + Epsilon);
  return float3(HCV.x, S, HCV.z);
}
///////////////////////////////////////////////////////////////////

half MotionAngle(half2 motion){ return atan2(motion.y, motion.x); }

bool TrimByAngle(half2 motion, half increment = 10.0, half range = 1.0)
{
    half angle = MotionAngle(motion);
    angle += UNITY_PI;
    angle *= 180 / UNITY_PI;
    angle %= increment;

    if (angle < range) { return true; }
    return false;
}

half3 MotionToRGB( half2 motion, float threshold = .01, half4 default_color = half4(1.0, 1.0, 1.0, 1.0) ) {
    half magnitude  = length(motion);

    if(magnitude < threshold) { return default_color; }

    half angle      = MotionAngle(motion);
    half hue        = (angle + UNITY_PI) / UNITY_TWO_PI;
    half value      = saturate(magnitude);
    
    return HSVtoRGB( half3(hue, 1, value) );
}

float4 SingleChannel( float channel, float alpha = 1.0 )  { return float4(channel, channel, channel, alpha); }
float4 DoubleChannel( float2 channel, float alpha = 1.0 ) { return float4(channel.x, channel.y, channel.y, alpha); }
float4 TripleChannel( float3 channel, float alpha = 1.0 ) { return float4(channel, alpha); }

int Edges(float val, float thresh = .5) {
    float diff = abs(ddx_fine(val)) + abs(ddy_fine(val));
    return diff;
    if (diff > thresh) return 255;
    else return 0;
}

half3 dot2(half3 v) {return dot(v, v);}

float SobelFilter(sampler2D tex, float2 uv) {
    float2 dx = ddx(uv);
    float2 dy = ddy(uv);
    
    float tl = tex2D(tex, uv - float2(dx.x, dy.y)).r;
    float tm = tex2D(tex, uv - float2(0, dy.y)).r;
    float tr = tex2D(tex, uv - float2(-dx.x, dy.y)).r;
    float ml = tex2D(tex, uv - float2(dx.x, 0)).r;
    float mr = tex2D(tex, uv - float2(-dx.x, 0)).r;
    float bl = tex2D(tex, uv - float2(dx.x, -dy.y)).r;
    float bm = tex2D(tex, uv - float2(0, -dy.y)).r;
    float br = tex2D(tex, uv - float2(-dx.x, -dy.y)).r;
    
    float3 sobelX = float3(-1, -2, -1);
    float3 sobelY = float3(-1, 0, 1);
    
    float gx = dot(float3(tl, ml, bl), sobelX) + dot(float3(tr, mr, br), sobelX);
    float gy = dot(float3(tl, tm, tr), sobelY) + dot(float3(bl, bm, br), sobelY);
    
    return sqrt(gx * gx + gy * gy);
}

// int2 Index(int i, int width, int bias = 0) { return int2(i % width + bias, i / width + bias); }

// float2[9] NearbyUV(float2 uv, float2 TexelSize, float pixelDistance = 1.0) {
//     float2[9] nb_uv;
//     for (int i=0; i<9; i++) {
//         idx = Index(i, 3, -1);
//         nb_uv[i] = saturate(uv + TexelSize * idx * pixelDistance); 
//     }

//     return nb_uv;
// }

// float4[9] Nearby(sampler2D tex, float2 uv, float2 TexelSize, float pixelDistance = 1.0) {
//     float4[9] nb;
//     float2[9] nb_uv = NearbyUV(uv, TexelSize, pixelDistance);

//     for (int i=0; i<9; i++) nb[i] = tex2D(tex, nb_uv[i]);

//     return nb;
// }

#endif // PANFILI_SHADER_TOOLS_INCLUDED