using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class USPItem : IShopItem
{
    public string Name()
    {
        return "USP-S";
    }

    public string SimpleName()
    {
        return "USP";
    }

    public int Price()
    {
        return 250;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.USPS);
        player.RemoveCredits(Price());
        return BuyResult.Successful;
    }
}