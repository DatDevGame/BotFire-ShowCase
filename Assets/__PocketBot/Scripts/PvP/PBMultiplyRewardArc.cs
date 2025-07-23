using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class PBMultiplyRewardArc : MultiplyRewardArc
{
    public Action<float> OnChangeMultiplier = delegate { };
    public bool IsPromoted => m_IsPromoted;
    [ShowInInspector] public float CurrentWeight => m_CurrentWeight;
    public List<SegmentInfo> NomalSegments
    {
        get
        {
            if (m_CurrentMode.value == Mode.Normal)
                return m_DataSO.config.SingleModeSegment.NomalSegments;
            else
                return m_DataSO.config.BattleModeSegment.NomalSegments;
        }
    }
    public List<SegmentInfo> PromotedSegments
    {
        get
        {
            if (m_CurrentMode.value == Mode.Normal)
                return m_DataSO.config.SingleModeSegment.PromotedSegments;
            else
                return m_DataSO.config.BattleModeSegment.PromotedSegments;
        }
    }

    public List<SegmentInfo> GetSegmentInfos => m_IsPromoted ? PromotedSegments : NomalSegments;

    [SerializeField, BoxGroup("Data")] private Variable<Mode> m_CurrentMode;
    [SerializeField, BoxGroup("Data")] private MultiplierRewardsDataSO m_DataSO;
    [SerializeField, BoxGroup("Data")] private List<TextMeshProUGUI> m_Texts;

    private float m_CurrentWeight;
    private bool m_IsPromoted = false;

    protected override IEnumerator CR_StartRun()
    {
        m_IsPromoted = m_DataSO.IsAbleToShowPromotedRewards();
        var segmentInfos = GetSegmentInfos;
        for (int i = 0; i < m_Texts.Count; i++)
        {
            m_Texts[i].text = $"x{segmentInfos[i].multiplier}";
        }

        isRunning = true;
        Vector3[] v = new Vector3[4];
        arcBound.GetWorldCorners(v);
        v[0] = arrowImg.transform.parent.InverseTransformPoint(v[0]);
        v[1] = arrowImg.transform.parent.InverseTransformPoint(v[1]);
        v[2] = arrowImg.transform.parent.InverseTransformPoint(v[2]);
        v[3] = arrowImg.transform.parent.InverseTransformPoint(v[3]);
        var startPos = new Vector3(v[0].x, v[0].y, transform.localPosition.z);
        var endPos = new Vector3(v[3].x, v[0].y, transform.localPosition.z);
        var midPos = new Vector3(transform.localPosition.x, v[1].y, transform.localPosition.z);
        var t = 0f;
        var runningValue = 0f;
        while (isRunning)
        {
            t += Time.deltaTime * runSpeed;
            runningValue = runCurve.Evaluate(Mathf.PingPong(t, 1f));
            var currentPos = GetBezierPos(startPos, midPos, endPos, runningValue);
            var adjacencyPos = GetBezierPos(startPos, midPos, endPos, runningValue + (runningValue > 0.5f ? -0.01f : 0.01f));
            var tangent = runningValue > 0.5f ? adjacencyPos - currentPos : currentPos - adjacencyPos;
            var normal = Vector3.Cross(tangent, Vector3.forward);
            arrowImg.transform.localPosition = currentPos;
            arrowImg.transform.rotation = Quaternion.LookRotation(Vector3.forward, normal);

            var totalWeight = m_IsPromoted ? PromotedSegments.Sum(segment => segment.weight) : NomalSegments.Sum(segment => segment.weight);
            var endNormalizedWeight = 0f;
            foreach (var segment in m_IsPromoted ? PromotedSegments : NomalSegments)
            {
                endNormalizedWeight += segment.weight / totalWeight;
                if (runningValue <= endNormalizedWeight)
                {
                    multiplierResult = segment.multiplier;
                    OnChangeMultiplier?.Invoke(multiplierResult);
                    break;
                }
            }
            m_CurrentWeight = endNormalizedWeight;
            yield return null;
        }
    }
}
