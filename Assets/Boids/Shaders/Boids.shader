Shader "Toguchi/Boids"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    struct BufferData
    {
        float4 position;
        float4 color;
    };

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
    StructuredBuffer<BufferData> _DataBuffer;
#endif

    void setup()
    {
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        float4 data = _DataBuffer[unity_InstanceID].position;

        unity_ObjectToWorld._11_21_31_41 = float4(data.w, 0, 0, 0);
        unity_ObjectToWorld._12_22_32_42 = float4(0, data.w, 0, 0);
        unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.w, 0);
        unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
        unity_WorldToObject = unity_ObjectToWorld;
        unity_WorldToObject._14_24_34 *= -1;
        unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
#endif
    }

    struct Attributes
    {
        float4 positionOS       : POSITION;
        float2 uv               : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float2 uv        : TEXCOORD0;
        float fogCoord  : TEXCOORD1;
        float4 vertex : SV_POSITION;

        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings vert(Attributes input)
    {
        Varyings output = (Varyings)0;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.vertex = vertexInput.positionCS;
        output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

        return output;
    }

    half4 frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float4 color = 1;

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        color = _DataBuffer[unity_InstanceID].color;
#endif
        return color;
    }
ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup
            ENDHLSL
        }
    }
}
