using UnityEngine;
using System.Collections;
using UnityEditor;

public class AssetBundlePacker : EditorWindow {

	Texture2D icon;
	Texture2D sprite;

	GameObject characterPrefab;

//	ExportAssetBundles.CharacterType characterType;

	[MenuItem("AssetBundlePacker/GUI")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(AssetBundlePacker));
	}

	void OnGUI() {
		icon = (Texture2D)EditorGUILayout.ObjectField("Icon Texture2D", icon, typeof(Texture2D), false);
		sprite = (Texture2D)EditorGUILayout.ObjectField("Sprite Texture2D", sprite, typeof(Texture2D), false);

		EditorGUILayout.LabelField("Character Prefab. Mixamo rigged humanoid character");
		characterPrefab = (GameObject)EditorGUILayout.ObjectField("", characterPrefab, typeof(GameObject), false);

//		characterType = (ExportAssetBundles.CharacterType)EditorGUILayout.EnumPopup (characterType);

		if(GUILayout.Button("Package Asset Bundle")) {
			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(icon), characterPrefab.name + "_Icon");
			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(sprite), characterPrefab.name + "_Sprite");
			ExportAssetBundles.ExportResource(new Object[] {characterPrefab, icon, sprite}/*, characterType*/);
		}
	}
}
