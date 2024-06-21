using DeathMatch;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;

public class UniqueEvents : MonoBehaviour
{
    [Header("x2 Score")]
    [SerializeField] private RectTransform x2Indicator;
    [SerializeField] private float xPosOnClose;
    [SerializeField] private float openingDuration = 0.5f;

    [Header("Pirates Attack")]
    [SerializeField] private Transform piratesShip;
    [SerializeField] private Vector3 initPos;
    [SerializeField] private Vector3 attackPos;
    [SerializeField] private Vector3 finalPos;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Transform[] cannonSpawnPoints;
    [SerializeField] private ParticleSystem[] fireParticles;
    [SerializeField] private CannonBall cannonBall;
    [SerializeField] private AudioSource fireAS;

    [Header("Water Rising")]
    [SerializeField] private Transform water;
    [SerializeField] private int blocksToRemove = 2;
    [SerializeField] private float riseHeight;

    [Header("Popup")]
    [SerializeField] private RectTransform popup;
    [SerializeField] private TextMeshProUGUI popupTitle;
    [SerializeField] private TextMeshProUGUI popopDescription;
    [SerializeField] private float startHeight;
    [SerializeField] private float transitionDuration;

    private Queue<Action> pirateShipAttacks = new Queue<Action>();

    private EventManager eventManager;
    private Scoring scoring;
    private TutorialSteps tutorialSteps;
    private SharkScript sharkScript;

    private bool isUniqueEventsActive;

    private void Start()
    {
        eventManager = FindObjectOfType<EventManager>();
        scoring = FindObjectOfType<Scoring>();
        tutorialSteps = FindObjectOfType<TutorialSteps>();
        sharkScript = FindObjectOfType<SharkScript>();

        isUniqueEventsActive = SaveManager.Get<bool>(SaveManager.UNIQUE_EVENTS_ACTIVE);

        HideX2Indicator(true);
        HidePopup(true);
        piratesShip.gameObject.SetActive(false);

        eventManager.OnGetReadyForNewRound += CheckNewRound;
    }

    private void OnDisable()
    {
        eventManager.OnGetReadyForNewRound -= CheckNewRound;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            PiratesAttack();
    }

    private void CheckNewRound()
    {
        if (tutorialSteps.TutorialMode)
        {
            eventManager.ConfirmStartNewRound();
            return;
        }

        if (!isUniqueEventsActive)
        {
            if (eventManager.Round == 1)
            {
                eventManager.ConfirmStartNewRound();
            }
            else
            {
                var playersWithPirateShip = eventManager.Characters.Where(x => x.HasAttackShip).ToList();

                for (int i = 0; i < playersWithPirateShip.Count; i++)
                {
                    var index = i;

                    if (!scoring.IsCorrectAnswer(playersWithPirateShip[index]))
                        continue;

                    pirateShipAttacks.Enqueue(() =>
                    {
                        EventManager.CharacterInstance characterToAttack = null;

                        if (scoring.Position(playersWithPirateShip[index].Player.Username) != 1)
                        {
                            characterToAttack = scoring.GetOrderedCharacters()[0];
                        }
                        else
                        {
                            characterToAttack = scoring.GetOrderedCharacters()[1];
                        }

                        if (characterToAttack != null)
                        {
                            ShowPopup("حمله کشتی", $"{playersWithPirateShip[index].Player.Username} درست جواب داد و به {(characterToAttack.Player.Username == eventManager.Player.Username ? "شما" : characterToAttack.Player.Username)} شلیک کرد", () =>
                            {
                                StartCoroutine(PiratesAttackCoroutine(characterToAttack));
                            });
                        }
                    });
                }

                CheckPirateShipAttacks();
            }

            return;
        }

        scoring.ResetScoreMultiplier();
        HideX2Indicator();

        switch (eventManager.Round)
        {
            case 6:
                X2Score();
                break;
            case 4:
                PiratesAttack();
                break;
            case 2:
                WaterRising();
                break;
            default:
                eventManager.ConfirmStartNewRound();
                break;
        }
    }

    private void X2Score()
    {
        ShowPopup("X2", "این مرحله همه چیز 2 برابره <sprite index=1>", () =>
        {
            StartCoroutine(X2ScoreCoroutine());
        });
    }

    private IEnumerator X2ScoreCoroutine()
    {
        yield return new WaitForEndOfFrame();
        scoring.ScoreMultiplier = 2;
        ShowX2Indicator();
        eventManager.ConfirmStartNewRound();
    }

    public void ShowX2Indicator()
    {
        x2Indicator.DOAnchorPosX(0, openingDuration);
    }

    public void HideX2Indicator(bool force = false)
    {
        x2Indicator.DOAnchorPosX(xPosOnClose, force ? 0 : openingDuration);
    }

    private void PiratesAttack()
    {
        ShowPopup("دزدان دریایی", "دزدان دریایی برای غارت اومدن <sprite index=5>", () =>
        {
            StartCoroutine(PiratesAttackCoroutine());
        });
    }

    private void CheckPirateShipAttacks()
    {
        if (pirateShipAttacks.Count == 0)
        {
            eventManager.ConfirmStartNewRound();
            return;
        }

        pirateShipAttacks.Dequeue()?.Invoke();
    }

    private IEnumerator PiratesAttackCoroutine(EventManager.CharacterInstance specificCharacter = null)
    {
        AudioManager.Instance.PirateShipEntranceSFX();

        piratesShip.gameObject.SetActive(true);
        piratesShip.localPosition = initPos;

        var isMovingToAttackPos = true;
        var distanceToAttack = Vector3.Distance(piratesShip.position, attackPos);
        piratesShip.DOMove(attackPos, moveDuration * distanceToAttack / 10f).OnComplete(() => isMovingToAttackPos = false);

        yield return new WaitWhile(() => isMovingToAttackPos);
        yield return new WaitForSeconds(0.5f);

        EventManager.CheckForNewRound = false;

        if (specificCharacter == null)
        {
            for (int i = 0; i < eventManager.Characters.Count; i++)
                AttackCharacter(i);
        }
        else
        {
            AttackCharacter(eventManager.Characters.FindIndex(x => x == specificCharacter), 1);
        }

        fireAS.Play();

        EventManager.CheckForNewRound = true;

        yield return new WaitForSeconds(3f);

        var distanceToFinal = Vector3.Distance(piratesShip.position, finalPos);
        var isMovingToFinal = true;
        piratesShip.DOMove(finalPos, moveDuration * distanceToFinal / 10f).OnComplete(() => isMovingToFinal = false);

        yield return new WaitWhile(() => isMovingToFinal);

        piratesShip.gameObject.SetActive(false);

        if (pirateShipAttacks.Count == 0)
            eventManager.ConfirmStartNewRound();
        else
            CheckPirateShipAttacks();

        void AttackCharacter(int i, int? overrideBlockCount = null)
        {
            var index = i;

            // Dont shoot player with 1 or less than 1 block
            if (eventManager.Characters[index].CurrentBlocks.Count <= 1)
                return;

            var targetPos = eventManager.Characters[i].Pivot.transform.position;

            var ball = Instantiate(cannonBall, cannonSpawnPoints[i].transform.position, cannonSpawnPoints[i].transform.rotation, null);
            ball.Fire(targetPos, () =>
            {
                var player = eventManager.Characters[index].Player;
                var playerBlocksCount = eventManager.Characters[index].CurrentBlocks.Count;

                if (overrideBlockCount.HasValue)
                    scoring.ScoreDown(player.Username, true, overrideBlockCount.Value);
                else
                    scoring.ScoreDown(player.Username, true, Mathf.Clamp(Mathf.CeilToInt(playerBlocksCount / 2f), 0, playerBlocksCount - 1));
            });

            fireParticles[i].Play();
        }
    }

    private void WaterRising()
    {
        ShowPopup("جزر و مد", "آب دریا در حال بالا اومدنه <sprite index=4>", () =>
        {
            StartCoroutine(WaterRisingCoroutine());
        });
    }

    private IEnumerator WaterRisingCoroutine()
    {
        var currentWaterHeight = water.transform.position.y;
        var waitingToRise = true;

        water.DOMoveY(currentWaterHeight + riseHeight, 2f).OnComplete(() =>
        {
            waitingToRise = false;
        });

        yield return new WaitWhile(() => waitingToRise);
        yield return new WaitForSeconds(1);

        scoring.ScoreMultiplier = blocksToRemove;

        EventManager.CheckForNewRound = false;

        foreach (var player in eventManager.Characters)
            scoring.ScoreDown(player.Player.Username, false);

        scoring.ResetScoreMultiplier();
        eventManager.UpdateWaterHeight();

        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => sharkScript.TargetsLeft == 0);
        yield return new WaitForSeconds(0.5f);

        EventManager.CheckForNewRound = true;

        if (eventManager.Characters.Where(x => x.Player.Dead == false).Count() <= 1)
            eventManager.IsGameEnded();
        else
            eventManager.ConfirmStartNewRound();
    }

    public void ShowPopup(string title, string description, Action onComplete)
    {
        StartCoroutine(ShowPopupCoroutine(title, description, onComplete));
    }

    public IEnumerator ShowPopupCoroutine(string title, string description, Action onComplete)
    {
        popup.DOAnchorPosY(startHeight, 0);
        popup.gameObject.SetActive(true);
        popupTitle.text = title;
        popopDescription.text = description;

        var isOpening = true;
        popup.DOAnchorPosY(0, transitionDuration).OnComplete(() => isOpening = false);

        yield return new WaitWhile(() => isOpening);
        yield return new WaitForSeconds(3f);

        HidePopup();
        onComplete?.Invoke();
    }

    public void HidePopup(bool force = false)
    {
        popup.DOAnchorPosY(startHeight, force ? 0 : transitionDuration).OnComplete(() => popup.gameObject.SetActive(false));
    }
}