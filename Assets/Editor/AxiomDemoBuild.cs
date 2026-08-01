using System;
using Axiom.Demo;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Axiom.Editor
{
    public static class AxiomDemoBuild
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string ScenePath = SceneFolder + "/AxiomDemo.unity";
        private const string WebOutputPath = "docs";

        [MenuItem("Axiom/Demo/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            var bootstrap = new GameObject("Axiom Demo Bootstrap");
            bootstrap.AddComponent<DemoArenaBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log($"Axiom demo scene created: {ScenePath}");
        }

        [MenuItem("Axiom/Demo/Build WebGL")]
        public static void BuildWebGL()
        {
            CreateDemoScene();
            PlayerSettings.companyName = "Axiom Team";
            PlayerSettings.productName = "Axiom";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = WebOutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"WebGL build failed: {report.summary.result}, " +
                    $"{report.summary.totalErrors} errors");
            }

            Debug.Log(
                $"Axiom WebGL build completed: {WebOutputPath}, " +
                $"{report.summary.totalSize} bytes");
        }
    }
}
