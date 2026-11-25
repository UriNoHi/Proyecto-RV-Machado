using System;

namespace TrucoVR.Domain
{
    [Serializable]
    public readonly struct SpanishCard
    {
        public CardSuit Suit { get; }
        public int Value { get; }

        public SpanishCard(CardSuit suit, int value)
        {
            if (value < 1 || value > 12 || value == 8 || value == 9)
                throw new ArgumentOutOfRangeException(nameof(value), "El mazo español Truco solo contiene valores 1-7 y 10-12.");

            Suit = suit;
            Value = value;
        }

        public string FileName
        {
            get
            {
                string suitName = Suit switch
                {
                    CardSuit.Espada => "espadas",
                    CardSuit.Basto => "bastos",
                    CardSuit.Oro => "oros",
                    CardSuit.Copa => "copas",
                    _ => throw new ArgumentOutOfRangeException()
                };

                return $"card_{suitName}_{Value:00}";
            }
        }

        public override string ToString() => $"{Value} de {Suit}";
    }
}
