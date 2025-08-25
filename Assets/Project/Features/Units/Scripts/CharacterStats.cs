using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public enum ArmorType
{
    None,      // 없음
    Light,     // 경갑
    Medium,    // 평갑
    Heavy      // 중갑
}

public enum ElementType
{
    None,      // 없음
    Water,     // 물
    Fire,      // 불
    Wind,      // 바람
    Earth,     // 땅
    Lightning, // 전기
    Dark,      // 암
    Light,     // 빛
    Normal,    // 노말
}

[System.Serializable]
public class ElementalDamageBonus
{
    public ElementType elementType;
    public long damageBonusRate;
}


[System.Serializable]
public class CharacterStats
{
    
    [Header("유닛 속성/ 방어 타입")]
    public ArmorType armorType;   // 방어구 타입
    public ElementType elementType; // 속성 타입

    [Header("기본 생명력 / 자원")]
    public long maxHealth; // 최대 체력
    public long currentHealth; // 현재 체력
    public long maxMana; // 최대 마나
    public long currentMana; // 현재 마나

    [Header("공격관련 스텟")]
    // 공격 관련 스탯
    public long attackPower; // 공격력
    public long criticalChance; // 치명타율
    public long criticalDamageMultiplier; // 치명타 배수
    public long damageIncrease; // 피해 증가
    public long manaRegeneration; // 마나 회복량
    public long penetrationRate; // 관통력%
    public long penetration; // 관통력
    public long manaOnKill; // 처치 시 마나 회복
    public long lifeSteal; // 흡혈율
    public int attackSpeed; // 공격 속도 (턴 속도 결정)

    [Header("방어관련 스텟")]
    // 방어 관련 스탯
    public long defense; // 방어력
    public long protectionRate; // 보호율
    public long damageReductionRate; // 피해 감소율
    public long damageReduction; // 피해 감소
    public long evasionRate; // 회피율
    public long recoveryAmount; // 회복량
    public long health; // 생명력 (고정 수치 증가 등)
    public long healthOnKill; // 처치 시 생명력 회복


    [Header("직업 특수 스텟")]
    // 특수 스텟(직업별 특수 스텟 )
    public long placeHolder; // 임시 변수

    [Header("속성 피해 증가")]
    public List<ElementalDamageBonus> elementalDamageBonuses;

    public long GetElementalDamageBonus(ElementType elementType)
    {
        // Null 안전성 체크
        if (elementalDamageBonuses == null)
        {
            LogManager.LogError("ElementalDamageBonuses 리스트가 null입니다!");
            return 0;
        }
        
        var elementalDamageBonus = elementalDamageBonuses.FirstOrDefault(b => b.elementType == elementType); // 속성 피해 증가 찾기
        return elementalDamageBonus != null ? elementalDamageBonus.damageBonusRate : 0; // 속성 피해 증가 반환
    }

    // 자신과 똑같은 내용의 새 CharacterStats 객체를 만들어 반환하는 함수
    public CharacterStats Clone()
    {
        CharacterStats newStats = this.MemberwiseClone() as CharacterStats;
        
        // Null 안전성 체크
        if (this.elementalDamageBonuses != null)
        {
            newStats.elementalDamageBonuses = new List<ElementalDamageBonus>(this.elementalDamageBonuses);
        }
        else
        {
            newStats.elementalDamageBonuses = new List<ElementalDamageBonus>();
            LogManager.LogError("원본 CharacterStats의 elementalDamageBonuses가 null입니다. 빈 리스트로 초기화합니다.");
        }
        
        return newStats;
    }
}
