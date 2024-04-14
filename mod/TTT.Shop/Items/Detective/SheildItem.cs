using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Detective;

public class SheildItem : IShopItem
{
    public string Name()
    {
        return "Sheild";
    }

    public string SimpleName()
    {
        return "Sheild";
    }

    public int Price()
    {
        return 550;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.Shield);
        return BuyResult.Successful;
    }
}