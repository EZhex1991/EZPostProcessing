/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-18 20:44:05
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
    [PostProcess(typeof(EZColorBasedOutlineRenderer), PostProcessEvent.BeforeStack, "EZUnity/EZColorBasedOutline", allowInSceneView: true)]
    public class EZColorBasedOutline : PostProcessEffectSettings
    {
        public ColorParameter _GrayWeight = new ColorParameter() { value = new Color(0.299f, 0.587f, 0.114f, 1) };
        [UnityEngine.Rendering.PostProcessing.Min(0)]
        public FloatParameter _Tolerance = new FloatParameter() { value = 5 };
        public ColorParameter _OutlineColor = new ColorParameter() { value = Color.black };
        public IntParameter _OutlineThickness = new IntParameter() { value = 1 };
    }

    public class EZColorBasedOutlineRenderer : PostProcessEffectRenderer<EZColorBasedOutline>
    {
        private static class Uniforms
        {
            public static readonly string Name = "EZColorBasedOutline";
            public static readonly string ShaderName = "Hidden/EZUnity/PostProcessing/EZColorBasedOutline";
            public static readonly int Property_GrayWeight = Shader.PropertyToID("_GrayWeight");
            public static readonly int Property_Tolerance = Shader.PropertyToID("_Tolerance");
            public static readonly int Property_OutlineColor = Shader.PropertyToID("_OutlineColor");
            public static readonly int Property_OutlineThickness = Shader.PropertyToID("_OutlineThickness");
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

            sheet.properties.SetColor(Uniforms.Property_GrayWeight, settings._GrayWeight);
            sheet.properties.SetFloat(Uniforms.Property_Tolerance, settings._Tolerance);
            sheet.properties.SetColor(Uniforms.Property_OutlineColor, settings._OutlineColor);
            sheet.properties.SetFloat(Uniforms.Property_OutlineThickness, settings._OutlineThickness);
            command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            command.EndSample(Uniforms.Name);
        }
    }
}
#endif
