using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    internal class SGResourcesView
    {
		BuildSettingsData bundlesData;
		ReorderableList reorderableList;
		List<SGResourcePackageData> ignoredSGResources;
		string[] packages;
		public SGResourcesView(BuildSettingsData bundlesData)
		{
			this.bundlesData = bundlesData;
			this.ignoredSGResources = bundlesData.sgResourcePackage;
			reorderableList = new ReorderableList(ignoredSGResources, typeof(SceneEditorData), true, true, true, true);
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
			EditorGUI.LabelField(rect, "Drag and drop folder to this window.");
			EditorGUI.EndDisabledGroup();
		}

		private void OnDrawHeader(Rect rect)
		{
			rect.x += 10;
			EditorGUI.LabelField(rect, "| Package |");
			rect.x += 120;
			EditorGUI.LabelField(rect, "| Folder into SGResources |");
		}

		private void OnReorder(ReorderableList list)
		{

		}

		private void OnAddElement(ReorderableList list)
		{
			var folderPath = EditorUtility.OpenFolderPanel("Choose folder into SGResources folder", Application.dataPath,"");
			if (!string.IsNullOrEmpty(folderPath))
			{
				var relativePath = SGResourcesBuilder.GetRelativePath(folderPath);
				if (!ignoredSGResources.Any(x=>x.localPath == relativePath))
				{
					var bundleName = SGResourcesBuilder.GetBundleName(relativePath);
					ignoredSGResources.Add(new SGResourcePackageData { localPath = relativePath, bundleName = bundleName });
				}
			}
		}

		private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (packages != null && packages.Length != 0)
			{
				var selectedIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, 18), Array.IndexOf(packages, ignoredSGResources[index].packageName), packages);
				if (selectedIndex >= 0 && selectedIndex < packages.Length)
				{
					ignoredSGResources[index].packageName = packages[selectedIndex];
				}
				rect.x += 120;
			}
			EditorGUI.LabelField(rect, ignoredSGResources[index].localPath);
		}
		IEnumerable<string> GetOptions()
		{
			yield return "None";
			foreach (var name in bundlesData.sharedFolders.Select(x => x.name))
			{
				yield return name;
			}
		}
		public void OnGUI()
		{
			packages = bundlesData.packages.ToArray();
			var dropArea = EditorGUILayout.BeginVertical();
			reorderableList.DoLayoutList();
			EditorGUILayout.EndVertical();
			dropArea.height += reorderableList.elementHeight * reorderableList.count;
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
				if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
				{
					var folderAssets = DragAndDrop.objectReferences.Where(x => x is DefaultAsset).Select(x => x as DefaultAsset);
					int index = 0;
					foreach (var folderAsset in folderAssets)
					{
						string path = DragAndDrop.paths[index];

						var relativePath = SGResourcesBuilder.GetRelativePath(path);
						if (!ignoredSGResources.Any(x => x.localPath == relativePath))
						{
							var bundleName = SGResourcesBuilder.GetBundleName(relativePath);
							ignoredSGResources.Add(new SGResourcePackageData { localPath = relativePath, bundleName = bundleName });
						}
						index++;
					}
				}
			}
			
		}
	}
}
