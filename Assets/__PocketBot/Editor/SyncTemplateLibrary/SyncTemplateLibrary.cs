using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SyncTemplateLibrary
{
    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    [MenuItem("PocketBots/GetFromTemplate", false, priority = -2)]
    private static void Pull()
    {
        var sourcePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Submodules/Assets/_HybridCasualLibrary");
        var destinationPath = Path.Combine(Application.dataPath, "_HybridCasualLibrary");
        CopyFilesRecursively(sourcePath, destinationPath);
        AssetDatabase.Refresh();
    }

    [MenuItem("PocketBots/PushToTemplate", false, priority = -1)]
    private static void Push()
    {
        var sourcePath = Path.Combine(Application.dataPath, "_HybridCasualLibrary");
        var destinationPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Submodules/Assets/_HybridCasualLibrary");
        CopyFilesRecursively(sourcePath, destinationPath);
        AssetDatabase.Refresh();
    }
}