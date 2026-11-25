using System;
using TrucoVR.Domain;
using UnityEngine;

namespace TrucoVR.Application
{
    [DisallowMultipleComponent]
    public sealed class ScoreManager : MonoBehaviour
    {
        [SerializeField] private int targetPoints = 30;
        [SerializeField] private bool logScore = true;

        private GameScore score;

        public event Action<int, int> ScoreChanged;

        public int PlayerPoints => score == null ? 0 : score.PlayerPoints;
        public int OpponentPoints => score == null ? 0 : score.OpponentPoints;
        public int TargetPoints => targetPoints;
        public bool IsGameOver => score != null && score.IsGameOver;

        private void Awake()
        {
            score = new GameScore(targetPoints);
            ScoreChanged?.Invoke(0, 0);
        }

        public void AddRoundPoint(bool playerWon)
        {
            AddPoints(playerWon, 1);
        }

        public void AddPoints(bool playerWon, int points)
        {
            if (IsGameOver)
                return;

            if (playerWon)
                score.AddPlayerPoints(points);
            else
                score.AddOpponentPoints(points);

            ScoreChanged?.Invoke(PlayerPoints, OpponentPoints);

            if (logScore)
                Debug.Log($"[TrucoVR] Marcador: jugador {PlayerPoints} - IA {OpponentPoints}.", this);

            if (score.IsGameOver)
            {
                string winner = PlayerPoints >= score.TargetPoints ? "jugador" : "IA";
                Debug.Log($"[TrucoVR] Partida terminada. Gana {winner}.", this);
            }
        }

        public void ResetMatch()
        {
            score.Reset();
            ScoreChanged?.Invoke(0, 0);
            Debug.Log("[TrucoVR] Marcador reiniciado: jugador 0 - IA 0.", this);
        }
    }
}
