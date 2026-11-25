using System;
using System.Collections.Generic;

namespace TrucoVR.Domain
{
    public sealed class SpanishDeck
    {
        private readonly List<SpanishCard> cards = new List<SpanishCard>(40);

        public int RemainingCount => cards.Count;

        public SpanishDeck()
        {
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                for (int value = 1; value <= 7; value++)
                    cards.Add(new SpanishCard(suit, value));

                cards.Add(new SpanishCard(suit, 10));
                cards.Add(new SpanishCard(suit, 11));
                cards.Add(new SpanishCard(suit, 12));
            }
        }

        public void Shuffle(int? seed = null)
        {
            Random random = seed.HasValue ? new Random(seed.Value) : new Random();

            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                SpanishCard temporary = cards[i];
                cards[i] = cards[j];
                cards[j] = temporary;
            }
        }

        public SpanishCard Draw()
        {
            if (cards.Count == 0)
                throw new InvalidOperationException("El mazo español está vacío.");

            int lastIndex = cards.Count - 1;
            SpanishCard card = cards[lastIndex];
            cards.RemoveAt(lastIndex);
            return card;
        }

        public IReadOnlyList<SpanishCard> Draw(int count)
        {
            if (count < 0 || count > cards.Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            List<SpanishCard> dealtCards = new List<SpanishCard>(count);

            for (int i = 0; i < count; i++)
                dealtCards.Add(Draw());

            return dealtCards;
        }
    }
}
