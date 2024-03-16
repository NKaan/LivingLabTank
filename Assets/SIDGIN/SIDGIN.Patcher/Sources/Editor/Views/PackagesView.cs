using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using SIDGIN.Patcher.Client;

namespace SIDGIN.Patcher.Editors
{
    internal class PackagesView
    {
		ReorderableList reorderableList;
		List<string> packages;
		string newPackageName;
		public PackagesView(BuildSettingsData buildSettingsCacheData)
		{
			this.packages = buildSettingsCacheData.packages;
			if(packages.Count == 0)
			{
				packages.Add("Main");
			}
			reorderableList = new ReorderableList(packages, typeof(string), true, true, false, true);
			reorderableList.headerHeight = 22;
			reorderableList.drawElementCallback += OnDrawElement;
			reorderableList.onAddCallback += OnAddElement;
			reorderableList.onRemoveCallback += OnRemoveElement;
			reorderableList.onReorderCallback += OnReorder;
			reorderableList.drawHeaderCallback += OnDrawHeader;
			reorderableList.drawNoneElementCallback += OnDrawNoneElement;
		}

        private void OnRemoveElement(ReorderableList list)
        {
			if ((list.list[list.index] as string) != Consts.MAIN_PACKAGE_NAME)
            {
				list.list.RemoveAt(list.index);
            }
        }

        private void OnDrawNoneElement(Rect rect)
		{
			rect.x += rect.width * 0.5f - 110;
			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.LabelField(rect, "Packages list is empty.");
			EditorGUI.EndDisabledGroup();
		}

		private void OnDrawHeader(Rect rect)
		{
			rect.x += 10;
			EditorGUI.LabelField(rect, "| Package name |");
		}

		private void OnReorder(ReorderableList list)
		{
		}

		private void OnAddElement(ReorderableList list)
		{

		}

		private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, 18), packages[index]);

		}

		public void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("New Package Name: ");
			newPackageName = EditorGUILayout.TextField(newPackageName);
			EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(newPackageName) || packages.Any(x => x == newPackageName));
            if (GUILayout.Button("Add", GUILayout.Width(50)))
			{
				reorderableList.list.Add(newPackageName);
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			reorderableList.DoLayoutList();
		}
	}

}