using UnityEngine;

[System.Serializable]
public enum UnitType
{
    None,   // 없음
    Common, // 일반
    Elite,  // 엘리트
    Boss    // 보스
}

[CreateAssetMenu(fileName = "SO_UnitData_NewUnit", menuName = "SO/UnitData", order = 1)]
public class UnitDataSO : ScriptableObject
{
    [Header("유닛 기본 정보")]
    public string unitName; // 유닛 이름
    public string unitDescription; // 유닛 설명
    public GameObject unitPrefab; // 유닛 프리팹 


    [Header("유닛 타입")]
    public UnitType unitType; // 유닛 등급 (일반, 엘리트, 보스)
    public int threatLevel;   // 위협 수준 (난이도)

    [Header("유닛 스탯")]
    public CharacterStats characterStats; // 유닛의 상세 스탯
}
