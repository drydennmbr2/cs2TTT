using TTT.Player;

namespace TTT.Public.Shop.Items;

public class TaserItem : IShopItem
{
    public string Name()
    {
       return "Taser";
    }

    public int Price()
    {
        return 1000;
    }

    public bool OnBuy(GamePlayer player)
    {
        throw new NotImplementedException();
    }
}