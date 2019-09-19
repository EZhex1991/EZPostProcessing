/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-19 11:39:34
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [System.Serializable]
    [PostProcess(typeof(EZDepthBasedGradientRenderer), PostProcessEvent.BeforeStack, "EZUnity/DepthBasedGradient", allowInSceneView: false)]
    public class EZDepthBasedGradientSettings : PostProcessEffectSettings
    {
        public ColorParameter _ColorNear = new ColorParameter() { value = Color.white };
        public ColorParameter _ColorFar = new ColorParameter() { value = Color.black };

        public FloatParameter _GradientPower = new FloatParameter() { value = 1 };
        [MinMax(0, 1)]
        public Vector2Parameter _GradientSoftness = new Vector2Parameter() { value = new Vector2(0, 1) };
    }
}
#endif
