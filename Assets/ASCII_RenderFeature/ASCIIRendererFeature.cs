using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.ComponentModel;


    public class ASCIIRendererFeature : ScriptableRendererFeature
    {
        class CustomRenderPass : ScriptableRenderPass
        {

            private RenderTargetIdentifier source;
            private Material asciiMaterial;
            private RenderTargetHandle asciiRenderTarget;

            ASCIIShaderData shaderData;

            public CustomRenderPass()
            {
                InitializeRenderTextures();
            }

            public void SetShaderData(ASCIIShaderData shaderData)
            {
                this.shaderData = shaderData;
            }

            public void SetAsciiMaterialParameters()
            {
                if (asciiMaterial == null)
                    return;

                asciiMaterial.SetTexture("_CharTex", shaderData.CharTex);

                asciiMaterial.SetFloat("_tilesX", shaderData.tilesX);
                asciiMaterial.SetFloat("_tilesY", shaderData.tilesY);

                asciiMaterial.SetFloat("_resolutionX", shaderData.resolutionX);
                asciiMaterial.SetFloat("_resolutionY", shaderData.resolutionY);

                asciiMaterial.SetFloat("_tileSize", shaderData.resolutionX / shaderData.tilesX);

                asciiMaterial.SetFloat("_charCount", shaderData.charCount);
                asciiMaterial.SetInt("_monochromatic", shaderData.monochromatic);
                asciiMaterial.SetFloat("_brightness", shaderData.brightness);
            }

            public void SetSource(RenderTargetIdentifier source)
            {
                this.source = source;
            }


            public void InitializeRenderTextures()
            {
                asciiRenderTarget.Init("_ASCIITarget");
            }


            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                RenderTextureDescriptor rtDescriptor = cameraTextureDescriptor;
                rtDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
                cmd.GetTemporaryRT(asciiRenderTarget.id, rtDescriptor);

                CoreUtils.Destroy(asciiMaterial);

                asciiMaterial = CoreUtils.CreateEngineMaterial("Custom/ASCIIShader");
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("ASCIIPass");

                if (asciiMaterial == null)
                    return;

                source = renderingData.cameraData.renderer.cameraColorTarget;
                RenderTextureDescriptor rtDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                rtDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;

                SetAsciiMaterialParameters();

                RenderTargetIdentifier asciiMatInputTex = source;
                Blit(cmd, asciiMatInputTex, asciiRenderTarget.Identifier(), asciiMaterial);
                Blit(cmd, asciiRenderTarget.Identifier(), source);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(asciiRenderTarget.id);
            }
        }

        [System.Serializable]
        public class Settings
        {
            [Header("SHADER PARAMETERS")]
            public Texture2D CharTex;
            public int resolutionX = 1920;
            public int resolutionY = 1080;
            public int tilesX = 80;
            public int tilesY = 45;
            public int charCount = 8;
            public float brightness = .8f;
            public int monochromatic = 0;
        }

        CustomRenderPass m_ScriptablePass;
        public Settings settings = new Settings();
        ASCIIShaderData shaderData;

        //maybe use this
        //Vector2Int screenSize;

        public override void Create()
        {
            m_ScriptablePass = new CustomRenderPass();

            //maybe use this
            //screenSize = new Vector2Int(Screen.width, Screen.height);

            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            shaderData = new ASCIIShaderData(

                settings.resolutionX,
                settings.resolutionY,
                settings.tilesX,
                settings.tilesY,
                settings.charCount,
                settings.brightness,
                settings.monochromatic,
                settings.CharTex
                );

            m_ScriptablePass.SetShaderData(shaderData);
            m_ScriptablePass.SetSource(renderer.cameraColorTarget);
            renderer.EnqueuePass(m_ScriptablePass);
        }

        public class ASCIIShaderData
        {
            public int resolutionX = 1920;
            public int resolutionY = 1080;
            public int tilesX = 80;
            public int tilesY = 45;
            public int charCount = 8;
            public float brightness = .8f;
            public int monochromatic = 0;
            public Texture2D CharTex;

            public ASCIIShaderData(int resolutionX, int resoultionY, int tilesX, int tilesY, int charCount, float brightness, int monochromatic, Texture2D CharTex)
            {
                this.resolutionX = resolutionX;
                this.resolutionY = resoultionY;
                this.tilesX = tilesX;
                this.tilesY = tilesY;
                this.charCount = charCount;
                this.brightness = brightness;
                this.monochromatic = monochromatic;
                this.CharTex = CharTex;
            }
        }
    }
