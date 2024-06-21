using DeathMatch;
using UnityEngine;

public class InitScene : MonoBehaviour
{
    public const string CHECK_SERVER_PREFS = "FIRST_LAUNCH";

    [System.Obsolete]
    private void Start()
    {
        SaveManager.Set(CHECK_SERVER_PREFS, true);
        LevelLoader.LoadLevelSilence(GameManager.Instance.loadingScene);
    }
}