using TTT.Public.Behaviors;
using TTT.Public.Shop;
using TTT.Shop.Items;

namespace TTT.Shop;

public class BaseShopHandler : IShopItemHandler, IPluginBehavior
{
    private readonly ISet<IShopItem> _items = new HashSet<IShopItem>();

    private static readonly BaseShopHandler Instance = new BaseShopHandler();

    protected BaseShopHandler()
    {
        OnLoad();
    }
    
    public void OnLoad()
    {
        AddShopItem(new RoleItem());
    }
    
    public static BaseShopHandler Get()
    {
        return Instance;
    }

    public ISet<IShopItem> GetShopItems()
    {
        return _items;
    }

    public void AddShopItem(IShopItem item)
    {
        _items.Add(item);
    }

    
}