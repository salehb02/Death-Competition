using SDM;
using UnityEngine;

public class CheckVersionClient : MonoBehaviour
{
    private CheckVersionClientPresentor presentor;
    private Loading loading;

    private void Start()
    {
        loading = FindObjectOfType<Loading>();
        presentor = GetComponent<CheckVersionClientPresentor>();
    }

    public void ShowUpdatePanel(CheckVersion checkVersion)
    {
        if (!checkVersion.updateNeed)
            return;

        presentor.SetPanelActivation(true);
        presentor.LoadInfo(checkVersion);
    }

    public void EnterGame()
    {
        loading.PassCheckVersion();
        presentor.SetPanelActivation(false);
    }
}