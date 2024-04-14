using System.Collections.Generic;
using System.Reflection;
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
        AddItems("All");
        AddItems("Innocent");
    }

    protected void AddItems(string name)
    {
        var fullName = "TTT.Shop.Items" + name;
        var q = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == fullName && t.GetInterface("IShopItem") != null
            select t;

        foreach (var type in q)
        {
            if (type == null) return;
            var item = (IShopItem?) Activator.CreateInstance(type);
            if (item == null) return;
            AddShopItem(item);
        }
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