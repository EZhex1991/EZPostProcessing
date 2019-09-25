/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-19 20:41:20
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EZhex1991.EZPostProcessing
{
    [System.Serializable]
    [PostProcess(typeof(EZDistortionRenderer), PostProcessEvent.BeforeStack, "EZUnity/EZDistortion", allowInSceneView: true)]
    public class EZDistortion : PostProcessEffectSettings
    {
        public enum Mode { Screen, Layer }
        [System.Serializable]
        public class ModeParameter : ParameterOverride<Mode> { }

        public ModeParameter mode = new ModeParameter() { value = Mode.Screen };
        public Vector2Parameter intensity = new Vector2Parameter() { value = new Vector2(0.1f, 0.1f) };

        // Layer Mode
        public LayerMaskParameter sourceLayer = new LayerMaskParameter();
        public Vector2IntParameter textureResolution = new Vector2IntParameter() { value = new Vector2Int(512, 512) };
        public RenderTextureFormatParameter textureFormat = new RenderTextureFormatParameter() { value = RenderTextureFormat.R8 };
        public RenderTextureDepthParameter textureDepth = new RenderTextureDepthParameter() { value = RenderTextureDepth.Bits16 };

        // Screen Mode
        public TextureParameter distortionTex = new TextureParameter() { defaultState = TextureParameterDefault.White };
    }

    public class EZDistortionRenderer : PostProcessEffectRenderer<EZDistortion>
    {
        private static class Uniforms
        {
            public static readonly string ShaderName = "Hidden/EZUnity/PostProcessing/EZDistortion";
            public static readonly string Keyword_DepthTest_On = "_DEPTHTEST_ON";
            public static readonly int Property_DistortionIntensity = Shader.PropertyToID("_DistortionIntensity");
            public static readonly int Property_DistortionTex = Shader.PropertyToID("_DistortionTex");
            public static readonly int Property_DistortionDepthTex = Shader.PropertyToID("_DistortionDepthTex");
        }

        private static Shader m_Shader;
        private static Shader shader
        {
            get
            {
                if (m_Shader == null)
                {
                    m_Shader = Shader.Find(Uniforms.ShaderName);
                }
                return m_Shader;
            }
        }

        private static Color ClearColor = Color.gray;

        private Camera m_DistortionCamera;
        private Camera distortionCamera
        {
            get
            {
                if (m_DistortionCamera == null)
                {
                    SetupCamera();
                }
                return m_DistortionCamera;
            }
        }

        private RenderTexture m_DistortionTex;
        private RenderTexture distortionTex
        {
            get
            {
                Vector2Int resolution = settings.textureResolution;
                if (m_DistortionTex == null
                    || m_DistortionTex.width != resolution.x || m_DistortionTex.height != resolution.y
                    || m_DistortionTex.format != settings.textureFormat
                )
                {
                    m_DistortionTex = new RenderTexture(resolution.x, resolution.y, 0, settings.textureFormat);
                }
                return m_DistortionTex;
            }
        }

        private RenderTexture m_DistortionDepthTex;
        private RenderTexture distortionDepthTex
        {
            get
            {
                Vector2Int resolution = settings.textureResolution;
                if (m_DistortionDepthTex == null
                    || m_DistortionDepthTex.width != resolution.x || m_DistortionDepthTex.height != resolution.y
                    || (settings.textureDepth != RenderTextureDepth.None && m_DistortionDepthTex.depth != settings.textureDepth)
                )
                {
                    m_DistortionDepthTex = new RenderTexture(resolution.x, resolution.y, settings.textureDepth, RenderTextureFormat.Depth);
                }
                return m_DistortionDepthTex;
            }
        }

        public override void Release()
        {
            base.Release();
            if (m_DistortionCamera != null)
            {
                RuntimeUtilities.Destroy(m_DistortionCamera.gameObject);
            }
            if (m_DistortionTex != null) m_DistortionTex.Release();
            if (m_DistortionDepthTex != null) m_DistortionDepthTex.Release();
        }
        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            switch (settings.mode.value)
            {
                case EZDistortion.Mode.Screen:
                    sheet.properties.SetTexture(Uniforms.Property_DistortionTex, settings.distortionTex.value == null ? EZPostProcessingUtility.grayTexture : settings.distortionTex.value);
                    break;
                case EZDistortion.Mode.Layer:
                    GetDistortionTexture(context);
                    sheet.properties.SetTexture(Uniforms.Property_DistortionTex, distortionTex);
                    if (settings.textureDepth.value != RenderTextureDepth.None)
                    {
                        sheet.properties.SetTexture(Uniforms.Property_DistortionDepthTex, distortionDepthTex);
                        sheet.EnableKeyword(Uniforms.Keyword_DepthTest_On);
                    }
                    else
                    {
                        sheet.DisableKeyword(Uniforms.Keyword_DepthTest_On);
                    }
                    break;
            }
            sheet.properties.SetVector(Uniforms.Property_DistortionIntensity, settings.intensity);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
        public override DepthTextureMode GetCameraFlags()
        {
            return settings.textureDepth.value != RenderTextureDepth.None ? (base.GetCameraFlags() | DepthTextureMode.Depth) : base.GetCameraFlags();
        }

        private void SetupCamera()
        {
            GameObject go = new GameObject(string.Format("DistortionCamera-{0}", GetHashCode()));
            go.hideFlags = HideFlags.HideAndDontSave;
            m_DistortionCamera = go.AddComponent<Camera>();
            m_DistortionCamera.enabled = false;
            m_DistortionCamera.clearFlags = CameraClearFlags.SolidColor;
            m_DistortionCamera.backgroundColor = ClearColor;
        }
        private void CopyCameraSettings(Camera src, Camera dst)
        {
            if (src == null || dst == null) return;
            dst.transform.position = src.transform.position;
            dst.transform.rotation = src.transform.rotation;
            dst.orthographic = src.orthographic;
            dst.farClipPlane = src.farClipPlane;
            dst.nearClipPlane = src.nearClipPlane;
            dst.fieldOfView = src.fieldOfView;
            dst.aspect = src.aspect;
            dst.orthographicSize = src.orthographicSize;
        }
        private void GetDistortionTexture(PostProcessRenderContext context)
        {
            CopyCameraSettings(context.camera, distortionCamera);
            distortionCamera.cullingMask = settings.sourceLayer;
            if (settings.textureDepth.value != RenderTextureDepth.None)
            {
                distortionCamera.depthTextureMode = DepthTextureMode.Depth;
                distortionCamera.SetTargetBuffers(distortionTex.colorBuffer, distortionDepthTex.depthBuffer);
            }
            else
            {
                distortionCamera.depthTextureMode = DepthTextureMode.None;
                distortionCamera.targetTexture = distortionTex;
            }
            distortionCamera.Render();
        }
    }
}
#endif
