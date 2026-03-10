Shader "CrowdGame/TransparentOverlay"
{
    Properties
    {
        _MainTex ("Overlay Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Overlay" }

        // Composites a transparent overlay onto the screen.
        // Used for display clients showing game content over video feeds,
        // OBS Virtual Camera overlays, and broadcast compositing.
        Pass
        {
            Name "TransparentOverlay"
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            half _Opacity;

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
                half4 col = tex2D(_MainTex, input.uv);
                col.a *= _Opacity;
                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}
