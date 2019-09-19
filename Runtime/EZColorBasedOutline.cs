/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-18 20:44:05
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [System.Serializable]
    [PostProcess(typeof(EZColorBasedOutlineRenderer), PostProcessEvent.BeforeStack, "EZUnity/EZColorBasedOutline", allowInSceneView: true)]
    public class EZColorBasedOutline : PostProcessEffectSettings
    {
        public ColorParameter _GrayWeight = new ColorParameter() { value = new Color(0.299f, 0.587f, 0.114f, 1) };
        public FloatParameter _Tolerance = new FloatParameter() { value = 50 };
        public ColorParameter _OutlineColor = new ColorParameter() { value = Color.black };
        public IntParameter _OutlineThickness = new IntParameter() { value = 1 };
    }

    public class EZColorBasedOutlineRenderer : PostProcessEffectRenderer<EZColorBasedOutline>
    {
        private const string ShaderName = "Hidden/EZUnity/PostProcessing/EZColorBasedOutline";
        private static Shader m_Shader;
        private static Shader shader
        {
            get
            {
                if (m_Shader == null)
                {
                    m_Shader = Shader.Find(ShaderName);
                }
                return m_Shader;
            }
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            sheet.properties.SetColor("_GrayWeight", settings._GrayWeight);
            sheet.properties.SetFloat("_Tolerance", settings._Tolerance);
            sheet.properties.SetColor("_OutlineColor", settings._OutlineColor);
            sheet.properties.SetFloat("_OutlineThickness", settings._OutlineThickness);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif
