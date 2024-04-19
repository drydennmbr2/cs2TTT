using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class M4A1Item : IShopItem
{
    public string Name()
    {
        return "M4A1-S";
    }

    public string SimpleName()
    {
        return "m4a1";
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
        player.Player().GiveNamedItem(CsItem.M4A1);
        player.RemoveCredits(Price());
        return BuyResult.Successful;
    }
}