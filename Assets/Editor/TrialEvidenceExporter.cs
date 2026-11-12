using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VrProf
{
    public static class TrialEvidenceExporter
    {
        [MenuItem("Tools/VrProf/Экспортировать проверочные журналы")]
        public static void Export()
        {
            PcTrialSceneBuilder.CheckAlgorithm();
            PcTrialSceneBuilder.ValidateScene();
            string root = Directory.GetParent(Application.dataPath).FullName;
            string examples = Path.Combine(root, "JavaAnalyzer", "examples");
            Directory.CreateDirectory(examples);
            ExportOne(examples, "completed", true);
            ExportOne(examples, "incomplete", false);
            Debug.Log("Проверены контроллер и сохранение: журналы записаны в JavaAnalyzer/examples. Это вызов компонента в Editor, не ручной VR-проход.");
        }

        private static void ExportOne(string directory, string name, bool complete)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/PcTrial.unity", OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindFirstObjectByType<PcTrialController>();
            if (controller == null) throw new Exception("Нет контроллера.");
            controller.PupilCode = complete ? "S001" : "S002";
            Invoke(controller, "Awake");
            Set(controller, "attemptId", "demo-" + name);
            Set(controller, "studentFeedback", complete ? "Сначала выбрал не тот модуль; работа интересна." : "Пока сложно найти нужную деталь.");
            Set(controller, "mentorFeedback", complete ? "После подсказки задание завершено." : "Нужно повторить выбор модуля.");
            Set(controller, "difficulty", complete ? 2 : 4);
            Set(controller, "interest", complete ? 4 : 3);
            TrialStep[] steps = TrialLogic.CreateRamReplacementSteps();
            for (int i = 0; i < 4; i++) Invoke(controller, "Act", steps[i].Code, steps[i].ObjectId);
            if (complete) Invoke(controller, "Hint");
            Invoke(controller, "Act", "SELECT_FAULTY", "ram_slot_a");
            if (complete)
                for (int i = 4; i < steps.Length; i++) Invoke(controller, "Act", steps[i].Code, steps[i].ObjectId);
            Invoke(controller, "SaveAttempt");
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var result = (TrialResult)typeof(PcTrialController).GetField("result", flags).GetValue(controller);
            if (result.Completed != complete || result.Errors != 1 || result.CompletedSteps != (complete ? 9 : 4))
                throw new Exception("Результат контроллера неверен.");
            string saved = (string)typeof(PcTrialController).GetField("savedPath", flags).GetValue(controller);
            if (string.IsNullOrEmpty(saved) || !File.Exists(saved)) throw new Exception("Журнал не сохранён.");
            File.Copy(saved, Path.Combine(directory, name + ".json"), true);
            if (complete && (!controller.CaseLid.activeSelf || controller.FaultyRam.activeSelf
                || Vector3.Distance(controller.SpareRam.transform.position, new Vector3(1.1f, 1.35f, 0.1f)) > 0.01f))
                throw new Exception("Состояние объектов не совпало с результатом.");
            Debug.Log(name + ": " + result.CompletedSteps + "/9; hints=" + result.Hints + "; errors=" + result.Errors + "; saved=" + saved);
        }
        private static void Invoke(PcTrialController controller, string method, params object[] args)
        {
            typeof(PcTrialController).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, args);
        }
        private static void Set(PcTrialController controller, string field, object value)
        {
            typeof(PcTrialController).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controller, value);
        }
    }
}
