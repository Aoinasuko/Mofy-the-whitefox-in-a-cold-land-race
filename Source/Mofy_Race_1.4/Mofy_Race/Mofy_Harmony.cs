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

    // 肉を人間扱いにしなくなる
    [HarmonyPatch(typeof(FoodUtility), "IsHumanlikeCorpseOrHumanlikeMeat")]
    static class Mofy_FixFoodUtility
    {
        [HarmonyPrefix]
        public static bool Fix_IsHumanlike(ref bool __result, ref Thing source, ref ThingDef foodDef)
        {
            Corpse corpse = source as Corpse;
            if (corpse != null && (corpse.InnerPawn.def.race.FleshType == FleshType_Mofy.Mofy))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "GetMeatSourceCategoryFromCorpse")]
    [HarmonyPatch(new Type[]
    {
        typeof(Thing)
    })]
    static class Mofy_FixFoodUtility_GetMeatSourceCategoryCorpse
    {
        [HarmonyPrefix]
        public static bool Fix_IsHumanlike(ref MeatSourceCategory __result, Thing thing)
        {
            Corpse corpse = thing as Corpse;
            if (corpse != null && corpse.InnerPawn.def.race.FleshType == FleshType_Mofy.Mofy)
            {
                __result = MeatSourceCategory.Undefined;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "GetMeatSourceCategory")]
    [HarmonyPatch(new Type[]
    {
        typeof(ThingDef)
    })]
    static class Mofy_FixFoodUtility_GetMeatSourceCategory
    {
        [HarmonyPrefix]
        public static bool Fix_IsHumanlike(ref MeatSourceCategory __result, ThingDef source)
        {
            if (source.ingestible == null)
            {
                return true;
            }
            if ((source.ingestible.foodType & FoodTypeFlags.Meat) == FoodTypeFlags.Meat)
            {
                if (source.ingestible.sourceDef != null)
                {
                    if (source.ingestible.sourceDef.race.FleshType == FleshType_Mofy.Mofy)
                    {
                        __result = MeatSourceCategory.Undefined;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Corpse), "ButcherProducts")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(float) })]
    static class Mofy_Corpse_Patch
    {

        [HarmonyBefore(new string[] { "rimworld.erdelf.alien_race.main" })]
        [HarmonyPrefix]
        static bool Prefix(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {

            if (__instance.InnerPawn.def.race.FleshType == FleshType_Mofy.Mofy)
            {
                Pawn corpse = __instance.InnerPawn;
                IEnumerable<Thing> enumerable = corpse.ButcherProducts(butcher, efficiency);
                if (corpse.RaceProps.BloodDef != null)
                {
                    FilthMaker.TryMakeFilth(butcher.Position, butcher.Map, corpse.RaceProps.BloodDef, corpse.LabelIndefinite());
                }
                __result = enumerable;
                return false;
            }
            return true;
        }

    }

    /// <summary>
    /// 脱走しない
    /// </summary>
    [HarmonyPatch(typeof(PrisonBreakUtility), "InitiatePrisonBreakMtbDays")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
        typeof(StringBuilder),
        typeof(bool)
    })]
    static class FixPrisonBreak
    {
        [HarmonyPrefix]
        static bool Prefix(ref float __result, Pawn pawn, StringBuilder sb, bool ignoreAsleep)
        {
            if (pawn.def.defName == "Mofy_Pawn")
            {
                __result = -1f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SlaveRebellionUtility), "InitiateSlaveRebellionMtbDays")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
    })]
    static class FixSlaveRebellion
    {
        [HarmonyPrefix]
        static bool Prefix(ref float __result, Pawn pawn)
        {
            if (pawn.def.defName == "Mofy_Pawn")
            {
                __result = -1f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SlaveRebellionUtility), "InitiateSlaveRebellionMtbDaysHelper")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
    })]
    static class FixSlaveRebellion_Fix2
    {
        [HarmonyPrefix]
        static bool Prefix(ref float __result, Pawn pawn)
        {
            if (pawn.def.defName == "Mofy_Pawn")
            {
                __result = -1f;
                return false;
            }
            return true;
        }
    }

    // モフィーの派閥追加時、服装規定を無効化する
    [HarmonyPatch(typeof(Precept_Role), "GenerateNewApparelRequirements")]
    [HarmonyPatch(new Type[]
    {
        typeof(FactionDef),
    })]
    internal static class ApparelRequirement_Patch
    {

        [HarmonyPrefix]
        static bool Prefix(ref Precept_Role __instance, ref List<PreceptApparelRequirement> __result, FactionDef generatingFor)
        {
            if (generatingFor != null)
            {
                if (generatingFor.defName == "Mofy_Kingdom")
                {
                    List<PreceptApparelRequirement> ApparelRequirement = new List<PreceptApparelRequirement>();
                    // 役割のdefName取得
                    String role = __instance.def.defName;
                    // 役割によって装備を指定
                    if (role == "IdeoRole_Leader")
                    {
                        PreceptApparelRequirement item = new PreceptApparelRequirement();
                        item.requirement = new ApparelRequirement();
                        item.requirement.bodyPartGroupsMatchAny = new List<BodyPartGroupDef>();
                        item.requirement.requiredDefs = new List<ThingDef>();
                        item.requirement.bodyPartGroupsMatchAny.Add(BodyPartGroupDefOf.Torso);
                        item.requirement.requiredDefs.Add(ThingDef.Named("Mofy_NobleCape"));
                        ApparelRequirement.Add(item);
                    }
                    if (!ApparelRequirement.NullOrEmpty())
                    {
                        __result = ApparelRequirement;
                    }
                    else
                    {
                        __result = null;
                    }
                    return false;
                }
                if (generatingFor.defName == "Mofy_WildMofy")
                {
                    List<PreceptApparelRequirement> ApparelRequirement = new List<PreceptApparelRequirement>();
                    // 役割のdefName取得
                    String role = __instance.def.defName;
                    // 役割によって装備を指定
                    if (!ApparelRequirement.NullOrEmpty())
                    {
                        __result = ApparelRequirement;
                    }
                    else
                    {
                        __result = null;
                    }
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 奴隷時に作業速度ボーナス
    /// <summary>
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(StatPart_Slave))]
    internal static class TransformValue_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StatPart_Slave.TransformValue),
            new Type[] { typeof(StatRequest), typeof(float) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
        static bool Prefix(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
                if (req.Thing as Pawn != null)
                {
                    Pawn pawn = (Pawn)req.Thing;
                    if (pawn.def.defName == "Mofy_Pawn")
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 奴隷時のボーナス説明
    /// <summary>
    [HarmonyPatch(typeof(StatPart_Slave), "ExplanationPart")]
    [HarmonyPatch(new Type[]
    {
        typeof(StatRequest),
    })]
    internal static class ExplanationPart_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ref StatPart_Slave __instance, ref String __result, StatRequest req)
        {
            if (req.HasThing)
            {
                if (req.Thing as Pawn != null)
                {
                    Pawn pawn = (Pawn)req.Thing;
                    if (pawn.def.defName == "Mofy_Pawn")
                    {
                        return false;
                    }
                }
            }
            return true;
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

    /// <summary>
    /// 年齢補正無効化
    /// <summary>
    [HarmonyPatch(typeof(StatPart_Age), "AgeMultiplier")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
    })]
    internal static class AgeMultiplier_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ref StatPart_Age __instance, ref float __result, Pawn pawn)
        {
            if (pawn.def.defName == "Mofy_Pawn")
            {
                __result = 1.0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StatPart_AgeOffset), "AgeOffset")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
    })]
    internal static class AgeMultiplier_Patch_Offset
    {
        [HarmonyPrefix]
        static bool Prefix(ref StatPart_AgeOffset __instance, ref float __result, Pawn pawn)
        {
            if (pawn.def.defName == "Mofy_Pawn")
            {
                __result = 0f;
                return false;
            }
            return true;
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
