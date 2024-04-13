using TTT.Player;
using TTT.Public.Shop;

namespace Shop.Items;

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

    public bool OnBuy(GamePlayer player)
    {
        throw new NotImplementedException();
    }
}