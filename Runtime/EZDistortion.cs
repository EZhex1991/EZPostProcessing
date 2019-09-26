/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-19 20:41:20
 * Organization:    #ORGANIZATION#
 * Description:     
 */
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering;
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

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return base.IsEnabledAndSupported(context) && intensity.value.x > 0f && intensity.value.y > 0f;
        }
    }

    public class EZDistortionRenderer : PostProcessEffectRenderer<EZDistortion>
    {
        private static class Uniforms
        {
            public static readonly string Name = "EZDistortion";
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

        private static readonly Color ClearColor = Color.gray;

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

        private RenderTexture m_DistortionTexture;
        private RenderTexture distortionTexture
        {
            get
            {
                Vector2Int resolution = settings.textureResolution;
                if (m_DistortionTexture == null
                    || m_DistortionTexture.width != resolution.x || m_DistortionTexture.height != resolution.y
                    || m_DistortionTexture.format != settings.textureFormat
                )
                {
                    m_DistortionTexture = RenderTexture.GetTemporary(resolution.x, resolution.y, 0, settings.textureFormat, RenderTextureReadWrite.Default);
                }
                return m_DistortionTexture;
            }
        }

        private RenderTexture m_DistortionDepthTexture;
        private RenderTexture distortionDepthTexture
        {
            get
            {
                Vector2Int resolution = settings.textureResolution;
                if (m_DistortionDepthTexture == null
                    || m_DistortionDepthTexture.width != resolution.x || m_DistortionDepthTexture.height != resolution.y
                    || (settings.textureDepth.value != RenderTextureDepth.None && m_DistortionDepthTexture.depth != settings.textureDepth)
                )
                {
                    m_DistortionDepthTexture = RenderTexture.GetTemporary(resolution.x, resolution.y, settings.textureDepth, RenderTextureFormat.Depth, RenderTextureReadWrite.Default);
                }
                return m_DistortionDepthTexture;
            }
        }

        public override void Release()
        {
            base.Release();
            if (m_DistortionCamera != null)
            {
                RuntimeUtilities.Destroy(m_DistortionCamera.gameObject);
            }
            if (m_DistortionTexture != null) RenderTexture.ReleaseTemporary(m_DistortionTexture);
            if (m_DistortionDepthTexture != null) RenderTexture.ReleaseTemporary(m_DistortionDepthTexture);
        }
        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer command = context.command;
            command.BeginSample(Uniforms.Name);
            sheet.ClearKeywords();

            switch (settings.mode.value)
            {
                case EZDistortion.Mode.Screen:
                    sheet.properties.SetTexture(Uniforms.Property_DistortionTex, settings.distortionTex.value == null ? EZPostProcessingUtility.grayTexture : settings.distortionTex.value);
                    break;
                case EZDistortion.Mode.Layer:
                    GetDistortionTexture(context);
                    sheet.properties.SetTexture(Uniforms.Property_DistortionTex, distortionTexture);
                    if (settings.textureDepth.value != RenderTextureDepth.None)
                    {
                        sheet.properties.SetTexture(Uniforms.Property_DistortionDepthTex, distortionDepthTexture);
                        sheet.EnableKeyword(Uniforms.Keyword_DepthTest_On);
                    }
                    break;
            }
            sheet.properties.SetVector(Uniforms.Property_DistortionIntensity, settings.intensity);
            command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            command.EndSample(Uniforms.Name);
        }
        public override DepthTextureMode GetCameraFlags()
        {
            return settings.textureDepth.value != RenderTextureDepth.None ? (base.GetCameraFlags() | DepthTextureMode.Depth) : base.GetCameraFlags();
        }

        private static void CopyCameraSettings(Camera src, Camera dst)
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
        private void SetupCamera()
        {
            GameObject go = new GameObject(string.Format("EZDistortionCamera-{0}", GetHashCode()));
            go.hideFlags = HideFlags.HideAndDontSave;
            m_DistortionCamera = go.AddComponent<Camera>();
            m_DistortionCamera.enabled = false;
            m_DistortionCamera.clearFlags = CameraClearFlags.SolidColor;
            m_DistortionCamera.backgroundColor = ClearColor;
        }
        private void GetDistortionTexture(PostProcessRenderContext context)
        {
            CopyCameraSettings(context.camera, distortionCamera);
            distortionCamera.cullingMask = settings.sourceLayer;
            if (settings.textureDepth.value != RenderTextureDepth.None)
            {
                distortionCamera.depthTextureMode = DepthTextureMode.Depth;
                distortionCamera.SetTargetBuffers(distortionTexture.colorBuffer, distortionDepthTexture.depthBuffer);
            }
            else
            {
                distortionCamera.depthTextureMode = DepthTextureMode.None;
                distortionCamera.targetTexture = distortionTexture;
            }
            distortionCamera.Render();
        }
    }
}
#endif
