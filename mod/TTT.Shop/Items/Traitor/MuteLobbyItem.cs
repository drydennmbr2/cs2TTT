using TTT.Player;
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
        if (player.Credits() < Price())
            return BuyResult.NotEnoughCredits;
        return BuyResult.Successful;
    }
}
//I was thinking we could make this item mute everyone to cause a bit more chaos, and let the T's take advantage of it.