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
        public BoolParameter depthTest = new BoolParameter() { value = true };

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

        private Camera distortionCamera;
        private RenderTexture distortionTex;
        private RenderTexture distortionDepthTex;

        public override void Init()
        {
            base.Init();
            SetupCamera();
        }
        public override void Release()
        {
            base.Release();
            if (distortionCamera != null)
            {
                RuntimeUtilities.Destroy(distortionCamera.gameObject);
            }
            if (distortionTex != null) RenderTexture.ReleaseTemporary(distortionTex);
            if (distortionDepthTex != null) RenderTexture.ReleaseTemporary(distortionDepthTex);
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
                    sheet.properties.SetTexture(Uniforms.Property_DistortionTex, distortionTex);
                    if (settings.depthTest)
                    {
                        sheet.properties.SetTexture(Uniforms.Property_DistortionDepthTex, distortionDepthTex);
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
            return settings.depthTest ? DepthTextureMode.Depth : DepthTextureMode.None;
        }

        private void SetupCamera()
        {
            GameObject go = new GameObject(string.Format("EZDistortionCamera-{0}", GetHashCode()));
            go.hideFlags = HideFlags.HideAndDontSave;
            distortionCamera = go.AddComponent<Camera>();
            distortionCamera.enabled = false;
            distortionCamera.clearFlags = CameraClearFlags.SolidColor;
            distortionCamera.backgroundColor = ClearColor;
        }
        private void GetDistortionTexture(PostProcessRenderContext context)
        {
            Vector2Int resolution = settings.textureResolution;
            EZPostProcessingUtility.GetTexture(ref distortionTex, resolution, 0, settings.textureFormat);
            if (settings.depthTest)
            {
                EZPostProcessingUtility.GetTexture(ref distortionDepthTex, resolution, 16, RenderTextureFormat.Depth);
                EZPostProcessingUtility.CopyCameraSettings(context.camera, distortionCamera,
                    settings.sourceLayer, DepthTextureMode.Depth);
                distortionCamera.SetTargetBuffers(distortionTex.colorBuffer, distortionDepthTex.depthBuffer);
            }
            else
            {
                EZPostProcessingUtility.CopyCameraSettings(context.camera, distortionCamera,
                    settings.sourceLayer, DepthTextureMode.None);
                distortionCamera.targetTexture = distortionTex;
            }
            distortionCamera.Render();
        }
    }
}
#endif
