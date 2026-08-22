using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace ShowDeepResourceMap
{
    [HarmonyPatch]
    public class ShowDeepResourceMap : Mod
    {
        public static bool ShowDeepResource = false;
        private static readonly Action<DeepResourceGrid> RenderMouseAttachments =
            (Action<DeepResourceGrid>)AccessTools
                .Method(typeof(DeepResourceGrid), "RenderMouseAttachments")
                .CreateDelegate(typeof(Action<DeepResourceGrid>));

        public ShowDeepResourceMap(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("memegoddess.ToggleableDeepResourceMap");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(DeepResourceGrid), nameof(DeepResourceGrid.DeepResourcesOnGUI))]
        [HarmonyPrefix]
        public static bool DeepResourceOverlayWhenToggled(DeepResourceGrid __instance)
        {
            try
            {
                if (!ShowDeepResource)
                    return true;
                
                if (!__instance.AnyActiveDeepScannersOnMap())
                    return true;
                
                RenderMouseAttachments(__instance);
                __instance.MarkForDraw();
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return true;
        }

        [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
        [HarmonyPostfix]
        public static void InsertDeepResourceToggle(WidgetRow row, bool worldView, PlaySettings __instance)
        {
            if (worldView || row == null)
                return;

            row.ToggleableIcon(ref ShowDeepResource, Textures.Mine, "Show Deep Resources", SoundDefOf.Mouseover_ButtonToggle);
            CheckKeyBindingToggle(ShowDeepResourceKeysDefOf.ShowDeepResourceKey, ref ShowDeepResource);
        }

        private static void CheckKeyBindingToggle(KeyBindingDef keyBinding, ref bool value)
        {
            if (!keyBinding.KeyDownEvent)
                return;
            value = !value;
            if (value)
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            else
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
        }
    }

    [DefOf]
    public static class ShowDeepResourceKeysDefOf
    {
        public static KeyBindingDef ShowDeepResourceKey;
    }
}