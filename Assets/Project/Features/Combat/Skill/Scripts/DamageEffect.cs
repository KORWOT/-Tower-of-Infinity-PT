using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageEffect : SkillEffect
{
    public long skillCoefficient;
    public long scalavelCoefficient;
    public long skillStaticDamage;
    public long scalavelStaticDamage;

    public override void ExecuteEffect(GameObject attacker, GameObject target, SkillDataSO skill, int skillLevel)
    {
        // 1. 입력 파라미터 null 체크
        if (attacker == null)
        {
            LogManager.LogError("공격자 GameObject가 null입니다!");
            return;
        }
        
        if (target == null)
        {
            LogManager.LogError("대상 GameObject가 null입니다!");
            return;
        }
        
        if (skill == null)
        {
            LogManager.LogError("SkillDataSO가 null입니다!");
            return;
        }
        
        // 2. 공격자와 방어자의 InGameUnit 컴포넌트를 가져옵니다.
        var attackerUnit = attacker.GetComponent<InGameUnit>();
        var targetUnit = target.GetComponent<InGameUnit>();

        if (attackerUnit == null)
        {
            LogManager.LogError($"{attacker.name}에서 InGameUnit 컴포넌트를 찾을 수 없습니다!");
            return;
        }
        
        if (targetUnit == null)
        {
            LogManager.LogError($"{target.name}에서 InGameUnit 컴포넌트를 찾을 수 없습니다!");
            return;
        }
        
        // 3. CombatManager 인스턴스 체크
        if (CombatManager.Instance == null)
        {
            LogManager.LogError("CombatManager 인스턴스가 없습니다!");
            return;
        }
        
        if (CombatManager.Instance.elementalMatchupTable == null)
        {
            LogManager.LogError("ElementalMatchupTable이 할당되지 않았습니다!");
            return;
        }

        // 4. 전문가에게 모든 재료를 넘겨주고 계산을 요청합니다.
        long finalDamage = DamageCalculator.CalculateFinalDamage(attackerUnit, targetUnit, skill, this, skillLevel, CombatManager.Instance.elementalMatchupTable);

        // 5. 실제 피해를 받는 유닛에게 피해를 적용합니다.
        targetUnit.TakeDamage(finalDamage);
    }




} 
