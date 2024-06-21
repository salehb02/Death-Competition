using DeathMatch;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ItemButton : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private Image baseImage;
    [SerializeField] private Sprite normalBase;
    [SerializeField] private Sprite purchasedBase;

    [Header("Icon")]
    [SerializeField] private Image iconImage;

    [Header("Unlock by Coin")]
    [SerializeField] private GameObject priceHolder;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Unlock by Ad")]
    [SerializeField] private GameObject adHolder;
    [SerializeField] private TextMeshProUGUI adText;

    [Header("Overlays")]
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject purchasedOverlay;

    private Button button;

    public void Setup(PlayerCustomization.UIInfo data, UnityAction onClick)
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(onClick);

        iconImage.sprite = data.Icon;

        priceHolder.SetActive(data.Style.virtualPayment.active);
        priceText.text = data.Style.virtualPayment.price.ToString();

        adHolder.SetActive(data.Style.adPurchase > 0);
        adText.text = $"{data.Style.adWatched}/{data.Style.adPurchase}";

        if (data.Style.locked)
        {
            lockOverlay.SetActive(true);
            adHolder.SetActive(false);
            priceHolder.SetActive(false);
        }

        if (data.Style.bought)
        {
            baseImage.sprite = purchasedBase;
            priceHolder.SetActive(false);
            adHolder.SetActive(false);
            lockOverlay.SetActive(false);

            if (data.Style.chosen)
                purchasedOverlay.SetActive(true);
            else
                purchasedOverlay.SetActive(false);
        }
        else
        {
            baseImage.sprite = normalBase;
            purchasedOverlay.SetActive(false);
        }
    }
}
