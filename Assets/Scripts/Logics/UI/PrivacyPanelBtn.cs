using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class PrivacyPanelBtn : MonoBehaviour
{
    [SerializeField] private GameObject privacyPanel;
    [SerializeField] private Ease openEase;
    [SerializeField] private Ease closeEase;
    private Button btn;
    private bool isOpen;

    private void Start()
    {
        btn = GetComponent<Button>();

        btn.onClick.AddListener(TogglePanel);
        btn.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    private void TogglePanel()
    {
        isOpen = !isOpen;

        privacyPanel.transform.DOKill();

        if (isOpen)
        {
            privacyPanel.transform.DOLocalMoveY(-225f, 0.5f).SetEase(openEase);
        }
        else
        {
            privacyPanel.transform.DOLocalMoveY(0, 0.5f).SetEase(closeEase);
        }
    }
}