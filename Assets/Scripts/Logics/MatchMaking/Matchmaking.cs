using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DeathMatch
{
    public class Matchmaking : MonoBehaviour
    {
        public enum MatchmakingState { Joining, WaitingForPlayers, StartingGame }

        public int MaxPlayers = 4;
        public Vector2 AIJoinDelayRange = new Vector2(1, 3);

        private MatchmakingState currentState;
        private int currentPlayersCount;
        private float elapsedTime;

        private MatchmakingPresentor presentor;
        private ServerConnection serverConnection;
        private LoadQuestions loadQuestions;
        private EventManager eventManager;
        private TutorialSteps tutorialSteps;

        public event Action OnStartGame;

        private void Start()
        {
            // Get necessary components
            loadQuestions = FindObjectOfType<LoadQuestions>();
            presentor = FindObjectOfType<MatchmakingPresentor>();
            serverConnection = ServerConnection.Instance;
            eventManager = FindObjectOfType<EventManager>();
            tutorialSteps = FindObjectOfType<TutorialSteps>();

            // Set current state to Joining player
            currentState = MatchmakingState.Joining;

            if (tutorialSteps.TutorialMode)
            {
                presentor.DisableUI();
                return;
            }
            else
            {
                // Request server to get player data
                serverConnection.GetPlayerInfo(OnGetUserInfo);
            }
        }

        private void Update()
        {
            // Show elapsed time in UI
            elapsedTime += Time.deltaTime;
            presentor.SetElapsedTimeText((int)elapsedTime);

            // Show joined players count
            presentor.SetJoinedPlayersCountText(currentPlayersCount, MaxPlayers);

            // Show matchmaking current state
            presentor.SetStateText(currentState);
        }

        private void OnGetUserInfo(USDM.UserInfo playerData)
        {
            // Request server to get match making data
            serverConnection.GetMatchMaking(OnGetMatchMaking);
        }

        private void OnGetMatchMaking(List<GSDM.MatchMakingUser> users)
        {
            // Add other players after getting matchmaking data from server
            StartCoroutine(JoinAICoroutine(users));
        }

        private IEnumerator JoinAICoroutine(List<GSDM.MatchMakingUser> users)
        {
            // Change current state to waiting for other players
            currentState = MatchmakingState.WaitingForPlayers;

            var count = 1;

            foreach (var user in users)
            {
                if (count >= MaxPlayers)
                    continue;

                count++;
                DataManager.PushOpponent(user);
            }

            currentPlayersCount++;
            eventManager.AddPlayer();

            yield return new WaitForSeconds(1f);

            foreach (var user in users)
            {
                if (currentPlayersCount >= MaxPlayers)
                    continue;

                // Add user to UI
                currentPlayersCount++;
                eventManager.AddOpponent();

                // This yield simulates real player joining random time
                yield return new WaitForSeconds(Random.Range(AIJoinDelayRange.x, AIJoinDelayRange.y));
            }

            // Change current state to starting game and wait for 2s
            currentState = MatchmakingState.StartingGame;

            eventManager.MoveToFinalCharactersSpace();
            eventManager.ZoomOutCamera();

            yield return new WaitForSeconds(2f);
            yield return new WaitWhile(() => !loadQuestions.IsLoaded);

            presentor.DisableUI();

            // Start game
            OnStartGame?.Invoke();
        }
    }
}