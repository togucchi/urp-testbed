Shader "Toguchi/2D-Depth"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", COLOR) = (1, 1, 1, 1)
    }
    
HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    struct Attributes
    {
        float4 positionOS       : POSITION;
        float2 uv               : TEXCOORD0;
        float4 color            : COLOR;
    };

    struct Varyings
    {
        float2 uv        : TEXCOORD0;
        float4 color   : COLOR;
        float4 vertex : SV_POSITION;
    };

    half4 _Color;

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);

    Varyings vert(Attributes input)
    {
        Varyings output = (Varyings)0;

        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.vertex = vertexInput.positionCS;
        output.color = input.color;
        output.uv = input.uv;

        return output;
    }

    half4 frag(Varyings input) : SV_Target
    {
        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
        color *= _Color;
        color.rgb *= color.a;

        return color;
    }

    half Frag_Depth(Varyings input) : SV_Target
    {
        half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
        alpha *= input.color.a;

        clip(alpha - 0.01);

        return 1.0 - input.color.rgb;
    }
ENDHLSL
    
    SubShader
    {
        Tags
        { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
        LOD 100
        
        ZWrite Off
        ZTest Always
        Cull Back
        Lighting Off

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" "RenderPipeline" = "UniversalPipeline" }
            
            Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass
        {
            Tags {"LightMode" = "Draw2DDepth" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment Frag_Depth
            ENDHLSL
        }
    }
}
