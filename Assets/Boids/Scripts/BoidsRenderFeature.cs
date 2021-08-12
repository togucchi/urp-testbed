using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace Toguchi.Rendering
{
    public class BoidsRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }
        
        public Settings settings = new Settings();

        private BoidsPass _pass;

        public override void Create()
        {
            this.name = "Boids";
            _pass = new BoidsPass(settings.renderPassEvent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_pass);
        }
    }
}