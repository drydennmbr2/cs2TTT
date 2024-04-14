## cs2TTT

## Contributing
TTT is in heavy development and I want you to know that contributions are always welcome. Please follow Microsoft's dependency injection system.

> [!TIP]
> Microsoft has some good documentation on dependency injection here: 
> [Overview](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection),
> [Using Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage),
> [Dependency Injection Guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines).

## Creating items

Creating new items or modifying existing ones is easy. Create a new class in the correct directory, mod/TTT.Shop/Items/{group}. Then modify it to your liking. Afterwards, compile the plugin and it's all set. The plugin handles loading all the items.

> [!TIP]
> Available groups are [All, Detective, Traitor].

```c#
namespace TTT.Shop.Items.Traitor;

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

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price()) return BuyResult.NotEnoughCredits;
        if (player.PlayerRole() != Role.Detective) return BuyResult.IncorrectRole;
        player.RemoveCredits(Price());
        player.Player().GiveNamedItem(CsItem.AWP);
        return BuyResult.Successful;
    }
}
```

## Base Release
- [ ] ~~Team assignemnt~~
- [ ] ~~Models on floor~~
- [ ] ~~ID models~~
- [ ] ~~Detective scan~~
     - [ ] Add states
- [ ] ~~Tazer~~
- [ ] Distinguish Traitors
     - [ ] Needs a fix
- [ ] ~~Scoreboard~~
- [ ] ~~Killfeed~~
- [ ] ~~Localization~~
- [ ] ~~Can't spawn in the middle of the round~~
- [ ] ~~Early round rdm~~
- [ ] ~~Logs~~

## After Base
- [ ] Shop
- [ ] Karma
- [ ] RDM Manager
- [ ] Add database support for logs and stats
