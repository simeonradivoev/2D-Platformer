using DefaultNamespace;
using UnityEditor;
using UnityEngine;

namespace Trive.Editor
{
	[CustomPropertyDrawer(typeof(bool1))]
	public class Bool1Drawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (var valProp = property.FindPropertyRelative("_value"))
			{
				var valueRect = EditorGUI.PrefixLabel(position, label);
				valProp.intValue = EditorGUI.Toggle(valueRect, valProp.intValue == 1) ? 1 : 0;
			}
		}
	}
}