using UnityEngine;

/// <summary>
/// 모든 스킬 효과의 기반이 되는 추상 클래스입니다.
/// </summary>
[System.Serializable]
public abstract class SkillEffect
{
    /// <summary>
    /// 스킬 효과를 실행하는 메서드입니다.
    /// </summary>
    /// <param name="attacker">스킬을 시전하는 게임 오브젝트</param>
    /// <param name="target">스킬의 대상이 되는 게임 오브젝트</param>
    public abstract void ExecuteEffect(GameObject attacker, GameObject target, SkillDataSO skill, int skillLevel);
}
