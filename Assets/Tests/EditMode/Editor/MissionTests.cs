using System.Collections.Generic;
using NUnit.Framework;

namespace EditModeTests
{
    public class MissionTests
    {
        private TestMission mission;
        private readonly List<MissionStatus> statusLog = new();

        [SetUp]
        public void SetUp()
        {
            statusLog.Clear();
            mission = new TestMission();
            mission.StatusChanged += m =>
            {
                Assert.AreSame(mission, m, "StatusChanged should pass the mission itself");
                statusLog.Add(m.Status);
            };
        }

        [Test]
        public void Constructor_StoresDataAndStartsPending()
        {
            Assert.AreEqual("Test", mission.Name);
            Assert.AreEqual("A mission used by unit tests.", mission.Description);
            Assert.AreEqual(MissionType.Sub, mission.Type);
            Assert.AreEqual("TestScene", mission.SceneName);
            Assert.AreEqual(MissionStatus.Pending, mission.Status);
            Assert.IsFalse(mission.IsCompleted);
        }

        [Test]
        public void FullLifecycle_GoesPendingToSubmitted_FiringStatusChangedEachStep()
        {
            mission.Accept();
            Assert.AreEqual(MissionStatus.OnProgress, mission.Status);
            Assert.IsFalse(mission.IsCompleted);

            mission.ForceComplete();
            Assert.AreEqual(MissionStatus.Completed, mission.Status);
            Assert.IsTrue(mission.IsCompleted);

            mission.Submit();
            Assert.AreEqual(MissionStatus.Submitted, mission.Status);
            Assert.IsTrue(mission.IsCompleted, "a submitted mission still counts as completed");

            CollectionAssert.AreEqual(
                new[] { MissionStatus.OnProgress, MissionStatus.Completed, MissionStatus.Submitted },
                statusLog);
            Assert.AreEqual(1, mission.AcceptedCalls);
            Assert.AreEqual(1, mission.CompletedCalls);
            Assert.AreEqual(1, mission.SubmittedCalls);
        }

        [Test]
        public void Complete_BeforeAccept_IsIgnored()
        {
            mission.ForceComplete();

            Assert.AreEqual(MissionStatus.Pending, mission.Status);
            Assert.AreEqual(0, mission.CompletedCalls);
            CollectionAssert.IsEmpty(statusLog);
        }

        [Test]
        public void Submit_BeforeComplete_IsIgnored()
        {
            mission.Submit();
            Assert.AreEqual(MissionStatus.Pending, mission.Status);

            mission.Accept();
            mission.Submit();
            Assert.AreEqual(MissionStatus.OnProgress, mission.Status);

            Assert.AreEqual(0, mission.SubmittedCalls);
            CollectionAssert.AreEqual(new[] { MissionStatus.OnProgress }, statusLog);
        }

        [Test]
        public void RepeatedTransitions_AreIgnored()
        {
            mission.Accept();
            mission.Accept();
            mission.ForceComplete();
            mission.ForceComplete();
            mission.Submit();
            mission.Submit();

            Assert.AreEqual(1, mission.AcceptedCalls);
            Assert.AreEqual(1, mission.CompletedCalls);
            Assert.AreEqual(1, mission.SubmittedCalls);
            Assert.AreEqual(3, statusLog.Count);
        }

        [Test]
        public void Accept_AfterMissionMovedOn_DoesNotRestartIt()
        {
            mission.Accept();
            mission.ForceComplete();

            mission.Accept();

            Assert.AreEqual(MissionStatus.Completed, mission.Status);
            Assert.AreEqual(1, mission.AcceptedCalls);
        }
    }
}
