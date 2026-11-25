using System;
using System.Collections;
using System.Collections.Generic;
using TrucoVR.Domain;
using TrucoVR.Presentation;
using UnityEngine;

namespace TrucoVR.Application
{
    [DisallowMultipleComponent]
    public sealed class RoundController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private CardFanController playerHand;
        [SerializeField] private SpanishCardDealer cardDealer;
        [SerializeField] private OpponentView opponentView;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private ActionPanelView actionPanel;
        [SerializeField] private RoundResultView roundResultView;

        [Header("Mazos")]
        [SerializeField] private GameObject playerDeckObject;
        [SerializeField] private GameObject opponentDeckObject;

        [Header("Configuración")]
        [SerializeField] private bool logRoundState = true;
        [SerializeField] private float nextRoundDelay = 2f;
        [SerializeField] private bool playerStartsFirstRound = true;

        private readonly List<SpanishCard> playerPlayedCards = new List<SpanishCard>(3);
        private readonly List<SpanishCard> opponentPlayedCards = new List<SpanishCard>(3);
        private readonly List<int> trickResults = new List<int>(3);
        private readonly List<string> envidoCallHistory = new List<string>(4);
        private readonly TrucoHand opponentHand = new TrucoHand();
        private SimpleOpponentAi opponentAi;
        private RoundPhase phase = RoundPhase.WaitingForPlayer;
        private int playerTricks;
        private int opponentTricks;
        private int tiedTricks;
        private TrucoBetLevel trucoBet = TrucoBetLevel.None;
        private bool waitingForTrucoResponse;
        private bool waitingForEnvidoResponse;
        private bool opponentInitiatedTruco;
        private bool opponentInitiatedEnvido;
        private bool envidoResolved;
        private EnvidoBetLevel envidoBet = EnvidoBetLevel.None;
        private int envidoPointsInPlay;
        private BetCaller betCaller;
        private bool playerLeadsCurrentTrick;
        private Coroutine nextRoundRoutine;

        public RoundPhase Phase => phase;
        public IReadOnlyList<SpanishCard> PlayerPlayedCards => playerPlayedCards;
        public IReadOnlyList<SpanishCard> OpponentPlayedCards => opponentPlayedCards;
        public int PlayerTricks => playerTricks;
        public int OpponentTricks => opponentTricks;
        public int TiedTricks => tiedTricks;

        private void Awake()
        {
            if (playerHand == null)
                playerHand = FindFirstObjectByType<CardFanController>();

            if (cardDealer == null)
                cardDealer = FindFirstObjectByType<SpanishCardDealer>();

            if (opponentView == null)
                opponentView = FindFirstObjectByType<OpponentView>();

            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<ScoreManager>();

            if (actionPanel == null)
                actionPanel = FindFirstObjectByType<ActionPanelView>();

            if (roundResultView == null)
                roundResultView = FindFirstObjectByType<RoundResultView>();

            opponentAi = new SimpleOpponentAi();
            playerLeadsCurrentTrick = playerStartsFirstRound;
        }

        private void OnEnable()
        {
            if (playerHand != null)
                playerHand.CardPlayed += OnPlayerCardPlayed;

            if (actionPanel != null)
            {
                actionPanel.TrucoRequested += OnTrucoRequested;
                actionPanel.RetrucoRequested += OnRetrucoRequested;
                actionPanel.ValeCuatroRequested += OnValeCuatroRequested;
                actionPanel.RealEnvidoRequested += OnRealEnvidoRequested;
                actionPanel.FaltaEnvidoRequested += OnFaltaEnvidoRequested;
                actionPanel.QuieroRequested += OnQuieroRequested;
                actionPanel.NoQuieroRequested += OnNoQuieroRequested;
                actionPanel.EnvidoRequested += OnEnvidoRequested;
            }
        }

        private void OnDisable()
        {
            if (nextRoundRoutine != null)
            {
                StopCoroutine(nextRoundRoutine);
                nextRoundRoutine = null;
            }

            if (playerHand != null)
                playerHand.CardPlayed -= OnPlayerCardPlayed;

            if (actionPanel != null)
            {
                actionPanel.TrucoRequested -= OnTrucoRequested;
                actionPanel.RetrucoRequested -= OnRetrucoRequested;
                actionPanel.ValeCuatroRequested -= OnValeCuatroRequested;
                actionPanel.RealEnvidoRequested -= OnRealEnvidoRequested;
                actionPanel.FaltaEnvidoRequested -= OnFaltaEnvidoRequested;
                actionPanel.QuieroRequested -= OnQuieroRequested;
                actionPanel.NoQuieroRequested -= OnNoQuieroRequested;
                actionPanel.EnvidoRequested -= OnEnvidoRequested;
            }
        }

        private void Start()
        {
            if (playerHand == null)
            {
                Debug.LogError("RoundController necesita una referencia a CardFanController.", this);
                enabled = false;
                return;
            }

            if (cardDealer == null)
            {
                Debug.LogError("RoundController necesita una referencia a SpanishCardDealer.", this);
                enabled = false;
                return;
            }

            if (playerDeckObject == null || opponentDeckObject == null)
                Debug.LogWarning("RoundController necesita referencias a Mazo y Mazo Oponente para alternar su visibilidad.", this);

            BeginRound();
        }

        private void BeginRound()
        {
            if (playerHand != null)
                playerHand.ResetPlayedCardsToHand();

            if (opponentView != null)
                opponentView.ShowBacks();

            cardDealer.DealThreeCards();
            opponentHand.Clear();
            opponentPlayedCards.Clear();
            playerPlayedCards.Clear();
            trickResults.Clear();
            playerTricks = 0;
            opponentTricks = 0;
            tiedTricks = 0;
            trucoBet = TrucoBetLevel.None;
            waitingForTrucoResponse = false;
            waitingForEnvidoResponse = false;
            opponentInitiatedTruco = false;
            opponentInitiatedEnvido = false;
            envidoResolved = false;
            envidoBet = EnvidoBetLevel.None;
            envidoPointsInPlay = 0;
            betCaller = BetCaller.None;
            envidoCallHistory.Clear();
            playerHand.SetCardPlayEnabled(true);

            foreach (SpanishCard card in cardDealer.DealOpponentCards())
                opponentHand.Add(card);

            phase = RoundPhase.WaitingForPlayer;
            UpdateDeckVisibility();
            playerHand.SetCardPlayEnabled(playerLeadsCurrentTrick);
            if (actionPanel != null)
            {
                actionPanel.SetActionsAvailable(true, playerLeadsCurrentTrick);
                UpdateRaiseActions();
                actionPanel.SetTrucoResponsesAvailable(false);
            }

            Log($"Ronda iniciada. Mano: {(playerLeadsCurrentTrick ? "jugador" : "IA")}. Jugador: {cardDealer.PlayerCards.Count} cartas. IA: {opponentHand.Count} cartas.");

            if (!playerLeadsCurrentTrick)
            {
                if (!TryStartOpponentEnvido())
                    PlayOpponentCard();
            }
        }

        private void UpdateDeckVisibility()
        {
            if (playerDeckObject == null || opponentDeckObject == null)
                return;

            playerDeckObject.SetActive(playerLeadsCurrentTrick);
            opponentDeckObject.SetActive(!playerLeadsCurrentTrick);
        }

        private void OnTrucoRequested()
        {
            if (phase != RoundPhase.WaitingForPlayer || waitingForTrucoResponse || waitingForEnvidoResponse || trucoBet != TrucoBetLevel.None)
                return;

            trucoBet = TrucoBet.Raise(trucoBet);
            betCaller = BetCaller.Player;
            waitingForTrucoResponse = true;
            Log("El jugador pidió Truco. En juego: 2 puntos.");
            ResolveOpponentTrucoResponse();
        }

        private void OnRetrucoRequested()
        {
            if (phase != RoundPhase.WaitingForPlayer || betCaller != BetCaller.Opponent || trucoBet != TrucoBetLevel.Truco)
                return;

            trucoBet = TrucoBet.Raise(trucoBet);
            betCaller = BetCaller.Player;
            waitingForTrucoResponse = true;
            Log("El jugador pidió Retruco. En juego: 3 puntos.");
            ResolveOpponentTrucoResponse();
        }

        private void OnValeCuatroRequested()
        {
            if (phase != RoundPhase.WaitingForPlayer || betCaller != BetCaller.Opponent || trucoBet != TrucoBetLevel.Retruco)
                return;

            trucoBet = TrucoBet.Raise(trucoBet);
            betCaller = BetCaller.Player;
            waitingForTrucoResponse = true;
            Log("El jugador pidió Vale Cuatro. En juego: 4 puntos.");
            ResolveOpponentTrucoResponse();
        }

        private void ResolveOpponentTrucoResponse()
        {
            TrucoResponse response = opponentAi.RespondToTruco(trucoBet, opponentHand);
            bool respondingToOpponentCall = opponentInitiatedTruco;

            if (response == TrucoResponse.Accepted)
            {
                waitingForTrucoResponse = false;
                opponentInitiatedTruco = false;
                if (respondingToOpponentCall)
                {
                    actionPanel?.SetTrucoResponsesAvailable(false);
                }

                UpdateRaiseActions();
                Log($"La IA aceptó. El Truco vale {TrucoBet.GetPointsInPlay(trucoBet)} puntos.");
                ResumeTurnAfterBet();
                return;
            }

            waitingForTrucoResponse = false;
            opponentInitiatedTruco = false;
            phase = RoundPhase.RoundFinished;
            playerHand.SetCardPlayEnabled(false);

            int rejectedPoints = TrucoBet.GetPointsForRejection(trucoBet);
            Log($"La IA rechazó. El jugador gana {rejectedPoints} punto(s).");

            if (scoreManager != null)
                scoreManager.AddPoints(true, rejectedPoints);

            ScheduleNextRound();

            if (actionPanel != null)
            {
                actionPanel.SetActionsAvailable(false, false);
                actionPanel.SetRaiseActionsAvailable(false, false, false);
                actionPanel.SetTrucoResponsesAvailable(false);
            }
        }

        private void OnQuieroRequested()
        {
            if (waitingForEnvidoResponse && opponentInitiatedEnvido)
            {
                AcceptOpponentEnvido();
                return;
            }

            if (!waitingForTrucoResponse || !opponentInitiatedTruco)
                return;

            waitingForTrucoResponse = false;
            opponentInitiatedTruco = false;
            actionPanel?.SetTrucoResponsesAvailable(false);
            UpdateRaiseActions();
            Log($"Quiero aceptado. El Truco de la IA vale {TrucoBet.GetPointsInPlay(trucoBet)} puntos.");
            ResumeTurnAfterBet();
        }

        private void OnNoQuieroRequested()
        {
            if (waitingForEnvidoResponse && opponentInitiatedEnvido)
            {
                RejectOpponentEnvido();
                return;
            }

            if (!waitingForTrucoResponse || !opponentInitiatedTruco)
                return;

            int rejectedPoints = TrucoBet.GetPointsForRejection(trucoBet);
            waitingForTrucoResponse = false;
            opponentInitiatedTruco = false;
            phase = RoundPhase.RoundFinished;
            playerHand.SetCardPlayEnabled(false);
            if (actionPanel != null)
            {
                actionPanel.SetActionsAvailable(false, false);
                actionPanel.SetRaiseActionsAvailable(false, false, false);
                actionPanel.SetTrucoResponsesAvailable(false);
            }

            Log($"No Quiero aceptado. La IA gana {rejectedPoints} punto(s).");

            if (scoreManager != null)
                scoreManager.AddPoints(false, rejectedPoints);

            ScheduleNextRound();
        }

        private void OnEnvidoRequested()
        {
            if (phase != RoundPhase.WaitingForPlayer ||
                waitingForTrucoResponse ||
                waitingForEnvidoResponse ||
                envidoResolved ||
                !playerLeadsCurrentTrick ||
                playerPlayedCards.Count > 0 ||
                opponentPlayedCards.Count > 0 ||
                trucoBet != TrucoBetLevel.None ||
                !EnvidoBet.CanRaiseTo(envidoBet, EnvidoBetLevel.Envido))
                return;

            StartEnvidoCall(EnvidoBetLevel.Envido, BetCaller.Player);
            if (actionPanel != null)
                actionPanel.SetEnvidoAvailable(false);

            ResolvePlayerEnvidoCall();
        }

        private void OnRealEnvidoRequested()
        {
            RaiseOpponentEnvidoTo(EnvidoBetLevel.RealEnvido);
        }

        private void OnFaltaEnvidoRequested()
        {
            RaiseOpponentEnvidoTo(EnvidoBetLevel.FaltaEnvido);
        }

        private void ResolvePlayerEnvidoCall()
        {
            GetEnvidoScores(out int playerEnvido, out int opponentEnvido);
            TrucoResponse response = opponentAi.RespondToEnvido(playerEnvido, opponentEnvido);
            envidoCallHistory.Add($"La IA dijo {(response == TrucoResponse.Accepted ? "Quiero" : "No Quiero")}.");

            if (response == TrucoResponse.Rejected)
            {
                int rejectedPoints = EnvidoBet.GetPointsForRejection(0);
                CompleteEnvido(true, rejectedPoints);
                return;
            }

            envidoCallHistory.Add($"Tantos revelados: jugador {playerEnvido}, IA {opponentEnvido}.");
            CompleteEnvido(playerEnvido >= opponentEnvido, GetEnvidoPointsInPlay());
        }

        private bool TryStartOpponentEnvido()
        {
            if (phase != RoundPhase.WaitingForPlayer ||
                waitingForTrucoResponse ||
                waitingForEnvidoResponse ||
                envidoResolved ||
                opponentHand.Count != 3 ||
                opponentPlayedCards.Count > 0 ||
                playerPlayedCards.Count > 1 ||
                trucoBet != TrucoBetLevel.None)
                return false;

            GetEnvidoScores(out int playerEnvido, out int opponentEnvido);
            EnvidoBetLevel call = opponentAi.ChooseEnvidoCall(
                opponentEnvido,
                PlayerPoints,
                OpponentPoints,
                TargetPoints);

            if (call == EnvidoBetLevel.None || !EnvidoBet.CanRaiseTo(envidoBet, call))
                return false;

            StartEnvidoCall(call, BetCaller.Opponent);
            waitingForEnvidoResponse = true;
            opponentInitiatedEnvido = true;
            playerHand.SetCardPlayEnabled(false);

            if (actionPanel != null)
            {
                actionPanel.SetActionsAvailable(false, false);
                actionPanel.SetRaiseActionsAvailable(false, false, false);
                actionPanel.SetEnvidoResponsesAvailable(
                    true,
                    EnvidoBet.CanRaiseTo(envidoBet, EnvidoBetLevel.RealEnvido),
                    EnvidoBet.CanRaiseTo(envidoBet, EnvidoBetLevel.FaltaEnvido));
            }

            Log($"{envidoCallHistory[envidoCallHistory.Count - 1]} Elige Quiero, No Quiero o subir.");
            return true;
        }

        private void AcceptOpponentEnvido()
        {
            GetEnvidoScores(out int playerEnvido, out int opponentEnvido);
            envidoCallHistory.Add("El jugador dijo Quiero.");
            envidoCallHistory.Add($"Tantos revelados: jugador {playerEnvido}, IA {opponentEnvido}.");
            CompleteEnvido(playerEnvido >= opponentEnvido, GetEnvidoPointsInPlay());
        }

        private void RejectOpponentEnvido()
        {
            int rejectedPoints = EnvidoBet.GetPointsForRejection(0);
            envidoCallHistory.Add("El jugador dijo No Quiero.");
            CompleteEnvido(false, rejectedPoints);
        }

        private void RaiseOpponentEnvidoTo(EnvidoBetLevel nextLevel)
        {
            if (!waitingForEnvidoResponse ||
                !opponentInitiatedEnvido ||
                !EnvidoBet.CanRaiseTo(envidoBet, nextLevel))
                return;

            EnvidoBetLevel previousLevel = envidoBet;
            int previousPointsInPlay = envidoPointsInPlay;
            envidoBet = nextLevel;
            envidoPointsInPlay = EnvidoBet.GetPointsInPlay(
                nextLevel,
                previousLevel,
                TargetPoints,
                PlayerPoints,
                OpponentPoints);
            betCaller = BetCaller.Player;
            opponentInitiatedEnvido = false;
            GetEnvidoScores(out int playerEnvido, out int opponentEnvido);
            envidoCallHistory.Add($"El jugador subió a {GetEnvidoName(nextLevel)}.");

            TrucoResponse response = opponentAi.RespondToEnvido(playerEnvido, opponentEnvido);
            envidoCallHistory.Add($"La IA dijo {(response == TrucoResponse.Accepted ? "Quiero" : "No Quiero")}.");

            if (response == TrucoResponse.Rejected)
            {
                int rejectedPoints = EnvidoBet.GetPointsForRejection(previousPointsInPlay);
                CompleteEnvido(true, rejectedPoints);
                return;
            }

            envidoCallHistory.Add($"Tantos revelados: jugador {playerEnvido}, IA {opponentEnvido}.");
            CompleteEnvido(playerEnvido >= opponentEnvido, GetEnvidoPointsInPlay());
        }

        private void StartEnvidoCall(EnvidoBetLevel level, BetCaller caller)
        {
            envidoBet = level;
            envidoPointsInPlay = EnvidoBet.GetPointsInPlay(
                level,
                EnvidoBetLevel.None,
                TargetPoints,
                PlayerPoints,
                OpponentPoints);
            betCaller = caller;
            envidoResolved = true;
            envidoCallHistory.Add($"{(caller == BetCaller.Player ? "El jugador" : "La IA")} cantó {GetEnvidoName(level)}.");
        }

        private void CompleteEnvido(bool playerWon, int points)
        {
            string winner = playerWon ? "el jugador" : "la IA";
            envidoCallHistory.Add($"Gana {winner} por {points} punto(s).");

            waitingForEnvidoResponse = false;
            opponentInitiatedEnvido = false;
            envidoBet = EnvidoBetLevel.None;
            envidoPointsInPlay = 0;
            betCaller = BetCaller.None;

            actionPanel?.SetEnvidoResponsesAvailable(false, false, false);
            UpdateRaiseActions();

            if (scoreManager != null)
                scoreManager.AddPoints(playerWon, points);

            Log(string.Join("\n", envidoCallHistory));
            ResumeTurnAfterEnvido();
        }

        private void ResumeTurnAfterEnvido()
        {
            if (opponentPlayedCards.Count < playerPlayedCards.Count)
            {
                PlayOpponentCard();
                ResolveCurrentTrick();
                return;
            }

            ResumeTurnAfterBet();
        }

        private int GetEnvidoPointsInPlay()
        {
            return envidoPointsInPlay;
        }

        private void GetEnvidoScores(out int playerEnvido, out int opponentEnvido)
        {
            playerEnvido = TrucoRules.CalculateEnvido(
                cardDealer.PlayerCards[0],
                cardDealer.PlayerCards[1],
                cardDealer.PlayerCards[2]);
            opponentEnvido = TrucoRules.CalculateEnvido(
                opponentHand.Cards[0],
                opponentHand.Cards[1],
                opponentHand.Cards[2]);
        }

        private int TargetPoints => scoreManager == null ? 30 : scoreManager.TargetPoints;
        private int PlayerPoints => scoreManager == null ? 0 : scoreManager.PlayerPoints;
        private int OpponentPoints => scoreManager == null ? 0 : scoreManager.OpponentPoints;

        private static string GetEnvidoName(EnvidoBetLevel level)
        {
            return level switch
            {
                EnvidoBetLevel.Envido => "Envido",
                EnvidoBetLevel.RealEnvido => "Real Envido",
                EnvidoBetLevel.FaltaEnvido => "Falta Envido",
                _ => throw new ArgumentOutOfRangeException(nameof(level))
            };
        }

        private void OnPlayerCardPlayed(CardView cardView)
        {
            if (phase != RoundPhase.WaitingForPlayer || waitingForTrucoResponse || waitingForEnvidoResponse)
                return;

            playerPlayedCards.Add(cardView.Card);
            Log($"Jugador jugó {cardView.Card}. Cartas jugadas: {playerPlayedCards.Count}/3.");

            playerHand.SetCardPlayEnabled(false);

            if (playerLeadsCurrentTrick)
            {
                if (!TryStartOpponentEnvido())
                    PlayOpponentCard();
            }

            ResolveCurrentTrick();
        }

        private void PlayOpponentCard()
        {
            if (opponentHand.Count == 0)
                return;

            SpanishCard opponentCard = opponentAi.ChooseCard(opponentHand);
            opponentPlayedCards.Add(opponentCard);
            opponentView?.ShowCard(opponentCard);
            actionPanel?.SetEnvidoAvailable(false);
            Log($"IA jugó {opponentCard}. Esperando la carta del jugador.");

            if (!playerLeadsCurrentTrick)
                playerHand.SetCardPlayEnabled(true);
        }

        private void ResolveCurrentTrick()
        {
            if (playerPlayedCards.Count != opponentPlayedCards.Count)
                return;

            int cardIndex = playerPlayedCards.Count - 1;
            int comparison = TrucoRules.CompareCards(playerPlayedCards[cardIndex], opponentPlayedCards[cardIndex]);

            int trickResult = Math.Sign(comparison);
            trickResults.Add(trickResult);

            if (trickResult != 0)
                playerLeadsCurrentTrick = trickResult > 0;

            if (trickResult > 0)
                playerTricks++;
            else if (trickResult < 0)
                opponentTricks++;
            else
                tiedTricks++;

            string result = trickResult > 0 ? "gana el jugador" : trickResult < 0 ? "gana la IA" : "empate";
            Log($"Baza {trickResults.Count}: {result}. Manos: jugador {playerTricks}, IA {opponentTricks}, empates {tiedTricks}.");

            if (roundResultView != null)
                roundResultView.ShowHandResult(trickResult > 0 ? "Ganaste la mano" : trickResult < 0 ? "La IA gana la mano" : "Mano empatada");

            int? roundWinner = ResolveWinnerFromTricks();
            if (roundWinner.HasValue)
            {
                phase = RoundPhase.RoundFinished;
                playerHand.SetCardPlayEnabled(false);
                ResolveRound(roundWinner.Value);
                return;
            }

            if (trickResults.Count == 1 && trucoBet == TrucoBetLevel.None)
            {
                StartOpponentTruco();
                return;
            }

            if (playerLeadsCurrentTrick)
                playerHand.SetCardPlayEnabled(true);
            else
                PlayOpponentCard();
        }

        private void ResumeTurnAfterBet()
        {
            if (phase != RoundPhase.WaitingForPlayer)
                return;

            bool opponentCardIsWaitingForPlayer = opponentPlayedCards.Count > playerPlayedCards.Count;

            if (playerLeadsCurrentTrick || opponentCardIsWaitingForPlayer)
                playerHand.SetCardPlayEnabled(true);
            else
                PlayOpponentCard();
        }

        private int? ResolveWinnerFromTricks()
        {
            if (trickResults.Count < 2)
                return null;

            int first = trickResults[0];
            int second = trickResults[1];

            if (first == 0)
            {
                if (second != 0)
                    return second;

                if (trickResults.Count < 3)
                    return null;

                int thirdAfterTwoPardas = trickResults[2];
                if (thirdAfterTwoPardas != 0)
                    return thirdAfterTwoPardas;

                return playerLeadsCurrentTrick ? 1 : -1;
            }

            if (second == 0 || second == first)
                return first;

            if (trickResults.Count < 3)
                return null;

            int third = trickResults[2];
            return third == 0 ? first : third;
        }

        private void ResolveRound(int winnerValue)
        {
            string winner = winnerValue > 0 ? "gana el jugador" : "gana la IA";

            Log($"Ronda completa: {winner}. Resultado final {playerTricks}-{opponentTricks}.");

            if (roundResultView != null)
                roundResultView.ShowRoundResult(winnerValue > 0 ? "Ganaste la ronda" : "La IA gana la ronda");

            if (scoreManager != null)
                scoreManager.AddPoints(winnerValue > 0, TrucoBet.GetPointsInPlay(trucoBet));

            ScheduleNextRound();
        }

        private void ScheduleNextRound()
        {
            if (scoreManager != null && scoreManager.IsGameOver)
                return;

            if (nextRoundRoutine == null)
                nextRoundRoutine = StartCoroutine(BeginNextRound());
        }

        private IEnumerator BeginNextRound()
        {
            yield return new WaitForSeconds(nextRoundDelay);
            nextRoundRoutine = null;
            BeginRound();
        }

        private void StartOpponentTruco()
        {
            trucoBet = TrucoBetLevel.Truco;
            betCaller = BetCaller.Opponent;
            waitingForTrucoResponse = true;
            opponentInitiatedTruco = true;
            playerHand.SetCardPlayEnabled(false);

            if (actionPanel != null)
            {
                UpdateRaiseActions();
                actionPanel.SetActionsAvailable(false, false);
                actionPanel.SetTrucoResponsesAvailable(true);
            }

            Log("La IA pidió Truco. El jugador debe elegir Quiero o No Quiero.");
        }

        private void UpdateRaiseActions()
        {
            if (actionPanel == null)
                return;

            actionPanel.SetRaiseActionsAvailable(
                trucoBet == TrucoBetLevel.None,
                betCaller == BetCaller.Opponent && trucoBet == TrucoBetLevel.Truco,
                betCaller == BetCaller.Opponent && trucoBet == TrucoBetLevel.Retruco);
        }

        private void Log(string message)
        {
            if (roundResultView != null)
                roundResultView.ShowStatus(message);

            if (logRoundState)
                Debug.Log($"[TrucoVR] {message}", this);
        }
    }

    public enum RoundPhase
    {
        WaitingForPlayer,
        RoundFinished
    }

    public enum BetCaller
    {
        None,
        Player,
        Opponent
    }
}
