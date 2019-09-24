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

        public ModeParameter distortionMode = new ModeParameter() { value = Mode.Screen };
        public Vector2Parameter distortionStrength = new Vector2Parameter() { value = new Vector2(0.1f, 0.1f) };

        // Layer Mode
        public LayerMaskParameter distortionSourceLayer = new LayerMaskParameter();
        public Vector2IntParameter distortionTextureResolution = new Vector2IntParameter() { value = new Vector2Int(512, 512) };
        public RenderTextureFormatParameter distortionTextureFormat = new RenderTextureFormatParameter() { value = RenderTextureFormat.R8 };
        public RenderTextureDepthParameter distortionDepth = new RenderTextureDepthParameter() { value = RenderTextureDepth.Bits16 };

        // Screen Mode
        public TextureParameter distortionTex = new TextureParameter() { defaultState = TextureParameterDefault.White };
    }

    public class EZDistortionRenderer : PostProcessEffectRenderer<EZDistortion>
    {
        private const string ShaderName = "Hidden/EZUnity/PostProcessing/EZDistortion";
        private const string Keyword_DepthTest_On = "_DEPTHTEST_ON";
        private static Shader m_Shader;
        private static Shader shader
        {
            get
            {
                if (m_Shader == null)
                {
                    m_Shader = Shader.Find(ShaderName);
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
                Vector2Int resolution = settings.distortionTextureResolution;
                if (m_DistortionTex == null
                    || m_DistortionTex.width != resolution.x || m_DistortionTex.height != resolution.y
                    || m_DistortionTex.format != settings.distortionTextureFormat
                )
                {
                    m_DistortionTex = new RenderTexture(resolution.x, resolution.y, 0, settings.distortionTextureFormat);
                }
                return m_DistortionTex;
            }
        }

        private RenderTexture m_DistortionDepthTex;
        private RenderTexture distortionDepthTex
        {
            get
            {
                Vector2Int resolution = settings.distortionTextureResolution;
                if (m_DistortionDepthTex == null
                    || m_DistortionDepthTex.width != resolution.x || m_DistortionDepthTex.height != resolution.y
                    || (settings.distortionDepth != RenderTextureDepth.None && m_DistortionDepthTex.depth != settings.distortionDepth)
                )
                {
                    m_DistortionDepthTex = new RenderTexture(resolution.x, resolution.y, settings.distortionDepth, RenderTextureFormat.Depth);
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
            m_DistortionTex.Release();
        }
        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            switch (settings.distortionMode.value)
            {
                case EZDistortion.Mode.Screen:
                    sheet.properties.SetTexture("_DistortionTex", settings.distortionTex.value == null ? EZPostProcessingUtility.grayTexture : settings.distortionTex.value);
                    break;
                case EZDistortion.Mode.Layer:
                    GetDistortionTexture(context);
                    sheet.properties.SetTexture("_DistortionTex", distortionTex);
                    if (settings.distortionDepth.value != RenderTextureDepth.None)
                    {
                        sheet.properties.SetTexture("_DistortionDepthTex", distortionDepthTex);
                        sheet.EnableKeyword(Keyword_DepthTest_On);
                    }
                    else
                    {
                        sheet.DisableKeyword(Keyword_DepthTest_On);
                    }
                    break;
            }
            sheet.properties.SetVector("_DistortionStrength", settings.distortionStrength);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
        public override DepthTextureMode GetCameraFlags()
        {
            return settings.distortionDepth.value != RenderTextureDepth.None ? (base.GetCameraFlags() | DepthTextureMode.Depth) : base.GetCameraFlags();
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
            distortionCamera.cullingMask = settings.distortionSourceLayer;
            if (settings.distortionDepth.value != RenderTextureDepth.None)
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
