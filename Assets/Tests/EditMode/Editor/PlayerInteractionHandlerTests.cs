using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace EditModeTests
{
    /// <summary>
    /// Items are real <see cref="Item"/> components: Unity refuses to AddComponent a MonoBehaviour
    /// compiled from an Editor folder, so a fake Item subclass can't live next to these tests.
    /// An item is made unavailable by disabling it, and pick-ups are observed via WorldEvents.
    /// </summary>
    public class PlayerInteractionHandlerTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<Item> pickedUp = new();

        private PlayerInteractionHandler handler;
        private GameObject interactor;

        [SetUp]
        public void SetUp()
        {
            handler = new PlayerInteractionHandler();
            interactor = CreateObject("Interactor", Vector3.zero);
            pickedUp.Clear();
            WorldEvents.ItemCollected += pickedUp.Add;
        }

        [TearDown]
        public void TearDown()
        {
            WorldEvents.ItemCollected -= pickedUp.Add;

            foreach (GameObject go in createdObjects)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            createdObjects.Clear();
        }

        private GameObject CreateObject(string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            createdObjects.Add(go);
            return go;
        }

        private FakeInteractable CreateInteractable(string name, InteractButton button, float distance)
            => new(CreateObject(name, new Vector3(distance, 0f, 0f)), button);

        private Item CreateItem(string name, float distance = 1f)
            => CreateObject(name, new Vector3(0f, 0f, distance)).AddComponent<Item>();

        private void RefreshAs(bool interactorCanInteract, params IInteractable[] detected)
            => handler.Refresh(detected, interactor, interactorCanInteract);

        private void Refresh(params IInteractable[] detected) => RefreshAs(true, detected);

        // ---------- Target selection ----------

        [Test]
        public void NothingDetected_HasNoTargetsOrItems()
        {
            Refresh();

            CollectionAssert.IsEmpty(handler.Items);
            Assert.IsNull(handler.SelectedItem);
            Assert.IsNull(handler.GetTarget(InteractButton.F));
            Assert.IsNull(handler.GetTarget(InteractButton.Spacebar));
        }

        [Test]
        public void SeveralInteractablesForSameButton_NearestWins()
        {
            FakeInteractable far = CreateInteractable("FarDoor", InteractButton.F, 5f);
            FakeInteractable near = CreateInteractable("NearDoor", InteractButton.F, 1f);
            FakeInteractable middle = CreateInteractable("MiddleDoor", InteractButton.F, 3f);

            Refresh(far, near, middle);

            Assert.AreSame(near, handler.GetTarget(InteractButton.F));
        }

        [Test]
        public void EachButton_GetsNearestInteractableUsingThatButton()
        {
            FakeInteractable vault = CreateInteractable("Vault", InteractButton.Spacebar, 1f);
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 3f);

            Refresh(door, vault);

            Assert.AreSame(door, handler.GetTarget(InteractButton.F), "a nearer object on another button must not block F");
            Assert.AreSame(vault, handler.GetTarget(InteractButton.Spacebar));
        }

        [Test]
        public void SelectedItem_WinsItsButtonOverNearerInteractable()
        {
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 0.5f);
            Item item = CreateItem("Item", distance: 5f);

            Refresh(door, item);

            Assert.AreSame(item, handler.GetTarget(InteractButton.F));
        }

        [Test]
        public void Item_DoesNotTakeOverOtherButtons()
        {
            FakeInteractable vault = CreateInteractable("Vault", InteractButton.Spacebar, 2f);
            Item item = CreateItem("Item");

            Refresh(item, vault);

            Assert.AreSame(item, handler.GetTarget(InteractButton.F));
            Assert.AreSame(vault, handler.GetTarget(InteractButton.Spacebar));
        }

        [Test]
        public void Items_GoToItemListOnly_InDetectionOrder()
        {
            Item first = CreateItem("First");
            Item second = CreateItem("Second");
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 1f);

            Refresh(first, door, second);

            CollectionAssert.AreEqual(new Item[] { first, second }, handler.Items);
            Assert.AreSame(first, handler.SelectedItem);
            Assert.AreEqual(0, handler.SelectedItemIndex);
        }

        [Test]
        public void CandidatesThatCannotInteract_AreSkipped()
        {
            FakeInteractable lockedDoor = CreateInteractable("LockedDoor", InteractButton.F, 1f);
            FakeInteractable openDoor = CreateInteractable("OpenDoor", InteractButton.F, 4f);
            Item unavailableItem = CreateItem("Unavailable");
            lockedDoor.Interactable = false;
            unavailableItem.enabled = false;   // Item.CanInteract is isActiveAndEnabled

            Refresh(lockedDoor, openDoor, unavailableItem);

            CollectionAssert.IsEmpty(handler.Items);
            Assert.AreSame(openDoor, handler.GetTarget(InteractButton.F));
        }

        [Test]
        public void InteractorCannotInteract_NothingIsSelectable()
        {
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 1f);
            FakeInteractable vault = CreateInteractable("Vault", InteractButton.Spacebar, 1f);
            Item item = CreateItem("Item");

            RefreshAs(false, door, vault, item);

            CollectionAssert.IsEmpty(handler.Items);
            Assert.IsNull(handler.SelectedItem);
            Assert.IsNull(handler.GetTarget(InteractButton.F));
            Assert.IsNull(handler.GetTarget(InteractButton.Spacebar));
            Assert.IsFalse(handler.TryInteract(InteractButton.F, interactor));
            Assert.AreEqual(0, door.InteractCount);
            CollectionAssert.IsEmpty(pickedUp);
        }

        [Test]
        public void InteractorRegainsAbility_TargetsComeBack()
        {
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 1f);

            RefreshAs(false, door);
            Assert.IsNull(handler.GetTarget(InteractButton.F));

            RefreshAs(true, door);
            Assert.AreSame(door, handler.GetTarget(InteractButton.F));
        }

        // ---------- Item selection across refreshes ----------

        [Test]
        public void ListChanges_SelectionStaysOnSameItem()
        {
            Item a = CreateItem("A");
            Item b = CreateItem("B");
            Item c = CreateItem("C");
            Item newcomer = CreateItem("New");

            Refresh(a, b, c);
            handler.ScrollItemSelection(-1f);   // select B
            Assert.AreSame(b, handler.SelectedItem);

            Refresh(newcomer, a, b, c);         // B shifts to index 2
            Assert.AreSame(b, handler.SelectedItem);
            Assert.AreEqual(2, handler.SelectedItemIndex);

            Refresh(b, c);                      // items before B leave
            Assert.AreSame(b, handler.SelectedItem);
            Assert.AreEqual(0, handler.SelectedItemIndex);
        }

        [Test]
        public void SelectedItemLeaves_IndexIsClampedToList()
        {
            Item a = CreateItem("A");
            Item b = CreateItem("B");
            Item c = CreateItem("C");

            Refresh(a, b, c);
            handler.ScrollItemSelection(1f);    // wrap to C (index 2)
            Assert.AreSame(c, handler.SelectedItem);

            Refresh(a, b);
            Assert.AreEqual(1, handler.SelectedItemIndex);
            Assert.AreSame(b, handler.SelectedItem);
        }

        [Test]
        public void AllItemsLeave_SelectionIsCleared()
        {
            Item a = CreateItem("A");
            Item b = CreateItem("B");

            Refresh(a, b);
            handler.ScrollItemSelection(-1f);

            Refresh();

            Assert.IsNull(handler.SelectedItem);
            Assert.AreEqual(0, handler.SelectedItemIndex);
        }

        // ---------- Scrolling ----------

        [Test]
        public void ScrollDown_SelectsNextItem_AndWrapsToFirst()
        {
            Item a = CreateItem("A");
            Item b = CreateItem("B");
            Item c = CreateItem("C");
            Refresh(a, b, c);

            handler.ScrollItemSelection(-1f);
            Assert.AreSame(b, handler.SelectedItem);
            handler.ScrollItemSelection(-1f);
            Assert.AreSame(c, handler.SelectedItem);
            handler.ScrollItemSelection(-1f);
            Assert.AreSame(a, handler.SelectedItem, "scrolling past the last item wraps to the first");
        }

        [Test]
        public void ScrollUp_SelectsPreviousItem_AndWrapsToLast()
        {
            Item a = CreateItem("A");
            Item b = CreateItem("B");
            Item c = CreateItem("C");
            Refresh(a, b, c);

            handler.ScrollItemSelection(1f);
            Assert.AreSame(c, handler.SelectedItem, "scrolling before the first item wraps to the last");
            handler.ScrollItemSelection(1f);
            Assert.AreSame(b, handler.SelectedItem);
        }

        [Test]
        public void Scroll_WithZeroDeltaOrSingleItem_DoesNothing()
        {
            Item a = CreateItem("A");
            Item b = CreateItem("B");

            Refresh(a, b);
            handler.ScrollItemSelection(0f);
            Assert.AreSame(a, handler.SelectedItem);

            Refresh(a);
            handler.ScrollItemSelection(1f);
            handler.ScrollItemSelection(-1f);
            Assert.AreSame(a, handler.SelectedItem);
            Assert.AreEqual(0, handler.SelectedItemIndex);
        }

        [Test]
        public void Scroll_ChangesWhichItemTheButtonUses()
        {
            Item a = CreateItem("A");
            Item b = CreateItem("B");
            Refresh(a, b);

            handler.ScrollItemSelection(-1f);

            Assert.AreSame(b, handler.GetTarget(InteractButton.F));
        }

        // ---------- TryInteract ----------

        [Test]
        public void TryInteract_InteractsWithTarget()
        {
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 1f);
            Refresh(door);

            Assert.IsTrue(handler.TryInteract(InteractButton.F, interactor));
            Assert.AreEqual(1, door.InteractCount);
        }

        [Test]
        public void TryInteract_PicksUpSelectedItemInsteadOfInteractable()
        {
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 0.5f);
            Item a = CreateItem("A");
            Item b = CreateItem("B");
            Refresh(door, a, b);
            handler.ScrollItemSelection(-1f);

            Assert.IsTrue(handler.TryInteract(InteractButton.F, interactor));

            CollectionAssert.AreEqual(new[] { b }, pickedUp);
            Assert.AreEqual(0, door.InteractCount);
        }

        [Test]
        public void TryInteract_NoTargetForButton_ReturnsFalse()
        {
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 1f);
            Refresh(door);

            Assert.IsFalse(handler.TryInteract(InteractButton.Spacebar, interactor));
            Assert.AreEqual(0, door.InteractCount);
        }

        [Test]
        public void TryInteract_RechecksAvailabilityAtPressTime()
        {
            FakeInteractable door = CreateInteractable("Door", InteractButton.F, 1f);
            Refresh(door);
            door.Interactable = false;   // e.g. became locked after the last refresh

            Assert.IsFalse(handler.TryInteract(InteractButton.F, interactor));
            Assert.AreEqual(0, door.InteractCount);
        }
    }
}
