/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-18 20:44:28
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    public class EZColorBasedOutlineRenderer : PostProcessEffectRenderer<EZColorBasedOutlineSettings>
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
