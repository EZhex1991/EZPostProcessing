// Author:			ezhex1991@outlook.com
// CreateTime:		2019-08-23 15:27:28
// Organization:	#ORGANIZATION#
// Description:		

Shader "Hidden/EZUnity/PostProcessing/EZColorBasedOutline" {
	HLSLINCLUDE
		#include "Assets/PostProcessing/PostProcessing/Shaders/StdLib.hlsl"
		//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
				
		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		float4 _MainTex_TexelSize;

		half4 _GrayWeight;
		float _Tolerance;

		half4 _OutlineColor;
		int _OutlineThickness;
	ENDHLSL

	SubShader {
		Cull Off
		ZWrite Off
		ZTest Always

		Pass {
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment frag

			float RGBToGray (half3 rgb, half3 weight) {
				return dot(rgb, weight);
			}
			float Difference (half3 rgb1, half3 rgb2, half3 weight) {
				return abs(RGBToGray(rgb1, weight) - RGBToGray(rgb2, weight)) * 255;
			}

			half4 frag (VaryingsDefault i) : SV_Target {
				half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
				int isBorder = 0;
				for (int x = -_OutlineThickness; x <= _OutlineThickness; x++) {
					for (int y = -_OutlineThickness; y <= _OutlineThickness; y++) {
						float2 uv = i.texcoord + float2(x, y) * _MainTex_TexelSize;
						isBorder += step(_Tolerance, Difference(color, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv), _GrayWeight));
					}
				}
				color.rgb = lerp(color.rgb, _OutlineColor.rgb, saturate(isBorder) * _OutlineColor.a);
				return color;
			}
			ENDHLSL
		}
	}
}
