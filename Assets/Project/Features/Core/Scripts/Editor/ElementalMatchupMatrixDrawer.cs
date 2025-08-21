using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ElementalMatchupMatrix))]
public class ElementalMatchupMatrixDrawer : PropertyDrawer
{
    private const float CELL_WIDTH = 60f;
    private const float CELL_HEIGHT = 20f;
    private const float HEADER_WIDTH = 80f;
    private const float SPACING = 2f;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var matchupDataProp = property.FindPropertyRelative("matchupData");
        
        if (matchupDataProp == null)
        {
            EditorGUI.LabelField(position, "매트릭스 프로퍼티를 찾을 수 없습니다.");
            EditorGUI.EndProperty();
            return;
        }
        
        // ElementType enum의 크기를 동적으로 가져옴
        ElementType[] elementOrder = (ElementType[])System.Enum.GetValues(typeof(ElementType));
        int matrixSize = elementOrder.Length;
        
        // 매트릭스 데이터 배열 크기 검증 및 자동 확장
        int expectedSize = matrixSize * matrixSize;
        if (matchupDataProp.arraySize != expectedSize)
        {
            // 기존 데이터 백업
            float[] oldData = new float[matchupDataProp.arraySize];
            int oldSize = (int)Mathf.Sqrt(matchupDataProp.arraySize);
            
            for (int i = 0; i < matchupDataProp.arraySize; i++)
            {
                oldData[i] = matchupDataProp.GetArrayElementAtIndex(i).floatValue;
            }
            
            // 새로운 크기로 확장
            matchupDataProp.arraySize = expectedSize;
            
            // 모든 값을 1.0f로 초기화
            for (int i = 0; i < matchupDataProp.arraySize; i++)
            {
                matchupDataProp.GetArrayElementAtIndex(i).floatValue = 1.0f;
            }
            
            // 기존 데이터 복원 (겹치는 영역만)
            if (oldData.Length > 0 && oldSize > 0)
            {
                int copySize = Mathf.Min(oldSize, matrixSize);
                for (int row = 0; row < copySize; row++)
                {
                    for (int col = 0; col < copySize; col++)
                    {
                        int oldIndex = row * oldSize + col;
                        int newIndex = row * matrixSize + col;
                        
                        if (oldIndex < oldData.Length && newIndex < matchupDataProp.arraySize)
                        {
                            matchupDataProp.GetArrayElementAtIndex(newIndex).floatValue = oldData[oldIndex];
                        }
                    }
                }
            }
            
            Debug.Log($"ElementType enum 변경 감지: 매트릭스가 {oldSize}x{oldSize}에서 {matrixSize}x{matrixSize}로 확장되었습니다.");
        }
        
        float currentY = position.y;
        
        // 제목
        EditorGUI.LabelField(new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight), 
            "속성 상성 매트릭스 (행: 공격자, 열: 방어자)", EditorStyles.boldLabel);
        currentY += EditorGUIUtility.singleLineHeight + SPACING;
        
        // 초기화 버튼들
        float buttonWidth = 150f;
        if (GUI.Button(new Rect(position.x, currentY, buttonWidth, EditorGUIUtility.singleLineHeight), 
            "기본 상성표로 초기화"))
        {
            InitializeWithDefaultValues(matchupDataProp, matrixSize);
        }
        
        if (GUI.Button(new Rect(position.x + buttonWidth + 10, currentY, buttonWidth, EditorGUIUtility.singleLineHeight), 
            "모든 값 1.0으로 초기화"))
        {
            InitializeWithOnes(matchupDataProp, matrixSize);
        }
        
        currentY += EditorGUIUtility.singleLineHeight + SPACING * 2;
        
        // 상단 헤더 (방어자 속성)
        float startX = position.x + HEADER_WIDTH;
        for (int col = 0; col < matrixSize; col++)
        {
            var elementType = elementOrder[col];
            string elementName = GetElementKoreanName(elementType);
            
            Rect headerRect = new Rect(startX + col * (CELL_WIDTH + SPACING), currentY, CELL_WIDTH, CELL_HEIGHT);
            GUI.Label(headerRect, elementName, GetHeaderStyle());
        }
        currentY += CELL_HEIGHT + SPACING;
        
        // 매트릭스 행들
        for (int row = 0; row < matrixSize; row++)
        {
            // 좌측 헤더 (공격자 속성)
            var attackerType = elementOrder[row];
            string attackerName = GetElementKoreanName(attackerType);
            
            Rect rowHeaderRect = new Rect(position.x, currentY, HEADER_WIDTH, CELL_HEIGHT);
            GUI.Label(rowHeaderRect, attackerName, GetHeaderStyle());
            
            // 매트릭스 셀들
            for (int col = 0; col < matrixSize; col++)
            {
                int dataIndex = row * matrixSize + col;
                var cellProperty = matchupDataProp.GetArrayElementAtIndex(dataIndex);
                
                Rect cellRect = new Rect(startX + col * (CELL_WIDTH + SPACING), currentY, CELL_WIDTH, CELL_HEIGHT);
                
                // 대각선(자기 자신) 셀은 다른 색상으로 표시
                Color originalColor = GUI.backgroundColor;
                if (row == col)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                
                float newValue = EditorGUI.FloatField(cellRect, cellProperty.floatValue);
                cellProperty.floatValue = Mathf.Round(newValue * 100f) / 100f; // 소수점 2자리로 제한
                
                GUI.backgroundColor = originalColor;
            }
            
            currentY += CELL_HEIGHT + SPACING;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // ElementType enum의 크기를 동적으로 가져옴
        ElementType[] elementOrder = (ElementType[])System.Enum.GetValues(typeof(ElementType));
        int matrixSize = elementOrder.Length;
        
        float height = EditorGUIUtility.singleLineHeight; // 제목
        height += EditorGUIUtility.singleLineHeight + SPACING * 2; // 버튼들
        height += CELL_HEIGHT + SPACING; // 상단 헤더
        height += (CELL_HEIGHT + SPACING) * matrixSize; // 매트릭스 행들
        height += SPACING * 2; // 여백
        
        return height;
    }
    
    private void InitializeWithDefaultValues(SerializedProperty matchupDataProp, int matrixSize)
    {
        ElementType[] elementOrder = (ElementType[])System.Enum.GetValues(typeof(ElementType));
        
        // 기본 상성표 매핑 (ElementType과 동적 매칭)
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
        for (int i = 0; i < matchupDataProp.arraySize; i++)
        {
            matchupDataProp.GetArrayElementAtIndex(i).floatValue = 1.0f;
        }
        
        // 특별한 상성값 적용
        foreach (var kvp in defaultMatchups)
        {
            int attackerIndex = System.Array.IndexOf(elementOrder, kvp.Key.Item1);
            int defenderIndex = System.Array.IndexOf(elementOrder, kvp.Key.Item2);
            
            if (attackerIndex >= 0 && defenderIndex >= 0 && attackerIndex < matrixSize && defenderIndex < matrixSize)
            {
                int dataIndex = attackerIndex * matrixSize + defenderIndex;
                if (dataIndex < matchupDataProp.arraySize)
                {
                    matchupDataProp.GetArrayElementAtIndex(dataIndex).floatValue = kvp.Value;
                }
            }
        }
    }
    
    private void InitializeWithOnes(SerializedProperty matchupDataProp, int matrixSize)
    {
        for (int i = 0; i < matchupDataProp.arraySize; i++)
        {
            matchupDataProp.GetArrayElementAtIndex(i).floatValue = 1.0f;
        }
    }
    
    private string GetElementKoreanName(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.None: return "무속성";
            case ElementType.Water: return "물";
            case ElementType.Fire: return "불";
            case ElementType.Wind: return "바람";
            case ElementType.Earth: return "땅";
            case ElementType.Lightning: return "전기";
            case ElementType.Dark: return "암";
            case ElementType.Light: return "빛";
            case ElementType.Normal: return "노말";
            default: return elementType.ToString();
        }
    }
    
    private GUIStyle GetHeaderStyle()
    {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 10;
        return style;
    }
}
