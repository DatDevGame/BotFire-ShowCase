using System;
using System.Collections;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.UI;

public class CheatAddMission : MonoBehaviour
{
    [SerializeField] private MissionSavedDataSO _savedDataSO;
    [SerializeField] private MissionGeneratorConfigSO _generatorConfigSO;
    [SerializeField] private Button _replaceMissionBtn, _settingBtnTemplate, _selectTargetTypeBtn, _selectScopeBtn, _selectDifficultyBtn;
    [SerializeField] private Text _selectTargetTypeText, _selectScopeText, _selectDifficultyText;
    [SerializeField] private VerticalLayoutGroup _mainGroup, _targetTypeSettingGroup, _scopeSettingGroup, _difficultySettingGroup;
    private MissionTargetType _selectedTargetType = MissionTargetType.MatchWins_Count_Any;
    private MissionScope _selectedScope = MissionScope.Daily;
    private MissionDifficulty _selectedDifficulty = MissionDifficulty.Easy;
    private bool _isInitCompleted = false;
    private float _baseHeight;
    private float _targetSettingHeight;
    private float _scopeSettingHeight;
    private float _difficultySettingHeight;

    private IEnumerator Start()
    {
        _isInitCompleted = false;
        _targetTypeSettingGroup.gameObject.SetActive(false);
        _scopeSettingGroup.gameObject.SetActive(false);
        _difficultySettingGroup.gameObject.SetActive(false);

        _selectTargetTypeBtn.gameObject.SetActive(true);
        _selectScopeBtn.gameObject.SetActive(true);
        _selectDifficultyBtn.gameObject.SetActive(true);

        _selectTargetTypeText.text = _selectedTargetType.ToString();
        _selectScopeText.text = _selectedScope.ToString();
        _selectDifficultyText.text = _selectedDifficulty.ToString();

        _selectTargetTypeBtn.onClick.AddListener(()=>
        {
            _targetTypeSettingGroup.gameObject.SetActive(true);
            _selectTargetTypeBtn.gameObject.SetActive(false);
            GetComponent<RectTransform>().sizeDelta = new Vector2(0, _baseHeight - _selectTargetTypeBtn.GetComponent<RectTransform>().sizeDelta.y + _targetSettingHeight);
        });
        _selectScopeBtn.onClick.AddListener(()=>
        {
            _scopeSettingGroup.gameObject.SetActive(true);
            _selectScopeBtn.gameObject.SetActive(false);
            GetComponent<RectTransform>().sizeDelta = new Vector2(0, _baseHeight - _selectScopeBtn.GetComponent<RectTransform>().sizeDelta.y + _scopeSettingHeight);
        });
        _selectDifficultyBtn.onClick.AddListener(()=>
        {
            _difficultySettingGroup.gameObject.SetActive(true);
            _selectDifficultyBtn.gameObject.SetActive(false);
            GetComponent<RectTransform>().sizeDelta = new Vector2(0, _baseHeight - _selectDifficultyBtn.GetComponent<RectTransform>().sizeDelta.y + _difficultySettingHeight);
        });
        _replaceMissionBtn.onClick.AddListener(() =>ReplaceMission(_selectedTargetType, _selectedScope, _selectedDifficulty));
        yield return null;
        //build target type
        foreach(MissionTargetType targetType in Enum.GetValues(typeof(MissionTargetType)))
        {
            Button newBtn = Instantiate(_settingBtnTemplate, _targetTypeSettingGroup.transform);
            newBtn.onClick.AddListener(()=> OnSelectTargetType(targetType));
            newBtn.GetComponentInChildren<Text>().text = targetType.ToString();
            newBtn.gameObject.SetActive(true);
            yield return null;
        }
        //build scope
        foreach(MissionScope scope in Enum.GetValues(typeof(MissionScope)))
        {
            Button newBtn = Instantiate(_settingBtnTemplate, _scopeSettingGroup.transform);
            newBtn.onClick.AddListener(()=> OnSelectScope(scope));
            newBtn.GetComponentInChildren<Text>().text = scope.ToString();
            newBtn.gameObject.SetActive(true);
            yield return null;
        }
        //build difficulty
        foreach(MissionDifficulty difficulty in Enum.GetValues(typeof(MissionDifficulty)))
        {
            Button newBtn = Instantiate(_settingBtnTemplate, _difficultySettingGroup.transform);
            newBtn.onClick.AddListener(()=> OnSelectDifficulty(difficulty));
            newBtn.GetComponentInChildren<Text>().text = difficulty.ToString();
            newBtn.gameObject.SetActive(true);
            yield return null;
        }
        float templateHeight = _settingBtnTemplate.GetComponent<RectTransform>().sizeDelta.y;
        float targetTypeCount = Enum.GetValues(typeof(MissionTargetType)).Length;
        float scopeCount = Enum.GetValues(typeof(MissionScope)).Length;
        float difficultyCount = Enum.GetValues(typeof(MissionDifficulty)).Length;
        float mainSpacing = _mainGroup.spacing;
        _targetSettingHeight = templateHeight * targetTypeCount + _targetTypeSettingGroup.spacing * (targetTypeCount - 1);
        _scopeSettingHeight = templateHeight * scopeCount + _scopeSettingGroup.spacing * (scopeCount - 1);
        _difficultySettingHeight = templateHeight * difficultyCount + _difficultySettingGroup.spacing * (difficultyCount - 1);
        _baseHeight = _replaceMissionBtn.GetComponent<RectTransform>().sizeDelta.y + mainSpacing + 
            _selectTargetTypeBtn.GetComponent<RectTransform>().sizeDelta.y + mainSpacing + 
            _selectScopeBtn.GetComponent<RectTransform>().sizeDelta.y + mainSpacing + 
            _selectDifficultyBtn.GetComponent<RectTransform>().sizeDelta.y;

        _targetTypeSettingGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(0, _targetSettingHeight);
        _scopeSettingGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(0, _scopeSettingHeight);
        _difficultySettingGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(0, _difficultySettingHeight);
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, _baseHeight);

        _isInitCompleted = true;
    }

    private void OnSelectTargetType(MissionTargetType targetType)
    {
        _selectedTargetType = targetType;
        _targetTypeSettingGroup.gameObject.SetActive(false);
        _selectTargetTypeBtn.gameObject.SetActive(true);
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, _baseHeight);
        _selectTargetTypeText.text = _selectedTargetType.ToString();
    }

    private void OnSelectScope(MissionScope scope)
    {
        _selectedScope = scope;
        _scopeSettingGroup.gameObject.SetActive(false);
        _selectScopeBtn.gameObject.SetActive(true);
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, _baseHeight);
        _selectScopeText.text = _selectedScope.ToString();
    }

    private void OnSelectDifficulty(MissionDifficulty difficulty)
    {
        _selectedDifficulty = difficulty;
        _difficultySettingGroup.gameObject.SetActive(false);
        _selectDifficultyBtn.gameObject.SetActive(true);
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, _baseHeight);
        _selectDifficultyText.text = _selectedDifficulty.ToString();
    }

    private void ReplaceMission(MissionTargetType targetType, MissionScope scope, MissionDifficulty difficulty)
    {
        if (!_isInitCompleted)
        {
            ToastUI.Show("Fail. Cheat UI has not completely inited yet. Wait 5 seconds more.");
            return;
        }
        if (!_savedDataSO.GetMissionTargetCalculator(targetType).IsHaveValidTarget())
        {
            ToastUI.Show("Fail. Can't cheat add mission. Have no valid target.");
            return;
        }
        MissionData newMission = _savedDataSO.CreateMission(
            scope,
            targetType,
            difficulty,
            _generatorConfigSO.GetAmountFactor(targetType, difficulty),
            _generatorConfigSO.GetAmountMultiplier(scope),
            _generatorConfigSO.GetTokenAmount(scope, difficulty)
        );
        var missions = _savedDataSO.GetMissionListByScope(scope);
        if (missions != null && missions.Count > 0)
            MissionManager.Instance.ReplaceMission(missions[0], newMission);
        else
            MissionManager.Instance.AddMission(newMission);
        GameEventHandler.Invoke(scope switch
        {
            MissionScope.Daily => SeasonPassEventCode.OnUpdateNewDailyMissions,
            MissionScope.Weekly => SeasonPassEventCode.OnUpdateNewWeeklyMissions,
            MissionScope.Season => SeasonPassEventCode.OnUpdateNewHalfSeasonMissions,
            _ => throw new NotImplementedException(),
        });
        ToastUI.Show("Success. Replace mission completed.");
    }
}