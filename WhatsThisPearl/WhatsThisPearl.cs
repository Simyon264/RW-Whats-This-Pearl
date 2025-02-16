using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Menu;

namespace WhatsThisPearl;

[BepInPlugin(ModID, ModName, ModVersion)]
public class WhatsThisPearl : BaseUnityPlugin
{
    public const string ModID      = "simyon.whatsthispearl";
    public const string ModName    = "What's This Pearl?";
    public const string ModVersion = "1.0.0";

    internal WhatsThisPearlOptions Options;

    public static ManualLogSource ExtLogger = null!;
    public static WhatsThisPearl Instance = null!;

    /// <summary>
    /// Holds a key-value pair of pearl type and its formatted name.
    /// </summary>
    public static Dictionary<DataPearl.AbstractDataPearl.DataPearlType, string> PearlNames = new();

    public WhatsThisPearl()
    {
        Instance = this;

    }

    public void OnEnable()
    {
        ExtLogger = Logger;

        try
        {
            Options = new WhatsThisPearlOptions(this);
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to initialize options: {e}\n{e.StackTrace}");
        }

        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;

        Logger.LogInfo($"{ModName} v{ModVersion} has been enabled.");
    }

    private bool _isInitialized;

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_isInitialized)
            return;

        _isInitialized = true;

        try
        {
            MachineConnector.SetRegisteredOI(ModID, Options);
            Logger.LogInfo("Registered options.");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to register options: {e}\n{e.StackTrace}");
        }
    }

    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            Logger.LogInfo("Initializing pearl names...");

            PearlNames.Clear();

            var tempPearlNames = new Dictionary<DataPearl.AbstractDataPearl.DataPearlType, string>();
            for (var i = 0; i < DataPearl.AbstractDataPearl.DataPearlType.values.Count; i++)
            {
                var enumPearl =  DataPearl.AbstractDataPearl.DataPearlType.values.entries[i];
                var pearlName = enumPearl switch
                {
                    "MS" => "Garbage Wastes",
                    "Red_stomach" => "Hunter",
                    "Rivulet_stomach" => "Rivulet",
                    "RM" => "Music",
                    "Spearmasterpearl" => "Spearmaster",
                    _ => Region.GetRegionFullName(enumPearl.Split('_')[0], null)
                };

                if (pearlName == "Unknown Region")
                    continue; // Skip unknown regions

                var enumValue = new DataPearl.AbstractDataPearl.DataPearlType(enumPearl);
                if (enumValue.Index == -1)
                {
                    Logger.LogError($"Failed to parse pearl type: {enumPearl}");
                    continue;
                }

                tempPearlNames.Add(enumValue, $"[{pearlName} pearl");
            }

            // Because there are sometimes multiple pearls in a region, we need to format them so they can be distinguished.
            // This is done by adding a number to the end of the pearl name.
            var pearlCounts = tempPearlNames
                .GroupBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Count());
            foreach (var kvp in tempPearlNames)
            {
                var pearlType = kvp.Key;
                var pearlName = kvp.Value;

                if (pearlCounts[pearlName] > 1)
                {
                    var count = 1;
                    var formattedName = $"{pearlName} {count}]";
                    while (PearlNames.ContainsValue(formattedName))
                    {
                        count++;
                        formattedName = $"{pearlName} {count}]";
                    }

                    PearlNames.Add(pearlType, formattedName);
                }
                else
                {
                    PearlNames.Add(pearlType, pearlName);
                }
            }

            // any pearl names that dont end with ] need one added
            foreach (var kvp in PearlNames.ToList())
            {
                if (!kvp.Value.EndsWith("]"))
                {
                    PearlNames[kvp.Key] = $"{kvp.Value}]";
                }
            }

            Logger.LogInfo($"Initialized {PearlNames.Count} pearl names.");

            foreach (var kvp in PearlNames)
            {
                Logger.LogInfo($"Pearl type: {kvp.Key}, Name: {kvp.Value}");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to initialize pearl names: {e}\n{e.StackTrace}");
        }
    }

    private void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);

        CreateHud(self, cam.room.game);
    }

    public void OnDisable()
    {
        On.HUD.HUD.InitSinglePlayerHud -= HUD_InitSinglePlayerHud;
        On.RainWorld.PostModsInit -= RainWorld_PostModsInit;
        On.RainWorld.OnModsInit -= RainWorld_OnModsInit;

        PearlNames.Clear();
    }

    private void CreateHud(HUD.HUD hud, RainWorldGame game)
    {
        Logger.LogInfo("Creating HUD...");

        if (game.session is not StoryGameSession ses)
        {
            Logger.LogInfo("HUD not created, not a story game session.");
            return;
        }

        var players = ses.Players;
        if (players == null || players.Count == 0)
        {
            Logger.LogInfo("HUD not created, no players.");
            return;
        }

        for (var index = 0; index < players.Count; index++)
        {
            var creature = players[index];
            if (creature.realizedCreature is not Player player)
            {
                Logger.LogInfo($"HUD not created, player {index} is not a player.");
                continue;
            }

            hud.AddPart(new PearlObjectHud(hud, player, index, players.Count, ses));
            Logger.LogInfo($"Created HUD for player {index}.");
        }

        Logger.LogInfo($"Created hud for {players.Count} players.");
    }
}