using Unity.Mathematics;
using UnityEngine;

public static class DamageCalculator
{

    public static long CalculateFinalDamage(InGameUnit attacker, InGameUnit target, SkillDataSO skill, DamageEffect effects, int skillLevel, ElementalMatchupTableSO elementalMatchupTable)
    {   // --- 1. 주는 피해량 계산 ---
        long attackPower = attacker.currentStats.attackPower;   // 공격력
        long skillCoefficient = effects.skillCoefficient;   // 스킬 계수
        long scalavelCoefficient = effects.scalavelCoefficient;  // 스케일 가능한 계수
        long skillStaticDamage = effects.skillStaticDamage;  // 고정 피해
        long scalavelStaticDamage = effects.scalavelStaticDamage;   // 스케일 가능한 고정 피해

        long finalSkillCoefficient = skillCoefficient + scalavelCoefficient*(skillLevel - 1);
        long finalStaticDamage = skillStaticDamage + scalavelStaticDamage*(skillLevel - 1);

        long finalDamage = (long)(attackPower * (finalSkillCoefficient/100.0f)) + finalStaticDamage;

        // --- 2. 받는 피해량 계산 ---
        long targetDefense = target.currentStats.defense;   // 방어력
        long targetProtectionRate = target.currentStats.protectionRate;   // 보호율
        long targetDamageReductionRate = target.currentStats.damageReductionRate;   // 피해감소율
        long targetDamageReduction = target.currentStats.damageReduction;   // 고정 피해감소

        long damageAfterDefense = (long)(finalDamage - targetDefense);

        // 규칙 1 적용: 데미지가 1 미만이면 1로 보정
        damageAfterDefense = (long)Mathf.Max(1, damageAfterDefense);

        // 보호율, 피해감소율 적용
        float protectionRate = 1.0f - (targetProtectionRate/100.0f);
        float damageReductionRate = 1.0f - (targetDamageReductionRate/100.0f);

        long damageAfterRate = (long)(damageAfterDefense * protectionRate * damageReductionRate);

        // 고정 피해감소 적용
        long takeDamage = damageAfterRate - targetDamageReduction;

        // --- 3. 최종 피해량 계산 ---
        // 규칙 2 적용 : 최소 데미지는 '주는 피해량'의 10% 이상이어야 함
        long minDamage = (long)(finalDamage * 0.1f);
        long finalTakeDamage = (long)Mathf.Max(minDamage, takeDamage);


        // --- 4. 원소 상성 적용 ---
        ElementType attackerElement = skill.elementType;
        ElementType targetElement = target.currentStats.elementType;
        float elementalDamageMultiplier = elementalMatchupTable.GetElementalDamageMultiplier(attackerElement, targetElement);

        long MultiplierAfterTakeDamage = (long)(finalTakeDamage * elementalDamageMultiplier);

        MultiplierAfterTakeDamage = (long)Mathf.Max(1, MultiplierAfterTakeDamage);

        return MultiplierAfterTakeDamage;
    }

    
}
