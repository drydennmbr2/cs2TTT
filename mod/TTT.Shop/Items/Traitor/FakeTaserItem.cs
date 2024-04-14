using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class FakeTaserItem : IShopItem
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
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.Taser);
        return BuyResult.Successful;
    }
}
    