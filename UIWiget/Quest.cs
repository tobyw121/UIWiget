// Dateiname: Quest.cs
using System.Collections.Generic;
using UnityEngine;

namespace YourGame.Quests
{
    // Diese Klasse enthält die Daten für ein einzelnes Quest-Ziel.
    [System.Serializable]
    public class Objective
    {
        public string Description;      // z.B. "Sammle Fische"
        public int RequiredAmount;     // z.B. 10
        public int CurrentProgress;    // z.B. 5
    }

    // Dies ist die Haupt-Datenklasse für eine Quest.
    [CreateAssetMenu(fileName = "NewQuest", menuName = "YourGame/Quest")]
    public class Quest : ScriptableObject
    {
        public string Title;            // Der Name der Quest
        public bool IsComplete;         // Ist die Quest abgeschlossen?
        public List<Objective> Objectives = new List<Objective>(); // Die Liste der Ziele
    }
}