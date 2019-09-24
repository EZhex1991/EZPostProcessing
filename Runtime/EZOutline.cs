/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-06-18 17:01:27
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [System.Serializable]
    [PostProcess(typeof(EZOutlineRenderer), PostProcessEvent.BeforeTransparent, "EZUnity/EZOutline", allowInSceneView: false)]
    public class EZOutline : PostProcessEffectSettings
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

    public class EZOutlineRenderer : PostProcessEffectRenderer<EZOutline>
    {
        private const string ShaderName = "Hidden/EZUnity/PostProcessing/EZOutline";
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
            sheet.properties.SetFloat("_SampleDistance", settings._SampleDistance);
            sheet.properties.SetFloat("_DepthSensitivity", settings._DepthSensitivity);
            sheet.properties.SetFloat("_NormalSensitivity", settings._NormalSensitivity);
            sheet.properties.SetColor("_CoverColor", settings._CoverColor);
            sheet.properties.SetFloat("_CoverStrength", settings._CoverStrength);
            sheet.properties.SetColor("_OutlineColor", settings._OutlineColor);
            sheet.properties.SetFloat("_OutlineStrength", settings._OutlineStrength);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return base.GetCameraFlags() | DepthTextureMode.DepthNormals;
        }
    }
}
#endif
