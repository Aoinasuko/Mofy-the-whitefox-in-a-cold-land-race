using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mofy_Race
{

    [StaticConstructorOnStartup]
    static class Mofy_Harmony
    {
        static Mofy_Harmony()
        {
            var harmony = new Harmony("BEP.Mofy");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Defパッチ
            if (ModLister.IdeologyInstalled)
            {
                foreach (FactionDef x in DefDatabase<FactionDef>.AllDefsListForReading.Where(x => x.allowedMemes.NullOrEmpty()))
                {
                    if (x.disallowedMemes == null)
                    {
                        x.disallowedMemes = new List<MemeDef>();
                    }
                    x.disallowedMemes.Add(Ideology_Mofy.Mofy_Mofy);
                }
            }

        }
    }

    // 建築スキルへの理解を強化
    [HarmonyPatch(typeof(SkillRecord), "LearnRateFactor")]
    [HarmonyPatch(new Type[]
    {
        typeof(bool),
    })]
    internal class Mofy_FixSkillRecord
    {
        [HarmonyPrefix]
        private static void Postfix(ref float __result, ref SkillRecord __instance, bool direct)
        {
            if (direct == false)
            {
                if (__instance.def == SkillDefOf.Construction)
                {
                    if (ModsConfig.IdeologyActive)
                    {
                        if (__instance.Pawn?.ideo?.Ideo?.memes?.Contains(Ideology_Mofy.Mofy_Mofy) ?? false)
                        {
                            __result *= 3.0f;
                        }
                    }
                }
            }
        }
    }

    // モフィーの強化
    [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
        typeof(SkillDef),
    })]
    internal class LitF_FixGenerateQuality
    {
        [HarmonyPostfix]
        private static void Postfix(ref QualityCategory __result, Pawn pawn, SkillDef relevantSkill)
        {
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Mofy_BlacksmithInstruction")))
            {
                if ((int)__result <= (int)QualityCategory.Good)
                {
                    __result = QualityCategory.Good;
                }
                if (Rand.Range(0, 3) == 0)
                {
                    Messages.Message("Mofy.UI.Instruction".Translate(pawn), pawn, MessageTypeDefOf.PositiveEvent, false);
                    __result = (QualityCategory)Mathf.Min((int)__result + 1, Enum.GetNames(typeof(QualityCategory)).Length);
                }
            }
        }
    }

}
