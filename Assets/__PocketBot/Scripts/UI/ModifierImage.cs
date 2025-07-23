using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

public class ModifierImage : Image
{
    [SerializeField]
    protected Vector3[] m_VertPositions = new Vector3[] 
    {
        new Vector3(-0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f),
        new Vector3(0.5f, 0.5f),
        new Vector3(0.5f, -0.5f)
    };

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);

        Rect r = GetPixelAdjustedRect();

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            UIVertex vert = UIVertex.simpleVert;
            vh.PopulateUIVertex(ref vert, i);
            vert.position = Vector3.Scale(m_VertPositions[i], new Vector3(r.width, r.height));
            vh.SetUIVertex(vert, i);
        }
    }
}
