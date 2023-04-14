using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mofy_Race
{

    [DefOf]
    public static class Job_Mofy
    {
        public static JobDef MilkingMofy;
    }

    [DefOf]
    public static class FleshType_Mofy
    {
        public static FleshTypeDef Mofy;
    }

    [DefOf]
    public static class Ideology_Mofy
    {
        [MayRequireIdeology]
        public static MemeDef Mofy_Mofy;
    }
}
