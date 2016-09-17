using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class AssetBundlePacker : EditorWindow {

	Texture2D icon;
	Texture2D sprite;

	GameObject characterPrefab;

	bool isYoutubeCharacter;

	List<AudioClip> taunts;

	bool tauntUnfolded = false;

//	ExportAssetBundles.CharacterType characterType;

	[MenuItem("3BlackDot/AssetBundlePacker")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(AssetBundlePacker));
	}

	void OnGUI() {
		isYoutubeCharacter = EditorGUILayout.Toggle("isYoutubeCharacter", isYoutubeCharacter);
		icon = (Texture2D)EditorGUILayout.ObjectField("Icon Texture2D", icon, typeof(Texture2D), false);
		sprite = (Texture2D)EditorGUILayout.ObjectField("Sprite Texture2D", sprite, typeof(Texture2D), false);
		tauntUnfolded = EditorGUILayout.Foldout(tauntUnfolded, "Taunts");
		if(tauntUnfolded) {
			if (taunts == null)
				taunts = new List<AudioClip>();
			bool end = false;
			for (int i = 0; i < taunts.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				taunts[i] = (AudioClip)EditorGUILayout.ObjectField("Taunts", taunts[i], typeof(AudioClip), false);
				if (GUILayout.Button("-")) {
					taunts.RemoveAt(i);
					i--;
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("+")) {
				taunts.Add(null);
//				taunts[taunts.Count-1] = (AudioClip)EditorGUILayout.ObjectField("Taunts", taunts, typeof(AudioClip), false);
			}
		}


		EditorGUILayout.LabelField("Character Prefab. Mixamo rigged humanoid character");
		characterPrefab = (GameObject)EditorGUILayout.ObjectField("", characterPrefab, typeof(GameObject), false);

//		characterType = (ExportAssetBundles.CharacterType)EditorGUILayout.EnumPopup (characterType);

		if(GUILayout.Button("Package Asset Bundle")) {
			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(icon), characterPrefab.name + "_Icon");
			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(sprite), characterPrefab.name + "_Sprite");
			ExportAssetBundles.ExportResource(new Object[] {characterPrefab, icon, sprite}, isYoutubeCharacter, taunts);
		}
	}
}
