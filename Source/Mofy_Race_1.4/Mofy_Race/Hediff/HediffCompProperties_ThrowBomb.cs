using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Mofy_Race
{

    public class HediffCompProperties_ThrowBomb : HediffCompProperties
    {
        public HediffCompProperties_ThrowBomb()
        {
            compClass = typeof(HediffComp_ThrowBomb);
        }
    }

    public class HediffComp_ThrowBomb : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(900))
            {
                // ランダムな敵に凍結ボムを投げる
                if (Pawn.Map != null)
                {
                    Pawn tag = Pawn.Map.mapPawns.AllPawnsSpawned.Where(x => x.HostileTo(Pawn)).RandomElement();
                    if (tag != null)
                    {
                        Projectile shot = (Projectile)GenSpawn.Spawn(ThingDef.Named("Mofy_FreezeBomb_Bomb"), Pawn.Position, Pawn.Map);
                        IntVec3 c = tag.Position;
                        Vector3 drawPos = Pawn.DrawPos;
                        shot.Launch(Pawn, drawPos, c, Pawn.Position, ProjectileHitFlags.NonTargetPawns, false, null);
                    }
                }
                
            }
        }
    }

}
