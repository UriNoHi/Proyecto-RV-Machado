using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrucoVR.Presentation
{
    public sealed class CardFanController : MonoBehaviour
    {
        [Header("Mesa")]
        [SerializeField] private Transform tableTarget;
        [SerializeField] private Transform[] tableCardTargets = new Transform[3];

        [Header("Distribución del abanico")]
        [SerializeField] private float spacing = 0.055f;
        [SerializeField] private float fanAngle = 8f;
        [SerializeField] private float outerDepth = 0.005f;

        private readonly List<CardView> cards = new List<CardView>(3);
        private int playedCardCount;
        private bool cardPlayEnabled = true;

        public event Action<CardView> CardPlayed;

        private void Awake()
        {
            CollectCards();
            RefreshLayout();
        }

        private void OnValidate()
        {
            if (UnityEngine.Application.isPlaying)
                RefreshLayout();
        }

        private void CollectCards()
        {
            cards.Clear();

            foreach (Transform child in transform)
            {
                CardView card = child.GetComponent<CardView>();
                if (card == null)
                    card = child.gameObject.AddComponent<CardView>();

                card.Configure(this, tableTarget);

                if (card.IsInHand)
                    cards.Add(card);
            }
        }

        public bool TryPlayCard(CardView card)
        {
            if (!cardPlayEnabled)
                return false;

            if (tableTarget == null)
            {
                Debug.LogWarning("CardFanController: asigná TableTarget en el Inspector.", this);
                return false;
            }

            if (!cards.Contains(card) || !card.IsInHand)
                return false;

            cards.Remove(card);
            card.MoveToTable(GetTableTarget(playedCardCount));
            playedCardCount++;
            CardPlayed?.Invoke(card);
            RefreshLayout();
            return true;
        }

        public void SetCardPlayEnabled(bool enabled)
        {
            cardPlayEnabled = enabled;
        }

        public void RefreshLayout()
        {
            if (cards.Count == 0)
                return;

            for (int i = 0; i < cards.Count; i++)
            {
                float normalizedIndex;

                if (cards.Count == 1)
                    normalizedIndex = 0f;
                else
                    normalizedIndex = Mathf.Lerp(-1f, 1f, i / (float)(cards.Count - 1));

                Vector3 localPosition = new Vector3(
                    normalizedIndex * spacing,
                    -normalizedIndex * normalizedIndex * 0.008f,
                    normalizedIndex * normalizedIndex * outerDepth);

                Quaternion localRotation = Quaternion.Euler(
                    0f,
                    normalizedIndex * fanAngle * 0.35f,
                    -normalizedIndex * fanAngle);
                cards[i].Configure(this, tableTarget);
                cards[i].SetFanPose(localPosition, localRotation);
            }
        }

        public void ResetPlayedCardsToHand()
        {
            List<CardView> playedCards = new List<CardView>();
            List<Transform> targets = new List<Transform>();

            if (tableTarget != null)
                targets.Add(tableTarget);

            if (tableCardTargets != null)
            {
                foreach (Transform target in tableCardTargets)
                {
                    if (target != null && !targets.Contains(target))
                        targets.Add(target);
                }
            }

            foreach (Transform target in targets)
            {
                foreach (CardView card in target.GetComponentsInChildren<CardView>(true))
                {
                    if (!playedCards.Contains(card))
                        playedCards.Add(card);
                }
            }

            foreach (CardView card in playedCards)
                card.ResetToHand(transform, this, tableTarget);

            CollectCards();
            cardPlayEnabled = true;
            playedCardCount = 0;
            RefreshLayout();
        }

        private Transform GetTableTarget(int index)
        {
            if (tableCardTargets != null && index < tableCardTargets.Length && tableCardTargets[index] != null)
                return tableCardTargets[index];

            return tableTarget;
        }

#if UNITY_EDITOR
        [ContextMenu("Reubicar cartas")]
        private void RepositionCardsInEditor()
        {
            CollectCards();
            RefreshLayout();
        }
#endif
    }
}
