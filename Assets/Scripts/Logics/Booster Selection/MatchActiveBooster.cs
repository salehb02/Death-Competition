using DeathMatch;
using System.Collections.Generic;
using UnityEngine;

public class MatchActiveBooster : MonoBehaviour
{
    private Boosters booster;
    private EventManager eventManager;

    private void Start()
    {
        booster = FindObjectOfType<Boosters>();
        eventManager = FindObjectOfType<EventManager>();

        eventManager.OnRoundStart += OnNewRound;
    }

    private void OnDisable()
    {
        eventManager.OnRoundStart -= OnNewRound;
    }

    private void OnNewRound()
    {
        if (HasFirstHelp())
            booster.Help(false, false);

        if (HasSecondHelp())
            booster.Help(false, false);
    }

    public bool HasFirstHelp()
    {
        if (!SaveManager.HasKey(SaveManager.SELECTED_BOOSTER))
            return false;

        return SaveManager.Get<List<string>>(SaveManager.SELECTED_BOOSTER).Contains(BoosterSelection.HELP_1);
    }

    public bool HasSecondHelp()
    {
        if (!SaveManager.HasKey(SaveManager.SELECTED_BOOSTER))
            return false;

        return SaveManager.Get<List<string>>(SaveManager.SELECTED_BOOSTER).Contains(BoosterSelection.HELP_2);
    }

    public bool HasExtraBlock()
    {
        if (!SaveManager.HasKey(SaveManager.SELECTED_BOOSTER))
            return false;
        
        return SaveManager.Get<List<string>>(SaveManager.SELECTED_BOOSTER).Contains(BoosterSelection.EXTRA_BLOCK);
    }

    public bool HasShield()
    {
        if (!SaveManager.HasKey(SaveManager.SELECTED_BOOSTER))
            return false;
        
        return SaveManager.Get<List<string>>(SaveManager.SELECTED_BOOSTER).Contains(BoosterSelection.SHIELD);
    }

    public bool HasPirateShip()
    {
        if (!SaveManager.HasKey(SaveManager.SELECTED_BOOSTER))
            return false;

        return SaveManager.Get<List<string>>(SaveManager.SELECTED_BOOSTER).Contains(BoosterSelection.PIRATE_SHIP);
    }

    public int GetPirateShipMinimumLevel()
    {
        return SaveManager.Get<int>(SaveManager.PIRATE_SHIP_BOOSTER_MIN_LEVEL);
    }

    public int GetShieldMinimumLevel()
    {
        return SaveManager.Get<int>(SaveManager.SHIELD_BOOSTER_MIN_LEVEL);
    }
}