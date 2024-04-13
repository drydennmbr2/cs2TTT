using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using TTT.Player;
using TTT.Public.Formatting;
using TTT.Public.Player;

namespace TTT.Public.Shop;

public class ShopMenu
{
    private readonly ChatMenu _menu = new("Shop Menu");
    private readonly IPlayerService _playerService;
    private readonly IShopItemHandler _shopItemHandler;
    private int _currentPage;

    public ShopMenu(IShopItemHandler shopItemHandler, IPlayerService playerService, int currentPage = 0)
    {
        _currentPage = currentPage;
        _shopItemHandler = shopItemHandler;
        _playerService = playerService;
        Create();
    }

    public void BuyItem(GamePlayer player, IShopItem item)
    {
        var successful = item.OnBuy(player);
        switch (successful)
        {
            //print message from enum
            case BuyResult.NotEnoughCredits:
                player.Player()
                    .PrintToChat(StringUtils.FormatTTT($"You don't have enough credits to buy {item.Name()}"));
                break;
            case BuyResult.Successful:
                player.Player().PrintToChat(StringUtils.FormatTTT($"You have bought {item.Name()}"));
                break;
            case BuyResult.AlreadyOwned:
                player.Player().PrintToChat(StringUtils.FormatTTT($"You already own {item.Name()}"));
                break;
            case BuyResult.IncorrectRole:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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

    public void Create()
    {
        for (var index = 0; index < _shopItemHandler.GetShopItems().Count; index++)
        {
            var item = _shopItemHandler.GetShopItems().ElementAt(index);
            _menu.AddMenuOption(item.Name() + $" - {item.Price()} credits",
                (player, option) => BuyItem(_playerService.GetPlayer(player), item));
        }
    }
    
    public void Open(CCSPlayerController player)
    {
        MenuManager.OpenChatMenu(player, _menu);
    }
}