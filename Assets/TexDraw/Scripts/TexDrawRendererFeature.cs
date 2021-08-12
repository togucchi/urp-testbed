using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Toguchi.Rendering
{
    public class TexDrawRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Shader shader;
        }
        
        public Settings settings = new Settings();

        private TexDrawPass _pass;


        public override void Create()
        {
            this.name = "TexDraw";
            _pass = new TexDrawPass(settings.renderPassEvent, settings.shader);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            
            _pass?.Dispose();
        }
    }
}