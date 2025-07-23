using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using System;

public enum CharacterUIEvent
{
    None = 0,
    Show,
    Hide
}
public class CharacterUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private ScrollRect m_ScrollRect;
    [SerializeField, BoxGroup("Ref")] private Transform m_Content;
    [SerializeField, BoxGroup("Ref")] private Button m_BackButton;
    [SerializeField, BoxGroup("Ref")] private GameObject m_SelectedImage;
    [SerializeField, BoxGroup("Ref")] private EZAnimVector3 m_EZSelectedImageScale;
    [SerializeField, BoxGroup("Ref")] private EZAnimSequence m_EZAnimSequenceInfoBox;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroup;
    [SerializeField, BoxGroup("Character")] private RawImage m_CharacterRaw;
    [SerializeField, BoxGroup("Info")] private TMP_Text m_NameText;
    [SerializeField, BoxGroup("Info")] private TMP_Text m_DescriptionText;
    [SerializeField, BoxGroup("Info")] private TMP_Text m_OutlineDescriptionText;
    [SerializeField, BoxGroup("Info")] private TMP_Text m_OutlineNameText;
    [SerializeField, BoxGroup("Info")] private TMP_Text m_GenderText;
    [SerializeField, BoxGroup("Info")] private TMP_Text m_AgeText;
    [SerializeField, BoxGroup("Info")] private TMP_Text m_HeightText;
    [SerializeField, BoxGroup("Resource")] private CharacterCell m_CharacterCellPrefab;
    [SerializeField, BoxGroup("Data")] private CharacterManagerSO m_CharacterManagerSO;

    [ShowInInspector] private CharacterMainScene m_CharacterMainScene;
    [ShowInInspector] private List<CharacterCell> m_CharacterCells;

    private CharacterCell m_CurrentCharacterCellPreview;

    private float m_RotateSpeed = 10f;
    private bool m_CanDrag = false;
    private Vector3 m_LastMousePosition;

    private void Awake()
    {
        m_CharacterCells = new List<CharacterCell>();
        m_BackButton.onClick.AddListener(OnBackButton);

        GameEventHandler.AddActionEvent(CharacterUIEvent.Show, OnShow);
        GameEventHandler.AddActionEvent(CharacterUIEvent.Hide, OnHide);
    }

    private void OnDestroy()
    {
        m_CharacterCells.ForEach((v) => 
        {
            v.OnSelectAction -= OnSelectCell;
            v.OnPreviewAction -= OnPreviewCell;
        });
        GameEventHandler.RemoveActionEvent(CharacterUIEvent.Show, OnShow);
        GameEventHandler.RemoveActionEvent(CharacterUIEvent.Hide, OnHide);
    }

    private void Start()
    {
        GenerateCell();
    }
    private void Update()
    {
        if (m_CanDrag && m_CharacterMainScene != null)
        {
            RotateObject(Input.mousePosition);
            m_LastMousePosition = Input.mousePosition;
        }
    }

    public void OnBackButton()
    {
        OnHide();
        SetCurrrentCharacter();
    }

    private void OnShow()
    {
        if (m_CharacterMainScene == null)
        {
            m_CharacterMainScene = FindObjectOfType<CharacterMainScene>();
            GenerateCell();
        }
        if (m_CharacterMainScene != null)
            m_CharacterMainScene.SetCameraEnable(true);
        m_MainCanvasGroup.Show();
        SnapToIndex(m_CharacterCells.FindIndex(v => v.IsSelected));

        #region Firebase Event
        GameEventHandler.Invoke(LogFirebaseEventCode.DriversMenuReached);
        #endregion
    }

    private void OnHide()
    {
        if (m_CharacterMainScene == null)
            m_CharacterMainScene = FindObjectOfType<CharacterMainScene>();
        if (m_CharacterMainScene != null)
            m_CharacterMainScene.SetCameraEnable(false);
        m_MainCanvasGroup.Hide();
        m_CurrentCharacterCellPreview = null;
    }

    private void GenerateCell()
    {
        m_CharacterCells.ForEach(v => Destroy(v.gameObject));
        m_CharacterCells.Clear();

        for (int i = 0; i < m_CharacterManagerSO.initialValue.Count; i++)
        {
            CharacterCell characterCell = Instantiate(m_CharacterCellPrefab, m_Content);
            m_CharacterCells.Add(characterCell);
            characterCell.InitData(m_CharacterManagerSO.initialValue[i].Cast<CharacterSO>());
            characterCell.OnSelectAction += OnSelectCell;
            characterCell.OnPreviewAction += OnPreviewCell;
        }

        CharacterCell characterCellSelected = m_CharacterCells.Find(v => v.CharacterSO == m_CharacterManagerSO.PlayerCharacterSO.value);
        if (characterCellSelected != null)
        {
            characterCellSelected.Select();
            characterCellSelected.UnPreview();
        }
    }

    private void OnPreviewCell(CharacterCell characterCell)
    {
        if (m_CurrentCharacterCellPreview != characterCell)
        {
            Action OnCompleteEZ = () =>
            {
                if (characterCell.IsSelected)
                    m_EZSelectedImageScale.Play();
            };

            m_EZAnimSequenceInfoBox.SetToStart();
            m_EZAnimSequenceInfoBox.Play();
            if (characterCell.IsSelected)
                m_EZSelectedImageScale.Play();
            else
                m_EZSelectedImageScale.SetToStart();

            m_CurrentCharacterCellPreview = characterCell;
            SpawnCharacter(characterCell.CharacterSO);
            LoadInfo(characterCell.CharacterSO);
        }
        else
        {
            if (characterCell.IsSelected && m_EZSelectedImageScale.transform.localScale == Vector3.zero)
                m_EZSelectedImageScale.Play();
        }

        m_CharacterCells
            .Where(v => v != characterCell)
            .ToList()
            .ForEach(x => x
            .UnPreview());
    }

    private void OnSelectCell(CharacterCell characterCell)
    {
        SpawnCharacter(characterCell.CharacterSO);
        //Set Character Current
        if (characterCell.CharacterSO.IsUnlocked())
        {
            m_EZSelectedImageScale.SetToStart();
            m_CharacterManagerSO.PlayerCharacterSO.value = characterCell.CharacterSO;
            if (characterCell.IsSelected)
                m_EZSelectedImageScale.Play();
        }

        LoadInfo(characterCell.CharacterSO);

        //UnSelect Cell other
        m_CharacterCells
            .Where(v => v != characterCell)
            .ToList()
            .ForEach(x => x
            .UnSelect());
    }
    private void LoadInfo(CharacterSO characterSO)
    {
        if (characterSO.TryGetModule<CharacterModule>(out var module))
        {
            m_NameText.SetText(characterSO.GetDisplayName());
            m_OutlineNameText.SetText(characterSO.GetDisplayName());
            m_GenderText.SetText(module.Gender);
            m_AgeText.SetText(module.Age.ToString());
            m_HeightText.SetText($"{module.Height}m");
        }

        if (characterSO.TryGetModule<DescriptionItemModule>(out var descriptionModule))
        {
            m_DescriptionText.SetText(descriptionModule.Description);
            m_OutlineDescriptionText.SetText(descriptionModule.Description);
        }
    }
    private void SpawnCharacter(CharacterSO characterSO)
    {
        if (m_CharacterMainScene == null)
            m_CharacterMainScene = FindObjectOfType<CharacterMainScene>();

        if (m_CharacterMainScene != null)
        {
            m_CharacterMainScene.SpawnCharacter(characterSO);
            m_CharacterRaw.texture = m_CharacterMainScene.RenderTextureCharacter;
        }
    }
    private void SetCurrrentCharacter()
    {
        m_CharacterCells.ForEach((characterCell) =>
        {
            if (characterCell.CharacterSO == m_CharacterManagerSO.PlayerCharacterSO.value)
                characterCell.Select();
            else
                characterCell.UnSelect();

            characterCell.UnPreview();
        });
    }


    private void SnapToIndex(int index)
    {
        if (m_CharacterCells.Count <= 0) return;

        // Clamp index to ensure it's within range
        index = Mathf.Clamp(index, 0, m_CharacterCells.Count - 1);

        float spacingCell = 50;
        // Calculate the target position
        float targetX = index * (m_CharacterCells[0].GetComponent<RectTransform>().sizeDelta.x + spacingCell);

        // Adjust target to center the cell
        float viewportWidth = m_ScrollRect.viewport.rect.width;
        targetX = Mathf.Clamp(targetX, 0, m_Content.GetComponent<RectTransform>().rect.width - viewportWidth);

        // Use DOTween to smoothly scroll to the target position
        m_Content.GetComponent<RectTransform>().DOAnchorPosX(-targetX, 0.5f).SetEase(Ease.InOutQuad);
    }

    private void RotateObject(Vector3 currentPosition)
    {
        Vector3 deltaPosition = currentPosition - m_LastMousePosition;
        float rotationY = deltaPosition.x * m_RotateSpeed * Time.deltaTime;
        m_CharacterMainScene.CurrentCharacter.transform.Rotate(Vector3.up, -rotationY);
    }

    public void EnableCanDrag()
    {
        m_LastMousePosition = Input.mousePosition;
        m_CanDrag = true;
    }
    public void DisableCanDrag() => m_CanDrag = false;
}
