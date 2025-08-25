# **📋 Tower of Infinity 프로젝트 종합 코드 검토 결과**

## **🎯 프로젝트 개요**

* **프로젝트:** Unity 기반 턴제 RPG 게임  
* **아키텍처:** Feature-Based Architecture  
* **검토 대상:** 총 12개 C\# 스크립트 파일  
  * Core: 2개  
  * Combat: 7개  
  * Units: 3개

## **⚠️ 1\. 기능적 문제점 (우선순위별)**

### **🔴 심각한 문제점**

1. **TurnManager.cs \- 성능 및 안정성 문제**  
   * 매 프레임 Update() 호출로 인한 불필요한 성능 저하가 발생합니다.  
   * 모든 유닛의 attackSpeed가 0일 경우, 무한 루프에 빠질 가능성이 있습니다.  
   * readiness 값을 계속 누적할 경우, 정수 오버플로우가 발생할 위험이 있습니다.  
2. **DamageCalculator.cs \- 계산 오류**  
   * 명시적이지 않은 타입 변환으로 인해 계산 결과가 의도와 다를 수 있습니다.  
   * 부동소수점 연산 시 정밀도 손실로 인한 오차가 발생할 수 있습니다.  
3. **CombatManager.cs \- 싱글톤 문제**  
   * 씬(Scene) 전환 시 현재 인스턴스가 소실되어 게임 로직이 멈출 수 있습니다.

### **🟡 중간 문제점**

* InGameUnit, DamageEffect, CharacterStats 등 여러 클래스에서 **Null 참조 예외**가 발생할 가능성이 있습니다.  
* Unity의 라이프사이클 메서드인 Awake()와 Start()의 실행 순서를 오해하여 사용할 경우, 초기화 문제가 발생할 수 있습니다.  
* 배열 및 리스트 접근 시 경계 값을 검사하지 않아 **IndexOutOfRangeException**이 발생할 수 있습니다.

## **🏗️ 2\. SOLID 원칙 준수도**

### **✅ 잘 준수하고 있는 부분**

* **SRP (단일 책임 원칙):** LogManager, SkillEffect, UnitDataSO 등 각자의 역할에 충실한 클래스들이 존재합니다.  
* **OCP (개방-폐쇄 원칙):** SkillEffect 추상 클래스와 같이, 기능 확장에 열려있는 구조를 잘 활용하고 있습니다.  
* **LSP (리스코프 치환 원칙):** 상속 구조가 적절하게 구현되어 부모-자식 클래스 간의 치환이 원활합니다.

### **❌ 개선이 필요한 부분**

* **DIP (의존관계 역전 원칙) 위반:** LogManager, DamageCalculator 같은 정적 클래스에 직접 의존하여 유연성과 테스트 용이성이 떨어집니다.  
* **SRP (단일 책임 원칙) 위반:** DamageCalculator는 기본 데미지, 크리티컬, 상성 계산 등 너무 많은 책임을 가지고 있습니다.  
* **OCP (개방-폐쇄 원칙) 위반:** 일부 클래스 내에 다른 클래스들이 하드코딩되어 있어 변경에 취약합니다.

## **🔧 3\. 우선순위별 개선 권장사항**

### **🎯 즉시 수정 필요 (Priority 1\)**

1. **TurnManager 성능 개선 (코루틴 활용)**  
   * Update() 대신 코루틴을 사용하여 불필요한 매 프레임 연산을 방지합니다.

// Update() 대신 코루틴 사용  
private IEnumerator TurnProgressCoroutine()  
{  
    while (isCombatActive)  
    {  
        yield return new WaitForSeconds(0.1f); // 매 프레임 대신 0.1초마다 실행  
        ProcessTurnProgress();  
    }  
}

2. **Null 체크 강화**  
   * GetComponent 등 외부 컴포넌트 참조 시 반드시 Null 체크를 추가합니다.

var unit \= GetComponent\<InGameUnit\>();  
if (unit \== null)  
{  
    LogManager.LogError("InGameUnit component is missing\!");  
    return;  
}

3. **CombatManager 싱글톤 수정**  
   * 씬이 전환되어도 인스턴스가 파괴되지 않도록 DontDestroyOnLoad를 적용합니다.

private void Awake()  
{  
    if (Instance \== null)  
    {  
        Instance \= this;  
        DontDestroyOnLoad(gameObject); // 이 코드를 추가하여 씬 전환 시 파괴 방지  
    }  
    else  
    {  
        Destroy(gameObject);  
    }  
}

### **🔄 리팩토링 권장 (Priority 2\)**

1. **DamageCalculator 책임 분리**  
   * 데미지 계산 로직을 인터페이스와 여러 클래스로 분리하여 SRP를 준수하고 확장성을 높입니다.

public interface IDamageCalculator  
{  
    long CalculateFinalDamage(DamageParams parameters);  
}

public class BaseDamageCalculator : IDamageCalculator { /\* ... \*/ }  
public class CriticalCalculator { /\* ... \*/ }  
public class ElementalCalculator { /\* ... \*/ }

2. **의존성 주입(DI) 도입**  
   * 정적 클래스 의존성을 제거하고, 인터페이스를 통해 외부에서 의존성을 주입받는 구조로 변경합니다.

public class InGameUnit : MonoBehaviour  
{  
    \[Inject\] private ILogger logger;  
    \[Inject\] private IDamageCalculator damageCalculator;  
}

### **📈 장기 개선 계획 (Priority 3\)**

1. **이벤트 시스템 도입**  
   * 클래스 간의 직접적인 참조를 줄이고, 이벤트 기반으로 소통하여 결합도를 낮춥니다.

public static class CombatEvents  
{  
    public static event System.Action\<InGameUnit\> OnUnitDamaged;  
    public static event System.Action\<InGameUnit\> OnTurnStart;  
}

2. **Command 패턴으로 액션 시스템 구현:** 유닛의 행동(공격, 스킬 등)을 객체로 캡슐화하여 관리합니다.  
3. **Repository 패턴으로 데이터 접근 통합:** 게임 데이터(유닛 정보, 아이템 등) 접근 로직을 중앙에서 관리합니다.

## **📊 4\. 현재 상태 요약**

| 항목 | 상태 | 점수 ( /10) |
| :---- | :---- | :---- |
| **아키텍처** | 양호 | 7 |
| **기능 안정성** | 보통 | 6 |
| **SOLID 준수** | 부족 | 5 |
| **성능** | 주의 필요 | 5 |
| **확장성** | 양호 | 7 |

## **🎯 5\. 실용적 개선 방향**

1. **단계적 리팩토링:** 한 번에 모든 것을 바꾸려 하지 말고, 제시된 우선순위에 따라 가장 시급한 문제부터 해결합니다.  
2. **Unity 생태계 활용:** 의존성 주입을 위해 Zenject/VContainer 같은 검증된 라이브러리 사용을 고려합니다.  
3. **테스트 가능성 향상:** 인터페이스를 적극적으로 도입하여 각 모듈을 독립적으로 테스트할 수 있는 환경을 구축합니다.  
4. **성능 최적화:** Update() 남용을 줄이고, 반복적으로 생성/파괴되는 객체(예: 이펙트, 투사체)에 오브젝트 풀링을 도입합니다.

**총평:** 전반적으로 잘 구조화된 프로젝트이지만, **성능**과 **안정성** 측면에서 개선이 필요합니다. 제시된 우선순위대로 단계적으로 개선해 나간다면 더욱 견고하고 확장성 있는 게임으로 발전할 수 있을 것입니다.