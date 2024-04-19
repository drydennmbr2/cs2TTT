using System;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Detective;

public class TaserItem : IShopItem
{
    public string Name()
    {
       return "Taser";
    }

    public string SimpleName()
    {
        return "taser";
    }

    public int Price()
    {
        return 1000;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        
        if (player.PlayerRole() != Role.Detective) return BuyResult.IncorrectRole;
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.Taser);
        player.RemoveCredits(Price());
        return BuyResult.Successful;
        
    }
}