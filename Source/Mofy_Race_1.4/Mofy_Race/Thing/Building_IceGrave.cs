using RimWorld;
using Verse;

namespace Mofy_Race
{
	public class Building_IceGrave : Building_Casket
	{
		public override Graphic Graphic
		{
			get
			{
				return base.Graphic;
			}
		}

		public override void Draw()
		{
			base.Draw();
			if (!base.innerContainer.NullOrEmpty())
			{
				ContainedThing.DrawAt(base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingBelowTop) + def.building.gibbetCorposeDrawOffset);
				base.DrawAt(base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Item));
			}
		}

		public void Addthing(Thing thing)
		{
			innerContainer.TryAdd(thing);
			return;
		}

		public override bool Accepts(Thing thing)
		{
			return innerContainer.CanAcceptAnyOf(thing);
		}

		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (!Accepts(thing))
			{
				return false;
			}
			bool flag = false;
			if (thing.holdingOwner != null)
			{
				thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
				flag = true;
			}
			else
			{
				flag = innerContainer.TryAdd(thing);
			}
			if (flag)
			{
				return true;
			}
			return false;
		}

		public override void TickRare()
		{
			if (base.innerContainer.NullOrEmpty())
            {
				this.TakeDamage(new DamageInfo(DamageDefOf.Burn, 50.0f));
            }
		}

	}
}
