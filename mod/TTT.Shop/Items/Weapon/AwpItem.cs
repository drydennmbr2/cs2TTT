using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items;

public class AwpItem : IShopItem
{
    public string Name()
    {
        return "AWP";
    }

    public int Price()
    {
        return 2000;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price()) return BuyResult.NotEnoughCredits;
        if (player.PlayerRole() != Role.Detective) return BuyResult.IncorrectRole;
        player.RemoveCredits(Price());
        player.Player().GiveNamedItem(CsItem.AWP);
        return BuyResult.Successful;
    }

    
}