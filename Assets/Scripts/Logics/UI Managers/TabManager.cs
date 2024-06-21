using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    [SerializeField] private Tab[] tabs;
    [SerializeField] private TabOptions selectedTabOptions;
    [SerializeField] private TabOptions unselectedTabOptions;

    [System.Serializable]
    public class Tab
    {
        public Button Button;
        public GameObject Panel;

        public UnityEvent OnClick;
    }

    [System.Serializable]
    public class TabOptions
    {
        public Sprite Sprite;
        public float PreferredHeight;
    }

    private void Start()
    {
        InitTabs();
        SelectTab(0);
    }

    private void InitTabs()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            var currentTab = tabs[i];
            var index = i;

            currentTab.Button.onClick.AddListener(() => SelectTab(index));
            currentTab.Button.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
            currentTab.Button.onClick.AddListener(() => currentTab.OnClick?.Invoke());
        }
    }

    public void SelectTab(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            var currentTab = tabs[i];

            if (i == index)
            {
                currentTab.Panel.SetActive(true);
                currentTab.Button.image.sprite = selectedTabOptions.Sprite;

                var layoutElement = currentTab.Button.GetComponent<LayoutElement>();

                if (layoutElement != null)
                {
                    layoutElement.preferredHeight = selectedTabOptions.PreferredHeight;
                }
            }
            else
            {
                currentTab.Panel.SetActive(false);
                currentTab.Button.image.sprite = unselectedTabOptions.Sprite;

                var layoutElement = currentTab.Button.GetComponent<LayoutElement>();

                if (layoutElement != null)
                {
                    layoutElement.preferredHeight = unselectedTabOptions.PreferredHeight;
                }
            }
        }
    }
}