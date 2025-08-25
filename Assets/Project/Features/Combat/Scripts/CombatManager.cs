using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance {get; private set;}

    public ElementalMatchupTableSO elementalMatchupTable;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
 