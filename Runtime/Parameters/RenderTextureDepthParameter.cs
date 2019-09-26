/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-23 11:42:15
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    public enum RenderTextureDepth { None = 0, Bits16 = 16, Bits24 = 24 }
    [System.Serializable]
    public class RenderTextureDepthParameter : ParameterOverride<RenderTextureDepth>
    {
        public static implicit operator RenderTextureDepth(RenderTextureDepthParameter prop) { return prop.value; }
        public static implicit operator int(RenderTextureDepthParameter prop) { return (int)prop.value; }
    }
}
#endif
