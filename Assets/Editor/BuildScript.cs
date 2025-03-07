using UnityEditor;

public static class BuildScript
{
    public static void PerformBuild()
    {
        string[] scenes = { "Assets/Scenes/MainScene.unity" };
        string buildPath = "Build/WebGL";  // Adjust if you want a different folder name

        BuildPipeline.BuildPlayer(
            scenes,
            buildPath,
            BuildTarget.WebGL,
            BuildOptions.None
        );
    }
}
