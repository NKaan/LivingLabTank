using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
	internal class SceneManagerView
    {
		BuildSettingsData buildSettings;
	    ReorderableList reorderableList;
		List<SceneEditorData> scenes;
		string[] sharedFolders;
		string[] packages;
		public SceneManagerView(BuildSettingsData buildSettings)
		{
			this.buildSettings = buildSettings;
			this.scenes = buildSettings.scenes;
			reorderableList = new ReorderableList(scenes, typeof(SceneEditorData), true, true, true, true);
			reorderableList.headerHeight = 22;
			reorderableList.drawElementCallback += OnDrawElement;
			reorderableList.onAddCallback += OnAddElement;
			reorderableList.onReorderCallback += OnReorder;
			reorderableList.drawHeaderCallback += OnDrawHeader;
			reorderableList.drawNoneElementCallback += OnDrawNoneElement;
		}

		private void OnDrawNoneElement(Rect rect)
		{
			rect.x += rect.width * 0.5f - 110;
			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.LabelField(rect, "Drag and drop scenes to this window.");
			EditorGUI.EndDisabledGroup();
		}

		private void OnDrawHeader(Rect rect)
		{
			rect.x += 30;
			EditorGUI.LabelField(rect, "| Shared Resources |");
			rect.x += 120;
			EditorGUI.LabelField(rect, " Package |");
			rect.x += 120;
			EditorGUI.LabelField(rect, "| Scene Path |");
		}

		private void OnReorder(ReorderableList list)
		{
			UpdateLevelIndexes();
		}

		private void OnAddElement(ReorderableList list)
		{
			int countLoaded = EditorSceneManager.sceneCount;

			for (int i = 0; i < countLoaded; i++)
			{
				var scene = EditorSceneManager.GetSceneAt(i);
				if (!scenes.Any(x => x.name == scene.name))
				{
					scenes.Add(new SceneEditorData { enabled = true, name = scene.name, path = scene.path });
				}
			}
			UpdateLevelIndexes();
		}

		void UpdateLevelIndexes()
		{
			int levelIndex = 0;
			foreach (var scene in scenes)
			{
				if (scene.enabled)
				{
					scene.index = levelIndex;
					levelIndex++;
				}
			}
		}
		private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			EditorGUI.BeginChangeCheck();
			scenes[index].enabled = EditorGUI.Toggle(new Rect(rect.x, rect.y, 18, 18), scenes[index].enabled);
			if (EditorGUI.EndChangeCheck())
			{
				UpdateLevelIndexes();
			}
			if (sharedFolders != null && sharedFolders.Length != 0)
			{
				rect.x += 20;
				var selectedIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, 18), Array.IndexOf(sharedFolders, scenes[index].sharedResourcesKey), sharedFolders);
				if (selectedIndex >= 0 && selectedIndex < sharedFolders.Length)
				{
					scenes[index].sharedResourcesKey = sharedFolders[selectedIndex];
				}
				rect.x += 100;
			}
			if (packages != null && packages.Length != 0)
			{
				rect.x += 20;
				var selectedIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, 18), Array.IndexOf(packages, scenes[index].packageName), packages);
				if (selectedIndex >= 0 && selectedIndex < packages.Length)
				{
					scenes[index].packageName = packages[selectedIndex];
				}
				rect.x += 100;
			}
			EditorGUI.BeginDisabledGroup(!scenes[index].enabled);
			rect.x += 20;
			EditorGUI.LabelField(rect, $"{(scenes[index].enabled ? $"[{scenes[index].index}] - " : "")}[{scenes[index].name}]-{scenes[index].path.Replace(".unity", "")}");
			EditorGUI.EndDisabledGroup();
		}
		IEnumerable<string> GetOptions()
		{
			yield return "None";
			foreach(var name in buildSettings.sharedFolders.Select(x => x.name))
			{
				yield return name;
			}
		}
		public void OnGUI()
		{
			sharedFolders = GetOptions().ToArray();
			packages = buildSettings.packages.ToArray();
			var dropArea = EditorGUILayout.BeginVertical();
			reorderableList.DoLayoutList();
			EditorGUILayout.EndVertical();
			if (Event.current.type == EventType.DragUpdated)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				Event.current.Use();
			}
			else if (Event.current.type == EventType.DragPerform)
			{
				if (!dropArea.Contains(Event.current.mousePosition))
					return;
				DragAndDrop.AcceptDrag();
				if (DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences.Length == 0)
				{
					foreach (string path in DragAndDrop.paths)
					{
						if (System.IO.Path.GetExtension(path) == ".unity")
						{
							var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
							if (!scenes.Any(x => x.name == sceneName))
							{
								scenes.Add(new SceneEditorData { name = sceneName, path = path });
								UpdateLevelIndexes();
							}
						}
					}
				}
				else if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
				{
					var sceneAssets = DragAndDrop.objectReferences.Where(x => x is SceneAsset).Select(x => x as SceneAsset);
					foreach (var sceneAsset in sceneAssets)
					{
						var assetPath = AssetDatabase.GetAssetPath(sceneAsset);
						if (!scenes.Any(x => x.name == sceneAsset.name))
						{
							scenes.Add(new SceneEditorData { enabled = true, name = sceneAsset.name, path = assetPath });
							UpdateLevelIndexes();
						}
					}
				}
			}
		}

	}
}
