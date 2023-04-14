using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mofy_Race
{
	public class IngestionOutcomeDoer_MofyMilk : IngestionOutcomeDoer
	{
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			pawn.health.AddHediff(HediffDef.Named("Mofy_MilkHediff"));
		}
	}
}
