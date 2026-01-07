Shader "Unlit/WaveFade"
{
    Properties
    {
        _MainColor ("Color", Color) = (0, 0, 0, 1)
        _Cutoff ("Progress", Range(0.0, 1.5)) = 0.0
        
        // モコモコの数と高さ
        _Frequency ("Frequency", Float) = 20.0
        _Amplitude ("Amplitude", Float) = 0.05
        
        // 0 = 下が黒(積み上げ), 1 =上が黒(ワイプ用)
        [Toggle] _Inverse ("Inverse Mode", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            fixed4 _MainColor;
            float _Cutoff;
            float _Frequency;
            float _Amplitude;
            float _Inverse;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 【重要】 abs(sin(...)) を使うことで、波ではなく「半円（モコモコ）」の形にします
                float mokoShape = abs(sin(i.uv.x * _Frequency));
                
                // 波の高さを計算
                float waveHeight = mokoShape * _Amplitude;

                // 本来のY座標に波の凹凸を加える
                float wavyY = i.uv.y + waveHeight;

                float alpha = 0;

                // モード切替
                if (_Inverse > 0.5)
                {
                    // フェードイン用（下から透明になり、黒が上に逃げていく）
                    // wavyY が _Cutoff より大きい場所だけ黒くする
                    alpha = step(_Cutoff, wavyY);
                }
                else
                {
                    // フェードアウト用（下から黒が積み上がっていく）
                    // wavyY が _Cutoff より小さい場所だけ黒くする
                    alpha = step(wavyY, _Cutoff);
                }

                fixed4 col = _MainColor;
                col.a = alpha;
                return col;
            }
            ENDHLSL
        }
    }
}