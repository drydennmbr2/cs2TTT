using CounterStrikeSharp.API.Core;
using TTT.Player;

namespace TTT.Public.Shop;

public interface IShopItem
{
    string Name();
    string SimpleName();
    int Price();
    BuyResult OnBuy(GamePlayer player);
}