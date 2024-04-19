using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class MuteLobbyItem : IShopItem
{
    public string Name()
    {
        return "Silence Everyone";
    }

    public string SimpleName()
    {
        return "SilenceEveryone";
    }

    public int Price()
    {
        return 1000;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        player.RemoveCredits(Price());
        return BuyResult.Successful;
    }
}
//I was thinking we could make this item mute everyone for 10-15 seconds to cause a bit more chaos, and let the T's take advantage of it.