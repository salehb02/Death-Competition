using DeathMatch;
using DG.Tweening;
using UnityEngine;

public class CrownManager : MonoBehaviour
{
    [SerializeField] private GameObject crownPrefab;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float sinusSpeed = 1f;
    [SerializeField] private float sinusDistance = 1f;

    private Scoring scoring;
    private GameObject createdCrown;
    private EventManager eventManager;

    private void Start()
    {
        scoring = FindObjectOfType<Scoring>();
        eventManager = FindObjectOfType<EventManager>();

        CreateCrowns();
    }

    private void Update()
    {
        if (!scoring)
            return;

        UpdateCrowns();
    }

    private void CreateCrowns()
    {
        createdCrown = Instantiate(crownPrefab, transform);
        createdCrown.SetActive(false);
    }

    private void UpdateCrowns()
    {
        var players = scoring.GetOrderedCharacters();

        if (players.Count == 0)
            return;

        if (eventManager.Round <= 1)
        {
            createdCrown.SetActive(false);
            return;
        }

        var topPlayer = players[0];

        createdCrown.SetActive(true);
        createdCrown.transform.position = topPlayer.Player.transform.position + offset + (Vector3.up * Mathf.Sin(Time.time * sinusSpeed) * sinusDistance);
        createdCrown.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    public void ScaleUp()
    {
        createdCrown.transform.DOScale(1.3f, 0.5f);
    }
}