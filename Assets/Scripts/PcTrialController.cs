using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VrProf
{
    // Демонстрационный ввод в Unity. Позже кнопки можно заменить действиями XR-контроллеров.
    public sealed class PcTrialController : MonoBehaviour
    {
        [Serializable]
        private sealed class SavedAttempt
        {
            public string ExerciseId = "ram-replacement-v1";
            public string PupilCode = "S001";
            public List<TrialEvent> Events = new List<TrialEvent>();
            public int Difficulty;
            public int Interest;
            public string StudentFeedback;
            public string MentorFeedback;
            public string Summary;
        }

        public GameObject CaseLid;
        public GameObject FaultyRam;
        public GameObject SpareRam;

        private readonly TrialStep[] steps = TrialLogic.CreateRamReplacementSteps();
        private readonly List<TrialEvent> events = new List<TrialEvent>();
        private TrialResult result;
        private string studentFeedback = "";
        private string mentorFeedback = "";
        private int difficulty = 3;
        private int interest = 3;
        private string message = "Прочитайте заявку и результат диагностики.";
        private string savedPath = "";

        private void Awake()
        {
            result = TrialLogic.Evaluate(steps, events);
            UpdateObjects();
        }

        private void OnGUI()
        {
            GUI.skin.label.wordWrap = true;
            GUILayout.BeginArea(new Rect(20, 20, 420, Screen.height - 40), GUI.skin.box);
            GUILayout.Label("ПРОФЕССИОНАЛЬНАЯ ПРОБА: ОБСЛУЖИВАНИЕ ПК");
            GUILayout.Label("Заявка: диагностика указала на неисправность модуля в слоте B. Замените его и проверьте память повторно.");
            GUILayout.Label("Шагов: " + result.CompletedSteps + "/" + result.TotalSteps
                + "    Подсказок: " + result.Hints + "    Ошибок: " + result.Errors);
            GUILayout.Label(message);

            if (GUILayout.Button("1. Прочитать результат диагностики"))
                Act("READ_REPORT", "diagnostic_report");
            if (GUILayout.Button("2. Выключить ПК"))
                Act("POWER_OFF", "pc");
            if (GUILayout.Button("3. Отсоединить кабель питания"))
                Act("UNPLUG", "power_cable");
            if (GUILayout.Button("4. Открыть корпус"))
                Act("OPEN_CASE", "case");
            if (GUILayout.Button("5. Выбрать модуль в слоте A"))
                Act("SELECT_FAULTY", "ram_slot_a");
            if (GUILayout.Button("5. Выбрать модуль в слоте B"))
                Act("SELECT_FAULTY", "ram_slot_b");
            if (GUILayout.Button("6. Извлечь модуль из слота B"))
                Act("REMOVE_RAM", "ram_slot_b");
            if (GUILayout.Button("7. Установить исправный модуль"))
                Act("INSTALL_RAM", "ram_slot_b");
            if (GUILayout.Button("8. Закрыть корпус"))
                Act("CLOSE_CASE", "case");
            if (GUILayout.Button("9. Повторить проверку памяти"))
                Act("RUN_TEST", "memory_test");
            if (GUILayout.Button("Запросить подсказку"))
                Hint();

            GUILayout.Label("Школьник: что было трудно и интересно?");
            studentFeedback = GUILayout.TextField(studentFeedback);
            GUILayout.Label("Сложность: " + difficulty + "/5   Интерес: " + interest + "/5");
            difficulty = Mathf.RoundToInt(GUILayout.HorizontalSlider(difficulty, 1, 5));
            interest = Mathf.RoundToInt(GUILayout.HorizontalSlider(interest, 1, 5));
            GUILayout.Label("Наставник: замечание по выполнению");
            mentorFeedback = GUILayout.TextField(mentorFeedback);
            if (GUILayout.Button("Сохранить журнал и сводку"))
                SaveAttempt();
            if (!string.IsNullOrEmpty(savedPath))
                GUILayout.Label("Файл: " + savedPath);
            GUILayout.EndArea();
        }

        private void Act(string code, string objectId)
        {
            int previousSteps = result.CompletedSteps;
            events.Add(new TrialEvent(events.Count + 1, "action", code, objectId));
            result = TrialLogic.Evaluate(steps, events);
            message = result.CompletedSteps > previousSteps
                ? "Верный шаг выполнен. " + (result.Completed ? "Проба завершена." : "Продолжайте работу.")
                : "Это действие пока неверно или нарушает порядок шагов.";
            UpdateObjects();
        }

        private void Hint()
        {
            events.Add(new TrialEvent(events.Count + 1, "hint"));
            result = TrialLogic.Evaluate(steps, events);
            message = result.Completed ? "Все шаги выполнены."
                : "Следующий шаг: " + steps[result.CompletedSteps].Code + ".";
        }

        private void UpdateObjects()
        {
            if (CaseLid != null)
                CaseLid.SetActive(result.CompletedSteps < 4 || result.CompletedSteps >= 8);
            if (FaultyRam != null)
                FaultyRam.SetActive(result.CompletedSteps < 6);
            if (SpareRam != null)
                SpareRam.transform.position = result.CompletedSteps >= 7
                    ? new Vector3(1.1f, 1.35f, 0.1f) : new Vector3(3.6f, 0.3f, 0.1f);
        }

        private void SaveAttempt()
        {
            string summary = TrialLogic.BuildSummary("Замена модуля оперативной памяти", "S001",
                result, difficulty, interest, studentFeedback, mentorFeedback);
            var saved = new SavedAttempt
            {
                Events = new List<TrialEvent>(events),
                Difficulty = difficulty,
                Interest = interest,
                StudentFeedback = studentFeedback,
                MentorFeedback = mentorFeedback,
                Summary = summary
            };
            savedPath = Path.Combine(Application.persistentDataPath, "pc-trial-attempt.json");
            try
            {
                File.WriteAllText(savedPath, JsonUtility.ToJson(saved, true));
                message = "Журнал сохранён для обсуждения с наставником.";
                Debug.Log(summary);
            }
            catch (IOException error)
            {
                savedPath = "";
                message = "Не удалось сохранить журнал: " + error.Message;
            }
        }
    }
}
