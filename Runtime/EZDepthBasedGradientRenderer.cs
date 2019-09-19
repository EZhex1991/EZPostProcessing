/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-19 11:40:00
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    public class EZDepthBasedGradientRenderer : PostProcessEffectRenderer<EZDepthBasedGradientSettings>
    {
        private const string ShaderName = "Hidden/EZUnity/PostProcessing/EZDepthBasedGradient";
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
            sheet.properties.SetColor("_ColorNear", settings._ColorNear);
            sheet.properties.SetColor("_ColorFar", settings._ColorFar);
            sheet.properties.SetFloat("_GradientPower", settings._GradientPower);
            sheet.properties.SetVector("_GradientSoftness", settings._GradientSoftness);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
#endif
