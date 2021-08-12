using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SocialPlatforms;

namespace Toguchi.Rendering
{
    public class BoidsPass : ScriptableRenderPass
    {
        public static readonly Dictionary<int, BoidsRenderData> RenderDataMap = new Dictionary<int, BoidsRenderData>();
        
        public BoidsPass(RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var commandBuffer = CommandBufferPool.Get("Boids");

            foreach (var data in RenderDataMap.Values)
            {
                commandBuffer.DrawMeshInstancedIndirect(data.Mesh, 0, data.Material, 0, data.ArgsBuffer);
            }
            
            context.ExecuteCommandBuffer(commandBuffer);
            
            CommandBufferPool.Release(commandBuffer);
        }

    }
}