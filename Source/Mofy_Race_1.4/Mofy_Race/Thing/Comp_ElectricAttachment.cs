using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mofy_Race
{
    public class Comp_FreezeAttachment : ThingComp
    {
        public override void CompTick()
        {
            if (this.parent.IsHashIntervalTick(180))
            {
                if (this.parent.GetComp<CompPowerTrader>().PowerOn)
                {
                    int num = GenRadial.NumCellsInRadius(5.9f);
                    for (int i = 0; i < num; i++)
                    {
                        IntVec3 c = this.parent.Position + GenRadial.RadialPattern[i];
                        if (!c.InBounds(this.parent.Map))
                        {
                            continue;
                        }
                        if (Rand.Range(0, 3) == 0)
                        {
                            if (GenSight.LineOfSight(c, this.parent.Position, this.parent.Map))
                            {
                                Thing thing = GenSpawn.Spawn(ThingDef.Named("Mofy_FreezeGas"), c, this.parent.Map);
                            }
                        }
                    }
                }
            }
        }
    }
}
