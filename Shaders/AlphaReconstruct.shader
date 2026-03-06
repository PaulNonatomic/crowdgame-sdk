Shader "CrowdGame/AlphaReconstruct"
{
    Properties
    {
        _MainTex ("Stacked Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        // Reconstructs RGBA from a double-height alpha-stacked frame.
        // Top half contains RGB, bottom half contains alpha as grayscale.
        // Used for editor preview and client-side reconstruction.
        Pass
        {
            Name "AlphaReconstruct"
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Sample RGB from top half (y: 0..0.5)
                float2 rgbUV = float2(input.uv.x, input.uv.y * 0.5);
                half4 rgb = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, rgbUV);

                // Sample alpha from bottom half (y: 0.5..1.0)
                float2 alphaUV = float2(input.uv.x, input.uv.y * 0.5 + 0.5);
                half4 alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, alphaUV);

                return half4(rgb.rgb, alpha.r);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
