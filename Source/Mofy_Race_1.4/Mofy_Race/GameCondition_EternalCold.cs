using RimWorld;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Mofy_Race
{
    public class GameCondition_EternalCold : GameCondition
    {
        public override int TransitionTicks => 15000;

        public override float TemperatureOffset()
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks, -100f);
        }

        // 30秒に1回氷結ダメージ
        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            if (Find.TickManager.TicksGame % 1800 == 0)
            {
                for (int i = 0; i < affectedMaps.Count; i++)
                {
                    DoPawnsDamage(affectedMaps[i]);
                }
            }
        }

        private void DoPawnsDamage(Map map)
        {
            List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                DoPawnDamage(allPawnsSpawned[i]);
            }
        }

        public static void DoPawnDamage(Pawn pawn)
        {
            // 屋外で氷点下の場合、氷ダメージ
            if (pawn.Spawned && !pawn.Position.Roofed(pawn.Map) && pawn.Position.GetTemperature(pawn.Map) <= 0f)
            {
                pawn.GetAttachment(ThingDefOf.Fire)?.Kill();
                Hediff deff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Mofy_Freeze"));
                if (deff != null)
                {
                    if (pawn.RaceProps.IsMechanoid)
                    {
                        deff.Severity += 2.0f;
                    }
                    else
                    {
                        deff.Severity += 5.0f;
                    }
                }
                else
                {
                    deff = pawn.health.AddHediff(HediffDef.Named("Mofy_Freeze"));
                    if (pawn.RaceProps.IsMechanoid)
                    {
                        deff.Severity += 2.0f;
                    }
                    else
                    {
                        deff.Severity += 5.0f;
                    }
                }
                int maxhp = (int)Math.Max(pawn.GetStatValue(StatDefOf.ComfyTemperatureMin) * -1 * 3, 50);
                if (maxhp - (int)deff.Severity <= 0)
                {
                    IntVec3 pos = pawn.Position;
                    Building_IceGrave icegrave = (Building_IceGrave)GenSpawn.Spawn(ThingDef.Named("Mofy_IceGrave"), pos, pawn.Map, WipeMode.VanishOrMoveAside);
                    pawn.DeSpawn();
                    icegrave.Addthing(pawn);
                    pawn.Kill(null);
                    icegrave.SetFactionDirect(Faction.OfPlayer);
                    return;
                }
                BodyPartRecord bodyPartRecord = (from p in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside) where p.def.defName.ToStringSafe() != "Waist" select p).RandomElement();
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Frostbite, deff.Severity / 10.0f, 5.0f, -1, null, bodyPartRecord));
            }
        }

    }
}
