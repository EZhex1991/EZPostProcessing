// Author:			ezhex1991@outlook.com
// CreateTime:		2018-08-31 16:41:15
// Organization:	#ORGANIZATION#
// Description:		

Shader "Hidden/EZUnity/PostProcessing/EZOutline" {
	HLSLINCLUDE
		#include "Assets/PostProcessing/PostProcessing/Shaders/StdLib.hlsl"
		//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
		
		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
		TEXTURE2D_SAMPLER2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture);
		float4 _MainTex_TexelSize;
			
		float _SampleDistance;

		float _DepthSensitivity;
		float _NormalSensitivity;

		float4 _CoverColor;
		float _CoverStrength;

		float4 _OutlineColor;
		float _OutlineStrength;
	ENDHLSL

	SubShader {
		Cull Off
		ZWrite Off
		ZTest Always

		Pass {
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			float CheckBounds(float2 uv, float d)
			{
				float ob = any(uv < 0) + any(uv > 1);
				#if defined(UNITY_REVERSED_Z)
					ob += (d <= 0.00001);
				#else
					ob += (d >= 0.99999);
				#endif
					return ob * 1e8;
			}
			float SampleDepth(float2 uv)
			{				
				float d = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv), 0));
				return d * _ProjectionParams.z + CheckBounds(uv, d);
			}
			float3 SampleNormal(float2 uv)
			{
				uv = UnityStereoTransformScreenSpaceTex(uv);
				float4 cdn = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, uv);
				return DecodeViewNormalStereo(cdn) * float3(1.0, 1.0, -1.0);
			}

			int EdgeCheck (float2 uv1, float2 uv2) {
				float3 normal1 = SampleNormal(uv1);
				float depth1 = SampleDepth(uv1);
				float3 normal2 = SampleNormal(uv2);
				float depth2 = SampleDepth(uv2);

				float3 normalDiff = abs(normal1 - normal2);
				int normalCheck = (normalDiff.x + normalDiff.y) * _NormalSensitivity < 1.0;
				int depthCheck = abs(depth1 - depth2) * _DepthSensitivity < depth1;
				return normalCheck * depthCheck ? 0 : 1;
			}
			half4 Frag (VaryingsDefault i) : SV_Target {
				half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

				float2 d = _SampleDistance * float2(0.5, -0.5);
				float2 uv1 = i.texcoord + d.xx * _MainTex_TexelSize;
				float2 uv2 = i.texcoord + d.xy * _MainTex_TexelSize;
				float2 uv3 = i.texcoord + d.yy * _MainTex_TexelSize;
				float2 uv4 = i.texcoord + d.yx * _MainTex_TexelSize;
				int edge = EdgeCheck(uv1, uv3) + EdgeCheck(uv2, uv4);
				edge = saturate(edge);

				color = lerp(color, _CoverColor, _CoverStrength);
				color = lerp(color, _OutlineColor, edge * _OutlineStrength);

				return color;
			}
			
			ENDHLSL
		}
	}
}
