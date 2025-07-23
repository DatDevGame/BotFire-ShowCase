using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class RecycleCellUI : MonoBehaviour
{
    public Action<RectTransform, int> OnUpdateCell;

    public int poolAmount = 10;
    public ScrollRect scrollRect;
    public GameObject sectionPrefab;

    RectTransform content => scrollRect.content;
    RectTransform viewport => scrollRect.viewport;
    List<GameObject> sections = new List<GameObject>();
    List<RectTransform> cells = new List<RectTransform>();
    List<float> cellPosList = new List<float>();
    float cellHeight;
    bool reverseArrangement = false;

    public void Init(GameObject recycledCell, int amount, List<InsertedCell> insertedCells)
    {
        //Remove old gameObjects
        foreach (var cell in cells)
        {
            Destroy(cell.gameObject);
        }
        cells.Clear();
        foreach (var section in sections)
        {
            Destroy(section);
        }
        sections.Clear();
        cellPosList.Clear();

        insertedCells.Sort((a, b) => a.index.CompareTo(b.index));

        var verticalGroup = content.GetComponent<VerticalLayoutGroup>();
        if (verticalGroup != null)
        {
            reverseArrangement = verticalGroup.reverseArrangement;
        }
        content.pivot = new Vector2(content.pivot.x, reverseArrangement ? 0 : 1);

        var rectRecycledCell = recycledCell.GetComponent<RectTransform>();
        cellHeight = rectRecycledCell.sizeDelta.y;

        float nextCellPos;
        if (reverseArrangement)
        {
            nextCellPos = cellHeight / 2 + (verticalGroup == null ? 0 : verticalGroup.padding.bottom);
        }
        else
        {
            nextCellPos = -cellHeight / 2 - (verticalGroup == null ? 0 : verticalGroup.padding.top);
        }

        var previousCellIndex = 0;
        for (var i = 0; i < insertedCells.Count + 1; i++)
        {
            var section = Instantiate(sectionPrefab, content);
            sections.Add(section);
            var rectSection = section.GetComponent<RectTransform>();
            if (i < insertedCells.Count)
            {
                var insertedCell = insertedCells[i];
                insertedCell.cell.SetAsLastSibling();
                var cellAmountFromPrevious = insertedCell.index - previousCellIndex;
                rectSection.sizeDelta = new Vector2(rectSection.sizeDelta.x, cellHeight * cellAmountFromPrevious + verticalGroup.spacing * (cellAmountFromPrevious - 1));
                for (var j = previousCellIndex; j < insertedCell.index; j++)
                {
                    cellPosList.Add(nextCellPos);
                    nextCellPos += (reverseArrangement ? 1 : -1) * (cellHeight + verticalGroup.spacing);
                }
                nextCellPos += (reverseArrangement ? 1 : -1) * (insertedCell.cell.sizeDelta.y + verticalGroup.spacing);
                previousCellIndex = insertedCell.index;
            }
            else
            {
                var cellAmountFromPrevious = amount - previousCellIndex;
                rectSection.sizeDelta = new Vector2(rectSection.sizeDelta.x, cellHeight * cellAmountFromPrevious + verticalGroup.spacing * (cellAmountFromPrevious - 1));
                for (var j = previousCellIndex; j < amount; j++)
                {
                    cellPosList.Add(nextCellPos);
                    nextCellPos += (reverseArrangement ? 1 : -1) * (cellHeight + verticalGroup.spacing);
                }
            }
        }

        for (var i = 0; i < poolAmount; i++)
        {
            var cell = Instantiate(recycledCell, content);
            cells.Add(cell.GetComponent<RectTransform>());
        }

        scrollRect.onValueChanged.AddListener(OnScroll);
        StartCoroutine(CommonCoroutine.Delay(0, false, () =>
        {
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                cell.gameObject.GetOrAddComponent<LayoutElement>().ignoreLayout = true;
                cell.anchorMax = Vector2.zero;
                cell.anchorMin = Vector2.zero;
            }
            StartCoroutine(CommonCoroutine.Delay(0, false, () =>
            {
                UpdateVisibleCells();
            }));
        }));
    }

    private float previousScrollPosition = 0f;

    void OnScroll(Vector2 vector2)
    {
        float currentScrollPosition = vector2.y;
        float deltaScroll = currentScrollPosition - previousScrollPosition;

        if (Mathf.Abs(deltaScroll) > 0.001f)
        {
            UpdateVisibleCells();
            previousScrollPosition = currentScrollPosition;
        }

    }

    void UpdateVisibleCells()
    {
        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Vector3 corner in viewportCorners)
        {
            Vector3 localCorner = content.InverseTransformPoint(corner);
            minY = Mathf.Min(minY, localCorner.y);
            maxY = Mathf.Max(maxY, localCorner.y);
        }

        for (int i = 0; i < cellPosList.Count; i++)
        {
            float cellTop = cellPosList[i] + cellHeight / 2;
            float cellBottom = cellPosList[i] - cellHeight / 2;

            if (reverseArrangement)
            {
                if (cellBottom >= minY && cellTop <= maxY)
                {
                    UpdateCell(i);
                }
            }
            else
            {
                if (cellTop >= minY && cellBottom <= maxY)
                {
                    UpdateCell(i);
                }
            }
        }
    }

    void UpdateCell(int i)
    {
        int cellIndexInPool = i % poolAmount;
        var cell = cells[cellIndexInPool];
        var nextCellPos = new Vector3(0, cellPosList[i]);
        if (cell.localPosition != nextCellPos)
        {
            cell.gameObject.SetActive(true);
            cell.localPosition = nextCellPos;
            OnUpdateCell?.Invoke(cell, i);
        }
    }

    public Vector2 GetCellPos(int index)
    {
        return new Vector2(0, cellPosList[Mathf.Min(index, cellPosList.Count - 1)]);
    }

    public class InsertedCell
    {
        public RectTransform cell;
        public int index;
    }
}
