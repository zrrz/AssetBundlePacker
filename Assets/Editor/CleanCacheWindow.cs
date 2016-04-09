using UnityEngine;
using System.Collections;
using UnityEditor;

public class CleanCacheWindow : EditorWindow {

	[MenuItem("Window/Clean Cache")]
	public static void CleanCache () {
		Debug.Log("Clean Cache");
		Caching.CleanCache();
	}
}
