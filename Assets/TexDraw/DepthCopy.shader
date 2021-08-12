Shader "Hidden/Toguchi/DepthCopy"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

    TEXTURE2D_X_FLOAT(_MainTex);
    float4 _MainTex_TexelSize;

   float4 EncodeFloatRGBA( float v )
    {
        float4 enc = float4(1.0, 255.0, 65025.0, 16581375.0) * v;
        enc = frac(enc);
        enc -= enc.yzww * float4(1.0/255.0,1.0/255.0,1.0/255.0,0.0);
        return enc;
    }

    half4 Frag(Varyings input) : SV_Target
    {
        float depth = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp , input.uv).r;
        depth = Linear01Depth(depth, _ZBufferParams);
        half4 output = EncodeFloatRGBA(depth);

        return output;
    }

ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "Copy"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }
}