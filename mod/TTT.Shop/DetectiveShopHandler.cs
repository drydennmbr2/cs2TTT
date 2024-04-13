
using TTT.Shop.Items;

namespace TTT.Shop;

public class DetectiveShopHandler : BaseShopHandler
{
    private static readonly DetectiveShopHandler _instance = new DetectiveShopHandler();
    
    public DetectiveShopHandler()
    {
        AddShopItem(new TaserItem());
    }
    
    public static DetectiveShopHandler Get()
    {
        return _instance;
    }
}