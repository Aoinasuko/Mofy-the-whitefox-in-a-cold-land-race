using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mofy_Race
{
    public class Mofy_FreezeBomb : Projectile_Explosive
    {
		protected override void Explode()
		{
			Map map = base.Map;
			Destroy();
			if (base.def.projectile.explosionEffect != null)
			{
				Effecter effecter = base.def.projectile.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
				effecter.Cleanup();
			}
			GenExplosion.DoExplosion(base.Position, map, def.projectile.explosionRadius, DamageDefOf.Bomb, base.launcher, def.projectile.GetDamageAmount(1.0f), -1f, null, null, null, null, ThingDef.Named("Mofy_FreezeGas"), 1f);
		}
	}

	public class Mofy_GasFreeze : Gas
	{
		public override void Tick()
		{
			base.Tick();
			if (this.Spawned)
			{
				if (this.IsHashIntervalTick(30))
				{
					IEnumerable<Pawn> pawns = this.Map.mapPawns.AllPawnsSpawned.Where(x => x.Position.DistanceTo(this.Position) <= 0.9f);
					if (!pawns.EnumerableNullOrEmpty())
					{
						for (int i = pawns.Count() - 1; i >= 0; i--)
                        {
                            Pawn pawn = pawns.ElementAt(i);
							pawn.GetAttachment(ThingDefOf.Fire)?.Kill();
							Hediff deff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Mofy_Freeze"));
							if (deff != null)
                            {
								if (pawn.RaceProps.IsMechanoid)
                                {
									deff.Severity += 2.0f;
								} else
                                {
									deff.Severity += 5.0f;
								}								
							} else
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
								Building_IceGrave icegrave = (Building_IceGrave)GenSpawn.Spawn(ThingDef.Named("Mofy_IceGrave"), pos, this.Map, WipeMode.VanishOrMoveAside);
								pawn.DeSpawn();
								icegrave.Addthing(pawn);
								pawn.Kill(null);
								icegrave.SetFactionDirect(Faction.OfPlayer);
								continue;
							}
							BodyPartRecord bodyPartRecord = (from p in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside) where p.def.defName.ToStringSafe() != "Waist" select p).RandomElement();
							pawn.TakeDamage(new DamageInfo(DamageDefOf.Frostbite, deff.Severity / 10.0f, 5.0f, -1, null, bodyPartRecord));
						}
					}
				}
			}
		}
	}

}
