using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Trive.Editor
{
	[CustomEditor(typeof(SoundLibrary)),CanEditMultipleObjects]
	public class SoundLibraryInspector : UnityEditor.Editor
	{
		private ReorderableList list;

		private void OnEnable()
		{
			list = new ReorderableList(serializedObject,serializedObject.FindProperty("audoClips"));
			list.drawElementCallback = (rect, index, active, focused) =>
			{
				var prop = list.serializedProperty.GetArrayElementAtIndex(index);
				var clipProp = prop.FindPropertyRelative("clip");
				var volumeProp = prop.FindPropertyRelative("volume");
				var weightProp = prop.FindPropertyRelative("weight");
				EditorGUI.PropertyField(new Rect(rect.x,rect.y,rect.width * 0.5f,EditorGUIUtility.singleLineHeight), clipProp,GUIContent.none);
				EditorGUI.PropertyField(new Rect(rect.width * 0.5f + rect.x, rect.y,rect.width * 0.4f, EditorGUIUtility.singleLineHeight), volumeProp, GUIContent.none);
				EditorGUI.PropertyField(new Rect(rect.width * 0.5f + rect.width * 0.4f + 4 + rect.x, rect.y, rect.width * 0.1f - 4, EditorGUIUtility.singleLineHeight), weightProp, GUIContent.none);
			};
			list.onAddCallback = reorderableList =>
			{
				reorderableList.serializedProperty.arraySize++;
				var prop = reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.serializedProperty.arraySize - 1);
				prop.FindPropertyRelative("clip").objectReferenceValue = null;
				prop.FindPropertyRelative("volume").floatValue = 1;
				prop.FindPropertyRelative("weight").intValue = 1;
				reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
			};
			list.onRemoveCallback = reorderableList =>
			{
				reorderableList.serializedProperty.DeleteArrayElementAtIndex(reorderableList.index);
				reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
				reorderableList.index--;
			};
		}

		public override void OnInspectorGUI()
		{
			if (Event.current.type == EventType.DragUpdated)
			{
				if (DragAndDrop.objectReferences.Any(o => o is AudioClip))
				{
					Event.current.Use();
					DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				}
			}
			else if (Event.current.type == EventType.DragPerform)
			{
				AudioClip[] clips = DragAndDrop.objectReferences.OfType<AudioClip>().Distinct().OrderBy(c => c.name).ToArray();
				if (clips.Length > 0)
				{
					Event.current.Use();
					DragAndDrop.AcceptDrag();
					var clipsProp = serializedObject.FindProperty("audoClips");
					clipsProp.arraySize += clips.Length;
					for (int i = 0; i < clips.Length; i++)
					{
						var clipProp = clipsProp.GetArrayElementAtIndex(clipsProp.arraySize - 1 - i);
						clipProp.FindPropertyRelative("clip").objectReferenceValue = clips[i];
						clipProp.FindPropertyRelative("volume").floatValue = 1;
						clipProp.FindPropertyRelative("weight").intValue = 1;
						serializedObject.ApplyModifiedProperties();
					}
				}
			}

			base.OnInspectorGUI();
			list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Play Random"))
			{
				foreach (var t in targets)
				{
					SoundLibrary library = t as SoundLibrary;
					SoundLibrary.AudoClipWrapper clip = library.RandomWrapper();
					PlayClip(clip.Clip);
				}
			}
			if (GUILayout.Button("Play Shuffle"))
			{
				foreach (var t in targets)
				{
					SoundLibrary library = t as SoundLibrary;
					SoundLibrary.AudoClipWrapper clip = library.ShuffledWrapper();
					PlayClip(clip.Clip);
				}
			}
			EditorGUILayout.EndHorizontal();
        }

		public static void PlayClip(AudioClip clip,int startSample = 0, bool loop = false)
		{
			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
			MethodInfo method = audioUtilClass.GetMethod(
				"PlayClip",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new System.Type[] {
		 typeof(AudioClip), typeof(int), typeof(bool)
			},
			null
			);
			method.Invoke(
				null,
				new object[] {
		 clip,
		 startSample,
		 loop
			}
			);
		}
	}
}