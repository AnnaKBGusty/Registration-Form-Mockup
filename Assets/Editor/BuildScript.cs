using UnityEditor;

public static class BuildScript
{
    public static void PerformBuild()
    {
        string[] scenes = { "Assets/Scenes/MainScene.unity" };
        string buildPath = "Build/WebGL";  

        BuildPipeline.BuildPlayer(
            scenes,
            buildPath,
            BuildTarget.WebGL,
            BuildOptions.None
        );
    }
}
