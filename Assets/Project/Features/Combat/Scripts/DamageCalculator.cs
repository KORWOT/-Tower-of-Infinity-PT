using Unity.Mathematics;
using UnityEngine;
using System;

public static class DamageCalculator
{

    public static long CalculateFinalDamage(InGameUnit attacker, InGameUnit target, SkillDataSO skill, DamageEffect effects, int skillLevel, ElementalMatchupTableSO elementalMatchupTable)
    {   // --- 1. 주는 피해량 계산 ---
        long attackPower = attacker.currentStats.attackPower;   // 공격력
        long skillCoefficient = effects.skillCoefficient;   // 스킬 계수
        long scalavelCoefficient = effects.scalavelCoefficient;  // 스케일 가능한 계수
        long skillStaticDamage = effects.skillStaticDamage;  // 고정 피해
        long scalavelStaticDamage = effects.scalavelStaticDamage;   // 스케일 가능한 고정 피해
        long damageIncrease = attacker.currentStats.damageIncrease;   // 피해 증가
    


        long finalSkillCoefficient = skillCoefficient + scalavelCoefficient*(skillLevel - 1);
        long finalStaticDamage = skillStaticDamage + scalavelStaticDamage*(skillLevel - 1);

        long finalDamage = (long)(attackPower * (finalSkillCoefficient/100.0f)) + finalStaticDamage;

        // --- 2. 치명타 여부 및 피해량 계산 ---
        long criticalChance = attacker.currentStats.criticalChance;     // 치명타 확률
        long criticalDamageMultiplier = attacker.currentStats.criticalDamageMultiplier;   // 치명타 배율
        bool isCritical = UnityEngine.Random.Range(0, 100) < criticalChance;   // 치명타 여부

      

        long criticalAfterDamage = CriticalAfterDamage(finalDamage, isCritical, criticalDamageMultiplier);  // 치명타 계산 후 피해량
        long damageIncreaseAfterDamage = (long)(criticalAfterDamage * (damageIncrease/100.0f));  // 피해 증가 계산 후 피해량
        

        // --- 3. 받는 피해량 계산 ---
        long targetDefense = target.currentStats.defense;   // 방어력
        long targetProtectionRate = target.currentStats.protectionRate;   // 보호율
        long targetDamageReductionRate = target.currentStats.damageReductionRate;   // 피해감소율
        long targetDamageReduction = target.currentStats.damageReduction;   // 고정 피해감소
        long attackerPenetration = attacker.currentStats.penetration;   // 관통력
        long attackerPenetrationRate = attacker.currentStats.penetrationRate;   // 관통력%


        // 관통력 적용
        long damageAfterDefense = (long)(damageIncreaseAfterDamage - ((targetDefense * (attackerPenetrationRate/100.0f)) - attackerPenetration));

        // 규칙 1 적용: 데미지가 1 미만이면 1로 보정
        damageAfterDefense = (long)Mathf.Max(1, damageAfterDefense);

        // 보호율, 피해감소율 적용
        float protectionRate = 1.0f - (targetProtectionRate/100.0f);
        float damageReductionRate = 1.0f - (targetDamageReductionRate/100.0f);

        long damageAfterRate = (long)(damageAfterDefense * protectionRate * damageReductionRate);

        // 고정 피해감소 적용
        long takeDamage = damageAfterRate - targetDamageReduction;

        // --- 4. 최종 피해량 계산 ---
        // 규칙 2 적용 : 최소 데미지는 '주는 피해량'의 10% 이상이어야 함
        long minDamage = (long)(criticalAfterDamage * 0.1f);
        long finalTakeDamage = (long)Mathf.Max(minDamage, takeDamage);


        // --- 5. 원소 상성 적용 ---
        ElementType attackerElement = skill.elementType;    //공격자 속성 타입
        ElementType targetElement = target.currentStats.elementType;    //방어자 속성 타입

        float elementalDamageMultiplier = elementalMatchupTable.GetElementalDamageMultiplier(attackerElement, targetElement);

        long MultiplierAfterTakeDamage = (long)(finalTakeDamage * elementalDamageMultiplier);

        MultiplierAfterTakeDamage = (long)Mathf.Max(1, MultiplierAfterTakeDamage);

        return MultiplierAfterTakeDamage;
    }

    public static long CriticalAfterDamage(long finalDamage, bool isCritical, long criticalDamageMultiplier)
    {
        if(isCritical == true)
        {
            return (long)(finalDamage * (criticalDamageMultiplier/100.0f));
        }
        else
        {
            return finalDamage;
        }
    }

    
}
