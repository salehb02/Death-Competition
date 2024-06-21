using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RTLTMPro;
using UnityEngine.SceneManagement;

namespace DeathMatch
{
    public class Character : MonoBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private GameObject playerInfoBox;
        [SerializeField] private RTLTextMeshPro3D usernameText;
        [SerializeField] private bool isLeaderboardUser;

        [SerializeField] private BlockCustomization[] blocks;

        public string Username { get; set; }
        public int Score { get; set; }
        public bool IsPlayer { get; set; }
        public bool IsAnswered { get; set; }
        public bool Helped { get; set; }
        public bool BoostersUsed
        {
            get => Helped ? true : false;
        }

        public float OverallTimer { get; private set; }
        public float AnswerTimer { get; private set; }
        public bool Dead { get; set; }
        public bool HasPirateShip { get; set; }

        public Rigidbody Rigidbody { get; private set; }
        public AI AI { get; set; }

        private Vector3 _initPlayerInfoBoxOffset;
        private EventManager _gameManager;
        private PlayerCustomization _customization;
        public Animator animator;
        private Transform playerInfoPivot;

        private List<string> _textualAnswers = new List<string>();
        private List<float> _numericAnswers = new List<float>();

        private int _correctAnswersCount;
        private int _wrongAnswersCount;

        public const string DROWNING_ANIMATION = "Drown";
        public const string STRESS_ANIMATION = "Stress";
        public const string IDLE_ANIMATION = "Idle";
        public const string HAPPY_ANIMATION = "Happy";
        public const string HAPPY_LOOP_INDEX = "Happy Index";
        public const string HAPPY_LOOP_ANIMATION = "Happy Loop";
        public const string SAD_ANIMATION = "Sad";

        private void Start()
        {
            _gameManager = GetComponentInParent<EventManager>();
            Rigidbody = GetComponent<Rigidbody>();
            animator.SetFloat("Offset", Random.Range(0f, 1f));

            _initPlayerInfoBoxOffset = playerInfoBox.transform.localPosition;
            playerInfoBox.transform.SetParent(null);

            if (!IsInGameplayScene())
            {
                if (!isLeaderboardUser)
                {
                    LoadPlayer(true, false);
                    playerInfoBox.gameObject.SetActive(false);
                }
            }
            else
            {
                playerInfoPivot = transform;
                playerInfoBox.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            PlayerInfoPosition();

            if (!_gameManager)
                return;

            if (!IsAnswered && _gameManager.IsGameStarted && !Dead)
            {
                OverallTimer += Time.deltaTime;
                AnswerTimer += Time.deltaTime;
            }
        }

        public void ResetForNewRound()
        {
            Helped = false;
            IsAnswered = false;
            AnswerTimer = 0;
        }

        private bool IsInGameplayScene()
        {
            return SceneManager.GetActiveScene().name == GameManager.Instance.gameplayScene;
        }

        private void PlayerInfoPosition()
        {
            if (playerInfoPivot == null)
                return;

            playerInfoBox.transform.localPosition = playerInfoPivot.position + _initPlayerInfoBoxOffset;
        }

        public void ChangePlayerInfoPivot(Transform newPivot)
        {
            playerInfoPivot = newPivot;
        }

        public int GetBlocksCount() => _gameManager.Characters.SingleOrDefault(x => x.Player == this).CurrentBlocks.Count;

        public void LoadPlayer(bool isPlayer, bool showUsername = true, GSDM.MatchMakingUser opponentData = null)
        {
            IsPlayer = isPlayer;
            Dead = false;

            if (isPlayer)
            {
                //Username = SaveManager.Get<string>(SaveManager.PLAYER_USERNAME);
                Username = "شما";
            }
            else
            {
                if (AI == null)
                    AI = gameObject.AddComponent<AI>();

                var tut = FindObjectOfType<TutorialSteps>();

                if (tut.TutorialMode)
                {
                    var bot = TutorialSteps.GetBot;
                    Username = bot.Name;
                    AI.SetScripted(bot);
                }
                else
                {
                    Username = opponentData.userName;
                }
            }

            if (!_customization)
                _customization = GetComponent<PlayerCustomization>();

            _customization.IsPlayer = IsPlayer;

            if (opponentData != null)
            {
                switch (opponentData.sex.ToLower())
                {
                    case "f":
                        _customization.Gender = Gender.Female;
                        break;
                    case "m":
                        _customization.Gender = Gender.Male;
                        break;
                    default:
                        break;
                }
            }

            // start avatar customization
            _customization.StartCustomization();

            foreach (var block in blocks)
                block.GetComponent<BlockCustomization>().StartCustomization();

            if (showUsername)
            {
                usernameText.text = Username;
            }
            else
            {
                usernameText.gameObject.SetActive(false);
            }

            if (isPlayer)
                return;

            if (opponentData != null)
            {
                Username = opponentData.userName;
                LoadCustomization(opponentData.set);
            }
            else
            {
                _customization.LoadRandomStyle();
            }
        }

        public void HideAvatar()
        {
            if (!_customization)
                _customization = GetComponent<PlayerCustomization>();

            _customization?.HideAvatar();
        }

        public void LoadCustomization(GSDM.UserSet set)
        {
            if (!_customization)
                _customization = GetComponent<PlayerCustomization>();

            if (set == null || string.IsNullOrEmpty(set.code))
                LoadRandomCustomization();
            else
                _customization?.LoadAIStyle(new List<GSDM.UserSet>() { set });
        }

        public void LoadRandomCustomization()
        {
            if (!_customization)
                _customization = GetComponent<PlayerCustomization>();

            _customization?.LoadRandomStyle();
        }

        public void ApplyPlayerBlock(BlockCustomization block)
        {
            if (GameManager.Instance.LatestPlayerInfo == null)
                throw new System.NullReferenceException("LatestPlayerInfo is null");

            block.ApplyStyleNoCheck(GameManager.Instance.LatestPlayerInfo.data.userPlatform);
        }

        public void Drowning(float waterSurfaceY)
        {
            if (transform.position.y + 0.5f > waterSurfaceY)
                return;

            if (Rigidbody.isKinematic)
                return;

            Rigidbody.isKinematic = true;
            SetAnimationMode(DROWNING_ANIMATION);
            AudioManager.Instance.WaterFallSFX();
        }

        public void Scream()
        {
            switch (GetGender())
            {
                case Gender.Both:
                case Gender.Null:
                case Gender.Male:
                    AudioManager.Instance.MaleScreamSFX();
                    break;
                case Gender.Female:
                    AudioManager.Instance.FemaleScreamSFX();
                    break;
                default:
                    break;
            }
        }

        public Gender GetGender()
        {
            if (!_customization)
                return Gender.Male;

            return _customization.GetSelectedStyle().Gender;
        }

        public void SetAnimationMode(string mode)
        {
            if (!animator)
                animator = GetComponentInChildren<Animator>();

            animator.SetTrigger(mode);
        }

        public void HidePlayerInfo()
        {
            usernameText.gameObject.SetActive(false);
        }

        public void AnswerTextual(string answer)
        {
            if (IsAnswered)
                return;

            _textualAnswers.Add(answer.Trim());
            IsAnswered = true;
        }

        public void AnswerNumeric(float answer)
        {
            if (IsAnswered)
                return;

            _numericAnswers.Add(answer);
            IsAnswered = true;
        }

        public void IncreaseCorrectAnswersCount() => _correctAnswersCount++;

        public void IncreaseWrongAnswersCount() => _wrongAnswersCount++;

        public string GetLastTextualAnswer()
        {
            return _textualAnswers[_textualAnswers.Count - 1];
        }

        public float GetLastNumericAnswer()
        {
            return _numericAnswers[_numericAnswers.Count - 1];
        }

        public int GetWrongAnswersCount() => _wrongAnswersCount;

        public int GetCorrectAnswersCount() => _correctAnswersCount;

        public void PlayHappyLoopAnimation()
        {
            animator.SetFloat(HAPPY_LOOP_INDEX, Random.Range(0, 4));
            animator.SetTrigger(HAPPY_LOOP_ANIMATION);
        }
    }
}