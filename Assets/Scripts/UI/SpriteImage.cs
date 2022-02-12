using Trive.Assets.Scripts.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class SpriteImage : MaskableGraphic
	{
		protected static Material s_ETC1DefaultUI;

		[SerializeField] private Sprite m_Sprite;

		public static Material defaultETC1GraphicMaterial
		{
			get
			{
				if (s_ETC1DefaultUI == null)
				{
					s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
				}
				return s_ETC1DefaultUI;
			}
		}

		public Sprite sprite
		{
			get => m_Sprite;
			set
			{
				if (m_Sprite != value)
				{
					m_Sprite = value;
					SetAllDirty();
				}
			}
		}

		public float pixelsPerUnit
		{
			get
			{
				float spritePixelsPerUnit = 100;
				if (sprite)
				{
					spritePixelsPerUnit = sprite.pixelsPerUnit;
				}

				float referencePixelsPerUnit = 100;
				if (canvas)
				{
					referencePixelsPerUnit = canvas.referencePixelsPerUnit;
				}

				return spritePixelsPerUnit / referencePixelsPerUnit;
			}
		}

		public override Texture mainTexture
		{
			get
			{
				if (sprite == null)
				{
					if (material != null && material.mainTexture != null)
					{
						return material.mainTexture;
					}
					return s_WhiteTexture;
				}

				return sprite.texture;
			}
		}

		public override Material material
		{
			get
			{
				if (m_Material != null)
				{
					return m_Material;
				}
#if UNITY_EDITOR
				if (Application.isPlaying && sprite && sprite.associatedAlphaSplitTexture != null)
				{
					return defaultETC1GraphicMaterial;
				}
#else
				if (sprite && sprite.associatedAlphaSplitTexture != null)
					return defaultETC1GraphicMaterial;
#endif

				return defaultMaterial;
			}

			set { base.material = value; }
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if (sprite == null)
			{
				base.OnPopulateMesh(toFill);
				return;
			}

			toFill.Clear();
			var rect = GetPixelAdjustedRect();
			var vertecies = sprite.vertices;

			if (vertecies.Length <= 0)
			{
				base.OnPopulateMesh(toFill);
				return;
			}

			var bounds = new Rect(vertecies[0], Vector2.zero);
			for (var i = 1; i < vertecies.Length; i++)
			{
				bounds = bounds.Encapculate(vertecies[i]);
			}

			var maxDist = Mathf.Max(bounds.width, bounds.height) * 0.5f;
			var triangles = sprite.triangles;
			var uv = sprite.uv;

			for (var i = 0; i < vertecies.Length; i++)
			{
				toFill.AddVert(
					new UIVertex
					{
						position = (vertecies[i] - bounds.center) / maxDist * Mathf.Min(rect.width, rect.height) * 0.5f,
						color = color,
						uv0 = uv[i]
					});
			}

			for (var i = 0; i < triangles.Length; i += 3)
			{
				toFill.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
			}
		}
	}
}