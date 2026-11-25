using NUnit.Framework;
using TrucoVR.Domain;

namespace TrucoVR.Tests
{
    public sealed class EnvidoBetTests
    {
        [TestCase(EnvidoBetLevel.None, EnvidoBetLevel.Envido, true)]
        [TestCase(EnvidoBetLevel.None, EnvidoBetLevel.RealEnvido, true)]
        [TestCase(EnvidoBetLevel.None, EnvidoBetLevel.FaltaEnvido, true)]
        [TestCase(EnvidoBetLevel.Envido, EnvidoBetLevel.RealEnvido, true)]
        [TestCase(EnvidoBetLevel.Envido, EnvidoBetLevel.FaltaEnvido, true)]
        [TestCase(EnvidoBetLevel.RealEnvido, EnvidoBetLevel.FaltaEnvido, true)]
        [TestCase(EnvidoBetLevel.Envido, EnvidoBetLevel.Envido, false)]
        [TestCase(EnvidoBetLevel.RealEnvido, EnvidoBetLevel.Envido, false)]
        [TestCase(EnvidoBetLevel.FaltaEnvido, EnvidoBetLevel.FaltaEnvido, false)]
        public void CanRaiseToOnlyAllowsLegalEscalations(
            EnvidoBetLevel current,
            EnvidoBetLevel next,
            bool expected)
        {
            Assert.AreEqual(expected, EnvidoBet.CanRaiseTo(current, next));
        }

        [TestCase(EnvidoBetLevel.Envido, 2)]
        [TestCase(EnvidoBetLevel.RealEnvido, 3)]
        public void AcceptedOpeningCallUsesItsStake(EnvidoBetLevel level, int expectedPoints)
        {
            Assert.AreEqual(expectedPoints, EnvidoBet.GetPointsInPlay(level, EnvidoBetLevel.None, 30, 8, 12));
        }

        [Test]
        public void AcceptedRealEnvidoAfterEnvidoAddsThreeMorePoints()
        {
            Assert.AreEqual(
                5,
                EnvidoBet.GetPointsInPlay(
                    EnvidoBetLevel.RealEnvido,
                    EnvidoBetLevel.Envido,
                    30,
                    8,
                    12));
        }

        [Test]
        public void AcceptedFaltaUsesPointsNeededByTheLeader()
        {
            Assert.AreEqual(
                6,
                EnvidoBet.GetPointsInPlay(
                    EnvidoBetLevel.FaltaEnvido,
                    EnvidoBetLevel.RealEnvido,
                    30,
                    18,
                    24));
        }

        [TestCase(0, 1)]
        [TestCase(2, 2)]
        [TestCase(3, 3)]
        [TestCase(5, 5)]
        public void RejectedRaiseAwardsPreviousStake(int previousPointsInPlay, int expectedPoints)
        {
            Assert.AreEqual(expectedPoints, EnvidoBet.GetPointsForRejection(previousPointsInPlay));
        }

        [Test]
        public void AIOnlyCallsWithSufficientStrengthAndAvoidsImmediateMatchPointRisk()
        {
            var ai = new SimpleOpponentAi(1);

            Assert.AreEqual(EnvidoBetLevel.None, ai.ChooseEnvidoCall(24, 0, 0, 30));
            Assert.AreEqual(EnvidoBetLevel.FaltaEnvido, ai.ChooseEnvidoCall(32, 8, 18, 30));
            Assert.AreEqual(EnvidoBetLevel.None, ai.ChooseEnvidoCall(32, 28, 10, 30));
        }

        [Test]
        public void AICanChooseEachLegalOpeningLevel()
        {
            var envidoAi = new SimpleOpponentAi(14);
            var realEnvidoAi = new SimpleOpponentAi(1);
            var faltaEnvidoAi = new SimpleOpponentAi(1);

            Assert.AreEqual(EnvidoBetLevel.Envido, envidoAi.ChooseEnvidoCall(26, 0, 0, 30));
            Assert.AreEqual(EnvidoBetLevel.RealEnvido, realEnvidoAi.ChooseEnvidoCall(29, 0, 0, 30));
            Assert.AreEqual(EnvidoBetLevel.FaltaEnvido, faltaEnvidoAi.ChooseEnvidoCall(32, 8, 18, 30));
        }
    }
}
