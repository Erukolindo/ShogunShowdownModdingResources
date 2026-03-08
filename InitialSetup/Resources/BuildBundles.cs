using UnityEditor;
#if UNITY_EDITOR
public class BuildBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAll()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
}
#endif