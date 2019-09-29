/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-23 16:47:02
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    public static class EZPostProcessingUtility
    {
        // Textures
        private static Texture2D m_WhiteTexture;
        private static Texture3D m_WhiteTexture3D;
        private static Texture2D m_BlackTexture;
        private static Texture3D m_BlackTexture3D;
        private static Texture2D m_GrayTexture;
        private static Texture3D m_GrayTexture3D;
        private static Texture2D m_TransparentTexture;
        private static Texture3D m_TransparentTexture3D;
        public static Texture2D SingleColorTexture(Color color, TextureFormat format = TextureFormat.ARGB32, string name = "")
        {
            if (string.IsNullOrEmpty(name)) name = "Texture-" + ColorUtility.ToHtmlStringRGBA(color);
            Texture2D texture = new Texture2D(1, 1, format, false) { name = name };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
        public static Texture3D SingleColorTexture3D(Color color, TextureFormat format = TextureFormat.ARGB32, string name = "")
        {
            if (string.IsNullOrEmpty(name)) name = "Texture-" + ColorUtility.ToHtmlStringRGBA(color);
            Texture3D texture = new Texture3D(1, 1, 1, format, false) { name = name };
            texture.SetPixels(new Color[] { color });
            texture.Apply();
            return texture;
        }
        public static Texture2D whiteTexture
        {
            get
            {
                if (m_WhiteTexture == null)
                {
                    m_WhiteTexture = SingleColorTexture(Color.white, TextureFormat.ARGB32, "Texture-White");
                }
                return m_WhiteTexture;
            }
        }
        public static Texture3D whiteTexture3D
        {
            get
            {
                if (m_WhiteTexture3D == null)
                {
                    m_WhiteTexture3D = SingleColorTexture3D(Color.white, TextureFormat.ARGB32, "Texture3D-White");
                }
                return m_WhiteTexture3D;
            }
        }
        public static Texture2D blackTexture
        {
            get
            {
                if (m_BlackTexture == null)
                {
                    m_BlackTexture = SingleColorTexture(Color.black, TextureFormat.ARGB32, "Texture-Black");
                }
                return m_BlackTexture;
            }
        }
        public static Texture3D blackTexture3D
        {
            get
            {
                if (m_BlackTexture3D == null)
                {
                    m_BlackTexture3D = SingleColorTexture3D(Color.black, TextureFormat.ARGB32, "Texture3D-Black");
                }
                return m_BlackTexture3D;
            }
        }
        public static Texture2D grayTexture
        {
            get
            {
                if (m_GrayTexture == null)
                {
                    m_GrayTexture = SingleColorTexture(Color.gray, TextureFormat.ARGB32, "Texture-Gray");
                }
                return m_GrayTexture;
            }
        }
        public static Texture3D grayTexture3D
        {
            get
            {
                if (m_GrayTexture3D == null)
                {
                    m_GrayTexture3D = SingleColorTexture3D(Color.gray, TextureFormat.ARGB32, "Texture3D-Gray");
                }
                return m_GrayTexture3D;
            }
        }
        public static Texture2D transparentTexture
        {
            get
            {
                if (m_TransparentTexture == null)
                {
                    m_TransparentTexture = SingleColorTexture(Color.clear, TextureFormat.ARGB32, "Texture-Transparent");
                }
                return m_TransparentTexture;
            }
        }
        public static Texture3D transparentTexture3D
        {
            get
            {
                if (m_TransparentTexture3D == null)
                {
                    m_TransparentTexture3D = SingleColorTexture3D(Color.clear, TextureFormat.ARGB32, "Texture3D-Transparent");
                }
                return m_TransparentTexture3D;
            }
        }

        public static void SetKeyword(this PropertySheet sheet, string keyword, bool value)
        {
            if (value)
            {
                sheet.EnableKeyword(keyword);
            }
            else
            {
                sheet.DisableKeyword(keyword);
            }
        }

        public static string FormatKeyword(string prefix, Enum selection)
        {
            return string.Format("{0}_{1}", prefix, selection).ToUpperInvariant();
        }
        public static void SetKeyword(this PropertySheet sheet, string prefix, Enum selection)
        {
            foreach (Enum value in Enum.GetValues(selection.GetType()))
            {
                sheet.DisableKeyword(FormatKeyword(prefix, value));
            }
            sheet.EnableKeyword(FormatKeyword(prefix, selection));
        }
    }
}
#endif
