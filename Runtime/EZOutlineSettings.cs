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
    [PostProcess(typeof(EZOutlineRenderer), PostProcessEvent.BeforeTransparent, "EZUnity/Outline", allowInSceneView: false)]
    public class EZOutlineSettings : PostProcessEffectSettings
    {
        public IntParameter _SampleDistance = new IntParameter() { value = 1 };

        public FloatParameter _DepthSensitivity = new FloatParameter() { value = 5 };
        public FloatParameter _NormalSensitivity = new FloatParameter();

        public ColorParameter _CoverColor = new ColorParameter();
        [Range(0, 1)]
        public FloatParameter _CoverStrength = new FloatParameter() { value = 0 };

        public ColorParameter _OutlineColor = new ColorParameter();
        [Range(0, 1)]
        public FloatParameter _OutlineStrength = new FloatParameter() { value = 1 };
    }
}
#endif
