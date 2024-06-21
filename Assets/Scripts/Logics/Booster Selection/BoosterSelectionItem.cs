using DeathMatch;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterSelectionItem : MonoBehaviour
{
    [SerializeField] private string boosterId;
    [SerializeField] private Image background;
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite selectedBackground;
    [SerializeField] private Transform statusHolder;
    [SerializeField] private Button noBoosterStatus;
    [SerializeField] private TextMeshProUGUI boostersLeftText;
    [SerializeField] private Transform lockIcon;
    [SerializeField] private TextMeshProUGUI requiredLevelText;
    [SerializeField] private Button selectionButton;
    [SerializeField] private TextMeshProUGUI selectionButtonText;

    public string BoosterId { get => boosterId; }

    private BoosterSelection boosterSelection;
    private MainUI mainUI;

    [Serializable]
    public class BoosterItemData
    {
        public string id;
        public int boostersLeft;
        public int reqLevel;

        internal int numericId;

        public BoosterItemData(string id,int boostersLeft, int reqLevel, int numericId)
        {
            this.id = id;
            this.boostersLeft = boostersLeft;
            this.reqLevel = reqLevel;
            this.numericId = numericId;
        }
    }

    private void Start()
    {
        SetUIAsNotSelected();
    }

    public void LoadBooster(BoosterItemData data)
    {
        boosterSelection = FindObjectOfType<BoosterSelection>();
        mainUI = FindObjectOfType<MainUI>();

        var isLocked = GameManager.Instance.LatestPlayerInfo.data.userScore.level.level < data.reqLevel;

        selectionButton.onClick.RemoveAllListeners();
        noBoosterStatus.onClick.RemoveAllListeners();

        if (isLocked)
        {
            statusHolder.gameObject.SetActive(false);
            lockIcon.gameObject.SetActive(true);
            requiredLevelText.text = $"سطح {data.reqLevel}";
            selectionButton.interactable = false;
        }
        else
        {
            selectionButton.interactable = true;
            statusHolder.gameObject.SetActive(true);
            lockIcon.gameObject.SetActive(false);

            if (data.boostersLeft > 0)
            {
                noBoosterStatus.gameObject.SetActive(false);
                boostersLeftText.gameObject.SetActive(true);
                boostersLeftText.text = data.boostersLeft.ToString();

                selectionButton.onClick.AddListener(() => boosterSelection.SelectBooster(boosterId));
            }
            else
            {
                noBoosterStatus.gameObject.SetActive(true);
                boostersLeftText.gameObject.SetActive(false);

                noBoosterStatus.onClick.AddListener(mainUI.OpenStore);
                noBoosterStatus.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
            }

            selectionButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        }
    }

    public void SetUIAsSelected()
    {
        background.sprite = selectedBackground;
        selectionButtonText.text = "حذف";
    }

    public void SetUIAsNotSelected()
    {
        background.sprite = normalBackground;
        selectionButtonText.text = "انتخاب";
    }
}