/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-23 14:36:10
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using System;
using UnityEditor;
using UnityEditor.Rendering.PostProcessing;
using UnityEngine;

namespace EZhex1991.EZPostProcessing
{
    [Decorator(typeof(EZMinMaxAttribute))]
    public class EZMinMaxDecorator : AttributeDecorator
    {
        public override bool OnGUI(SerializedProperty property, SerializedProperty overrideState, GUIContent title, Attribute attribute)
        {
            EZMinMaxAttribute minMaxAttribute = attribute as EZMinMaxAttribute;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = EditorGUILayout.Slider(title, property.floatValue, minMaxAttribute.limitMin, minMaxAttribute.limitMax);
                return true;
            }
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUILayout.IntSlider(title, property.intValue, (int)minMaxAttribute.limitMin, (int)minMaxAttribute.limitMax);
                return true;
            }

            Rect position = EditorGUILayout.GetControlRect();
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), title);
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                property.vector2Value = MinMaxSliderV2(position, property.vector2Value, minMaxAttribute.limitMin, minMaxAttribute.limitMax);
                return true;
            }
            if (property.propertyType == SerializedPropertyType.Vector4)
            {
                if (minMaxAttribute.fixedLimit)
                {
                    property.vector4Value = MinMaxSliderV4(position, property.vector4Value, minMaxAttribute.limitMin, minMaxAttribute.limitMax);
                }
                else
                {
                    property.isExpanded = EditorGUI.Foldout(new Rect(position) { width = 0 }, property.isExpanded, GUIContent.none, false);
                    if (property.isExpanded)
                    {
                        property.vector4Value = EditorGUI.Vector4Field(position, "", property.vector4Value);
                    }
                    else
                    {
                        property.vector4Value = MinMaxSliderV4(position, property.vector4Value);
                    }
                }
                return true;
            }

            EditorGUI.HelpBox(position, string.Format("EZMinMaxAttribute not suitable for {0}: {1}", property.type, property.name), MessageType.Warning);
            return false;
        }

        public static Vector2 MinMaxSliderV2(Rect position, Vector2 value, float limitMin, float limitMax)
        {
            float valueRectWidth = 50f;
            float margin = 5f;
            float sliderRectWidth = position.width - (valueRectWidth + margin) * 2f;

            position.width = valueRectWidth;
            value.x = EditorGUI.FloatField(position, value.x);

            position.x += valueRectWidth + margin;
            position.width = sliderRectWidth;
            EditorGUI.MinMaxSlider(position, ref value.x, ref value.y, limitMin, limitMax);

            position.x += sliderRectWidth + margin;
            position.width = valueRectWidth;
            value.y = EditorGUI.FloatField(position, value.y);

            value.x = Mathf.Clamp(value.x, limitMin, limitMax);
            value.y = Mathf.Clamp(value.y, value.x, limitMax);
            return value;
        }
        public static Vector4 MinMaxSliderV4(Rect position, Vector4 value)
        {
            Vector2 valueXY = MinMaxSliderV2(position, value, value.z, value.w);
            value.x = valueXY.x;
            value.y = valueXY.y;
            return value;
        }
        public static Vector4 MinMaxSliderV4(Rect position, Vector4 value, float limitMin, float limitMax)
        {
            Vector2 valueXY = MinMaxSliderV2(position, value, limitMin, limitMax);
            value.x = valueXY.x;
            value.y = valueXY.y;
            return value;
        }
    }
}
#endif
