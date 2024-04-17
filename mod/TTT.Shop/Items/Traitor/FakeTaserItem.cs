using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class FakeTaserItem : IShopItem
{
    public string Name()
    {
        return "Fake Taser";
    }

    public string SimpleName()
    {
        return "faketaser";
    }

    public int Price()
    {
        return 1250;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.Taser);
        return BuyResult.Successful;
    }
}
    