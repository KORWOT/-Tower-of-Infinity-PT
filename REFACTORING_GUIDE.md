# 🏗️ Tower of Infinity - SOLID 기반 리팩토링 가이드

> **상용 게임 개발을 위한 단계적 아키텍처 개선 문서**

## 📋 목차
1. [SOLID 원칙의 실무적 적용](#solid-원칙의-실무적-적용)
2. [현재 시스템 분석](#현재-시스템-분석)
3. [우선순위별 개선 계획](#우선순위별-개선-계획)
4. [단계별 학습 로드맵](#단계별-학습-로드맵)
5. [실무 가이드라인](#실무-가이드라인)

---

## 🎯 SOLID 원칙의 실무적 적용

### 💡 언제 SOLID를 "깨야" 하는가?

#### ✅ **SOLID를 지켜야 하는 경우**
- **복잡한 비즈니스 로직** (피해 계산, 스킬 시스템)
- **자주 변경되는 기능** (밸런싱, 새 컨텐츠 추가)
- **테스트가 중요한 시스템** (결제, 랭킹, 데이터 저장)
- **팀 개발** (여러 개발자가 동시에 작업)

#### ⚠️ **SOLID를 완화해도 되는 경우**
- **Unity 특화 시스템** (MonoBehaviour, ScriptableObject)
- **성능 크리티컬** (매 프레임 실행되는 로직)
- **단순한 데이터 컨테이너** (설정값, 상수)
- **프로토타입 단계** (빠른 검증이 우선)

#### ❌ **절대 깨면 안 되는 경우**
- **결제/보안 관련 로직**
- **데이터 무결성이 중요한 부분**
- **멀티플레이어 동기화**

---

## 🔍 현재 시스템 분석

### 🚨 **Critical (즉시 개선 필요)**

#### 1. 의존성 역전 원칙(DIP) 위반
```csharp
// 현재 - DamageEffect.cs:21
DamageCalculator.CalculateFinalDamage(..., CombatManager.Instance.elementalMatchupTable);
```
**문제점:**
- Static 클래스와 Singleton에 강하게 결합
- 유닛 테스트 불가능
- 다른 계산 방식으로 교체 불가능

**영향도:** ⭐⭐⭐⭐⭐ (매우 높음)

#### 2. 단일 책임 원칙(SRP) 위반 - DamageCalculator
```csharp
// 현재 - DamageCalculator.cs 전체
public static class DamageCalculator
{
    // 스킬 계수 계산 + 치명타 + 방어력 + 원소상성 + 최종 피해량 산정
    public static long CalculateFinalDamage(...) 
}
```
**문제점:**
- 하나의 메서드가 너무 많은 책임을 가짐 (75줄)
- 새로운 계산 방식 추가시 전체 메서드 수정 필요
- 각 계산 단계별 테스트 어려움

**영향도:** ⭐⭐⭐⭐ (높음)

### ⚠️ **Warning (다음 단계에서 개선)**

#### 3. 개방-폐쇄 원칙(OCP) 위반
```csharp
// 현재 - ElementalMatchupTableSO.cs
[CreateAssetMenu(...)]
public class ElementalMatchupTableSO : ScriptableObject
```
**문제점:**
- 새로운 상성 계산 알고리즘 추가시 기존 코드 수정 필요
- 런타임 상성표 변경 어려움

**영향도:** ⭐⭐⭐ (보통) - 게임 특성상 자주 변경되지 않음

#### 4. 인터페이스 분리 원칙(ISP) 위반
```csharp
// 현재 - CharacterStats.cs
public class CharacterStats
{
    // 공격/방어/특수 스탯이 모두 하나의 클래스에 (95줄)
}
```
**문제점:**
- 공격만 필요한 클래스도 모든 스탯에 접근 가능
- 불필요한 의존성 증가

**영향도:** ⭐⭐ (낮음) - Unity에서 일반적인 패턴

### ✅ **Good (현재 상태 유지)**

#### 5. 리스코프 치환 원칙(LSP)
```csharp
// 현재 - SkillEffect 계층구조
public abstract class SkillEffect
public class DamageEffect : SkillEffect
```
**상태:** 양호함 ✅

---

## 🚀 우선순위별 개선 계획

### 🔴 **Phase 1: Foundation (기반 구축) - 2주**

#### 목표: 테스트 가능한 아키텍처 구축

**1.1 인터페이스 도입 (Day 1-3)**
```csharp
// 새로운 파일들
public interface IDamageable
{
    long CurrentHealth { get; }
    void TakeDamage(long damage);
    bool IsAlive { get; }
}

public interface IDamageCalculator  
{
    long CalculateDamage(IUnit attacker, IDamageable target, ISkill skill, int skillLevel);
}

public interface ILogger
{
    void Log(string message);
    void LogWarning(string message);  
}
```

**1.2 기존 클래스 인터페이스 구현 (Day 4-7)**
```csharp
// InGameUnit 수정
public class InGameUnit : MonoBehaviour, IDamageable, IUnit
{
    // 기존 코드 + 인터페이스 구현
}
```

**1.3 의존성 주입 시스템 도입 (Day 8-14)**
```csharp
// 새로운 파일: ServiceLocator.cs
public static class GameServices
{
    public static IDamageCalculator DamageCalculator { get; set; }
    public static ILogger Logger { get; set; }
    
    public static void Initialize()
    {
        DamageCalculator = new StandardDamageCalculator();
        Logger = new UnityLogger();
    }
}
```

**학습 목표:**
- 인터페이스 설계 원칙 이해
- 의존성 주입의 기본 개념
- Unity에서 DI 적용 방법

### 🟡 **Phase 2: Separation (책임 분리) - 3주**

#### 목표: 복잡한 로직 분해

**2.1 DamageCalculator 분해 (Week 1)**
```csharp
public interface ICriticalCalculator
{
    (bool isCritical, long damage) CalculateCritical(long baseDamage, IUnit attacker);
}

public interface IDefenseCalculator  
{
    long CalculateDefense(long incomingDamage, IUnit defender, IUnit attacker);
}

public interface IElementalCalculator
{
    float GetMultiplier(ElementType attacker, ElementType defender);
}

// 조합 클래스
public class CompositeDamageCalculator : IDamageCalculator
{
    private readonly ICriticalCalculator _critical;
    private readonly IDefenseCalculator _defense;
    private readonly IElementalCalculator _elemental;
    
    public long CalculateDamage(...) 
    {
        // 각 계산기 조합
    }
}
```

**2.2 스킬 시스템 개선 (Week 2-3)**
```csharp
public interface ISkillExecutor
{
    void ExecuteSkill(IUnit caster, IUnit target, ISkill skill, int level);
}

// Command 패턴 적용
public class SkillCommand : ICommand
{
    public void Execute();
    public void Undo(); // 스킬 취소용
}
```

**학습 목표:**
- 단일 책임 원칙의 실제 적용
- Composition over Inheritance
- Command 패턴 활용

### 🟢 **Phase 3: Extension (확장성) - 2주**

#### 목표: 새 컨텐츠 추가 용이성

**3.1 Factory 패턴 도입 (Week 1)**
```csharp
public interface IUnitFactory
{
    IUnit CreateUnit(UnitDataSO data);
}

public interface ISkillFactory
{
    ISkill CreateSkill(SkillDataSO data);
}
```

**3.2 이벤트 시스템 구축 (Week 2)**
```csharp
public interface IGameEventBus
{
    void Subscribe<T>(Action<T> handler) where T : IGameEvent;
    void Publish<T>(T gameEvent) where T : IGameEvent;
}

// 이벤트 예시
public struct UnitDeathEvent : IGameEvent
{
    public IUnit DeadUnit { get; }
    public IUnit Killer { get; }
}
```

**학습 목표:**
- Factory 패턴과 생성 관리
- Observer 패턴과 이벤트 시스템
- 느슨한 결합의 실제 효과

---

## 📚 단계별 학습 로드맵

### 📖 **Level 1: SOLID 기초 이해 (1주)**

**필수 개념:**
- [ ] 인터페이스 vs 추상 클래스
- [ ] 의존성이란 무엇인가?
- [ ] Static의 문제점
- [ ] Unity의 생명주기와 DI

**실습 과제:**
```csharp
// 연습: 간단한 로거 인터페이스 만들기
public interface ISimpleLogger { void Log(string msg); }
public class ConsoleLogger : ISimpleLogger { /* 구현 */ }
public class FileLogger : ISimpleLogger { /* 구현 */ }
```

### 📖 **Level 2: 의존성 관리 (2주)**

**필수 개념:**
- [ ] 의존성 주입 3가지 방법 (Constructor, Property, Method)
- [ ] Service Locator 패턴
- [ ] Unity에서 DI 구현 방법
- [ ] 인터페이스 설계 원칙

**실습 과제:**
- [ ] LogManager를 인터페이스 기반으로 리팩토링
- [ ] InGameUnit에 IDamageable 인터페이스 적용

### 📖 **Level 3: 복잡한 로직 분해 (3주)**

**필수 개념:**
- [ ] 단일 책임 원칙의 실제 적용
- [ ] Composition 패턴
- [ ] Strategy 패턴
- [ ] Command 패턴

**실습 과제:**
- [ ] DamageCalculator 완전 분해
- [ ] 새로운 계산 방식 추가해보기
- [ ] 단위 테스트 작성

### 📖 **Level 4: 고급 패턴 적용 (2주)**

**필수 개념:**
- [ ] Factory 패턴 변형들
- [ ] Observer vs Event Bus
- [ ] State 패턴
- [ ] 성능과 설계의 균형점

---

## 📋 실무 가이드라인

### ✅ **DO (해야 할 것들)**

1. **점진적 리팩토링**
   ```csharp
   // 기존 코드를 완전히 바꾸지 말고 인터페이스 추가부터
   public class InGameUnit : MonoBehaviour, IDamageable  // ✅
   // 기존 public 메서드들은 그대로 유지
   ```

2. **하위 호환성 유지**
   ```csharp
   // 새로운 방식과 기존 방식 병행
   public static class DamageCalculator  // 기존
   {
       // 새로운 방식 추가
       public static IDamageCalculator Instance { get; set; } = new StandardDamageCalculator();
       
       // 기존 메서드는 내부에서 새로운 방식 호출
       public static long CalculateFinalDamage(...) => Instance.CalculateDamage(...);
   }
   ```

3. **Unity 친화적 설계**
   ```csharp
   // MonoBehaviour는 그대로 두고 로직만 분리
   public class InGameUnit : MonoBehaviour
   {
       private IHealthManager _healthManager;  // 로직은 분리
       
       void Awake() => _healthManager = new HealthManager(this);
   }
   ```

### ❌ **DON'T (하지 말아야 할 것들)**

1. **과도한 추상화**
   ```csharp
   // 단순한 데이터 클래스까지 인터페이스 만들지 말 것
   public interface IPosition { float X { get; set; } }  // ❌ 불필요
   ```

2. **성능 무시**
   ```csharp
   // 매 프레임 실행되는 로직에 과도한 인터페이스 사용 금지
   void Update()
   {
       // 직접 계산이 나을 수 있음
   }
   ```

3. **Unity 규칙 위반**
   ```csharp
   // ScriptableObject는 생성자 DI 사용 불가
   [CreateAssetMenu(...)]
   public class UnitDataSO : ScriptableObject  // Unity 방식 유지
   ```

### 🎯 **성공 지표**

**Phase 1 완료 기준:**
- [ ] 모든 주요 클래스가 인터페이스를 통해 상호작용
- [ ] 단위 테스트 작성 가능한 구조
- [ ] 기존 기능 정상 동작

**Phase 2 완료 기준:**
- [ ] DamageCalculator가 5개 이하 클래스로 분해
- [ ] 새로운 스킬 효과 추가가 기존 코드 수정 없이 가능
- [ ] 각 계산 단계별 독립 테스트 가능

**Phase 3 완료 기준:**
- [ ] 새 유닛/스킬 추가가 데이터 생성만으로 가능
- [ ] 이벤트 기반 시스템으로 기능간 결합도 최소화
- [ ] 밸런싱 변경이 설정 파일만으로 가능

---

## 🔗 참고 자료

### 📚 **추천 학습 자료**
1. **Unity 특화 아키텍처**
   - Unity Application Architecture with MVC
   - Zenject (Dependency Injection for Unity)

2. **SOLID 원칙**
   - Clean Architecture (Robert C. Martin)
   - Design Patterns (Gang of Four)

3. **게임 개발 패턴**
   - Game Programming Patterns (Robert Nystrom)
   - Architecture Patterns with Python (적용 원리 학습용)

### 🛠️ **도구 추천**
- **Unity Test Runner** (단위 테스트)
- **Zenject** (DI Container)
- **UniRx** (Reactive Extensions)

---

*문서 작성일: 2025-08-21*  
*다음 업데이트: Phase 1 완료 후*