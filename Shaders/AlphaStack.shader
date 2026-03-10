Shader "CrowdGame/AlphaStack"
{
    Properties
    {
        _SrcTex ("Source Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        // Single pass: pack RGB in top half, alpha as grayscale in bottom half.
        // Input: ARGB source texture at base resolution.
        // Output: Double-height texture — top half is RGB, bottom half is alpha as grayscale.
        Pass
        {
            Name "AlphaStackPack"
            ZTest Always
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            sampler2D _SrcTex;

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // UV.y 0..0.5 = top half (RGB), UV.y 0.5..1.0 = bottom half (Alpha)
                // Remap UV to sample from full source texture
                float2 srcUV;
                half4 result;

                if (input.uv.y < 0.5)
                {
                    // Top half: sample RGB from source, remap y from [0, 0.5] to [0, 1]
                    srcUV = float2(input.uv.x, input.uv.y * 2.0);
                    half4 src = tex2D(_SrcTex, srcUV);
                    result = half4(src.rgb, 1.0);
                }
                else
                {
                    // Bottom half: sample alpha from source, output as grayscale
                    // Remap y from [0.5, 1.0] to [0, 1]
                    srcUV = float2(input.uv.x, (input.uv.y - 0.5) * 2.0);
                    half4 src = tex2D(_SrcTex, srcUV);
                    result = half4(src.aaa, 1.0);
                }

                return result;
            }
            ENDCG
        }
    }

    Fallback Off
}
