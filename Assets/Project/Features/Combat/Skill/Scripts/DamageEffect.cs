using UnityEngine;

public class DamageEffect : SkillEffect
{
    public long skillCoefficient;
    public long scalavelCoefficient;
    public long skillStaticDamage;
    public long scalavelStaticDamage;

    public override void ExecuteEffect(GameObject attacker, GameObject target, SkillDataSO skill, int skillLevel)
    {
    // 1. 공격자와 방어자의 InGameUnit 컴포넌트를 가져옵니다.
    var attackerUnit = attacker.GetComponent<InGameUnit>();
    var targetUnit = target.GetComponent<InGameUnit>();

    if (attackerUnit == null || targetUnit == null) return;

    // 2. 전문가에게 모든 재료를 넘겨주고 계산을 요청합니다.
    long finalDamage = DamageCalculator.CalculateFinalDamage(attackerUnit, targetUnit, skill, this, skillLevel, CombatManager.Instance.elementalMatchupTable);

    // 3. 실제 피해를 받는 유닛에게 피해를 적용합니다.
    targetUnit.TakeDamage(finalDamage);
    
    }




}
