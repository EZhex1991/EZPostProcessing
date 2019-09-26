/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-23 14:38:55
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using System;

namespace EZhex1991.EZPostProcessing
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EZMinMaxAttribute : Attribute
    {
        public readonly bool fixedLimit;
        public float limitMin;
        public float limitMax;

        public EZMinMaxAttribute()
        {
            // limits will be retrived from zw component of the vector
            fixedLimit = false;
            limitMin = 0;
            limitMax = 1;
        }
        public EZMinMaxAttribute(float min, float max)
        {
            fixedLimit = true;
            this.limitMin = min;
            this.limitMax = max;
        }
    }
}
#endif
