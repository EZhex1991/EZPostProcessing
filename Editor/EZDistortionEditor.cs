/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-23 19:11:16
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEditor.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [PostProcessEditor(typeof(EZDistortion))]
    public class EZDistortionEditor : PostProcessEffectEditor<EZDistortion>
    {
        private SerializedParameterOverride m_DistortionMode;
        private SerializedParameterOverride m_DistortionStrength;
        private SerializedParameterOverride m_DistortionSourceLayer;
        private SerializedParameterOverride m_DistortionTextureResolution;
        private SerializedParameterOverride m_DistortionTextureFormat;
        private SerializedParameterOverride m_DistortionDepth;
        private SerializedParameterOverride m_DistortionTex;

        public override void OnEnable()
        {
            base.OnEnable();
            m_DistortionMode = FindParameterOverride(x => x.distortionMode);
            m_DistortionStrength = FindParameterOverride(x => x.distortionStrength);
            m_DistortionSourceLayer = FindParameterOverride(x => x.distortionSourceLayer);
            m_DistortionTextureResolution = FindParameterOverride(x => x.distortionTextureResolution);
            m_DistortionTextureFormat = FindParameterOverride(x => x.distortionTextureFormat);
            m_DistortionDepth = FindParameterOverride(x => x.distortionDepth); ;
            m_DistortionTex = FindParameterOverride(x => x.distortionTex);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PropertyField(m_DistortionMode);
            PropertyField(m_DistortionStrength);
            int mode = m_DistortionMode.value.intValue;
            if (mode == (int)EZDistortion.Mode.Screen)
            {
                PropertyField(m_DistortionTex);
            }
            else if (mode == (int)EZDistortion.Mode.Layer)
            {
                PropertyField(m_DistortionSourceLayer);
                PropertyField(m_DistortionTextureResolution);
                PropertyField(m_DistortionTextureFormat);
                PropertyField(m_DistortionDepth);
            }
        }
    }
}
