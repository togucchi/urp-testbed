using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Toguchi.Rendering
{
    public class TexDrawPass : ScriptableRenderPass
    {
        public static bool isRecord = false;
        public static string outputName = "output";
        
        protected static readonly int TempColorBufferId = UnityEngine.Shader.PropertyToID("_TempColorBuffer");
        private const string RenderTag = "DrawTex";

        private readonly ShaderTagId _depthTag = new ShaderTagId("DepthOnly");

        private readonly List<ShaderTagId> _colorTags =
            new List<ShaderTagId>(new []{new ShaderTagId("UniversalForward"), new ShaderTagId("SRPDefaultUnlit")});
        
        
        protected Shader Shader;
        protected Material Material;

        private RenderTexture _depthOutputTexture;
        private RenderTexture _colorOutputTexture;

        public TexDrawPass(RenderPassEvent renderPassEvent, Shader shader)
        {
            this.renderPassEvent = renderPassEvent;
            Shader = shader;

            if (shader == null)
            {
                return;
            }
            
            Material = CoreUtils.CreateEngineMaterial(shader);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            
            var width = renderingData.cameraData.camera.pixelWidth;
            var height = renderingData.cameraData.camera.pixelHeight;
            
            if (isRecord && _depthOutputTexture != null && _colorOutputTexture != null)
            {
                isRecord = false;

                // Output Depth
                Texture2D outputTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                var oldTex = RenderTexture.active;
                RenderTexture.active = _depthOutputTexture;
                outputTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                outputTex.Apply();

                var bytes = outputTex.EncodeToPNG();
                File.WriteAllBytes($"{Application.streamingAssetsPath}/{outputName}-depth.png", bytes);

                RenderTexture.active = _colorOutputTexture;
                outputTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                outputTex.Apply();
                
                bytes = outputTex.EncodeToPNG();
                File.WriteAllBytes($"{Application.streamingAssetsPath}/{outputName}-color.png", bytes);

                RenderTexture.active = oldTex;
            }

            if (_depthOutputTexture == null)
            {
                var depthDesc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32);
                _depthOutputTexture = new RenderTexture(depthDesc);
            }

            if (_colorOutputTexture == null)
            {
                var colorDesc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32);
                _colorOutputTexture = new RenderTexture(colorDesc);
            }
        }

        public void Dispose()
        {
            if (_depthOutputTexture != null)
            {
                _depthOutputTexture.Release();
                _depthOutputTexture = null;
            }

            if (_colorOutputTexture != null)
            {
                _colorOutputTexture.Release();
                _colorOutputTexture = null;
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (Material == null || !renderingData.cameraData.postProcessEnabled)
            {
                return;
            }
            
            var volumeStack = VolumeManager.instance.stack;
            var component = volumeStack.GetComponent<TexDraw>();
            if (component == null)
            {
                return;
            }

            var commandBuffer = CommandBufferPool.Get(RenderTag);

            ref var cameraData = ref renderingData.cameraData;
            var width = cameraData.camera.scaledPixelWidth;
            var height = cameraData.camera.scaledPixelHeight;
            
            // Draw Depth
            commandBuffer.GetTemporaryRT(TempColorBufferId, width, height, 24, FilterMode.Point, RenderTextureFormat.Depth);
            commandBuffer.SetRenderTarget(TempColorBufferId);
            commandBuffer.ClearRenderTarget(true, true, Color.black);
            context.ExecuteCommandBuffer(commandBuffer);

            var drawingSettings = CreateDrawingSettings(_depthTag, ref renderingData,
                SortingCriteria.CommonOpaque);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
           
            commandBuffer.Clear();

            commandBuffer.Blit(TempColorBufferId, _depthOutputTexture, Material);
            commandBuffer.ReleaseTemporaryRT(TempColorBufferId);
            context.ExecuteCommandBuffer(commandBuffer);
            
            // Draw Color
            commandBuffer.Clear();
            commandBuffer.GetTemporaryRT(TempColorBufferId, width, height, 24, FilterMode.Bilinear, RenderTextureFormat.Default);
            commandBuffer.SetRenderTarget(TempColorBufferId);
            commandBuffer.ClearRenderTarget(true, true, Color.black);
            context.ExecuteCommandBuffer(commandBuffer);

            var colorDrawingSettings =
                CreateDrawingSettings(_colorTags, ref renderingData, SortingCriteria.CommonOpaque);
            var colorFilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(renderingData.cullResults, ref colorDrawingSettings, ref colorFilteringSettings);
            
            commandBuffer.Clear();
            commandBuffer.Blit(TempColorBufferId, _colorOutputTexture);
            commandBuffer.ReleaseTemporaryRT(TempColorBufferId);
            context.ExecuteCommandBuffer(commandBuffer);

            CommandBufferPool.Release(commandBuffer);
        }
    }
}