using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using TTT.Player;
using TTT.Public.Formatting;
using TTT.Public.Player;

namespace TTT.Public.Shop;

public class ShopMenu
{
    private int _currentPage;
    private readonly IShopItemHandler _shopItemHandler;
    private readonly IPlayerService _playerService;
    private readonly BaseMenu _menu = new ChatMenu("Shop Menu");

    public ShopMenu(IShopItemHandler shopItemHandler, IPlayerService playerService, int currentPage = 0)
    {
        _currentPage = currentPage;
        _shopItemHandler = shopItemHandler;
        _playerService = playerService;
    }

    public void BuyItem(GamePlayer player, IShopItem item)
    {
        var successful = item.OnBuy(player);

        player.Player().PrintToChat(successful
            ? StringUtils.FormatTTT($"You bought {item.Name()} for ${item.Price()}")
            : StringUtils.FormatTTT($"You don't have enough credits to buy {item.Name()}"));
    }

    public void BuyItem(GamePlayer player, int index)
    {
        var item = _shopItemHandler.GetShopItems().ElementAt(index);
        BuyItem(player, item);
    }

    public void BuyItem(GamePlayer player, string name)
    {
        foreach (var item in _shopItemHandler.GetShopItems())
        {
            if (!item.Name().Equals(name)) continue;
            BuyItem(player, item);
            return;
        }
    }

    public void NextPage()
    {
        _currentPage++;
        ShowPage();
    }

    public void ShowPage()
    {
        for (var index = 1 * _currentPage; index < index + 9; index++)
        {
            var item = _shopItemHandler.GetShopItems().ElementAt(index);
            _menu.AddMenuOption(item.Name() + $" - {item.Price()} credits", (player, option) => BuyItem(_playerService.GetPlayer(player), item));
        }
    }
}