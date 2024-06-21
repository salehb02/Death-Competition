using DeathMatch;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayersProperties : MonoBehaviour
{
    [SerializeField] private PlayerData[] playerData;
    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;
    [SerializeField] private RectTransform holder;
    [SerializeField] private float xPosOnClose;
    [SerializeField] private float transitionDuration = 0.5f;

    private EventManager eventManager;

    [Serializable]
    public class PlayerData
    {
        public TextMeshProUGUI UsernameText;
        public Image[] Shields;
        public Image PirateShip;
    }

    private void Start()
    {
        eventManager = FindObjectOfType<EventManager>();

        HidePlayersData(true);
    }

    public void ShowPlayersData()
    {
        if (SaveManager.Get<bool>(SaveManager.UNIQUE_EVENTS_ACTIVE))
            return;

        holder.DOAnchorPosX(0, transitionDuration);
    }

    public void HidePlayersData(bool force)
    {
        holder.DOAnchorPosX(xPosOnClose, force ? 0 : transitionDuration);
    }

    public void UpdateInfo()
    {
        for (int i = 0; i < playerData.Length; i++)
        {
            var username = eventManager.Characters[i].Player.Username;

            if (username.Length > 10)
                playerData[i].UsernameText.text = username.Substring(0, 10) + "...";
            else
                playerData[i].UsernameText.text = username;

            playerData[i].PirateShip.color = eventManager.Characters[i].HasAttackShip ? onColor : offColor;

            for (int j = 0; j < playerData[i].Shields.Length; j++)
            {
                playerData[i].Shields[j].color = j < eventManager.Characters[i].ShieldLeft ? onColor : offColor;
            }
        }
    }
}