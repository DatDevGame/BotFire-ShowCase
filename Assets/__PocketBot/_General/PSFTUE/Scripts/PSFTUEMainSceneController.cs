using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.SerializedDataStructure;

public class PSFTUEMainSceneController : MonoBehaviour
{
    private const int BLOCK_SORTING_ORDER = 99;
    private const int HIGH_LINE_SORTING_ORDER = 9000;

    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<PSFTUEBubbleText, CanvasGroupVisibility> m_BubbleTextCanvasGroupDic;
    [SerializeField, BoxGroup("Resource")] private GameObject m_DarkenBackgroundPrefab;

    [ShowInInspector] private Dictionary<GameObject, FTUEBockInfo> m_FTUEBockInfos;
    private Transform m_HolderBackGround;
    private void Awake()
    {
        m_FTUEBockInfos = new Dictionary<GameObject, FTUEBockInfo>();
        if (m_HolderBackGround == null)
            m_HolderBackGround = FindObjectOfType<MainScreenUI>().transform;

        GameEventHandler.AddActionEvent(PSFTUEBlockAction.Block, Block);
        GameEventHandler.AddActionEvent(PSFTUEBlockAction.Unblock, Unblock);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PSFTUEBlockAction.Block, Block);
        GameEventHandler.RemoveActionEvent(PSFTUEBlockAction.Unblock, Unblock);
    }

    private void Block(params object[] eventData)
    {
        if (eventData == null || eventData.Length <= 0)
            return;

        if (eventData[0] is not GameObject bockInfo)
            return;

        if (eventData[1] is not PSFTUEBubbleText PSFTUEBubbleText)
            return;

        if (PSFTUEBubbleText != PSFTUEBubbleText.None && m_BubbleTextCanvasGroupDic.ContainsKey(PSFTUEBubbleText))
            m_BubbleTextCanvasGroupDic[PSFTUEBubbleText].Show();

        if (!m_FTUEBockInfos.ContainsKey(bockInfo))
        {
            FTUEBockInfo info = new FTUEBockInfo(bockInfo);
            info.PSFTUEBubbleText = PSFTUEBubbleText;

            m_FTUEBockInfos.Add(bockInfo, info);
            HandleDarkenUI(info);
        }
        else
        { 
            HandleDarkenUI(m_FTUEBockInfos[bockInfo]); 
        }
    }

    private void Unblock(params object[] eventData)
    {
        if (eventData == null || eventData.Length <= 0)
            return;

        if (eventData[0] is not GameObject bockInfo)
            return;

        Canvas canvas = null;
        GraphicRaycaster graphicRaycaster = null;
        GameObject DarkBackGround = null;

        if (m_FTUEBockInfos.ContainsKey(bockInfo))
        {
            canvas = m_FTUEBockInfos[bockInfo].Canvas;
            graphicRaycaster = m_FTUEBockInfos[bockInfo].GraphicRaycaster;
            DarkBackGround = m_FTUEBockInfos[bockInfo].DarkBackGround;

            if (m_FTUEBockInfos.ContainsKey(bockInfo))
            {
                if(m_BubbleTextCanvasGroupDic.ContainsKey(m_FTUEBockInfos[bockInfo].PSFTUEBubbleText))
                    m_BubbleTextCanvasGroupDic[m_FTUEBockInfos[bockInfo].PSFTUEBubbleText].Hide();
            }
        }

        if (graphicRaycaster != null)
            Destroy(graphicRaycaster);

        if (canvas != null)
            Destroy(canvas);

        if (DarkBackGround != null)
            Destroy(DarkBackGround);

        m_FTUEBockInfos.Remove(bockInfo);
    }

    private void HandleDarkenUI(FTUEBockInfo bockInfo)
    {
        Canvas canvas = null;
        GraphicRaycaster graphicRaycaster = null;

        //Add Canvas
        if (bockInfo.Object.GetComponent<Canvas>() == null)
        {
            canvas = bockInfo.Object.AddComponent<Canvas>();
            bockInfo.Canvas = canvas;
        }
        else
            canvas = bockInfo.Canvas;

        if (bockInfo.Object.GetComponent<GraphicRaycaster>() == null)
        {
            graphicRaycaster = bockInfo.Object.AddComponent<GraphicRaycaster>();
            bockInfo.GraphicRaycaster = graphicRaycaster;
        }

        if (bockInfo.DarkBackGround == null)
        {
            bockInfo.DarkBackGround = Instantiate(m_DarkenBackgroundPrefab, m_HolderBackGround);
            bockInfo.DarkBackGround.GetComponent<Canvas>().sortingOrder = BLOCK_SORTING_ORDER;
        }

        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            canvas.sortingOrder = HIGH_LINE_SORTING_ORDER;
        }
    }
}

[Serializable]
public class FTUEBockInfo
{
    public GameObject Object;
    public Canvas Canvas;
    public GraphicRaycaster GraphicRaycaster;
    public GameObject DarkBackGround;
    public PSFTUEBubbleText PSFTUEBubbleText;

    public FTUEBockInfo(GameObject Object)
    {
        this.Object = Object;
    }
}

public enum PSFTUEBlockAction
{
    Block,
    Unblock
}


public enum PSFTUEBubbleText
{
    None,
    StartFirstMatch,
    OpenBuild,
    Start2ndMatch,
    NewWeapon,
}
