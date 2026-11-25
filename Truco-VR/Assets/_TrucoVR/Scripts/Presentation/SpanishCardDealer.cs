using System.Collections.Generic;
using TrucoVR.Domain;
using UnityEngine;

namespace TrucoVR.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CardFanController))]
    public sealed class SpanishCardDealer : MonoBehaviour
    {
        [SerializeField] private bool shuffleAtStart = true;

        private readonly List<CardView> dealtViews = new List<CardView>(3);
        private readonly List<SpanishCard> playerCards = new List<SpanishCard>(3);
        private SpanishDeck deck;

        public IReadOnlyList<SpanishCard> PlayerCards => playerCards;
        public int RemainingCards => deck == null ? 0 : deck.RemainingCount;

        [ContextMenu("Repartir tres cartas")]
        public void DealThreeCards()
        {
            deck = new SpanishDeck();

            if (shuffleAtStart)
                deck.Shuffle();

            playerCards.Clear();

            dealtViews.Clear();

            foreach (Transform child in transform)
            {
                CardView cardView = child.GetComponent<CardView>();
                if (cardView != null)
                    dealtViews.Add(cardView);

                if (dealtViews.Count == 3)
                    break;
            }

            if (dealtViews.Count != 3)
            {
                Debug.LogWarning("SpanishCardDealer necesita exactamente tres CardView como hijos directos de CardFan.", this);
                return;
            }

            IReadOnlyList<SpanishCard> cards = deck.Draw(3);
            playerCards.AddRange(cards);

            for (int i = 0; i < dealtViews.Count; i++)
            {
                SpanishCard card = cards[i];
                Sprite sprite = Resources.Load<Sprite>($"SpanishCards/{card.FileName}");

                if (sprite == null)
                {
                    Debug.LogError($"No se encontró el Sprite Resources/SpanishCards/{card.FileName}.jpg", this);
                    continue;
                }

                dealtViews[i].SetCard(card, sprite);
            }

            if (TryGetComponent(out CardFanController fanController))
                fanController.RefreshLayout();
        }

        public IReadOnlyList<SpanishCard> DealOpponentCards()
        {
            if (deck == null)
                throw new System.InvalidOperationException("Primero debe repartirse la mano del jugador.");

            return deck.Draw(3);
        }
    }
}
