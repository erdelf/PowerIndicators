using RimWorld;
using Verse;
using System.Linq;
using UnityEngine;
using Harmony;
using System.Collections.Generic;

namespace ShowMeThePower
{
    [StaticConstructorOnStartup]
    internal class ShowMeThePower
    {
        private static readonly Texture2D power = ContentFinder<Texture2D>.Get(itemPath: "UI/Overlays/NeedsPower");

        private static readonly Dictionary<string, Texture2D> fuel = new Dictionary<string, Texture2D>();

        static ShowMeThePower() => HarmonyInstance.Create(id: "rimworld.erdelf.powerShower").Patch(
            original: AccessTools.Method(type: AccessTools.Method(type: typeof(Designator_Build), name: nameof(Designator_Build.GizmoOnGUI)).DeclaringType, name: nameof(Designator_Build.GizmoOnGUI)), prefix: null,
            postfix: new HarmonyMethod(type: typeof(ShowMeThePower), name: nameof(DesignatorShower)));

        public static void DesignatorShower(Command __instance, Vector2 topLeft, GizmoResult __result)
        {
            if (!(__instance is Designator_Build db) || !(db.PlacingDef is ThingDef td)) return;
            if (td.ConnectToPower)
            {
                GUI.DrawTexture(
                    position: new Rect(x: topLeft.x + __instance.GetWidth(maxWidth: float.MaxValue) - power.width / 3 * 2 / 4 * 3, y: topLeft.y, width: power.width / 3 * 2,
                        height: power.height                                                                                  / 3                                       * 2), image: power);

            }
            else if (td.GetCompProperties<CompProperties_Refuelable>() is CompProperties_Refuelable refuelProps)
            {
                ThingDef def  = refuelProps.fuelFilter.AllowedThingDefs.First();
                Graphic  g    = def.graphic;
                string   path = g is Graphic_Collection gc ? Traverse.Create(root: gc).Field(name: "subGraphics").GetValue<Graphic[]>().Last().path : g.path;
                if (path.NullOrEmpty())
                    path = def.graphicData != null ? def.graphicData.texPath : Traverse.Create(root: ThingDefOf.Fire.graphic).Field(name: "subGraphics").GetValue<Graphic[]>().RandomElement().path;
                if (!fuel.ContainsKey(key: path))
                    fuel.Add(key: path, value: ContentFinder<Texture2D>.Get(itemPath: path));

                GUI.DrawTexture(
                    position: new Rect(x: topLeft.x + __instance.GetWidth(maxWidth: float.MaxValue) - power.width / 4 * 3, y: topLeft.y, width: power.width / 3 * 2, height: power.height / 3 * 2),
                    image: fuel[key: path]);
            }
        }
    }
}