using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;

namespace TTT.Public.Shop.Items;

public class AwpItem : IShopItem
{
    public string Name()
    {
        return "AWP";
    }

    public int Price()
    {
        return 2000;
    }

    public bool OnBuy(GamePlayer player)
    {
        throw new NotImplementedException();
    }

    public bool OnBuy(CCSPlayerController player)
    {
        if (false) return true;
        player.DropActiveWeapon();
        player.GiveNamedItem(CsItem.AWP);
        return true;
    }
}