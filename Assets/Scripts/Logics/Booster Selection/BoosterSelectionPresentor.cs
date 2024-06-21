using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoosterSelectionPresentor : MonoBehaviour
{
    [SerializeField] private BoosterSelectionItem[] boosters;
    [SerializeField] private Button playButton;
    [SerializeField] private Button resetSelectionButton;

    private BoosterSelection boosterSelection;

    private void Start()
    {
        boosterSelection = GetComponent<BoosterSelection>();

        playButton.onClick.AddListener(boosterSelection.StartGame);
        resetSelectionButton.onClick.AddListener(() => boosterSelection.SelectBooster(null));
    }

    public void LoadBoosters(BoosterSelectionItem.BoosterItemData[] datas)
    {
        for (int i = 0; i < boosters.Length; i++)
            boosters[i].LoadBooster(datas[i]);
    }

    public void SelectBoostersUI(params string[] ids)
    {
        foreach (var booster in boosters)
            booster.SetUIAsNotSelected();

        for (int i = 0; i < boosters.Length; i++)
        {
            if (ids.Contains(boosters[i].BoosterId))
                boosters[i].SetUIAsSelected();
        }
    }
}