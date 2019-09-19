// Author:			ezhex1991@outlook.com
// CreateTime:		2018-08-31 16:41:15
// Organization:	#ORGANIZATION#
// Description:		

Shader "Hidden/EZUnity/PostProcessing/EZDepthBasedGradient" {
	HLSLINCLUDE
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
		
		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

		half4 _ColorNear;
		half4 _ColorFar;

		half _GradientPower;
		float2 _GradientSoftness;
	ENDHLSL

	SubShader {
		Cull Off
		ZWrite Off
		ZTest Always

		Pass {
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment frag

			float InverseLerp(float min, float max, float value) {
				return saturate((value - min) / (max - min));
			}

			half4 frag (VaryingsDefault i) : SV_Target {
				half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

				float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));

				depth = InverseLerp(_GradientSoftness.x, _GradientSoftness.y, pow(depth, _GradientPower));

				color *= lerp(_ColorNear, _ColorFar, depth);

				return color;
			}
			ENDHLSL
		}
	}
}
