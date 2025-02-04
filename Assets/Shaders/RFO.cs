using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RFO : ScriptableRendererFeature
{
    class RFOPass : ScriptableRenderPass
    {
        // shader pass indices
        const int SHADER_PASS_INTERIOR_STENCIL = 0;
        const int SHADER_PASS_SILHOUETTE_BUFFER_FILL = 1;
        const int SHADER_PASS_JFA_INIT = 2;
        const int SHADER_PASS_JFA_FLOOD = 3;
        const int SHADER_PASS_JFA_FLOOD_SINGLE_AXIS = 4;
        const int SHADER_PASS_JFA_OUTLINE = 5;

        private int nearestPointID = Shader.PropertyToID("_NearestPoint");
        private int nearestPointPingPongID = Shader.PropertyToID("_NearestPointPingPong");
        private int silhouetteRT = Shader.PropertyToID("_SilhouetteBuffer");

        // shader properties
        private int outlineColorID = Shader.PropertyToID("_OutlineColor");
        private int outlineWidthID = Shader.PropertyToID("_OutlineWidth");
        private int axisWidthID = Shader.PropertyToID("_AxisWidth");
        private int stepWidthID = Shader.PropertyToID("_StepWidth");

        private Material outlineMat = null;
        private LayerMask targetLayer;
        private Color outlineColor;
        private float outlineWidth;
        private bool useSeparableAxisMethod;

        public void Setup(LayerMask targetLayer, Color outlineColor, float outlineWidth, bool useSeparableAxisMethod)
        {
            this.targetLayer = targetLayer;
            this.outlineColor = outlineColor;
            // unity will crash if outline width is less than zero
            this.outlineWidth = outlineWidth > 0 ? outlineWidth : 0;
            this.useSeparableAxisMethod = useSeparableAxisMethod;
        }

        public RFOPass(Material outlineMaterial)
        {
            outlineMat = outlineMaterial;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera != ArenaCamera.CurrentCamera)
                return;

            // SETUP
            CommandBuffer cb = CommandBufferPool.Get("PostProcessOutline");

            RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDescriptor.depthBufferBits = 0;

            var renderer = renderingData.cameraData.renderer;

            int msaa = Mathf.Max(1, QualitySettings.antiAliasing);

            int width = renderingData.cameraData.cameraTargetDescriptor.width;
            int height = renderingData.cameraData.cameraTargetDescriptor.height;

            RenderTextureDescriptor silhouetteDescriptor = new RenderTextureDescriptor
            {
                dimension = TextureDimension.Tex2D,
                graphicsFormat = GraphicsFormat.R8_UNorm,

                width = width,
                height = height,

                msaaSamples = msaa,
                depthBufferBits = 0,

                sRGB = false,

                useMipMap = false,
                autoGenerateMips = false
            };

            // PHASE 1: PREPARE OUTLINE SILHOUETTE RT
            cb.GetTemporaryRT(silhouetteRT, silhouetteDescriptor, FilterMode.Point);
            cb.SetRenderTarget(silhouetteRT);
            cb.ClearRenderTarget(true, true, Color.clear);

            context.ExecuteCommandBuffer(cb);
            cb.Clear();

            // PHASE 2: OUTLINE PROCESS INITIALIZE
            Color adjustedOutlineColor = outlineColor;
            adjustedOutlineColor.a *= Mathf.Clamp01(outlineWidth);
            cb.SetGlobalColor(outlineColorID, adjustedOutlineColor.linear);
            cb.SetGlobalFloat(outlineWidthID, Mathf.Max(1f, outlineWidth));


            // PHASE 3: DRAW SILHOUETTE WITH LAYER MASK
            var renderQueueRange = new RenderQueueRange(0, (int)RenderQueue.GeometryLast);
            FilteringSettings filters = new FilteringSettings(renderQueueRange, targetLayer.value);

            var jumpFloodDescriptor = silhouetteDescriptor;
            jumpFloodDescriptor.msaaSamples = 1;
            jumpFloodDescriptor.graphicsFormat = GraphicsFormat.R16G16_SNorm;

            cb.GetTemporaryRT(nearestPointID, jumpFloodDescriptor, FilterMode.Point);
            cb.GetTemporaryRT(nearestPointPingPongID, jumpFloodDescriptor, FilterMode.Point);

            int numMips = Mathf.CeilToInt(Mathf.Log(outlineWidth + 1.0f, 2f));
            int jfaIter = numMips - 1;

            DrawingSettings drawingSettings = CreateDrawingSettings(
                // maybe you want to change this if you're not using forward
                new ShaderTagId("SRPDefaultUnlit"),
                ref renderingData,
                SortingCriteria.CommonOpaque
            );

            drawingSettings.overrideMaterial = outlineMat;
            drawingSettings.overrideMaterialPassIndex = 1;

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filters);
            context.Submit();


            // PHASE 4: GENERATE THE NEAREST DISTANCE EACH POINT NEAR THE EDGE BY JUMP FLOOD ALGORITHM
            cb.Blit(silhouetteRT, nearestPointID, outlineMat, SHADER_PASS_JFA_INIT);

            // Alan Wolfe's separable axis JFA - https://www.shadertoy.com/view/Mdy3D3
            if (useSeparableAxisMethod)
            {

                // jfa init
                cb.Blit(silhouetteRT, nearestPointID, outlineMat, SHADER_PASS_JFA_INIT);

                // jfa flood passes
                for (int i = jfaIter; i >= 0; i--)
                {
                    // calculate appropriate jump width for each iteration
                    // + 0.5 is just me being cautious to avoid any floating point math rounding errors
                    float stepWidth = Mathf.Pow(2, i) + 0.5f;

                    // the two separable passes, one axis at a time
                    cb.SetGlobalVector(axisWidthID, new Vector2(stepWidth, 0f));
                    cb.Blit(nearestPointID, nearestPointPingPongID, outlineMat, SHADER_PASS_JFA_FLOOD_SINGLE_AXIS);
                    cb.SetGlobalVector(axisWidthID, new Vector2(0f, stepWidth));
                    cb.Blit(nearestPointPingPongID, nearestPointID, outlineMat, SHADER_PASS_JFA_FLOOD_SINGLE_AXIS);
                }
            }

            // traditional JFA
            else
            {
                // choose a starting buffer so we always finish on the same buffer
                int startBufferID = (jfaIter % 2 == 0) ? nearestPointPingPongID : nearestPointID;

                // jfa init
                cb.Blit(silhouetteRT, startBufferID, outlineMat, SHADER_PASS_JFA_INIT);

                // jfa flood passes
                for (int i = jfaIter; i >= 0; i--)
                {
                    // calculate appropriate jump width for each iteration
                    // + 0.5 is just me being cautious to avoid any floating point math rounding errors
                    cb.SetGlobalFloat(stepWidthID, Mathf.Pow(2, i) + 0.5f);

                    // ping pong between buffers
                    if (i % 2 == 1)
                        cb.Blit(nearestPointID, nearestPointPingPongID, outlineMat, SHADER_PASS_JFA_FLOOD);
                    else
                        cb.Blit(nearestPointPingPongID, nearestPointID, outlineMat, SHADER_PASS_JFA_FLOOD);
                }
            }

            // PHASE 5: DRAW OUTLINE TO CAMERA
            cb.Blit(nearestPointID, renderer.cameraColorTargetHandle, outlineMat, SHADER_PASS_JFA_OUTLINE);


            cb.ReleaseTemporaryRT(silhouetteRT);
            cb.ReleaseTemporaryRT(nearestPointID);
            cb.ReleaseTemporaryRT(nearestPointPingPongID);


            context.ExecuteCommandBuffer(cb);
            CommandBufferPool.Release(cb);
        }

        public override void FrameCleanup(CommandBuffer commandBuffer)
        {
            commandBuffer.ReleaseTemporaryRT(silhouetteRT);
            commandBuffer.ReleaseTemporaryRT(nearestPointID);
            commandBuffer.ReleaseTemporaryRT(nearestPointPingPongID);
        }
    }

    [System.Serializable]
    public class RFOSettings
    {
        public Material outlineMaterial;
        public LayerMask targetLayerMask;
        public Color outlineColor;
        public float outlineWidth;
        public bool useSeparableAxisMethod;
    }

    public RFOSettings settings = new();
    RFOPass outlinePass;

    public override void Create()
    {
        outlinePass = new RFOPass(settings.outlineMaterial);
        outlinePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null)
        {
            Debug.LogError("Missing Outline Material!");
            return;
        }

        outlinePass.Setup(
            settings.targetLayerMask,
            settings.outlineColor,
            settings.outlineWidth,
            settings.useSeparableAxisMethod);

        renderer.EnqueuePass(outlinePass);
    }
}
