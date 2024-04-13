using System;
using CounterStrikeSharp.API.Core;
using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items;

public class RoleItem : IShopItem
{
    public string Name()
    {
        return "Role Item";
    }

    public int Price()
    {
        return 0;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        return BuyResult.Successful;
    }
}