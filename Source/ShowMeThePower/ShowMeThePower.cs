using RimWorld;
using Verse;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Harmony;
using System.Collections.Generic;

namespace ShowMeThePower
{
    [StaticConstructorOnStartup]
    class ShowMeThePower
    {
        static FieldInfo designatorInfo = typeof(DesignationCategoryDef).GetField("resolvedDesignators", BindingFlags.NonPublic | BindingFlags.Instance);
        static Texture2D power = ContentFinder<Texture2D>.Get("UI/Overlays/NeedsPower");

        static Dictionary<string, Texture2D> fuel = new Dictionary<string, Texture2D>();

        static ShowMeThePower() => HarmonyInstance.Create("rimworld.erdelf.powerShower").Patch(
            AccessTools.Method(typeof(Designator_Build), nameof(Designator_Build.GizmoOnGUI)), null, new HarmonyMethod(typeof(ShowMeThePower), nameof(DesignatorShower)));

        public static void DesignatorShower(Command __instance, Vector2 topLeft, GizmoResult __result)
        {
            if (__instance is Designator_Build db && db.PlacingDef is ThingDef td)
            {
                if (td.ConnectToPower)
                {
                    GUI.DrawTexture(new Rect(topLeft.x + __instance.Width - power.width/3*2 / 4 * 3, topLeft.y, power.width/3*2, power.height/3*2), power);
                    /*
                    CompProperties_Power props = td.GetCompProperties<CompProperties_Power>();
                    if (props != null)
                    {
                        Rect rect = new Rect(topLeft.x + __instance.Width/6*3, topLeft.y, __instance.Width, 75f);
                        Widgets.Label(rect, props.basePowerConsumption.ToString());
                    }*/
                } else if(td.GetCompProperties<CompProperties_Refuelable>() is CompProperties_Refuelable refuelProps)
                {
                    ThingDef def = refuelProps.fuelFilter.AllowedThingDefs.First();
                    Graphic g = def.graphic;
                    string path = g is Graphic_Collection gc ? Traverse.Create(gc).Field("subGraphics").GetValue<Graphic[]>().Last().path : g.path;
                    if (path.NullOrEmpty())
                    {
                        if (def.graphicData != null)
                            path = def.graphicData.texPath;
                        else
                            path = Traverse.Create(ThingDefOf.Fire.graphic).Field("subGraphics").GetValue<Graphic[]>().RandomElement().path;
                    }
                    if (!fuel.ContainsKey(path))
                        fuel.Add(path, ContentFinder<Texture2D>.Get(path));
                        
                    GUI.DrawTexture(new Rect(topLeft.x + __instance.Width - power.width / 4 * 3, topLeft.y, power.width / 3 * 2, power.height / 3 * 2), fuel[path]);
                }
            }
        }
    }
}