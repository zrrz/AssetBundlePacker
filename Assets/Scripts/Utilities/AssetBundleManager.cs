//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

public class AssetBundleManager : MonoBehaviour {

	public class CharacterBundle {
		public CharacterBundle(GameObject p_model, Texture2D p_icon, Texture2D p_sprite, bool p_isYoutubeCharacter) {
			model = p_model;
			icon = p_icon;
			sprite = p_sprite;
			isYoutubeCharacter = p_isYoutubeCharacter;
		}

		void MakeWarningsGoAway() {
			print(model.ToString() + icon.ToString() + sprite.ToString() + isYoutubeCharacter.ToString() + " ");
		}

		public GameObject model;
		public Texture2D icon;
		public Texture2D sprite;
		public bool isYoutubeCharacter;
	}

	private class ABInfo {
		public ABInfo(string p_url, int p_version, int p_isYoutubeCharacter) {
			url = p_url;
			version = p_version;
			isYoutubeCharacter = p_isYoutubeCharacter;
		}

		public string url;
		public int version;
		public int isYoutubeCharacter;
	}

	enum DownloadState {
		NotStarted,
		Downloading,
		Done}

	;

	/// <summary>
	/// URL for asset list .txt document.
	/// Format:
	/// url version
	/// www.url.url/filename.unity3d 4
	/// </summary>
	string remoteAssetListURL = "https://s3.amazonaws.com/dead-realm/characters/files.txt";

	string localAssetListURL = "CharacterBundles/localCharacterBundles";

	static DownloadState downloadState;

	List<CharacterBundle> characterBundles;

	[SerializeField]
	bool downloadOnStart = true;

	//  public dfLabel downloadLabel;

	bool newestFilesDownloaded = false;

	static AssetBundleManager s_instance;

	public static AssetBundleManager Instance
	{
		get {
			if (s_instance != null) {
				return s_instance;
			}
			else {
				Debug.LogError("No AssetBundleManager in scene");
				return null;
			}
		}
	}

	void Awake() {
		DontDestroyOnLoad(gameObject);
		s_instance = this;

//    downloadLabel.gameObject.SetActive(false);

		//Caching.CleanCache();

		if (downloadOnStart) {
			StartDownload();
		}

	}

	void Update() {
		// Set to true if the current player downloaded all available characters
		//GHGameManager.LobbyManager.SetCurrentLobbyCustCharDledState(downloadState == DownloadState.Done);
	}

	/// <summary>
	/// Gets the uninstatiated character bundles.
	/// </summary>
	/// <returns>The character bundles.</returns>
	public static List<CharacterBundle> GetCharacterBundles() {
		return Instance.characterBundles;
	}

	/// <summary>
	/// Converts and returns instantiated character bundles into FBXImportData.
	/// </summary>
	/// <returns>The FBX import data.</returns>
	//  public static List<GHFBXImportData> GetFBXImportData()
	//  {
	//    List<GHFBXImportData> importData = new List<GHFBXImportData>();
	//
	//    if (Instance.characterBundles == null) { Debug.LogError("Charbundles are null"); }      
	//
	//    List<CharacterBundle> characterBundles = Instance.characterBundles;
	//    for (int i = 0; i < characterBundles.Count; i++)
	//    {
	//      GameObject character = (GameObject)Instantiate(characterBundles[i].model, Vector3.one * 9999f, Quaternion.identity);
	//
	//	  GHFBXImportData data = new GHFBXImportData();
	//
	//      //Get Avatar
	//      data.avatar = character.GetComponent<Animator>().avatar;
	//      if (data.avatar == null)
	//      {
	//        Debug.LogError("Can't find Avatar");
	//        return null;
	//      }
	//
	//      //Remove Animator
	//      DestroyImmediate(character.GetComponent<Animator>());
	//      if (data.avatar == null)
	//      {
	//        Debug.LogError("Avatar deleted");
	//        return null;
	//      }
	//
	//      //Get Transforms
	//      data.modelTransforms = character.GetComponentsInChildren<Transform>();
	//
	//      // Get the name, icon, and sprite for character select screen
	//      data.charName = character.gameObject.name.Replace("(Clone)", "");
	//      data.gridCellIcon = characterBundles[i].icon;
	//      data.hoverSprite = characterBundles[i].sprite;
	//
	//	  data.isYoutubeCharacter = characterBundles[i].isYoutubeCharacter;
	//
	//      importData.Add(data);
	//    }
	//    return importData;
	//  }
	/// <summary>
	/// Checks the current version of downloaded characters against the newest on the servers.
	/// </summary>
	/// <returns><c>true</c>, if download for newest was checked, <c>false</c> otherwise.</returns>
	public static bool CheckDownloadForNewest() {
		Instance.StartCoroutine(Instance.CheckDownloadForNewestInternal());
		return Instance.newestFilesDownloaded;
	}

	IEnumerator CheckDownloadForNewestInternal() {
		newestFilesDownloaded = false;
		using (WWW wwwFileList = new WWW(remoteAssetListURL)) {
			yield return wwwFileList; 
			if (wwwFileList.error != null) {
				Debug.LogError("wwwFileList download had an error:" + wwwFileList.error);
				yield break;
			}
            
			// Parse asset list into ABInfos - URL, Version
			List<ABInfo> assetBundleInfos = new List<ABInfo>();
			string[] assetBundlesURLs = wwwFileList.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
			foreach (string url in assetBundlesURLs) {
				string[] info = url.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
				assetBundleInfos.Add(new ABInfo(info[0], int.Parse(info[1]), int.Parse(info[2])));
			}
            
			// Wait for the Caching system to be ready
			while (!Caching.ready)
				yield return null;
            
			foreach (ABInfo abInfo in assetBundleInfos) {
				if (!Caching.IsVersionCached(abInfo.url, abInfo.version)) {
					newestFilesDownloaded = false;
					yield break;
				}
			}
			newestFilesDownloaded = true;
		}
	}

	/// <summary>
	/// Starts the download from assetListURL for character bundles.
	/// </summary>
	public static void StartDownload() {
		if (downloadState != DownloadState.Downloading) {
//        if(Instance.downloadLabel != null) {
//            Instance.downloadLabel.gameObject.SetActive(true);
//            Instance.UpdateDownloadText(0);
//        }
			Instance.StartCoroutine(Instance.RetryDownload());
		}
	}

	/// <summary>
	/// Determines whether the character bundles are done downloading.
	/// </summary>
	/// <returns><c>true</c> if the download is done; otherwise, <c>false</c>.</returns>
	public static bool IsDownloadDone() {
		return (downloadState == DownloadState.Done);
	}

	IEnumerator RetryDownload() {
		downloadState = DownloadState.Downloading;
		while (downloadState == DownloadState.Downloading) {
			yield return StartCoroutine(DownloadAndCache());
			if (downloadState == DownloadState.Downloading) {
				Debug.LogError("Download failed. Trying again in 3 seconds.");
				yield return new WaitForSeconds(3f);
			}
		}
//        downloadLabel.gameObject.SetActive(false);
	}

	//HACK can't return in a coroutine
	AssetBundle decryptedAB;

	/// <summary>
	/// Decrypts the asset bundle contents into another asset bundle.
	/// </summary>
	/// <returns>The asset bundle contents.</returns>
	/// <param name="">.</param>
	IEnumerator DecryptAssetBundleContents(AssetBundle encryptedAssetBundle) {
		TextAsset ABTextAsset = encryptedAssetBundle.LoadAllAssets<TextAsset>()[0];

		byte[] toDecryptArray = ABTextAsset.bytes;
		byte[] resultArray = null;
		bool done = false;

		Thread thread = new Thread((ThreadStart)delegate {
				DecryptBytes(toDecryptArray, ref resultArray, ref done);
			}
		);
		thread.Start();

		while (!done) {
			yield return null;
		}
		thread.Join();

		encryptedAssetBundle.Unload(false); //Clear outer AB because silly me named them the same. TODO rename them all
		
		AssetBundleCreateRequest decryptedABRequest = AssetBundle.LoadFromMemoryAsync(resultArray);
		
		while (!decryptedABRequest.isDone)
			yield return null;

		decryptedAB = decryptedABRequest.assetBundle;
	}

	void DecryptBytes(byte[] toDecryptArray, ref byte[] resultArray, ref bool done) {  
		byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");
		// AES-256 key
		//byte[] toDecryptArray = Convert.FromBase64String (toDecrypt);
		RijndaelManaged rDel = new RijndaelManaged();
		rDel.Key = keyArray;
		rDel.Mode = CipherMode.ECB;
		// http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
		rDel.Padding = PaddingMode.PKCS7;
		// better lang support
		ICryptoTransform encryptor = rDel.CreateDecryptor();

		//This is blocking but we gucci because it's in a thread
		resultArray = encryptor.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);

		done = true;
	}  

	IEnumerator DownloadAndCache() {
//		{
//			//Local characters
//			List<ABInfo> assetBundleInfos = new List<ABInfo>();
//			TextAsset localAssetList = (TextAsset)Resources.Load (localAssetListURL, typeof(TextAsset));
//			if(localAssetList == null) {
//				Debug.LogError("Can't find Resources/" + localAssetListURL, this);
//				yield break;
//			}
//			string[] assetBundlesURLs = localAssetList.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
//			
//			foreach (string url in assetBundlesURLs)
//			{
//				string[] info = url.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
//				assetBundleInfos.Add(new ABInfo(info[0], int.Parse(info[1]), int.Parse (info[2])));
//			}
//
//			// Load all character bundles from asset list
//			characterBundles = new List<CharacterBundle>();
//			foreach (ABInfo info in assetBundleInfos)
//			{		
//				TextAsset bundleAsText = Resources.Load(info.url) as TextAsset;
//				byte[] bundleData  = bundleAsText.bytes.Clone() as byte[];
//				AssetBundle bundle = AssetBundle.LoadFromMemory( bundleData );
//
//				Debug.Log("loading " + info.url);
//
//				if(bundle == null) {
//					Debug.LogError("Can't load " + info.url);
//				}
//
//				//Parse asset bundle for character prefab
//				GameObject model = (GameObject)bundle.LoadAll(typeof(GameObject))[0];
//				Debug.Log(bundle.LoadAll(typeof(GameObject)).Length + " models in bundle");
//				
//				//Parse asset bundle for Icon and Sprite
//				UnityEngine.Object[] images = bundle.LoadAll(typeof(Texture2D));
//				Texture2D icon = null, sprite = null;
//				for (int i = 0; i < images.Length; i++)
//				{
//					if (images[i].name.ToLower().Contains("icon"))
//						icon = (Texture2D)images[i];
//				}
//				for (int i = 0; i < images.Length; i++)
//				{
//					if (images[i].name.ToLower().Contains("sprite") || images[i].name.ToLower().Contains("portrait"))
//						sprite = (Texture2D)images[i];
//				}
//				
//				characterBundles.Add(new CharacterBundle(model, icon, sprite, info.isYoutubeCharacter > 0));
//				// Unload the AssetBundles compressed contents to conserve memory
//				//bundle.Unload(false); //TODO unload later
//			}
//	}
	
		//Remote characters
		using (WWW wwwFileList = new WWW(remoteAssetListURL)) {
			//Download file list of character bundles
			yield return wwwFileList; 
			if (wwwFileList.error != null) {
				Debug.LogError("wwwFileList download had an error:" + wwwFileList.error);
				yield break;
			}
      
			// Parse asset list into ABInfos - URL, Version
			List<ABInfo> assetBundleInfos = new List<ABInfo>();
			string[] assetBundlesURLs = wwwFileList.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string url in assetBundlesURLs) {
				string[] info = url.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
				if (info.Length != 3)
					continue;
				assetBundleInfos.Add(new ABInfo(info[0], int.Parse(info[1]), int.Parse(info[2])));
			}

			// Wait for the Caching system to be ready
			while (!Caching.ready)
				yield return null;

			int downloaded = -1;
			// Load all character bundles from asset list
			characterBundles = new List<CharacterBundle>();
			foreach (ABInfo info in assetBundleInfos) {
				downloaded++;
				using (WWW www = WWW.LoadFromCacheOrDownload(info.url, info.version)) {
					yield return www;
					if (www.error != null) {
						Debug.LogError("WWW download had an error:" + www.error + " " + info.url);
						yield break;
					}
		
					AssetBundle bundle;

					if (info.isYoutubeCharacter == 1) {
						yield return StartCoroutine(DecryptAssetBundleContents(www.assetBundle));
						bundle = decryptedAB; 
					}
					else {
						bundle = www.assetBundle;
					}

//					foreach (UnityEngine.Object obj in bundle.LoadAllAssets()) {
//						Debug.Log(obj.name + obj.GetType());
//					}

					//Parse asset bundle for character prefab
					GameObject model = (GameObject)bundle.LoadAllAssets(typeof(GameObject))[0];
//					Debug.Log(bundle.LoadAllAssets(typeof(GameObject)).Length + " models in bundle");

					//Parse asset bundle for Icon and Sprite
					UnityEngine.Object[] images = bundle.LoadAllAssets(typeof(Texture2D));
					Texture2D icon = null, sprite = null;
					for (int i = 0; i < images.Length; i++) {
						if (images[i].name.ToLower().Contains("icon"))
							icon = (Texture2D)images[i];
					}
					for (int i = 0; i < images.Length; i++) {
						if (images[i].name.ToLower().Contains("sprite") || images[i].name.ToLower().Contains("portrait"))
							sprite = (Texture2D)images[i];
					}

					characterBundles.Add(new CharacterBundle(model, icon, sprite, info.isYoutubeCharacter > 0));

					// Unload the AssetBundles compressed contents to conserve memory
					//bundle.Unload(false); //TODO unload later
				}
			}
		}

		string loaded = characterBundles.Count + " characters loaded: ";
		for (int i = 0; i < characterBundles.Count; i++) {
			loaded += characterBundles[i].model.name + (i == characterBundles.Count - 1 ? "" : ", ");
		}
		Debug.Log(loaded, this);

		downloadState = DownloadState.Done;

		// Set to true if the current player downloaded all available characters
//    GHGameManager.LobbyManager.SetCurrentLobbyCustCharDledState(downloadState == DownloadState.Done);
	}

	//    void UpdateDownloadText(int percentage) {
	//        downloadLabel.Text = "Downloading new Characters: " + percentage + "%";
	//    }
}
