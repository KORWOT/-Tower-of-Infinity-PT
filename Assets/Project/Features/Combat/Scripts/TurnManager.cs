using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnManager : MonoBehaviour
{

    public static TurnManager Instance;

    [SerializeField] private List<InGameUnit> units;

    private int readinessThreshold = 10000;

    private bool isCombatActive = false;

    private InGameUnit currentTurnUnit; // 현재 턴 유닛

    public void Awake()
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

    public void StartCombat()
    {
        units = FindObjectsByType<InGameUnit>(FindObjectsSortMode.None).ToList();
        isCombatActive = true;
        LogManager.Log("전투가 시작되었습니다.");
        
        // 코루틴으로 턴 진행 시작
        StartCoroutine(TurnProgressCoroutine());
    }

    /// <summary>
    /// 코루틴으로 턴 진행을 처리하여 성능 최적화
    /// </summary>
    private IEnumerator TurnProgressCoroutine()
    {
        while (isCombatActive)
        {
            // 매 프레임 대신 0.1초마다 실행하여 성능 최적화
            yield return new WaitForSeconds(0.1f);
            
            ProcessTurnProgress();
        }
    }
    
    /// <summary>
    /// 턴 진행 로직 처리
    /// </summary>
    private void ProcessTurnProgress()
    {
        if (!isCombatActive) return;
        
        foreach (var unit in units)
        {
            // Null 체크 추가
            if (unit == null || unit.currentStats == null)
            {
                LogManager.LogError("유닛 또는 스탯 정보가 null입니다.");
                continue;
            }
            
            unit.readiness += unit.currentStats.attackSpeed;

            if (unit.readiness >= readinessThreshold)
            {
                isCombatActive = false; // 턴게이지 일시정지

                currentTurnUnit = unit;
                currentTurnUnit.readiness -= readinessThreshold;

                LogManager.Log($"{unit.unitData.unitName}의 턴 입니다.");

                // TODO: 여기서 실제 유닛이 행동하도록 명령해야 합니다.
                // (예: 만약 currentTurnUnit이 플레이어라면, UI를 활성화)

                break;
            }
        }
    }

    private int GetTicksToTurn(InGameUnit unit) 
    {
        if (unit.currentStats.attackSpeed <= 0)     
        {
            return int.MaxValue; // 속도가 0이면 턴이 오지 않으므로 무한대로 취급
        }
        return (readinessThreshold - unit.readiness) / (int)unit.currentStats.attackSpeed;  // 턴 순서 계산
}


    public List<InGameUnit> PredictTurnOrder()
    {
        var predictList = units.OrderBy(unit => GetTicksToTurn(unit)).ToList();  // 턴 순서 예측
        return predictList;  // 턴 순서 반환
    }

    public void EndTurn()
    {
        if (currentTurnUnit != null)
        {
            LogManager.Log($"{currentTurnUnit.unitData.unitName}의 턴이 끝났습니다.");
        }

        currentTurnUnit = null; // 현재 턴 유닛 초기화
        isCombatActive = true; // 턴게이지 활성화
        
        // 코루틴이 중단되어 있었다면 다시 시작
        if (isCombatActive)
        {
            StartCoroutine(TurnProgressCoroutine());
        }
    }

    public List<InGameUnit> GetEnemyUnits()
    {
        return units.Where(unit => unit.unitFaction == UnitFaction.Enemy).ToList();
    }
}
