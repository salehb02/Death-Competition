using DeathMatch;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoosterSelection : MonoBehaviour
{
    private BoosterSelectionPresentor presentor;
    private GameplayEntrance gameplayEntrance;
    private BoosterPurchase boosterPurchase;
    private MainUI mainUI;

    private List<string> selectedBoosters = new List<string>();
    private BoosterSelectionItem.BoosterItemData[] currentBoostersData;

    public const string HELP_1 = "HELP_1";
    public const string HELP_2 = "HELP_2";
    public const string EXTRA_BLOCK = "EXTRA_BLOCK";
    public const string SHIELD = "SHIELD";
    public const string PIRATE_SHIP = "PIRATE_SHIP";

    private void Start()
    {
        presentor = GetComponent<BoosterSelectionPresentor>();
        gameplayEntrance = FindObjectOfType<GameplayEntrance>();
        boosterPurchase = FindObjectOfType<BoosterPurchase>();
        mainUI = FindObjectOfType<MainUI>();

        RemovePreviousSelectedBoosters();
        LoadAllBoosters();
    }

    public void SelectBooster(string id)
    {
        if (selectedBoosters.Contains(id))
        {
            selectedBoosters.Remove(id);

            if (id == HELP_1)
                selectedBoosters.Remove(HELP_2);

            presentor.SelectBoostersUI(selectedBoosters.ToArray());
            return;
        }

        selectedBoosters.Add(id);

        if (id == HELP_2)
        {
            if (!selectedBoosters.Contains(HELP_1))
            {
                if (currentBoostersData.SingleOrDefault(x => x.id == HELP_1).boostersLeft == 0)
                    mainUI.OpenStore();
                else
                    selectedBoosters.Add(HELP_1);
            }
        }

        presentor.SelectBoostersUI(selectedBoosters.ToArray());
    }

    [System.Obsolete]
    public void LoadAllBoosters(MSDM.Boosters existBoosters = null)
    {
        ServerConnection.Instance.GetBoosters((data) =>
        {
            var boostersPacket = existBoosters ?? data;

            var boosters = new BoosterSelectionItem.BoosterItemData[]
            {
                new BoosterSelectionItem.BoosterItemData(EXTRA_BLOCK, boostersPacket.boosters[0].buyCount,boostersPacket.boosters[0].level, boostersPacket.boosters[0].id),
                new BoosterSelectionItem.BoosterItemData(HELP_1, boostersPacket.boosters[1].buyCount,boostersPacket.boosters[1].level, boostersPacket.boosters[1].id),
                new BoosterSelectionItem.BoosterItemData(HELP_2, boostersPacket.boosters[2].buyCount,boostersPacket.boosters[2].level, boostersPacket.boosters[2].id),
                new BoosterSelectionItem.BoosterItemData(SHIELD, boostersPacket.boosters[3].buyCount,boostersPacket.boosters[3].level, boostersPacket.boosters[3].id),
                new BoosterSelectionItem.BoosterItemData(PIRATE_SHIP, boostersPacket.boosters[4].buyCount,boostersPacket.boosters[4].level, boostersPacket.boosters[4].id),
            };

            currentBoostersData = boosters;

            presentor.LoadBoosters(boosters);
            boosterPurchase.LoadBoosters(boostersPacket);
            SetBoostersMinimumLevel();
        });
    }

    private void RemovePreviousSelectedBoosters()
    {
        SaveManager.Remove(SaveManager.SELECTED_BOOSTER);
    }

    public void StartGame()
    {
        foreach (var booster in selectedBoosters)
            ServerConnection.Instance.UseBooster(currentBoostersData.SingleOrDefault(x => x.id == booster).numericId);

        SaveManager.Set(SaveManager.SELECTED_BOOSTER, selectedBoosters);
        gameplayEntrance.EnterGame();
    }

    private void SetBoostersMinimumLevel()
    {
        SaveManager.Set(SaveManager.PIRATE_SHIP_BOOSTER_MIN_LEVEL, currentBoostersData[4].reqLevel);
        SaveManager.Set(SaveManager.SHIELD_BOOSTER_MIN_LEVEL, currentBoostersData[3].reqLevel);
    }
}