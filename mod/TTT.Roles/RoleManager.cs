using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Player;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;
using TTT.Public.Player;
using TTT.Round;

namespace TTT.Roles;

public class RoleManager : IRoleService, IPluginBehavior
{
    private const int MaxDetectives = 3;

    private readonly List<IPlayerService> _playerServices = [];
    private int _innocentsLeft;
    private IRoundService _roundService;
    private int _traitorsLeft;
    private InfoManager _infoManager;

    public void Start(BasePlugin parent)
    {
        _roundService = new RoundManager(this, parent);
        _infoManager = new InfoManager(this, parent);
        
        parent.RegisterEventHandler<EventRoundFreezeEnd>(OnRoundStart);
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        parent.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Pre);
        parent.RegisterEventHandler<EventGameStart>(OnMapStart);
        parent.RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect);
    }

    [GameEventHandler]
    private HookResult OnRoundStart(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        _roundService.SetRoundStatus(RoundStatus.Waiting);
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        _playerServices.Add(new GamePlayer(Role.Unassigned, 0, null, null, @event.Userid.UserId.Value));
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnMapStart(EventGameStart @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        info.DontBroadcast = true;
        var attacker = @event.Attacker;
        var target = @event.Userid;

        if (!attacker.IsValid || !target.IsValid) return HookResult.Continue;
        if (_playerServices.All(gamePlayer => gamePlayer.Player() != target)) return HookResult.Continue;
        
        ApplyColorFromRole(target, GetRole(target));
        
        Server.NextFrame(() =>
        {
            Server.PrintToChatAll(StringUtils.FormatTTT($" {GetRole(target).FormatStringFullAfter(" has been found.")}"));
            if (attacker == target) return;
            @event.Userid.PrintToChat(StringUtils.FormatTTT(
                $"You were killed by {GetRole(attacker).FormatStringFullAfter(" " + attacker.PlayerName)}."));
            @event.Attacker.PrintToChat(StringUtils.FormatTTT($"You killed {GetRole(target).FormatStringFullAfter(" " + target.PlayerName)}."));
        });

        if (IsTraitor(target)) _traitorsLeft--;
        if (IsDetective(target) || IsInnocent(target)) _innocentsLeft--;

        if (_traitorsLeft == 0 || _innocentsLeft == 0) _roundService.ForceEnd();

        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid).Where(player => player.IsReal()).ToList();

        foreach (var player in players) player.PrintToCenter(GetWinner().FormatStringFullAfter("s has won!"));

        Clear();
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        _playerServices.RemoveAll(match => match.Player() == @event.Userid);
        return HookResult.Continue;
    }
    
    public void AddRoles()
    {
        var eligible = Utilities.GetPlayers()
            .Where(player => player.PawnIsAlive)
            .Where(player => player.IsReal())
            .Where(player => player.Team != CsTeam.Spectator)
            .ToList();

        var traitorCount = (int)Math.Floor(Convert.ToDouble(eligible.Count / 3));
        var detectiveCount = (int)Math.Floor(Convert.ToDouble(eligible.Count / 4));

        _traitorsLeft = traitorCount;
        _innocentsLeft = eligible.Count - traitorCount;

        if (detectiveCount > MaxDetectives) detectiveCount = MaxDetectives;

        for (var i = 0; i < traitorCount; i++)
        {
            var chosen = eligible[Random.Shared.Next(eligible.Count)];
            eligible.Remove(chosen);
            AddTraitor(chosen);
        }

        for (var i = 0; i < detectiveCount; i++)
        {
            var chosen = eligible[Random.Shared.Next(eligible.Count)];
            eligible.Remove(chosen);
            AddDetective(chosen);
        }

        AddInnocents(eligible);
        SetColors();
    }

    public ISet<CCSPlayerController> GetTraitors()
    {
        return _playerServices.Where(player => player.PlayerRole() == Role.Traitor).Select(player => player.Player()).ToHashSet();
    }

    public ISet<CCSPlayerController> GetDetectives()
    {
        return _playerServices.Where(player => player.PlayerRole() == Role.Detective).Select(player => player.Player()).ToHashSet();
    }

    public ISet<CCSPlayerController> GetInnocents()
    {
        return _playerServices.Where(player => player.PlayerRole() == Role.Innocent).Select(player => player.Player()).ToHashSet();
    }

    public List<IPlayerService> GetPlayers()
    {
        return _playerServices;
    }

    public IPlayerService GetPlayer(CCSPlayerController player)
    {
        return _playerServices[player.UserId.Value];
    }

    public Role GetRole(CCSPlayerController player)
    {
        return _playerServices[player.UserId.Value].PlayerRole();
    }

    public void AddTraitor(CCSPlayerController player)
    {
        _playerServices[player.UserId.Value].SetPlayerRole(Role.Traitor);
        player.SwitchTeam(CsTeam.Terrorist);
        player.PrintToCenter(Role.Traitor.FormatStringFullBefore("You are now a(n)"));
        player.PrintToChat(Role.Traitor.FormatStringFullBefore("You are now a(n)"));
    }

    public void AddDetective(CCSPlayerController player)
    {
        _playerServices[player.UserId.Value].SetPlayerRole(Role.Detective);
        player.SwitchTeam(CsTeam.CounterTerrorist);
        player.PrintToCenter(Role.Detective.FormatStringFullBefore("You are now a(n)"));
        player.GiveNamedItem("weapon_taser");
    }

    public void AddInnocents(IEnumerable<CCSPlayerController> players)
    {
        foreach (var player in players)
        {
            _playerServices[player.UserId.Value].SetPlayerRole(Role.Innocent);
            player.PrintToCenter(Role.Innocent.FormatStringFullBefore("You are now an"));
            player.SwitchTeam(CsTeam.Terrorist);
        }
    }

    public bool IsDetective(CCSPlayerController player)
    {
        return _playerServices.Where(gamePlayer => gamePlayer.Player() == player)
            .Any(gameProfile => gameProfile.PlayerRole() == Role.Detective);
    }

    public bool IsTraitor(CCSPlayerController player)
    {
        return _playerServices.Where(gamePlayer => gamePlayer.Player() == player)
            .Any(gameProfile => gameProfile.PlayerRole() == Role.Traitor);
    }

    public void Clear()
    {
        RemoveColors();

        foreach (var key in _playerServices) key.SetPlayerRole(Role.Unassigned);
    }

    public void ApplyColorFromRole(CCSPlayerController player, Role role)
    {
        switch (role)
        {
            case Role.Traitor:
                ApplyTraitorColor(player);
                break;
            case Role.Detective:
                ApplyDetectiveColor(player);
                break;
            case Role.Innocent:
                ApplyInnocentColor(player);
                break;
            case Role.Unassigned:
            default:
                break;
        }
    }
    
    public bool IsInnocent(CCSPlayerController player)
    {
        return IsTraitor(player) || IsDetective(player);
    }

    private void SetColors()
    {
        foreach (var key in _playerServices)
        {
            if (IsDetective(key.Player()))
                ApplyDetectiveColor(key.Player());

            if (IsTraitor(key.Player()))
                ApplyTraitorColor(key.Player());
        }
    }

    private void RemoveColors()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid).ToList();

        foreach (var player in players)
        {
            if (player.Pawn.Value == null) return;
            player.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
            player.Pawn.Value.Render =  Color.FromArgb(254, 255, 255, 255);
            Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity", "m_clrRender");
        }
    }

    private void ApplyDetectiveColor(CCSPlayerController player)
    {
        if (!player.IsReal() || player.Pawn.Value == null)
            return;

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
        player.Pawn.Value.Render = Color.Blue;
        Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity", "m_clrRender");
    }

    private void ApplyTraitorColor(CCSPlayerController player)
    {
        if (!player.IsReal() || player.Pawn.Value == null)
            return;

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderGlow;
        player.Pawn.Value.Render = Color.Red;
        Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity", "m_clrRender");
        //apply for traitors only somehow?
    }

    private void ApplyInnocentColor(CCSPlayerController player)
    {
        if (!player.IsReal() || player.Pawn.Value == null)
            return;

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderGlow;
        player.Pawn.Value.Render = Color.Green;
        
        Utilities.SetStateChanged(player.Pawn.Value, "CBaseModelEntity", "m_clrRender");
    }

    private Role GetWinner()
    {
        return _traitorsLeft == 0 ? Role.Traitor : Role.Innocent;
    }
}