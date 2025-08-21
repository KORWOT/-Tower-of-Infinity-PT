using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ElementalMatchupMatrix
{
    [Header("상성 매트릭스 설정")]
    [Tooltip("ElementType enum의 모든 값이 자동으로 포함됩니다")]
    public ElementType[] elementOrder 
    {
        get 
        {
            // ElementType enum의 모든 값을 자동으로 가져옴
            return (ElementType[])System.Enum.GetValues(typeof(ElementType));
        }
    }
    
    [Header("피해 배율 매트릭스 (행: 공격속성, 열: 방어속성)")]
    [SerializeField] private float[] matchupData;
    
    // 매트릭스 크기 (동적)
    public int MatrixSize => elementOrder.Length;
    
    public float GetMultiplier(ElementType attacker, ElementType defender)
    {
        // 매트릭스 크기가 변경되었는지 확인하고 자동 확장
        ValidateAndExpandMatrix();
        
        int attackerIndex = System.Array.IndexOf(elementOrder, attacker);
        int defenderIndex = System.Array.IndexOf(elementOrder, defender);
        
        if (attackerIndex == -1 || defenderIndex == -1) return 1.0f;
        if (matchupData == null || matchupData.Length != MatrixSize * MatrixSize) return 1.0f;
        
        return matchupData[attackerIndex * MatrixSize + defenderIndex];
    }
    
    public void SetMultiplier(ElementType attacker, ElementType defender, float multiplier)
    {
        // 매트릭스 크기가 변경되었는지 확인하고 자동 확장
        ValidateAndExpandMatrix();
        
        int attackerIndex = System.Array.IndexOf(elementOrder, attacker);
        int defenderIndex = System.Array.IndexOf(elementOrder, defender);
        
        if (attackerIndex == -1 || defenderIndex == -1) return;
        
        matchupData[attackerIndex * MatrixSize + defenderIndex] = multiplier;
    }
    
    public void InitializeMatrix()
    {
        matchupData = new float[MatrixSize * MatrixSize];
        
        // 기본값 1.0f로 초기화
        for (int i = 0; i < matchupData.Length; i++)
        {
            matchupData[i] = 1.0f;
        }
    }
    
    /// <summary>
    /// 매트릭스 크기를 확인하고 필요시 자동 확장 (기존 데이터 보존)
    /// </summary>
    public void ValidateAndExpandMatrix()
    {
        int currentSize = MatrixSize;
        int expectedDataLength = currentSize * currentSize;
        
        if (matchupData == null)
        {
            InitializeMatrix();
            return;
        }
        
        // 매트릭스 크기가 변경되었을 때만 확장
        if (matchupData.Length != expectedDataLength)
        {
            ExpandMatrix(currentSize);
        }
    }
    
    /// <summary>
    /// 기존 데이터를 보존하면서 매트릭스 확장
    /// </summary>
    private void ExpandMatrix(int newSize)
    {
        float[] oldData = matchupData;
        int oldSize = (int)Mathf.Sqrt(oldData?.Length ?? 0);
        
        // 새로운 크기로 매트릭스 생성
        matchupData = new float[newSize * newSize];
        
        // 기본값 1.0f로 초기화
        for (int i = 0; i < matchupData.Length; i++)
        {
            matchupData[i] = 1.0f;
        }
        
        // 기존 데이터가 있다면 복사 (겹치는 영역만)
        if (oldData != null && oldSize > 0)
        {
            int copySize = Mathf.Min(oldSize, newSize);
            for (int row = 0; row < copySize; row++)
            {
                for (int col = 0; col < copySize; col++)
                {
                    int oldIndex = row * oldSize + col;
                    int newIndex = row * newSize + col;
                    
                    if (oldIndex < oldData.Length)
                    {
                        matchupData[newIndex] = oldData[oldIndex];
                    }
                }
            }
        }
    }
    
    public void LoadDefaultMatchups()
    {
        ValidateAndExpandMatrix();
        
        // 기본 상성표 매핑 (ElementType과 순서 매칭)
        var defaultMatchups = new Dictionary<(ElementType, ElementType), float>
        {
            // Water 공격
            {(ElementType.Water, ElementType.Fire), 1.5f},
            {(ElementType.Water, ElementType.Dark), 1.15f},
            {(ElementType.Water, ElementType.Light), 1.15f},
            {(ElementType.Water, ElementType.Normal), 1.15f},
            
            // Fire 공격
            {(ElementType.Fire, ElementType.Water), 0.5f},
            {(ElementType.Fire, ElementType.Wind), 1.5f},
            {(ElementType.Fire, ElementType.Dark), 1.15f},
            {(ElementType.Fire, ElementType.Light), 1.15f},
            {(ElementType.Fire, ElementType.Normal), 1.15f},
            
            // Wind 공격
            {(ElementType.Wind, ElementType.Fire), 0.5f},
            {(ElementType.Wind, ElementType.Earth), 1.5f},
            {(ElementType.Wind, ElementType.Dark), 1.15f},
            {(ElementType.Wind, ElementType.Light), 1.15f},
            {(ElementType.Wind, ElementType.Normal), 1.15f},
            
            // Earth 공격
            {(ElementType.Earth, ElementType.Wind), 0.5f},
            {(ElementType.Earth, ElementType.Lightning), 1.5f},
            {(ElementType.Earth, ElementType.Dark), 1.15f},
            {(ElementType.Earth, ElementType.Light), 1.15f},
            {(ElementType.Earth, ElementType.Normal), 1.15f},
            
            // Lightning 공격
            {(ElementType.Lightning, ElementType.Water), 1.5f},
            {(ElementType.Lightning, ElementType.Earth), 0.5f},
            {(ElementType.Lightning, ElementType.Dark), 1.15f},
            {(ElementType.Lightning, ElementType.Light), 1.15f},
            {(ElementType.Lightning, ElementType.Normal), 1.15f},
            
            // Dark 공격
            {(ElementType.Dark, ElementType.Lightning), 0.5f},
            {(ElementType.Dark, ElementType.Dark), 1.15f},
            {(ElementType.Dark, ElementType.Light), 1.5f},
            {(ElementType.Dark, ElementType.Normal), 1.15f},
            
            // Light 공격
            {(ElementType.Light, ElementType.Dark), 1.5f},
            {(ElementType.Light, ElementType.Light), 1.15f},
            {(ElementType.Light, ElementType.Normal), 1.15f},
        };
        
        // 모든 조합을 1.0f로 초기화
        for (int row = 0; row < MatrixSize; row++)
        {
            for (int col = 0; col < MatrixSize; col++)
            {
                matchupData[row * MatrixSize + col] = 1.0f;
            }
        }
        
        // 특별한 상성값 적용
        foreach (var kvp in defaultMatchups)
        {
            SetMultiplier(kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
        }
    }
}

[CreateAssetMenu(fileName = "SO_ElementalMatchupTable", menuName = "SO/ElementalMatchupTable", order = 3)]
public class ElementalMatchupTableSO : ScriptableObject
{
    [Header("속성 상성 매트릭스")]
    public ElementalMatchupMatrix matchupMatrix = new ElementalMatchupMatrix();
    
    // 향후 성능 최적화를 위한 캐시 (현재는 직접 매트릭스 조회 사용)
    // private Dictionary<(ElementType, ElementType), float> _cachedMatchups;
    
    public float GetElementalDamageMultiplier(ElementType attacker, ElementType defender)
    {
        // 매트릭스에서 직접 조회 (O(1) 성능)
        float result = matchupMatrix.GetMultiplier(attacker, defender);
        
        #if UNITY_EDITOR
        // Editor에서 매트릭스가 변경되었을 때 자동으로 SetDirty 처리
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
        
        return result;
    }
    
    [ContextMenu("기본 상성표로 초기화")]
    public void InitializeWithDefaultMatchups()
    {
        matchupMatrix.LoadDefaultMatchups();
        // _cachedMatchups = null; // 캐시 초기화 (현재 사용 안함)
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    [ContextMenu("매트릭스 데이터 검증")]
    public void ValidateMatrixData()
    {
        if (matchupMatrix.elementOrder == null || matchupMatrix.elementOrder.Length == 0)
        {
            Debug.LogWarning("ElementOrder가 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log($"매트릭스 크기: {matchupMatrix.elementOrder.Length}x{matchupMatrix.elementOrder.Length}");
        Debug.Log($"총 데이터 개수: {matchupMatrix.elementOrder.Length * matchupMatrix.elementOrder.Length}");
    }
}
