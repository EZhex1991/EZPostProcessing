/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-09-25 10:13:21
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
    [PostProcess(typeof(EZGlowRenderer), PostProcessEvent.BeforeStack, "EZUnity/EZGlow", allowInSceneView: true)]
    public class EZGlow : PostProcessEffectSettings
    {
        public enum BlendMode { Additive, Substract, Multiply, Lerp }
        [System.Serializable]
        public class BlendModeParameter : ParameterOverride<BlendMode> { }

        public LayerMaskParameter sourceLayer = new LayerMaskParameter();
        public Vector2IntParameter textureResolution = new Vector2IntParameter() { value = new Vector2Int(512, 512) };
        public BoolParameter depthTest = new BoolParameter() { value = true };
        [Range(1, 10)]
        public FloatParameter diffusion = new FloatParameter() { value = 7 };

        public BlendModeParameter blendMode = new BlendModeParameter() { value = BlendMode.Additive };
        [ColorUsage(false, true)]
        public ColorParameter outerGlowColor = new ColorParameter() { value = Color.white };
        [ColorUsage(false, true)]
        public ColorParameter innerGlowColor = new ColorParameter() { value = Color.black };
    }

    public class EZGlowRenderer : PostProcessEffectRenderer<EZGlow>
    {
        // From bloom
        private struct Level
        {
            public int down;
            public int up;
        }
        private const int k_MaxPyramidSize = 16;
        private Level[] m_Pyramid;
        private RenderTextureFormat pyramidFormat = RenderTextureFormat.R8;

        private enum Pass
        {
            Combine,
            GlowBase,
            DownSample,
            UpSample,
        }

        private static class Uniforms
        {
            public static readonly string Name = "EZGlow";
            public static readonly string ShaderName = "Hidden/EZUnity/PostProcessing/EZGlow";
            public static readonly string Keyword_BlendMode = "_BLENDMODE";
            public static readonly string Keyword_DepthTest_On = "_DEPTHTEST_ON";
            public static int Property_GlowTex = Shader.PropertyToID("_GlowTex");
            public static int Property_GlowBloomTex = Shader.PropertyToID("_GlowBloomTex");
            public static int Property_OuterGlowColor = Shader.PropertyToID("_OuterGlowColor");
            public static int Property_InnerGlowColor = Shader.PropertyToID("_InnerGlowColor");
            public static int Property_SampleScale = Shader.PropertyToID("_SampleScale");
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

        private static readonly Color ClearColor = Color.black;

        private Camera m_GlowCamera;
        private Camera glowCamera
        {
            get
            {
                if (m_GlowCamera == null)
                {
                    SetupCamera();
                }
                return m_GlowCamera;
            }
        }

        private RenderTexture m_GlowTexture;
        private RenderTexture glowTexture
        {
            get
            {
                Vector2Int resolution = settings.textureResolution;
                if (m_GlowTexture == null
                    || m_GlowTexture.width != resolution.x || m_GlowTexture.height != resolution.y
                )
                {
                    m_GlowTexture = RenderTexture.GetTemporary(resolution.x, resolution.y, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Default);
                }
                return m_GlowTexture;
            }
        }

        public override void Init()
        {
            m_Pyramid = new Level[k_MaxPyramidSize];
            for (int i = 0; i < k_MaxPyramidSize; i++)
            {
                m_Pyramid[i] = new Level
                {
                    down = Shader.PropertyToID("_GlowBloomMipDown" + i),
                    up = Shader.PropertyToID("_GlowBloomMipUp" + i)
                };
            }
        }
        public override void Release()
        {
            base.Release();
            if (m_GlowCamera != null)
            {
                RuntimeUtilities.Destroy(m_GlowCamera.gameObject);
            }
            if (m_GlowTexture != null) RenderTexture.ReleaseTemporary(m_GlowTexture);
        }
        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer command = context.command;
            command.BeginSample(Uniforms.Name);
            sheet.ClearKeywords();

            GetGlowTexture(context);

            // Determine the iteration count
            int width = glowTexture.width / 2;
            int height = glowTexture.height / 2;
            int size = Mathf.Max(width, height);
            float logSize = Mathf.Log(size, 2f) + settings.diffusion - 10f;
            int logSizeInt = Mathf.FloorToInt(logSize);
            int iterations = Mathf.Clamp(logSizeInt, 1, k_MaxPyramidSize);
            float sampleScale = 0.5f + logSize - logSizeInt;
            sheet.properties.SetFloat(Uniforms.Property_SampleScale, sampleScale);

            for (int i = 0; i < iterations; i++)
            {
                context.GetScreenSpaceTemporaryRT(command, m_Pyramid[i].down, 0, pyramidFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, width, height);
                context.GetScreenSpaceTemporaryRT(command, m_Pyramid[i].up, 0, pyramidFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear, width, height);
                width = Mathf.Max(width / 2, 1);
                height = Mathf.Max(height / 2, 1);
            }

            int lastDown = m_Pyramid[0].down;
            command.BlitFullscreenTriangle(glowTexture, lastDown, sheet, (int)Pass.GlowBase);
            for (int i = 1; i < iterations; i++)
            {
                int mipDown = m_Pyramid[i].down;
                command.BlitFullscreenTriangle(lastDown, mipDown, sheet, (int)Pass.DownSample);
                lastDown = mipDown;
            }

            int lastUp = m_Pyramid[iterations - 1].down;
            for (int i = iterations - 2; i >= 0; i--)
            {
                int mipDown = m_Pyramid[i].down;
                int mipUp = m_Pyramid[i].up;
                command.SetGlobalTexture(Uniforms.Property_GlowBloomTex, mipDown);
                command.BlitFullscreenTriangle(lastUp, mipUp, sheet, (int)Pass.UpSample);
                lastUp = mipUp;
            }

            command.SetGlobalTexture(Uniforms.Property_GlowBloomTex, lastUp);
            sheet.properties.SetTexture(Uniforms.Property_GlowTex, glowTexture);
            sheet.properties.SetColor(Uniforms.Property_OuterGlowColor, settings.outerGlowColor);
            sheet.properties.SetColor(Uniforms.Property_InnerGlowColor, settings.innerGlowColor);
            sheet.SetKeyword(Uniforms.Keyword_DepthTest_On, settings.depthTest);
            sheet.SetKeyword(Uniforms.Keyword_BlendMode, settings.blendMode);
            command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)Pass.Combine);

            for (int i = 0; i < iterations; i++)
            {
                command.ReleaseTemporaryRT(m_Pyramid[i].down);
                command.ReleaseTemporaryRT(m_Pyramid[i].up);
            }

            command.EndSample(Uniforms.Name);
        }
        public override DepthTextureMode GetCameraFlags()
        {
            return settings.depthTest ? DepthTextureMode.Depth : DepthTextureMode.None;
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
            GameObject go = new GameObject(string.Format("EZOuterGlowCamera-{0}", GetHashCode()));
            go.hideFlags = HideFlags.HideAndDontSave;
            m_GlowCamera = go.AddComponent<Camera>();
            m_GlowCamera.enabled = false;
            m_GlowCamera.clearFlags = CameraClearFlags.SolidColor;
            m_GlowCamera.backgroundColor = ClearColor;
        }
        private void GetGlowTexture(PostProcessRenderContext context)
        {
            CopyCameraSettings(context.camera, glowCamera);
            glowCamera.cullingMask = settings.sourceLayer;
            glowCamera.depthTextureMode = DepthTextureMode.Depth;
            glowCamera.targetTexture = glowTexture;
            glowCamera.Render();
        }
    }
}
#endif
