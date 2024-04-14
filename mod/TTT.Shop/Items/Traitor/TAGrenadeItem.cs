﻿using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class TAGrenadeItem : IShopItem
{
    public string Name()
    {
        return "TA-Grenade";
    }

    public string SimpleName()
    {
        return "TAGrenade";
    }

    public int Price()
    {
        return 200;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.TAGrenade);
        return BuyResult.Successful;
    }
}