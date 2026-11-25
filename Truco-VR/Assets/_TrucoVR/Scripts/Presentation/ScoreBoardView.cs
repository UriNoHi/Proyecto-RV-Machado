using TMPro;
using TrucoVR.Application;
using UnityEngine;

namespace TrucoVR.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ScoreBoardView : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private TMP_Text playerScoreText;
        [SerializeField] private TMP_Text opponentScoreText;
        private bool isSubscribed;

        private void Awake()
        {
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<ScoreManager>();
        }

        private void OnEnable()
        {
            if (scoreManager != null)
            {
                scoreManager.ScoreChanged += Refresh;
                isSubscribed = true;
            }

            RefreshCurrentScore();
        }

        private void Start()
        {
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<ScoreManager>();

            if (scoreManager != null)
            {
                if (!isSubscribed)
                    scoreManager.ScoreChanged += Refresh;

                isSubscribed = true;
                RefreshCurrentScore();
            }
        }

        private void OnDisable()
        {
            if (scoreManager != null && isSubscribed)
            {
                scoreManager.ScoreChanged -= Refresh;
                isSubscribed = false;
            }
        }

        private void Update()
        {
            RefreshCurrentScore();
        }

        private void RefreshCurrentScore()
        {
            if (scoreManager != null)
                Refresh(scoreManager.PlayerPoints, scoreManager.OpponentPoints);
        }

        private void Refresh(int playerPoints, int opponentPoints)
        {
            if (playerScoreText != null)
                playerScoreText.text = playerPoints.ToString();

            if (opponentScoreText != null)
                opponentScoreText.text = opponentPoints.ToString();
        }
    }
}
