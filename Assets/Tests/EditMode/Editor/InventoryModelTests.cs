using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace EditModeTests
{
    public class InventoryModelTests
    {
        private readonly List<ItemData> createdItems = new();

        private ItemData apple;  // maxStack 5
        private ItemData coin;   // maxStack 10
        private ItemData key;    // maxStack 1

        private int changedCount;
        private readonly List<ItemData> addedItems = new();

        [SetUp]
        public void SetUp()
        {
            apple = CreateItem("Apple", 5);
            coin = CreateItem("Coin", 10);
            key = CreateItem("Key", 1);
            changedCount = 0;
            addedItems.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (ItemData item in createdItems)
                Object.DestroyImmediate(item);
            createdItems.Clear();
        }

        private ItemData CreateItem(string itemName, int maxStack)
        {
            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.name = itemName;
            item.itemName = itemName;
            item.maxStack = maxStack;
            createdItems.Add(item);
            return item;
        }

        private InventoryModel CreateModel(int capacity)
        {
            var model = new InventoryModel(capacity);
            model.Changed += () => changedCount++;
            model.ItemAdded += addedItems.Add;
            return model;
        }

        private static void AssertSlot(InventoryModel model, int index, ItemData item, int stack)
        {
            InventoryModel.Slot slot = model.Slots[index];
            Assert.AreSame(item, slot.Item, $"slot {index} item");
            Assert.AreEqual(stack, slot.Stack, $"slot {index} stack");
        }

        private static void AssertEmpty(InventoryModel model, int index)
        {
            InventoryModel.Slot slot = model.Slots[index];
            Assert.IsTrue(slot.IsEmpty, $"slot {index} should be empty");
            Assert.IsNull(slot.Item, $"slot {index} item");
            Assert.AreEqual(0, slot.Stack, $"slot {index} stack");
        }

        // ---------- Construction ----------

        [Test]
        public void NewModel_HasRequestedCapacity_AllSlotsEmpty()
        {
            var model = CreateModel(4);

            Assert.AreEqual(4, model.Capacity);
            Assert.AreEqual(4, model.Slots.Count);
            for (int i = 0; i < model.Capacity; i++)
                AssertEmpty(model, i);
        }

        [Test]
        public void NewModel_CapacityBelowOne_IsClampedToOne()
        {
            Assert.AreEqual(1, new InventoryModel(0).Capacity);
            Assert.AreEqual(1, new InventoryModel(-3).Capacity);
        }

        [Test]
        public void MaxStackOf_TreatsMissingOrInvalidMaxStackSafely()
        {
            Assert.AreEqual(0, InventoryModel.MaxStackOf(null));
            Assert.AreEqual(5, InventoryModel.MaxStackOf(apple));
            Assert.AreEqual(1, InventoryModel.MaxStackOf(CreateItem("Broken", 0)));
        }

        // ---------- AddItem / stacking ----------

        [Test]
        public void AddItem_IntoEmptyInventory_UsesFirstSlot()
        {
            var model = CreateModel(3);

            Assert.IsTrue(model.AddItem(apple));

            AssertSlot(model, 0, apple, 1);
            AssertEmpty(model, 1);
            AssertEmpty(model, 2);
        }

        [Test]
        public void AddItem_SameItem_StacksInsteadOfUsingNewSlot()
        {
            var model = CreateModel(3);

            model.AddItem(apple, 3);
            model.AddItem(apple, 2);

            AssertSlot(model, 0, apple, 5);
            AssertEmpty(model, 1);
        }

        [Test]
        public void AddItem_MoreThanMaxStack_SpillsIntoNextEmptySlot()
        {
            var model = CreateModel(3);

            Assert.IsTrue(model.AddItem(apple, 7));

            AssertSlot(model, 0, apple, 5);
            AssertSlot(model, 1, apple, 2);
            AssertEmpty(model, 2);
        }

        [Test]
        public void AddItem_TopsUpExistingPartialStackBeforeUsingEarlierEmptySlot()
        {
            var model = CreateModel(3);
            model.AddItem(key);
            model.AddItem(apple, 2);   // slot 1
            model.ClearSlot(0);        // slot 0 is now empty, slot 1 holds a partial apple stack

            model.AddItem(apple, 3);

            AssertEmpty(model, 0);
            AssertSlot(model, 1, apple, 5);
        }

        [Test]
        public void AddItem_MaxStackOne_PutsEachUnitInItsOwnSlot()
        {
            var model = CreateModel(3);

            Assert.IsTrue(model.AddItem(key, 2));

            AssertSlot(model, 0, key, 1);
            AssertSlot(model, 1, key, 1);
            AssertEmpty(model, 2);
        }

        [Test]
        public void AddItem_ExactlyFillsInventory_Succeeds()
        {
            var model = CreateModel(2);

            Assert.IsTrue(model.AddItem(apple, 10));

            AssertSlot(model, 0, apple, 5);
            AssertSlot(model, 1, apple, 5);
            Assert.IsFalse(model.CanAdd(apple));
        }

        [Test]
        public void AddItem_NotEnoughRoom_ReturnsFalseAndChangesNothing()
        {
            var model = CreateModel(2);
            model.AddItem(key);        // only slot 1 (5 apples of room) is left
            changedCount = 0;
            addedItems.Clear();

            Assert.IsFalse(model.AddItem(apple, 6));

            AssertSlot(model, 0, key, 1);
            AssertEmpty(model, 1);
            Assert.AreEqual(0, changedCount, "Changed must not fire on a rejected add");
            CollectionAssert.IsEmpty(addedItems, "ItemAdded must not fire on a rejected add");
        }

        [Test]
        public void AddItem_NotEnoughRoomEvenWithPartialStack_DoesNotPartiallyTopUp()
        {
            var model = CreateModel(1);
            model.AddItem(apple, 3);

            Assert.IsFalse(model.AddItem(apple, 3));   // only 2 fit

            AssertSlot(model, 0, apple, 3);
        }

        [Test]
        public void AddItem_InvalidArguments_ReturnFalse()
        {
            var model = CreateModel(2);

            Assert.IsFalse(model.AddItem(null));
            Assert.IsFalse(model.AddItem(apple, 0));
            Assert.IsFalse(model.AddItem(apple, -1));

            AssertEmpty(model, 0);
            Assert.AreEqual(0, changedCount);
            CollectionAssert.IsEmpty(addedItems);
        }

        [Test]
        public void CanAdd_CountsRoomInEmptySlotsAndMatchingPartialStacks()
        {
            var model = CreateModel(2);
            model.AddItem(apple, 3);   // 2 room left in slot 0, 5 in slot 1

            Assert.IsTrue(model.CanAdd(apple, 7));
            Assert.IsFalse(model.CanAdd(apple, 8));
            Assert.IsTrue(model.CanAdd(coin, 10), "only the empty slot counts for a different item");
            Assert.IsFalse(model.CanAdd(coin, 11));
        }

        // ---------- HasItem / IItemHolder ----------

        [Test]
        public void HasItem_ReflectsContents()
        {
            var model = CreateModel(2);

            Assert.IsFalse(model.HasItem(null));
            Assert.IsFalse(model.HasItem(apple));

            model.AddItem(apple);
            Assert.IsTrue(model.HasItem(apple));
            Assert.IsFalse(model.HasItem(coin));

            model.UseOne(0);
            Assert.IsFalse(model.HasItem(apple), "using the last one removes the item");
        }

        [Test]
        public void Model_WorksThroughIItemHolderAbstraction()
        {
            IItemHolder holder = CreateModel(1);

            Assert.IsTrue(holder.AddItem(key));
            Assert.IsTrue(holder.HasItem(key));
            Assert.IsFalse(holder.AddItem(key), "inventory is full");
        }

        // ---------- UseOne / ClearSlot ----------

        [Test]
        public void UseOne_DecrementsStack()
        {
            var model = CreateModel(2);
            model.AddItem(apple, 3);
            changedCount = 0;

            model.UseOne(0);

            AssertSlot(model, 0, apple, 2);
            Assert.AreEqual(1, changedCount);
        }

        [Test]
        public void UseOne_OnLastUnit_EmptiesSlot()
        {
            var model = CreateModel(2);
            model.AddItem(key);

            model.UseOne(0);

            AssertEmpty(model, 0);
        }

        [Test]
        public void UseOne_OnEmptyOrInvalidSlot_DoesNothing()
        {
            var model = CreateModel(2);

            Assert.DoesNotThrow(() =>
            {
                model.UseOne(0);
                model.UseOne(-1);
                model.UseOne(99);
            });

            Assert.AreEqual(0, changedCount);
        }

        [Test]
        public void ClearSlot_RemovesWholeStack()
        {
            var model = CreateModel(2);
            model.AddItem(apple, 4);
            changedCount = 0;

            model.ClearSlot(0);

            AssertEmpty(model, 0);
            Assert.IsFalse(model.HasItem(apple));
            Assert.AreEqual(1, changedCount);
        }

        [Test]
        public void ClearSlot_InvalidIndex_DoesNothing()
        {
            var model = CreateModel(2);
            model.AddItem(apple);
            changedCount = 0;

            Assert.DoesNotThrow(() =>
            {
                model.ClearSlot(-1);
                model.ClearSlot(2);
            });

            AssertSlot(model, 0, apple, 1);
            Assert.AreEqual(0, changedCount);
        }

        // ---------- Move (drag & drop) ----------

        [Test]
        public void Move_ToEmptySlot_MovesStack()
        {
            var model = CreateModel(3);
            model.AddItem(apple, 3);
            changedCount = 0;

            model.Move(0, 2);

            AssertEmpty(model, 0);
            AssertSlot(model, 2, apple, 3);
            Assert.AreEqual(1, changedCount);
        }

        [Test]
        public void Move_OntoDifferentItem_SwapsSlots()
        {
            var model = CreateModel(2);
            model.AddItem(apple, 3);
            model.AddItem(coin, 7);

            model.Move(0, 1);

            AssertSlot(model, 0, coin, 7);
            AssertSlot(model, 1, apple, 3);
        }

        [Test]
        public void Move_OntoSameItem_MergesWhatFits()
        {
            var model = CreateModel(2);
            model.AddItem(apple, 8);   // 5 + 3
            model.UseOne(0);
            model.UseOne(0);           // 3 + 3

            model.Move(1, 0);

            AssertSlot(model, 0, apple, 5);
            AssertSlot(model, 1, apple, 1);
        }

        [Test]
        public void Move_OntoSameItem_WhenEverythingFits_EmptiesSource()
        {
            var model = CreateModel(2);
            model.AddItem(apple, 8);   // 5 + 3
            model.UseOne(0);
            model.UseOne(0);           // 3 + 3
            model.UseOne(1);           // 3 + 2

            model.Move(1, 0);

            AssertSlot(model, 0, apple, 5);
            AssertEmpty(model, 1);
        }

        [Test]
        public void Move_OntoFullStackOfSameItem_ChangesNothing()
        {
            var model = CreateModel(2);
            model.AddItem(apple, 7);   // 5 + 2
            changedCount = 0;

            model.Move(1, 0);

            AssertSlot(model, 0, apple, 5);
            AssertSlot(model, 1, apple, 2);
            Assert.AreEqual(0, changedCount);
        }

        [Test]
        public void Move_InvalidMoves_AreIgnored()
        {
            var model = CreateModel(2);
            model.AddItem(apple, 2);
            changedCount = 0;

            model.Move(1, 0);    // from empty slot
            model.Move(0, 0);    // onto itself
            model.Move(-1, 0);   // invalid source
            model.Move(0, 5);    // invalid target

            AssertSlot(model, 0, apple, 2);
            AssertEmpty(model, 1);
            Assert.AreEqual(0, changedCount);
        }

        // ---------- Sort ----------

        /// <summary>Slots: [Key 1] [Apple 3] [Apple 2] [Coin 4] - unsorted, with split apple stacks.</summary>
        private InventoryModel CreateMessyInventory()
        {
            var model = CreateModel(5);
            model.AddItem(key);
            model.AddItem(apple, 7);   // slot 1: 5, slot 2: 2
            model.UseOne(1);
            model.UseOne(1);           // slot 1: 3
            model.AddItem(coin, 4);    // slot 3
            changedCount = 0;
            return model;
        }

        [Test]
        public void Sort_Ascending_OrdersByNameAndMergesPartialStacks()
        {
            var model = CreateMessyInventory();

            model.Sort();

            AssertSlot(model, 0, apple, 5);
            AssertSlot(model, 1, coin, 4);
            AssertSlot(model, 2, key, 1);
            AssertEmpty(model, 3);
            AssertEmpty(model, 4);
            Assert.AreEqual(1, changedCount);
        }

        [Test]
        public void Sort_Descending_ReversesOrder()
        {
            var model = CreateMessyInventory();

            model.Sort(ascending: false);

            AssertSlot(model, 0, key, 1);
            AssertSlot(model, 1, coin, 4);
            AssertSlot(model, 2, apple, 5);
            AssertEmpty(model, 3);
        }

        [Test]
        public void Sort_TotalAboveMaxStack_SplitsIntoFullStackThenRemainder()
        {
            var model = CreateModel(4);
            model.AddItem(coin);
            model.AddItem(apple, 7);   // slots 1-2: 5 + 2
            model.UseOne(1);           // 4 + 2 = 6 apples in total

            model.Sort();

            AssertSlot(model, 0, apple, 5);
            AssertSlot(model, 1, apple, 1);
            AssertSlot(model, 2, coin, 1);
            AssertEmpty(model, 3);
        }

        [Test]
        public void Sort_KeepsTotalAmountOfEveryItem()
        {
            var model = CreateMessyInventory();

            model.Sort();

            int Total(ItemData item)
            {
                int sum = 0;
                foreach (InventoryModel.Slot slot in model.Slots)
                    if (slot.Item == item) sum += slot.Stack;
                return sum;
            }

            Assert.AreEqual(5, Total(apple));
            Assert.AreEqual(4, Total(coin));
            Assert.AreEqual(1, Total(key));
        }

        // ---------- Events ----------

        [Test]
        public void AddItem_FiresItemAddedAndChangedOncePerCall()
        {
            var model = CreateModel(3);

            model.AddItem(apple, 7);   // spans two slots, still one call

            Assert.AreEqual(1, changedCount);
            CollectionAssert.AreEqual(new[] { apple }, addedItems);
        }

        [Test]
        public void EveryMutation_FiresChanged()
        {
            var model = CreateModel(3);
            model.AddItem(apple, 2);
            model.AddItem(coin, 2);
            Assert.AreEqual(2, changedCount);

            model.UseOne(0);
            model.Move(0, 2);
            model.Sort();
            model.ClearSlot(0);

            Assert.AreEqual(6, changedCount);
        }
    }
}
