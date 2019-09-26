/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-25 10:14:48
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
}
#endif
