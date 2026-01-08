Shader "UI/TVBroadcastNoise"
{
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _DotNoiseTex ("Dot Noise Texture", 2D) = "white" {}
        _LineNoiseTex ("Line Noise Texture", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Float) = 0.5
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _DotNoiseTex;
            sampler2D _LineNoiseTex;
            float _NoiseStrength;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float rand(float2 co) {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453 + _Time.y * 100);
            }

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 baseCol = tex2D(_MainTex, i.uv);

                // ランダムにノイズ出現（時間と位置に依存）
                float noiseMask = step(0.98, rand(i.uv * _Time.y));
                fixed4 dotNoise = tex2D(_DotNoiseTex, i.uv) * noiseMask;

                float lineMask = step(0.995, rand(float2(i.uv.y, _Time.y * 1.3)));
                fixed4 lineNoise = tex2D(_LineNoiseTex, i.uv) * lineMask;

                fixed4 finalCol = baseCol + _NoiseStrength * (dotNoise + lineNoise);
                return saturate(finalCol);
            }
            ENDCG
        }
    }
}
