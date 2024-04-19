using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class HurtStationItem : IShopItem
{
    public string Name()
    {
        return "Hurt Station";
    }

    public string SimpleName()
    {
        return "hurtstation";  //does damage to nearby people over time i'd say 15 damage per second within 5-10 meters?
    }

    public int Price()
    {
        return 1250;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        player.RemoveCredits(Price());
        return player.Credits() < Price() ? BuyResult.NotEnoughCredits : BuyResult.Successful;
    }
}