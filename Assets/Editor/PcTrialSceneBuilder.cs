using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VrProf
{
    public static class PcTrialSceneBuilder
    {
        [MenuItem("Tools/VrProf/Создать сцену пробы")]
        public static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.16f, 0.23f);
            cameraObject.transform.position = new Vector3(2.1f, 3.2f, -8.5f);
            cameraObject.transform.LookAt(new Vector3(2.0f, 0.8f, 0));
            cameraObject.AddComponent<AudioListener>();

            var lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.6f;
            lightObject.transform.rotation = Quaternion.Euler(45, -30, 0);

            Box("Стол", new Vector3(1.8f, -0.15f, 0), new Vector3(7.5f, 0.25f, 4), new Color(0.35f, 0.25f, 0.16f));
            Box("Корпус ПК", new Vector3(1.2f, 0.7f, 0), new Vector3(2.5f, 1.7f, 2), new Color(0.13f, 0.15f, 0.18f));
            GameObject caseLid = Box("Съёмная крышка корпуса", new Vector3(1.2f, 1.58f, 0),
                new Vector3(2.7f, 0.12f, 2.1f), new Color(0.32f, 0.36f, 0.42f));
            Box("Материнская плата", new Vector3(1.1f, 1.35f, 0), new Vector3(1.6f, 0.08f, 1.3f),
                new Color(0.07f, 0.32f, 0.19f));
            Box("Модуль RAM в слоте A", new Vector3(0.75f, 1.43f, 0.1f),
                new Vector3(0.22f, 0.12f, 1.0f), new Color(0.1f, 0.55f, 0.25f));
            GameObject faultyRam = Box("Неисправный RAM в слоте B", new Vector3(1.1f, 1.43f, 0.1f),
                new Vector3(0.22f, 0.12f, 1.0f), new Color(0.65f, 0.18f, 0.16f));
            GameObject spareRam = Box("Исправный запасной RAM", new Vector3(3.6f, 0.3f, 0.1f),
                new Vector3(0.22f, 0.12f, 1.0f), new Color(0.1f, 0.55f, 0.25f));

            var controllerObject = new GameObject("Контроллер пробы");
            PcTrialController controller = controllerObject.AddComponent<PcTrialController>();
            controller.CaseLid = caseLid;
            controller.FaultyRam = faultyRam;
            controller.SpareRam = spareRam;

            const string path = "Assets/Scenes/PcTrial.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            Selection.activeGameObject = controllerObject;
            Debug.Log("Создана сцена " + path + ". Запустите Play Mode для демонстрации сценария.");
        }

        [MenuItem("Tools/VrProf/Проверить алгоритм")]
        public static void CheckAlgorithm()
        {
            TrialStep[] steps = TrialLogic.CreateRamReplacementSteps();
            var completed = new List<TrialEvent>();
            for (int i = 0; i < steps.Length; i++)
                completed.Add(new TrialEvent(i + 1, "action", steps[i].Code, steps[i].ObjectId));
            completed.Insert(4, new TrialEvent(5, "hint"));
            for (int i = 0; i < completed.Count; i++)
                completed[i].Number = i + 1;
            completed.Insert(5, new TrialEvent(6, "action", "SELECT_FAULTY", "ram_slot_a"));
            for (int i = 0; i < completed.Count; i++)
                completed[i].Number = i + 1;
            TrialResult first = TrialLogic.Evaluate(steps, completed);
            if (!first.Completed || first.Hints != 1 || first.Errors != 1)
                throw new Exception("Проверка завершённой попытки не прошла.");

            var incomplete = new List<TrialEvent>
            {
                new TrialEvent(1, "action", "READ_REPORT", "diagnostic_report"),
                new TrialEvent(2, "action", "POWER_OFF", "pc"),
                new TrialEvent(3, "action", "UNPLUG", "power_cable"),
                new TrialEvent(4, "action", "OPEN_CASE", "case"),
                new TrialEvent(5, "action", "SELECT_FAULTY", "ram_slot_a")
            };
            TrialResult second = TrialLogic.Evaluate(steps, incomplete);
            if (second.Completed || second.CompletedSteps != 4 || second.Errors != 1)
                throw new Exception("Проверка незавершённой попытки не прошла.");

            string summary = TrialLogic.BuildSummary("Замена модуля оперативной памяти", "S001",
                first, 2, 4, "Работа интересна.", "После подсказки задание завершено.");
            if (!summary.Contains("Выполнено шагов: 9 из 9.")
                || !summary.Contains("Подсказок: 1; ошибок: 1."))
                throw new Exception("Проверка сводки не прошла.");

            Debug.Log("Пример 1: 9/9 шагов, 1 подсказка, 1 ошибка. Пример 2: 4/9 шагов, 0 подсказок, 1 ошибка.");
            Debug.Log("Проверка сводки: выполненные шаги, подсказки и ошибки совпадают с исходными данными.");
        }

        [MenuItem("Tools/VrProf/Проверить сцену")]
        public static void ValidateScene()
        {
            const string path = "Assets/Scenes/PcTrial.unity";
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PcTrialController controller = UnityEngine.Object.FindFirstObjectByType<PcTrialController>();
            if (controller == null || controller.CaseLid == null || controller.FaultyRam == null || controller.SpareRam == null)
                throw new Exception("В сцене отсутствует контроллер или один из основных объектов.");
            if (EditorBuildSettings.scenes.Length != 1 || EditorBuildSettings.scenes[0].path != path)
                throw new Exception("Сцена пробы не выбрана для сборки.");
            Debug.Log("Сцена пробы проверена: контроллер, корпус, неисправный и запасной модули найдены.");
        }

        private static GameObject Box(string name, Vector3 position, Vector3 size, Color color)
        {
            GameObject value = GameObject.CreatePrimitive(PrimitiveType.Cube);
            value.name = name;
            value.transform.position = position;
            value.transform.localScale = size;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            var material = new Material(shader);
            material.color = color;
            value.GetComponent<Renderer>().sharedMaterial = material;
            return value;
        }
    }
}
