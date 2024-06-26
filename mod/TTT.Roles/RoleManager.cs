﻿using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using TTT.Player;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;
using TTT.Roles.Shop;
using TTT.Round;

namespace TTT.Roles;

public class RoleManager : PlayerHandler, IRoleService, IPluginBehavior
{
    private const int MaxDetectives = 3;

    private int _innocentsLeft;
    private IRoundService _roundService;
    private int _traitorsLeft;
    private InfoManager _infoManager;
    
    public void Start(BasePlugin parent)
    {
        _roundService = new RoundManager(this, parent);
        _infoManager = new InfoManager(this, _roundService, parent);
        ModelHandler.RegisterListener(parent);
        ShopManager.Register(parent, this); //disabled until items are implemented.
        CreditManager.Register(parent, this);
        
        parent.RegisterListener<Listeners.OnEntitySpawned>((entity) =>
        {
            if (entity.IsValid) return;
            if (entity is not CRagdollProp prop) return;
            
            if (prop.OwnerEntity.Value == null) return;
            if (prop.OwnerEntity.Value is not CCSPlayerController player) return;
            
            Server.PrintToConsole(player.PlayerName);
            GetPlayer(player).SetRagdollProp(prop);
        });
        
        parent.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        parent.RegisterEventHandler<EventRoundFreezeEnd>(OnRoundStart);
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        parent.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Pre);
        parent.RegisterEventHandler<EventGameStart>(OnMapStart);
    }

    [GameEventHandler]
    private HookResult OnRoundStart(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        _roundService.SetRoundStatus(RoundStatus.Waiting);
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (Utilities.GetPlayers().Count(player => player.IsReal() && player.Team != CsTeam.None || player.Team == CsTeam.Spectator) <= 3)
        {
            _roundService.ForceEnd();
        }
        
        CreatePlayer(@event.Userid);
        
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
        
        if (!attacker.IsReal() || !target.IsReal()) return HookResult.Continue;
        
        if (IsTraitor(target)) _traitorsLeft--;
        
        if (IsDetective(target) || IsInnocent(target)) _innocentsLeft--;

        Server.NextFrame(() =>
        {
            Server.PrintToChatAll(StringUtils.FormatTTT($"{GetRole(target).FormatStringFullAfter(" has been found.")}"));
            if (attacker == target) return;
        
            target.PrintToChat(StringUtils.FormatTTT(
                $"You were killed by {GetRole(attacker).FormatStringFullAfter(" " + attacker.PlayerName)}."));
            attacker.PrintToChat(StringUtils.FormatTTT($"You killed {GetRole(target).FormatStringFullAfter(" " + target.PlayerName)}."));
        });
        
        GetPlayer(target).SetKiller(attacker);
        
        if (_traitorsLeft == 0 || _innocentsLeft == 0) Server.NextFrame(() => _roundService.ForceEnd());

        
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid).Where(player => player.IsReal()).ToList();

        foreach (var player in players) player.PrintToCenter(GetWinner().FormatStringFullAfter("s has won!"));

        Server.NextFrame(Clear);
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        Server.NextFrame(() =>
        {
            RemovePlayer(player);
            if (GetPlayers().Count == 0) _roundService.SetRoundStatus(RoundStatus.Paused);
        });
        
        
        return HookResult.Continue;
    }
    
    public void AddRoles()
    {
        var eligible = Utilities.GetPlayers()
            .Where(player => player.PawnIsAlive)
            .Where(player => player.IsReal())
            .Where(player => player.Team is not (CsTeam.Spectator or CsTeam.None))
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
        return Players().Where(player => player.PlayerRole() == Role.Traitor).Select(player => player.Player()).ToHashSet();
    }

    public ISet<CCSPlayerController> GetDetectives()
    {
        return Players().Where(player => player.PlayerRole() == Role.Detective).Select(player => player.Player()).ToHashSet();
    }

    public ISet<CCSPlayerController> GetInnocents()
    {
        return Players().Where(player => player.PlayerRole() == Role.Innocent).Select(player => player.Player()).ToHashSet();
    }
    

    public Role GetRole(CCSPlayerController player)
    {
        return GetPlayer(player).PlayerRole();
    }

    public void AddTraitor(CCSPlayerController player)
    {
        GetPlayer(player).SetPlayerRole(Role.Traitor);
        player.SwitchTeam(CsTeam.Terrorist);
        player.PrintToCenter(Role.Traitor.FormatStringFullBefore("You are now a(n)"));
        player.PrintToChat(Role.Traitor.FormatStringFullBefore("You are now a(n)"));
        ModelHandler.SetModelNextServerFrame(player, ModelHandler.ModelPathTmPhoenix);
    }

    public void AddDetective(CCSPlayerController player)
    {
        GetPlayer(player).SetPlayerRole(Role.Detective);
        player.SwitchTeam(CsTeam.CounterTerrorist);
        player.PrintToCenter(Role.Detective.FormatStringFullBefore("You are now a(n)"));
        player.GiveNamedItem(CsItem.Taser);
        ModelHandler.SetModelNextServerFrame(player, ModelHandler.ModelPathCtmSas);
    }

    public void AddInnocents(IEnumerable<CCSPlayerController> players)
    {
        foreach (var player in players)
        {
            GetPlayer(player).SetPlayerRole(Role.Innocent);
            player.PrintToCenter(Role.Innocent.FormatStringFullBefore("You are now an"));
            player.SwitchTeam(CsTeam.Terrorist);     
            ModelHandler.SetModelNextServerFrame(player, ModelHandler.ModelPathTmPhoenix);
        }
    }

    public bool IsDetective(CCSPlayerController player)
    {
        return GetPlayer(player).PlayerRole() == Role.Detective;
    }

    public bool IsTraitor(CCSPlayerController player)
    {
        return GetPlayer(player).PlayerRole() == Role.Traitor;
    }

    public void Clear()
    {
        RemoveColors();
        Clr();
        foreach (var key in GetPlayers()) key.Value.SetPlayerRole(Role.Unassigned);
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
        return GetPlayer(player).PlayerRole() == Role.Innocent;
    }

    private void SetColors()
    {
        foreach (var key in Players())
        {
            if (key.PlayerRole() != Role.Innocent) return;
            ApplyInnocentColor(key.Player());
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
    
    private void SetColorTeam(CBaseEntity entity, string className, string fieldName, Role role = Role.Innocent)
    {
        Guard.IsValidEntity(entity);

        if (!Schema.IsSchemaFieldNetworked(className, fieldName))
        {
            Application.Instance.Logger.LogWarning("Field {ClassName}:{FieldName} is not networked, but SetStateChanged was called on it.", className, fieldName);
            return;
        }

        foreach (var player in GetPlayers().Values.Where(player => player.PlayerRole() == role))
        {
            Guard.IsValidEntity(player.Player());   
            
            int offset = Schema.GetSchemaOffset(className, fieldName);

            VirtualFunctions.StateChanged(player.Player().NetworkTransmitComponent.Handle, entity.Handle, offset, -1, -1);

            player.Player().LastNetworkChange = Server.CurrentTime;
            player.Player().IsSteadyState.Clear();
        }
    }
    
    private void ApplyDetectiveColor(CCSPlayerController player)
    {
        if (!player.IsReal() || player.Pawn.Value == null)
            return;

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
        player.Pawn.Value.Render = Color.Blue;
        SetColorTeam(player, "CBaseModelEntity", "m_clrRender", Role.Detective);
    }

    private void ApplyTraitorColor(CCSPlayerController player)
    {
        if (!player.IsReal() || player.Pawn.Value == null)
            return;

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderGlow;
        player.Pawn.Value.Render = Color.Red;
        SetColorTeam(player, "CBaseModelEntity", "m_clrRender", Role.Traitor);
        //apply for traitors only somehow?
    }

    private void ApplyInnocentColor(CCSPlayerController player)
    {
        if (!player.IsReal() || player.Pawn.Value == null)
            return;

        player.Pawn.Value.RenderMode = RenderMode_t.kRenderGlow;
        player.Pawn.Value.Render = Color.Green;
        
        SetColorTeam(player, "CBaseModelEntity", "m_clrRender");
    }

    private Role GetWinner()
    {
        return _traitorsLeft == 0 ? Role.Traitor : Role.Innocent;
    }
}