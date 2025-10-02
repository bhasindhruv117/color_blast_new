using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript : IPreprocessBuildWithReport
{
    public int callbackOrder
    {
        get { return 0; }
    }
    public void OnPreprocessBuild(BuildReport report)
    {
        SetAndroidPlayerSettings();
    }

    private void SetAndroidPlayerSettings()
    {
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = "release.keystore";
        PlayerSettings.Android.keystorePass = "G@mebr0s";
        PlayerSettings.Android.keyaliasName = "block_connect";
        PlayerSettings.Android.keyaliasPass = "ShiromaniAkaliDal";
    }
}
