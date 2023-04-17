using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DepthNormalsFeature : ScriptableRendererFeature
{
    class DepthNormalsPass : ScriptableRenderPass
    {

        private Material m_depthMaterial;

        public DepthNormalsPass(Material depthMaterial)//(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
        {
            m_depthMaterial = depthMaterial;
        }

        public void Setup(RenderTextureDescriptor baseDescriptor, RTHandle depthAttachmentHandle)
        {
  
            
        }


        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {

        }

      
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
    }
    [SerializeField]
    private Shader m_depthNormalShader;

    private Material m_depthNormalMaterial;

    private DepthNormalsPass m_depthNormalPass;
    public override void Create()
    {
        m_depthNormalMaterial = CoreUtils.CreateEngineMaterial(m_depthNormalShader);
        m_depthNormalPass = new DepthNormalsPass(m_depthNormalMaterial);
    }


    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_depthNormalMaterial);
        
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_depthNormalPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if(renderingData.cameraData.cameraType == CameraType.Game)
        { 
            //m_depthNormalPass.ConfigureInput(ScriptableRenderPassInput.Depth);
            //m_depthNormalPass.ConfigureInput(ScriptableRenderPassInput.Normal);
            //m_depthNormalPass.ConfigureTarget
        }
    }
}
