using System;

namespace TrucoVR.Domain
{
    public enum EnvidoBetLevel
    {
        None,
        Envido,
        RealEnvido,
        FaltaEnvido
    }

    public static class EnvidoBet
    {
        public static bool CanRaiseTo(EnvidoBetLevel current, EnvidoBetLevel next)
        {
            return current switch
            {
                EnvidoBetLevel.None => next == EnvidoBetLevel.Envido ||
                                       next == EnvidoBetLevel.RealEnvido ||
                                       next == EnvidoBetLevel.FaltaEnvido,
                EnvidoBetLevel.Envido => next == EnvidoBetLevel.RealEnvido ||
                                         next == EnvidoBetLevel.FaltaEnvido,
                EnvidoBetLevel.RealEnvido => next == EnvidoBetLevel.FaltaEnvido,
                _ => false
            };
        }

        public static int GetPointsInPlay(
            EnvidoBetLevel level,
            EnvidoBetLevel previousLevel,
            int targetPoints,
            int playerPoints,
            int opponentPoints)
        {
            return level switch
            {
                EnvidoBetLevel.Envido => 2,
                EnvidoBetLevel.RealEnvido => previousLevel == EnvidoBetLevel.Envido ? 5 : 3,
                EnvidoBetLevel.FaltaEnvido => GetFaltaEnvidoPoints(targetPoints, playerPoints, opponentPoints),
                _ => throw new ArgumentOutOfRangeException(nameof(level))
            };
        }

        public static int GetPointsForRejection(int previousPointsInPlay)
        {
            if (previousPointsInPlay < 0)
                throw new ArgumentOutOfRangeException(nameof(previousPointsInPlay));

            return previousPointsInPlay == 0 ? 1 : previousPointsInPlay;
        }

        private static int GetFaltaEnvidoPoints(int targetPoints, int playerPoints, int opponentPoints)
        {
            if (targetPoints <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetPoints));

            if (playerPoints < 0)
                throw new ArgumentOutOfRangeException(nameof(playerPoints));

            if (opponentPoints < 0)
                throw new ArgumentOutOfRangeException(nameof(opponentPoints));

            return Math.Max(1, targetPoints - Math.Max(playerPoints, opponentPoints));
        }
    }
}
