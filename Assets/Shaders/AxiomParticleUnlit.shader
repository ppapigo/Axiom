Shader "Axiom/ParticleUnlit"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha One
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.color = input.color * _Color;
                output.uv = input.uv;
                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float distanceFromCenter = length((input.uv - 0.5) * 2.0);
                float glow = saturate(1.0 - distanceFromCenter);
                glow = smoothstep(0.0, 1.0, glow);
                fixed4 color = input.color;
                color.rgb *= glow;
                color.a *= glow;
                return color;
            }
            ENDCG
        }
    }
}
