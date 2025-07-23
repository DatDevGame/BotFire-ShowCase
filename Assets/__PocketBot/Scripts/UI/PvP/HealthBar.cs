using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HyrphusQ.Events;
using LatteGames;
using System;
using Sirenix.OdinInspector;

public class HealthBar : MonoBehaviour
{
    public event Action onSetupCompleted = delegate { };

    [SerializeField] float speed;
    [SerializeField] SlicedFilledImage filler;
    [SerializeField] Vector3 offset;
    [SerializeField] Color localPlayerHealthColor;
    [SerializeField] Color localTeamPlayerHealthColor;
    [SerializeField] Color opponentPlayerHealthColor;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] Image playerFlagIcon;
    [SerializeField] HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] ContentSizeFitter contentSizeFitter;
    [SerializeField, BoxGroup("Ref")] private Transform m_CirclePlayer;
    [SerializeField, BoxGroup("Ref")] private List<SpriteRenderer> m_ColorSpriteCircle;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI m_HealthValueText;
    [SerializeField, BoxGroup("Ref")] private List<Image> m_CellHealths;
    [NonSerialized] public Transform robotTransform;

    Camera mainCam;
    Competitor competitor;

    public Competitor Competitor
    {
        get => competitor;
        set
        {
            competitor = value;
            Setup();
        }
    }

    public HealthBarSpawner Spawner { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        mainCam = MainCameraFindCache.Get();
    }

    void Setup()
    {

        var personalInfo = competitor.PersonalInfo;
        Color colorPlayer = LayerMask.LayerToName(competitor.gameObject.layer).Contains("Player") ? localTeamPlayerHealthColor : opponentPlayerHealthColor;
        if (personalInfo.isLocal)
            colorPlayer = localPlayerHealthColor;
        m_CellHealths.ForEach(v => v.color = colorPlayer);

        m_HealthValueText.SetText($"{competitor.Health.RoundToInt()}");
        filler.fillAmount = competitor.Health / competitor.MaxHealth;
        playerName.text = personalInfo.name;
        playerName.color = colorPlayer;
        playerFlagIcon.sprite = personalInfo.nationalFlag;
        StartCoroutine(CommonCoroutine.Delay(0.1f, false, () => { contentSizeFitter.enabled = false; horizontalLayoutGroup.enabled = false; }));
        competitor.OnHealthChanged += HandleHealthChanged;
        onSetupCompleted.Invoke();

        if (competitor is PBRobot robot)
        {
            m_ColorSpriteCircle.ForEach(v => v.color = colorPlayer);
            m_CirclePlayer.SetParent(robot.ChassisInstance.CarPhysics.transform);
            m_CirclePlayer.localPosition = new Vector3(0, 0.1f, 0);
            m_CirclePlayer.localScale = Vector3.one * 3.7f;
        }
    }

    void OnDestroy()
    {
        if (competitor != null) competitor.OnHealthChanged -= HandleHealthChanged;
    }

    void HandleHealthChanged(Competitor.HealthChangedEventData eventData)
    {
        float fillAmount = eventData.CurrentHealth / eventData.MaxHealth;
        m_HealthValueText.SetText($"{eventData.CurrentHealth.RoundToInt()}");
        filler.fillAmount = fillAmount;
        m_CirclePlayer.gameObject.SetActive(eventData.CurrentHealth > 0);
        if (eventData.CurrentHealth <= 0)
        {
            Destroy(gameObject, 2f);
            gameObject.SetActive(false);
            Spawner.RemoveHealthBar(this);
            m_HealthValueText.SetText("0");
        }
    }

    private void LateUpdate()
    {
        if (robotTransform == null) return;
        if (competitor == null) return;
        var targetPos = mainCam.WorldToScreenPoint(robotTransform.position + offset);
        transform.position = Vector3.Lerp(transform.position, targetPos, speed);
    }

    public void TurnOffFlag()
    {
        playerFlagIcon.gameObject.SetActive(false);
    }
}
