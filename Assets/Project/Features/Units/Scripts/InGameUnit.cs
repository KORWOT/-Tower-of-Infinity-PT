using UnityEngine;

public class InGameUnit : MonoBehaviour
{
    [Header("유닛 기본 정보")]
    public UnitDataSO unitData;

    [Header("유닛 실시간 정보")]
    public CharacterStats currentStats;

    void Awake()
    {
        currentStats = unitData.characterStats.Clone();
    }


    public void TakeDamage(long damageAmount)
    {
        currentStats.currentHealth -= damageAmount;
        LogManager.Log($"{gameObject.name}이(가) {damageAmount}의 피해를 입었습니다.");

        if(currentStats.currentHealth <= 0)
        {
            currentStats.currentHealth = 0;
            Die();

        }
    }

    private void Die()
    {
        LogManager.Log($"{gameObject.name}이(가) 사망했습니다.");
        // TODO: 여기에 유닛이 죽었을 때의 로직 (애니메이션, 비활성화 등)을 추가합니다.
    }
}
