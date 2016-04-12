using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public class ExportAssetBundles {

	public static void ExportResource(Object[] selections) {
		// Create the array of bundle build details.
		AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
		
		buildMap[0].assetBundleName = selections[0].name + ".unity3D";

		List<string> dependencies = new List<string>();
		dependencies.Add(AssetDatabase.GetAssetPath(selections[0]));
		dependencies.Add(AssetDatabase.GetAssetPath(selections[1]));
		dependencies.Add(AssetDatabase.GetAssetPath(selections[2]));

//		Object[] dependencyObjs = EditorUtility.CollectDependencies(selections);
//		foreach (Object dependency in dependencyObjs) {
//			string path = AssetDatabase.GetAssetPath(dependency);
//			if(!dependencies.Contains(path))
//				dependencies.Add(path);
//		}

		buildMap[0].assetNames = dependencies.ToArray();

		BuildPipeline.BuildAssetBundles("Assets/AssetBundles/Unencrypted/", buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

		EncryptAssetBundle(selections[0].name, "Assets/AssetBundles/Unencrypted/");
	}

	/// <summary>
	/// Encrypts and saves the asset bundle.
	/// </summary>
	/// <param name="bundleName">Bundle name.</param>
	static void EncryptAssetBundle(string bundleName, string path) {

		//Encrypt
		Debug.Log("Opening " + path + bundleName + ".unity3D");
		byte[] assetBundleBytes = File.ReadAllBytes(path + bundleName + ".unity3D");

		byte[] keyArray = UTF8Encoding.UTF8.GetBytes ("12345678901234567890123456789012");
		// 256-AES key
		RijndaelManaged rDel = new RijndaelManaged ();
		rDel.Key = keyArray;
		rDel.Mode = CipherMode.ECB;
		// http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
		rDel.Padding = PaddingMode.PKCS7;
		// better lang support
		ICryptoTransform cTransform = rDel.CreateEncryptor ();
		byte[] resultArray = cTransform.TransformFinalBlock (assetBundleBytes, 0, assetBundleBytes.Length);

		string fullPath = path + bundleName + ".bytes";
		File.WriteAllBytes(fullPath, resultArray);
		Debug.Log("Writing to " + fullPath);

		//Build AB of encrypted AB
		AssetBundleBuild[] encryptedBuildMap = new AssetBundleBuild[1];
		encryptedBuildMap[0].assetBundleName = bundleName + ".unity3D";
		Debug.Log("Creating AB around " + path + bundleName + ".bytes");
		encryptedBuildMap[0].assetNames = new string[] {path + bundleName + ".bytes"};

		AssetDatabase.Refresh();

		BuildPipeline.BuildAssetBundles("Assets/AssetBundles", encryptedBuildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
	}
}
