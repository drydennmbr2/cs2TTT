using TTT.Public.Shop.Items;

namespace Shop;

public class TraitorShopHandler : BaseShopHandler
{
 
    private static readonly TraitorShopHandler _instance = new TraitorShopHandler();
    
    public TraitorShopHandler()
    {
        AddShopItem(new AwpItem());
    }
    
    public static TraitorShopHandler Get()
    {
        return _instance;
    }
    
}