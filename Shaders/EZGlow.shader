// Author:			ezhex1991@outlook.com
// CreateTime:		2019-09-25 10:12:08
// Organization:	#ORGANIZATION#
// Description:		

Shader "Hidden/EZUnity/PostProcessing/EZGlow" {
	HLSLINCLUDE
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
		TEXTURE2D_SAMPLER2D(_GlowTex, sampler_GlowTex);
		TEXTURE2D_SAMPLER2D(_GlowBloomTex, sampler_GlowBloomTex);
		
		float4 _MainTex_TexelSize;
		float _SampleScale;

		half4 _OuterGlowColor;
		half _OuterGlowIntensity;
		half4 _InnerGlowColor;
		half _InnerGlowIntensity;
		
        half4 FragDepthTo01(VaryingsDefault i) : SV_Target
        {
			half4 depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
            return step(depth, 0.999);
        }
        half4 FragDownsample4(VaryingsDefault i) : SV_Target
        {
            half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
            return color;
        }
        half4 Combine(half4 bloom, float2 uv)
        {
            half4 color = SAMPLE_TEXTURE2D(_GlowBloomTex, sampler_GlowBloomTex, uv);
            return (bloom + color) * 0.5;
        }
        half4 FragUpsampleBox(VaryingsDefault i) : SV_Target
        {
            half4 bloom = UpsampleBox(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
            return Combine(bloom, i.texcoordStereo);
        }
	ENDHLSL
	SubShader {
		Cull Off
		ZWrite Off
		ZTest Always

		// 0: Combine
		Pass {
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment frag
				#pragma multi_compile _ _DEPTHTEST_ON

				half4 frag (VaryingsDefault i) : SV_Target {
					half4 glowDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_GlowTex, sampler_GlowTex, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
					int glow = step(glowDepth, 0.999);
					half4 glowBloom = SAMPLE_TEXTURE2D(_GlowBloomTex, sampler_GlowBloomTex, i.texcoord);

					half diff = glowBloom.r - glow.r;
					half outerGlow = max(0, diff) * _OuterGlowIntensity;
					half innerGlow = max(0, -diff) * _InnerGlowIntensity;

					#if _DEPTHTEST_ON
						float mainDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
						int intensity = step(glowDepth, mainDepth);
						outerGlow *= intensity;
						innerGlow *= intensity;
					#endif
					
					half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
					color.rgb += outerGlow * _OuterGlowColor.rgb;
					color.rgb += innerGlow * _InnerGlowColor.rgb;

					return color;
				}

			ENDHLSL
		}

		// 1: GlowBase
		Pass {
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment FragDepthTo01

			ENDHLSL
		}

		// 2: Downsample
		Pass {
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment FragDownsample4

			ENDHLSL
		}

        // 3: Upsample
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleBox

            ENDHLSL
        }
	}
}
