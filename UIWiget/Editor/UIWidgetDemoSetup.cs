// Dateiname: UIWidgetDemoSetup.cs (Vollständig & Final Korrigiert)
// Muss im "Editor"-Ordner platziert werden.
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using YourGame.UI;
using YourGame.UI.Widgets;
using YourGame.Quests;
using TMPro;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class UIWidgetDemoSetup : EditorWindow
{
    [MenuItem("UI-System Demo/Demo-Fenster öffnen")]
    public static void ShowWindow()
    {
        GetWindow<UIWidgetDemoSetup>("UIWidget Demo Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("UIWidget Demo Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Klicke im Play-Modus auf die Buttons, um die UI-Elemente zu demonstrieren.", MessageType.Info);
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Starte den Play-Modus, um die Demo zu verwenden.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("1. Basis-UI & Manager erstellen")) CreateBaseUI();

        EditorGUILayout.Space();
        GUILayout.Label("Komponenten-Demonstrationen", EditorStyles.boldLabel);
        if (GUILayout.Button("UIWidget (Show/Hide/Tween)")) DemonstrateUIWidget();
        if (GUILayout.Button("UIMenu & UIScrollList (Pooling)")) DemonstrateMenus();
        
        EditorGUILayout.Space();
        GUILayout.Label("Weitere Komponenten", EditorStyles.boldLabel);
        if (GUILayout.Button("Modal-Dialog anzeigen")) DemonstrateModalDialog();
        if (GUILayout.Button("Slider & Progress Bars (Interaktiv)")) DemonstrateSlidersAndProgress();
        if (GUILayout.Button("Toggle Button")) DemonstrateToggleButton();
        
        EditorGUILayout.Space();
        GUILayout.Label("Fortgeschrittene RPG-Module", EditorStyles.boldLabel);
        if (GUILayout.Button("Crafting-Panel demonstrieren")) DemonstrateCraftingPanel();
        if (GUILayout.Button("Ausrüstungs-Vergleich demonstrieren")) DemonstrateComparisonTooltip();
        if (GUILayout.Button("Skill-Baum demonstrieren")) DemonstrateSkillTree();
        if (GUILayout.Button("Kantine / Buff-Food demonstrieren")) DemonstrateKantine(); // NEUER BUTTON

        EditorGUILayout.Space();
        GUILayout.Label("MMORPG-System-Demonstrationen", EditorStyles.boldLabel);
        if (GUILayout.Button("Quest-Tracker demonstrieren")) DemonstrateQuestTracker();
        if (GUILayout.Button("Minimap demonstrieren")) DemonstrateMinimap();
        if (GUILayout.Button("Drag & Drop Inventar")) DemonstrateInventory();
        if (GUILayout.Button("Aktionsleiste (mit Tasten 1-3)")) DemonstrateActionBar();
    }

    #region Demo Methods

    // NEUE DEMO-METHODE
    private void DemonstrateKantine()
    {
        // 1. Demo-Daten für Zutaten erstellen (mit dem universellen Buff-System)
        var heartySteak = ScriptableObject.CreateInstance<IngredientData>();
        heartySteak.itemName = "Herzhaftes Steak";
        heartySteak.category = IngredientData.IngredientCategory.Fleisch;
        heartySteak.buffs = new List<Buff> { new Buff { type = BuffType.Angriff, value = 10 }, new Buff { type = BuffType.Gesundheit, value = 5 } };

        var giantFish = ScriptableObject.CreateInstance<IngredientData>();
        giantFish.itemName = "Riesenfisch";
        giantFish.category = IngredientData.IngredientCategory.Fisch;
        giantFish.buffs = new List<Buff> { new Buff { type = BuffType.Ausdauer, value = 25 } };

        var sunHerb = ScriptableObject.CreateInstance<IngredientData>();
        sunHerb.itemName = "Sonnenkraut";
        sunHerb.category = IngredientData.IngredientCategory.Gemüse;
        sunHerb.buffs = new List<Buff> { new Buff { type = BuffType.Verteidigung, value = 15 } };

        var allIngredients = new List<IngredientData> { heartySteak, giantFish, sunHerb };

        // 2. UI-Elemente für die Kantine erstellen
        var kantineGO = CreatePanel("KantinePanel", new Color(0.3f, 0.2f, 0.2f), Vector2.zero, new Vector2(550, 350));
        var kantine = kantineGO.AddComponent<UIKantine>();

        // Zutatenauswahl-Liste
        var selectionList = CreateScrollList("IngredientSelection", new Vector2(-150, 0));
        selectionList.transform.SetParent(kantineGO.transform, false);

        // Slots zum Kochen
        var slotsPanel = CreatePanel("KochSlots", Color.clear, new Vector2(125, 100), new Vector2(250, 70));
        slotsPanel.transform.SetParent(kantineGO.transform, false);
        slotsPanel.AddComponent<HorizontalLayoutGroup>().spacing = 10;
        
        var slots = new List<UIInventorySlot>();
        for (int i = 0; i < 3; i++)
        {
            var slotGO = CreatePanel($"KochSlot_{i}", Color.black, Vector2.zero, new Vector2(60, 60));
            slotGO.transform.SetParent(slotsPanel.transform, false);
            slots.Add(slotGO.AddComponent<UIInventorySlot>());
        }

        // Ergebnis-Anzeige
        var resultText = CreateText("ResultText", "...", kantineGO.transform, 16);
        resultText.rectTransform.anchoredPosition = new Vector2(125, -20);
        resultText.rectTransform.sizeDelta = new Vector2(240, 100);
        resultText.alignment = TextAlignmentOptions.TopLeft;

        var cookButton = CreateButton("CookButton", "Kochen", new Vector2(125, -125)).GetComponent<UIButton>();

        // 3. Kantinen-Panel initialisieren und mit Daten füllen
        kantine.Initialize(slots, resultText, cookButton, selectionList);
        kantine.PopulateIngredients(allIngredients);
    }
    
    private void DemonstrateCraftingPanel()
    {
        var ironOre = ScriptableObject.CreateInstance<ItemData>();
        ironOre.itemName = "Eisenerz";
        var wood = ScriptableObject.CreateInstance<ItemData>();
        wood.itemName = "Holz";
        var ironSword = ScriptableObject.CreateInstance<EquipmentData>();
        ironSword.itemName = "Eisenschwert";

        var swordRecipe = ScriptableObject.CreateInstance<CraftingRecipe>();
        swordRecipe.resultItem = ironSword;
        swordRecipe.requiredMaterials = new List<RequiredMaterial>
        {
            new RequiredMaterial { item = ironOre, quantity = 5 },
            new RequiredMaterial { item = wood, quantity = 2 }
        };

        var craftingGO = CreatePanel("CraftingPanel", new Color(0.2f, 0.2f, 0.3f), Vector2.zero, new Vector2(500, 300));
        if (craftingGO == null) return;
        var craftingPanel = craftingGO.AddComponent<UICraftingPanel>();

        var recipeList = CreateScrollList("RecipeList", new Vector2(-125, 0));
        recipeList.transform.SetParent(craftingGO.transform, false);

        var resultIcon = CreatePanel("ResultIcon", Color.black, new Vector2(125, 100), new Vector2(80, 80)).GetComponent<Image>();
        var resultName = CreateText("ResultName", "Item Name", craftingGO.transform, 20);
        resultName.rectTransform.anchoredPosition = new Vector2(125, 40);
        
        var materialsContainer = CreatePanel("MaterialsContainer", Color.clear, new Vector2(125, -30), new Vector2(220, 150)).GetComponent<RectTransform>();
        materialsContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        var craftButton = CreateButton("CraftButton", "Herstellen", new Vector2(125, -120)).GetComponent<UIButton>();
        
        var matTemplate = CreatePanel("MaterialTemplate", Color.clear, Vector2.zero, new Vector2(220, 30));
        var hlg = matTemplate.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childControlWidth = false;
        var iconPanel = CreatePanel("Icon", Color.white, Vector2.zero, new Vector2(25,25));
        iconPanel.transform.SetParent(matTemplate.transform, false);
        var nameText = CreateText("Name", "Mat Name", matTemplate.transform, 14);
        nameText.alignment = TextAlignmentOptions.Left;
        var amountText = CreateText("Amount", "0/0", matTemplate.transform, 14);
        amountText.alignment = TextAlignmentOptions.Right;
        matTemplate.transform.SetParent(materialsContainer.transform, false);
        matTemplate.SetActive(false);
        
        craftingPanel.Initialize(recipeList, resultIcon, resultName, materialsContainer, matTemplate, craftButton);
        craftingPanel.PopulateRecipes(new List<CraftingRecipe> { swordRecipe });
    }

    private void DemonstrateComparisonTooltip()
    {
        var currentSword = ScriptableObject.CreateInstance<EquipmentData>();
        currentSword.itemName = "Altes Schwert";
        currentSword.attack = 10;
        currentSword.defense = 5;

        var newSword = ScriptableObject.CreateInstance<EquipmentData>();
        newSword.itemName = "Neues Eisenschwert";
        newSword.attack = 15;
        newSword.defense = 3;
        
        var tooltipGO = CreatePanel("ComparisonTooltip", new Color(0.1f, 0.1f, 0.1f, 0.9f), new Vector2(Screen.width / 2f, Screen.height / 2f), new Vector2(350, 150));
        if (tooltipGO == null) return;
        var tooltip = tooltipGO.AddComponent<UIComparisonTooltip>();
        
        var currentPanel = CreatePanel("CurrentItemPanel", Color.clear, new Vector2(-85, 0), new Vector2(160, 140));
        currentPanel.transform.SetParent(tooltipGO.transform, false);
        var newPanel = CreatePanel("NewItemPanel", Color.clear, new Vector2(85, 0), new Vector2(160, 140));
        newPanel.transform.SetParent(tooltipGO.transform, false);

        var currentName = CreateText("CurrentName", "Current", currentPanel.transform, 16);
        var currentIcon = CreatePanel("CurrentIcon", Color.gray, new Vector2(0, 20), new Vector2(50,50)).GetComponent<Image>();
        var currentAtk = CreateText("CurrentAtk", "Atk: 0", currentPanel.transform, 14);
        var currentDef = CreateText("CurrentDef", "Def: 0", currentPanel.transform, 14);
        currentName.rectTransform.anchoredPosition = new Vector2(0, 55);
        currentAtk.rectTransform.anchoredPosition = new Vector2(0, -25);
        currentDef.rectTransform.anchoredPosition = new Vector2(0, -45);
        
        var newName = CreateText("NewName", "New", newPanel.transform, 16);
        var newIcon = CreatePanel("NewIcon", Color.cyan, new Vector2(0, 20), new Vector2(50,50)).GetComponent<Image>();
        var newAtk = CreateText("NewAtk", "Atk: 0", newPanel.transform, 14);
        var newDef = CreateText("NewDef", "Def: 0", newPanel.transform, 14);
        newName.rectTransform.anchoredPosition = new Vector2(0, 55);
        newAtk.rectTransform.anchoredPosition = new Vector2(0, -25);
        newDef.rectTransform.anchoredPosition = new Vector2(0, -45);

        SetPrivateField(tooltip, "_currentItemPanel", currentPanel);
        SetPrivateField(tooltip, "_currentItemName", currentName);
        SetPrivateField(tooltip, "_currentItemIcon", currentIcon);
        SetPrivateField(tooltip, "_currentItemAttack", currentAtk);
        SetPrivateField(tooltip, "_currentItemDefense", currentDef);
        
        SetPrivateField(tooltip, "_newItemPanel", newPanel);
        SetPrivateField(tooltip, "_newItemName", newName);
        SetPrivateField(tooltip, "_newItemIcon", newIcon);
        SetPrivateField(tooltip, "_newItemAttack", newAtk);
        SetPrivateField(tooltip, "_newItemDefense", newDef);
        tooltip.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

        tooltip.ShowComparison(newSword, currentSword);
    }

    private void DemonstrateSkillTree()
    {
        var skillTreeGO = CreatePanel("SkillTree", new Color(0.1f, 0.2f, 0.1f), Vector2.zero, new Vector2(600, 400));
        if (skillTreeGO == null) return;
        var skillTree = skillTreeGO.AddComponent<UISkillTree>();
        
        var pointsText = CreateText("SkillPointsText", "Punkte: 10", skillTreeGO.transform, 18);
        pointsText.rectTransform.anchoredPosition = new Vector2(0, 170);
        
        var linePrefabGO = CreatePanel("LinePrefab", Color.yellow, Vector2.zero, new Vector2(1,1));
        linePrefabGO.AddComponent<UILineRenderer>();
        linePrefabGO.transform.SetParent(skillTreeGO.transform, false);
        linePrefabGO.SetActive(false);
        
        var node1 = CreateSkillNode("Stärke I", skillTreeGO.transform, new Vector2(0, 100));
        var node2 = CreateSkillNode("Stärke II", skillTreeGO.transform, new Vector2(0, 20), node1);
        var node3 = CreateSkillNode("Wirbelwind", skillTreeGO.transform, new Vector2(-100, -60), node2);
        var node4 = CreateSkillNode("Standfest", skillTreeGO.transform, new Vector2(100, -60), node2);
        var nodes = new List<UISkillNode> { node1, node2, node3, node4 };
        
        skillTree.Initialize(pointsText, nodes, linePrefabGO);
        skillTree.gameObject.SetActive(true);
    }
    
    // (Der Rest des Skripts bleibt unverändert)

    #endregion

    #region Helper Methods & Unchanged Demos
    
    private UISkillNode CreateSkillNode(string name, Transform parent, Vector2 position, UISkillNode dependency = null)
    {
        var nodeWidget = CreateButton(name, "", position, new Vector2(60, 60));
        if (nodeWidget == null) return null;
        nodeWidget.transform.SetParent(parent, false);
        var skillNode = nodeWidget.gameObject.AddComponent<UISkillNode>();
        
        skillNode.skillName = name;
        if (dependency != null)
        {
            skillNode.dependencies.Add(dependency);
        }
        return skillNode;
    }
    
    private void DemonstrateSlidersAndProgress()
    {
        var progressBarGO = CreatePanel("DemoProgressBar", Color.black, new Vector2(-200, 100), new Vector2(150, 20));
        var fillImgGo = CreatePanel("Fill", Color.green, Vector2.zero, new Vector2(150, 20));
        var fillImg = fillImgGo.GetComponent<Image>();
        fillImg.transform.SetParent(progressBarGO.transform, false);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.raycastTarget = false;
        
        var progressBar = progressBarGO.AddComponent<UIProgressBar>();
        progressBar.FillImage = fillImg;
        progressBar.UpdateFill();

        var sliderGO = CreatePanel("DemoSlider", new Color(0.3f, 0.3f, 0.3f), new Vector2(200, 100), new Vector2(150, 20));
        var fillRectGO = CreatePanel("SliderFill", Color.blue, Vector2.zero, Vector2.zero);
        var handleRectGO = CreatePanel("SliderHandle", Color.white, Vector2.zero, new Vector2(20, 20));

        fillRectGO.GetComponent<Image>().raycastTarget = false;
        handleRectGO.GetComponent<Image>().raycastTarget = false;

        var fillRect = fillRectGO.GetComponent<RectTransform>();
        var handleRect = handleRectGO.GetComponent<RectTransform>();
        fillRect.transform.SetParent(sliderGO.transform, false);
        handleRect.transform.SetParent(sliderGO.transform, false);
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);

        var slider = sliderGO.AddComponent<UISlider>();
        SetPrivateField(slider, "_fillRect", fillRect);
        SetPrivateField(slider, "_handleRect", handleRect);
        slider.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
        
        slider.OnValueChanged.AddListener((val) => 
        {
            if (progressBar != null)
            {
                progressBar.Progress = val;
            }
        });
    }
    
    private void CreateBaseUI()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        
        if (FindObjectOfType<Canvas>() == null)
        {
            var canvasGO = new GameObject("DemoCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        if (FindObjectOfType<UIWidgetManager>() == null) new GameObject("UIWidgetManager").AddComponent<UIWidgetManager>();
        if (FindObjectOfType<UIInputHandler>() == null) new GameObject("UIInputHandler").AddComponent<UIInputHandler>();
        if (FindObjectOfType<UIOverlayManager>() == null) new GameObject("UIOverlayManager").AddComponent<UIOverlayManager>();

        Debug.Log("Basis-UI und Manager wurden erstellt.");
    }

    private void DemonstrateUIWidget()
    {
        var widget = CreateSimpleWidget("DemoWidget", "Ich bin ein Widget!", Color.cyan);
        widget.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 200);
        StartCoroutineInEditor(DemoSequence(widget));
    }
    
    private IEnumerator DemoSequence(UIWidget widget)
    {
        if (widget == null) yield break;
        Debug.Log("Demonstriere Hide...");
        widget.Hide();
        yield return new WaitForSeconds(1);

        if (widget == null) yield break;
        Debug.Log("Demonstriere Show...");
        widget.Show();
        yield return new WaitForSeconds(1);

        if (widget == null) yield break;
        Debug.Log("Demonstriere TweenPosition...");
        widget.TweenPosition(widget.GetComponent<RectTransform>().anchoredPosition + Vector2.right * 100, 0.5f, Easing.EaseType.EaseInOutQuad);
        yield return new WaitForSeconds(1);

        if (widget == null) yield break;
        Debug.Log("Demonstriere TweenScale...");
        widget.TweenScale(Vector3.one * 1.5f, 0.5f, Easing.EaseType.EaseOutQuad);
        yield return new WaitForSeconds(1);

        if (widget == null) yield break;
        widget.TweenScale(Vector3.one, 0.5f, Easing.EaseType.EaseInQuad);
        yield return new WaitForSeconds(1);

        if (widget == null) yield break;
        Debug.Log("Demonstriere SetState(Disabled)...");
        widget.SetState(UIWidget.UIState.Disabled);
    }

    private void DemonstrateMenus()
    {
        var menu = CreateSimpleMenu("StandardMenü", new Vector2(0, 150));
        menu.AddWidget("Eintrag A");
        menu.AddWidget("Eintrag B");

        var scrollList = CreateScrollList("ScrollListe", new Vector2(250, 0));
        for (int i = 0; i < 100; i++) scrollList.AddWidget($"Gepoolter Eintrag {i + 1}");
    }
    
    private void DemonstrateModalDialog()
    {
        var dialogGO = CreatePanel("DemoDialog", Color.gray, Vector2.zero, new Vector2(300, 150));
        var modalDialog = dialogGO.AddComponent<UIModalDialog>();

        var title = CreateText("Title", "Titel", dialogGO.transform, 18);
        var message = CreateText("Message", "Nachricht", dialogGO.transform, 14);
        var okButton = CreateButton("OK", "OK", new Vector2(-60, -50)).GetComponent<UIButton>();
        var cancelButton = CreateButton("Cancel", "Cancel", new Vector2(60, -50)).GetComponent<UIButton>();

        title.rectTransform.anchoredPosition = new Vector2(0, 50);
        message.rectTransform.anchoredPosition = Vector2.zero;
        okButton.transform.SetParent(dialogGO.transform, false);
        cancelButton.transform.SetParent(dialogGO.transform, false);
        
        SetPrivateField(modalDialog, "titleWidget", title.GetComponent<UIWidget>());
        SetPrivateField(modalDialog, "messageWidget", message.GetComponent<UIWidget>());
        SetPrivateField(modalDialog, "okButton", okButton);
        SetPrivateField(modalDialog, "cancelButton", cancelButton);
        modalDialog.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

        modalDialog.ShowDialog(
            "Bestätigung",
            "Möchtest du diese Aktion wirklich ausführen?",
            () => Debug.Log("OK wurde geklickt!"),
            () => Debug.Log("Abbrechen wurde geklickt!")
        );
    }

    private void DemonstrateToggleButton()
    {
        var toggleButtonGO = CreateButton("DemoToggle", "Toggle Me", new Vector2(0, -50));
        var toggleButton = toggleButtonGO.gameObject.AddComponent<UIToggleButton>();
        
        var checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(toggleButton.transform, false);
        var checkmarkText = checkmark.AddComponent<TextMeshProUGUI>();
        checkmarkText.text = "✓";
        checkmarkText.color = Color.green;
        checkmarkText.fontSize = 24;
        checkmarkText.rectTransform.anchoredPosition = new Vector2(-60, 0);

        SetPrivateField(toggleButton, "_checkmark", checkmark);
        toggleButton.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
        
        toggleButton.OnCheckChanged.AddListener((isChecked) => Debug.Log($"Toggle-Status: {isChecked}"));
    }
    
    private void DemonstrateQuestTracker()
    {
        var questTrackerGO = CreatePanel("QuestTracker", new Color(0,0,0,0.5f), new Vector2(-155, 250), new Vector2(300, 100));
        var vlg = questTrackerGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 5;
        questTrackerGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var title = CreateText("QuestTitle", "Quest Titel", questTrackerGO.transform, 18);
        title.color = Color.yellow;
        title.alignment = TextAlignmentOptions.Left;

        var objectivesContainer = CreatePanel("ObjectivesContainer", Color.clear, Vector2.zero, new Vector2(280, 60));
        objectivesContainer.transform.SetParent(questTrackerGO.transform, false);
        objectivesContainer.AddComponent<VerticalLayoutGroup>();

        var objectiveTemplate = CreateText("ObjectiveTemplate", "Ziel-Text", objectivesContainer.transform, 14);
        objectiveTemplate.color = Color.white;
        objectiveTemplate.alignment = TextAlignmentOptions.Left;
        objectiveTemplate.gameObject.SetActive(false);
        
        var questTracker = questTrackerGO.AddComponent<UIQuestTracker>();
        SetPrivateField(questTracker, "_questTitleText", title);
        SetPrivateField(questTracker, "_objectivesContainer", objectivesContainer.GetComponent<RectTransform>());
        SetPrivateField(questTracker, "_objectiveTemplate", objectiveTemplate.gameObject);
        questTracker.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
        
        var demoQuest = ScriptableObject.CreateInstance<Quest>();
        demoQuest.Title = "Finde die verlorenen Artefakte";
        demoQuest.Objectives = new List<Objective>
        {
            new Objective { Description = "Sammle das Sonnenamulett", CurrentProgress = 0, RequiredAmount = 1 },
            new Objective { Description = "Besiege 5 Goblins", CurrentProgress = 3, RequiredAmount = 5 }
        };
        questTracker.UpdateTracker(demoQuest);
    }

    private void DemonstrateMinimap()
    {
        var playerGO = GameObject.Find("DemoPlayer");
        if(playerGO == null) playerGO = new GameObject("DemoPlayer");
        playerGO.transform.position = Vector3.zero;

        var npcGO = GameObject.Find("DemoNPC");
        if(npcGO == null) npcGO = new GameObject("DemoNPC");
        npcGO.transform.position = new Vector3(10, 0, 15);

        var minimapGO = CreatePanel("Minimap", Color.black, new Vector2(120, -280), new Vector2(200, 200));
        minimapGO.GetComponent<Image>().sprite = GetPlaceholderSprite(true);
        minimapGO.AddComponent<Mask>().showMaskGraphic = false;
        
        var iconContainer = CreatePanel("IconContainer", Color.clear, Vector2.zero, new Vector2(200,200));
        iconContainer.transform.SetParent(minimapGO.transform, false);

        var playerIconGO = CreatePanel("PlayerIcon", Color.white, Vector2.zero, new Vector2(15,15));
        playerIconGO.transform.SetParent(iconContainer.transform, false);
        
        var iconPrefab = CreatePanel("IconPrefab", Color.yellow, Vector2.zero, new Vector2(10,10));
        iconPrefab.AddComponent<UIMinimapIcon>();
        iconPrefab.GetComponent<Image>().sprite = GetPlaceholderSprite(true);
        iconPrefab.transform.SetParent(minimapGO.transform, false);
        iconPrefab.SetActive(false);

        var minimap = minimapGO.AddComponent<UIMinimap>();
        minimap.SetPlayer(playerGO.transform);
        SetPrivateField(minimap, "_iconContainer", iconContainer.GetComponent<RectTransform>());
        SetPrivateField(minimap, "_playerIcon", playerIconGO.AddComponent<UIMinimapIcon>());
        SetPrivateField(minimap, "_iconPrefab", iconPrefab);

        minimap.AddIcon(npcGO.transform, GetPlaceholderSprite(true), Color.yellow);
    }
    
    private void DemonstrateInventory()
    {
        var inventoryPanel = CreatePanel("Inventar", new Color(0.2f, 0.2f, 0.2f, 0.8f), new Vector2(-250, -150), new Vector2(220, 220));
        var grid = inventoryPanel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(50, 50);
        grid.spacing = new Vector2(5, 5);
        grid.padding = new RectOffset(5,5,5,5);

        var itemPrefab = CreateDraggableItem("ItemPrefab", "Item");
        itemPrefab.gameObject.SetActive(false);

        for (int i = 0; i < 16; i++)
        {
            var slotGO = CreatePanel($"Slot_{i}", Color.black, Vector2.zero, new Vector2(50, 50));
            slotGO.transform.SetParent(inventoryPanel.transform, false);
            var slot = slotGO.AddComponent<UIInventorySlot>();

            var stackText = CreateText("StackText", "", slotGO.transform, 12);
            stackText.rectTransform.anchorMin = new Vector2(1,0);
            stackText.rectTransform.anchorMax = new Vector2(1,0);
            stackText.rectTransform.anchoredPosition = new Vector2(-5, 5);
            stackText.alignment = TextAlignmentOptions.BottomRight;
            
            SetPrivateField(slot, "_itemInSlotPrefab", itemPrefab);
            SetPrivateField(slot, "_stackSizeText", stackText);
            
            if (i < 3) slot.SetItem(GetPlaceholderSprite(), i + 2);
        }
        if (itemPrefab != null) DestroyImmediate(itemPrefab);
    }

    private void DemonstrateActionBar()
    {
        var actionBarPanel = CreatePanel("Aktionsleiste", new Color(0.1f, 0.1f, 0.1f, 0.7f), new Vector2(0, -300), new Vector2(170, 55));
        var hlg = actionBarPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 5;
        hlg.padding = new RectOffset(5,5,5,5);

        for (int i = 0; i < 3; i++)
        {
            var slotGO = CreatePanel($"ActionSlot_{i+1}", Color.black, Vector2.zero, new Vector2(50, 50));
            slotGO.transform.SetParent(actionBarPanel.transform, false);
            
            var iconImg = new GameObject("Icon").AddComponent<Image>();
            iconImg.transform.SetParent(slotGO.transform, false);
            iconImg.rectTransform.sizeDelta = new Vector2(50,50);

            var keyText = CreateText("KeybindText", (i+1).ToString(), slotGO.transform, 14);
            keyText.rectTransform.anchoredPosition = new Vector2(10, -10);
            
            var cooldownImgGo = CreatePanel("Cooldown", Color.clear, Vector2.zero, new Vector2(50,50));
            cooldownImgGo.transform.SetParent(slotGO.transform, false);
            var cooldownImg = cooldownImgGo.GetComponent<Image>();
            cooldownImg.color = new Color(0,0,0,0.7f);
            cooldownImg.type = Image.Type.Filled;
            cooldownImg.fillMethod = Image.FillMethod.Radial360;

            var slot = slotGO.AddComponent<UIActionSlot>();
            slot.ActivationKey = KeyCode.Alpha1 + i;
            SetPrivateField(slot, "_icon", iconImg);
            SetPrivateField(slot, "_cooldownOverlay", cooldownImg);
            SetPrivateField(slot, "_keybindText", keyText);
            
            slot.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
            slot.Initialize();
            slot.Assign(GetPlaceholderSprite());
        }
    }
    
    private Canvas GetCanvas() => FindObjectOfType<Canvas>();

    private UIWidget CreateSimpleWidget(string name, string text, Color color)
    {
        var panel = CreatePanel(name, color, Vector2.zero, new Vector2(150, 50));
        CreateText(name + "_Text", text, panel.transform);
        return panel.GetComponent<UIWidget>();
    }

    private UIWidget CreateButton(string name, string text, Vector2 position, Vector2 size = default)
    {
        if (size == default) size = new Vector2(100, 30);
        var buttonPanel = CreatePanel(name, Color.white, position, size);
        buttonPanel.AddComponent<UIButton>();
        CreateText(name + "_Text", text, buttonPanel.transform);
        return buttonPanel.GetComponent<UIWidget>();
    }
    
    private GameObject CreatePanel(string name, Color color, Vector2 position, Vector2 size)
    {
        var canvas = GetCanvas();
        if (canvas == null)
        {
            Debug.LogError("Kann kein Panel erstellen, da kein Canvas in der Szene ist. Bitte zuerst 'Basis-UI erstellen' klicken.");
            return null;
        }
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.color = color;
        var widget = go.AddComponent<UIWidget>();
        widget.Name = name;
        return go;
    }

    private TextMeshProUGUI CreateText(string name, string content, Transform parent, int fontSize = 20)
    {
        var go = new GameObject(name, typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        var rt = text.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        return text;
    }
    
    private UIMenu CreateSimpleMenu(string name, Vector2 position)
    {
        var menuPanel = CreatePanel(name, Color.yellow, position, new Vector2(200, 120));
        var vlg = menuPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5,5,5,5);
        vlg.spacing = 5;
        menuPanel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var menu = menuPanel.AddComponent<UIMenu>();
        
        var template = CreateButton(name + "_Template", "Template", Vector2.zero);
        template.transform.SetParent(menu.transform, false);
        template.gameObject.SetActive(false);
        
        SetProtectedField(menu, "_itemTemplate", template);
        SetProtectedField(menu, "_contentContainer", menu.GetComponent<RectTransform>());
        
        return menu;
    }

    private UIScrollList CreateScrollList(string name, Vector2 position)
    {
        var scrollRectGO = CreatePanel(name, new Color(0.5f, 0.5f, 0.5f), position, new Vector2(200, 200));
        var scrollRect = scrollRectGO.AddComponent<ScrollRect>();
        
        var viewport = CreatePanel("Viewport", Color.clear, Vector2.zero, new Vector2(200, 200));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.transform.SetParent(scrollRect.transform, false);
        var content = CreatePanel("Content", Color.clear, Vector2.zero, new Vector2(200, 500));
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        content.transform.SetParent(viewport.transform, false);
        
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = content.GetComponent<RectTransform>();
        var scrollList = scrollRectGO.AddComponent<UIScrollList>();
        var template = CreateButton(name + "_Template", "Template", Vector2.zero);
        template.transform.SetParent(content.transform, false);
        template.gameObject.SetActive(false);

        SetProtectedField(scrollList, "_itemTemplate", template);
        SetProtectedField(scrollList, "_contentContainer", content.GetComponent<RectTransform>());
        
        return scrollList;
    }
    
    private UIDraggable CreateDraggableItem(string name, string text)
    {
        var itemPanel = CreatePanel(name, Color.magenta, Vector2.zero, new Vector2(50, 50));
        var draggable = itemPanel.AddComponent<UIDraggable>();
        CreateText("ItemText", text, itemPanel.transform, 12);
        return draggable;
    }
    
    private Sprite GetPlaceholderSprite(bool circle = false)
    {
        Texture2D texture;
        if (circle)
        {
            texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var center = new Vector2(31.5f, 31.5f);
            for(int y = 0; y < texture.height; y++)
            {
                for(int x = 0; x < texture.width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, dist < 32 ? Color.white : Color.clear);
                }
            }
        }
        else
        {
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        if (obj == null) return;
        obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(obj, value);
    }
    
    private void SetProtectedField(object obj, string fieldName, object value)
    {
        if (obj == null) return;
        obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)?.SetValue(obj, value);
    }
    
    private void StartCoroutineInEditor(IEnumerator coroutine)
    {
        EditorApplication.CallbackFunction update = null;
        update = () =>
        {
            if (coroutine == null || !coroutine.MoveNext())
            {
                EditorApplication.update -= update;
            }
        };
        EditorApplication.update += update;
    }
    #endregion
}