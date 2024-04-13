using CounterStrikeSharp.API.Core;
using TTT.Player;

namespace TTT.Public.Shop;

public interface IShopItem
{
    string Name();
    int Price();
    bool OnBuy(GamePlayer player);
}