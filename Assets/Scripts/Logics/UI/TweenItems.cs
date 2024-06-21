using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public class TweenItems : MonoBehaviour
{
    [SerializeField] private RectTransform[] items;
    [SerializeField] private float randomationRadius = 1f;
    [SerializeField] private Ease creationEase;
    [SerializeField] private float creationEaseDuration = 0.2f;
    [SerializeField] private Ease moveEase;
    [SerializeField] private float moveEaseDuration = 0.5f;
    [SerializeField] private RectTransform destination;
    [SerializeField] private float delayBetweenItems = 0.2f;
    [SerializeField] private float startDelay;
    [SerializeField] private AudioClip sfxOnEnd;
    [SerializeField] private float sfxVolume = 1f; 

    private void Start()
    {
        InitTween();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            PlayTween(null);
    }

    private void InitTween()
    {
        foreach (var item in items)
            item.gameObject.SetActive(false);
    }

    public void PlayTween(Action onPlay = null)
    {
        StartCoroutine(PlayTweenCoroutine(onPlay));
    }

    private IEnumerator PlayTweenCoroutine(Action onPlay)
    {
        yield return new WaitForSeconds(startDelay);

        var isActionPlayed = false;

        foreach (var item in items)
        {
            item.anchoredPosition = Vector2.zero;
            item.gameObject.SetActive(true);
            item.DOAnchorPos(new Vector2(Random.Range(-randomationRadius, randomationRadius), Random.Range(-randomationRadius, randomationRadius)), creationEaseDuration)
                .SetEase(creationEase).OnComplete(() =>
                {
                    item.DOAnchorPos(destination.localPosition, moveEaseDuration).SetEase(moveEase).OnComplete(() =>
                    {
                        item.gameObject.SetActive(false);
                        AudioManager.Instance.CustomPlayOneShot(sfxOnEnd, sfxVolume + Random.Range(-0.2f, 0.2f));

                        if (!isActionPlayed)
                        {
                            onPlay?.Invoke();
                            isActionPlayed = true;
                        }
                    });
                });

            yield return new WaitForSeconds(delayBetweenItems);
        }
    }
}