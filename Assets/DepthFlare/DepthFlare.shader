Shader "Toguchi/DepthFlare"
{
    Properties
    {
        [HDR] _Color ("Color", COLOR) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector" = "True" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        LOD 100
        Blend SrcAlpha One
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
            CBUFFER_END
           
            Varyings Vert (Attributes input)
            {
                Varyings o;

                // 原点の座標変換
                VertexPositionInputs pivotInput = GetVertexPositionInputs(float3(0, 0, 0));

                // 原点でDepth判定
                // フレアの原点のプロジェクション座標
                float4 projection = pivotInput.positionNDC;

                // フレアの原点の地点に書き込まれているDepth
                float sceneZ = LinearEyeDepth(SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, projection.xy / projection.w, 0).r, _ZBufferParams);
                // フレアの原点のDepth
                float thisZ = LinearEyeDepth(projection.z / projection.w, _ZBufferParams);
                // フレアの原点が遮蔽されていたら0になる
                float fade = step(thisZ, sceneZ);

                // ビルボード処理
                // スケール・回転のみをワールド座標変換
                float3 billboardWS = mul((float3x3)UNITY_MATRIX_M, input.vertex.xyz);
                // ビュー座標にスケール・回転を加算
                float3 billboardVS = pivotInput.positionVS + float3(billboardWS.xy, -billboardWS.z);

                o.vertex = mul(UNITY_MATRIX_P, float4(billboardVS, 1.0));

                // 遮蔽されていたらすべての頂点座標を同一にして計算する
                o.vertex = lerp(pivotInput.positionCS, o.vertex, fade);
    
                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return o;
            }

            half4 Frag (Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                col *= _Color;
                return col;
            }
            ENDHLSL
        }
    }
}
