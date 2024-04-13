using TTT.Public.Behaviors;
using TTT.Public.Player;
using TTT.Public.Shop;
using TTT.Public.Shop.Items;

namespace Shop;

public class BaseShopHandler : IShopItemHandler, IPluginBehavior
{
    private readonly ISet<IShopItem> _items = new HashSet<IShopItem>();

    private static readonly BaseShopHandler _instance = new BaseShopHandler();

    protected BaseShopHandler()
    {
        OnLoad();
    }
    
    public void OnLoad()
    {
        AddShopItem(new TaserItem());
        AddShopItem(new RoleItem());
    }
    
    public static BaseShopHandler Get()
    {
        return _instance;
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