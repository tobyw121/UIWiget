# Vorschläge für Verbesserungen

## UIWidget
- **Single Responsibility**: Die Klasse bündelt Animation, Lokalisierung, Thematisierung, Eingabeverarbeitung und Async-Handling. Ziehen Sie in Erwägung, Helper-Komponenten für Animation (`UIWidgetAnimator`), Lokalisierung (`UILocalizationBehaviour`) und Async-Interaktionen zu extrahieren, um die Testbarkeit zu verbessern und `MonoBehaviour`-Lebenszyklen zu entflechten. 【F:UIWiget/UIWidget.cs†L14-L142】
- **Enum-Flags kombinieren**: Für `VisualTransition` wäre ein konfigurierbarer Transition-Pipeline-Ansatz sinnvoller, der konkrete Strategy-Klassen (z.B. `IFadeTransition`, `IScaleTransition`) nutzt statt eines großen `switch`-Blocks. Das reduziert Feature-Flags und erleichtert Erweiterungen. 【F:UIWiget/UIWidget.cs†L43-L95】
- **Tooltip-Auflösung**: Die Methode `GetResolvedTooltipText` enthält direkten Zugriff auf `UIThemeManager` und `LanguageManager`. Kapseln Sie dies in Services, um UI-Logik von globalen Singletons zu trennen und Mocking zu ermöglichen. 【F:UIWiget/UIWidget.cs†L109-L139】
- **Async-Handling**: `HandleAsyncAction` startet eine Coroutine, wartet auf `Task`, prüft aber Fehler nur implizit. Führen Sie ein Logging bzw. einen Error-Callback ein und erlauben Sie `CancellationToken`, damit lange Operationen abgebrochen werden können. 【F:UIWiget/UIWidget.cs†L141-L160】

## UIWidgetManager
- **Thread-Safety**: Obwohl `lock` genutzt wird, greifen Unity-APIs (`FindObjectOfType`) auf dem Main Thread. Vereinfachen Sie den Singleton-Zugriff und entfernen Sie den Lock, um Deadlocks beim Domain-Reload zu verhindern. Alternativ: `Lazy<UIWidgetManager>` ohne direkten Unity-Call im Lock. 【F:UIWiget/UIWidgetManager.cs†L1-L61】
- **Scene-Unload Cleanup**: Bei `PopulateWidgetCache` werden alle Widgets neu gesammelt. Erwägen Sie `OnSceneLoaded` gezielt zu ergänzen statt komplettes Clear, um Garbage zu reduzieren und `DontDestroyOnLoad`-Widgets zu behalten. 【F:UIWiget/UIWidgetManager.cs†L63-L105】
- **Cursor-API**: Der Manager hält Cursor-Status als statische Strings. Ersetzen Sie diese durch ScriptableObject-Konfigurationen oder ein Interface (`ICursorProvider`) für bessere Integrationen mit Plattform-spezifischen Implementierungen. 【F:UIWiget/UIWidgetManager.cs†L107-L153】

## Lokalisierung & Themen
- **Dependency Inversion**: `UIWidget` hört direkt auf `LanguageManager.Instance` und `UIThemeManager.OnThemeChanged`. Eine Event-Bus- oder Observer-Abstraktion erlaubt es, Widgets in isolierten Tests ohne Singletons zu betreiben. 【F:UIWiget/UIWidget.cs†L77-L108】
- **Batch-Updates**: Bei vielen Widgets führt jeder Sprachwechsel zu `UpdateLocalizedText`-Aufrufen. Implementieren Sie einen `LocalizationBatchUpdate`, der DOM-Updates bündelt. Gleiches gilt für Theme-Wechsel. 【F:UIWiget/UIWidget.cs†L77-L108】

## Tooling & Tests
- **Play Mode Tests**: Ergänzen Sie Unity Play Mode Tests, die die Lebenszyklus-Methoden (`Awake`, `OnEnable`, `OnDisable`) für Widgets verifizieren. So lassen sich Regressionsfehler in der Registrierung schneller erkennen.
- **Editor Utilities**: Ein Custom-Editor zur Validierung der `styleKey`-Referenzen könnte Inkonsistenzen aufdecken, bevor sie zur Laufzeit auftreten.

## Performance
- **Pooling**: Für häufig geöffnete Panels (z.B. Notifications) lohnt sich ein UI-Pooling-System statt ständiger Instanziierung, um GC-Spitzen zu vermeiden.
- **Async Awaitables**: Prüfen Sie, ob `UniTask` (o.ä.) besser zu den Coroutine/Task-Mix passt, um Overhead zu reduzieren und den Code konsistenter zu gestalten.
