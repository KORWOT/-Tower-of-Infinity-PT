using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;   // 싱글톤 인스턴스

    public List<SkillButtonUI> SkillSlots;

    public GameObject skillButtonPrefab;   // 스킬 버튼 프리팹

    public Transform skillButtonParent;   // 스킬 버튼 부모 트랜스폼

    public void Awake()   // 싱글톤 인스턴스 생성
    {
        if(Instance == null)
        {
            Instance = this;    
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void DisplaySkillButtons(InGameUnit playerUnit)   // 스킬 버튼 생성
    {
        List<InGameUnit.LearnedSkill> skillList = playerUnit.learnedSkills;   // 유닛이 배운 스킬 리스트
        
        var slots = this.SkillSlots;

        for(int i = 0; i < slots.Count; i++)
        {
            if(i < skillList.Count)
            {
               slots[i].SkillSetup(playerUnit, skillList[i].skill, skillList[i].skillLevel);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }


    }


}
