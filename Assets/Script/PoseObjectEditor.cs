#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;
using System.Collections.Generic;


public partial class PoseObjectEditor : EditorWindow 
{
    [MenuItem("VSnap/PoseObjectEditor")]
    private static void ShowWindow() {
        var window = GetWindow<PoseObjectEditor>("UIElements");
        window.titleContent = new GUIContent("VSnap PoseObjectEditor");

        var root = window.rootVisualElement;
        root.Clear();

        var label = new Label("PoseObjectを選択してからBuild AssetBundlesを押してください");
        root.Add(label);

        // PoseObjectの参照を入れられるフィールドを追加
        var poseObjectField = new ObjectField("PoseObject");
        poseObjectField.objectType = typeof(PoseObject);
        root.Add(poseObjectField);

        // windowで保持するPoseObjectの参照
        PoseObject poseObject = null;

        // PoseObjectの参照を入れられるフィールドの値が変更されたときにposeObjectに代入
        poseObjectField.RegisterValueChangedCallback(evt => {
            // window.BuildAllAssetBundles((PoseObject)evt.newValue);
            poseObject = (PoseObject)evt.newValue;
        });

        // BuildAllAssetBundlesボタンを追加
        var button = new Button(() => {
            window.BuildAllAssetBundles((PoseObject)poseObjectField.value);
            window.CheckBuild();
        });
        button.text = "Build AssetBundles";
        root.Add(button);

        window.Show();
    }

    private void BuildAllAssetBundles(PoseObject _poseObject)
    {
        if (_poseObject == null)
        {
            return;
        }

        // AssetBundleNameにposeobjectを割り当てられているアセットを取得
        string bundleName = "poseobject";

        // 他のオブジェクトのうち、AssetBundleNameにposeobjectを割り当てられているものを取得
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        List<string> assetPaths = new List<string>();
        foreach (string path in allAssetPaths)
        {
            if (AssetImporter.GetAtPath(path)?.assetBundleName == bundleName)
            {
                assetPaths.Add(path);
            }
        }
        // assetPathsの全てに対してnullを割り当てる
        foreach (string path in assetPaths)
        {
            AssetImporter importer1 = AssetImporter.GetAtPath(path);
            importer1.assetBundleName = null;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string assetPath = AssetDatabase.GetAssetPath(_poseObject);
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        importer.assetBundleName = bundleName;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string assetBundleDirectory = "Assets/AssetBundles";

        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        else
        {
            Directory.Delete(assetBundleDirectory, true);
            Directory.CreateDirectory(assetBundleDirectory);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
        {
            var androidPath = assetBundleDirectory + "/Android";
            if (!Directory.Exists(androidPath))
            {
                Directory.CreateDirectory(androidPath);
            }

            BuildPipeline.BuildAssetBundles(androidPath, BuildAssetBundleOptions.None, BuildTarget.Android);
        }

        if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
        {
            var iOSPath = assetBundleDirectory + "/iOS";
            if (!Directory.Exists(iOSPath))
            {
                Directory.CreateDirectory(iOSPath);
            }
            BuildPipeline.BuildAssetBundles(iOSPath, BuildAssetBundleOptions.None, BuildTarget.iOS);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public void CheckBuild()
    {
        string assetBundleDirectory = "Assets/AssetBundles";

        if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
        {
            var androidPath = assetBundleDirectory + "/Android";
            Object[] allAssets = LoadAllAssetsFromPath(androidPath + "/poseobject");
            foreach (Object asset in allAssets)
            {
                if (asset.GetType() == typeof(PoseObject))
                {
                    PoseObject poseObject = (PoseObject)asset;
                    Debug.Log("Android : Poses included in PoseObject\n" + CheckPoseObject(poseObject));
                }
            }
        }

        if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
        {
            var iOSPath = assetBundleDirectory + "/iOS";

            // AssetBundle内の全てのアセットのうち、PoseObjectを取得
            Object[] allAssets = LoadAllAssetsFromPath(iOSPath + "/poseobject");
            foreach (Object asset in allAssets)
            {
                if (asset.GetType() == typeof(PoseObject))
                {
                    PoseObject poseObject = (PoseObject)asset;
                    Debug.Log("iOS : Poses included in PoseObject\n" + CheckPoseObject(poseObject));
                }
            }
        }
    }

    private Object[] LoadAllAssetsFromPath(string path)
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
        if (assetBundle == null)
        {
            Debug.Log("AssetBundle is null");
            return null;
        }

        Object[] allAssets = assetBundle.LoadAllAssets();
        return allAssets;
    }

    private string CheckPoseObject(PoseObject _poseObject)
    {
        var poseListText = "";
        foreach (var poseGroup in _poseObject.animationClipGroup)
        {
            poseListText += poseGroup.name + " : ";
            foreach (var pose in poseGroup.animationClip)
            {
                poseListText += pose.name + ", ";
            }
            poseListText += "\n";
        }
        return poseListText;
    }
}
#endif