using System;

namespace TrucoVR.Domain
{
    public sealed class SimpleOpponentAi
    {
        private readonly Random random;

        public SimpleOpponentAi(int? seed = null)
        {
            random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public SpanishCard ChooseCard(TrucoHand hand)
        {
            if (hand == null)
                throw new ArgumentNullException(nameof(hand));

            if (hand.Count == 0)
                throw new InvalidOperationException("La IA no tiene cartas para jugar.");

            int selectedIndex = random.Next(hand.Count);
            return hand.PlayAt(selectedIndex);
        }

        public TrucoResponse RespondToTruco(TrucoBetLevel level, TrucoHand hand)
        {
            if (hand == null)
                throw new ArgumentNullException(nameof(hand));

            if (hand.Count == 0)
                return TrucoResponse.Rejected;

            int strongestRank = 0;

            foreach (SpanishCard card in hand.Cards)
                strongestRank = Math.Max(strongestRank, TrucoRules.GetTrucoRank(card));

            int minimumRank = level == TrucoBetLevel.ValeCuatro ? 8 : 6;
            bool acceptsWithStrength = strongestRank >= minimumRank;
            bool acceptsByChance = random.NextDouble() >= 0.25;

            return acceptsWithStrength || acceptsByChance
                ? TrucoResponse.Accepted
                : TrucoResponse.Rejected;
        }

        public TrucoResponse RespondToEnvido(int playerEnvido, int opponentEnvido)
        {
            if (opponentEnvido >= playerEnvido)
                return TrucoResponse.Accepted;

            return random.NextDouble() >= 0.35
                ? TrucoResponse.Accepted
                : TrucoResponse.Rejected;
        }

        public EnvidoBetLevel ChooseEnvidoCall(
            int envido,
            int playerPoints,
            int opponentPoints,
            int targetPoints)
        {
            if (envido < 25)
                return EnvidoBetLevel.None;

            int faltaPoints = EnvidoBet.GetPointsInPlay(
                EnvidoBetLevel.FaltaEnvido,
                EnvidoBetLevel.None,
                targetPoints,
                playerPoints,
                opponentPoints);
            bool faltaIsSafe = playerPoints + faltaPoints < targetPoints;
            bool realIsSafe = playerPoints + 3 < targetPoints;
            bool envidoIsSafe = playerPoints + 2 < targetPoints;
            double callChance = envido >= 32 ? 0.55 : envido >= 29 ? 0.35 : 0.2;

            if (random.NextDouble() >= callChance)
                return EnvidoBetLevel.None;

            if (envido >= 32 && faltaIsSafe)
                return EnvidoBetLevel.FaltaEnvido;

            if (envido >= 29 && realIsSafe)
                return EnvidoBetLevel.RealEnvido;

            return envidoIsSafe ? EnvidoBetLevel.Envido : EnvidoBetLevel.None;
        }
    }
}
