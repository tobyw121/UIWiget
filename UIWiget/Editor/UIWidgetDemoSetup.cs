// Dateiname: UIWidgetDemoSetup.cs
// WICHTIG: Muss in einem Ordner namens "Editor" platziert werden.
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
using UnityEngine.EventSystems;

public class UIWidgetDemoSetup : EditorWindow
{
    [MenuItem("Mein Spiel/UI-System Komplett-Demo")]
    public static void ShowWindow()
    {
        GetWindow<UIWidgetDemoSetup>("UI-System Komplett-Demo");
    }

    private Vector2 _scrollPosition;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("UI-System Demonstrations-Fenster", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Klicken Sie im Play-Modus auf die Buttons, um die entsprechenden UI-Elemente und Systeme live zu erstellen.", MessageType.Info);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Bitte starten Sie den Play-Modus, um die Demo-Funktionen zu nutzen.", MessageType.Warning);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        // --- Basis-Setup ---
        if (GUILayout.Button("1. Basis-UI & Manager erstellen")) CreateBaseUI();
        EditorGUILayout.HelpBox("Erstellt Canvas, EventSystem und die notwendigen Manager. Dies sollte als Erstes ausgeführt werden.", MessageType.None);
        
        EditorGUILayout.Space(10);

        // --- Komponenten-Demonstrationen ---
        GUILayout.Label("Grundlegende UI-Funktionen", EditorStyles.boldLabel);
        DrawTwoColumnButtons(
            ("UIWidget Kernfunktionen", DemonstrateUIWidget),
            ("Sounds & Tooltips", DemonstrateSoundsAndTooltips),
            ("Fokus & Tastatur-Toggle", DemonstrateFocusAndKeyToggle),
            ("Exklusiv-Modus & Overlay", DemonstrateExclusiveMode)
        );

        EditorGUILayout.Space(10);
        
        GUILayout.Label("Interaktive Elemente & Layouts", EditorStyles.boldLabel);
        DrawTwoColumnButtons(
            ("Buttons (Standard & Toggle)", DemonstrateButtons),
            ("Slider & Progress Bars", DemonstrateSlidersAndProgress),
            ("Tab-Ansicht (UITabView)", DemonstrateTabView),
            ("Radial-Menü", DemonstrateRadialMenu)
        );

        EditorGUILayout.Space(10);
        
        GUILayout.Label("Komplexe RPG-Module", EditorStyles.boldLabel);
        DrawTwoColumnButtons(
            ("Drag & Drop Inventar", DemonstrateInventory),
            ("Aktionsleiste (Tasten 1-3)", DemonstrateActionBar),
            ("Kantine / Buff-Food", DemonstrateKantine),
            ("Crafting-Panel", DemonstrateCraftingPanel),
            ("Ausrüstungs-Vergleich", DemonstrateComparisonTooltip),
            ("Skill-Baum", DemonstrateSkillTree)
        );

        EditorGUILayout.Space(10);

        // --- System-Integrationen ---
        GUILayout.Label("System-Integrationen", EditorStyles.boldLabel);
        DrawTwoColumnButtons(
            ("Quest-Tracker", DemonstrateQuestTracker),
            ("Minimap", DemonstrateMinimap),
            ("Notification Manager", DemonstrateNotifications)
        );
        
        EditorGUILayout.Space(15);
        
        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
        if (GUILayout.Button("ALLES LÖSCHEN (Neue Demo starten)"))
        {
            var canvas = FindObjectOfType<Canvas>();
            if(canvas) DestroyImmediate(canvas.gameObject);
            
            var managers = FindObjectsOfType<UIWidgetManager>();
            foreach(var m in managers) DestroyImmediate(m.gameObject);
            
            var eventSystems = FindObjectsOfType<EventSystem>();
            foreach(var es in eventSystems) DestroyImmediate(es.gameObject);
        }
        GUI.backgroundColor = originalColor;

        EditorGUILayout.EndScrollView();
    }
    
    private void DrawTwoColumnButtons(params (string label, System.Action action)[] buttons)
    {
        for (int i = 0; i < buttons.Length; i += 2)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(buttons[i].label)) buttons[i].action.Invoke();
            if (i + 1 < buttons.Length)
            {
                if (GUILayout.Button(buttons[i + 1].label)) buttons[i + 1].action.Invoke();
            }
            else
            {
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    #region Demo Methods
    
    private void DemonstrateUIWidget()
    {
        var widget = CreateSimpleWidget("DemoWidget", "Ich bin ein Widget!", new Color(0.1f, 0.4f, 0.7f));
        widget.RectTransform.anchoredPosition = new Vector2(-250, 200);
        StartCoroutineInEditor(DemoSequence(widget));
    }

    private void DemonstrateSoundsAndTooltips()
    {
        var soundWidget = CreateSimpleWidget("SoundWidget", "Widget mit Sound", new Color(0.8f, 0.4f, 0.1f));
        soundWidget.RectTransform.anchoredPosition = new Vector2(-250, -50);
        var audioSource = soundWidget.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (FindObjectOfType<UITooltipDisplay>() == null)
        {
             var tooltipDisplayGO = new GameObject("UITooltipDisplay");
             var tooltipDisplay = tooltipDisplayGO.AddComponent<UITooltipDisplay>();
             var tooltipTemplate = CreateSimpleWidget("TooltipTemplate", "", Color.black);
             tooltipTemplate.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
             SetPrivateField(tooltipDisplay, "tooltipWidget", tooltipTemplate);
             tooltipTemplate.Hide();
        }

        var tooltipWidget = CreateSimpleWidget("TooltipWidget", "Hover für Tooltip!", new Color(0.1f, 0.6f, 0.6f));
        tooltipWidget.RectTransform.anchoredPosition = new Vector2(250, -50);
        tooltipWidget.Tooltip.Enabled = true;
        tooltipWidget.Tooltip.TooltipText = "Dies ist ein dynamisch generierter Tooltip!\nEr kann auch mehrzeilig sein.";
        tooltipWidget.Tooltip.Delay = 0.7f;
        tooltipWidget.OnHoverEvent.AddListener((w, isHovering) => {
            var display = FindObjectOfType<UITooltipDisplay>();
            if (display == null) return;
            if (isHovering) display.ShowTooltip(w);
            else display.HideTooltip(w);
        });
    }

    private void DemonstrateFocusAndKeyToggle()
    {
        var focusWidget1 = CreateSimpleWidget("FocusWidget1", "Klick mich für Fokus", new Color(0.5f, 0.2f, 0.5f));
        focusWidget1.RectTransform.anchoredPosition = new Vector2(-250, -200);
        focusWidget1.OnFocusGainedEvent.AddListener(() => { Debug.Log("Widget 1 hat den Fokus erhalten!"); focusWidget1.GetComponent<Image>().color = Color.magenta; });
        focusWidget1.OnFocusLostEvent.AddListener(() => { Debug.Log("Widget 1 hat den Fokus verloren!"); focusWidget1.GetComponent<Image>().color = new Color(0.5f, 0.2f, 0.5f); });
        focusWidget1.OnClickEvent.AddListener((w,d) => UIInputHandler.Instance?.SetFocus(w));
        
        var focusWidget2 = CreateSimpleWidget("FocusWidget2", "Oder klick mich!", new Color(0.5f, 0.2f, 0.5f));
        focusWidget2.RectTransform.anchoredPosition = new Vector2(0, -200);
        focusWidget2.OnFocusGainedEvent.AddListener(() => { Debug.Log("Widget 2 hat den Fokus erhalten!"); focusWidget2.GetComponent<Image>().color = Color.magenta; });
        focusWidget2.OnFocusLostEvent.AddListener(() => { Debug.Log("Widget 2 hat den Fokus verloren!"); focusWidget2.GetComponent<Image>().color = new Color(0.5f, 0.2f, 0.5f); });
        focusWidget2.OnClickEvent.AddListener((w,d) => UIInputHandler.Instance?.SetFocus(w));

        var keyToggleWidget = CreateSimpleWidget("KeyToggleWidget", "Drücke 'P' zum toggeln", new Color(0.2f, 0.5f, 0.2f));
        keyToggleWidget.RectTransform.anchoredPosition = new Vector2(250, -200);
        keyToggleWidget.SetToggleKey(KeyCode.P);
    }
    
    private void DemonstrateExclusiveMode()
    {
        var exclusiveWidget = CreateSimpleWidget("ExclusiveWidget", "Ich bin im Exklusiv-Modus", Color.red);
        exclusiveWidget.Hide(); 
        
        var button = CreateButton("ShowExclusiveButton", "Exklusiven Dialog anzeigen", Vector2.zero).GetComponent<UIButton>();
        button.OnClickEvent.AddListener((w,d) => {
            exclusiveWidget.Show();
            UIWidgetManager.SetExclusive(exclusiveWidget, new Color(0,0,0,0.8f));
        });
        
        var closeButton = CreateButton("CloseExclusive", "Schließen", Vector2.zero).GetComponent<UIButton>();
        closeButton.transform.SetParent(exclusiveWidget.transform, false);
        closeButton.OnClickEvent.AddListener((w,d) => {
             UIWidgetManager.RemoveExclusive(exclusiveWidget);
             exclusiveWidget.Hide();
        });
    }

    private void DemonstrateTabView()
    {
        var tabViewGO = CreatePanel("TabViewContainer", new Color(0.2f, 0.2f, 0.2f), new Vector2(-250, 100), new Vector2(300, 200));
        var tabView = tabViewGO.AddComponent<UITabView>();

        var tabContainer = CreatePanel("TabContainer", Color.clear, new Vector2(0, 88), new Vector2(300, 25));
        tabContainer.transform.SetParent(tabViewGO.transform, false);
        var hlg = tabContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;

        var pageContainer = CreatePanel("PageContainer", Color.black, new Vector2(0, -12), new Vector2(280, 155));
        pageContainer.transform.SetParent(tabViewGO.transform, false);

        var tabs = new List<UITab>();
        for (int i = 0; i < 3; i++)
        {
            var page = CreatePanel($"Page{i+1}", new Color(Random.value, Random.value, Random.value, 0.7f), Vector2.zero, new Vector2(280, 155));
            page.transform.SetParent(pageContainer.transform, false);
            CreateText($"PageText{i+1}", $"Inhalt von Tab {i+1}", page.transform);

            var tabGO = CreateButton($"Tab{i+1}", $"Tab {i+1}", Vector2.zero);
            tabGO.transform.SetParent(tabContainer.transform, false);
            var tab = tabGO.AddComponent<UITab>();
            SetPrivateField(tab, "_tabPage", page);
            tabs.Add(tab);
        }
        
        SetPrivateField(tabView, "_tabs", tabs);
        tabView.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
        tabView.SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
    }

    private void DemonstrateRadialMenu()
    {
        var radialMenu = new GameObject("RadialMenu", typeof(RectTransform)).AddComponent<UIRadialMenu>();
        radialMenu.transform.SetParent(GetCanvas().transform, false);
        var itemTemplateGO = CreateButton("RadialItemTemplate", "Item", Vector2.zero, new Vector2(80,30));
        itemTemplateGO.SetActive(false);
        itemTemplateGO.transform.SetParent(radialMenu.transform,false);
        var itemTemplate = itemTemplateGO.GetComponent<UIWidget>();
        SetProtectedField(radialMenu, "_itemTemplate", itemTemplate);
        
        for (int i = 0; i < 5; i++)
        {
            var item = radialMenu.AddWidget($"Radial {i+1}");
            item.SetText($"Aktion {i+1}");
        }
        radialMenu.Hide();

        var openButton = CreateButton("OpenRadial", "Radialmenü hier öffnen", new Vector2(0, -300));
        openButton.GetComponent<UIButton>().OnClickEvent.AddListener((w,d) => radialMenu.Show());
    }

    private void DemonstrateNotifications()
    {
        if (FindObjectOfType<UINotificationManager>() == null)
        {
            var managerGO = new GameObject("NotificationManager");

            var container = CreatePanel("NotificationContainer", Color.clear, new Vector2(0, Screen.height/2f - 50), new Vector2(400,10));
            container.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
            container.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
            container.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            container.AddComponent<VerticalLayoutGroup>();
            container.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            var templateGO = CreatePanel("NotificationTemplate", new Color(0.1f, 0.1f, 0.1f, 0.9f), Vector2.zero, new Vector2(300, 50));
            var template = templateGO.AddComponent<UINotification>();
            
            var titleText = CreateText("Title", "Titel", template.transform, 16);
            titleText.rectTransform.anchoredPosition = new Vector2(0, 10);
            var messageText = CreateText("Message", "Nachricht", template.transform, 12);
            messageText.rectTransform.anchoredPosition = new Vector2(0, -10);
            
            SetPrivateField(template, "_titleText", titleText);
            SetPrivateField(template, "_messageText", messageText);
            
            template.gameObject.SetActive(false);
            
            var manager = managerGO.AddComponent<UINotificationManager>();
            
            SetPrivateField(manager, "_notificationPrefab", template);
            SetPrivateField(manager, "_container", container.transform);
            
            manager.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
        }
        
        UINotificationManager.Show("Erfolg!", "Du hast eine Notification ausgelöst.");
        StartCoroutineInEditor(DelayedNotification());
    }
    
    private void DemonstrateMenus()
    {
        var menu = CreateSimpleMenu("StandardMenü", new Vector2(0, 150));
        menu.AddWidget("Eintrag A").SetText("Eintrag A");
        menu.AddWidget("Eintrag B").SetText("Eintrag B");
        menu.Show();

        var scrollList = CreateScrollList("ScrollListe", new Vector2(250, 0));
        for (int i = 0; i < 50; i++)
        {
            var item = scrollList.AddWidget($"Gepoolter Eintrag {i + 1}");
            item.SetText($"Gepoolter Eintrag {i + 1}");
        }
        scrollList.Show();
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
        progressBar.Show();

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
        
        slider.OnValueChanged.AddListener((val) => {
            if (progressBar != null) progressBar.Progress = val;
        });
        slider.Show();
    }
    
    private void DemonstrateButtons()
    {
        var standardButton = CreateButton("DemoButton", "Klick Mich", new Vector2(-150, -50)).GetComponent<UIButton>();
        standardButton.OnClickEvent.AddListener((w, d) => Debug.Log("Standard-Button wurde geklickt!"));
        standardButton.Show();

        var toggleButtonGO = CreateButton("DemoToggle", "Toggle Me", new Vector2(150, -50));
        var toggleButton = toggleButtonGO.AddComponent<UIToggleButton>();
        
        var checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(toggleButton.transform, false);
        var checkmarkText = checkmark.AddComponent<TextMeshProUGUI>();
        checkmarkText.text = "✓";
        checkmarkText.color = Color.green;
        checkmarkText.fontSize = 24;
        checkmarkText.rectTransform.anchoredPosition = new Vector2(-70, 0);

        SetPrivateField(toggleButton, "_checkmark", checkmark);
        toggleButton.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
        toggleButton.OnCheckChanged.AddListener((isChecked) => Debug.Log($"Toggle-Status: {isChecked}"));
        toggleButton.Show();
    }
    
    private void DemonstrateModalDialog()
    {
        var dialogGO = CreatePanel("DemoDialog", new Color(0.2f,0.2f,0.2f), Vector2.zero, new Vector2(350, 180));
        var modalDialog = dialogGO.AddComponent<UIModalDialog>();

        var title = CreateText("Title", "Titel", dialogGO.transform, 18);
        var message = CreateText("Message", "Nachricht", dialogGO.transform, 14);
        var okButtonGO = CreateButton("OK", "OK", new Vector2(-70, -60));
        var cancelButtonGO = CreateButton("Cancel", "Abbrechen", new Vector2(70, -60));

        title.rectTransform.anchoredPosition = new Vector2(0, 65);
        message.rectTransform.sizeDelta = new Vector2(-20, -80);
        okButtonGO.transform.SetParent(dialogGO.transform, false);
        cancelButtonGO.transform.SetParent(dialogGO.transform, false);
        
        SetPrivateField(modalDialog, "titleWidget", title.gameObject.AddComponent<UIWidget>());
        SetPrivateField(modalDialog, "messageWidget", message.gameObject.AddComponent<UIWidget>());
        SetPrivateField(modalDialog, "okButton", okButtonGO.GetComponent<UIButton>());
        SetPrivateField(modalDialog, "cancelButton", cancelButtonGO.GetComponent<UIButton>());
        modalDialog.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

        modalDialog.ShowDialog(
            "Dies ist eine Bestätigung",
            "Bist du sicher, dass du diese Aktion ausführen möchtest?",
            () => Debug.Log("OK wurde geklickt!"),
            () => Debug.Log("Abbrechen wurde geklickt!")
        );
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
        itemPrefab.transform.SetParent(inventoryPanel.transform);

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
            
            SetPrivateField(slot, "_itemInSlotPrefab", itemPrefab.GetComponent<UIDraggable>());
            SetPrivateField(slot, "_stackSizeText", stackText);
            
            if (i < 3) slot.SetItem(GetPlaceholderSprite(), i + 2);
        }
        inventoryPanel.GetComponent<UIWidget>().Show();
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
            iconImg.sprite = GetPlaceholderSprite();

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
        actionBarPanel.GetComponent<UIWidget>().Show();
    }
    
    private void DemonstrateKantine()
    {
        var heartySteak = ScriptableObject.CreateInstance<IngredientData>();
        heartySteak.itemName = "Herzhaftes Steak";
        heartySteak.buffs = new List<Buff> { new Buff { type = BuffType.Angriff, value = 10 } };
        var sunHerb = ScriptableObject.CreateInstance<IngredientData>();
        sunHerb.itemName = "Sonnenkraut";
        sunHerb.buffs = new List<Buff> { new Buff { type = BuffType.Verteidigung, value = 15 } };
        var allIngredients = new List<IngredientData> { heartySteak, sunHerb };

        var kantineGO = CreatePanel("KantinePanel", new Color(0.3f, 0.2f, 0.2f), Vector2.zero, new Vector2(550, 350));
        var kantine = kantineGO.AddComponent<UIKantine>();

        var selectionList = CreateScrollList("IngredientSelection", new Vector2(-150, 0));
        selectionList.transform.SetParent(kantineGO.transform, false);

        var slotsPanel = CreatePanel("KochSlots", Color.clear, new Vector2(125, 100), new Vector2(250, 70));
        slotsPanel.transform.SetParent(kantineGO.transform, false);
        slotsPanel.AddComponent<HorizontalLayoutGroup>().spacing = 10;
        
        var slots = new List<UIInventorySlot>();
        for (int i = 0; i < 3; i++)
        {
            var slotGO = CreatePanel($"KochSlot_{i}", Color.black, Vector2.zero, new Vector2(60, 60));
            slotGO.transform.SetParent(slotsPanel.transform, false);
            var invSlot = slotGO.AddComponent<UIInventorySlot>();
            SetPrivateField(invSlot, "_itemInSlotPrefab", CreateDraggableItem("dummy", "").GetComponent<UIDraggable>());
            slots.Add(invSlot);
        }

        var resultText = CreateText("ResultText", "Wähle Zutaten...", kantineGO.transform, 16);
        resultText.rectTransform.anchoredPosition = new Vector2(125, -20);
        resultText.rectTransform.sizeDelta = new Vector2(240, 100);
        resultText.alignment = TextAlignmentOptions.TopLeft;

        var cookButtonGO = CreateButton("CookButton", "Kochen", new Vector2(125, -125));
        cookButtonGO.transform.SetParent(kantineGO.transform, false);
        var cookButton = cookButtonGO.GetComponent<UIButton>();

        kantine.Initialize(slots, resultText, cookButton, selectionList);
        kantine.PopulateIngredients(allIngredients);
        kantine.Show();
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
        swordRecipe.requiredMaterials = new List<RequiredMaterial> {
            new RequiredMaterial { item = ironOre, quantity = 5 },
            new RequiredMaterial { item = wood, quantity = 2 }
        };

        var craftingGO = CreatePanel("CraftingPanel", new Color(0.2f, 0.2f, 0.3f), Vector2.zero, new Vector2(500, 300));
        var craftingPanel = craftingGO.AddComponent<UICraftingPanel>();

        var recipeList = CreateScrollList("RecipeList", new Vector2(-125, 0));
        recipeList.transform.SetParent(craftingGO.transform, false);

        var resultIcon = CreatePanel("ResultIcon", Color.black, new Vector2(125, 100), new Vector2(80, 80)).GetComponent<Image>();
        var resultName = CreateText("ResultName", "Wähle ein Rezept", craftingGO.transform, 20);
        resultName.rectTransform.anchoredPosition = new Vector2(125, 40);
        
        var materialsContainer = CreatePanel("MaterialsContainer", Color.clear, new Vector2(125, -30), new Vector2(220, 150)).GetComponent<RectTransform>();
        materialsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        materialsContainer.transform.SetParent(craftingGO.transform, false);

        var craftButtonGO = CreateButton("CraftButton", "Herstellen", new Vector2(125, -120));
        craftButtonGO.transform.SetParent(craftingGO.transform, false);
        var craftButton = craftButtonGO.GetComponent<UIButton>();
        
        var matTemplate = CreatePanel("MaterialTemplate", Color.clear, Vector2.zero, new Vector2(220, 30));
        var hlg = matTemplate.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        CreatePanel("Icon", Color.white, Vector2.zero, new Vector2(25,25)).transform.SetParent(matTemplate.transform, false);
        CreateText("Name", "Mat Name", matTemplate.transform, 14).alignment = TextAlignmentOptions.Left;
        CreateText("Amount", "0/0", matTemplate.transform, 14).alignment = TextAlignmentOptions.Right;
        matTemplate.transform.SetParent(materialsContainer.transform, false);
        matTemplate.SetActive(false);
        
        craftingPanel.Initialize(recipeList, resultIcon, resultName, materialsContainer, matTemplate, craftButton);
        craftingPanel.PopulateRecipes(new List<CraftingRecipe> { swordRecipe });
        craftingPanel.Show();
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
        
        var tooltipGO = CreatePanel("ComparisonTooltip", new Color(0.1f, 0.1f, 0.1f, 0.95f), new Vector2(Screen.width / 2f, Screen.height / 2f), new Vector2(400, 180));
        var tooltip = tooltipGO.AddComponent<UIComparisonTooltip>();
        
        var currentPanel = CreatePanel("CurrentItemPanel", Color.clear, new Vector2(-100, 0), new Vector2(180, 160));
        var newPanel = CreatePanel("NewItemPanel", Color.clear, new Vector2(100, 0), new Vector2(180, 160));
        currentPanel.transform.SetParent(tooltipGO.transform, false);
        newPanel.transform.SetParent(tooltipGO.transform, false);

        void CreateStatBlock(Transform parent, out TextMeshProUGUI name, out Image icon, out TextMeshProUGUI atk, out TextMeshProUGUI def)
        {
            name = CreateText("Name", "Item", parent, 16);
            name.rectTransform.anchoredPosition = new Vector2(0, 65);
            icon = CreatePanel("Icon", Color.gray, new Vector2(0, 20), new Vector2(50,50)).GetComponent<Image>();
            atk = CreateText("Atk", "Atk: 0", parent, 14);
            atk.rectTransform.anchoredPosition = new Vector2(0, -25);
            def = CreateText("Def", "Def: 0", parent, 14);
            def.rectTransform.anchoredPosition = new Vector2(0, -45);
        }

        CreateStatBlock(currentPanel.transform, out var cName, out var cIcon, out var cAtk, out var cDef);
        CreateStatBlock(newPanel.transform, out var nName, out var nIcon, out var nAtk, out var nDef);
        
        SetPrivateField(tooltip, "_currentItemPanel", currentPanel);
        SetPrivateField(tooltip, "_currentItemName", cName);
        SetPrivateField(tooltip, "_currentItemIcon", cIcon);
        SetPrivateField(tooltip, "_currentItemAttack", cAtk);
        SetPrivateField(tooltip, "_currentItemDefense", cDef);
        
        SetPrivateField(tooltip, "_newItemPanel", newPanel);
        SetPrivateField(tooltip, "_newItemName", nName);
        SetPrivateField(tooltip, "_newItemIcon", nIcon);
        SetPrivateField(tooltip, "_newItemAttack", nAtk);
        SetPrivateField(tooltip, "_newItemDefense", nDef);
        tooltip.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

        tooltip.ShowComparison(newSword, currentSword);
    }

    private void DemonstrateSkillTree()
    {
        var skillTreeGO = CreatePanel("SkillTree", new Color(0.1f, 0.2f, 0.1f), Vector2.zero, new Vector2(600, 400));
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
        skillTree.Show();
    }

    private void DemonstrateQuestTracker()
    {
        var questTrackerGO = CreatePanel("QuestTracker", new Color(0,0,0,0.5f), new Vector2(250, 250), new Vector2(300, 100));
        var vlg = questTrackerGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 5;
        questTrackerGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var title = CreateText("QuestTitle", "Quest Titel", questTrackerGO.transform, 18);
        title.color = Color.yellow;
        title.alignment = TextAlignmentOptions.Left;

        var objectivesContainer = CreatePanel("ObjectivesContainer", Color.clear, Vector2.zero, new Vector2(280, 10));
        objectivesContainer.transform.SetParent(questTrackerGO.transform, false);
        objectivesContainer.AddComponent<VerticalLayoutGroup>();
        objectivesContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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
        demoQuest.Objectives = new List<Objective> {
            new Objective { Description = "Sammle das Sonnenamulett", CurrentProgress = 0, RequiredAmount = 1 },
            new Objective { Description = "Besiege 5 Goblins", CurrentProgress = 3, RequiredAmount = 5 }
        };
        questTracker.UpdateTracker(demoQuest);
    }
    
    private void DemonstrateMinimap()
    {
        var playerGO = GameObject.Find("DemoPlayer") ?? new GameObject("DemoPlayer");
        playerGO.transform.position = Vector3.zero;

        var npcGO = GameObject.Find("DemoNPC") ?? new GameObject("DemoNPC");
        npcGO.transform.position = new Vector3(10, 0, 15);

        var minimapGO = CreatePanel("Minimap", Color.black, new Vector2(Screen.width/2f - 120, -Screen.height/2f + 120), new Vector2(200, 200));
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
        minimap.Show();
    }
    
    #endregion
    
    #region Helper Methods
    
    private IEnumerator DemoSequence(UIWidget widget)
    {
        if (widget == null) yield break;
        Debug.Log("DEMO: Zeige Widget...");
        widget.Show();
        yield return new EditorWaitForSeconds(1.5f);

        if (widget == null) yield break;
        Debug.Log("DEMO: Tweene Position...");
        widget.TweenPosition(widget.RectTransform.anchoredPosition + Vector2.right * 100, 0.5f, Easing.EaseType.EaseInOutQuad);
        yield return new EditorWaitForSeconds(1.0f);
        
        if (widget == null) yield break;
        Debug.Log("DEMO: Tweene Skalierung...");
        widget.TweenScale(Vector3.one * 1.5f, 0.5f, Easing.EaseType.EaseOutBack);
        yield return new EditorWaitForSeconds(1.0f);
        
        if (widget == null) yield break;
        widget.TweenScale(Vector3.one, 0.5f, Easing.EaseType.EaseOutBack);
        yield return new EditorWaitForSeconds(1.0f);

        if (widget == null) yield break;
        Debug.Log("DEMO: Setze auf 'Disabled'...");
        widget.SetState(UIWidget.UIState.Disabled);
        yield return new EditorWaitForSeconds(1.5f);
        
        if (widget == null) yield break;
        Debug.Log("DEMO: Setze auf 'Interactive'...");
        widget.SetState(UIWidget.UIState.Interactive);
        yield return new EditorWaitForSeconds(1.5f);

        if (widget == null) yield break;
        Debug.Log("DEMO: Verstecke Widget...");
        widget.Hide();
        Debug.Log("DEMO-SEQUENZ BEENDET");
    }
    
    private IEnumerator DelayedNotification()
    {
        yield return new EditorWaitForSeconds(0.5f);
        UINotificationManager.Show("Quest Abgeschlossen", "Finde die verlorenen Artefakte.");
        yield return new EditorWaitForSeconds(0.3f);
        UINotificationManager.Show("Warnung", "Inventar ist fast voll.");
    }

    private UISkillNode CreateSkillNode(string name, Transform parent, Vector2 position, UISkillNode dependency = null)
    {
        var nodeWidgetGO = CreateButton(name, name.Substring(0,1), position, new Vector2(60, 60));
        if (nodeWidgetGO == null) return null;
        nodeWidgetGO.transform.SetParent(parent, false);
        var skillNode = nodeWidgetGO.AddComponent<UISkillNode>();
        
        skillNode.skillName = name;
        if (dependency != null)
        {
            skillNode.dependencies.Add(dependency);
        }
        return skillNode;
    }

    private UIDraggable CreateDraggableItem(string name, string text)
    {
        var itemPanel = CreatePanel(name, Color.magenta, Vector2.zero, new Vector2(50, 50));
        var draggable = itemPanel.AddComponent<UIDraggable>();
        CreateText("ItemText", text, itemPanel.transform, 12).color = Color.white;
        return draggable;
    }
    
    private UIMenu CreateSimpleMenu(string name, Vector2 position)
    {
        var menuPanel = CreatePanel(name, new Color(0.1f, 0.1f, 0.3f), position, new Vector2(200, 10));
        var vlg = menuPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5,5,5,5);
        vlg.spacing = 5;
        menuPanel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var menu = menuPanel.AddComponent<UIMenu>();
        
        var templateGO = CreateButton(name + "_Template", "Template", Vector2.zero);
        templateGO.transform.SetParent(menu.transform, false);
        templateGO.SetActive(false);
        var templateWidget = templateGO.GetComponent<UIWidget>();
        
        SetProtectedField(menu, "_itemTemplate", templateWidget);
        SetProtectedField(menu, "_contentContainer", menu.GetComponent<RectTransform>());
        
        return menu;
    }

    private UIScrollList CreateScrollList(string name, Vector2 position)
    {
        var scrollRectGO = CreatePanel(name, new Color(0.1f, 0.1f, 0.1f, 0.5f), position, new Vector2(200, 250));
        var scrollRect = scrollRectGO.AddComponent<ScrollRect>();
        
        var viewport = CreatePanel("Viewport", Color.clear, Vector2.zero, new Vector2(200, 250));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.transform.SetParent(scrollRect.transform, false);

        var content = CreatePanel("Content", Color.clear, Vector2.zero, new Vector2(200, 0));
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.padding = new RectOffset(5,5,5,5);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        content.transform.SetParent(viewport.transform, false);
        
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = content.GetComponent<RectTransform>();

        var scrollList = scrollRectGO.AddComponent<UIScrollList>();
        var templateGO = CreateButton(name + "_Template", "Template", Vector2.zero);
        templateGO.transform.SetParent(content.transform, false);
        templateGO.SetActive(false);
        var templateWidget = templateGO.GetComponent<UIWidget>();
        
        SetProtectedField(scrollList, "_itemTemplate", templateWidget);
        SetProtectedField(scrollList, "_contentContainer", content.GetComponent<RectTransform>());
        
        return scrollList;
    }
    
    private void CreateBaseUI()
    {
        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        
        if (GetCanvas() == null)
        {
            var canvasGO = new GameObject("DemoCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        if (FindObjectOfType<UIWidgetManager>() == null) new GameObject("UIWidgetManager", typeof(UIWidgetManager));
        if (FindObjectOfType<UIInputHandler>() == null) new GameObject("UIInputHandler", typeof(UIInputHandler));
        if (FindObjectOfType<UIOverlayManager>() == null) new GameObject("UIOverlayManager_AutoCreated", typeof(UIOverlayManager));
        
        Debug.Log("Basis-UI und Manager wurden erfolgreich in der Szene erstellt oder waren bereits vorhanden.");
    }
    
    private UIWidget CreateSimpleWidget(string name, string text, Color color)
    {
        var panel = CreatePanel(name, color, Vector2.zero, new Vector2(200, 80));
        CreateText(name + "_Text", text, panel.transform).color = Color.white;
        return panel.GetComponent<UIWidget>();
    }

    private GameObject CreateButton(string name, string text, Vector2 position, Vector2 size = default)
    {
        if (size == default) size = new Vector2(180, 30);
        var buttonPanel = CreatePanel(name, new Color(0.3f, 0.3f, 0.3f), position, size);
        buttonPanel.AddComponent<UIButton>();
        CreateText(name + "_Text", text, buttonPanel.transform, 16).color = Color.white;
        return buttonPanel;
    }
    
    private GameObject CreatePanel(string name, Color color, Vector2 position, Vector2 size)
    {
        var canvas = GetCanvas();
        if (canvas == null) {
            Debug.LogError("Fehler: Kein Canvas. Bitte 'Basis-UI erstellen' klicken.");
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
        widget.SetWidgetName(name);
        widget.SetTargetGraphic(image);
        return go;
    }

    private TextMeshProUGUI CreateText(string name, string content, Transform parent, int fontSize = 20)
    {
        var go = new GameObject(name, typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.color = Color.black;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        var rt = text.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        return text;
    }

    private Canvas GetCanvas() => FindObjectOfType<Canvas>();

    private Sprite GetPlaceholderSprite(bool circle = false)
    {
        var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        var center = new Vector2(31.5f, 31.5f);
        for(int y=0; y<tex.height; y++) {
            for(int x=0; x<tex.width; x++) {
                if (circle) {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, dist < 32 ? Color.white : Color.clear);
                } else {
                    tex.SetPixel(x, y, Color.white);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,64,64), Vector2.one * 0.5f);
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        obj?.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(obj, value);
    }
    
    private void SetProtectedField(object obj, string fieldName, object value)
    {
        obj?.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)?.SetValue(obj, value);
    }
    
    private void StartCoroutineInEditor(IEnumerator coroutine)
    {
        EditorApplication.CallbackFunction update = null;
        update = () => {
            if (!coroutine.MoveNext()) EditorApplication.update -= update;
        };
        EditorApplication.update += update;
    }
    
    public class EditorWaitForSeconds : CustomYieldInstruction
    {
        private readonly float _endTime;
        public override bool keepWaiting => EditorApplication.timeSinceStartup < _endTime;
        public EditorWaitForSeconds(float seconds) => _endTime = (float)EditorApplication.timeSinceStartup + seconds;
    }
    
    #endregion
}