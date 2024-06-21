using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateLogChecker : MonoBehaviour
{
    [SerializeField] private GameObject updateLogPanel;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        closeButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        closeButton.onClick.AddListener(() => updateLogPanel.SetActive(false));

        updateLogPanel.SetActive(false);

        var log = GameManager.Instance.CheckForUpdateLog();

        if (log != null)
        {
            logText.text = log;
            updateLogPanel.SetActive(true);
        }
    }
}