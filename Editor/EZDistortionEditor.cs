/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-23 19:11:16
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEditor.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [PostProcessEditor(typeof(EZDistortion))]
    public class EZDistortionEditor : PostProcessEffectEditor<EZDistortion>
    {
        private SerializedParameterOverride m_Mode;
        private SerializedParameterOverride m_Intensity;
        private SerializedParameterOverride m_SourceLayer;
        private SerializedParameterOverride m_TextureResolution;
        private SerializedParameterOverride m_TextureFormat;
        private SerializedParameterOverride m_TextureDepth;
        private SerializedParameterOverride m_DistortionTex;

        public override void OnEnable()
        {
            base.OnEnable();
            m_Mode = FindParameterOverride(x => x.mode);
            m_Intensity = FindParameterOverride(x => x.intensity);
            m_SourceLayer = FindParameterOverride(x => x.sourceLayer);
            m_TextureResolution = FindParameterOverride(x => x.textureResolution);
            m_TextureFormat = FindParameterOverride(x => x.textureFormat);
            m_TextureDepth = FindParameterOverride(x => x.textureDepth); ;
            m_DistortionTex = FindParameterOverride(x => x.distortionTex);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PropertyField(m_Mode);
            PropertyField(m_Intensity);
            int mode = m_Mode.value.intValue;
            if (mode == (int)EZDistortion.Mode.Screen)
            {
                PropertyField(m_DistortionTex);
            }
            else if (mode == (int)EZDistortion.Mode.Layer)
            {
                PropertyField(m_SourceLayer);
                PropertyField(m_TextureResolution);
                PropertyField(m_TextureFormat);
                PropertyField(m_TextureDepth);
            }
        }
    }
}
#endif
