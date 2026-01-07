Shader "Custom/URPToonCodeShader_Advanced_MultiLight"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _TintColor ("Tint Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Tint", Color) = (0.5, 0.5, 0.5, 1) 
        _Threshold ("Threshold", Range(0, 1)) = 0.5
        _Smoothness ("Border Smoothness", Range(0.01, 1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // GetVertexPositionInputsとGetShadowCoordのために必要
            // (Core.hlslやLighting.hlsl内で既includeされている場合も多いですが、明示します)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl" 

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _TintColor;
                half4 _ShadowColor;
                half _Threshold;
                half _Smoothness;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 normalWS     : TEXCOORD0;
                float2 uv           : TEXCOORD1;
                // ワールド座標 (ライティング計算に必要)
                float3 positionWS   : TEXCOORD2; 
                // 影計算用の座標 (メインライト・追加ライト兼用)
                float4 shadowCoord  : TEXCOORD3; 
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // ライティングと影に必要な座標をまとめて計算
                // GetVertexPositionInputs は HCS, WS の両方の座標を計算してくれます
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS; // ｸﾘｯﾌﾟ空間座標
                OUT.positionWS = posInputs.positionWS;  // ﾜｰﾙﾄﾞ空間座標
                
                // 影計算用の座標を取得
                OUT.shadowCoord = GetShadowCoord(posInputs);
                
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1. ベース色の準備
                half4 textureColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half3 baseColor = textureColor.rgb * _TintColor.rgb;

                // vertから渡された情報を受け取る
                float3 positionWS = IN.positionWS;
                float3 normalWS = normalize(IN.normalWS); // 補間された法線は正規化

                // 2. メインライトの計算
                // GetMainLightに影座標を渡し、影を考慮させる
                Light mainLight = GetMainLight(IN.shadowCoord);
                half nl_main = saturate(dot(normalWS, mainLight.direction));

                // トゥーンの境界を計算
                half shadowEdge = _Threshold - _Smoothness;
                half lightEdge = _Threshold;
                half toonStep_main = smoothstep(shadowEdge, lightEdge, nl_main);

                // 3. メインライト用の色を計算
                half3 litColor_main = baseColor * mainLight.color; // ライトの色も反映
                half3 shadowColor = baseColor * _ShadowColor.rgb; 
                
                // 4. メインライトを適用した「基本色」を計算
                // (影色 と 明色 をトゥーンステップで補間)
                half3 finalColor = lerp(shadowColor, litColor_main, toonStep_main);
                
                
                // このピクセルに影響する追加ライトの数を取得
                int addLightsCount = GetAdditionalLightsCount();
                for (int i = 0; i < addLightsCount; i++)
                {
                    // i番目の追加ライトの情報を取得 (影座標も渡す)
                    Light addLight = GetAdditionalLight(i, positionWS, IN.shadowCoord);
                    
                    // NdotL
                    half nl_add = saturate(dot(normalWS, addLight.direction));
                    
                    // トゥーンステップ
                    half toonStep_add = smoothstep(shadowEdge, lightEdge, nl_add);
                    
                    // 減衰 (距離減衰と影減衰)
                    half attenuation = addLight.distanceAttenuation * addLight.shadowAttenuation;
                    
                    // 追加ライトの色 (減衰を考慮)
                    half3 addLightColor = addLight.color * attenuation;
                    
                    // 追加ライトの寄与分
                    // (ベース色 * ライト色) にトゥーンステップを適用し、「加算」する
                    // (toonStep_addが0なら 0 が加算される)
                    finalColor += baseColor * addLightColor * toonStep_add;
                }
                
                return half4(finalColor, textureColor.a);
            }
            ENDHLSL
        }
    }
}