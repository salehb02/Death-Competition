using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingPresentor : MonoBehaviour
{
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI percentText;

    private string loadingSituation = "درحال بارگزاری";

    private void Start()
    {
        loadingBar.value = 0;
        loadingText.text = "0%";

        StartCoroutine(LoadingTextCoroutine());
    }

    public void SetLoadingSituation(string text)
    {
        loadingSituation = text;
    }

    public void SetBarValue(float value)
    {
        loadingBar.DOKill();
        loadingBar.DOValue(value, 0.5f).OnUpdate(() =>
        {
            percentText.text = Convert.ToInt32(loadingBar.value * 100f) + "%";
        });
    }

    private IEnumerator LoadingTextCoroutine()
    {
        var dotCount = 0;

        while (true)
        {
            switch (dotCount)
            {
                case 0:
                    loadingText.text = $"{loadingSituation}";
                    break;
                case 1:
                    loadingText.text = $"{loadingSituation}.";
                    break;
                case 2:
                    loadingText.text = $"{loadingSituation}..";
                    break;
                case 3:
                    loadingText.text = $"{loadingSituation}...";
                    break;
                default:
                    break;
            }

            dotCount++;

            if (dotCount == 4)
                dotCount = 0;

            yield return new WaitForSeconds(0.5f);
        }
    }
}