using System;

namespace TrucoVR.Domain
{
    public static class TrucoRules
    {
        public static int GetTrucoRank(SpanishCard card)
        {
            if (card.Suit == CardSuit.Espada && card.Value == 1) return 14;
            if (card.Suit == CardSuit.Basto && card.Value == 1) return 13;
            if (card.Suit == CardSuit.Espada && card.Value == 7) return 12;
            if (card.Suit == CardSuit.Oro && card.Value == 7) return 11;
            if (card.Value == 3) return 10;
            if (card.Value == 2) return 9;
            if (card.Value == 1 && (card.Suit == CardSuit.Oro || card.Suit == CardSuit.Copa)) return 8;
            if (card.Value == 12) return 7;
            if (card.Value == 11) return 6;
            if (card.Value == 10) return 5;
            if (card.Value == 7 && (card.Suit == CardSuit.Copa || card.Suit == CardSuit.Basto)) return 4;
            if (card.Value == 6) return 3;
            if (card.Value == 5) return 2;
            if (card.Value == 4) return 1;

            throw new ArgumentOutOfRangeException(nameof(card), $"Carta no válida: {card}");
        }

        public static int CompareCards(SpanishCard first, SpanishCard second)
        {
            return GetTrucoRank(first).CompareTo(GetTrucoRank(second));
        }

        public static int CalculateEnvido(SpanishCard first, SpanishCard second, SpanishCard third)
        {
            int best = GetEnvidoValue(first);
            best = Math.Max(best, GetEnvidoValue(second));
            best = Math.Max(best, GetEnvidoValue(third));

            best = Math.Max(best, SameSuitValue(first, second));
            best = Math.Max(best, SameSuitValue(first, third));
            best = Math.Max(best, SameSuitValue(second, third));
            return best;
        }

        private static int SameSuitValue(SpanishCard first, SpanishCard second)
        {
            if (first.Suit != second.Suit)
                return 0;

            return 20 + GetEnvidoValue(first) + GetEnvidoValue(second);
        }

        private static int GetEnvidoValue(SpanishCard card)
        {
            return card.Value >= 10 ? 0 : card.Value;
        }
    }
}
