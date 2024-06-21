using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DeathMatch;

public class Keyboard : MonoBehaviour
{
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;

    private KeyboardButton[] _keyboardButtons;
    private KeyboardsManager _keyboardsManager;

    public event Action<string> OnSubmitAnswer;

    private void Start()
    {
        _keyboardsManager = GetComponentInParent<KeyboardsManager>();
        _keyboardButtons = GetComponentsInChildren<KeyboardButton>(true);

        submitButton.onClick.AddListener(Submit);
    }

    private void Submit()
    {
        OnSubmitAnswer?.Invoke(answerInput.text);
        AudioManager.Instance.AnswerSendSFX();
        _keyboardsManager.DisableKeyboadInteration();
    }

    public void AddLetter(string letter)
    {
        SetSubmitInteraction(true);
        answerInput.text += letter;
    }

    public void Backspace()
    {
        if (answerInput.text.Length > 0)
            answerInput.text = answerInput.text.Remove(answerInput.text.Length - 1);
    }

    public void SetSubmitInteraction(bool interaction)
    {
        submitButton.interactable = interaction;
    }

    public void SetAnswerInputText(string text) => answerInput.text = text;

    public void SetButtonsInteraction(bool interaction)
    {
        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            btn.interactable = interaction;
        }

        //for (int i = 0; i < _keyboardButtons.Length; i++)
        //{
        //    _keyboardButtons[i].GetButton.interactable = interaction;
        //}
    }
}