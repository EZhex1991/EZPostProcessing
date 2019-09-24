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
    [System.Serializable]
    public class Vector2IntParameter : ParameterOverride<Vector2Int>
    {
        public static implicit operator Vector2Int(Vector2IntParameter prop) { return prop.value; }
        public static implicit operator Vector2(Vector2IntParameter prop) { return prop.value; }
    }

    [System.Serializable]
    public class LayerMaskParameter : ParameterOverride<LayerMask>
    {
        public static implicit operator LayerMask(LayerMaskParameter prop) { return prop.value; }
        public static implicit operator int(LayerMaskParameter prop) { return prop.value; }
    }

    [System.Serializable]
    public class RenderTextureFormatParameter : ParameterOverride<RenderTextureFormat>
    {
        public static implicit operator RenderTextureFormat(RenderTextureFormatParameter prop) { return prop.value; }
    }

    public enum RenderTextureDepth { None = 0, Bits16 = 16, Bits24 = 24 }
    [System.Serializable]
    public class RenderTextureDepthParameter : ParameterOverride<RenderTextureDepth>
    {
        public static implicit operator RenderTextureDepth(RenderTextureDepthParameter prop) { return prop.value; }
        public static implicit operator int(RenderTextureDepthParameter prop) { return (int)prop.value; }
    }
}
#endif
