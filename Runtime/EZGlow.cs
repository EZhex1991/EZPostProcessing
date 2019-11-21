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
        public enum Mode { Outer, Inner }
        public enum EdgeMode { None, Projection, Intersection }
        public enum BlendMode { Additive, Substract, Multiply, Lerp }

        [System.Serializable]
        public class ModeParameter : ParameterOverride<Mode> { }
        [System.Serializable]
        public class BlendModeParameter : ParameterOverride<BlendMode> { }
        [System.Serializable]
        public class EdgeModeParameter : ParameterOverride<EdgeMode> { }

        public ModeParameter mode = new ModeParameter() { value = Mode.Outer };
        public LayerMaskParameter sourceLayer = new LayerMaskParameter();
        public Vector2IntParameter textureResolution = new Vector2IntParameter() { value = new Vector2Int(512, 512) };
        public EdgeModeParameter edgeMode = new EdgeModeParameter() { value = EdgeMode.Intersection };
        [Range(1, 10)]
        public FloatParameter diffusion = new FloatParameter() { value = 7 };

        public BlendModeParameter blendMode = new BlendModeParameter() { value = BlendMode.Additive };
        [UnityEngine.Rendering.PostProcessing.Min(0)]
        public FloatParameter intensity = new FloatParameter() { value = 0 };
        [ColorUsage(false, true)]
        public ColorParameter color = new ColorParameter() { value = Color.white };
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
            GlowOuter,
            GlowInner,
            DownSample,
            UpSample,
        }

        private static class Uniforms
        {
            public static readonly string Name = "EZGlow";
            public static readonly string ShaderName = "Hidden/EZUnity/PostProcessing/EZGlow";
            public static readonly string Keyword_BlendMode = "_BLENDMODE";
            public static readonly string Keyword_EdgeMode = "_EDGEMODE";
            public static int Property_GlowTex = Shader.PropertyToID("_GlowTex");
            public static int Property_GlowBloomTex = Shader.PropertyToID("_GlowBloomTex");
            public static int Property_GlowDepthTex = Shader.PropertyToID("_GlowDepthTex");
            public static int Property_GlowIntensity = Shader.PropertyToID("_GlowIntensity");
            public static int Property_GlowColor = Shader.PropertyToID("_GlowColor");
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

        private Camera glowCamera;
        private RenderTexture glowDepthTexture;

        public override void Init()
        {
            SetupCamera();
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
            if (glowCamera != null)
            {
                RuntimeUtilities.Destroy(glowCamera.gameObject);
            }
            if (glowDepthTexture != null) RenderTexture.ReleaseTemporary(glowDepthTexture);
        }
        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer command = context.command;
            command.BeginSample(Uniforms.Name);
            sheet.ClearKeywords();
            sheet.SetKeyword(Uniforms.Keyword_EdgeMode, settings.edgeMode);
            sheet.SetKeyword(Uniforms.Keyword_BlendMode, settings.blendMode);

            GetGlowTexture(context);
            sheet.properties.SetTexture(Uniforms.Property_GlowDepthTex, glowDepthTexture);

            // Determine the iteration count
            int width = glowDepthTexture.width / 2;
            int height = glowDepthTexture.height / 2;
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
            command.BlitFullscreenTriangle(glowDepthTexture, lastDown, sheet, settings.mode.value == EZGlow.Mode.Outer ? (int)Pass.GlowOuter : (int)Pass.GlowInner);
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

            command.SetGlobalTexture(Uniforms.Property_GlowTex, m_Pyramid[0].down);
            command.SetGlobalTexture(Uniforms.Property_GlowBloomTex, lastUp);
            sheet.properties.SetFloat(Uniforms.Property_GlowIntensity, settings.intensity);
            sheet.properties.SetColor(Uniforms.Property_GlowColor, settings.color);
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
            return settings.edgeMode.value != EZGlow.EdgeMode.None ? DepthTextureMode.Depth : DepthTextureMode.None;
        }

        private void SetupCamera()
        {
            GameObject go = new GameObject(string.Format("EZOuterGlowCamera-{0}", GetHashCode()));
            go.hideFlags = HideFlags.HideAndDontSave;
            glowCamera = go.AddComponent<Camera>();
            glowCamera.enabled = false;
            glowCamera.clearFlags = CameraClearFlags.SolidColor;
            glowCamera.backgroundColor = ClearColor;
            glowCamera.allowMSAA = false;
            glowCamera.allowHDR = false;
        }
        private void GetGlowTexture(PostProcessRenderContext context)
        {
            Vector2Int resolution = settings.textureResolution;
            EZPostProcessingUtility.GetTexture(ref glowDepthTexture, resolution, 16, RenderTextureFormat.Depth);
            EZPostProcessingUtility.CopyCameraSettings(context.camera, glowCamera,
                settings.sourceLayer, DepthTextureMode.Depth);
            glowCamera.targetTexture = glowDepthTexture;
            glowCamera.Render();
        }
    }
}
#endif
