using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;


namespace TTT.Shop.Items.Detective;

public class HealthStationItem : IShopItem
{
    public string Name()
    {
        return "Health Station";
    }

    public string SimpleName()
    {
        return "Healthstation";  //health to nearby people over time I'd say 5 health per second within 5 meters?
    }

    public int Price()
    {
        return 1500;
    
        
        
    }
    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.PlayerRole() != Role.Detective) return BuyResult.IncorrectRole;
        player.RemoveCredits(Price());
        return player.Credits() < Price() ? BuyResult.NotEnoughCredits : BuyResult.Successful;
    }
}