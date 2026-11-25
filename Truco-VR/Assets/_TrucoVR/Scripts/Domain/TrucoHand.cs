using System;
using System.Collections.Generic;

namespace TrucoVR.Domain
{
    public sealed class TrucoHand
    {
        private readonly List<SpanishCard> cards = new List<SpanishCard>(3);

        public IReadOnlyList<SpanishCard> Cards => cards;
        public int Count => cards.Count;

        public void Add(SpanishCard card)
        {
            if (cards.Count >= 3)
                throw new InvalidOperationException("Una mano de Truco no puede tener más de tres cartas.");

            cards.Add(card);
        }

        public SpanishCard PlayAt(int index)
        {
            if (index < 0 || index >= cards.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            SpanishCard card = cards[index];
            cards.RemoveAt(index);
            return card;
        }

        public void Clear()
        {
            cards.Clear();
        }
    }
}
