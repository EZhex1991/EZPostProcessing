/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-25 10:16:32
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [System.Serializable]
    public class RenderTextureFormatParameter : ParameterOverride<RenderTextureFormat>
    {
        public static implicit operator RenderTextureFormat(RenderTextureFormatParameter prop) { return prop.value; }
    }
}
#endif
