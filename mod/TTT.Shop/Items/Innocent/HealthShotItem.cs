using System.Net.Security;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Innocent;

public class HealthShotItem : IShopItem
{
    public string Name()
    {
        return "Health Shot";
    }

    public string SimpleName()
    {
        return "HealthShot";
    }

    public int Price()
    {
        return 250;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.Healthshot);
        return BuyResult.Successful;
    }
}