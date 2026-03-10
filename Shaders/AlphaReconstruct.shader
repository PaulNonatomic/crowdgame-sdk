Shader "CrowdGame/AlphaReconstruct"
{
    Properties
    {
        _MainTex ("Stacked Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

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

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

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
                // Sample RGB from top half (y: 0..0.5)
                float2 rgbUV = float2(input.uv.x, input.uv.y * 0.5);
                half4 rgb = tex2D(_MainTex, rgbUV);

                // Sample alpha from bottom half (y: 0.5..1.0)
                float2 alphaUV = float2(input.uv.x, input.uv.y * 0.5 + 0.5);
                half4 alpha = tex2D(_MainTex, alphaUV);

                return half4(rgb.rgb, alpha.r);
            }
            ENDCG
        }
    }

    Fallback Off
}
