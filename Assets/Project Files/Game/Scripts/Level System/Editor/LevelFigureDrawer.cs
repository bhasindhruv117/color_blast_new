using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(LevelFigure), false)]
    [StaticUnload]
    public class LevelFigureDrawer : PropertyDrawer
    {
        private static string SKINS_PROPERTY_PATH = "skins";
        private static string SPRITES_PROPERTY_PATH = "sprites";
        private static string SPRITE_PROPERTY_PATH = "sprite";
        private static string PREFS_SKIN = "editor_skin_index";

        private const float BackgroundPadding = 2f;
        private const float PreviewWidth = 70f;
        private const float PreviewHeight = 70f;

        private static List<FieldInfo> levelFigureFields;
        private static Texture2D cellTexture;


        static LevelFigureDrawer()
        {
            Type type = typeof(LevelFigure);
            levelFigureFields = new List<FieldInfo>();

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<LevelEditorSetting>() == null)
                {
                    levelFigureFields.Add(field);
                }
            }

            InitializeCellTexture();
        }

        private static void InitializeCellTexture()
        {
            SerializedObject databaseSerializedObject = new SerializedObject(EditorUtils.GetAsset<LevelSkinDatabase>());
            SerializedProperty skinsProp = databaseSerializedObject.FindProperty(SKINS_PROPERTY_PATH);
            int currentTileSkinIndex = PlayerPrefs.GetInt(PREFS_SKIN, 0);
            SerializedProperty spritesProp = skinsProp.GetArrayElementAtIndex(currentTileSkinIndex).FindPropertyRelative(SPRITES_PROPERTY_PATH);
            cellTexture = SpriteToTexture((Sprite)spritesProp.GetArrayElementAtIndex(0).FindPropertyRelative(SPRITE_PROPERTY_PATH).objectReferenceValue);
        }

        private static Texture2D SpriteToTexture(Sprite sprite)
        {
            Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.rect.x,
                (int)sprite.rect.y,
                (int)sprite.rect.width,
                (int)sprite.rect.height
            );

            tex.SetPixels(pixels);
            tex.Apply();

            return tex;
        }

        private static void UnloadStatic()
        {
            try
            {
                if (Application.isEditor)
                {
                    Texture2D.DestroyImmediate(cellTexture);
                }
                else
                {
                    Texture2D.Destroy(cellTexture);
                }
            }
            catch
            {

            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(cellTexture == null)
            {
                InitializeCellTexture();
            }

            EditorGUI.BeginProperty(position, label, property);

            // Calculate rects
            var leftRect = new Rect(position.x, position.y, position.width - PreviewWidth, position.height);
            var rightRect = new Rect(position.x + position.width - PreviewWidth, position.y, PreviewWidth, PreviewHeight);

            // Draw fields found with reflection on the left
            float yOffset = 0;
            foreach (FieldInfo field in levelFigureFields)
            {
                var fieldProperty = property.FindPropertyRelative(field.Name);
                if (fieldProperty != null)
                {
                    float propertyHeight = EditorGUI.GetPropertyHeight(fieldProperty, true);
                    EditorGUI.PropertyField(new Rect(leftRect.x, leftRect.y + yOffset, leftRect.width, propertyHeight), fieldProperty, true);
                    yOffset += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            // Draw points matrix preview on the right
            var sizeProperty = property.FindPropertyRelative("size");
            var pointsProperty = property.FindPropertyRelative("points");
            var activePointsCountProperty = property.FindPropertyRelative("activePoints");

            if(activePointsCountProperty.intValue == -1)
                activePointsCountProperty.intValue = GetActivePointsCount(pointsProperty);

            int rows = sizeProperty.vector2IntValue.y;
            int cols = sizeProperty.vector2IntValue.x;

            float cellSize = Mathf.Min((PreviewWidth - BackgroundPadding * 2) / cols, (PreviewHeight - BackgroundPadding * 2) / rows);

            // Draw background
            var backgroundRect = new Rect(rightRect.x, rightRect.y, cols * cellSize + BackgroundPadding * 2, rows * cellSize + BackgroundPadding * 2);
            EditorGUI.DrawRect(backgroundRect, Color.gray);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int index = y * cols + x;
                    if (index < pointsProperty.arraySize)
                    {
                        var pointRect = new Rect(rightRect.x + BackgroundPadding + x * cellSize, rightRect.y + BackgroundPadding + (rows - 1 - y) * cellSize, cellSize, cellSize);

                        if (pointsProperty.GetArrayElementAtIndex(index).FindPropertyRelative("isActive").boolValue)
                        {
                            GUI.DrawTexture(pointRect, cellTexture);
                        }
                    }
                }
            }

            // Draw Edit button
            var buttonRect = new Rect(rightRect.x, rightRect.y + PreviewHeight + BackgroundPadding, PreviewWidth, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, "Edit"))
            {
                // Open custom editor window
                LevelFigureEditorWindow.Open(property, cellTexture);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalPropertiesHeight = 0;
            foreach (var field in levelFigureFields)
            {
                var fieldProperty = property.FindPropertyRelative(field.Name);
                if (fieldProperty != null)
                {
                    totalPropertiesHeight += EditorGUI.GetPropertyHeight(fieldProperty, true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            float previewHeightWithButton = PreviewHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return Mathf.Max(totalPropertiesHeight, previewHeightWithButton);
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