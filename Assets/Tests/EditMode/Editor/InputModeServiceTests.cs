using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace EditModeTests
{
    /// <summary>
    /// InputModeService is static, so every test releases what it requested in TearDown.
    /// Cursor lock/visibility isn't asserted: in edit mode the Editor owns the cursor.
    /// </summary>
    public class InputModeServiceTests
    {
        private const string PlayerMap = PlayerConstants.ACTIONMAP_PLAYER;
        private const string UIMap = PlayerConstants.ACTIONMAP_UI;
        private const string StruggleMap = PlayerConstants.ACTIONMAP_STRUGGLE;

        private readonly List<object> requesters = new();
        private readonly List<GameObject> createdObjects = new();
        private readonly List<string> mapChanges = new();

        [SetUp]
        public void SetUp()
        {
            InputModeService.SetStruggling(false);
            Assume.That(InputModeService.CurrentActionMap, Is.EqualTo(PlayerMap),
                "another requester is still holding UI mode; these tests need a clean service");

            mapChanges.Clear();
            InputModeService.ActionMapChanged += mapChanges.Add;
        }

        [TearDown]
        public void TearDown()
        {
            InputModeService.ActionMapChanged -= mapChanges.Add;

            foreach (object requester in requesters)
            {
                InputModeService.RequestUI(requester, false);
                InputModeService.RequestCursor(requester, false);
            }
            requesters.Clear();
            InputModeService.SetStruggling(false);

            foreach (GameObject go in createdObjects)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            createdObjects.Clear();
        }

        private object CreateRequester()
        {
            var requester = new object();
            requesters.Add(requester);
            return requester;
        }

        private GameObject CreateUnityRequester(string name)
        {
            var go = new GameObject(name);
            createdObjects.Add(go);
            requesters.Add(go);
            return go;
        }

        [Test]
        public void NoRequests_UsesPlayerMap()
        {
            InputModeService.Refresh();

            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void RequestUI_SwitchesToUIMap_AndReleaseSwitchesBack()
        {
            object window = CreateRequester();

            InputModeService.RequestUI(window, true);
            Assert.AreEqual(UIMap, InputModeService.CurrentActionMap);

            InputModeService.RequestUI(window, false);
            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void OverlappingWindows_UIStaysUntilLastOneReleases()
        {
            object readNote = CreateRequester();
            object passwordPad = CreateRequester();

            InputModeService.RequestUI(readNote, true);
            InputModeService.RequestUI(passwordPad, true);

            InputModeService.RequestUI(readNote, false);
            Assert.AreEqual(UIMap, InputModeService.CurrentActionMap, "password pad is still open");

            InputModeService.RequestUI(passwordPad, false);
            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void SameRequesterTwice_NeedsOnlyOneRelease()
        {
            object window = CreateRequester();

            InputModeService.RequestUI(window, true);
            InputModeService.RequestUI(window, true);
            InputModeService.RequestUI(window, false);

            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void ReleasingWithoutRequest_IsHarmless()
        {
            object window = CreateRequester();
            object other = CreateRequester();
            InputModeService.RequestUI(window, true);

            InputModeService.RequestUI(other, false);

            Assert.AreEqual(UIMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void NullRequester_IsIgnored()
        {
            Assert.DoesNotThrow(() => InputModeService.RequestUI(null, true));

            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void CursorRequest_KeepsPlayerMap()
        {
            object radialMenu = CreateRequester();

            InputModeService.RequestCursor(radialMenu, true);

            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap,
                "a cursor-only request must not take gameplay input away");
        }

        [Test]
        public void DestroyedRequester_IsPrunedOnRefresh()
        {
            GameObject window = CreateUnityRequester("Window");
            InputModeService.RequestUI(window, true);
            Assert.AreEqual(UIMap, InputModeService.CurrentActionMap);

            Object.DestroyImmediate(window);   // closed without releasing (e.g. scene unloaded)
            InputModeService.Refresh();

            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void DestroyedRequester_DoesNotKeepUIAliveForOthers()
        {
            GameObject destroyed = CreateUnityRequester("Destroyed");
            object window = CreateRequester();
            InputModeService.RequestUI(destroyed, true);
            InputModeService.RequestUI(window, true);

            Object.DestroyImmediate(destroyed);
            InputModeService.RequestUI(window, false);

            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void Struggling_OverridesUI()
        {
            object window = CreateRequester();
            InputModeService.RequestUI(window, true);

            InputModeService.SetStruggling(true);
            Assert.AreEqual(StruggleMap, InputModeService.CurrentActionMap);

            InputModeService.SetStruggling(false);
            Assert.AreEqual(UIMap, InputModeService.CurrentActionMap, "UI comes back once the struggle ends");

            InputModeService.RequestUI(window, false);
            Assert.AreEqual(PlayerMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void Struggling_StaysWhenUIOpensDuringIt()
        {
            object window = CreateRequester();
            InputModeService.SetStruggling(true);

            InputModeService.RequestUI(window, true);

            Assert.AreEqual(StruggleMap, InputModeService.CurrentActionMap);
        }

        [Test]
        public void ActionMapChanged_ReportsCurrentMapOnEveryChange()
        {
            object window = CreateRequester();

            InputModeService.RequestUI(window, true);
            InputModeService.SetStruggling(true);
            InputModeService.SetStruggling(false);
            InputModeService.RequestUI(window, false);

            CollectionAssert.AreEqual(new[] { UIMap, StruggleMap, UIMap, PlayerMap }, mapChanges);
        }
    }
}
