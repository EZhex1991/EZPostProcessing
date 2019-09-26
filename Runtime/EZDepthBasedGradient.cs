/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-19 11:39:34
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
    [PostProcess(typeof(EZDepthBasedGradientRenderer), PostProcessEvent.BeforeStack, "EZUnity/EZDepthBasedGradient", allowInSceneView: false)]
    public class EZDepthBasedGradient : PostProcessEffectSettings
    {
        public ColorParameter _ColorNear = new ColorParameter() { value = Color.white };
        public ColorParameter _ColorFar = new ColorParameter() { value = Color.black };

        public FloatParameter _GradientPower = new FloatParameter() { value = 1 };
        [EZMinMax(0, 1)]
        public Vector2Parameter _GradientSoftness = new Vector2Parameter() { value = new Vector2(0, 1) };
    }

    public class EZDepthBasedGradientRenderer : PostProcessEffectRenderer<EZDepthBasedGradient>
    {
        private static class Uniforms
        {
            public static readonly string Name = "EZDepthBasedGradient";
            public static readonly string ShaderName = "Hidden/EZUnity/PostProcessing/EZDepthBasedGradient";
            public static readonly int Property_ColorNear = Shader.PropertyToID("_ColorNear");
            public static readonly int Property_ColorFar = Shader.PropertyToID("_ColorFar");
            public static readonly int Property_GradientPower = Shader.PropertyToID("_GradientPower");
            public static readonly int Property_GradientSoftness = Shader.PropertyToID("_GradientSoftness");

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

            sheet.properties.SetColor(Uniforms.Property_ColorNear, settings._ColorNear);
            sheet.properties.SetColor(Uniforms.Property_ColorFar, settings._ColorFar);
            sheet.properties.SetFloat(Uniforms.Property_GradientPower, settings._GradientPower);
            sheet.properties.SetVector(Uniforms.Property_GradientSoftness, settings._GradientSoftness);
            command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            command.EndSample(Uniforms.Name);
        }
    }
}
#endif
