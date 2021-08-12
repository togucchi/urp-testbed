using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Toguchi.Rendering
{
    public class Draw2DPass : ScriptableRenderPass
    {
        private const string Tag = "Draw2D";

        private static readonly ShaderTagId DepthTagId = new ShaderTagId("Draw2DDepth");
        private static readonly int DepthTexId = Shader.PropertyToID("_Depth2DTexture");
        private static readonly RenderTargetIdentifier DepthTexIdentifier = new RenderTargetIdentifier(DepthTexId);
        
        private RenderTargetIdentifier _colorIdentifier;
        private RenderStateBlock _renderStateBlock;
        private FilteringSettings _filteringSettings;

        public Draw2DPass(RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public void Setup(in RenderTargetIdentifier colorIdentifier)
        {
            _colorIdentifier = colorIdentifier;

            _filteringSettings = new FilteringSettings(RenderQueueRange.all);
            _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            var camera = renderingData.cameraData.camera;
            var width = camera.scaledPixelWidth;
            var height = camera.scaledPixelHeight;

            var depthTexDesc = new RenderTextureDescriptor(width, height, RenderTextureFormat.R8, 0);
            cmd.GetTemporaryRT(DepthTexId, depthTexDesc);
            cmd.SetRenderTarget(DepthTexIdentifier);
            cmd.ClearRenderTarget(false, true, Color.black);

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var commandBuffer = CommandBufferPool.Get(Tag);
            
            commandBuffer.SetRenderTarget(DepthTexIdentifier);
            context.ExecuteCommandBuffer(commandBuffer);
            
            var drawingSettings = CreateDrawingSettings(DepthTagId, ref renderingData, SortingCriteria.SortingLayer);
            var filteringSettings = _filteringSettings;
            
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref _renderStateBlock);
            
            context.ExecuteCommandBuffer(commandBuffer);

            CommandBufferPool.Release(commandBuffer);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            
            
            cmd.ReleaseTemporaryRT(DepthTexId);
        }
    }
}