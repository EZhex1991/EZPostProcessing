/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-06-18 17:01:27
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [System.Serializable]
    [PostProcess(typeof(EZDepthBasedOutlineRenderer), PostProcessEvent.BeforeTransparent, "EZUnity/EZDepthBasedOutline", allowInSceneView: false)]
    public class EZDepthBasedOutline : PostProcessEffectSettings
    {
        public IntParameter _SampleDistance = new IntParameter() { value = 1 };

        [UnityEngine.Rendering.PostProcessing.Min(0)]
        public FloatParameter _DepthSensitivity = new FloatParameter() { value = 5 };
        [UnityEngine.Rendering.PostProcessing.Min(0)]
        public FloatParameter _NormalSensitivity = new FloatParameter() { value = 0 };

        public ColorParameter _CoverColor = new ColorParameter();
        [Range(0, 1)]
        public FloatParameter _CoverStrength = new FloatParameter() { value = 0 };

        public ColorParameter _OutlineColor = new ColorParameter();
        [Range(0, 1)]
        public FloatParameter _OutlineStrength = new FloatParameter() { value = 1 };
    }

    public class EZDepthBasedOutlineRenderer : PostProcessEffectRenderer<EZDepthBasedOutline>
    {
        private static class Uniforms
        {
            public static readonly string Name = "EZDepthBasedOutline";
            public static readonly string ShaderName = "Hidden/EZUnity/PostProcessing/EZDepthBasedOutline";
            public static readonly int Property_SampleDistance = Shader.PropertyToID("_SampleDistance");
            public static readonly int Property_DepthSensitivity = Shader.PropertyToID("_DepthSensitivity");
            public static readonly int Property_NormalSensitivity = Shader.PropertyToID("_NormalSensitivity");
            public static readonly int Property_CoverColor = Shader.PropertyToID("_CoverColor");
            public static readonly int Property_CoverStrength = Shader.PropertyToID("_CoverStrength");
            public static readonly int Property_OutlineColor = Shader.PropertyToID("_OutlineColor");
            public static readonly int Property_OutlineStrength = Shader.PropertyToID("_OutlineStrength");
        }

        private static Shader m_Shader;
        private static Shader shader
        {
            get
            {
                if (m_Shader == null)
                {
                    m_Shader = Shader.Find(Uniforms.ShaderName);
                }
                return m_Shader;
            }
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer command = context.command;
            command.BeginSample(Uniforms.Name);

            sheet.properties.SetFloat(Uniforms.Property_SampleDistance, settings._SampleDistance);
            sheet.properties.SetFloat(Uniforms.Property_DepthSensitivity, settings._DepthSensitivity);
            sheet.properties.SetFloat(Uniforms.Property_NormalSensitivity, settings._NormalSensitivity);
            sheet.properties.SetColor(Uniforms.Property_CoverColor, settings._CoverColor);
            sheet.properties.SetFloat(Uniforms.Property_CoverStrength, settings._CoverStrength);
            sheet.properties.SetColor(Uniforms.Property_OutlineColor, settings._OutlineColor);
            sheet.properties.SetFloat(Uniforms.Property_OutlineStrength, settings._OutlineStrength);
            command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            command.EndSample(Uniforms.Name);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return base.GetCameraFlags() | DepthTextureMode.DepthNormals;
        }
    }
}
#endif
