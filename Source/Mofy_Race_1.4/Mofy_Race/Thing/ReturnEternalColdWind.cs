using AlienRace;
using BEPRace_Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace Mofy_Race
{
    /*
     * ボス「再来する永遠の寒波」
     */
    public class RECW : ThingWithComps
    {
        public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
        {
            Comp_BossHP bosshp = GetComp<Comp_BossHP>();
            if (bosshp.GetHP() <= 0)
            {
                base.Kill(dinfo, exactCulprit);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Comp_BossHP bosshp = GetComp<Comp_BossHP>();
            if (bosshp.GetHP() <= 0)
            {
                base.Destroy(mode);
            }
        }
    }


    public class ReturnEternalColdWind : ThingComp
    {
        // 存在するコアの数
        private int corecount = 0;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                CellRect cellRect = CellRect.CenteredOn(this.parent.Position, 10);
                cellRect.ClipInsideMap(this.parent.Map);
                // 召喚時に周りに3個コア召喚
                for (int i = 0; i < 3; i++)
                {
                    IntVec3 randomCell = cellRect.RandomCell;
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("Mofy_FreezeCore"));
                    GenPlace.TryPlaceThing(thing, randomCell, this.parent.Map, ThingPlaceMode.Near);
                    thing.SetFactionDirect(Faction.OfMechanoids);
                    Effecter_BEPCore.BEP_UseSkill_D.Spawn(randomCell, this.parent.Map, Vector3.zero);
                }
            }
        }

        public override void CompTick()
        {
            if (this.parent.IsHashIntervalTick(60000))
            {
                // １日ごとに周りに4個コア召喚
                CellRect cellRect = CellRect.CenteredOn(this.parent.Position, 10);
                cellRect.ClipInsideMap(this.parent.Map);
                for (int i = 0; i < 4; i++)
                {
                    IntVec3 randomCell = cellRect.RandomCell;
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("Mofy_FreezeCore"));
                    GenPlace.TryPlaceThing(thing, randomCell, this.parent.Map, ThingPlaceMode.Near);
                    thing.SetFactionDirect(Faction.OfMechanoids);
                    Effecter_BEPCore.BEP_UseSkill_D.Spawn(randomCell, this.parent.Map, Vector3.zero);
                }
            }
            // N秒ごとにダメージ
            if (this.parent.IsHashIntervalTick(60 + (Math.Min(10, corecount) * 60)))
            {
                Comp_BossHP bosshp = this.parent.GetComp<Comp_BossHP>();
                bosshp.HPDamage(1);
                // もしHPが0になったらアイテム落としてkill
                if (bosshp.GetHP() <= 0)
                {
                    GenExplosion.DoExplosion(this.parent.Position, this.parent.Map, 5.9f, DamageDefOf.Smoke, null, 0, -1f, null, null, null, null, null);
                    if (this.parent.Spawned)
                    {
                        if (this.parent.Map != null)
                        {
                            Effecter_BEPCore.BEP_UseSkill_D.Spawn(this.parent.Position, this.parent.Map, Vector3.zero);
                            // アイテムをドロップする
                            Thing thing = ThingMaker.MakeThing(ThingDef.Named("Mofy_WK_Freeze"));
                            GenPlace.TryPlaceThing(thing, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
                        }
                    }
                    this.parent.Destroy();
                }
            }
            // 10秒ごとにワールド上にあるコアの数を数える
            if (this.parent.IsHashIntervalTick(600))
            {
                corecount = this.parent.Map.spawnedThings.Where(x => x.def.defName == "Mofy_FreezeCore").Count();
            }
        }
    }

    /*
     * フリーズコア
     */
    public class FreezeCore : ThingComp
    {
        public override void CompTick()
        {
            // もし、ボスが破壊されている場合は消滅する
            if (this.parent.IsHashIntervalTick(600))
            {
                if (this.parent.Map.spawnedThings.Where(x => x.def.defName == "Mofy_FreezeCore").EnumerableNullOrEmpty())
                {
                    this.parent.Kill();
                }
            }
        }

        /// <summary>
        /// 破壊時に特殊バフのモーフィーを召喚
        /// </summary>
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            // デバフ付与
            List<Pawn> pawns = previousMap.mapPawns.AllPawnsSpawned.Where(x => x.Faction != Faction.OfMechanoids).ToList();
            HediffDef hediffDef = HediffDef.Named("Mofy_FreezeWish");
            foreach (Pawn pawn in pawns)
            {
                Effecter_BEPCore.BEP_UseSkill_D.Spawn(pawn.Position, pawn.Map, Vector3.zero);
                pawn.health.AddHediff(hediffDef);
            }

            // モーフィー召喚
            PawnKindDef kind = PawnKindDef.Named("Mofy_Morphy");
            HediffDef hediffDef2 = HediffDef.Named("Mofy_FreezeWish_Enemy");
            CellRect cellRect = CellRect.CenteredOn(this.parent.Position, 5);
            cellRect.ClipInsideMap(previousMap);
            List<Pawn> Summons = new List<Pawn>();
            for (int i = 0; i < 5; i++)
            {
                // 召喚
                PawnGenerationRequest request = new PawnGenerationRequest(kind);
                Pawn SummonPawn = PawnGenerator.GeneratePawn(request);
                SummonPawn.SetFactionDirect(Faction.OfMechanoids);
                SummonPawn.health.AddHediff(hediffDef2);
                IntVec3 randomCell = cellRect.RandomCell;
                GenSpawn.Spawn(SummonPawn, randomCell, previousMap);
                Summons.Add(SummonPawn);
                Effecter_BEPCore.BEP_UseSkill_D.Spawn(randomCell, previousMap, Vector3.zero);
            }
            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_DefendPoint(this.parent.Position, 30.9f), previousMap, Summons);

            // 煙幕展開
            GenExplosion.DoExplosion(this.parent.Position, previousMap, 5.9f, DamageDefOf.Smoke, null, 0, -1f, null, null, null, null, null);
        }
    }

}
