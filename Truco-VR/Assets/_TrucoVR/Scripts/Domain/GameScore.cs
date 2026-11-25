using System;

namespace TrucoVR.Domain
{
    public sealed class GameScore
    {
        public int PlayerPoints { get; private set; }
        public int OpponentPoints { get; private set; }
        public int TargetPoints { get; }
        public bool IsGameOver => PlayerPoints >= TargetPoints || OpponentPoints >= TargetPoints;

        public GameScore(int targetPoints = 30)
        {
            if (targetPoints <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetPoints));

            TargetPoints = targetPoints;
        }

        public void AddPlayerPoints(int points)
        {
            ValidatePoints(points);
            PlayerPoints = Math.Min(PlayerPoints + points, TargetPoints);
        }

        public void AddOpponentPoints(int points)
        {
            ValidatePoints(points);
            OpponentPoints = Math.Min(OpponentPoints + points, TargetPoints);
        }

        public void Reset()
        {
            PlayerPoints = 0;
            OpponentPoints = 0;
        }

        private static void ValidatePoints(int points)
        {
            if (points <= 0)
                throw new ArgumentOutOfRangeException(nameof(points));
        }
    }
}
