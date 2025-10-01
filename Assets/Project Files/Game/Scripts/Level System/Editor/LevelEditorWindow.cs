#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Watermelon
{
    public class LevelEditorWindow : LevelEditorBase
    {

        //used variables
        private const string GAME_SCENE_PATH = "Assets/Project Files/Game/Scenes/Game.unity";
        private const string LEVELS_PROPERTY_NAME = "levels";
        private const string INFINITE_LEVEL_DATA_PROPERTY_NAME = "infiniteLevelData";
        private const string FIGURES_PROPERTY_NAME = "figures";
        private const string COLLECTABLE_PROPERTY_NAME = "collectables";
        private const string ID_PROPERTY_NAME = "id";
        private const string ICON_PROPERTY_NAME = "icon";
        private const string TEST_LEVEL = "Test Level";
        private SerializedProperty levelsSerializedProperty;
        private SerializedProperty infiniteLevelDataProperty;
        private SerializedProperty figuresSerializedProperty;
        private SerializedProperty collectablesSerializedProperty;
        private LevelRepresentation selectedLevelRepresentation;
        private LevelsHandler levelsHandler;
        private CellTypesHandler gridHandler;

        //sidebar
        private const int SIDEBAR_WIDTH = 320;
        //PlayerPrefs
        private const string PREFS_LEVEL = "editor_level_index";
        private const string PREFS_WIDTH = "editor_sidebar_width";
        private const string PREFS_SKIN = "editor_skin_index";

        //instructions
        private const string RIGHT_CLICK_INSTRUCTION = "Use right click on colored cells to spawn resourse .";
        private const int INFO_HEIGH = 38; //found out using Debug.Log(infoRect) on worst case scenario
        private Rect infoRect;

        //level drawing
        private Rect drawRect;
        private float xSize;
        private float ySize;
        private float elementSize;
        private Event currentEvent;
        private Vector2 elementUnderMouseIndex;
        private Vector2Int elementPosition;
        private int invertedY;
        private float buttonRectX;
        private float buttonRectY;
        private Rect buttonRect;

        //Menu
        private int menuIndex1;
        private int menuIndex2;
        private Rect separatorRect;
        private bool separatorIsDragged;
        private int currentSideBarWidth;
        private TextureHolder textureHolder;
        private int currentTileSkinIndex;
        private TabHandler tabHandler;
        private bool lastActiveLevelOpened;


        private SerializedObject infiniteSerializedObject;
        private IEnumerable<SerializedProperty> infiniteLevelProperties;
        private Rect gridLineRect;
        private Rect resourseRect;
        private List<Vector2Int> positions;
        private bool needToReloadTextures;
        private bool goalFoldoutIsExpanded;
        private GUIStyle boxModifiedStyle;
        private bool dockFoldoutIsExpanded;
        private bool otherFoldoutIsExpanded;
        private GUIStyle iconButtonStyle;

        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            return builder.SetWindowMinSize(new Vector2(700, 500)).Build();
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(AdventureModeLevelData);
        }

        protected override void ReadLevelDatabaseFields()
        {
            LoadLevelDatabaseFromGameData();
            levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
            infiniteLevelDataProperty = levelsDatabaseSerializedObject.FindProperty(INFINITE_LEVEL_DATA_PROPERTY_NAME);
            figuresSerializedProperty = levelsDatabaseSerializedObject.FindProperty(FIGURES_PROPERTY_NAME);
            collectablesSerializedProperty = levelsDatabaseSerializedObject.FindProperty(COLLECTABLE_PROPERTY_NAME);
        }

        private void LoadLevelDatabaseFromGameData()
        {
            levelsDatabase = null;

            GameData gameData = EditorUtils.GetAsset<GameData>();
            levelsDatabase = gameData.LevelDatabase;

            if (levelsDatabase != null)
            {
                levelsDatabaseSerializedObject = new SerializedObject(levelsDatabase);
                unmarkedProperties = LevelEditorUtils.GetUnmarkedProperties(levelsDatabaseSerializedObject);
            }
        }

        protected override void InitializeVariables()
        {
            InitGridHandlerAndTextureHandler();

            currentSideBarWidth = PlayerPrefs.GetInt(PREFS_WIDTH, SIDEBAR_WIDTH);


            tabHandler = new TabHandler();
            tabHandler.AddTab(new TabHandler.Tab("Adventure", DrawAdventureTab));
            tabHandler.AddTab(new TabHandler.Tab("Infinite", DrawInfiniteTab, PrepareToDrawInfiniteTab));
            tabHandler.AddTab(new TabHandler.Tab("Figures", DrawFiguresTab));
            tabHandler.AddTab(new TabHandler.Tab("Collectables", DrawCollectablesTab));
            tabHandler.AddTab(new TabHandler.Tab("General", DrawGeneralTab));
            positions = new List<Vector2Int>();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void InitGridHandlerAndTextureHandler()
        {
            textureHolder = new TextureHolder();
            textureHolder.Initialize();


            gridHandler = new CellTypesHandler();
            gridHandler.AddCellType(new CellTypesHandler.CellType(-1, "Empty", new Color(0f, 0f, 0f)));
            gridHandler.AddCellType(new CellTypesHandler.CellType(0, "Red", new Color(0.79f, 0.15f, 0.15f)));
            gridHandler.AddCellType(new CellTypesHandler.CellType(1, "Orange", new Color(0.92f, 0.44f, 0.09f)));
            gridHandler.AddCellType(new CellTypesHandler.CellType(2, "Yellow", new Color(1f, 0.76f, 0.29f)));
            gridHandler.AddCellType(new CellTypesHandler.CellType(3, "Green", new Color(0.11f, 0.71f, 0.22f)));
            gridHandler.AddCellType(new CellTypesHandler.CellType(4, "Cyan", new Color(0.1f, 0.75f, 0.97f)));
            gridHandler.AddCellType(new CellTypesHandler.CellType(5, "Blue", new Color(0.19f, 0.28f, 0.88f)));
            gridHandler.AddCellType(new CellTypesHandler.CellType(6, "Purple", new Color(0.48f, 0.25f, 0.83f)));
            gridHandler.AddExtraProp(new CellTypesHandler.ExtraProp(-1, string.Empty, false));

            for (int i = 0; i < collectablesSerializedProperty.arraySize; i++)
            {
                gridHandler.AddExtraProp(new CellTypesHandler.ExtraProp(i, collectablesSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ID_PROPERTY_NAME).stringValue));
                textureHolder.AddExtraProp(i, (Sprite)collectablesSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ICON_PROPERTY_NAME).objectReferenceValue);
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                needToReloadTextures = true;
            }
        }

        private void PrepareToDrawInfiniteTab()
        {
            infiniteSerializedObject = new SerializedObject(infiniteLevelDataProperty.objectReferenceValue);
            infiniteLevelProperties = LevelEditorUtils.GetUnmarkedProperties(infiniteSerializedObject);
        }

        private void DrawInfiniteTab()
        {
            EditorGUIUtility.wideMode = true;
            float backupLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent(infiniteSerializedObject.FindProperty(LevelRepresentation.USE_RANDOM_COLORS_FOR_DOCK_FIGURES_PROPERTY_NAME).displayName)).x + 16;
            EditorGUILayout.PropertyField(infiniteSerializedObject.FindProperty(LevelRepresentation.SIZE_PROPERTY_NAME));
            EditorGUILayout.PropertyField(infiniteSerializedObject.FindProperty(LevelRepresentation.USE_RANDOM_COLORS_FOR_DOCK_FIGURES_PROPERTY_NAME));

            if (!infiniteSerializedObject.FindProperty(LevelRepresentation.USE_RANDOM_COLORS_FOR_DOCK_FIGURES_PROPERTY_NAME).boolValue)
            {
                DrawDockFiguresForInfiniteTab(infiniteSerializedObject.FindProperty(LevelRepresentation.DOCK_FIGURES_COLORS_PROPERTY_NAME));
            }
            
            EditorGUILayout.PropertyField(infiniteSerializedObject.FindProperty(LevelRepresentation.DIFFICULTY_PRESET_PROPERTY_NAME));

            foreach (SerializedProperty item in infiniteLevelProperties)
            {
                EditorGUILayout.PropertyField(item);
            }

            EditorGUIUtility.labelWidth = backupLabelWidth;
            infiniteSerializedObject.ApplyModifiedProperties();
        }

        private void DrawDockFiguresForInfiniteTab(SerializedProperty dockFiguresColorsProperty)
        {
            GUIContent dockFiguresColors = new GUIContent(dockFiguresColorsProperty.displayName);
            Rect textFieldRect = GUILayoutUtility.GetRect(dockFiguresColors, GUI.skin.textField);
            textFieldRect = EditorGUI.PrefixLabel(textFieldRect, dockFiguresColors);
            buttonRect = new Rect(textFieldRect);
            buttonRect.width = 18;
            bool currentValue;
            bool newValue;

            for (int i = 1; i < gridHandler.cellTypes.Count; i++) //start from 1 because we skip empty cell
            {
                currentValue = IsColorUsedInDockFiguresColors(dockFiguresColorsProperty, gridHandler.cellTypes[i].value);

                if (currentValue)
                {
                    DrawColorRect(buttonRect, gridHandler.cellTypes[i].color);
                    GUI.Label(buttonRect, "✓", gridHandler.GetLabelStyle(gridHandler.cellTypes[i].color));
                }
                else
                {
                    DrawColorRect(buttonRect, gridHandler.cellTypes[i].color * 0.5f);
                }

                if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                {

                    newValue = !currentValue;

                    if (newValue)
                    {
                        dockFiguresColorsProperty.arraySize++;
                        dockFiguresColorsProperty.GetArrayElementAtIndex(dockFiguresColorsProperty.arraySize - 1).intValue = gridHandler.cellTypes[i].value;
                    }
                    else
                    {
                        for (int j = 0; j < dockFiguresColorsProperty.arraySize; j++)
                        {
                            if (dockFiguresColorsProperty.GetArrayElementAtIndex(j).intValue == gridHandler.cellTypes[i].value)
                            {
                                dockFiguresColorsProperty.DeleteArrayElementAtIndex(j);
                                break;
                            }
                        }
                    }
                }

                buttonRect.x += 24;
            }
        }

        private bool IsColorUsedInDockFiguresColors(SerializedProperty dockFiguresColorsProperty, int value)
        {
            for (int i = 0; i < dockFiguresColorsProperty.arraySize; i++)
            {
                if (dockFiguresColorsProperty.GetArrayElementAtIndex(i).intValue == value)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawFiguresTab()
        {
            EditorGUILayout.PropertyField(figuresSerializedProperty);
        }

        private void DrawCollectablesTab()
        {
            EditorGUILayout.PropertyField(collectablesSerializedProperty);
        }

        private void DrawGeneralTab()
        {
            DisplayProperties();
            EditorGUILayout.LabelField("Editor", EditorCustomStyles.labelBold);
            textureHolder.DrawSkinSelection();
        }

        private void OpenLastActiveLevel()
        {
            if (!lastActiveLevelOpened)
            {
                if ((levelsSerializedProperty.arraySize > 0) && PlayerPrefs.HasKey(PREFS_LEVEL))
                {
                    int levelIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREFS_LEVEL, 0), 0, levelsSerializedProperty.arraySize - 1);
                    levelsHandler.CustomList.SelectedIndex = levelIndex;
                    levelsHandler.OpenLevel(levelIndex);
                }

                lastActiveLevelOpened = true;
            }
        }


        protected override void Styles()
        {
            if (gridHandler != null)
            {
                gridHandler.SetDefaultLabelStyle();
            }

            if (tabHandler != null)
            {
                tabHandler.SetDefaultToolbarStyle();
            }

            if (levelsDatabase != null)
            {
                levelsHandler = new LevelsHandler(levelsDatabaseSerializedObject, levelsSerializedProperty);
            }

            boxModifiedStyle = new GUIStyle(EditorCustomStyles.box);
            boxModifiedStyle.overflow = new RectOffset(0, 0, 0, 0);
            iconButtonStyle = new GUIStyle(GUI.skin.button);
            iconButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            iconButtonStyle.border = new RectOffset(0, 0, 0, 0);
            iconButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            iconButtonStyle.imagePosition = ImagePosition.ImageOnly;
        }

        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
            PlayerPrefs.SetInt(PREFS_LEVEL, index);
            PlayerPrefs.Save();
            AssetDatabase.SaveAssets();
            selectedLevelRepresentation = new LevelRepresentation(levelObject);
        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            return new LevelRepresentation(levelObject).GetLevelLabel(index, stringBuilder);
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
            new LevelRepresentation(levelObject).Clear();
        }
        public override void LogErrorsForGlobalValidation(UnityEngine.Object levelObject, int index)
        {
            LevelRepresentation level = new LevelRepresentation(levelObject);
            level.ValidateLevel();

            if (!level.IsLevelCorrect)
            {
                Debug.Log("Logging validation errors for level #" + (index + 1) + " :");

                foreach (string error in level.errorLabels)
                {
                    Debug.LogWarning(error);
                }
            }
            else
            {
                Debug.Log($"Level # {(index + 1)} passed validation.");
            }
        }

        protected override void DrawContent()
        {
            tabHandler.DisplayTab();
        }

        private void DrawAdventureTab()
        {
            if (needToReloadTextures)
            {
                needToReloadTextures = false;
                InitGridHandlerAndTextureHandler();
                gridHandler.SetDefaultLabelStyle();
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            DisplayListArea();
            HandleChangingSideBar();
            DisplayMainArea();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void HandleChangingSideBar()
        {
            separatorRect = EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
            separatorRect.xMin -= GUI.skin.box.margin.right;
            separatorRect.xMax += GUI.skin.box.margin.left;
            EditorGUIUtility.AddCursorRect(separatorRect, MouseCursor.ResizeHorizontal);


            if (separatorRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    separatorIsDragged = true;
                    levelsHandler.IgnoreDragEvents = true;
                    Event.current.Use();
                }
            }

            if (separatorIsDragged)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    separatorIsDragged = false;
                    levelsHandler.IgnoreDragEvents = false;
                    PlayerPrefs.SetInt(PREFS_WIDTH, currentSideBarWidth);
                    PlayerPrefs.Save();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    currentSideBarWidth = Mathf.RoundToInt(Event.current.delta.x) + currentSideBarWidth;
                    Event.current.Use();
                }
            }
        }

        private void DisplayListArea()
        {
            OpenLastActiveLevel();
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(currentSideBarWidth));
            levelsHandler.DisplayReordableList();
            levelsHandler.DrawRenameLevelsButton();
            levelsHandler.DrawGlobalValidationButton();
            gridHandler.DrawCellButtons();
            EditorGUILayout.EndVertical();
        }

        private void DisplayMainArea()
        {

            if (levelsHandler.SelectedLevelIndex == -1)
            {
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (IsPropertyChanged(levelsHandler.SelectedLevelProperty, new GUIContent("File")))
            {
                levelsHandler.ReopenLevel();
            }

            if (selectedLevelRepresentation.NullLevel)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUIUtility.wideMode = true;

            DisplayLevelProperties();

            if (IsPropertyChanged(selectedLevelRepresentation.sizeProperty))
            {
                selectedLevelRepresentation.HandleSizePropertyChange();
            }

            DrawLevel();

            levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
            selectedLevelRepresentation.ApplyChanges();
            

            EditorGUILayout.BeginHorizontal();
            DrawTipsAndWarnings();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TEST_LEVEL, GUILayout.Width(EditorGUIUtility.labelWidth), GUILayout.Height(SINGLE_LINE_HEIGHT * 2)))
            {
                TestLevel();
            }

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndVertical();
        }

        private void TestLevel()
        {
            GlobalSave tempSave = SaveController.GetGlobalSave();
            SimpleIntSave level = tempSave.GetSaveObject<SimpleIntSave>("levelSave");

            level.Value = levelsHandler.SelectedLevelIndex;
            SaveController.SaveCustom(tempSave);
            OpenScene(GAME_SCENE_PATH);
            ActiveSession.SetSession(new ActiveSession(GameMode.Adventure, levelsHandler.SelectedLevelIndex));
            EditorApplication.isPlaying = true;
        }

        private void DisplayLevelProperties()
        {
            goalFoldoutIsExpanded = DrawStyledFoldout(goalFoldoutIsExpanded, "Goal");

            if (goalFoldoutIsExpanded)
            {
                selectedLevelRepresentation.DiplayLevelRequirements(collectablesSerializedProperty, textureHolder, iconButtonStyle);
                EditorGUILayout.PropertyField(selectedLevelRepresentation.completeRewardProperty);
            }

            EditorGUILayout.EndVertical();

            dockFoldoutIsExpanded = DrawStyledFoldout(dockFoldoutIsExpanded, "Dock");

            if (dockFoldoutIsExpanded)
            {
                selectedLevelRepresentation.HandleDockFiguresColors(gridHandler);
                EditorGUILayout.PropertyField(selectedLevelRepresentation.difficultyPresetProperty);
            }

            selectedLevelRepresentation.UpdateColorsForDockFiguresArray();

            EditorGUILayout.EndVertical();

            otherFoldoutIsExpanded = DrawStyledFoldout(otherFoldoutIsExpanded, "Other");
            ;

            if (otherFoldoutIsExpanded)
            {
                EditorGUILayout.PropertyField(selectedLevelRepresentation.useInRandomizerProperty);
                selectedLevelRepresentation.DisplayProperties();
            }

            EditorGUILayout.EndVertical();
        }

        private bool DrawStyledFoldout(bool IsExpanded, string label)
        {
            Rect blockRect = EditorGUILayout.BeginVertical(boxModifiedStyle, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            GUI.Box(new Rect(blockRect.x, blockRect.y, blockRect.width, 21), GUIContent.none);
            GUILayout.Space(14);

            bool isExpnaded = EditorGUI.Foldout(new Rect(blockRect.x + 8, blockRect.y, blockRect.width - 30, 21), IsExpanded, label, true);

            if (isExpnaded)
            {
                GUILayout.Space(12);
            }


            return isExpnaded;
        }



        private void DrawLevel()
        {
            drawRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            xSize = Mathf.Floor(drawRect.width / selectedLevelRepresentation.sizeProperty.vector2IntValue.x);
            ySize = Mathf.Floor(drawRect.height / selectedLevelRepresentation.sizeProperty.vector2IntValue.y);
            elementSize = Mathf.Min(xSize, ySize);
            currentEvent = Event.current;
            CellTypesHandler.CellType cellType;
            CellTypesHandler.ExtraProp extraProp;
            Vector2Int position;

            if (currentEvent.type == EventType.MouseUp)
            {
                if (positions.Count != 0)
                {
                    positions.Clear();
                }
            }

            if ((currentEvent.type == EventType.MouseDrag) && (currentEvent.button == 0))
            {
                elementUnderMouseIndex = (currentEvent.mousePosition - drawRect.position) / (elementSize);
                elementPosition = new Vector2Int(Mathf.FloorToInt(elementUnderMouseIndex.x), selectedLevelRepresentation.sizeProperty.vector2IntValue.y - 1 - Mathf.FloorToInt(elementUnderMouseIndex.y));

                if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.sizeProperty.vector2IntValue.x) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.sizeProperty.vector2IntValue.y) && (!positions.Contains(elementPosition)))
                {
                    positions.Add(elementPosition);
                    selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                    Repaint();
                }
            }


            //Handle drag and click
            if (currentEvent.type == EventType.MouseDown)
            {
                elementUnderMouseIndex = (currentEvent.mousePosition - drawRect.position) / (elementSize);

                elementPosition = new Vector2Int(Mathf.FloorToInt(elementUnderMouseIndex.x), selectedLevelRepresentation.sizeProperty.vector2IntValue.y - 1 - Mathf.FloorToInt(elementUnderMouseIndex.y));

                if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.sizeProperty.vector2IntValue.x) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.sizeProperty.vector2IntValue.y))
                {
                    if (currentEvent.button == 0)
                    {
                        selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, gridHandler.selectedCellTypeValue);
                        positions.Add(elementPosition);
                        currentEvent.Use();
                    }
                    else if ((currentEvent.button == 1) && (currentEvent.type == EventType.MouseDown))
                    {
                        int index = selectedLevelRepresentation.FindCellIndex(elementPosition.x, elementPosition.y);

                        if (index != -1)
                        {
                            extraProp = gridHandler.GetExtraProp(selectedLevelRepresentation.GetExtraPropsValue(index));

                            GenericMenu menu = new GenericMenu();

                            menuIndex1 = index;
                            menu.AddItem(new GUIContent("Remove collectable"), extraProp.label.Length == 0, () => selectedLevelRepresentation.SetExtraPropsValue(menuIndex1, string.Empty));

                            for (int i = 0; i < selectedLevelRepresentation.requirementCollectablesProperty.arraySize; i++)
                            {
                                string label = selectedLevelRepresentation.requirementCollectablesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(LevelRepresentation.COLLECTABLE_NAME_PROPERTY_NAME).stringValue;
                                menu.AddItem(new GUIContent(label), label.Equals(extraProp.label), () => selectedLevelRepresentation.SetExtraPropsValue(menuIndex1, label));
                            }

                            menu.ShowAsContext();
                        }

                    }
                }
            }

            //draw level
            for (int i = 0; i < selectedLevelRepresentation.itemsProperty.arraySize; i++)
            {
                position = selectedLevelRepresentation.GetPosition(i);
                invertedY = selectedLevelRepresentation.sizeProperty.vector2IntValue.y - 1 - position.y;
                cellType = gridHandler.GetCellType(selectedLevelRepresentation.GetItemsValue(i));
                extraProp = gridHandler.GetExtraProp(selectedLevelRepresentation.GetExtraPropsValue(i));
                buttonRectX = drawRect.position.x + position.x * elementSize;
                buttonRectY = drawRect.position.y + invertedY * elementSize;
                buttonRect = new Rect(buttonRectX, buttonRectY, elementSize, elementSize);

                if (extraProp.value != -1)
                {
                    GUI.DrawTexture(buttonRect, textureHolder.SpecialTileSprite);
                    resourseRect = new Rect(buttonRect);
                    resourseRect.xMin += 4;
                    resourseRect.yMin += 4;
                    resourseRect.xMax -= 4;
                    resourseRect.yMax -= 4;
                    GUI.DrawTexture(resourseRect, textureHolder.GetExtraPropTexture(extraProp.value));
                }
                else
                {
                    GUI.DrawTexture(buttonRect, textureHolder.GetCellTexture((ElementSpriteType)cellType.value));
                }
            }

            //draw grid
            gridLineRect = new Rect(drawRect.x, drawRect.y, 2, selectedLevelRepresentation.sizeProperty.vector2IntValue.y * elementSize);

            for (int x = 0; x <= selectedLevelRepresentation.sizeProperty.vector2IntValue.x; x++)
            {
                DrawColorRect(gridLineRect, Color.gray);
                gridLineRect.x += elementSize;
            }

            gridLineRect = new Rect(drawRect.x, drawRect.y, selectedLevelRepresentation.sizeProperty.vector2IntValue.x * elementSize, 2);

            for (int y = 0; y <= selectedLevelRepresentation.sizeProperty.vector2IntValue.y; y++)
            {
                DrawColorRect(gridLineRect, Color.gray);
                gridLineRect.y += elementSize;
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        private void DrawTipsAndWarnings()
        {
            infoRect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(INFO_HEIGH));
            EditorGUILayout.HelpBox(RIGHT_CLICK_INSTRUCTION, MessageType.Info);
            EditorGUILayout.EndVertical();
            //Debug.Log(infoRect.height);
        }

        public override void OnBeforeAssemblyReload()
        {
            lastActiveLevelOpened = false;
        }


        public override bool WindowClosedInPlaymode()
        {
            return false;
        }

        private void OnDestroy()
        {
            textureHolder.UnloadAllTextures();
            AssetDatabase.SaveAssets();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        protected class TextureHolder
        {
            private string SKINS_PROPERTY_PATH = "skins";
            private string SPRITES_PROPERTY_PATH = "sprites";
            private string SPRITE_PROPERTY_PATH = "sprite";
            private string SPECIAL_SPRITE_PROPERTY_PATH = "specialSprite";
            private string TYPE_PROPERTY_PATH = "type";
            public SerializedObject databaseSerializedObject;
            public int currentTileSkinIndex;
            private int tempTileSkinIndex;
            Dictionary<ElementSpriteType, Texture2D> tileSprites;
            Dictionary<int, Texture2D> extraPropSprites;
            private SerializedProperty skinsProp;
            private Texture2D specialTileSprite;

            public Texture2D SpecialTileSprite => specialTileSprite;

            public void Initialize()
            {
                LevelSkinDatabase levelSkinDatabase = EditorUtils.GetAsset<LevelSkinDatabase>();
                databaseSerializedObject = new SerializedObject(levelSkinDatabase);
                extraPropSprites = new Dictionary<int, Texture2D>();
                LoadTextures();
            }

            public void LoadTextures()
            {
                UnloadCellTextures();

                currentTileSkinIndex = PlayerPrefs.GetInt(PREFS_SKIN, 0);
                tileSprites = new Dictionary<ElementSpriteType, Texture2D>();
                skinsProp = databaseSerializedObject.FindProperty(SKINS_PROPERTY_PATH);
                specialTileSprite = SpriteToTexture((Sprite)skinsProp.GetArrayElementAtIndex(currentTileSkinIndex).FindPropertyRelative(SPECIAL_SPRITE_PROPERTY_PATH).objectReferenceValue);
                SerializedProperty spritesProp = skinsProp.GetArrayElementAtIndex(currentTileSkinIndex).FindPropertyRelative(SPRITES_PROPERTY_PATH);
                ElementSpriteType key;
                Texture2D value;

                for (int i = 0; i < spritesProp.arraySize; i++)
                {
                    key = (ElementSpriteType)spritesProp.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_PATH).intValue;
                    value = SpriteToTexture((Sprite)spritesProp.GetArrayElementAtIndex(i).FindPropertyRelative(SPRITE_PROPERTY_PATH).objectReferenceValue);
                    tileSprites.Add(key, value);
                }
            }

            public void UnloadCellTextures()
            {
                if (tileSprites != null)
                {
                    foreach (Texture2D item in tileSprites.Values)
                    {
                        DestroyImmediate(item);
                    }
                }

                if (specialTileSprite != null)
                {
                    DestroyImmediate(specialTileSprite);
                }
            }

            public Texture2D GetCellTexture(ElementSpriteType type)
            {
                return tileSprites.GetValueOrDefault(type, Texture2D.redTexture);
            }

            public void DrawSkinSelection()
            {
                tempTileSkinIndex = currentTileSkinIndex;
                tempTileSkinIndex = EditorGUILayout.IntSlider("Skin Used For Drawing", tempTileSkinIndex, 0, skinsProp.arraySize - 1);

                if (tempTileSkinIndex != currentTileSkinIndex)
                {
                    PlayerPrefs.SetInt(PREFS_SKIN, tempTileSkinIndex);
                    LoadTextures();
                }

            }


            private Texture2D SpriteToTexture(Sprite sprite)
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

            public void UnloadAllTextures()
            {
                UnloadCellTextures();

                if (extraPropSprites != null)
                {
                    foreach (Texture2D item in extraPropSprites.Values)
                    {
                        DestroyImmediate(item);
                    }
                }
            }

            public void AddExtraProp(int i, Sprite objectReferenceValue)
            {
                extraPropSprites.Add(i, SpriteToTexture(objectReferenceValue));
            }

            public Texture2D GetExtraPropTexture(int index)
            {
                return extraPropSprites.GetValueOrDefault(index, Texture2D.redTexture);
            }
        }


        protected class LevelRepresentation : LevelRepresentationBase
        {
            public const string SIZE_PROPERTY_NAME = "size";
            private const string PRELOAD_LEVEL_DATA_PROPERTY_NAME = "preloadLevelData";
            private const string USE_IN_RANDOMIZER_PROPERTY_NAME = "useInRandomizer";

            private const string POSITION_PROPERTY_NAME = "position";
            private const string SPRITE_TYPE_PROPERTY_NAME = "spriteType";
            private const string HAS_COLLECTABLE_PROPERTY_NAME = "hasCollectable";
            public const string COLLECTABLE_NAME_PROPERTY_NAME = "collectableName";
            private const string REQUIREMENT_TYPE_PROPERTY_NAME = "requirementType";
            private const string REQUIREMENT_SCORE_PROPERTY_NAME = "requirementScore";
            private const string REQUIREMENT_COLLECTABLES_PROPERTY_NAME = "requirementCollectables";
            private const string COLLECTABLE_BLOCKS_PERCENT_PROPERTY_NAME = "collectableBlocksPercent";
            private const string COLLECTABLE_SPAWN_CHANCE_PROPERTY_NAME = "collectableSpawnChance";
            private const string AMOUNT_PROPERTY_NAME = "amount";
            private const string COMPLETE_REWARD_PROPERTY_NAME = "completeReward";

            public const string USE_RANDOM_COLORS_FOR_DOCK_FIGURES_PROPERTY_NAME = "useRandomColorsForDockFigures";
            public const string DOCK_FIGURES_COLORS_PROPERTY_NAME = "dockFiguresColors";
            public const string DIFFICULTY_PRESET_PROPERTY_NAME = "difficultyPreset";



            public SerializedProperty sizeProperty;
            public SerializedProperty itemsProperty;
            public SerializedProperty requirementTypeProperty;
            public SerializedProperty requirementScoreProperty;
            public SerializedProperty requirementCollectablesProperty;
            public SerializedProperty collectableBlocksPercentProperty;
            public SerializedProperty collectableSpawnChanceProperty;
            public SerializedProperty useInRandomizerProperty;
            public SerializedProperty completeRewardProperty;


            public SerializedProperty useRandomColorsForDockFiguresProperty;
            public SerializedProperty dockFiguresColorsProperty;
            public SerializedProperty difficultyPresetProperty;
            private bool autoFillDocsWithColorsByColorsOnTheField;
            public bool checkPassedCached;
            public string checkStatus;

            //temp variables
            Vector2Int tempPosition;
            int tempIndex;


            //resourseField
            private bool collectableFieldInitialized;
            private GUIContent collectableContent;
            private List<string> collectableValues;
            private List<int> usedCollectables;
            private List<int> usedCellTypes;
            private Rect collectableRect;
            private Rect textFieldRect;
            private Rect buttonRect;
            private int collectableIndex;
            private int menuIndex;
            private bool colorsArrayUpToDate;
            private bool colorsTempBool1;
            private bool colorsTempBool2;
            private GUIContent dockFiguresColors;
            private bool dockFiguresColorsFieldInitialized;
            private Color backupColor;

            public LevelRepresentation(UnityEngine.Object levelObject) : base(levelObject)
            {
            }

            protected override void ReadFields()
            {
                sizeProperty = serializedLevelObject.FindProperty(SIZE_PROPERTY_NAME);
                itemsProperty = serializedLevelObject.FindProperty(PRELOAD_LEVEL_DATA_PROPERTY_NAME);
                requirementTypeProperty = serializedLevelObject.FindProperty(REQUIREMENT_TYPE_PROPERTY_NAME);
                requirementScoreProperty = serializedLevelObject.FindProperty(REQUIREMENT_SCORE_PROPERTY_NAME);
                requirementCollectablesProperty = serializedLevelObject.FindProperty(REQUIREMENT_COLLECTABLES_PROPERTY_NAME);
                collectableBlocksPercentProperty = serializedLevelObject.FindProperty(COLLECTABLE_BLOCKS_PERCENT_PROPERTY_NAME);
                collectableSpawnChanceProperty = serializedLevelObject.FindProperty(COLLECTABLE_SPAWN_CHANCE_PROPERTY_NAME);
                useInRandomizerProperty = serializedLevelObject.FindProperty(USE_IN_RANDOMIZER_PROPERTY_NAME);
                completeRewardProperty = serializedLevelObject.FindProperty(COMPLETE_REWARD_PROPERTY_NAME);
                useRandomColorsForDockFiguresProperty = serializedLevelObject.FindProperty(USE_RANDOM_COLORS_FOR_DOCK_FIGURES_PROPERTY_NAME);
                dockFiguresColorsProperty = serializedLevelObject.FindProperty(DOCK_FIGURES_COLORS_PROPERTY_NAME);
                difficultyPresetProperty = serializedLevelObject.FindProperty(DIFFICULTY_PRESET_PROPERTY_NAME);

                autoFillDocsWithColorsByColorsOnTheField = IsAutoFillEnabled();
                dockFiguresColors = new GUIContent(dockFiguresColorsProperty.displayName);
            }

            public override void Clear()
            {
                sizeProperty.vector2IntValue = new Vector2Int(8, 8);
                itemsProperty.arraySize = 0;
                useInRandomizerProperty.boolValue = true;
                ApplyChanges();
            }

            public int FindCellIndex(int index1, int index2)
            {
                tempPosition = new Vector2Int(index1, index2);

                for (int i = 0; i < itemsProperty.arraySize; i++)
                {
                    if (itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(POSITION_PROPERTY_NAME).vector2IntValue.Equals(tempPosition))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public Vector2Int GetPosition(int index)
            {
                return itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(POSITION_PROPERTY_NAME).vector2IntValue;
            }

            public int GetItemsValue(int index)
            {
                if (index == -1)
                {
                    return -1;
                }
                else
                {
                    return itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(SPRITE_TYPE_PROPERTY_NAME).intValue;
                }
            }

            public string GetExtraPropsValue(int index)
            {
                if (itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(HAS_COLLECTABLE_PROPERTY_NAME).boolValue)
                {
                    return itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(COLLECTABLE_NAME_PROPERTY_NAME).stringValue;
                }
                else
                {
                    return string.Empty;
                }
            }

            public void SetExtraPropsValue(int index, string newValue)
            {
                itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(HAS_COLLECTABLE_PROPERTY_NAME).boolValue = (newValue.Length > 0);
                itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(COLLECTABLE_NAME_PROPERTY_NAME).stringValue = newValue;
            }

            public int GetItemsValue(int index1, int index2)
            {
                tempIndex = FindCellIndex(index1, index2);
                return GetItemsValue(tempIndex);
            }

            public void SetItemsValue(int index1, int index2, int newValue)
            {
                tempIndex = FindCellIndex(index1, index2);

                if (newValue == -1)
                {
                    if (tempIndex != -1)
                    {
                        itemsProperty.DeleteArrayElementAtIndex(tempIndex);
                    }
                }
                else if (tempIndex == -1)
                {
                    tempIndex = AddElement(index1, index2);
                    itemsProperty.GetArrayElementAtIndex(tempIndex).FindPropertyRelative(SPRITE_TYPE_PROPERTY_NAME).intValue = newValue;
                }
                else if (itemsProperty.GetArrayElementAtIndex(tempIndex).FindPropertyRelative(SPRITE_TYPE_PROPERTY_NAME).intValue == newValue)
                {
                    itemsProperty.DeleteArrayElementAtIndex(tempIndex);
                }
                else
                {
                    itemsProperty.GetArrayElementAtIndex(tempIndex).FindPropertyRelative(SPRITE_TYPE_PROPERTY_NAME).intValue = newValue;
                }

                colorsArrayUpToDate = false;
            }

            private int AddElement(int index1, int index2)
            {
                //case 1 : add element to start
                if (itemsProperty.arraySize == 0)
                {
                    itemsProperty.arraySize++;
                    itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1).ClearProperty();
                    itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1).FindPropertyRelative(POSITION_PROPERTY_NAME).vector2IntValue = new Vector2Int(index1, index2);
                    return itemsProperty.arraySize - 1;
                }

                //case 2 : add element in the middle sorted
                for (int i = 0; i < itemsProperty.arraySize; i++)
                {
                    tempPosition = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(POSITION_PROPERTY_NAME).vector2IntValue;

                    if ((tempPosition.y < index2) || ((tempPosition.y == index2) && (tempPosition.x < index1)))
                    {
                        itemsProperty.InsertArrayElementAtIndex(i);
                        itemsProperty.GetArrayElementAtIndex(i).ClearProperty();
                        itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(POSITION_PROPERTY_NAME).vector2IntValue = new Vector2Int(index1, index2);
                        return i;
                    }

                }

                //case 3 : add element in the end
                itemsProperty.arraySize++;
                itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1).ClearProperty();
                itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1).FindPropertyRelative(POSITION_PROPERTY_NAME).vector2IntValue = new Vector2Int(index1, index2);
                return itemsProperty.arraySize - 1;
            }

            public void HandleSizePropertyChange()
            {
                if (sizeProperty.vector2IntValue.x < 2)
                {
                    sizeProperty.vector2IntValue = new Vector2Int(2, sizeProperty.vector2IntValue.y);
                }

                if (sizeProperty.vector2IntValue.y < 2)
                {
                    sizeProperty.vector2IntValue = new Vector2Int(sizeProperty.vector2IntValue.x, 2);
                }

                for (int i = itemsProperty.arraySize - 1; i >= 0; i--)
                {
                    tempPosition = GetPosition(i);

                    if ((tempPosition.x >= sizeProperty.vector2IntValue.x) || (tempPosition.y >= sizeProperty.vector2IntValue.y))
                    {
                        itemsProperty.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            public void DiplayLevelRequirements(SerializedProperty collectablesSerializedProperty, TextureHolder textureHolder, GUIStyle iconButtonStyle)
            {
                EditorGUILayout.PropertyField(requirementTypeProperty);

                if (requirementTypeProperty.intValue == 0)
                {
                    EditorGUILayout.PropertyField(requirementScoreProperty);
                    return;
                }


                if (!collectableFieldInitialized)
                {
                    InitializeStuffForCollectableField(collectablesSerializedProperty);
                }

                collectableRect = GUILayoutUtility.GetRect(collectableContent, GUI.skin.textField);
                GUILayout.Space(12);
                collectableRect.y += 6;
                collectableRect = EditorGUI.PrefixLabel(collectableRect, collectableContent);

                textFieldRect = new Rect(collectableRect);
                textFieldRect.width = 32;

                buttonRect = new Rect(textFieldRect);
                buttonRect.width = 16;
                buttonRect.xMin += textFieldRect.width + 2;


                for (int i = 0; i < requirementCollectablesProperty.arraySize; i++)
                {
                    if (i == 0)
                    {
                        textFieldRect = new Rect(collectableRect);
                        textFieldRect.width = 24;

                        buttonRect = new Rect(textFieldRect);
                        buttonRect.xMin += textFieldRect.width + 2;
                        buttonRect.width = 30;
                        buttonRect.yMin -= 6;
                        buttonRect.yMax += 6;
                    }
                    else
                    {
                        textFieldRect.x += 60;
                        buttonRect.x += 60;
                    }


                    requirementCollectablesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(AMOUNT_PROPERTY_NAME).intValue = EditorGUI.IntField(textFieldRect, requirementCollectablesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(AMOUNT_PROPERTY_NAME).intValue);
                    collectableIndex = collectableValues.IndexOf(requirementCollectablesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(COLLECTABLE_NAME_PROPERTY_NAME).stringValue);

                    if (collectableIndex == -1)
                    {
                        Debug.LogError("Unknown collectable with id:" + requirementCollectablesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(COLLECTABLE_NAME_PROPERTY_NAME).stringValue);
                    }
                    else
                    {

                        if (GUI.Button(buttonRect, textureHolder.GetExtraPropTexture(collectableIndex), iconButtonStyle))
                        {
                            menuIndex = i;
                            int collectableIndexBackup = collectableIndex;
                            GenericMenu genericMenu = new GenericMenu();
                            genericMenu.AddItem(new GUIContent("Remove collectable"), false, () => requirementCollectablesProperty.DeleteArrayElementAtIndex(menuIndex));
                            genericMenu.AddSeparator("");
                            UpdateUsedCollectablesList();

                            for (int j = 0; j < collectableValues.Count; j++)
                            {
                                if (usedCollectables.Contains(j) && (j != collectableIndexBackup))
                                {
                                    genericMenu.AddDisabledItem(new GUIContent(collectableValues[j]));
                                }
                                else
                                {
                                    genericMenu.AddItem(new GUIContent(collectableValues[j]), j == collectableIndexBackup, MenuFunction, j);
                                }
                            }

                            genericMenu.ShowAsContext();
                        }
                    }

                }

                if (requirementCollectablesProperty.arraySize == 0)
                {
                    buttonRect = new Rect(textFieldRect);
                    buttonRect.width = 24;
                }
                else
                {
                    buttonRect.x += buttonRect.width + 10;
                }

                EditorGUI.BeginDisabledGroup(requirementCollectablesProperty.arraySize == collectablesSerializedProperty.arraySize);

                if (GUI.Button(buttonRect, "+"))
                {

                    UpdateUsedCollectablesList();


                    for (int i = 0; i < collectableValues.Count; i++)
                    {
                        if (usedCollectables.Contains(i))
                        {
                            continue;
                        }

                        requirementCollectablesProperty.arraySize++;
                        requirementCollectablesProperty.GetArrayElementAtIndex(requirementCollectablesProperty.arraySize - 1).FindPropertyRelative(AMOUNT_PROPERTY_NAME).intValue = 1;
                        requirementCollectablesProperty.GetArrayElementAtIndex(requirementCollectablesProperty.arraySize - 1).FindPropertyRelative(COLLECTABLE_NAME_PROPERTY_NAME).stringValue = collectableValues[i];
                        break;
                    }

                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(collectableBlocksPercentProperty);
                EditorGUILayout.PropertyField(collectableSpawnChanceProperty);
            }

            private void UpdateUsedCollectablesList()
            {
                usedCollectables.Clear();

                for (int i = 0; i < requirementCollectablesProperty.arraySize; i++)
                {
                    collectableIndex = collectableValues.IndexOf(requirementCollectablesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(COLLECTABLE_NAME_PROPERTY_NAME).stringValue);
                    usedCollectables.Add(collectableIndex);
                }
            }

            private void InitializeStuffForCollectableField(SerializedProperty collectablesSerializedProperty)
            {
                collectableContent = new GUIContent(requirementCollectablesProperty.displayName);
                collectableValues = new List<string>();
                usedCollectables = new List<int>();
                collectableFieldInitialized = true;

                for (int i = 0; i < collectablesSerializedProperty.arraySize; i++)
                {
                    collectableValues.Add(collectablesSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ID_PROPERTY_NAME).stringValue);
                }
            }

            public void HandleDockFiguresColors(CellTypesHandler gridHandler)
            {
                colorsTempBool1 = EditorGUILayout.ToggleLeft(useRandomColorsForDockFiguresProperty.displayName, useRandomColorsForDockFiguresProperty.boolValue);
                colorsTempBool2 = EditorGUILayout.ToggleLeft("Auto Fill Dock Colors By Colors On The Field", autoFillDocsWithColorsByColorsOnTheField);

                if (colorsTempBool1 != useRandomColorsForDockFiguresProperty.boolValue)
                {
                    useRandomColorsForDockFiguresProperty.boolValue = colorsTempBool1;

                    if (colorsTempBool1)
                    {
                        autoFillDocsWithColorsByColorsOnTheField = false;
                    }
                }
                else if (colorsTempBool2 != autoFillDocsWithColorsByColorsOnTheField)
                {
                    autoFillDocsWithColorsByColorsOnTheField = colorsTempBool2;

                    if (colorsTempBool2)
                    {
                        colorsArrayUpToDate = false;
                        useRandomColorsForDockFiguresProperty.boolValue = false;
                    }
                }

                UpdateColorsForDockFiguresArray();

                if (useRandomColorsForDockFiguresProperty.boolValue)
                {
                    return;
                }

                textFieldRect = GUILayoutUtility.GetRect(dockFiguresColors, GUI.skin.textField);
                textFieldRect = EditorGUI.PrefixLabel(textFieldRect, dockFiguresColors);
                buttonRect = new Rect(textFieldRect);
                buttonRect.width = 18;
                bool currentValue;
                bool newValue;

                for (int i = 1; i < gridHandler.cellTypes.Count; i++) //start from 1 because we skip empty cell
                {
                    currentValue = IsColorUsedInDockFiguresColors(gridHandler.cellTypes[i].value);

                    if (currentValue)
                    {
                        DrawColorRect(buttonRect, gridHandler.cellTypes[i].color);
                        GUI.Label(buttonRect, "✓", gridHandler.GetLabelStyle(gridHandler.cellTypes[i].color));
                    }
                    else
                    {
                        DrawColorRect(buttonRect, gridHandler.cellTypes[i].color * 0.5f);
                    }

                    if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                    {
                        if (autoFillDocsWithColorsByColorsOnTheField)
                        {
                            autoFillDocsWithColorsByColorsOnTheField = false;
                        }

                        newValue = !currentValue;

                        if (newValue)
                        {
                            dockFiguresColorsProperty.arraySize++;
                            dockFiguresColorsProperty.GetArrayElementAtIndex(dockFiguresColorsProperty.arraySize - 1).intValue = gridHandler.cellTypes[i].value;
                        }
                        else
                        {
                            for (int j = 0; j < dockFiguresColorsProperty.arraySize; j++)
                            {
                                if (dockFiguresColorsProperty.GetArrayElementAtIndex(j).intValue == gridHandler.cellTypes[i].value)
                                {
                                    dockFiguresColorsProperty.DeleteArrayElementAtIndex(j);
                                    break;
                                }
                            }
                        }
                    }

                    buttonRect.x += 24;
                }
            }

            private bool IsColorUsedInDockFiguresColors(int value)
            {
                for (int i = 0; i < dockFiguresColorsProperty.arraySize; i++)
                {
                    if (dockFiguresColorsProperty.GetArrayElementAtIndex(i).intValue == value)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void UpdateColorsForDockFiguresArray()
            {
                if (autoFillDocsWithColorsByColorsOnTheField)
                {
                    if (colorsArrayUpToDate)
                    {
                        return;
                    }

                    if (usedCellTypes == null)
                    {
                        usedCellTypes = new List<int>();
                    }
                    else
                    {
                        usedCellTypes.Clear();
                    }

                    for (int i = 0; i < itemsProperty.arraySize; i++)
                    {
                        tempIndex = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(SPRITE_TYPE_PROPERTY_NAME).intValue;

                        if (!usedCellTypes.Contains(tempIndex))
                        {
                            usedCellTypes.Add(tempIndex);
                        }
                    }

                    usedCellTypes.Sort();
                    dockFiguresColorsProperty.arraySize = usedCellTypes.Count;

                    for (int i = 0; i < usedCellTypes.Count; i++)
                    {
                        dockFiguresColorsProperty.GetArrayElementAtIndex(i).intValue = usedCellTypes[i];
                    }

                    colorsArrayUpToDate = true;
                }
            }

            public bool IsAutoFillEnabled()
            {
                if (useRandomColorsForDockFiguresProperty.boolValue)
                {
                    return false;
                }

                usedCellTypes = new List<int>();

                for (int i = 0; i < itemsProperty.arraySize; i++)
                {
                    tempIndex = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(SPRITE_TYPE_PROPERTY_NAME).intValue;

                    if (!usedCellTypes.Contains(tempIndex))
                    {
                        usedCellTypes.Add(tempIndex);
                    }
                }

                usedCellTypes.Sort();

                if (dockFiguresColorsProperty.arraySize != usedCellTypes.Count)
                {
                    return false;
                }

                for (int i = 0; i < dockFiguresColorsProperty.arraySize; i++)
                {
                    usedCellTypes.Remove(dockFiguresColorsProperty.GetArrayElementAtIndex(i).intValue); //remove existing elements from temp array
                }

                return (usedCellTypes.Count == 0); //if there still elements then arrays isn`t equal

            }

            private void MenuFunction(object userData)
            {
                int index = (int)userData;
                requirementCollectablesProperty.GetArrayElementAtIndex(menuIndex).FindPropertyRelative(COLLECTABLE_NAME_PROPERTY_NAME).stringValue = collectableValues[index];
            }





            public override string GetLevelLabel(int index, StringBuilder stringBuilder)
            {
                stringBuilder.Clear();
                stringBuilder.Append(NUMBER);
                stringBuilder.Append(index + 1);
                stringBuilder.Append(SEPARATOR);

                if (NullLevel)
                {
                    stringBuilder.Append(NULL_FILE);
                }
                else
                {
                    if (requirementTypeProperty.intValue == 0)
                    {
                        stringBuilder.Append("Score: ");
                        stringBuilder.Append(requirementScoreProperty.intValue);
                    }
                    else
                    {
                        stringBuilder.Append("Collect: ");

                        for (int i = 0; i < requirementCollectablesProperty.arraySize; i++)
                        {
                            stringBuilder.Append(requirementCollectablesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(AMOUNT_PROPERTY_NAME).intValue);
                            stringBuilder.Append(' ');
                        }

                    }
                }

                return stringBuilder.ToString();
            }
        }
    }
}

// -----------------
// 2d grid level editor V1.2.1
// -----------------

// Changelog
// v 1.2.1
// • Some small fixes after update
// v 1.2
// • Reordered some methods
// v 1.1
// • Added global validation
// • Added validation example
// • Fixed mouse click bug
// v 1 basic version works