using UnityEngine;

public class LoadingCharacterIndicator : MonoBehaviour
{
    #region Singlton
    public static LoadingCharacterIndicator Instance;

    private void Awake()
    {
        transform.SetParent(null);

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [SerializeField] private GameObject loadingSprite;

    private void Start()
    {
        HideLoadingIndicator();
    }

    public void ShowLoadingIndicator(Transform player)
    {
        var rectTransform = loadingSprite.GetComponent<RectTransform>();

        var pos = (Vector2)player.position;
        var viewportPoint = (Vector2)Camera.main.WorldToViewportPoint(pos);

        rectTransform.anchorMin = viewportPoint;
        rectTransform.anchorMax = viewportPoint;

        loadingSprite.SetActive(true);
    }

    public void HideLoadingIndicator()
    {
        loadingSprite.SetActive(false);
    }
}