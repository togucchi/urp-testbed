Shader "Unlit/DepthDecode"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthTex ("DepthTexture", 2D) = "white" {}
    }
    
    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    sampler2D _MainTex;
    float4 _MainTex_ST;

    sampler2D _DepthTex;
    float4 _DepthTex_ST;

    float DecodeFloatRGBA( half4 rgba )
    {
         return dot( rgba, float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0) );
    }

    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

    struct output
    {
        half4 color : SV_Target;
        float depth : SV_Depth;
    };

    output frag (v2f i)
    {
        half4 tex = tex2D(_MainTex, i.uv);
        half4 depthTex = tex2D(_DepthTex, i.uv);
      
        float depth = DecodeFloatRGBA(depthTex);
        depth = (1 / depth - _ZBufferParams.y) / _ZBufferParams.x;
        
        output output;
        output.color = tex;
        output.depth = depth;
        return output;
    }

    output frag_depth_only(v2f i)
    {
        half4 depthTex = tex2D(_DepthTex, i.uv);
      
        float depth = DecodeFloatRGBA(depthTex);
        depth = (1 / depth - _ZBufferParams.y) / _ZBufferParams.x;
        
        output output;
        output.color = 0;
        output.depth = depth;
        return output;
    }
    ENDCG
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
        
        Pass
        {
            Tags {"LightMode" = "DepthOnly"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_depth_only
            ENDCG
        }
    }
}
