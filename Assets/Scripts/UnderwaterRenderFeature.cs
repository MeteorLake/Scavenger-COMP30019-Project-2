using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
 
public class UnderwaterRenderFeature : ScriptableRendererFeature
{
    CustomRenderPass customPass;

    public override void Create()
    {
        // Creates the custom pass
        customPass = new CustomRenderPass(
            CoreUtils.CreateEngineMaterial("Hidden/Camera_Shader")
        );
 
        // Configures where the render pass should be injected.
        customPass.renderPassEvent = RenderPassEvent.AfterRendering;
    }
 
    // Runs once per camera, enqueues the custom pass.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customPass);
    }

    public void SetMaterialVars(float currentHeight, float topHeight, float bottomHeight) {
        customPass.SetMaterialVars(currentHeight, topHeight, bottomHeight);
    }
 
    class CustomRenderPass : ScriptableRenderPass
    {
        Material screenMaterial;
        PlayerController playerController;

        RenderTargetHandle tempTexture;

        public CustomRenderPass(Material screenMaterial) {
            this.screenMaterial = screenMaterial;
        }

        // Called prior to executing the render pass
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // create a temporary render texture that matches the camera
            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
        }
        public void SetMaterialVars(float currentHeight, float topHeight, float bottomHeight) {
            screenMaterial.SetFloat("CurrentHeight", currentHeight);
            screenMaterial.SetFloat("TopHeight", topHeight);
            screenMaterial.SetFloat("BottomHeight", bottomHeight);
        }
 
        // Dispatches commands to use during the render pass
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            // if (Application.isPlaying) {
                using (new ProfilingScope(cmd, new ProfilingSampler("Underwater Effect")))
                {
                    RenderTargetIdentifier src = renderingData.cameraData.renderer.cameraColorTarget;
                    Blit(cmd, src, BuiltinRenderTextureType.CurrentActive, screenMaterial);
                }
            // }
            
            // Executes whatever's been pushed to the command buffer
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
 
        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
}