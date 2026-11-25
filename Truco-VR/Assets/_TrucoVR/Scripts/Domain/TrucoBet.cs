using System;

namespace TrucoVR.Domain
{
    public enum TrucoBetLevel
    {
        None,
        Truco,
        Retruco,
        ValeCuatro
    }

    public static class TrucoBet
    {
        public static int GetPointsInPlay(TrucoBetLevel level)
        {
            return level switch
            {
                TrucoBetLevel.None => 1,
                TrucoBetLevel.Truco => 2,
                TrucoBetLevel.Retruco => 3,
                TrucoBetLevel.ValeCuatro => 4,
                _ => throw new ArgumentOutOfRangeException(nameof(level))
            };
        }

        public static bool CanRaise(TrucoBetLevel level)
        {
            return level == TrucoBetLevel.None ||
                   level == TrucoBetLevel.Truco ||
                   level == TrucoBetLevel.Retruco;
        }

        public static TrucoBetLevel Raise(TrucoBetLevel level)
        {
            return level switch
            {
                TrucoBetLevel.None => TrucoBetLevel.Truco,
                TrucoBetLevel.Truco => TrucoBetLevel.Retruco,
                TrucoBetLevel.Retruco => TrucoBetLevel.ValeCuatro,
                TrucoBetLevel.ValeCuatro => TrucoBetLevel.ValeCuatro,
                _ => throw new ArgumentOutOfRangeException(nameof(level))
            };
        }

        public static int GetPointsForRejection(TrucoBetLevel level)
        {
            return level switch
            {
                TrucoBetLevel.None => 0,
                TrucoBetLevel.Truco => 1,
                TrucoBetLevel.Retruco => 2,
                TrucoBetLevel.ValeCuatro => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(level))
            };
        }
    }
}
