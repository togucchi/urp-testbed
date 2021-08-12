using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Toguchi.Rendering
{
    public class Draw2DPostProcessPass : CustomPostProcessingPass<Draw2D>
    {
        private const string Tag = "Draw2DPostProcess";
        private static readonly int DepthTexId = Shader.PropertyToID("_Depth2DTexture");
        private static readonly RenderTargetIdentifier DepthTexIdentifier = new RenderTargetIdentifier(DepthTexId);

        private static readonly int TempBlurBuffer1 = UnityEngine.Shader.PropertyToID("_TempBlurBuffer1");
        private static readonly int TempBlurBuffer2 = UnityEngine.Shader.PropertyToID("_TempBlurBuffer2");
        private static readonly int BlurTexId = UnityEngine.Shader.PropertyToID("_BlurTex");
        
        public Draw2DPostProcessPass(RenderPassEvent renderPassEvent, Shader shader) : base(renderPassEvent, shader)
        {
        }

        protected override string RenderTag => Tag;

        protected override void BeforeRender(CommandBuffer commandBuffer, ref RenderingData renderingData)
        {
            Material.SetFloat("_FocusDistance", Component.FocusDistance.value);
            Material.SetFloat("_FocusRange", Component.FocusRange.value);
            commandBuffer.SetGlobalTexture(DepthTexId, DepthTexIdentifier);
        }

        protected override void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source,
            RenderTargetIdentifier dest)
        {
            ref var cameraData = ref renderingData.cameraData;
            commandBuffer.GetTemporaryRT(TempBlurBuffer1, cameraData.camera.scaledPixelWidth / 3, cameraData.camera.scaledPixelHeight / 3);
            commandBuffer.GetTemporaryRT(TempBlurBuffer2, cameraData.camera.scaledPixelWidth / 3, cameraData.camera.scaledPixelHeight / 3);

            commandBuffer.Blit(source, TempBlurBuffer1, Material);
            commandBuffer.Blit(TempBlurBuffer1, TempBlurBuffer2, Material, 1);
            commandBuffer.Blit(TempBlurBuffer2, TempBlurBuffer1, Material, 2);
            
            commandBuffer.SetGlobalTexture(BlurTexId, TempBlurBuffer1);
            
            commandBuffer.Blit(source, dest, Material, 0);
            
            commandBuffer.ReleaseTemporaryRT(TempBlurBuffer1);
            commandBuffer.ReleaseTemporaryRT(TempBlurBuffer2);
        }

        protected override bool IsActive()
        {
            return true;
        }

    }
}