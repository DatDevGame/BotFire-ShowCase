using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PaintableRenderTexture : RenderTexture
{
    #region Constructors
    public PaintableRenderTexture(int width, int height, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default) : base (width, height, depth, format, readWrite)
    {
        
    }
    #endregion

    public enum UpdateMode
    {
        //
        // Summary:
        //     Update will only occur when triggered by the script.
        OnDemand,
        //
        // Summary:
        //     Update will occur at every frame.
        Realtime,
    }

    [SerializeField]
    protected bool m_AutoClearBeforeUpdate = true;
    [SerializeField]
    protected UpdateMode m_UpdateMode;
    [SerializeField]
    protected Color m_ClearColor = Color.clear;
    [SerializeField]
    protected Material m_Material;

    public bool autoClearBeforeUpdate
    {
        get => m_AutoClearBeforeUpdate;
        set => m_AutoClearBeforeUpdate = value;
    }
    public UpdateMode updateMode
    {
        get => m_UpdateMode;
        set => m_UpdateMode = value;
    }
    public Color clearColor
    {
        get => m_ClearColor;
        set => m_ClearColor = value;
    }
    public Material material
    {
        get => m_Material;
        set => m_Material = value;
    }

    public void Update()
    {
        if (material == null)
            return;
        if (autoClearBeforeUpdate)
            Clear();
        Graphics.Blit(material.mainTexture, this, material);
    }

    public void Initialize()
    {
        Clear();
    }

    public void Clear()
    {
        var commandBuffer = CommandBufferPool.Get();
        commandBuffer.SetRenderTarget(this);
        commandBuffer.ClearRenderTarget(true, true, clearColor);
        Graphics.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
        CommandBufferPool.Release(commandBuffer);
    }
}