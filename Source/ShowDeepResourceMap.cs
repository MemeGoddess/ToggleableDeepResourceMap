using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace ShowDeepResourceMap
{
    [StaticConstructorOnStartup]
    [HarmonyPatch]
    public class ShowDeepResourceMap : Mod
    {
        public static KeyBindingDef ToggleShowDeepResourcesMap = new KeyBindingDef();
        public static bool ShowDeepResource = false;
        public ShowDeepResourceMap(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("memegoddess.ToggleableDeepResourceMap");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(DeepResourceGrid), nameof(DeepResourceGrid.DeepResourcesOnGUI))]
        [HarmonyPrefix]
        public static bool DeepResourcesOnGUI_Post(DeepResourceGrid __instance)
        {
            try
            {
                if (!__instance.AnyActiveDeepScannersOnMap())
                    return false;

                var method = __instance.GetType().GetMethod("RenderMouseAttachments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null)
                    Log.Error("Method is null");

                if (ShowDeepResource)
                {
                    method.Invoke(__instance, null);
                    __instance.MarkForDraw();
                }
                else
                {
                    Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
                    if (singleSelectedThing == null)
                        return false;
                    CompDeepScanner comp1 = singleSelectedThing.TryGetComp<CompDeepScanner>();
                    CompDeepDrill comp2 = singleSelectedThing.TryGetComp<CompDeepDrill>();
                    if (comp1 == null && comp2 == null || !__instance.AnyActiveDeepScannersOnMap())
                        return false;
                    method.Invoke(__instance, null);
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return false;
        }

        [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
        [HarmonyPostfix]
        public static void InsertDeepResourceToggle(WidgetRow row, bool worldView, PlaySettings __instance)
        {
            if (worldView || row == null)
                return;

            row.ToggleableIcon(ref ShowDeepResource, TexButton.ShowZones, "Show Deep Resources", SoundDefOf.Mouseover_ButtonToggle);
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