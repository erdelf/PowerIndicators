using RimWorld;
using Verse;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace ShowMeThePower
{
    [StaticConstructorOnStartup]
    internal class ShowMeThePower
    {
        private static readonly Texture2D power = ContentFinder<Texture2D>.Get(itemPath: "UI/Overlays/NeedsPower");

        private static readonly Dictionary<string, Texture2D> fuel = new Dictionary<string, Texture2D>();

        static ShowMeThePower()
        {
            Harmony harmony = new Harmony(id: "rimworld.erdelf.powerShower");
            harmony.Patch(original: AccessTools.Method(type: AccessTools.Method(type: typeof(Designator_Build), name: nameof(Designator_Build.GizmoOnGUI)).DeclaringType, name: nameof(Designator_Build.GizmoOnGUI)),
                                                        postfix: new HarmonyMethod(methodType: typeof(ShowMeThePower), methodName: nameof(DesignatorShower)));
            harmony.Patch(original: AccessTools.Method(type: AccessTools.Method(type: typeof(Designator_Dropdown), name: nameof(Designator_Dropdown.GizmoOnGUI)).DeclaringType, name: nameof(Designator_Dropdown.GizmoOnGUI)),
                          postfix: new HarmonyMethod(methodType: typeof(ShowMeThePower), methodName: nameof(DesignatorShower)));
        }

        public static void DesignatorShower(Command __instance, Vector2 topLeft)
        {

            Command des = __instance;

            if (__instance is Designator_Dropdown ddd)
                des = Traverse.Create(ddd).Field<Designator>("activeDesignator").Value;

            if (!(des is Designator_Build db) || !(db.PlacingDef is ThingDef td)) return;


            if (td.ConnectToPower)
            {
                GUI.DrawTexture(new Rect(x: topLeft.x + __instance.GetWidth(maxWidth: float.MaxValue) - 32f / 3 * 2 / 4 * 3, y: topLeft.y, width: 32f / 3 * 2, height: 32f / 3 * 2), image: power, ScaleMode.ScaleToFit);

            }
            else if (td.GetCompProperties<CompProperties_Refuelable>() is CompProperties_Refuelable refuelProps && refuelProps.fuelFilter.AllowedThingDefs.Any())
            {
                ThingDef def  = refuelProps.fuelFilter.AllowedThingDefs.First();
                Graphic  g    = def.graphic;
                string   path = g is Graphic_Collection gc ? Traverse.Create(root: gc).Field(name: "subGraphics").GetValue<Graphic[]>().Last().path : g.path;
                if (path.NullOrEmpty())
                    path = def.graphicData != null ? def.graphicData.texPath : Traverse.Create(root: ThingDefOf.Fire.graphic).Field(name: "subGraphics").GetValue<Graphic[]>().RandomElement().path;
                if (!fuel.ContainsKey(key: path))
                    fuel.Add(key: path, value: ContentFinder<Texture2D>.Get(itemPath: path));

                GUI.DrawTexture(new Rect(x: topLeft.x + __instance.GetWidth(maxWidth: float.MaxValue) - 32f / 4 * 3, y: topLeft.y, width: 32f / 3 * 2, height: 32f / 3 * 2), image: fuel[path], ScaleMode.ScaleToFit);
            }
        }
    }
}