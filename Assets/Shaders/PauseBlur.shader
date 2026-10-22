
Shader "Custom/PauseBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 2
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float _BlurSize;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        fixed4 blur(float2 uv, float2 direction)
        {
            float2 stepSize =
                _MainTex_TexelSize.xy * _BlurSize * direction;

            fixed4 color = tex2D(_MainTex, uv) * 0.227027;

            color += tex2D(_MainTex, uv + stepSize * 1.384615) * 0.316216;
            color += tex2D(_MainTex, uv - stepSize * 1.384615) * 0.316216;
            color += tex2D(_MainTex, uv + stepSize * 3.230769) * 0.070270;
            color += tex2D(_MainTex, uv - stepSize * 3.230769) * 0.070270;

            return color;
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                return blur(i.uv, float2(1, 0));
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                return blur(i.uv, float2(0, 1));
            }
            ENDCG
        }
    }

    Fallback Off
}
