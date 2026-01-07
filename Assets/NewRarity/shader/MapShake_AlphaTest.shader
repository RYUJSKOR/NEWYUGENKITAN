// Made with Amplify Shader Editor v1.9.5.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MapShake_AlphaTest"
{
	Properties
	{
		_MainTex("Base Map", 2D) = "linearGrey" {}
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Float) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MetallicMap("Metallic Map", 2D) = "white" {}
		_BaseMetalic("Base Metalic", Range( 0 , 1)) = 0
		_BaseSmooth("Base Smooth", Range( 0 , 1)) = 0.5
		_BaseOcclusion("Base Occlusion", Range( 0 , 5)) = 1
		_BaseNormal("Base Normal", 2D) = "bump" {}
		_NormalIntensity("Normal Intensity", Range( 0 , 10)) = 1
		_EmissionMap("Emission Map", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,1)
		_VertexIntensity("Vertex Intensity", Range( 0 , 5)) = 0.5
		_ShakeNoiseMap("Shake Noise Map", 2D) = "linearGrey" {}
		[Toggle(_SHAKEOBJ_ON)] _ShakeOBJ("Shake OBJ", Float) = 0
		[Toggle]_ShakePowerY("Shake Power Y", Range( 0 , 1)) = 0
		_ShakeDirection("Shake Direction", Vector) = (1,1,1,0)
		_ShakeMovement("Shake Movement", Vector) = (1,1,0,0)
		_ShakeStrength("Shake Strength", Float) = 1
		_ShakeDensity("Shake Density", Float) = 1
		_ShakeHighlightColor("Shake Highlight Color", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull [_CullMode]
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _SHAKEOBJ_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _CullMode;
		uniform sampler2D _ShakeNoiseMap;
		uniform float _ShakePowerY;
		uniform float2 _ShakeMovement;
		uniform float _ShakeDensity;
		uniform float _ShakeStrength;
		uniform float3 _ShakeDirection;
		uniform sampler2D _BaseNormal;
		uniform float4 _BaseNormal_ST;
		uniform float _NormalIntensity;
		uniform float _VertexIntensity;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _ShakeHighlightColor;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform float4 _EmissionColor;
		uniform sampler2D _MetallicMap;
		uniform float4 _MetallicMap_ST;
		uniform float _BaseMetalic;
		uniform float _BaseSmooth;
		uniform float _BaseOcclusion;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 transform50 = mul(unity_ObjectToWorld,float4( 0,0,0,1 ));
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 worldToObj133 = mul( unity_WorldToObject, float4( ase_worldPos, 1 ) ).xyz;
			#ifdef _SHAKEOBJ_ON
				float3 staticSwitch134 = worldToObj133;
			#else
				float3 staticSwitch134 = ase_worldPos;
			#endif
			float4 tex2DNode1 = tex2Dlod( _ShakeNoiseMap, float4( ( ( ( float3( (staticSwitch134).xz ,  0.0 ) + ( staticSwitch134 * _ShakePowerY ) ) + float3( ( ( _ShakeMovement * ( 10.0 / _ShakeDensity ) ) * _Time.y ) ,  0.0 ) ) * ( _ShakeDensity * 0.01 ) ).xy, 0, 0.0) );
			float temp_output_38_0 = ( 1.0 - v.color.a );
			float4 lerpResult44 = lerp( transform50 , ( transform50 + ( ( tex2DNode1 - float4( 0.5,0.5,0.5,0 ) ) * _ShakeStrength ) ) , float4( ( _ShakeDirection * temp_output_38_0 ) , 0.0 ));
			float4 transform132 = mul(unity_WorldToObject,lerpResult44);
			v.vertex.xyz += transform132.xyz;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BaseNormal = i.uv_texcoord * _BaseNormal_ST.xy + _BaseNormal_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _BaseNormal, uv_BaseNormal ), _NormalIntensity );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode18 = tex2D( _MainTex, uv_MainTex );
			float3 ase_worldPos = i.worldPos;
			float3 worldToObj133 = mul( unity_WorldToObject, float4( ase_worldPos, 1 ) ).xyz;
			#ifdef _SHAKEOBJ_ON
				float3 staticSwitch134 = worldToObj133;
			#else
				float3 staticSwitch134 = ase_worldPos;
			#endif
			float4 tex2DNode1 = tex2D( _ShakeNoiseMap, ( ( ( float3( (staticSwitch134).xz ,  0.0 ) + ( staticSwitch134 * _ShakePowerY ) ) + float3( ( ( _ShakeMovement * ( 10.0 / _ShakeDensity ) ) * _Time.y ) ,  0.0 ) ) * ( _ShakeDensity * 0.01 ) ).xy );
			float4 blendOpSrc69 = tex2DNode18;
			float4 blendOpDest69 = ( _ShakeHighlightColor * tex2DNode1 );
			float temp_output_38_0 = ( 1.0 - i.vertexColor.a );
			float4 lerpResult71 = lerp( tex2DNode18 , ( saturate( (( blendOpDest69 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest69 ) * ( 1.0 - blendOpSrc69 ) ) : ( 2.0 * blendOpDest69 * blendOpSrc69 ) ) )) , ( temp_output_38_0 * _ShakeHighlightColor.a ));
			o.Albedo = ( saturate( ( i.vertexColor - ( ( i.vertexColor - ( i.vertexColor * i.vertexColor ) ) * _VertexIntensity ) ) ) * lerpResult71 ).rgb;
			float2 uv_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			o.Emission = ( tex2D( _EmissionMap, uv_EmissionMap ) * _EmissionColor ).rgb;
			float2 uv_MetallicMap = i.uv_texcoord * _MetallicMap_ST.xy + _MetallicMap_ST.zw;
			float4 tex2DNode19 = tex2D( _MetallicMap, uv_MetallicMap );
			o.Metallic = ( tex2DNode19.r * _BaseMetalic );
			o.Smoothness = ( tex2DNode19.a * _BaseSmooth );
			float lerpResult29 = lerp( 1.0 , tex2DNode19.g , _BaseOcclusion);
			o.Occlusion = lerpResult29;
			o.Alpha = 1;
			clip( tex2DNode18.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19501
Node;AmplifyShaderEditor.WorldPosInputsNode;73;-3360,576;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;13;-2763.792,1242.877;Inherit;False;Property;_ShakeDensity;Shake Density;18;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;133;-3168,768;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;64;-2933.808,891.9655;Inherit;False;Property;_ShakePowerY;Shake Power Y;14;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;14;-2929.9,997.924;Inherit;False;Property;_ShakeMovement;Shake Movement;16;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;79;-2569.576,1094.111;Inherit;False;2;0;FLOAT;10;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;134;-2944,576;Inherit;False;Property;_ShakeOBJ;Shake OBJ;13;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-2632.797,868.3846;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;24;-2644.971,673.1734;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;17;-2443.661,1161.958;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-2453.8,995.7029;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-2451.095,846.5651;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-2236.534,993.8544;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-2075.016,846.0472;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-2085.423,1241.078;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;95;-1429.413,-1166.764;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1905.053,762.7606;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;-1195.558,-1098.207;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-1729.352,738.6447;Inherit;True;Property;_ShakeNoiseMap;Shake Noise Map;12;0;Create;True;0;0;0;False;0;False;-1;None;d36c9bd1ee46184478fe4fda37093c01;True;0;False;linearGrey;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleSubtractOpNode;99;-1009.556,-1165.207;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-1523.142,-914.8666;Inherit;False;Property;_VertexIntensity;Vertex Intensity;11;0;Create;True;0;0;0;False;0;False;0.5;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;37;-1368.992,1160.154;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;67;-1767.927,-488.2371;Inherit;False;Property;_ShakeHighlightColor;Shake Highlight Color;19;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;18;-1768.671,-713.6066;Inherit;True;Property;_MainTex;Base Map;0;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;linearGrey;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-1436.764,-236.2903;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-1227.694,-939.7324;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;36;-1235.433,738.6921;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0.5,0.5,0.5,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-929.3683,867.9566;Inherit;False;Property;_ShakeStrength;Shake Strength;17;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;38;-1181.092,1256.913;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;69;-1261.07,-261.8388;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;96;-1005.556,-984.2075;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector3Node;40;-1361.53,987.4818;Inherit;False;Property;_ShakeDirection;Shake Direction;15;0;Create;True;0;0;0;False;0;False;1,1,1;1,1,1;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-721.1508,734.2635;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;50;-896.38,523.5896;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-1033.951,-398.8218;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;71;-827.3483,-442.1469;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;97;-843.0082,-927.1823;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-575.5265,979.214;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;-560.4582,631.6772;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-705.9492,-679.5072;Inherit;False;Property;_NormalIntensity;Normal Intensity;8;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-843.2925,88.26257;Inherit;False;Property;_BaseMetalic;Base Metalic;4;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;19;-851.7423,-107.3307;Inherit;True;Property;_MetallicMap;Metallic Map;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;27;-846.2878,169.6768;Inherit;False;Property;_BaseSmooth;Base Smooth;5;0;Create;True;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-846.485,255.502;Inherit;False;Property;_BaseOcclusion;Base Occlusion;6;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;34;-536.3069,-322.6518;Inherit;False;Property;_EmissionColor;Emission Color;10;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,1;0,0,0,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;31;-602.6323,-531.7292;Inherit;True;Property;_EmissionMap;Emission Map;9;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-645.8925,-926.4373;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;44;-388.6333,528.2912;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;20;-420.7923,-727.1598;Inherit;True;Property;_BaseNormal;Base Normal;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-500.6532,-81.45867;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-499.6067,62.18642;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;29;-495.1016,195.4072;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-289.8549,-394.5633;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;122;266.8981,-748.5475;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;132;-73.72876,579.3943;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;135;576,-112;Inherit;False;Property;_CullMode;CullMode;1;1;[Enum];Create;True;0;1;Option1;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;571.9246,-33.83528;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;MapShake_AlphaTest;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;2;-1;-1;-1;0;False;0;0;True;_CullMode;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;133;0;73;0
WireConnection;79;1;13;0
WireConnection;134;1;73;0
WireConnection;134;0;133;0
WireConnection;63;0;134;0
WireConnection;63;1;64;0
WireConnection;24;0;134;0
WireConnection;80;0;14;0
WireConnection;80;1;79;0
WireConnection;51;0;24;0
WireConnection;51;1;63;0
WireConnection;75;0;80;0
WireConnection;75;1;17;0
WireConnection;77;0;51;0
WireConnection;77;1;75;0
WireConnection;10;0;13;0
WireConnection;74;0;77;0
WireConnection;74;1;10;0
WireConnection;98;0;95;0
WireConnection;98;1;95;0
WireConnection;1;1;74;0
WireConnection;99;0;95;0
WireConnection;99;1;98;0
WireConnection;70;0;67;0
WireConnection;70;1;1;0
WireConnection;100;0;99;0
WireConnection;100;1;102;0
WireConnection;36;0;1;0
WireConnection;38;0;37;4
WireConnection;69;0;18;0
WireConnection;69;1;70;0
WireConnection;96;0;95;0
WireConnection;96;1;100;0
WireConnection;41;0;36;0
WireConnection;41;1;42;0
WireConnection;81;0;38;0
WireConnection;81;1;67;4
WireConnection;71;0;18;0
WireConnection;71;1;69;0
WireConnection;71;2;81;0
WireConnection;97;0;96;0
WireConnection;39;0;40;0
WireConnection;39;1;38;0
WireConnection;43;0;50;0
WireConnection;43;1;41;0
WireConnection;101;0;97;0
WireConnection;101;1;71;0
WireConnection;44;0;50;0
WireConnection;44;1;43;0
WireConnection;44;2;39;0
WireConnection;20;5;21;0
WireConnection;25;0;19;1
WireConnection;25;1;26;0
WireConnection;28;0;19;4
WireConnection;28;1;27;0
WireConnection;29;1;19;2
WireConnection;29;2;30;0
WireConnection;33;0;31;0
WireConnection;33;1;34;0
WireConnection;122;0;101;0
WireConnection;132;0;44;0
WireConnection;0;0;122;0
WireConnection;0;1;20;0
WireConnection;0;2;33;0
WireConnection;0;3;25;0
WireConnection;0;4;28;0
WireConnection;0;5;29;0
WireConnection;0;10;18;4
WireConnection;0;11;132;0
ASEEND*/
//CHKSM=294F99C9F10BADAFFEA2AA0C6C564C1658B7A553