using UnityEngine;

[ExecuteInEditMode, AddComponentMenu("Image Effects/PixelBoy")]
public class PixelBoy : MonoBehaviour
{
	public Camera cam;
	public int w = 720;
	private int h;

	protected void Start()
	{
		cam = GetComponent<Camera>();
	}

	private void Update()
	{
		var ratio = cam.pixelHeight / (float)cam.pixelWidth;
		h = Mathf.RoundToInt(w * ratio);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		source.filterMode = FilterMode.Point;
		var buffer = RenderTexture.GetTemporary(w, h, -1);
		buffer.filterMode = FilterMode.Point;
		Graphics.Blit(source, buffer);
		Graphics.Blit(buffer, destination);
		RenderTexture.ReleaseTemporary(buffer);
	}
}