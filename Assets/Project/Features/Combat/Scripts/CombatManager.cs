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
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
