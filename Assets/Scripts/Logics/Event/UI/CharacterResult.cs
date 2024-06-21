using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DeathMatch
{
    public class CharacterResult : MonoBehaviour
    {
        public TextMeshProUGUI usernameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI rankLevel;
        public TextMeshProUGUI correctAnswersCountText;
        public TextMeshProUGUI wrongAnswersCountText;
        public GameObject notFinishedCover;

        public Image positionImage;

        public void Setup(string userName, int position, int correctAnswers, int wrongAnswers, bool finishedGame)
        {
            usernameText.text = userName;
            positionImage.sprite = GameManager.Instance.positionSprites[position];

            correctAnswersCountText.text = correctAnswers.ToString();
            wrongAnswersCountText.text = wrongAnswers.ToString();

            positionImage.gameObject.SetActive(finishedGame ? true : false);
            notFinishedCover.SetActive(finishedGame ? false : true);
        }
    }
}