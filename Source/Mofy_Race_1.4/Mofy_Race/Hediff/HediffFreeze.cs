using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mofy_Race
{
	public class Hediff_Freeze : HediffWithComps
	{
		public override string LabelBase
		{
			get
			{
				int maxhp = (int)Math.Max(this.pawn.GetStatValue(StatDefOf.ComfyTemperatureMin) * -1 * 3, 50);
				return base.LabelBase + "(" + (maxhp - (int)this.Severity) + "/" + maxhp + ")";
			}
		}
	}
}
