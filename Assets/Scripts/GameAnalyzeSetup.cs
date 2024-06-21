using UnityEngine;
using GameAnalyticsSDK;

public class GameAnalyzeSetup : MonoBehaviour
{
	public static GameAnalyzeSetup Instance;
	
    private void Awake()
    {
		transform.SetParent(null);
		
		if(Instance != null)
		{
				Destroy(gameObject);
				return;
		}
		
		Instance = this;
		DontDestroyOnLoad(gameObject);
		
        GameAnalytics.Initialize();
    }
}