using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public UnityEvent OnPanelOpened;
    public UnityEvent OnPanelClosed;

    [Header("UI Panel References")]
    [Tooltip("Das Haupt-Panel des Spieler-Inventars")]
    public GameObject playerInventoryPanel;
    [Tooltip("Das Panel für das Inventar des Begleiters")]
    public GameObject petInventoryPanel;
    [Tooltip("Das Panel für Dialoge mit NPCs")]
    public GameObject dialoguePanel;
    [Tooltip("Das Haupt-Panel des Shops")]
    public GameObject shopPanel;
    [Tooltip("Das Panel für die Quest-Interaktion mit NPCs")]
    public GameObject questInteractionPanel;
    [Tooltip("Das Panel für das Quest-Log des Spielers")]
    public GameObject questLogPanel;
    public GameObject ActionProgressBarUI;
    public GameObject PlayerHUD;

    [Header("Panel Behavior")]
    [Tooltip("Panels in dieser Liste werden die Spielersteuerung NICHT deaktivieren, wenn sie geöffnet werden.")]
    public List<GameObject> nonBlockingPanels = new List<GameObject>();
    
    private GameObject _currentlyOpenPanel;
    private InputManager _inputManager;
    private PlayerController _playerController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnPanelOpened = new UnityEvent();
            OnPanelClosed = new UnityEvent();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            _inputManager = GameManager.Instance.inputManager;
            if (GameManager.Instance.playerLoaded && GameManager.Instance.playerControllerInstance != null)
            {
                _playerController = GameManager.Instance.playerControllerInstance;
            }
        }
        else
        {
            _inputManager = FindObjectOfType<InputManager>();
            _playerController = FindObjectOfType<PlayerController>();
        }

        if (_inputManager == null)
        {
            Debug.LogError("[UIManager] InputManager nicht in der Szene gefunden!");
        }

        if (questLogPanel != null && !nonBlockingPanels.Contains(questLogPanel))
        {
            nonBlockingPanels.Add(questLogPanel);
        }

        playerInventoryPanel?.SetActive(false);
        petInventoryPanel?.SetActive(false);
        dialoguePanel?.SetActive(false);
        shopPanel?.SetActive(false);
        questInteractionPanel?.SetActive(false);
        questLogPanel?.SetActive(false);
        ActionProgressBarUI?.SetActive(false);

    }

    private void Update()
    {
        HandleCloseOnMove();
    }
    
    public void OpenPanel(GameObject panelToOpen)
    {
        if (panelToOpen == null)
        {
            Debug.LogError("[UIManager] Versuch, ein NULL-Panel zu öffnen.");
            return;
        }

        if (_playerController == null && GameManager.Instance != null && GameManager.Instance.playerLoaded)
        {
            _playerController = GameManager.Instance.playerControllerInstance;
        }
        
        if (_currentlyOpenPanel != null && _currentlyOpenPanel != panelToOpen)
        {
            _currentlyOpenPanel.SetActive(false);
        }

        panelToOpen.SetActive(true);
        _currentlyOpenPanel = panelToOpen;

        if (!nonBlockingPanels.Contains(panelToOpen))
        {
            if (GameManager.Instance != null && GameManager.Instance.playerLoaded)
            {
                GameManager.Instance.SetPlayerControlActive(false);
                ResetPlayerAnimationsToIdle();
            }
        }

        OnPanelOpened?.Invoke();
    }

    public void CloseCurrentPanel()
    {
        if (_currentlyOpenPanel != null)
        {
            GameObject panelToClose = _currentlyOpenPanel;
            panelToClose.SetActive(false);
            _currentlyOpenPanel = null;

            if (!nonBlockingPanels.Contains(panelToClose))
            {
                if (GameManager.Instance != null && GameManager.Instance.playerLoaded)
                {
                    GameManager.Instance.SetPlayerControlActive(true);
                }
            }

            OnPanelClosed?.Invoke();
        }
    }
    
    public bool IsPanelOpen(GameObject panel)
    {
        return _currentlyOpenPanel != null && _currentlyOpenPanel == panel;
    }

    public void OpenPetInventoryPanel(PetInventory petInventoryComponent)
    {
        if (petInventoryPanel == null)
        {
            Debug.LogError("[UIManager] Das 'petInventoryPanel' ist im UIManager nicht zugewiesen!");
            return;
        }

        if (petInventoryComponent == null)
        {
            Debug.LogError("[UIManager] OpenPetInventoryPanel: petInventoryComponent ist null.");
            return;
        }
        
        OpenPanel(petInventoryPanel);

        PetInventoryUI petUI = petInventoryPanel.GetComponent<PetInventoryUI>();
        if (petUI != null)
        {
            petUI.OpenInventory(petInventoryComponent);
        }
        else
        {
            Debug.LogError("[UIManager] Das zugewiesene 'petInventoryPanel' hat keine 'PetInventoryUI'-Komponente!");
        }
    }

    public void OpenDialoguePanel(DialogueSO dialogue, NPCInteraction npc)
    {
        if (dialoguePanel == null)
        {
            Debug.LogError("[UIManager] Das 'dialoguePanel' ist im UIManager nicht zugewiesen!");
            return;
        }

        if (dialogue == null || npc == null)
        {
            Debug.LogError("[UIManager] OpenDialoguePanel: dialogue oder npc ist null.");
            return;
        }
        
        OpenPanel(dialoguePanel);
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue, npc);
        }
        else
        {
            Debug.LogError("[UIManager] DialogueManager.Instance nicht gefunden. Dialog kann nicht gestartet werden.");
        }
    }
    
    public void OpenQuestInteractionPanelForNPC(NPCInteraction npc, PlayerController player)
    {
        if (questInteractionPanel == null)
        {
            Debug.LogError("[UIManager] Das 'questInteractionPanel' ist im UIManager nicht zugewiesen!");
            return;
        }

        if (npc == null || player == null)
        {
            Debug.LogError("[UIManager] OpenQuestInteractionPanelForNPC: NPC oder Player ist null.");
            return;
        }
        
        OpenPanel(questInteractionPanel);
        
        QuestInteractionUI questUI = questInteractionPanel.GetComponent<QuestInteractionUI>();
        if (questUI != null)
        {
            questUI.InitializePanel(npc, player);
        }
        else
        {
            Debug.LogError("[UIManager] Das zugewiesene 'questInteractionPanel' hat keine 'QuestInteractionUI'-Komponente!");
        }
    }
    
    private void HandleCloseOnMove()
    {
        if (_currentlyOpenPanel == null || nonBlockingPanels.Contains(_currentlyOpenPanel))
        {
            return;
        }

        if (_inputManager != null &&
           (Input.GetKey(_inputManager.forwardKey) ||
            Input.GetKey(_inputManager.backwardKey) ||
            Input.GetKey(_inputManager.leftKey) ||
            Input.GetKey(_inputManager.rightKey)))
        {
            CloseCurrentPanel();
        }
    }

    private void ResetPlayerAnimationsToIdle()
    {
        if (_playerController == null) return;

        Animator animator = _playerController.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MotionSpeed", 0f);
        }
    }
}