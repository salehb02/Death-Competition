using UnityEngine;
using DG.Tweening;
using System.Collections;

public class KeyboardsManager : MonoBehaviour
{
    [Header("Keyboards")]
    public Keyboard textualKeyboard;
    public Keyboard numericKeyboard;

    [Space(2)]
    [Header("DoTween")]
    public float closePosition;
    public float openPosition;
    public float transitionTime = 1f;

    private RectTransform _rectTransform;
    private DeathMatch.EventManager _eventManager;
    public Keyboard CurrentKeyboard { get; private set; }

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _eventManager = FindObjectOfType<DeathMatch.EventManager>();
    }

    public void ActiveTextualKeyboard()
    {
        textualKeyboard.gameObject.SetActive(true);
        numericKeyboard.gameObject.SetActive(false);
        textualKeyboard.SetSubmitInteraction(false);

        CurrentKeyboard = textualKeyboard;
    }

    public void ActiveNumericKeyboard()
    {
        textualKeyboard.gameObject.SetActive(false);
        numericKeyboard.gameObject.SetActive(true);
        numericKeyboard.SetSubmitInteraction(false);

        CurrentKeyboard = numericKeyboard;
    }

    public void HideKeyboard(bool force = false)
    {
        _rectTransform.DOKill();
        _rectTransform.DOAnchorPosY(closePosition, force ? 0 : transitionTime);
    }

    public void DisableKeyboadInteration()
    {
        textualKeyboard.SetAnswerInputText(string.Empty);
        numericKeyboard.SetAnswerInputText(string.Empty);

        textualKeyboard.SetSubmitInteraction(false);
        numericKeyboard.SetSubmitInteraction(false);

        textualKeyboard.SetButtonsInteraction(false);
        numericKeyboard.SetButtonsInteraction(false);
    }

    public void ShowKeyboards(float delay)
    {
        StartCoroutine(ShowKeyboardsCoroutine(delay));
    }

    private IEnumerator ShowKeyboardsCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        textualKeyboard.SetAnswerInputText(string.Empty);
        numericKeyboard.SetAnswerInputText(string.Empty);

        textualKeyboard.SetButtonsInteraction(true);
        numericKeyboard.SetButtonsInteraction(true);

        // textualKeyboard.SetSubmitInteraction(true);
        //numericKeyboard.SetSubmitInteraction(true);

        _rectTransform.DOKill();
        _rectTransform.DOAnchorPosY(openPosition, transitionTime).OnComplete(() => _eventManager?.TUT_StartTrigger());
    }
}