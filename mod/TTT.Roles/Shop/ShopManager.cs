using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Player;
using TTT.Public.Shop;
using TTT.Shop;

namespace TTT.Roles.Shop;

public class ShopManager
{
    private readonly ShopMenu _innocentShopManager;
    private readonly ShopMenu _detectiveShopManager;
    private readonly ShopMenu _traitorShopManager;
    private readonly IPlayerService _playerService;
    private static ShopManager Manager;
    
    private ShopManager(BasePlugin plugin, IPlayerService manager)
    {
        _playerService = manager;
        _innocentShopManager = new ShopMenu(BaseShopHandler.Get(), manager);
        _detectiveShopManager = new ShopMenu(DetectiveShopHandler.Get(), manager);
        _traitorShopManager = new ShopMenu(TraitorShopHandler.Get(), manager);
        plugin.AddCommand("css_shop", "Open the shop menu", OnShopCommand);
        plugin.AddCommand("css_buy", "Buys specified item", OnBuyCommand);
    }
    
    public static void Register(BasePlugin plugin, IPlayerService manager)
    {
        Manager = new ShopManager(plugin, manager);
    }
    
    public void OpenShop(GamePlayer player)
    {
        var role = player.PlayerRole();
        switch (role)
        {
            case Role.Innocent:
                _innocentShopManager.Open(player.Player());
                player.SetShopOpen(true);
                break;
            case Role.Detective:
                _detectiveShopManager.Open(player.Player());
                player.SetShopOpen(true);
                break;
            case Role.Traitor:
                _traitorShopManager.Open(player.Player());
                player.SetShopOpen(true);
                break;
            case Role.Unassigned:
                break;
        }
    }
    
    public void OnShopCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            info.ReplyToCommand("Can only be executed by a player!");
            return;
        }
        OpenShop(_playerService.GetPlayer(player));
    }
    
    public void OnBuyCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            info.ReplyToCommand("Can only be executed by a player!");
            return;
        }

        var item = info.GetArg(1);
        
        if (string.IsNullOrEmpty(item))
        {
            info.ReplyToCommand("Please specify an item to buy!");
            return;
        }
        
        var gamePlayer = _playerService.GetPlayer(player);

        switch (gamePlayer.PlayerRole())
        {
            case Role.Traitor:
                _traitorShopManager.BuyItem(gamePlayer, item);
                break;
            case Role.Detective:
                _detectiveShopManager.BuyItem(gamePlayer, item);
                break;
            case Role.Innocent:
                _innocentShopManager.BuyItem(gamePlayer, item);
                break;
            case Role.Unassigned:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}