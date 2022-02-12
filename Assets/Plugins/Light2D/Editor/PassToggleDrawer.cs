using UnityEditor;
using UnityEngine;

namespace Light2D
{
	public class PassToggleDrawer : MaterialPropertyDrawer
	{
		private string arg;

		public PassToggleDrawer()
		{
		}

		public PassToggleDrawer(string arg)
		{
			this.arg = arg;
		}

		static bool IsPropertyTypeSuitable(MaterialProperty prop)
		{
			return prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range;
		}

		public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
		{
			if (!IsPropertyTypeSuitable(prop))
            {
                return;
            }

			// Setup
			bool value = (prop.floatValue != 0.0f);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			// Show the toggle control
			value = EditorGUI.Toggle(position, label, value);

			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				// Set the new value if it has changed
				prop.floatValue = value ? 1.0f : 0.0f;
				if (!string.IsNullOrWhiteSpace(arg))
				{
					foreach (Material target in prop.targets)
					{
						target.SetShaderPassEnabled(arg, value);
					}
				}
			}
		}

		public override void Apply(MaterialProperty prop)
		{
			if (prop.hasMixedValue)
				return;

			if (!string.IsNullOrWhiteSpace(arg))
			{
				foreach (Material target in prop.targets)
				{
					target.SetShaderPassEnabled(arg, prop.floatValue != 0.0f);
				}
			}
		}
	}
}