public interface IItemHolder
{
    bool HasItem(ItemData item);
    bool AddItem(ItemData item, int amount = 1);
}
