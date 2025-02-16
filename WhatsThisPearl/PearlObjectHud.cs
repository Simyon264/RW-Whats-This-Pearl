using System;
using System.Collections.Generic;
using System.Linq;
using HUD;
using RWCustom;
using UnityEngine;

namespace WhatsThisPearl;

public class PearlObjectHud : HudPart
{
    private Player _player;

    private FContainer _container;
    private FLabel _label;
    private Color _labelGoalColor;
    private DataPearl.AbstractDataPearl.DataPearlType? _lastPearl;
    private StoryGameSession _session;

    public PearlObjectHud(HUD.HUD hud, Player player, int index, int playerCount, StoryGameSession session) : base(hud)
    {
        _player = player;
        _session = session;

        _container = new FContainer();
        hud.fContainers[1].AddChild(_container);
        CreateHud(index, playerCount);
        PlaceContainer(index, playerCount);

        WhatsThisPearl.ExtLogger.LogInfo($"All registered pearls:");
        foreach (var pearlType in WhatsThisPearl.PearlNames.Keys)
        {
            WhatsThisPearl.ExtLogger.LogInfo($"Pearl type: {pearlType}");
            if (_session.saveState.progression.miscProgressionData.GetPearlDeciphered(pearlType))
                WhatsThisPearl.ExtLogger.LogInfo($"Deciphered by: LM"); // Looks to the Moon
            if (_session.saveState.progression.miscProgressionData.GetFuturePearlDeciphered(pearlType))
                WhatsThisPearl.ExtLogger.LogInfo($"Deciphered by: LMF"); // Looks to the Moon Future
            if (_session.saveState.progression.miscProgressionData.GetPebblesPearlDeciphered(pearlType))
                WhatsThisPearl.ExtLogger.LogInfo($"Deciphered by: PB"); // Pebbles
            if (_session.saveState.progression.miscProgressionData.GetDMPearlDeciphered(pearlType))
                WhatsThisPearl.ExtLogger.LogInfo($"Deciphered by: PLM"); // Past Looks to the Moon

            WhatsThisPearl.ExtLogger.LogInfo($"Formatted name: {WhatsThisPearl.PearlNames[pearlType]}");
        }

        // debug log all read pearls
        var decipheredPearls = _session.saveState.progression.miscProgressionData.decipheredPearls;
        var decipheredFuturePearls = _session.saveState.progression.miscProgressionData.decipheredFuturePearls;
        var decipheredPebblesPearls = _session.saveState.progression.miscProgressionData.decipheredPebblesPearls;
        var decipheredDmPearls = _session.saveState.progression.miscProgressionData.decipheredDMPearls;

        WhatsThisPearl.ExtLogger.LogInfo($"Deciphered pearls: {string.Join(", ", decipheredPearls)}");
        WhatsThisPearl.ExtLogger.LogInfo($"Deciphered future pearls: {string.Join(", ", decipheredFuturePearls)}");
        WhatsThisPearl.ExtLogger.LogInfo($"Deciphered pebbles pearls: {string.Join(", ", decipheredPebblesPearls)}");
        WhatsThisPearl.ExtLogger.LogInfo($"Deciphered DM pearls: {string.Join(", ", decipheredDmPearls)}");

        WhatsThisPearl.ExtLogger.LogInfo($"Created HUD for player {index}.");
    }

    public override void Update()
    {
        base.Update();

        //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD...");
        if (_player == null || _player.mainBodyChunk == null)
        { // just in case
            ResetLabel();
            _lastPearl = null;
            return;
        }

        //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD... B");

        if (_player.touchedNoInputCounter > 20)
        {
            //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD... C");
            if (_player.grasps == null || _player.grasps.Length == 0)
            {
                ResetLabel();
                _lastPearl = null;
                return;
            }

            //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD... D");

            if (_player.grasps[0] == null)
            {
                ResetLabel();
                _lastPearl = null;
                return;
            }

            //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD... D2");

            var pearl = _player.grasps[0].grabbed;
            if (pearl == null)
            {
                ResetLabel();
                _lastPearl = null;
                return;
            }

            if (pearl.abstractPhysicalObject is not DataPearl.AbstractDataPearl dataPearl)
            {
                ResetLabel();
                _lastPearl = null;
                return;
            }

            if (_lastPearl == dataPearl.dataPearlType)
            {
                if (_label.color.a >= 1f)
                {
                    return;
                }

                // Increase label alpha
                var alpha = _label.color.a + 0.05f;
                if (alpha > 1f)
                {
                    alpha = 1f;
                }

                _label.color = new Color(_labelGoalColor.r, _labelGoalColor.g, _labelGoalColor.b, alpha);

                return;
            }

            _lastPearl = dataPearl.dataPearlType;

            //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD... E");

            if (!WhatsThisPearl.PearlNames.TryGetValue(dataPearl.dataPearlType, out var pearlName))
            {
                ResetLabel();
                return;
            }

            //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD... F");

            var labelColor = DataPearl.UniquePearlMainColor(dataPearl.dataPearlType);
            if (labelColor.r + labelColor.g + labelColor.b <= 0.3f)
            {
                labelColor = DataPearl.UniquePearlHighLightColor(dataPearl.dataPearlType) ?? labelColor;
            }

            //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD... G");

            if (WhatsThisPearl.Instance.Options.ColorLabels.Value)
            {
                _labelGoalColor = labelColor;
            }
            else
            {
                _labelGoalColor = Color.white;
            }

            _label.color = new Color(_labelGoalColor.r, _labelGoalColor.g, _labelGoalColor.b, 0f);

            //WhatsThisPearl.ExtLogger.LogInfo("Updating HUD... H");

            var readBy = new List<string>();

            if (_session.saveState.progression.miscProgressionData.GetPearlDeciphered(dataPearl.dataPearlType))
                readBy.Add("LM"); // Looks to the Moon

            if (_session.saveState.progression.miscProgressionData.GetFuturePearlDeciphered(dataPearl.dataPearlType))
                readBy.Add("LMF"); // Looks to the Moon Future

            if (_session.saveState.progression.miscProgressionData.GetPebblesPearlDeciphered(dataPearl.dataPearlType))
                readBy.Add("PB"); // Pebbles

            if (_session.saveState.progression.miscProgressionData.GetDMPearlDeciphered(dataPearl.dataPearlType))
                readBy.Add("PLM"); // Past Looks to the Moon

            WhatsThisPearl.ExtLogger.LogInfo($"Pearl name: {pearlName}");
            WhatsThisPearl.ExtLogger.LogInfo($"Read by: {string.Join(", ", readBy)}");

            //if (WhatsThisPearl.Instance.Options.ShowHasRead.Value)
            //{
            //    pearlName += "\n" + string.Join(", ", readBy);
            //}

            if (WhatsThisPearl.Instance.Options.ShowPearlID.Value)
            {
                // remove [ from the end
                pearlName = pearlName.Substring(0, pearlName.Length - 1);
                pearlName += $" #{dataPearl.dataPearlType}#]";
            }

            _label.text = pearlName;
        }
        else
        {
            ResetLabel();
        }
    }

    private void ResetLabel()
    {
        //WhatsThisPearl.ExtLogger.LogInfo("RESETTING LABEL");
        var requiresReset = _labelGoalColor != Color.white || _label.text != "";
        if (!requiresReset)
        {
            return; // nothing to do
        }

        _lastPearl = null;
        _label.text = "";
        _labelGoalColor = Color.white;
        _label.color = Color.white;
    }

    private void CreateHud(int playerId, int playerCount)
    {
        _label = new FLabel(Custom.GetFont(), "Pearl Object");
        var pos = new Vector2(0, 0) - new Vector2(0.0f, (float)_label.FontLineHeight / 2 + 0.1f);
        _label.SetPosition(pos);
        _container.AddChild(_label);
    }

    private void PlaceContainer(int playerId, int playerCount)
    {
        var newPosition = new Vector2(hud.rainWorld.screenSize.x / 2f, hud.rainWorld.options.SafeScreenOffset.y + 17);
        if ((!ModManager.JollyCoop ? 0 : (playerCount > 1 ? 1 : 0)) != 0)
        {
            var offset = (playerId - (float) (playerCount / 2.0 - 0.5)) * 8;
            newPosition.x += offset;
        }

        _container.SetPosition(newPosition);
    }
}