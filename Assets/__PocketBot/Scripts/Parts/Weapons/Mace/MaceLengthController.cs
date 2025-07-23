using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class MaceLengthController : MonoBehaviour
{
#if UNITY_EDITOR
    public List<Transform> maceNodes;
    public Transform maceHead;
    [OnValueChanged("UpdateLength")]
    public float spacing = 0.227f;
    [OnValueChanged("UpdateLength")]
    public float scaleY = 0.2f;
    [OnValueChanged("UpdateLength")]
    public float firstNodeY = 0.77f;
    void UpdateLength()
    {
        var maceHeadOffset = maceHead.position - maceNodes.Last().position;
        maceNodes[0].position = Vector3.up * firstNodeY;
        for (var i = 0; i < maceNodes.Count; i++)
        {
            var maceNode = maceNodes[i];
            maceNode.localScale = new Vector3(maceNode.localScale.x, scaleY, maceNode.localScale.z);
            if (i > 0)
            {
                maceNode.position = maceNodes[i - 1].position + Vector3.up * spacing;
            }
        }
        maceHead.position = maceNodes.Last().position + maceHeadOffset;
    }
#endif
}
