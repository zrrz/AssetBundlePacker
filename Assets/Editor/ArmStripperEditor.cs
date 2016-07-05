﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

public class ArmStripperEditor : EditorWindow {

	bool selectingBody = false;
//	bool dragging = false;
//	Vector2 startPos; 
//	Vector2 currentPos;

	GameObject character;

	Vector3 selectionCenter;
	Quaternion selectionRotation;
	Vector3 selectionSize;

	[MenuItem("3BlackDot/ArmStripper")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(ArmStripperEditor));
	}

	void OnGUI() {
		if(GUILayout.Button("Set character")) {
			character = Selection.activeGameObject;
			if (!ValidateMixamoCharacter()) {
				character = null;
				SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
				return;
			}
		}
		if (character != null)
			GUILayout.Label("Character: " + character.name);
		else
			GUILayout.Label("Not Set");

		if(GUILayout.Button("Set Bind Pose")) {
			SetTPose(character);
		}

		if (selectingBody) {
			if (GUILayout.Button("Stop Selecting Body Vertices")) {
				selectingBody = false;
			}
			if (GUILayout.Button("Delete Selected Vertices")) {
				DeleteSelectedVertices();
			}
		}
		else {
			if (GUILayout.Button("Select Body Vertices")) {
				SelectBodyVertices();
			}
		}
	}

	void SetTPose(GameObject humanoid) {
		if (!ValidateMixamoCharacter())
			return;

		//Reflection magic to set bind pose
		System.Type assmblyType = typeof(UnityEditor.Animations.AnimatorController);
		foreach (System.Type t in Assembly.GetAssembly(assmblyType).GetTypes()) {
			if (t.Name.Contains("AvatarSetupTool")) {
				System.Type[] paramaters = new System.Type[] { typeof(GameObject) };
				MethodInfo method = t.GetMethod("SampleBindPose", paramaters);
				method.Invoke(null, new object[] { humanoid });
			}
		}
	}

	void SelectBodyVertices() {
		if (!ValidateMixamoCharacter())
			return;
		
		selectingBody = true;
		selectionCenter = Vector3.zero;
		selectionRotation = Quaternion.identity;
		selectionSize = Vector3.one;
	}

	List<Vector3> GetVertsInSelection() {
		List<Vector3> selectedVertices = new List<Vector3>();

		Bounds bounds = new Bounds(character.transform.position + selectionCenter, selectionSize);
		SkinnedMeshRenderer[] skinnedMeshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers) {
			foreach (Vector3 vert in skinnedMeshRenderer.sharedMesh.vertices) {
				Vector3 vertWorldPoint = character.transform.TransformPoint(vert);
				if (bounds.Contains(vertWorldPoint)) {
					selectedVertices.Add(vertWorldPoint);
				}
			}
		}
		return selectedVertices;
	}

	bool ValidateMixamoCharacter() {
		if (character == null) {
			Debug.LogError("Must select a GameObject");
			return false;
		}
		if (character.GetComponent<Animator>() == null) {
			Debug.LogError("Must select GameObject with Animator");
			return false;
		}
		Transform root = character.transform.FindChild("mixamorig:Hips");
		if (root == null) {
			Debug.LogError("Can't find mixamorig:Hips");
			return false;
		}
		return true;
	}

	void DeleteSelectedVertices() {

		Object parentObject = EditorUtility.GetPrefabParent(character); 
		string characterPath = AssetDatabase.GetAssetPath(parentObject);
		string modelPath = AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<GameObject>(characterPath).GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh);

		string modelArmsPath = modelPath.Replace(".fbx", "_Arms.fbx");
		if (!AssetDatabase.CopyAsset(modelPath, modelArmsPath)) {
			Debug.LogError("Couldn't copy asset");
			return;
		}

		AssetDatabase.Refresh();

		GameObject newMesh = AssetDatabase.LoadAssetAtPath<GameObject>(modelArmsPath);

		for (int i = 0; i < character.transform.childCount; i++) {
			SkinnedMeshRenderer skinnedMeshRenderer = character.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
			if (skinnedMeshRenderer != null) {
				skinnedMeshRenderer.sharedMesh = newMesh.transform.FindChild(character.transform.GetChild(i).name).GetComponent<SkinnedMeshRenderer>().sharedMesh;
			}
		}

		SetTPose(character);

		Bounds bounds = new Bounds(character.transform.position + selectionCenter, selectionSize);
		SkinnedMeshRenderer[] skinnedMeshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers) {
			List<int> triangleList = new List<int>();
			List<Vector3> vertList = new List<Vector3>();
			List<Vector2> uvList = new List<Vector2>();
			List<Vector3> normalsList = new List<Vector3>();

			Mesh mesh = skinnedMeshRenderer.sharedMesh;
			Vector3[] vertices = mesh.vertices;

			for (int i = 0; i < vertices.Length; i++) {
				vertList.Add (vertices[i]); 
				uvList.Add (mesh.uv[i]);
				normalsList.Add (mesh.normals[i]);
			}

			int[] triangles = mesh.triangles;
			for (int i = 0; i < triangles.Length; i += 3) {
				if (bounds.Contains(vertices[triangles[i]]) && bounds.Contains(vertices[triangles[i + 1]]) && bounds.Contains(vertices[triangles[i + 2]])) {
					//In selection
				}
				else {
					triangleList.Add(triangles[i]);
					triangleList.Add(triangles[i + 1]);
					triangleList.Add(triangles[i + 2]);
				}
			}

			mesh.triangles = triangleList.ToArray();
			mesh.vertices = vertList.ToArray();
			mesh.uv = uvList.ToArray();
			mesh.normals = normalsList.ToArray();

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}

		AssetDatabase.SaveAssets();

		//TODO cleanup old character
	}

	void OnFocus() {
		// Remove delegate listener if it has previously
		// been assigned.
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		// Add (or re-add) the delegate.
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
	}

	void OnDestroy() {
		// When the window is destroyed, remove the delegate
		// so that it will no longer do any drawing.
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}

	void OnSceneGUI(SceneView sceneView) {
		if (selectingBody) {
			Vector3 characterPosition = character.transform.position;
//			selectionRotation = Handles.RotationHandle(selectionRotation, selectionCenter - (Vector3.up*0.4f));
			selectionSize = Handles.ScaleHandle(selectionSize, characterPosition + selectionCenter, selectionRotation, HandleUtility.GetHandleSize(characterPosition + selectionCenter));
			selectionCenter = Handles.PositionHandle(characterPosition + selectionCenter + (Vector3.up*0.4f), selectionRotation) - (Vector3.up*0.4f) - characterPosition;
			DrawWireCube(characterPosition + selectionCenter, selectionSize);

			Handles.color = Color.green;
			List<Vector3> selectedVertices = GetVertsInSelection();
			foreach (Vector3 vertex in selectedVertices) {
				Handles.DotCap(0, vertex, Quaternion.identity, 0.005f);
			}

		}
		sceneView.Repaint();
	}

	public static void DrawWireCube(Vector3 position, Vector3 size)
	{
		var half = size / 2;
		// draw front
		Handles.DrawLine(position + new Vector3(-half.x, -half.y, half.z), position + new Vector3(half.x, -half.y, half.z));
		Handles.DrawLine(position + new Vector3(-half.x, -half.y, half.z), position + new Vector3(-half.x, half.y, half.z));
		Handles.DrawLine(position + new Vector3(half.x, half.y, half.z), position + new Vector3(half.x, -half.y, half.z));
		Handles.DrawLine(position + new Vector3(half.x, half.y, half.z), position + new Vector3(-half.x, half.y, half.z));
		// draw back
		Handles.DrawLine(position + new Vector3(-half.x, -half.y, -half.z), position + new Vector3(half.x, -half.y, -half.z));
		Handles.DrawLine(position + new Vector3(-half.x, -half.y, -half.z), position + new Vector3(-half.x, half.y, -half.z));
		Handles.DrawLine(position + new Vector3(half.x, half.y, -half.z), position + new Vector3(half.x, -half.y, -half.z));
		Handles.DrawLine(position + new Vector3(half.x, half.y, -half.z), position + new Vector3(-half.x, half.y, -half.z));
		// draw corners
		Handles.DrawLine(position + new Vector3(-half.x, -half.y, -half.z), position + new Vector3(-half.x, -half.y, half.z));
		Handles.DrawLine(position + new Vector3(half.x, -half.y, -half.z), position + new Vector3(half.x, -half.y, half.z));
		Handles.DrawLine(position + new Vector3(-half.x, half.y, -half.z), position + new Vector3(-half.x, half.y, half.z));
		Handles.DrawLine(position + new Vector3(half.x, half.y, -half.z), position + new Vector3(half.x, half.y, half.z));
	}
}
