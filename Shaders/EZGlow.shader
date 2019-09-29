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
		half4 _InnerGlowColor;
		
        half FragDepthTo01(VaryingsDefault i) : SV_Target
        {
			half depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
            return step(depth, 0.999);
        }
        half4 FragDownsample4(VaryingsDefault i) : SV_Target
        {
            half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
            return color;
        }
        half4 FragUpsampleBox(VaryingsDefault i) : SV_Target
        {
            half4 blur = UpsampleBox(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
			half4 base = SAMPLE_TEXTURE2D(_GlowBloomTex, sampler_GlowBloomTex, i.texcoordStereo);
            return (base + blur) * 0.5;
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
				#pragma multi_compile _ _BLENDMODE_ADDITIVE _BLENDMODE_SUBSTRACT _BLENDMODE_MULTIPLY _BLENDMODE_LERP
				#pragma multi_compile _ _DEPTHTEST_ON

				half4 frag (VaryingsDefault i) : SV_Target {
					half glowDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_GlowTex, sampler_GlowTex, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
					half glow = step(glowDepth, 0.999);
					half glowBloom = SAMPLE_TEXTURE2D(_GlowBloomTex, sampler_GlowBloomTex, i.texcoord).r;

					half diff = glowBloom - glow;
					half outerGlow = max(0, diff);
					half innerGlow = max(0, -diff);

					#if _DEPTHTEST_ON
						float mainDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
						int intensity = step(glowDepth, mainDepth);
						outerGlow *= intensity;
						innerGlow *= intensity;
					#endif
					
					half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
					#if _BLENDMODE_ADDITIVE
						color.rgb += outerGlow * _OuterGlowColor.rgb;
						color.rgb += innerGlow * _InnerGlowColor.rgb;
					#elif _BLENDMODE_SUBSTRACT
						color.rgb -= outerGlow * _OuterGlowColor.rgb;
						color.rgb -= innerGlow * _InnerGlowColor.rgb;
					#elif _BLENDMODE_MULTIPLY
						color.rgb *= lerp(1, _OuterGlowColor.rgb, outerGlow);
						color.rgb *= lerp(1, _InnerGlowColor.rgb, innerGlow);
					#elif _BLENDMODE_LERP
						color.rgb = lerp(color.rgb, _OuterGlowColor.rgb, outerGlow);
						color.rgb = lerp(color.rgb, _InnerGlowColor.rgb, innerGlow);
					#endif

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
