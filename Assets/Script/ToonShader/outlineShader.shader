Shader "Unlit/outlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RampTex ("Ramp", 2D) = "white" {}
        _OutlineWidth("Outline Width", Float) = 0.04
        _TintColor ("Tint Color", Color) = (1,1,1,1) 
        // ▼▼▼ 追加：アウトラインの色を変更するプロパティ ▼▼▼
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "OUTLINE"
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float _OutlineWidth;
            // ▼▼▼ 追加：変数の宣言 ▼▼▼
            fixed4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                worldPos += normalize(worldNormal) * _OutlineWidth;
                o.vertex = UnityWorldToClipPos(float4(worldPos, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ▼▼▼ 変更：ハードコード(黒)をやめてプロパティの色を返す ▼▼▼
                return _OutlineColor; 
            }
            ENDCG
        }

        Pass
        {
            Name "BASE"
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_RampTex : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_RampTex : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _RampTex;
            float4 _RampTex_ST;
            fixed4 _TintColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(UnityObjectToWorldNormal(v.normal));
                o.uv_MainTex = TRANSFORM_TEX(v.uv_MainTex, _MainTex);
                o.uv_RampTex = TRANSFORM_TEX(v.uv_RampTex, _RampTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half nl = dot(i.normal, _WorldSpaceLightPos0.xyz) * 0.5 + 0.5;
                fixed3 ramp = tex2D(_RampTex, float2(nl, 0.5)).rgb;
                fixed4 col = tex2D(_MainTex, i.uv_MainTex);
                col.rgb *= ramp;
                col.rgb *= _TintColor.rgb; // 色を乗算
                return col;
            }
            ENDCG
        }
    }
}