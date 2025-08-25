using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;   
using System.Collections.Generic;
using System.Linq;




public class SkillButtonUI : MonoBehaviour
{

    public Image skillIcon;
    public TextMeshProUGUI skillName;
    
    public InGameUnit unit;
    public SkillDataSO skillData;

    public TurnManager turnManager;

    public int skillLevel;


    public void SkillSetup(InGameUnit unit, SkillDataSO skillData, int skillLevel)
    {
        this.unit = unit;
        this.skillData = skillData;
        this.skillLevel = skillLevel;

        // 스킬 버튼 텍스트 설정
        GetComponent<TextMeshProUGUI>().text = skillData.skillName;
        
        // 스킬 버튼 이미지 설정
        GetComponent<Image>().sprite = skillData.skillIcon;

        // 스킬 버튼 클릭 이벤트 설정
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        List<InGameUnit> enemies = new List<InGameUnit>();
        enemies = TurnManager.Instance.GetEnemyUnits();   // 적 유닛 리스트

        if(enemies == null || enemies.Count == 0)
        {
            LogManager.LogError("적 유닛이 없습니다.");
            return;
        }

        InGameUnit target = enemies.First();

        skillData.skillEffects.ForEach(effect => effect.ExecuteEffect(unit.gameObject, target.gameObject, skillData, skillLevel));

        TurnManager.Instance.EndTurn();


    }
}
