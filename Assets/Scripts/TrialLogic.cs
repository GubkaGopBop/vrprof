using System;
using System.Collections.Generic;
using System.Text;

namespace VrProf
{
    [Serializable]
    public sealed class TrialStep
    {
        public string Code;
        public string ObjectId;

        public TrialStep(string code, string objectId)
        {
            Code = code;
            ObjectId = objectId;
        }
    }

    [Serializable]
    public sealed class TrialEvent
    {
        public int Number;
        public string Kind;
        public string StepCode;
        public string ObjectId;
        public bool Success;

        public TrialEvent(int number, string kind, string stepCode = "", string objectId = "", bool success = true)
        {
            Number = number;
            Kind = kind;
            StepCode = stepCode;
            ObjectId = objectId;
            Success = success;
        }
    }

    public sealed class TrialResult
    {
        public int CompletedSteps;
        public int TotalSteps;
        public int Hints;
        public int Errors;
        public readonly List<string> ErrorDetails = new List<string>();
        public bool Completed { get { return CompletedSteps == TotalSteps; } }
    }

    public static class TrialLogic
    {
        public static TrialStep[] CreateRamReplacementSteps()
        {
            return new[]
            {
                new TrialStep("READ_REPORT", "diagnostic_report"),
                new TrialStep("POWER_OFF", "pc"),
                new TrialStep("UNPLUG", "power_cable"),
                new TrialStep("OPEN_CASE", "case"),
                new TrialStep("SELECT_FAULTY", "ram_slot_b"),
                new TrialStep("REMOVE_RAM", "ram_slot_b"),
                new TrialStep("INSTALL_RAM", "ram_slot_b"),
                new TrialStep("CLOSE_CASE", "case"),
                new TrialStep("RUN_TEST", "memory_test")
            };
        }

        // Один проход по событиям. Верный шаг продвигает пробу, ошибка сохраняется в журнале.
        public static TrialResult Evaluate(IReadOnlyList<TrialStep> steps, IReadOnlyList<TrialEvent> events)
        {
            if (steps == null || steps.Count == 0)
                throw new ArgumentException("В задании должен быть хотя бы один шаг.", nameof(steps));
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            var result = new TrialResult { TotalSteps = steps.Count };
            for (int i = 0; i < events.Count; i++)
            {
                TrialEvent item = events[i];
                if (item == null || item.Number != i + 1)
                    throw new ArgumentException("Номера событий должны идти подряд, начиная с 1.", nameof(events));
                if (item.Kind == "hint")
                {
                    result.Hints++;
                    continue;
                }
                if (item.Kind != "action")
                    throw new ArgumentException("Неизвестный тип события: " + item.Kind, nameof(events));

                bool matches = result.CompletedSteps < steps.Count
                    && item.StepCode == steps[result.CompletedSteps].Code
                    && item.ObjectId == steps[result.CompletedSteps].ObjectId;
                if (matches && item.Success)
                    result.CompletedSteps++;
                else
                {
                    result.Errors++;
                    result.ErrorDetails.Add("Событие " + item.Number + ": действие " + item.StepCode
                        + " с объектом " + item.ObjectId + " не завершило ожидаемый шаг.");
                }
            }
            return result;
        }

        // Сводка помогает наставнику обсудить работу. Она не оценивает пригодность к профессии.
        public static string BuildSummary(string title, string pupilCode, TrialResult result,
            int difficulty, int interest, string studentFeedback, string mentorFeedback)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (difficulty < 1 || difficulty > 5 || interest < 1 || interest > 5)
                throw new ArgumentOutOfRangeException(nameof(difficulty), "Оценки должны быть от 1 до 5.");

            var text = new StringBuilder();
            text.AppendLine("Проба: " + title + ".");
            text.AppendLine("Код школьника: " + pupilCode + ".");
            text.AppendLine("Выполнено шагов: " + result.CompletedSteps + " из " + result.TotalSteps + ".");
            text.AppendLine("Подсказок: " + result.Hints + "; ошибок: " + result.Errors + ".");
            text.AppendLine("Сложность: " + difficulty + "/5; интерес: " + interest + "/5.");
            text.AppendLine("Самооценка школьника: " + studentFeedback);
            text.AppendLine("Комментарий наставника: " + mentorFeedback);
            foreach (string error in result.ErrorDetails)
                text.AppendLine(error);
            return text.ToString().TrimEnd();
        }
    }
}
