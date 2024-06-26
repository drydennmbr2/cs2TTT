﻿using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class AssaultSuitItem
{
    public string Name()
    {
        return "Assault Suit";
    }

    public string SimpleName()
    {
        return "AssaultSuit";
    }

    public int Price()
    {
        return 2000;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.AssaultSuit);
        player.RemoveCredits(Price());
        return BuyResult.Successful;

    }

}
//needs to be tested, not sure if these still work.