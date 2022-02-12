#include "UnityCG.cginc"

// Object rendering things

float4x4 _NonJitteredVP;
float4x4 _PreviousVP;
float4x4 _PreviousM;
bool _HasLastPositionData;
bool _ForceNoMotion;
float _MotionVectorDepthBias;

struct MotionVectorData
{
    float4 transferPos : TEXCOORD0;
    float4 transferPosOld : TEXCOORD1;
    float4 pos : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

struct MotionVertexInput
{
    float4 vertex : POSITION;
    float3 oldPos : TEXCOORD4;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

MotionVectorData VertMotionVectors(MotionVertexInput v)
{
    MotionVectorData o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.pos = UnityFlipSprite(v.vertex, _Flip);
    o.pos = UnityObjectToClipPos(o.pos);
    #ifdef PIXELSNAP_ON
			o.pos = UnityPixelSnap (o.pos);
    #endif

    // this works around an issue with dynamic batching
    // potentially remove in 5.4 when we use instancing
    #if defined(UNITY_REVERSED_Z)
    o.pos.z -= _MotionVectorDepthBias * o.pos.w;
    #else
            o.pos.z += _MotionVectorDepthBias * o.pos.w;
    #endif
    o.transferPos = UnityFlipSprite(v.vertex, _Flip);
    o.transferPosOld = UnityFlipSprite(v.vertex, _Flip);

    #if defined(USING_STEREO_MATRICES)
            o.transferPos = mul(_StereoNonJitteredVP[unity_StereoEyeIndex], mul(unity_ObjectToWorld, o.transferPosOld));
            o.transferPosOld = mul(_StereoPreviousVP[unity_StereoEyeIndex], mul(_PreviousM, _HasLastPositionData ? float4(v.oldPos, 1) : o.transferPosOld));
    #else
    o.transferPos = mul(_NonJitteredVP, mul(unity_ObjectToWorld, o.transferPos));
    o.transferPosOld = mul(_PreviousVP, mul(_PreviousM, _HasLastPositionData ? float4(v.oldPos, 1) : o.transferPosOld));
    #endif
    #ifdef PIXELSNAP_ON
			o.transferPos  = UnityPixelSnap (o.transferPos);
			o.transferPosOld  = UnityPixelSnap (o.transferPos);
    #endif
    return o;
}

half4 FragMotionVectors(MotionVectorData i) : SV_Target
{
    float3 hPos = (i.transferPos.xyz / i.transferPos.w);
    float3 hPosOld = (i.transferPosOld.xyz / i.transferPosOld.w);

    // V is the viewport position at this pixel in the range 0 to 1.
    float2 vPos = (hPos.xy + 1.0f) / 2.0f;
    float2 vPosOld = (hPosOld.xy + 1.0f) / 2.0f;

    #if UNITY_UV_STARTS_AT_TOP
    vPos.y = 1.0 - vPos.y;
    vPosOld.y = 1.0 - vPosOld.y;
    #endif
    half2 uvDiff = vPos - vPosOld;
    return lerp(half4(uvDiff, 0, 1), 0, (half)_ForceNoMotion);
}
