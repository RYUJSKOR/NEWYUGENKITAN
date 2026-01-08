using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Blit : ScriptableRendererFeature
{
    public class BlitPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        public int blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }

        private RTHandle source { get; set; }
        private RTHandle destination { get; set; }

        RTHandle m_TemporaryColorTexture;
        string m_ProfilerTag;

        // 出力先の種類 と 外部RT を保持
        private global::Blit.Target _targetKind;
        private RTHandle _externalTargetRT;

        // 出力先の種類(Target) と 外部RT(必要時) をパスへ渡す
        public BlitPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag,
                global::Blit.Target targetKind, RTHandle externalTargetRT)
        {
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitShaderPassIndex = blitShaderPassIndex;
            m_ProfilerTag = tag;

            // 日本語コメント：出力先情報を保持
            _targetKind = targetKind;
            _externalTargetRT = externalTargetRT;

            // 日本語コメント：このパスはカラー入力が必要
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        public void Setup(RTHandle source, RTHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // ：ここで一度だけソース/デスティネーションを決定（古いURP向け）
            if (source == null || destination == null)
            {
                var renderer = renderingData.cameraData.renderer;                 // レンダラー取得
                source = renderer.cameraColorTargetHandle;                        // ソース：カメラカラー
                destination = (_targetKind == global::Blit.Target.Color)                  // デスティ：画面 or 外部RT
                    ? renderer.cameraColorTargetHandle
                    : _externalTargetRT;
            }

            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // Can't read and write to same color target, use a TemporaryRT
            if (destination == renderingData.cameraData.renderer.cameraColorTargetHandle)
            {
                m_TemporaryColorTexture = RTHandles.Alloc(opaqueDesc);
                DoBlit(cmd, source, m_TemporaryColorTexture, blitMaterial, blitShaderPassIndex);
                DoBlit(cmd, m_TemporaryColorTexture, source);
            }
            else
            {
                DoBlit(cmd, source, destination, blitMaterial, blitShaderPassIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (m_TemporaryColorTexture != null)
            {
                m_TemporaryColorTexture.Release();
                m_TemporaryColorTexture = null;
            }
        }

        private void DoBlit(CommandBuffer cmd, RTHandle source, RTHandle dest, Material mat = null, int pass = 0)
        {
            cmd.Blit(source, dest, mat, pass);
        }

        private void DoBlit(CommandBuffer cmd, RTHandle source, RTHandle dest)
        {
            DoBlit(cmd, source, dest, null, 0);
        }
    }

    [System.Serializable]
    public class BlitSettings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
        public Material blitMaterial = null;
        public int blitMaterialPassIndex = 0;
        public Target destination = Target.Color;
        public string textureId = "_BlitPassTexture";
    }

    public enum Target
    {
        Color,
        Texture
    }

    public BlitSettings settings = new BlitSettings();
    RTHandle m_RenderTextureHandle;

    BlitPass blitPass;

    public override void Create()
    {
        var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);

        // 日本語コメント：Texture出力なら先に外部RTを確保
        if (settings.destination == Target.Texture)
        {
            RenderTextureDescriptor descriptor =
                new RenderTextureDescriptor(1, 1, RenderTextureFormat.ARGB32, 0, 0);
            m_RenderTextureHandle = RTHandles.Alloc(
                descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: settings.textureId);
        }
        else
        {
            m_RenderTextureHandle = null; // 念のため
        }

        // 日本語コメント：確保済みの外部RTを渡してパス生成
        blitPass = new BlitPass(
            settings.Event,
            settings.blitMaterial,
            settings.blitMaterialPassIndex,
            name,
            settings.destination,
            m_RenderTextureHandle
        );
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 日本語コメント：ここではターゲットに触らず、キュー投入のみ行う
        if (settings.blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        // 日本語コメント：必要ならゲームカメラ限定のガードを入れる
        if (renderingData.cameraData.cameraType != CameraType.Game) return;

        renderer.EnqueuePass(blitPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (m_RenderTextureHandle != null)
        {
            m_RenderTextureHandle.Release();
            m_RenderTextureHandle = null;
        }
    }


}