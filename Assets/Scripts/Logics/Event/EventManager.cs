using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GSDM;
using System;
using Random = UnityEngine.Random;
using DG.Tweening;
using USDM;
using TMPro;

namespace DeathMatch
{
    public class EventManager : MonoBehaviour
    {
        [Header("Main")]
        public int maximumPlayerCount = 5;
        public int maxRoundCount = 18;
        public float InitCharactersSpace = 3f;
        public float FinalCharactersSpaceMultiplier = 2f;
        public float addBlockFromYDistance = -10f;

        public Character characterPrefab;
        public Block blockPrefab;
        public Material blockMaterial;
        [SerializeField] private Transform sharkSystemHolder;

        [Space(2)]
        public GameObject[] charactersLoading;

        [Space(2)]
        [Header("Camera")]
        public new Camera camera;
        public Vector3 cameraOffset;
        public Vector3 resultsPhaseOffset;
        public float InitFOV = 20;
        public float FinalFOV = 30;

        [Space(2)]
        [Header("SFX")]
        public AudioSource _CountDownSound;

        [Space(2)]
        [Header("Skull On Die")]
        [SerializeField] private GameObject skullPrefab;
        [SerializeField] private float skullInstantiateHeight = -5f;
        [SerializeField] private float skullInstantiateDelay = 3f;

        [Space(2)]
        [Header("Final")]
        [SerializeField] private LightingManager lightingManager;
        [SerializeField] private int lightPresentIndex;
        [SerializeField] private GameObject lightProjectorPrefab;
        [SerializeField] private Vector3 lightProjectorOffset;
        [SerializeField] private TextMeshPro playerRankPrefab;
        [SerializeField] private Vector3 playerRankOffset;

        [Space(2)]
        [Header("Event Timings")]
        public float instantResultTime = 0.75f;
        public float showAnswersTime = 1.5f;
        public float newRoundDelay = 1f;
        public float showKeyboardDelay = 1.5f;

        private Scoring scoring;
        private Timer timer;
        private EmojiManager emojiManager;
        private EventManagerPresentor presentor;
        public SharkScript attackingShark;
        private StylizedWater2.WaterObject waterObject;
        private JSONFileReader JSONFileReader;
        private TutorialSteps tutorial;
        private Matchmaking matchMaking;
        private CrownManager crownManager;
        private MatchReward matchReward;
        private MatchActiveBooster activeBooster;
        private PlayersProperties playerProperties;

        private float firstBlockPositionHeight;
        private bool isGameSaved = false;
        private bool playerIsGoingToBeEaten = false;
        private bool isQuestionReported;

        // AI Boosters
        private bool AIWithExtraBlock;
        private bool AIWithShield;
        private bool AIWithPirateShip;
        //

        public Character Player { get; set; }
        public CharacterInstance PlayerInstance { get => Characters.SingleOrDefault(x => x.Username == Player.Username); }
        public List<CharacterInstance> Characters { get; set; } = new List<CharacterInstance>();
        public List<Answer> CorrectAnswersTextual { get; set; } = new List<Answer>();
        public List<Answer> WrongAnswersTextual { get; set; } = new List<Answer>();
        public List<Answer> CorrectAnswerNumeric { get; set; } = new List<Answer>();
        public List<Answer> WrongAnswerNumeric { get; set; } = new List<Answer>();
        public bool IsGameStarted { get; set; }
        public bool IsRoundInProgress { get; private set; }
        public bool GameEnded { get; private set; }
        public bool IsNumericQuestion { get; set; }
        public int Round { get; private set; }
        public float CurrentWaterHeight { get; set; }

        public static bool CheckForNewRound = true;

        public event Action OnRoundStart;
        public event Action OnGetReadyForNewRound;

        [System.Serializable]
        public class CharacterInstance
        {
            public string Username;
            public GameObject Pivot;
            public Character Player;
            public List<Block> CurrentBlocks = new List<Block>();
            public GameObject BlocksHolder;
            public int ShieldLeft;
            public bool HasAttackShip;

            internal CharacterID _charID;
            internal MatchMakingUser opponentData;
        }

        private void Start()
        {
            timer = FindObjectOfType<Timer>();
            matchMaking = FindObjectOfType<Matchmaking>();
            crownManager = FindObjectOfType<CrownManager>();
            matchReward = FindObjectOfType<MatchReward>();
            activeBooster = FindObjectOfType<MatchActiveBooster>();
            playerProperties = FindObjectOfType<PlayersProperties>();

            matchMaking.OnStartGame += StartGame;

            if (timer)
                timer.OnTimeOut += EndRound;

            scoring = FindObjectOfType<Scoring>();
            JSONFileReader = FindObjectOfType<JSONFileReader>();
            emojiManager = FindObjectOfType<EmojiManager>();
            presentor = FindObjectOfType<EventManagerPresentor>();
            waterObject = FindObjectOfType<StylizedWater2.WaterObject>();
            tutorial = FindObjectOfType<TutorialSteps>();
            UpdateWaterHeight();

            camera.fieldOfView = InitFOV;

            if (tutorial.TutorialMode)
            {
                AddPlayer();

                for (int i = 0; i < maximumPlayerCount - 1; i++)
                    AddOpponent();

                ZoomOutCamera();
                MoveToFinalCharactersSpace();

                StartGame();
            }

            for (int i = 0; i < charactersLoading.Length; i++)
                charactersLoading[i].transform.localPosition = new Vector3((i % 2 == 0 ? -Mathf.Floor(i / 2f) : Mathf.Ceil(i / 2f)) * InitCharactersSpace, 2, 0);
        }

        private void OnDisable()
        {
            if (timer)
                timer.OnTimeOut -= EndRound;

            if (matchMaking)
                matchMaking.OnStartGame -= StartGame;
        }

        private void Update()
        {
            if (sharkSystemHolder != null && waterObject != null)
            {
                var currentPos = sharkSystemHolder.transform.position;
                currentPos.y = waterObject.transform.position.y;
                sharkSystemHolder.transform.position = currentPos;
            }

            CameraPosition();
            BlocksPosition();
            DrowningPlayers();
            DebugInputs();
        }

        public void UpdateWaterHeight()
        {
            CurrentWaterHeight = waterObject.transform.position.y;
        }

        public void SetQuestion(Question question, bool isNumeric)
        {
            presentor.ShowQuestionBox(question);
            IsNumericQuestion = isNumeric;

            if (isNumeric)
            {
                CorrectAnswerNumeric = question.answers;
                WrongAnswerNumeric = question.wrongAnswers;
            }
            else
            {
                CorrectAnswersTextual = question.answers;
                WrongAnswersTextual = question.wrongAnswers;
            }

            isQuestionReported = false;
        }

        public void ZoomOutCamera()
        {
            camera.DOFieldOfView(FinalFOV, 2f);
        }

        public void MoveToFinalCharactersSpace()
        {
            for (int i = 0; i < Characters.Count; i++)
            {
                Characters[i].Pivot.transform.DOLocalMoveX(FinalCharactersSpaceMultiplier * Characters[i].Pivot.transform.localPosition.x, 2f);
            }
        }

        private void StartGame()
        {
            SetKnowledgeLevel();
            StartCoroutine(StartGameCoroutine());
        }

        private int playerPos;

        private List<int> playerIndexes = new List<int>();

        public void AddPlayer()
        {
            playerPos = Random.Range(0, maximumPlayerCount - 1);

            AddCharacter(playerPos, playerPos);
            playerIndexes.Add(playerPos);
            charactersLoading[playerPos].SetActive(false);
        }

        public void AddOpponent()
        {
            for (int i = 0; i < maximumPlayerCount; i++)
            {
                if (playerIndexes.Contains(i))
                    continue;

                AddCharacter(i, playerPos);
                playerIndexes.Add(i);
                charactersLoading[i].SetActive(false);
                break;
            }
        }

        private void AddCharacter(int i, int playerPos)
        {
            // create a new instance
            var playerData = new CharacterInstance();

            // create character base
            playerData.Pivot = new GameObject($"Player {i + 1}");
            playerData.Pivot.transform.SetParent(transform);
            var xPos = (i % 2 == 0 ? -Mathf.Floor(i / 2f) : Mathf.Ceil(i / 2f)) * InitCharactersSpace;
            playerData.Pivot.transform.position = new Vector3(xPos, 0, 0);
            playerData.Username = playerData.Pivot.name;

            // create character's blocks holder
            var blocksHolder = new GameObject("Blocks");
            blocksHolder.transform.SetParent(playerData.Pivot.transform);
            blocksHolder.transform.localPosition = Vector3.zero;
            playerData.BlocksHolder = blocksHolder;

            // save y position for player Y
            var playerYPosition = 0f;

            // save y position for next block
            var lastBlockYPosition = 0f;

            // make it AI if it was not first character
            var IsAI = i == playerPos ? false : true;

            // instantiate blocks
            var blocksCount = 3;

            if (!IsAI)
            {
                // Extra block booster
                if (activeBooster.HasExtraBlock())
                    blocksCount++;

                // Sheild Booster
                if (activeBooster.HasShield())
                    playerData.ShieldLeft = 3;

                // Pirate Ship Booster
                if (activeBooster.HasPirateShip())
                    playerData.HasAttackShip = true;
            }
            else
            {
                if (SaveManager.Get<bool>(SaveManager.UNIQUE_EVENTS_ACTIVE) == false)
                {
                    // Extra block booster for AI
                    if (!AIWithExtraBlock && Random.value < 0.5f)
                    {
                        blocksCount++;
                        AIWithExtraBlock = true;
                    }
                }
            }

            for (int j = 0; j < blocksCount; j++)
            {
                var newPos = new Vector3(blocksHolder.transform.position.x, blocksHolder.transform.position.y + (lastBlockYPosition + GameObjectBounds(blocksHolder.gameObject).extents.y + .4f), blocksHolder.transform.position.z);
                var block = Instantiate(blockPrefab, newPos, Quaternion.identity, blocksHolder.transform);

                block.GetComponent<Rigidbody>().isKinematic = j == 0 ? true : false;

                if (j == 0)
                    firstBlockPositionHeight = GameObjectBounds(blocksHolder.gameObject).extents.y;

                playerData.CurrentBlocks.Add(block);
                lastBlockYPosition = GameObjectBounds(blocksHolder.gameObject).extents.y;
                playerYPosition = block.transform.position.y;
            }

            // instantiate character
            var character = Instantiate(characterPrefab, new Vector3(xPos, playerYPosition + GameObjectBounds(characterPrefab.gameObject).extents.y, 0), Quaternion.identity, playerData.Pivot.transform);

            if (blocksCount == 4)
                character.Score++;

            var opponentData = (IsAI && !tutorial.TutorialMode) ? DataManager.PopOpponent() : null;

            playerData.opponentData = opponentData;

            if (!IsAI)
            {
                Player = character;

                foreach (var block in playerData.CurrentBlocks)
                {
                    block.GetComponent<BlockCustomization>().IsPlayer = true;
                    character.ApplyPlayerBlock(block.GetComponent<BlockCustomization>());
                }
            }
            else
            {
                if (opponentData != null)
                {
                    if (opponentData.platfrom == null || string.IsNullOrEmpty(opponentData.platfrom.code))
                    {
                        var randomModel = playerData.CurrentBlocks[0].GetComponent<BlockCustomization>().GetRandomSetModel();
                        playerData.opponentData.platfrom = randomModel;
                    }

                    foreach (var block in playerData.CurrentBlocks)
                    {
                        var setModel = new SetModel();
                        setModel.code = opponentData.platfrom.code;
                        block.GetComponent<BlockCustomization>().ApplyStyleNoCheck(setModel ?? null);
                    }

                    if (SaveManager.Get<bool>(SaveManager.UNIQUE_EVENTS_ACTIVE) == false)
                    {
                        // Shield booster for AI
                        if (!AIWithShield && Random.value < 0.5f && opponentData.score.level.level >= activeBooster.GetShieldMinimumLevel())
                        {
                            playerData.ShieldLeft = 3;
                            AIWithShield = true;
                        }

                        // Pirate Ship for AI
                        if (!AIWithPirateShip && Random.value < 0.5f && opponentData.score.level.level >= activeBooster.GetPirateShipMinimumLevel())
                        {
                            playerData.HasAttackShip = true;
                            AIWithPirateShip = true;
                        }
                    }
                }
            }

            // load character data
            character.LoadPlayer(!IsAI, opponentData: opponentData);

            // assign main player
            playerData.Player = character;

            // add characted id
            playerData._charID = playerData.Player.gameObject.AddComponent<CharacterID>();
            playerData._charID.ID = i;

            // add instance to list
            Characters.Add(playerData);
        }

        private void SetKnowledgeLevel()
        {
            if (tutorial.TutorialMode)
            {
                for (int i = 0; i < Characters.Count; i++)
                {
                    if (Characters[i].Player.IsPlayer)
                        continue;

                    var randk = Random.Range(0, System.Enum.GetNames(typeof(AI.KnowledgeLevel)).Length);
                    var level = (AI.KnowledgeLevel)randk;

                    if (level == AI.KnowledgeLevel.Smart)
                        level = AI.KnowledgeLevel.Average;

                    Characters[i].Player.AI.SetKnowledgeLevel(level);
                }

                return;
            }

            var smartAdded = false;
            var averageAdded = false;

            for (int i = 0; i < Characters.Count; i++)
            {
                if (Characters[i].Player.IsPlayer)
                    continue;

                if (!smartAdded)
                {
                    if (Random.value <= i / (float)Characters.Count)
                    {
                        Characters[i].Player.AI.SetKnowledgeLevel(AI.KnowledgeLevel.Smart);
                        smartAdded = true;
                        continue;
                    }
                }

                if (!averageAdded)
                {
                    if (Random.value <= i / (float)(Characters.Count - 1))
                    {
                        Characters[i].Player.AI.SetKnowledgeLevel(AI.KnowledgeLevel.Average);
                        averageAdded = true;
                        continue;
                    }
                }

                Characters[i].Player.AI.SetKnowledgeLevel(AI.KnowledgeLevel.Idiot);
            }
        }

        private Bounds GameObjectBounds(GameObject GO)
        {
            var bound = new Bounds();

            foreach (var renderer in GO.GetComponentsInChildren<Renderer>())
            {
                bound.Encapsulate(renderer.bounds);
            }

            return bound;
        }

        public IEnumerator StartGameCoroutine()
        {
            _CountDownSound.Play();
            presentor.SetCountDownText(3.ToString());
            yield return new WaitForSeconds(1f);

            presentor.SetCountDownText(2.ToString());
            yield return new WaitForSeconds(1f);

            presentor.SetCountDownText(1.ToString());
            yield return new WaitForSeconds(1f);

            presentor.SetCountDownText("شروع");
            StartRound();
            yield return new WaitForSeconds(1f);

            presentor.SetCountDownText(string.Empty);
            _CountDownSound.Stop();
        }

        public void StartRound()
        {
            if (IsGameEnded() == true)
                return;

            if (Player.Dead)
                return;

            StartCoroutine(presentor.HideInstantResultsCoroutine(true));
            StartCoroutine(presentor.HideAnswersCoroutine(true));

            foreach (var character in Characters)
                character.Player.ResetForNewRound();

            Round++;
            presentor.SetRoundText(Round.ToString());

            OnGetReadyForNewRound?.Invoke();
        }

        public void ConfirmStartNewRound()
        {
            if (GameEnded)
                return;

            if (JSONFileReader)
                JSONFileReader.GenerateQuestion();

            presentor.ShowKeyboards(showKeyboardDelay);
            presentor.ShowBoosters(showKeyboardDelay + 0.1f);
            playerProperties.ShowPlayersData();
            playerProperties.UpdateInfo();
            IsGameStarted = true;
            IsRoundInProgress = true;
            timer.StartTimer();

            OnRoundStart?.Invoke();
        }

        private void EndRound()
        {
            IsGameStarted = false;
            IsRoundInProgress = false;
            presentor.ShowInstantResult(scoring.IsCorrectAnswer(Characters.FirstOrDefault(x => x.Player.IsPlayer)));
            presentor.ShowAnswers(Characters, IsNumericQuestion);
            presentor.HideQuestionBox();
            presentor.HideBoosters();
            presentor.HideKeyboards();
            presentor.HideBoosters();
            playerProperties.HidePlayersData(false);

            TUT_ShowResultsStep();
        }

        public void SubmitAnswerPlayer(string answer, bool isNumeric)
        {
            if (!isNumeric)
            {
                Player.AnswerTextual(answer);
            }
            else
            {
                Player.AnswerNumeric(System.Convert.ToSingle(answer));
            }

            presentor.HideBoosters();

            if (tutorial.TutorialMode)
                timer.StopTimer();
        }

        public void SubmitAnswerAI(string AIName, string answer, bool isNumeric)
        {
            if (!isNumeric)
            {
                Characters.SingleOrDefault(x => x.Player.Username == AIName).Player.AnswerTextual(answer);
            }
            else
            {
                Characters.SingleOrDefault(x => x.Player.Username == AIName).Player.AnswerNumeric(System.Convert.ToSingle(answer));
            }
        }

        public void HelpBooster(Character usedPlayer)
        {
            usedPlayer.Helped = true;
            //_presentor.HideBoosters();
        }

        public IEnumerator AddBlockCoroutine(CharacterInstance character, int blockCount)
        {
            for (int i = 0; i < blockCount; i++)
            {
                var block = Instantiate(blockPrefab, new Vector3(character.CurrentBlocks[0].transform.position.x, waterObject.transform.position.y + addBlockFromYDistance, character.CurrentBlocks[0].transform.position.z), Quaternion.identity, character.BlocksHolder.transform);

                if (character.opponentData != null)
                {
                    var setModel = new SetModel();
                    setModel.code = character.opponentData.platfrom.code;
                    block.GetComponent<BlockCustomization>().ApplyStyleNoCheck(setModel);
                }

                block.Rigidbody.isKinematic = true;
                character.CurrentBlocks.Insert(0, block);

                if (character.Player.IsPlayer)
                {
                    AudioManager.Instance.BlockRiseSFX();
                    block.GetComponent<BlockCustomization>().IsPlayer = true;
                    character.Player.ApplyPlayerBlock(block.GetComponent<BlockCustomization>());
                }

                if (character.CurrentBlocks.Count > 1)
                    character.Player.SetAnimationMode(Character.IDLE_ANIMATION);

                yield return new WaitUntil(() => block.transform.position.y >= CurrentWaterHeight + -0.8f);

                if (character.CurrentBlocks.Count > 1)
                    character.CurrentBlocks[1].Rigidbody.isKinematic = false;

                yield return new WaitForSeconds(0.3f);
            }
        }

        public void EncouragementPlayer(string playerName, int addBlock)
        {
            var player = Characters.SingleOrDefault(x => x.Player.Username == playerName);

            StartCoroutine(AddBlockCoroutine(player, addBlock));
            player.Player.SetAnimationMode(Character.HAPPY_ANIMATION);
        }

        public void PunishPlayer(string playerName, int blockToRemove, bool attackByShip)
        {
            var player = Characters.SingleOrDefault(x => x.Player.Username == playerName);

            if (player.Player.Dead)
                return;

            if (attackByShip)
            {
                if (player.ShieldLeft > 0)
                {
                    player.ShieldLeft--;
                    playerProperties.UpdateInfo();
                    return;
                }
            }

            for (int i = 0; i < blockToRemove; i++)
            {
                if (player.CurrentBlocks.Count > 0)
                {
                    if (player.CurrentBlocks.Count > 1)
                    {
                        player.CurrentBlocks[1].Rigidbody.isKinematic = true;
                    }

                    player.CurrentBlocks[0].Rigidbody.isKinematic = false;
                    player.CurrentBlocks[0].Rigidbody.drag = 0.05f;
                    player.CurrentBlocks.RemoveAt(0);

                    if (player.Player.IsPlayer)
                        AudioManager.Instance.BlockFallSFX();
                }
            }

            if (player.CurrentBlocks.Count == 1)
            {
                player.Player.SetAnimationMode(Character.STRESS_ANIMATION);
            }
            else if (player.CurrentBlocks.Count > 1)
            {
                player.Player.SetAnimationMode(Character.SAD_ANIMATION);
            }

            if (player.CurrentBlocks.Count == 0)
            {
                if (player.Player.IsPlayer)
                    AudioManager.Instance.FallingSFX();

                Die(player);
            }

            WakeAllRigibodies(player);

            if (CheckForNewRound)
                IsGameEnded();
        }

        private void WakeAllRigibodies(CharacterInstance character)
        {
            foreach (var block in character.CurrentBlocks)
                block.Rigidbody.WakeUp();

            character.Player.Rigidbody.WakeUp();
        }

        private void Die(CharacterInstance player)
        {
            player.Player.Dead = true;
            player.Player.HidePlayerInfo();
            player.Player.Rigidbody.isKinematic = false;
            player.Player.GetComponent<StickToGround>().enabled = false;
            emojiManager.ShowEmoji(player.Player.transform, Emoji.EmojiType.OnDie);

            attackingShark.AddTarget(player._charID, () =>
            {
                if (CheckForNewRound)
                {
                    CheckStatusForNewRound();
                    TUT_ShowSharkStep();
                }

                StartCoroutine(CreateSkullCoroutine(player));

                if (player.Player.IsPlayer)
                    LoseGame();
            });

            // shark attack
            if (player.Player.IsPlayer)
                playerIsGoingToBeEaten = true;
        }

        public void CheckStatusForNewRound()
        {
            if (Player.Dead)
                return;

            if (attackingShark.TargetsLeft == 0)
                Invoke(nameof(StartRound), newRoundDelay);
        }

        private void LoseGame()
        {
            StartCoroutine(LoseGameCoroutine());
        }

        public IEnumerator LoseGameCoroutine()
        {
            yield return new WaitForSeconds(1f);

            IsGameStarted = false;
            timer.EndGame();
            SaveGame();
        }

        private void SaveGame()
        {
            if (isGameSaved)
                return;

            isGameSaved = true;

            StartCoroutine(FinalGameCoroutine());

            if (!tutorial.TutorialMode)
                SaveManager.Set(UserComment.PLAYED_MATCHES_COUNT_PREFS, SaveManager.Get<int>(UserComment.PLAYED_MATCHES_COUNT_PREFS) + 1);
        }

        private IEnumerator FinalGameCoroutine()
        {
            presentor.HideAllUI();
            lightingManager.LoadPreset(lightPresentIndex);
            yield return new WaitForSeconds(1f);

            var characters = scoring.GetOrderedCharacters();

            for (int i = 0; i < characters.Count; i++)
            {
                var projector = Instantiate(lightProjectorPrefab, lightProjectorOffset, lightProjectorPrefab.transform.rotation, characters[i].Player.transform);
                projector.transform.localPosition = lightProjectorOffset;

                var number = Instantiate(playerRankPrefab, playerRankOffset, playerRankPrefab.transform.rotation, characters[i].Player.transform);
                number.transform.localPosition = playerRankOffset;
                number.text = (i + 1).ToString();

                yield return new WaitForSeconds(1f);
            }

            crownManager.ScaleUp();
            characters[0].Player.PlayHappyLoopAnimation();

            yield return new WaitForSeconds(2f);

            if (scoring.Position(Player.Username) > maximumPlayerCount - 1)
                AudioManager.Instance.LoseSFX();
            else
                AudioManager.Instance.EndGameSFX();

            matchReward.ShowMatchReward(scoring.Position(Player.Username));
        }

        public bool IsGameEnded()
        {
            if (IsRoundInProgress)
                return false;

            if (GameEnded == true)
                return true;

            var playersAlive = Characters.Where(x => x.Player.Dead == false).ToList();

            if (playersAlive.Count > 1 && Round < maxRoundCount)
                return false;

            timer.EndGame();
            IsGameStarted = false;

            GameEnded = true;

            // Submit as they all ended the game
            foreach (var character in Characters)
                character.Player.Dead = true;

            if (!playerIsGoingToBeEaten)
                SaveGame();

            return true;
        }

        private void CameraPosition()
        {
            if (!Player)
                return;

            var xPos = 0f;
            var charactersCount = 0;

            foreach (var character in Characters)
            {
                //if (character.player.Dead)
                //  continue;

                xPos += character.Player.transform.position.x;
                charactersCount++;
            }

            xPos /= charactersCount;

            var targetOffset = !GameEnded ? cameraOffset : resultsPhaseOffset;
            var cameraPosition = new Vector3(xPos + targetOffset.x, (Player.Dead ? 0 : Player.transform.position.y) + targetOffset.y, -targetOffset.z);

            camera.transform.position = Vector3.Lerp(camera.transform.position, cameraPosition, Time.deltaTime * 3f);
        }

        private void BlocksPosition()
        {
            foreach (var character in Characters)
            {
                if (character.CurrentBlocks.Count == 0)
                    continue;

                character.CurrentBlocks[0].transform.position = Vector3.MoveTowards(character.CurrentBlocks[0].transform.position, new Vector3(character.CurrentBlocks[0].transform.position.x, CurrentWaterHeight + firstBlockPositionHeight, character.CurrentBlocks[0].transform.position.z), Time.deltaTime * 2f);
            }
        }

        private void DrowningPlayers()
        {
            foreach (var player in Characters)
            {
                if (player.Player.Dead)
                    player.Player.Drowning(waterObject.transform.position.y);
            }
        }

        public IEnumerator CreateSkullCoroutine(CharacterInstance character)
        {
            yield return new WaitForSeconds(skullInstantiateDelay);

            var skull = Instantiate(skullPrefab, character.Pivot.transform.position + new Vector3(0, skullInstantiateHeight, 0), Quaternion.identity, null);
            character.Player.ChangePlayerInfoPivot(skull.transform);
        }

        private void DebugInputs()
        {
            // kill players
            if (Input.GetKeyDown(KeyCode.H))
            {
                for (int i = 0; i < Characters.Count; i++)
                {
                    if (!Characters[i].Player.Dead)
                    {
                        Characters[i].Player.Dead = true;
                        break;
                    }
                }

                IsGameEnded();
            }

            // punish player
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PunishPlayer(Player.Username, 1, false);
            }

            // punish other players
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                var alivePlayers = Characters.Where(x => x.Player.Dead == false && x.Player.IsPlayer == false).ToList();
                PunishPlayer(alivePlayers[Random.Range(0, alivePlayers.Count)].Player.Username, 1, false);
            }

            // add block
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                var alivePlayers = Characters.Where(x => x.Player.Dead == false).ToList();
                StartCoroutine(AddBlockCoroutine(alivePlayers[Random.Range(0, alivePlayers.Count)], 1));
            }

            // shark attack
            if (Input.GetKeyDown(KeyCode.S))
            {
                attackingShark.AddTarget(Characters[Random.Range(0, Characters.Count)]._charID, null);
            }

            // final panel
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                StartCoroutine(LoseGameCoroutine());
            }

            // End game
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Round = maxRoundCount;
                IsGameEnded();
            }
        }

        #region Tutorial Mode
        public void TUT_StartTrigger()
        {
            if (!tutorial.TutorialMode)
                return;

            if (tutorial.CurrentStep == 0)
                tutorial.StartTutorial();
        }

        public void TUT_ShowSharkStep()
        {
            if (!tutorial.TutorialMode)
                return;

            if (tutorial.CurrentStep == 7)
                tutorial.NextStep();
        }

        public void TUT_ShowResultsStep()
        {
            if (!tutorial.TutorialMode)
                return;

            if (tutorial.CurrentStep == 1)
                tutorial.NextStep();
        }

        public void TUT_ShowBoostersStep()
        {
            if (!tutorial.TutorialMode)
                return;

            if (tutorial.CurrentStep == 3 || tutorial.CurrentStep == 5)
                tutorial.NextStep();
        }
        #endregion
    }
}