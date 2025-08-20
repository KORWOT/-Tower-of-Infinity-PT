using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ElementalMatchup
{
    public ElementType attackingElement;
    public ElementType defendingElement;
    public float damageMultiplier =  1.0f;
}

[CreateAssetMenu(fileName = "SO_ElementalMatchupTable", menuName = "SO/ElementalMatchupTable", order = 3)]
public class ElementalMatchupTableSO : ScriptableObject
{
     // 에디터에서 설정할 상성 목록
    public List<ElementalMatchup> elementalMatchups;

    // 빠른 조회를 위해 List를 Dictionary 형태로 변환해서 저장할 변수
    private Dictionary<(ElementType, ElementType), float> _matchupDict;

    public float GetElementalDamageMultiplier(ElementType attacker, ElementType defender)
    {
        // 맨 처음 조회할 때, List를 Dictionary로 변환해서 성능을 최적화합니다.
        if(_matchupDict == null)
        {
            _matchupDict = new Dictionary<(ElementType, ElementType), float>();
            foreach(var matchup in elementalMatchups)
            {
                _matchupDict[(matchup.attackingElement, matchup.defendingElement)] = matchup.damageMultiplier;
            }
        }
        // Dictionary에서 배율을 찾아보고, 없으면 기본값인 1.0f를 반환합니다.
        if(_matchupDict.TryGetValue((attacker, defender), out float multiplier))
        {
            return multiplier;
        }
        return 1.0f;
    }

}
