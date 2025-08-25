using Unity.Mathematics;
using UnityEngine;
using System;

public static class DamageCalculator
{
    private const int BP = 10000;      // 100% = 10000
    private const int PEN_MAX_BP = 6000; // 관통률 상한 60%

    public static long CalculateFinalDamage(InGameUnit attacker, InGameUnit target, SkillDataSO skill, DamageEffect effects, int skillLevel, ElementalMatchupTableSO elementalMatchupTable)
    {   // --- 1. 주는 피해량 계산 ---
        long attackPower = attacker.currentStats.attackPower;   // 공격력
        long skillCoefficient = effects.skillCoefficient;   // 스킬 계수
        long scalavelCoefficient = effects.scalavelCoefficient;  // 스케일 가능한 계수
        long skillStaticDamage = effects.skillStaticDamage;  // 고정 피해
        long scalavelStaticDamage = effects.scalavelStaticDamage;   // 스케일 가능한 고정 피해
        double damageIncrease = attacker.currentStats.damageIncrease;   // 피해 증가
        double elementalDamageBonus = attacker.currentStats.GetElementalDamageBonus(skill.elementType); // 속성 피해 증가
    


        long finalSkillCoefficient = skillCoefficient + scalavelCoefficient*(skillLevel - 1); 
        long finalStaticDamage = skillStaticDamage + scalavelStaticDamage*(skillLevel - 1); 

        long finalDamage = (long)(attackPower * (finalSkillCoefficient/100.0)) + finalStaticDamage; // 스킬계수는 100을 기준으로 계산(10000을 기준으로 계산하면 문제 발생)

        // --- 2. 치명타 여부 및 피해량 계산 ---
        double criticalChance = System.Math.Max(0, System.Math.Min(BP, attacker.currentStats.criticalChance));     // 치명타 확률
        double criticalDamageMultiplier = attacker.currentStats.criticalDamageMultiplier;   // 치명타 배율
        bool isCritical = UnityEngine.Random.Range(0, BP) < criticalChance;   // 치명타 여부

      

        long criticalAfterDamage = CriticalAfterDamage(finalDamage, isCritical, criticalDamageMultiplier);  // 치명타 계산 후 피해량
        long elementalAfterDamage = (long)(criticalAfterDamage * (1.0 + elementalDamageBonus/BP));  // 속성 피해 증가 계산 후 피해량
        long damageIncreaseAfterDamage = (long)(elementalAfterDamage * (1.0 + damageIncrease/BP));  // 피해 증가 계산 후 피해량
            

        // --- 3. 받는 피해량 계산 ---
        long targetDefense = target.currentStats.defense;   // 방어력
        double targetProtectionRate = System.Math.Max(0, System.Math.Min(BP, target.currentStats.protectionRate));   // 보호율
        double targetDamageReductionRate = System.Math.Max(0, System.Math.Min(BP, target.currentStats.damageReductionRate));   // 피해감소율
        long targetDamageReduction = target.currentStats.damageReduction;   // 고정 피해감소
        long attackerPenetration = attacker.currentStats.penetration;   // 관통력
        double attackerPenetrationRate = System.Math.Max(0, System.Math.Min(PEN_MAX_BP, attacker.currentStats.penetrationRate));   // 관통력% (상한 60%)

        // 관통력 적용
        long targetDefenseAfterPenetration = System.Math.Max(0, (long)(targetDefense * (1.0 - attackerPenetrationRate/BP) - attackerPenetration));  // 관통력 적용 후 방어력
        long damageAfterDefense = (long)(damageIncreaseAfterDamage - targetDefenseAfterPenetration);  // 관통력 적용 후 피해량


        // 규칙 1 적용: 데미지가 1 미만이면 1로 보정
        damageAfterDefense = (long)System.Math.Max(1, damageAfterDefense);  // 데미지가 1 미만이면 1로 보정

        // 보호율, 피해감소율 적용
        double protectionRate = 1.0 - (targetProtectionRate/BP);  // 보호율 적용
        double damageReductionRate = 1.0 - (targetDamageReductionRate/BP);  // 피해감소율 적용

        long damageAfterRate = (long)(damageAfterDefense * protectionRate * damageReductionRate);  // 보호율, 피해감소율 적용 후 피해량

        // 고정 피해감소 적용
        long takeDamage = damageAfterRate - targetDamageReduction;  

        // --- 4. 최종 피해량 계산 ---
        // 규칙 2 적용 : 최소 데미지는 '주는 피해량'의 10% 이상이어야 함
        long minDamage = (long)(damageIncreaseAfterDamage * 0.1);
        long finalTakeDamage = System.Math.Max(minDamage, takeDamage);


        // --- 5. 원소 상성 적용 ---
        ElementType attackerElement = skill.elementType;    //공격자 속성 타입
        ElementType targetElement = target.currentStats.elementType;    //방어자 속성 타입

        float elementalDamageMultiplier = elementalMatchupTable.GetElementalDamageMultiplier(attackerElement, targetElement);  // 원소 상성 적용

        long MultiplierAfterTakeDamage = (long)(finalTakeDamage * elementalDamageMultiplier);  // 원소 상성 적용 후 피해량

        long absoluteFinalDamage = System.Math.Max(1L, MultiplierAfterTakeDamage);  // 최소 데미지는 1 이상이어야 함

        return absoluteFinalDamage;  // 최종 피해량 반환
    }

    public static long CriticalAfterDamage(long finalDamage, bool isCritical, double criticalDamageMultiplier)
    {
        if(isCritical == true)
        {
            return (long)(finalDamage * (criticalDamageMultiplier/BP));
        }
        else
        {
            return finalDamage;
        }
    }

    
}
