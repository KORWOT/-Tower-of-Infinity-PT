using UnityEngine;
using System;

public static class DamageCalculator
{
    private const int BP = 10000;      // 100% = 10000
    private const int PEN_MAX_BP = 6000; // 관통률 상한 60%

    /// <summary>
    /// 최종 데미지를 계산하는 메인 메서드 - 명확한 5단계로 구성
    /// </summary>
    public static long CalculateFinalDamage(InGameUnit attacker, InGameUnit target, SkillDataSO skill, DamageEffect effects, int skillLevel, ElementalMatchupTableSO elementalMatchupTable)
    {
        // 1단계: 기본 데미지 계산 (공격력 * 스킬계수 + 고정피해)
        long baseDamage = CalculateBaseDamage(attacker, skill, effects, skillLevel);
        
        // 2단계: 치명타 적용
        long criticalDamage = ApplyCritical(baseDamage, attacker);
        
        // 3단계: 피해 증가 및 속성 보너스 적용
        long enhancedDamage = ApplyDamageEnhancements(criticalDamage, attacker, skill);
        
        // 4단계: 방어력, 관통력, 보호율 등 방어 계산
        long defendedDamage = ApplyDefense(enhancedDamage, attacker, target);
        
        // 5단계: 원소 상성 적용 및 최종 보정
        long finalDamage = ApplyElementalMatchup(defendedDamage, skill, target, elementalMatchupTable);
        
        return finalDamage;
    }

    /// <summary>
    /// 1단계: 기본 데미지 계산 (공격력 * 스킬계수 + 고정피해)
    /// </summary>
    private static long CalculateBaseDamage(InGameUnit attacker, SkillDataSO skill, DamageEffect effects, int skillLevel)
    {
        long attackPower = attacker.currentStats.attackPower;
        long skillCoefficient = effects.skillCoefficient;
        long scalableCoefficient = effects.scalavelCoefficient;
        long skillStaticDamage = effects.skillStaticDamage;
        long scalableStaticDamage = effects.scalavelStaticDamage;

        // 스킬 레벨에 따른 계수 및 고정 피해 증가
        long finalSkillCoefficient = skillCoefficient + scalableCoefficient * (skillLevel - 1);
        long finalStaticDamage = skillStaticDamage + scalableStaticDamage * (skillLevel - 1);

        // 기본 데미지 = 공격력 * 스킬계수(100 기준) + 고정 피해
        long baseDamage = (long)(attackPower * (finalSkillCoefficient / 100.0)) + finalStaticDamage;
        
        return baseDamage;
    }

    /// <summary>
    /// 2단계: 치명타 적용
    /// </summary>
    private static long ApplyCritical(long baseDamage, InGameUnit attacker)
    {
        double criticalChance = System.Math.Max(0, System.Math.Min(BP, attacker.currentStats.criticalChance));
        double criticalDamageMultiplier = attacker.currentStats.criticalDamageMultiplier;
        bool isCritical = UnityEngine.Random.Range(0, BP) < criticalChance;

        if (isCritical)
        {
            return (long)(baseDamage * (criticalDamageMultiplier / BP));
        }
        else
        {
            return baseDamage;
        }
    }

    /// <summary>
    /// 3단계: 피해 증가 및 속성 피해 보너스 적용
    /// </summary>
    private static long ApplyDamageEnhancements(long criticalDamage, InGameUnit attacker, SkillDataSO skill)
    {
        double damageIncrease = attacker.currentStats.damageIncrease;
        double elementalDamageBonus = attacker.currentStats.GetElementalDamageBonus(skill.elementType);

        // 속성 피해 보너스 적용
        long elementalEnhancedDamage = (long)(criticalDamage * (1.0 + elementalDamageBonus / BP));
        
        // 일반 피해 증가 적용
        long finalEnhancedDamage = (long)(elementalEnhancedDamage * (1.0 + damageIncrease / BP));

        return finalEnhancedDamage;
    }

    /// <summary>
    /// 4단계: 방어력, 관통력, 보호율 등 모든 방어 관련 계산
    /// </summary>
    private static long ApplyDefense(long enhancedDamage, InGameUnit attacker, InGameUnit target)
    {
        // 대상의 방어 스탯들
        long targetDefense = target.currentStats.defense;
        double targetProtectionRate = System.Math.Max(0, System.Math.Min(BP, target.currentStats.protectionRate));
        double targetDamageReductionRate = System.Math.Max(0, System.Math.Min(BP, target.currentStats.damageReductionRate));
        long targetDamageReduction = target.currentStats.damageReduction;

        // 공격자의 관통 스탯들
        long attackerPenetration = attacker.currentStats.penetration;
        double attackerPenetrationRate = System.Math.Max(0, System.Math.Min(PEN_MAX_BP, attacker.currentStats.penetrationRate));

        // 관통력 적용으로 방어력 감소
        long effectiveDefense = System.Math.Max(0, (long)(targetDefense * (1.0 - attackerPenetrationRate / BP) - attackerPenetration));
        
        // 방어력 적용
        long damageAfterDefense = enhancedDamage - effectiveDefense;
        
        // 규칙 1: 데미지가 1 미만이면 1로 보정
        damageAfterDefense = System.Math.Max(1, damageAfterDefense);

        // 보호율과 피해감소율 적용
        double protectionMultiplier = 1.0 - (targetProtectionRate / BP);
        double damageReductionMultiplier = 1.0 - (targetDamageReductionRate / BP);
        long damageAfterRates = (long)(damageAfterDefense * protectionMultiplier * damageReductionMultiplier);

        // 고정 피해감소 적용
        long damageAfterReduction = damageAfterRates - targetDamageReduction;

        // 규칙 2: 최소 데미지는 원래 피해량의 10% 이상이어야 함
        long minDamage = (long)(enhancedDamage * 0.1);
        long finalDefendedDamage = System.Math.Max(minDamage, damageAfterReduction);

        return finalDefendedDamage;
    }

    /// <summary>
    /// 5단계: 원소 상성 적용 및 최종 보정
    /// </summary>
    private static long ApplyElementalMatchup(long defendedDamage, SkillDataSO skill, InGameUnit target, ElementalMatchupTableSO elementalMatchupTable)
    {
        ElementType attackerElement = skill.elementType;
        ElementType targetElement = target.currentStats.elementType;

        // 원소 상성 배율 가져오기
        float elementalMultiplier = elementalMatchupTable.GetElementalDamageMultiplier(attackerElement, targetElement);

        // 원소 상성 적용
        long elementalDamage = (long)(defendedDamage * elementalMultiplier);

        // 최종 보정: 최소 1 데미지 보장
        long finalDamage = System.Math.Max(1L, elementalDamage);

        return finalDamage;
    }

    /// <summary>
    /// 레거시 메서드 - 호환성을 위해 유지 (현재는 ApplyCritical에서 내부적으로 처리)
    /// </summary>
    [System.Obsolete("이 메서드는 ApplyCritical로 대체되었습니다. 호환성을 위해 유지됩니다.")]
    public static long CriticalAfterDamage(long finalDamage, bool isCritical, double criticalDamageMultiplier)
    {
        if (isCritical == true)
        {
            return (long)(finalDamage * (criticalDamageMultiplier / BP));
        }
        else
        {
            return finalDamage;
        }
    }
}