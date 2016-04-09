using UnityEngine;
using UnityEditor;

public class ExportAssetBundles {

	/*public enum CharacterType {
		User, Youtube
	}*/

//	[MenuItem("Assets/Build AssetBundle From Selection - Track dependencies")]
	public static void ExportResource (Object[] selections/*, CharacterType characterType*/) {
		// Bring up save panel
		string path = EditorUtility.SaveFilePanel ("Save Resource", "", selections[0].name, "unity3d");
		if (path.Length != 0) {
			// Build the resource file from the active selection.
//			Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
			for(int i = 0; i < selections.Length; i++) {
				Debug.Log(selections[i].GetType().ToString() + " " + selections[i].name);
			}
			BuildPipeline.BuildAssetBundle(Selection.activeGameObject, selections, path, 
			                               BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
			Selection.objects = selections;
		}
	}
//	[MenuItem("Assets/Build AssetBundle From Selection - No dependency tracking")]
//	static void ExportResourceNoTrack () {
//		// Bring up save panel
//		string path = EditorUtility.SaveFilePanel ("Save Resource", "", "New Resource", "unity3d");
//		if (path.Length != 0) {
//			// Build the resource file from the active selection.
//			BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, path);
//		}
//	}
}
