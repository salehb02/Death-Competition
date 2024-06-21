using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryItem : MonoBehaviour
{
    [SerializeField] private Image categoryIcon;
    [SerializeField] private TextMeshProUGUI categoryTitle;
    [SerializeField] private Image background;
    [SerializeField] private Sprite selectedBackground;
    [SerializeField] private Sprite unselectedBackground;

    private Button button;
    private QuestionCreation questionCreation;

    public void Setup(ESDM.EventCategory data)
    {
        button = GetComponent<Button>();
        questionCreation = FindObjectOfType<QuestionCreation>();

        categoryTitle.text = data.category;

        categoryIcon.sprite = GameManager.Instance.questionCategoryIcons.SingleOrDefault(x => x.name == data.id.ToString());

        SetUIAsUnSelected();
        button.onClick.AddListener(() => questionCreation.SelectCategory(this, data));
        button.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    public void SetUIAsSelected()
    {
        background.sprite = selectedBackground;
    }

    public void SetUIAsUnSelected()
    {
        background.sprite = unselectedBackground;
    }
}