using UnityEngine;
using System.Collections.Generic;

public enum SkillRarity
{
    Common,     // 일반
    Uncommon,   // 고급
    Rare,       // 희귀
    Epic,       // 영웅
    Legendary,  // 전설
    Mythic      // 신화
}

[CreateAssetMenu(fileName = "SO_SkillData_", menuName = "SO/SkillData", order = 2)]
public class SkillDataSO : ScriptableObject
{

    [Header("스킬 기본 정보")]
    public string skillName;            // 스킬 이름
    public string skillDescription;     // 스킬 설명
    public Sprite skillIcon;            // 스킬 아이콘
    public ElementType elementType;     // 스킬 속성
    public int skillMaxLevel;           // 스킬 최대 레벨
    public int skillCooldown;           // 스킬 쿨타임
    public SkillRarity skillRarity;     // 스킬 희귀도

    [Header("스킬 효과 목록")]
    public DamageEffect damageEffect;           // 피해 효과
    // 향후 추가될 다른 효과들
    // public HealEffect healEffect;
    // public BuffEffect buffEffect;




}
