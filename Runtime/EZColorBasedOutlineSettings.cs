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
    [PostProcess(typeof(EZColorBasedOutlineRenderer), PostProcessEvent.BeforeStack, "EZUnity/ColorBasedOutline", allowInSceneView: true)]
    public class EZColorBasedOutlineSettings : PostProcessEffectSettings
    {
        public ColorParameter _GrayWeight = new ColorParameter() { value = new Color(0.299f, 0.587f, 0.114f, 1) };
        public FloatParameter _Tolerance = new FloatParameter() { value = 50 };
        public ColorParameter _OutlineColor = new ColorParameter() { value = Color.black };
        public IntParameter _OutlineThickness = new IntParameter() { value = 1 };
    }
}
#endif
