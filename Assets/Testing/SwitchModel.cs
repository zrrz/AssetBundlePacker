using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchModel : MonoBehaviour {

	void Change(AssetBundleManager.CharacterBundle characterBundle) {
		GameObject newCharacter = Instantiate(characterBundle.model) as GameObject;
		StartCoroutine (ChangeModel (transform, newCharacter.transform));

		GameObject.Find("Icon").GetComponent<Renderer>().material.mainTexture = characterBundle.icon;
		GameObject.Find("Sprite").GetComponent<Renderer>().material.mainTexture = characterBundle.sprite;
	}

	IEnumerator ChangeModel(Transform controller, Transform newCharacter) {
		Animator anim = controller.GetComponent<Animator> ();

		Transform[] oldObjs = controller.GetComponentsInChildren<Transform> ();

		Vector3 skeletonPos = Vector3.zero;
		for (int i = 0; i < oldObjs.Length; i++) {
			if(oldObjs[i].childCount > 1) {
				skeletonPos = oldObjs[i].position;
			}
			if(oldObjs[i].gameObject != controller.gameObject)
				Destroy(oldObjs[i].gameObject);
		}


		Transform[] transforms = newCharacter.GetComponentsInChildren<Transform> ();
		for (int i = 0; i < transforms.Length; i++) {
			if(transforms[i] != newCharacter.transform) {
				if(transforms[i].parent == newCharacter.transform)
					transforms[i].parent = controller.transform;
			}
			if(transforms[i].childCount > 1) {
				transforms[i].position = skeletonPos;
			}
		}

		anim.avatar = newCharacter.GetComponent<Animator> ().avatar;

		Destroy (newCharacter.gameObject);

		yield return new WaitForEndOfFrame();
		controller.GetComponent<Animator>().Rebind ();
	}

	void OnGUI() {
		if(AssetBundleManager.IsDownloadDone()) {
			List<AssetBundleManager.CharacterBundle> characterBundles = AssetBundleManager.GetCharacterBundles();
			for(int i = 0; i < characterBundles.Count; i++) {
				if(GUILayout.Button(characterBundles[i].model.name)) {
					Change (characterBundles[i]);
				}
			}
		} else {
			GUILayout.Label("Downloading");
		}
	}
}
