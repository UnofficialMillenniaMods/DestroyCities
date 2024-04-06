using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CPrompt;
using HarmonyLib;
using UnityEngine;


namespace DestroyCities
{
    [BepInPlugin("DestroyCities", "DestroyCities", "1.0.0")]
    [BepInProcess("Millennia.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        private void Awake()
        {
            Logger.LogInfo($"Plugin DestroyCities is getting intiated!");
            var harmony = new Harmony("DestroyCities");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin DestroyCities finished patching!");
        }
    }

    [HarmonyPatch(typeof(AActionPanel))]
    public class AActionPanelPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ParseTileAction")]
        private static bool parseTileAction(string tileActionText, out string tooltipText, out string buttonText, out Sprite buttonSprite)
        {
            buttonText = "";
            tooltipText = "";
            buttonSprite = null;
            if (tileActionText != AEntityTilePatch.actionDestroyCity)
            {
                return true;
            }
            buttonText = string.Format(AStringTable.Instance.GetString("UI-Tooltip-EffectDestroy"), "");
            tooltipText = "Destroys the selected city.";
            buttonSprite = (Resources.Load("UI/Icons/ActionDestroyImprovementIcon", typeof(Sprite)) as Sprite);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnTileActionButtonPressed")]
        private static bool onTileActionButtonPressed(AActionPanel __instance, string tileActionText)
        {
            if (tileActionText != "TLAC,actionDestroyCity,,")
            {
                return true;
            }
            GameObject target = Traverse.Create(__instance).Field<GameObject>("ActionTarget").Value;
            AEntityTile tile = target.GetComponent<AEntityTile>();
            AUIHelpers.DialogYorN("Do you really want to destroy the city?", delegate ()
            {
                tile.DestroyEntity(false, false, true, false);
            }, null);
            return false;
        }
    }

    [HarmonyPatch(typeof(AEntityTile))]
    public class AEntityTilePatch
    {
        // Token: 0x06000006 RID: 6 RVA: 0x00002124 File Offset: 0x00000324
        [HarmonyPostfix]
        [HarmonyPatch("GetTileActions", new Type[]
        {
            typeof(List<string>),
            typeof(int),
            typeof(string),
            typeof(bool)
        })]
        private static void getTileActions(AEntityTile __instance, List<string> retval, int forPlayer)
        {
            if (__instance.IsCity(false) && __instance.PlayerNum == forPlayer)
            {
                retval.Add(actionDestroyCity);
            }            
        }

        // Token: 0x04000002 RID: 2
        public const string actionDestroyCity = "TLAC,actionDestroyCity,,";
    }
}
