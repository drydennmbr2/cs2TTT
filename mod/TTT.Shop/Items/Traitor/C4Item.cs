﻿using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class C4Item : IShopItem
{
    public string Name()
    {
        return "Jihad Bomb";
    }

    public string SimpleName()
    {
        return "JihadBomb";
    }

    public int Price()
    {
        return 750;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.C4);
        player.RemoveCredits(Price());
        return BuyResult.Successful;

    }
}