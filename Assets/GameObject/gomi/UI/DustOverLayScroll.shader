Shader "UI/DustOverlayScrollRandom"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Base Scroll Speed", Float) = 0.05
        _RandomOffsetStrength ("Random Offset Strength", Float) = 1.0
        _RandomSpeedStrength ("Random Speed Strength", Float) = 0.1
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Speed;
            float _RandomOffsetStrength;
            float _RandomSpeedStrength;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // 簡易ノイズ関数（位置から擬似乱数を生成）
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);

                // 位置から乱数を作ってスクロールにバリエーションを与える
                float offsetRand = rand(v.vertex.xy);
                float speedRand = rand(v.vertex.xy * 1.3); // 別の乱数

                // オフセットとスピードをそれぞれずらす
                uv += float2(0, _RandomOffsetStrength * offsetRand);
                uv.y += (_Speed + speedRand * _RandomSpeedStrength) * _Time.y;

                o.uv = uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
