using System;
using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items;

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

    public BuyResult OnBuy(GamePlayer player)
    {
        return BuyResult.Successful;
    }
}