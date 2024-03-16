using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
	internal class SharedResourcesView
    {
		ReorderableList reorderableList;
		List<SharedFolderData> sharedFolders;
		public SharedResourcesView(List<SharedFolderData> sharedFolders)
		{
			this.sharedFolders = sharedFolders;
			reorderableList = new ReorderableList(sharedFolders, typeof(SharedFolderData), true, true, false, true);
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
			EditorGUI.LabelField(rect, "| Name |");
			rect.x += 120;
			EditorGUI.LabelField(rect, "| Shared Resources Folder |");
		}

		private void OnReorder(ReorderableList list)
		{
		}

		private void OnAddElement(ReorderableList list)
		{
			
		}

		private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			EditorGUI.BeginChangeCheck();
			sharedFolders[index].name = EditorGUI.TextField(new Rect(rect.x, rect.y, 100, 18),sharedFolders[index].name);
			if (EditorGUI.EndChangeCheck())
			{
			}
			rect.x += 120;
			EditorGUI.LabelField(rect, $"{sharedFolders[index].path}");
		
		}

		public void OnGUI()
		{
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
				if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
				{
					var folderAssets = DragAndDrop.objectReferences.Where(x => x is DefaultAsset).Select(x => x as DefaultAsset);
					int index = 0;
					foreach (var folderAsset in folderAssets)
					{
						string path = DragAndDrop.paths[index];
						sharedFolders.Add(new SharedFolderData { name = $"shared_{sharedFolders.Count}", path = path });
						index++;
					}
				}
			}
			
		}

	}
}
