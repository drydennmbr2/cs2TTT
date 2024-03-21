using CounterStrikeSharp.API.Core;
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Public.Mod.Role;
using TTT.Public.Behaviors;
using TTT.Public.Configuration;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Round;
using TTT.Round;

namespace TTT.Roles;

public class RoleManager : IRoleService, IPluginBehavior
{

    private readonly Dictionary<CCSPlayerController, Role> _roles = new();
    private const int MaxDetectives = 3;
    private int _traitorsLeft;
    private int _innocentsLeft;
    private IRoundService _roundService;
    
    public void Start(BasePlugin parent)
    {
        _roundService = new RoundManager(this, parent);
        parent.RegisterEventHandler<EventRoundFreezeEnd>(OnRoundStart);
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        parent.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        parent.RegisterEventHandler<EventGameStart>(OnMapStart);
        parent.RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect);

        
    }
    
    [GameEventHandler]
    private HookResult OnRoundStart(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        AddRoles();
        //_roundService.SetRoundStatus(RoundStatus.Waiting);
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    private HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        _roles.Add(@event.Userid, Role.Unassigned);
        return HookResult.Continue;
    } 

    [GameEventHandler]
    private HookResult OnMapStart(EventGameStart @event, GameEventInfo info)
    {
        //_roundService.TickWaiting();
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        info.DontBroadcast = true;
        var attacker = @event.Attacker;
        var target = @event.Userid;

        if (!attacker.IsValid || !target.IsValid) return HookResult.Continue;
        
        @event.Userid.PrintToChat($"You were killed by {GetRole(attacker).FormatStringAfter(attacker.PlayerName)}.");
        @event.Attacker.PrintToChat($"You killed {GetRole(target).FormatStringAfter(target.PlayerName)}.");
        
        if (IsTraitor(target)) _traitorsLeft--;
        if (IsDetective(target) || IsInnocent(target)) _innocentsLeft--;

        if (_traitorsLeft == 0 || _innocentsLeft == 0)
        {
            _roundService.ForceEnd();
        }

        target.VoiceFlags = VoiceFlags.Muted;
        
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid).Where(player => player.IsReal()).ToList();

        foreach (var player in players)
        {
            player.PrintToCenter(GetWinner().FormatStringFullAfter("s has won!"));
        }
        
        Clear();
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        _roles.Remove(@event.Userid);

        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerHintMessage(EventPlayerHintmessage @event, GameEventInfo info)
    {
        //unused
        return HookResult.Continue;
    }
    
    public void AddRoles() {
        var eligible = Utilities.GetPlayers()
            .Where(player => player.PawnIsAlive)
            .Where(player => player.IsReal())
            .ToList();
    
        if (eligible.Count < 3) {
            _roundService.ForceEnd();
            return;
        }

        var traitorCount = (int)Math.Floor(Convert.ToDouble(eligible.Count / 2));
        var detectiveCount = (int)Math.Floor(Convert.ToDouble(eligible.Count / Config.TTTConfig.DetectiveRatio));

        _traitorsLeft = traitorCount;
        _innocentsLeft = eligible.Count - traitorCount;

        if (detectiveCount > MaxDetectives) {
            detectiveCount = MaxDetectives;
        }
        
        for (var i = 0; i < traitorCount; i++) {
            var chosen = eligible[Random.Shared.Next(eligible.Count)];
            eligible.Remove(chosen); 
            AddTraitor(chosen);
        }

        for (var i = 0; i < detectiveCount; i++) {
            var chosen = eligible[Random.Shared.Next(eligible.Count)];
            eligible.Remove(chosen);
            AddDetective(chosen);
        } 

        AddInnocents(eligible);
        SetColors();
    }

    public Dictionary<CCSPlayerController, Role> GetRoles()
    {
        return _roles;
    }

    public Role GetRole(CCSPlayerController player)
    {
        return !_roles.TryGetValue(player, out var value) ? Role.Unassigned : value;
    }

    public void AddTraitor(CCSPlayerController player)
    {
        _roles.Add(player, Role.Traitor);
        player.SwitchTeam(CsTeam.Terrorist);
        player.PrintToCenter(Role.Traitor.FormatStringFullBefore("You are now a(n)"));
        player.PrintToChat(Role.Traitor.FormatStringFullBefore("You are now a(n)"));
    }
    
    public void AddDetective(CCSPlayerController player)
    {
        _roles.Add(player, Role.Detective);
        player.SwitchTeam(CsTeam.CounterTerrorist);
        player.PrintToChat(Role.Detective.FormatStringFullBefore("You are now a(n)"));
        player.PrintToCenter(Role.Detective.FormatStringFullBefore("You are now a(n)"));
        player.GiveNamedItem("weapon_taser");
    }
    
    public void AddInnocents(IEnumerable<CCSPlayerController> players)
    {
        foreach (var player in players)
        {
            _roles.Add(player, Role.Innocent);
            player.PrintToChat(Role.Innocent.FormatStringFullBefore("You are now an"));
            player.PrintToCenter(Role.Innocent.FormatStringFullBefore("You are now an"));
            player.SwitchTeam(CsTeam.Terrorist);
        }
    }

    public bool IsDetective(CCSPlayerController player)
    {
        return _roles[player] == Role.Detective;
    }

    public bool IsTraitor(CCSPlayerController player)
    {
        return _roles[player] == Role.Traitor;
    }

    public bool IsInnocent(CCSPlayerController player)
    {
        return _roles[player] == Role.Innocent;
    }

    public void Clear()
    {
        RemoveColors();

        foreach (var key in _roles.Keys.ToList())
        {
            _roles[key] = Role.Unassigned;
        }
    }

    private void SetColors()
    {
        foreach (var pair in _roles)
        {
            if (IsDetective(pair.Key))
                ApplyDetectiveColor(pair.Key);
            
            if (IsTraitor(pair.Key))
                ApplyTraitorColor(pair.Key);
        }
    }

    private void RemoveColors()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid).ToList();

        foreach (var player in players)
        {
            if (player.Pawn.Value == null) return;
            player.Pawn.Value.RenderMode = RenderMode_t.kRenderNormal;
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


    private Role GetWinner()
    {
        return _traitorsLeft == 0 ? Role.Traitor : Role.Innocent;
    }
    
}