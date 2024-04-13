using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items;

public class DNAScannerItem : IShopItem
{
    public string Name()
    {
        return "DNA Scanner";
    }

    public int Price()
    {
        return 800;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        throw new NotImplementedException();
    }
}