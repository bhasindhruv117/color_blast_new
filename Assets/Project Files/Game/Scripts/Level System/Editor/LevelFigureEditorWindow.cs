using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public class LevelFigureEditorWindow : EditorWindow
    {
        private SerializedProperty property;
        private const float BackgroundPadding = 2f;
        private static readonly Color EnabledColor = new Color(0.2f, 0.8f, 0.2f);
        private static readonly Color DisabledColor = new Color(0.8f, 0.2f, 0.2f);

        private Vector2Int size;
        private Texture2D cellTexture;

        public static void Open(SerializedProperty property, Texture2D cellTexture)
        {
            LevelFigureEditorWindow window = GetWindow<LevelFigureEditorWindow>(true, "Edit Level Figure", true);
            window.property = property;

            window.size = property.FindPropertyRelative("size").vector2IntValue;
            window.cellTexture = cellTexture;

            window.ShowUtility();
        }

        private void OnGUI()
        {
            if (property == null)
            {
                Close();
                return;
            }

            property.serializedObject.Update();

            // Display and edit size variable
            Vector2Int newSize = EditorGUILayout.Vector2IntField("Size", size);
            newSize.x = Mathf.Max(1, newSize.x);
            newSize.y = Mathf.Max(1, newSize.y);
            if (newSize != size)
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Change Size");
                size = newSize;
                RecalculatePointsArray(size);
            }

            // Draw big preview with clickable elements
            SerializedProperty pointsProperty = property.FindPropertyRelative("points");
            int rows = size.y;
            int cols = size.x;

            float cellSize = Mathf.Min(((270 - BackgroundPadding * 2) / cols), ((270 - BackgroundPadding * 2) / rows));
            Rect previewRect = GUILayoutUtility.GetRect(cols * cellSize + BackgroundPadding * 2, rows * cellSize + BackgroundPadding * 2);
            EditorGUI.DrawRect(previewRect, Color.gray);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int index = y * cols + x;
                    if (index < pointsProperty.arraySize)
                    {
                        SerializedProperty activeProperty = pointsProperty.GetArrayElementAtIndex(index).FindPropertyRelative("isActive");

                        Rect pointRect = new Rect(previewRect.x + BackgroundPadding + x * cellSize, previewRect.y + BackgroundPadding + (rows - 1 - y) * cellSize, cellSize, cellSize);

                        if (activeProperty.boolValue)
                        {
                            GUI.DrawTexture(pointRect, cellTexture);
                        }

                        if (Event.current.type == EventType.MouseDown && pointRect.Contains(Event.current.mousePosition))
                        {
                            activeProperty.boolValue = !activeProperty.boolValue;

                            property.FindPropertyRelative("activePoints").intValue = GetActivePointsCount(pointsProperty);

                            Event.current.Use();
                        }
                    }
                }
            }

            //draw grid
            Rect gridLineRect = new Rect(previewRect.x + BackgroundPadding, previewRect.y + BackgroundPadding, 2, rows * cellSize);

            for (int x = 0; x <= cols; x++)
            {
                EditorGUI.DrawRect(gridLineRect, Color.white);
                gridLineRect.x += cellSize;
            }

            gridLineRect = new Rect(previewRect.x + BackgroundPadding, previewRect.y + BackgroundPadding, cols * cellSize, 2);

            for (int y = 0; y <= rows; y++)
            {
                EditorGUI.DrawRect(gridLineRect, Color.white);
                gridLineRect.y += cellSize;
            }

            property.serializedObject.ApplyModifiedProperties();

            // Recalculate window size
            float windowHeight = previewRect.height + BackgroundPadding * 2 + 50;
            Vector2 windowSize = new Vector2(270, windowHeight);
            minSize = windowSize;
            maxSize = windowSize;
        }

        private void RecalculatePointsArray(Vector2Int newSize)
        {
            SerializedProperty pointsProperty = property.FindPropertyRelative("points");
            bool[] newPoints = new bool[newSize.x * newSize.y];

            for (int y = 0; y < Mathf.Min(size.y, newSize.y); y++)
            {
                for (int x = 0; x < Mathf.Min(size.x, newSize.x); x++)
                {
                    int oldIndex = y * size.x + x;
                    int newIndex = y * newSize.x + x;
                    if (oldIndex < pointsProperty.arraySize && newIndex < newPoints.Length)
                    {
                        SerializedProperty activeProperty = pointsProperty.GetArrayElementAtIndex(oldIndex).FindPropertyRelative("isActive");

                        newPoints[newIndex] = activeProperty.boolValue;
                    }
                }
            }

            pointsProperty.arraySize = newPoints.Length;
            for (int i = 0; i < newPoints.Length; i++)
            {
                pointsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("isActive").boolValue = newPoints[i];
            }

            property.FindPropertyRelative("size").vector2IntValue = newSize;
            property.FindPropertyRelative("activePoints").intValue = GetActivePointsCount(pointsProperty);
        }

        private int GetActivePointsCount(SerializedProperty arrayProperty)
        {
            int count = 0;

            int arraySize = arrayProperty.arraySize;
            for (int i = 0; i < arraySize; i++)
            {
                if (arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative("isActive").boolValue)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
