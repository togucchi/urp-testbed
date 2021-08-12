using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Toguchi.Rendering
{
    public class Draw2DRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            public RenderPassEvent postProcessEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Shader shader;
        }

        public Settings settings = new Settings();

        private Draw2DPass _pass;
        private Draw2DPostProcessPass _postProcessPass;

        public override void Create()
        {
            this.name = "Draw2D";
            _pass = new Draw2DPass(settings.renderPassEvent);
            _postProcessPass = new Draw2DPostProcessPass(settings.postProcessEvent, settings.shader);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _pass.Setup(renderer.cameraColorTarget); 
            _postProcessPass.Setup(renderer.cameraColorTarget);
            
            renderer.EnqueuePass(_pass);
            renderer.EnqueuePass(_postProcessPass);
        }
    }
}