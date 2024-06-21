using SDM;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckVersionClientPresentor : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI currentVersionText;
    [SerializeField] private TextMeshProUGUI allowedVersionText;
    [SerializeField] private TextMeshProUGUI latestVersionText;
    [SerializeField] private Button downloadButton;
    [SerializeField] private Button downloadLaterButton;

    private CheckVersionClient checkVersion;

    private void Start()
    {
        checkVersion = GetComponent<CheckVersionClient>();

        SetPanelActivation(false);
        downloadLaterButton.onClick.AddListener(checkVersion.EnterGame);
    }

    public void SetPanelActivation(bool active)
    {
        panel.SetActive(active);
    }

    public void LoadInfo(CheckVersion checkVersion)
    {
        currentVersionText.text = Application.version;
        allowedVersionText.text = checkVersion.allowedVersion;
        latestVersionText.text = checkVersion.currentVersion;

        var currVersion = new Version(Application.version);
        var allowedVersion = new Version(checkVersion.allowedVersion);

        var isAllowedToEnter = currVersion.CompareTo(allowedVersion) >= 0 ? true : false;

        downloadLaterButton.gameObject.SetActive(isAllowedToEnter);

#if MYKET_SUPPORTED
        downloadButton.onClick.AddListener(() => Application.OpenURL(checkVersion.myketUrl));
#elif BAZAAR_SUPPORTED
        downloadButton.onClick.AddListener(() => Application.OpenURL(checkVersion.bazarUrl));
#endif
    }
}