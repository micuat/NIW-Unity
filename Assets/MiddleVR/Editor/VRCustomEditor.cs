/* VRCustomEditor
 * MiddleVR
 * (c) MiddleVR
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using MiddleVR_Unity3D;
using UnityEditor.Callbacks;

[CustomEditor(typeof(VRManagerScript))]
public class VRCustomEditor : Editor
{
    //This will just be a shortcut to the target, ex: the object you clicked on.
    private VRManagerScript mgr;

    static private bool m_SettingsApplied = false;

    void Awake()
    {
        mgr = (VRManagerScript)target;

        if( !m_SettingsApplied )
        {
            ApplyVRSettings();
            m_SettingsApplied = true;
        }
    }

    void Start()
    {
        Debug.Log("MT: " + PlayerSettings.MTRendering);
    }

    public void ApplyVRSettings()
    {
        PlayerSettings.defaultIsFullScreen = false;
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
        PlayerSettings.runInBackground = true;
        PlayerSettings.captureSingleScreen = false;
        PlayerSettings.MTRendering = false;
        //PlayerSettings.usePlayerLog = false;
        //PlayerSettings.useDirect3D11 = false;

        MVRTools.Log("VR Player settings changed:");
        MVRTools.Log("- DefaultIsFullScreen = false");
        MVRTools.Log("- DisplayResolutionDialog = Disabled");
        MVRTools.Log("- RunInBackground = true");
        MVRTools.Log("- CaptureSingleScreen = false");
        //MVRTools.Log("- UsePlayerLog = false");

        string[] names = QualitySettings.names;
        int qualityLevel = QualitySettings.GetQualityLevel();

        // Disable VSync on all quality levels
        for( int i=0 ; i<names.Length ; ++i )
        {
            QualitySettings.SetQualityLevel( i );
            QualitySettings.vSyncCount = 0;
        }

        QualitySettings.SetQualityLevel( qualityLevel );

        MVRTools.Log("Quality settings changed for all quality levels:");
        MVRTools.Log("- VSyncCount = 0");
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();

        if (GUILayout.Button("Re-apply VR player settings"))
        {
            ApplyVRSettings();
        }

        if (GUILayout.Button("Pick configuration file"))
        {
            string path = EditorUtility.OpenFilePanel("Please choose MiddleVR configuration file", "C:/Program Files (x86)/MiddleVR/data/Config", "vrx");
            MVRTools.Log("[+] Picked " + path );
            mgr.ConfigFile = path;
            EditorUtility.SetDirty(mgr);
        }

        DrawDefaultInspector();
        GUILayout.EndVertical();
    }

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) 
    {
        // Copy web assets for HTML5 default GUI
        string webAssetsPathSource = Path.Combine(Path.Combine(Application.dataPath, "MiddleVR"), ".WebAssets");
        if(Directory.Exists(webAssetsPathSource))
        {
            // The player executable file and the data folder share the same base name
            string webAssetsPathDestination =
                Path.Combine(Path.Combine(Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data", "MiddleVR"), ".WebAssets");
            FileSystemTools.DirectoryCopy(webAssetsPathSource, webAssetsPathDestination, true, true);
        }

        // Sign Application
        MVRTools.SignApplication( pathToBuiltProject );
    }
}

public class AdditionnalImports : AssetPostprocessor
{
    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // If this is not a MiddleVR package import, skip
        bool importingMvrDll = false;
        foreach (string s in importedAssets)
        {
            // MiddleVR_Unity3D.dll should be imported only at package import
            if (s.Contains("MiddleVR_Unity3D.dll"))
            {
                importingMvrDll = true;
                break;
            }
        }

        if (!importingMvrDll)
        {
            return;
        }

        MVRTools.Log(3, "[>] Begin package import post-process...");

        if (!File.Exists(Path.Combine(Application.dataPath, "MiddleVR_Source_Project.txt")))
        {
            // Clean old deprecated files from previous MiddleVR versions
            string[] filesToDelete = { Path.Combine("Editor", "VRCustomEditor.cs"),
                                       Path.Combine("Resources", "OVRLensCorrectionMat.mat"),
                                       Path.Combine(Path.Combine(Path.Combine("MiddleVR", "Scripts"), "Internal"), "VRCameraCB.cs"),
                                       Path.Combine(Path.Combine(Path.Combine("MiddleVR", "Assets"), "Materials"), "WandRayMaterial.mat"),
                                       Path.Combine("Plugins", "MiddleVR_UnityRendering.dll"),
                                       Path.Combine("Plugins", "MiddleVR_UnityRendering_x64.dll") };

            foreach (string fileToDelete in filesToDelete)
            {
                string filePath = Path.Combine(Application.dataPath, fileToDelete);
                if (File.Exists(filePath))
                {
                    MVRTools.Log(3, "[ ] Package import post process: clean deprecated MiddleVR files. Deleting file '" + filePath + "'.");
                    File.Delete(filePath);
                }
            }
        }

        MVRTools.Log(3, "[<] End package import post-process.");
    }
}

public class FileSystemTools
{
    public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite)
    {
        // Get the subdirectories for the specified directory
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] dirs = dir.GetDirectories();

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: '"
                + sourceDirName + "'.");
        }

        // If the destination directory doesn't exist, create it
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, overwrite);
        }

        // If copying subdirectories, copy them and their contents to new location
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs, overwrite);
            }
        }
    }
}
