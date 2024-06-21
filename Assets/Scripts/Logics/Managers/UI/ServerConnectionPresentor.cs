using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DeathMatch
{
    public class ServerConnectionPresentor : MonoBehaviour
    {
        [SerializeField] private GameObject connectingPanel;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI ensureNetConnectionText;

        [Header("Server Status")]
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private TextMeshProUGUI statusTitle;
        [SerializeField] private TextMeshProUGUI statusDescription;
        [SerializeField] private Button okButton;

        private void Start()
        {
            ResetUI();

            okButton.onClick.AddListener(Application.Quit);
            okButton.onClick.AddListener(() => { Debug.Log("FUCK"); });
        }

        public void ActiveConnectionPanel(bool active)
        {
            connectingPanel.SetActive(active);
        }

        public void SetTimerText(string timer) => timerText.text = timer == string.Empty ? null : $"({timer})";

        public void ActiveEnsureNetText(bool active) => ensureNetConnectionText.gameObject.SetActive(active);
        
        public void SetServerStatusPanelActivation(bool active)
        {
            statusPanel.SetActive(active);
        }
        
        public void SetServerStatusTitle(string text)
        {
            statusTitle.text = text;
        }

        public void SetServerStatusDescription(string text)
        {
            statusDescription.text = text;
        }

        public void ResetUI()
        {
            ActiveConnectionPanel(false);
            SetTimerText(string.Empty);
            ActiveEnsureNetText(false);
            
            SetServerStatusPanelActivation(false);
            SetServerStatusDescription(null);
        }
    }
}