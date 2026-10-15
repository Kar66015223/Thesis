using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace EditModeTests
{
    public class Mission1Tests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<string> tips = new();

        private Mission1 mission;
        private MorseCode morseCode;
        private Item otherItem;

        [SetUp]
        public void SetUp()
        {
            tips.Clear();
            UIEvents.TipRequested += tips.Add;

            mission = new Mission1();
            morseCode = CreateObject("MorseCode").AddComponent<MorseCode>();
            otherItem = CreateObject("OtherItem").AddComponent<Item>();
        }

        [TearDown]
        public void TearDown()
        {
            // Mission1 only unsubscribes from WorldEvents when it completes, so finish any mission
            // a test left in progress; otherwise it would keep listening during later tests.
            WorldEvents.RaiseItemCollected(morseCode);

            UIEvents.TipRequested -= tips.Add;

            foreach (GameObject go in createdObjects)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            createdObjects.Clear();
        }

        private GameObject CreateObject(string name)
        {
            var go = new GameObject(name);
            createdObjects.Add(go);
            return go;
        }

        [Test]
        public void Mission1_IsTheMainMissionForTheMission1Scene()
        {
            Assert.AreEqual("Mission1", mission.Name);
            Assert.AreEqual(MissionType.Main, mission.Type);
            Assert.AreEqual(SceneConstants.MISSION1_NAME, mission.SceneName);
            Assert.AreEqual(MissionStatus.Pending, mission.Status);
        }

        [Test]
        public void Accept_StartsMissionAndShowsTip()
        {
            mission.Accept();

            Assert.AreEqual(MissionStatus.OnProgress, mission.Status);
            CollectionAssert.AreEqual(new[] { "Mission Accepted" }, tips);
        }

        [Test]
        public void CollectingMorseCode_BeforeAccept_DoesNothing()
        {
            WorldEvents.RaiseItemCollected(morseCode);

            Assert.AreEqual(MissionStatus.Pending, mission.Status);
            CollectionAssert.IsEmpty(tips);
        }

        [Test]
        public void CollectingOtherItem_DoesNotComplete()
        {
            mission.Accept();

            WorldEvents.RaiseItemCollected(otherItem);

            Assert.AreEqual(MissionStatus.OnProgress, mission.Status);
            Assert.IsFalse(mission.IsCompleted);
        }

        [Test]
        public void CollectingMorseCode_AfterAccept_CompletesMission()
        {
            var changes = new List<MissionStatus>();
            mission.StatusChanged += m => changes.Add(m.Status);
            mission.Accept();

            WorldEvents.RaiseItemCollected(morseCode);

            Assert.AreEqual(MissionStatus.Completed, mission.Status);
            Assert.IsTrue(mission.IsCompleted);
            CollectionAssert.AreEqual(new[] { MissionStatus.OnProgress, MissionStatus.Completed }, changes);
            CollectionAssert.AreEqual(new[] { "Mission Accepted", "Mission1 completed" }, tips);
        }

        [Test]
        public void CollectingMorseCodeAgain_AfterCompletion_HasNoFurtherEffect()
        {
            mission.Accept();
            WorldEvents.RaiseItemCollected(morseCode);
            int changeCount = 0;
            mission.StatusChanged += _ => changeCount++;
            int tipCount = tips.Count;

            WorldEvents.RaiseItemCollected(morseCode);

            Assert.AreEqual(MissionStatus.Completed, mission.Status);
            Assert.AreEqual(0, changeCount);
            Assert.AreEqual(tipCount, tips.Count);
        }

        [Test]
        public void CompletedMission1_CanBeSubmitted()
        {
            mission.Accept();
            WorldEvents.RaiseItemCollected(morseCode);

            mission.Submit();

            Assert.AreEqual(MissionStatus.Submitted, mission.Status);
        }
    }
}
