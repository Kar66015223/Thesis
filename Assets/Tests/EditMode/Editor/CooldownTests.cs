using NUnit.Framework;

namespace EditModeTests
{
    public class CooldownTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void NewCooldown_IsReady()
        {
            var cooldown = new Cooldown();

            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(0f, cooldown.Remaining);
        }

        [Test]
        public void Start_MakesCooldownNotReady()
        {
            var cooldown = new Cooldown();

            cooldown.Start(2f);

            Assert.IsFalse(cooldown.IsReady);
            Assert.AreEqual(2f, cooldown.Remaining, Tolerance);
        }

        [Test]
        public void Tick_CountsDown()
        {
            var cooldown = new Cooldown();
            cooldown.Start(2f);

            cooldown.Tick(0.5f);

            Assert.IsFalse(cooldown.IsReady);
            Assert.AreEqual(1.5f, cooldown.Remaining, Tolerance);
        }

        [Test]
        public void Tick_ReachingExactlyZero_IsReady()
        {
            var cooldown = new Cooldown();
            cooldown.Start(1f);

            cooldown.Tick(0.5f);
            cooldown.Tick(0.5f);

            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(0f, cooldown.Remaining);
        }

        [Test]
        public void Tick_PastZero_IsReadyAndRemainingNeverGoesNegative()
        {
            var cooldown = new Cooldown();
            cooldown.Start(1f);

            cooldown.Tick(3f);
            cooldown.Tick(3f);

            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(0f, cooldown.Remaining);
        }

        [Test]
        public void Start_WhileRunning_RestartsWithNewDuration()
        {
            var cooldown = new Cooldown();
            cooldown.Start(5f);
            cooldown.Tick(4f);

            cooldown.Start(3f);

            Assert.AreEqual(3f, cooldown.Remaining, Tolerance);
        }

        [Test]
        public void Start_WithZeroDuration_IsReadyImmediately()
        {
            var cooldown = new Cooldown();

            cooldown.Start(0f);

            Assert.IsTrue(cooldown.IsReady);
        }

        [Test]
        public void Start_AfterBeingReady_CanBeReused()
        {
            var cooldown = new Cooldown();
            cooldown.Start(1f);
            cooldown.Tick(2f);
            Assert.IsTrue(cooldown.IsReady);

            cooldown.Start(1f);

            Assert.IsFalse(cooldown.IsReady);
            Assert.AreEqual(1f, cooldown.Remaining, Tolerance);
        }
    }
}
