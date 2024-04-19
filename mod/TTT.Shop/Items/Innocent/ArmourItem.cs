using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Innocent;

public class ArmourItem : IShopItem
{
    public string Name()
    {
        return "Armour";
    }

    public string SimpleName()
    {
        return "armour";
    }

    public int Price()
    {
        return 500;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.Player().GiveNamedItem(CsItem.KevlarHelmet);
        player.RemoveCredits(Price());
        return BuyResult.Successful;
    }
}