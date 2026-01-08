using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenPassRendererFeature : ScriptableRendererFeature
{
    public enum InjectionPoint
    {
        BeforeRenderingTransparents = RenderPassEvent.BeforeRenderingTransparents,
        BeforeRenderingPostProcessing = RenderPassEvent.BeforeRenderingPostProcessing,
        AfterRenderingPostProcessing = RenderPassEvent.AfterRenderingPostProcessing
    }

    public Material passMaterial;
    public InjectionPoint injectionPoint = InjectionPoint.AfterRenderingPostProcessing;
    public ScriptableRenderPassInput requirements = ScriptableRenderPassInput.Color;
    [HideInInspector] public int passIndex = 0;

    private FullScreenRenderPass fullScreenPass;
    private bool requiresColor;
    private bool injectedBeforeTransparents;

    public override void Create()
    {
        fullScreenPass = new FullScreenRenderPass();
        fullScreenPass.renderPassEvent = (RenderPassEvent)injectionPoint;

        ScriptableRenderPassInput modifiedRequirements = requirements;

        requiresColor = (requirements & ScriptableRenderPassInput.Color) != 0;
        injectedBeforeTransparents = injectionPoint <= InjectionPoint.BeforeRenderingTransparents;

        if (requiresColor && !injectedBeforeTransparents)
        {
            modifiedRequirements ^= ScriptableRenderPassInput.Color;
        }

        fullScreenPass.ConfigureInput(modifiedRequirements);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (passMaterial == null)
        {
            Debug.LogWarningFormat("Missing Post Processing effect Material. {0} Fullscreen pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        fullScreenPass.Setup(passMaterial, passIndex, requiresColor, injectedBeforeTransparents, GetType().Name, renderingData);
        renderer.EnqueuePass(fullScreenPass);
    }

    protected override void Dispose(bool disposing)
    {
        fullScreenPass.Dispose();
    }

    class FullScreenRenderPass : ScriptableRenderPass
    {
        private Material m_PassMaterial;
        private int m_PassIndex;
        private bool m_RequiresColor;
        private bool m_IsBeforeTransparents;
        private PassData m_PassData;
        private ProfilingSampler m_ProfilingSampler;
        private RTHandle m_CopiedColor;

        public void Setup(Material mat, int index, bool requiresColor, bool isBeforeTransparents, string featureName, in RenderingData renderingData)
        {
            m_PassMaterial = mat;
            m_PassIndex = index;
            m_RequiresColor = requiresColor;
            m_IsBeforeTransparents = isBeforeTransparents;
            m_ProfilingSampler ??= new ProfilingSampler(featureName);

            var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;
            RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");

            m_PassData ??= new PassData();
        }

        public void Dispose()
        {
            m_CopiedColor?.Release();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_PassData.effectMaterial = m_PassMaterial;
            m_PassData.passIndex = m_PassIndex;
            m_PassData.requiresColor = m_RequiresColor;
            m_PassData.isBeforeTransparents = m_IsBeforeTransparents;
            m_PassData.profilingSampler = m_ProfilingSampler;
            m_PassData.copiedColor = m_CopiedColor;

            ExecutePass(m_PassData, ref renderingData, ref context);
        }

        private static void ExecutePass(PassData passData, ref RenderingData renderingData, ref ScriptableRenderContext context)
        {
            var passMaterial = passData.effectMaterial;
            var passIndex = passData.passIndex;
            var requiresColor = passData.requiresColor;
            var isBeforeTransparents = passData.isBeforeTransparents;
            var copiedColor = passData.copiedColor;
            var profilingSampler = passData.profilingSampler;
            var renderer = renderingData.cameraData.renderer; // Renderer を取得

            if (passMaterial == null)
            {
                return;
            }

            if (renderingData.cameraData.isPreviewCamera)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("FullScreenPass"); // CommandBuffer を取得

            using (new ProfilingScope(cmd, profilingSampler))
            {
                RTHandle source = (isBeforeTransparents) ? renderer.cameraColorTargetHandle : renderer.cameraColorTargetHandle; // BackBuffer の取得方法が変わっている可能性があるので、一旦 cameraColorTargetHandle を使用

                if (requiresColor)
                {
                    Blitter.BlitCameraTexture(cmd, source, copiedColor);
                    passMaterial.SetTexture(Shader.PropertyToID("_BlitTexture"), copiedColor);
                }

                CoreUtils.SetRenderTarget(cmd, renderer.cameraColorTargetHandle);
                CoreUtils.DrawFullScreen(cmd, passMaterial, null, passIndex);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }

            CommandBufferPool.Release(cmd); // CommandBuffer の解放
        }
        private class PassData
        {
            internal Material effectMaterial;
            internal int passIndex;
            internal bool requiresColor;
            internal bool isBeforeTransparents;
            public ProfilingSampler profilingSampler;
            public RTHandle copiedColor;
        }
    }
}