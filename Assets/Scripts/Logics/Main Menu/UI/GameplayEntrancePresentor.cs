using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DeathMatch {
    public class GameplayEntrancePresentor : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI entrancePrice;
        [SerializeField] private Button entranceButton;
        [SerializeField] private Button watchAdButton;

        private GameplayEntrance gameplayEntrance;

        private void Start()
        {
            gameplayEntrance = FindObjectOfType<GameplayEntrance>();

            entranceButton.onClick.AddListener(gameplayEntrance.CheckCoin);
            watchAdButton.onClick.AddListener(gameplayEntrance.WatchAd);
        }

        public void SetEntrancePrice(int price)
        {
            entrancePrice.text = price.ToString();
        }
    }
}