/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-25 10:16:12
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [System.Serializable]
    public class LayerMaskParameter : ParameterOverride<LayerMask>
    {
        public static implicit operator LayerMask(LayerMaskParameter prop) { return prop.value; }
        public static implicit operator int(LayerMaskParameter prop) { return prop.value; }
    }
}
#endif
