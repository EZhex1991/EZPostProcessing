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
		TEXTURE2D_SAMPLER2D(_GlowDepthTex, sampler_GlowDepthTex);
		
		float4 _MainTex_TexelSize;
		float _SampleScale;
		half _GlowIntensity;
		half4 _GlowColor;
		
        half FragDepthOuter(VaryingsDefault i) : SV_Target
        {
			half depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
            return step(depth, 0.999);
        }
        half FragDepthInner(VaryingsDefault i) : SV_Target
        {
			half depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
            return step(0.999, depth);
        }
        half4 FragDownsample(VaryingsDefault i) : SV_Target
        {
            half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
            return color;
        }
        half4 FragUpsample(VaryingsDefault i) : SV_Target
        {
            half4 blur = UpsampleBox(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
			half4 base = SAMPLE_TEXTURE2D(_GlowBloomTex, sampler_GlowBloomTex, i.texcoordStereo);
            return base + blur;
        }

		half4 FragCombine (VaryingsDefault i) : SV_Target {
			half glowbase = SAMPLE_TEXTURE2D(_GlowTex, sampler_GlowTex, i.texcoord).r;
			half glowBloom = SAMPLE_TEXTURE2D(_GlowBloomTex, sampler_GlowBloomTex, i.texcoord).r;
			half glow = max(glowBloom - glowbase, 0) * _GlowIntensity;

			#if _DEPTHTEST_ON
				half mainDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
				half glowDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_GlowDepthTex, sampler_GlowDepthTex, UnityStereoTransformScreenSpaceTex(i.texcoord), 0));
				glow *= step(glowDepth, mainDepth);
			#endif
					
			half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
			#if _BLENDMODE_ADDITIVE
				color.rgb += glow * _GlowColor.rgb;
			#elif _BLENDMODE_SUBSTRACT
				color.rgb = max(0, color.rgb - glow * _GlowColor.rgb);
			#elif _BLENDMODE_MULTIPLY
				color.rgb *= lerp(1, _GlowColor.rgb, glow);
			#elif _BLENDMODE_LERP
				color.rgb = lerp(color.rgb, _GlowColor.rgb, glow);
			#endif

			return color;
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
				#pragma fragment FragCombine
				#pragma multi_compile _ _BLENDMODE_ADDITIVE _BLENDMODE_SUBSTRACT _BLENDMODE_MULTIPLY _BLENDMODE_LERP
				#pragma multi_compile _ _DEPTHTEST_ON

			ENDHLSL
		}

		// 1: GlowOuter
		Pass {
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment FragDepthOuter

			ENDHLSL
		}

		// 2: GlowInner
		Pass {
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment FragDepthInner

			ENDHLSL
		}

		// 3: Downsample
		Pass {
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment FragDownsample

			ENDHLSL
		}

        // 4: Upsample
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsample

            ENDHLSL
        }
	}
}
