using CounterStrikeSharp.API.Core;
using MySqlConnector;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Player;

namespace TTT.Public.Database;

public class DatabaseService
{
    private MySqlConnection _connector;

    public DatabaseService()
    {
        _connector = new MySqlConnection();
        _connector.OpenAsync();
        CreateTable();
    }

    public void CreateTable()
    {
        Task.Run(async () =>
        {
            var command = new MySqlCommand("CREATE TABLE IF NOT EXISTS PlayerData(steamid VARCHAR(17) NOT NULL," +
                                           "kills INT," +
                                           "deaths INT," +
                                           "karma INT," +
                                           "traitor_kills INT," +
                                           "traitors_killed INT," +
                                           "PRIMARY KEY(steamid));");
            command.Connection = _connector;
            await command.ExecuteNonQueryAsync();
        });
        
    }

    public GamePlayer CreateProfile(CCSPlayerController player)
    {
        var id = player.SteamID;

        Task.Run(async () =>
        {
            var command = new MySqlCommand("INSERT IGNORE INTO PlayerData(steamid, kills, deaths, karma, traitor_kills, traitors_killed)" +
                                           $" VALUES ({id}, 0, 0, 80, 0, 0);");
            command.Connection = _connector;
            await command.ExecuteNonQueryAsync();
        });
        return new GamePlayer(Role.Unassigned, 800, 0, player.UserId.Value);
    }
}