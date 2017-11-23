using System;
using UnityEngine;
using System.IO;
using StixGames;
using UnityEditor;

public class GrassPainter : EditorWindow
{
	private static readonly string[] channelLabels = {
		"1",
		"2",
		"3",
		"4"
	};

	private static readonly string[] paintModeLabels = {
		"Add",
		"Remove",
		"Set",
	};

	//TODO: Add these to playerprefs or something similar
	private static int channel = 0;
	private static int brushMode = 0;
	private static float strength = 1;
	private static float size = 1;
	private static float softness = 0.5f;
	private static float spacing = 0.5f;
	private static float rotation = 0;

	private static bool useUndo = true;

	private static GrassPainter window;

	private GameObject grassObject;
	private Material grassMaterial;
	private Collider grassCollider;
	private Texture2D texture;
	private bool startedWithCollider;

	private int textureSize = 512;

	private bool mouseDown;
	private Vector3 lastPaintPos;
	private bool didDraw;
	private bool showCloseMessage;
	private bool showTargetSwitchMessage;

	private const float EPSILON = 0.01f;

	[MenuItem("Window/Stix Games/Grass Painter")]
	public static void OpenWindow()
	{
		window = GetWindow<GrassPainter>();
		window.Show();
	}

	private void RecreateWindow()
	{
		window = Instantiate(this);
		window.Show();
	}

	private void OnGUI()
	{
		TextureSettings();

		BrushSettings();

		PainterSettings();
	}

	void OnFocus()
	{
		channel = EditorPrefs.GetInt("StixGames.Painter.Channel", channel);
		brushMode = EditorPrefs.GetInt("StixGames.Painter.BrushMode", brushMode);
		strength = EditorPrefs.GetFloat("StixGames.Painter.Strength", strength);
		size = EditorPrefs.GetFloat("StixGames.Painter.Size", size);
		softness = EditorPrefs.GetFloat("StixGames.Painter.Softness", softness);
		spacing = EditorPrefs.GetFloat("StixGames.Painter.Spacing", spacing);
		rotation = EditorPrefs.GetFloat("StixGames.Painter.Rotation", rotation);
		showCloseMessage = EditorPrefs.GetBool("StixGames.Painter.ShowCloseMessage", true);
		showTargetSwitchMessage = EditorPrefs.GetBool("StixGames.Painter.ShowTargetSwitchMessage", true);
		useUndo = EditorPrefs.GetBool("StixGames.Painter.UseUndo", true);

		SceneView.onSceneGUIDelegate -= OnSceneGUI;
		SceneView.onSceneGUIDelegate += OnSceneGUI;

		Undo.undoRedoPerformed -= SaveTexture;
		Undo.undoRedoPerformed += SaveTexture;
	}

	void OnDestroy()
	{
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
		Undo.undoRedoPerformed -= SaveTexture;
		ResetRenderer(true);

		if (ShowUndoLossWarning(true))
		{
			Undo.ClearUndo(texture);
		}
		else
		{
			RecreateWindow();
		}
	}

	private bool ShowUndoLossWarning(bool isWindowClose)
	{
		if (isWindowClose && showCloseMessage || !isWindowClose && showTargetSwitchMessage)
		{
			string message = isWindowClose
				? "After closing the painter changes will be permanent, undo will no longer be possible."
				: "After switching grass object changes will be permanent, undo will no longer be possible.";

			int result = EditorUtility.DisplayDialogComplex("Make changes permanent?",
				message, isWindowClose ? "Close" : "Switch", "Cancel",
				isWindowClose ? "Close and don't show again" : "Switch and don't show again");

			if (result == 1)
			{
				return false;
			}

			if (result == 2)
			{
				if (showCloseMessage)
				{
					showCloseMessage = false;
					EditorPrefs.SetBool("StixGames.Painter.ShowCloseMessage", false);
				}
				else
				{
					showTargetSwitchMessage = false;
					EditorPrefs.SetBool("StixGames.Painter.ShowTargetSwitchMessage", false);
				}
			}
		}

		//Always accept if message is hidden
		return true;
	}

	private void TextureSettings()
	{
		if (grassObject == null)
		{
			EditorGUILayout.LabelField("No grass object selected. Select a grass object in the scene Hierarchy.");
			return;
		}

		EditorGUILayout.LabelField("Current object: " + grassObject.name);
		EditorGUILayout.Space();

		if (grassMaterial.GetTexture("_Density") == null)
		{
			EditorGUILayout.LabelField("Create new texture", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			textureSize = EditorGUILayout.IntField("Texture Size", textureSize);

			if (GUILayout.Button("Create texture"))
			{
				//Select the path and return on cancel
				string path = EditorUtility.SaveFilePanelInProject("Create density texture", "newDensityTexture", "png",
					"Choose where to save the new density texture");

				if (path == null)
				{
					return;
				}

				//Create the new texture and save it at the selected path
				texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false, true);

				Color[] colors = new Color[textureSize * textureSize];
				for (int i = 0; i < colors.Length; i++)
				{
					colors[i] = new Color(0, 0, 0, 0);
				}

				texture.SetPixels(colors);
				texture.Apply();

				File.WriteAllBytes(path, texture.EncodeToPNG());

				//Import and load the new texture
				AssetDatabase.ImportAsset(path);
				var importer = (TextureImporter)AssetImporter.GetAtPath(path);
				importer.wrapMode = TextureWrapMode.Repeat;
				importer.isReadable = true;
				importer.textureFormat = TextureImporterFormat.ARGB32;
				AssetDatabase.ImportAsset(path);

				texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;

				//Set texture to material
				grassMaterial.SetTexture("_Density", texture);
			}

			EditorGUILayout.Space();
		}
	}

	private void BrushSettings()
	{
		EditorGUI.BeginChangeCheck();

		EditorGUILayout.LabelField("Brush settings", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Channel");
		channel = GUILayout.SelectionGrid(channel, channelLabels, 4);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Mode");
		brushMode = GUILayout.SelectionGrid(brushMode, paintModeLabels, 4);
		EditorGUILayout.EndHorizontal();

		//TODO: Make slider max value dynamically changeable by writing into field.
		//For now, just change the right value if you want more or less size
		strength = EditorGUILayout.Slider("Strength", strength, 0, 1);
		size = EditorGUILayout.Slider("Size", size, 0.01f, 50);
		softness = EditorGUILayout.Slider("Softness", softness, 0, 1);
		spacing = EditorGUILayout.Slider("Spacing", spacing, 0, 2);
		//rotation = EditorGUILayout.Slider("Rotation", rotation, 0, 360);

		if (EditorGUI.EndChangeCheck())
		{
			EditorPrefs.SetInt("StixGames.Painter.Channel", channel);
			EditorPrefs.SetInt("StixGames.Painter.BrushMode", brushMode);
			EditorPrefs.SetFloat("StixGames.Painter.Strength", strength);
			EditorPrefs.SetFloat("StixGames.Painter.Size", size);
			EditorPrefs.SetFloat("StixGames.Painter.Softness", softness);
			EditorPrefs.SetFloat("StixGames.Painter.Spacing", spacing);
			EditorPrefs.SetFloat("StixGames.Painter.Rotation", rotation);
		}

		EditorGUILayout.Space();
	}

	private void PainterSettings()
	{
		EditorGUI.BeginChangeCheck();

		EditorGUILayout.LabelField("Painter settings", EditorStyles.boldLabel);

		useUndo = EditorGUILayout.ToggleLeft("Save Undo/Redo data (may cause lag)", useUndo);

		if (EditorGUI.EndChangeCheck())
		{
			EditorPrefs.SetBool("StixGames.Painter.UseUndo", useUndo);

			if (!useUndo && texture != null)
			{
				Undo.ClearUndo(texture);
			}
		}
	}

	void OnSceneGUI(SceneView sceneView)
	{
		BlockSceneSelection();

		UpdateInput();

		//Update grass renderer
		UpdateSelectedRenderer();

		if (grassObject == null)
		{
			return;
		}

		//Calculate ray from mouse cursor
		var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		//Check if grass was hit
		RaycastHit hit;
		if (!grassCollider.Raycast(ray, out hit, Mathf.Infinity))
		{
			return;
		}

		Handles.color = new Color(1, 0, 0, 1);
		Handles.CircleCap(0, hit.point, Quaternion.LookRotation(Vector3.up), size);

		//Paint
		if (mouseDown)
		{
			float newDist = Vector3.Distance(lastPaintPos, hit.point);

			//Check draw spacing
			if (!didDraw || newDist > spacing * size)
			{
				//Draw brush
				ApplyBrush(hit.point);

				lastPaintPos = hit.point;
			}

			didDraw = true;
		}
	}

	private void BlockSceneSelection()
	{
		//Only block when a grass object is selected
		if (grassObject == null)
		{
			return;
		}

		//Disable selection in editor view, only painting will be accepted as input
		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
	}

	private void UpdateInput()
	{
		if (texture != null && Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			mouseDown = true;

			if (useUndo)
			{
				Undo.RegisterCompleteObjectUndo(texture, "Texture paint");
			}
		}
		if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
		{
			mouseDown = false;

			if (didDraw)
			{
				SaveTexture();
				didDraw = false;
			}
		}
	}

	private void UpdateSelectedRenderer()
	{
		//Return if no new object was selected
		if (Selection.activeGameObject == null)
		{
			return;
		}

		//Return if the new object is not a grass object
		var newGrassObject = Selection.activeGameObject;
		Selection.activeGameObject = null;

		if (newGrassObject == grassObject)
		{
			return;
		}

		//Get the grass material, if non was found, return.
		var newGrassMaterial = GetGrassMaterial(newGrassObject);
		if (newGrassMaterial == null)
		{
			return;
		}

		//A new object was selected. If another object was selected before, tell the user that this will make changes permanent.
		if (grassObject != null && !ShowUndoLossWarning(false))
		{
			return;
		}

		//Clear undo history for current texture
		Undo.ClearUndo(texture);

		//Reset the previously selected grass object
		ResetRenderer(false);

		//Assign new grass object
		grassObject = newGrassObject;
		grassMaterial = newGrassMaterial;
		texture = grassMaterial.GetTexture("_Density") as Texture2D;

		grassCollider = grassObject.GetComponent<Collider>();
		startedWithCollider = grassCollider != null;

		//If the object had no collider, add one. It will be destroyed when deselected.
		if (!startedWithCollider)
		{
			AddTempCollider();
		}

		Repaint();
	}

	private Material GetGrassMaterial(GameObject newGrassObject)
	{
		var renderer = newGrassObject.GetComponent<Renderer>();
		if (renderer != null && renderer.sharedMaterial != null 
			&& renderer.sharedMaterial.shader != null 
			&& renderer.sharedMaterial.shader.name == "Stix Games/Grass")
		{
			return renderer.sharedMaterial;
		}

		var terrain = newGrassObject.GetComponent<Terrain>();
		if (terrain != null && terrain.materialTemplate != null
			&& terrain.materialTemplate.shader != null
			&& terrain.materialTemplate.shader.name == "Stix Games/Grass")
		{
			return terrain.materialTemplate;
		}

		return null;
	}

	private void AddTempCollider()
	{
		if (grassObject.GetComponent<Renderer>() != null)
		{
			grassCollider = grassObject.AddComponent<MeshCollider>();
			return;
		}

		if (grassObject.GetComponent<Terrain>() != null)
		{
			grassCollider = grassObject.AddComponent<TerrainCollider>();
			return;
		}
	}

	private void ResetRenderer(bool reselectPrevious)
	{
		if (reselectPrevious && grassObject != null)
		{
			Selection.activeGameObject = grassObject;
		}

		if (grassCollider != null && !startedWithCollider)
		{
			DestroyImmediate(grassCollider);
		}

		grassObject = null;
		grassMaterial = null;
		grassCollider = null;
	}

	private void SaveTexture()
	{
		string path = AssetDatabase.GetAssetPath(texture);
		File.WriteAllBytes(path, texture.EncodeToPNG());
	}

	private void ApplyBrush(Vector3 hitPoint)
	{
		RaycastHit hit;
		Vector2 texForward, texRight;
		if (!GrassManipulationUtility.GetWorldToTextureSpaceMatrix(new Ray(hitPoint + Vector3.up * 1000, Vector3.down),
			EPSILON, grassCollider, out hit, out texForward, out texRight))
		{
			return;
		}

		Vector2 texCoord = hit.textureCoord;

		//Convert the world space radius to a pixel radius in texture space. This requires square textures.
		int pixelRadius = (int)(size * texForward.magnitude * texture.width);

		//Calculate the pixel coordinates of the point where the raycast hit the texture.
		Vector2 mid = new Vector2(texCoord.x * texture.width, texCoord.y * texture.height);

		//Calculate the pixel area where the texture will be changed
		int targetStartX = (int)(mid.x - pixelRadius);
		int targetStartY = (int)(mid.y - pixelRadius);
		int startX = Mathf.Clamp(targetStartX, 0, texture.width);
		int startY = Mathf.Clamp(targetStartY, 0, texture.height);
		int width = Mathf.Min(targetStartX + pixelRadius * 2, texture.width) - targetStartX;
		int height = Mathf.Min(targetStartY + pixelRadius * 2, texture.height) - targetStartY;

		mid -= new Vector2(startX, startY);

		//Get pixels
		Color[] pixels = texture.GetPixels(startX, startY, width, height);

		//Iterate trough all pixels
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int index = y * width + x;
				Vector2 uv = ((new Vector2(x, y) - mid) / pixelRadius) * 0.5f + new Vector2(0.5f, 0.5f);
				pixels[index] = ApplyBrushToPixel(pixels[index], uv);
			}
		}

		//Save pixels and apply them to the texture
		texture.SetPixels(startX, startY, width, height, pixels);
		texture.Apply();

		EditorUtility.SetDirty(texture);
	}

	private Color ApplyBrushToPixel(Color c, Vector2 v)
	{
		v -= new Vector2(0.5f, 0.5f);
		v *= 2;
		float distance = v.magnitude;

		//Calculate brush smoothness
		float value = SmoothStep(1, Mathf.Min(1 - softness, 0.999f), distance);

		switch (brushMode)
		{
			case 0:
				c = ChangeChannel(c, x => x + value * strength);
				break;

			case 1:
				c = ChangeChannel(c, x => x - value * strength);
				break;

			case 2:
				c = ChangeChannel(c, x => x * (1 - value) + strength * value);
				break;
		}

		return c;
	}

	private static Color ChangeChannel(Color c, Func<float, float> densityChange)
	{
		switch (channel)
		{
			case 0:
				c.r = Mathf.Clamp01(densityChange(c.r));
				break;
			case 1:
				c.g = Mathf.Clamp01(densityChange(c.g));
				break;
			case 2:
				c.b = Mathf.Clamp01(densityChange(c.b));
				break;
			case 3:
				c.a = Mathf.Clamp01(densityChange(c.a));
				break;
			default:
				throw new InvalidOperationException("Channel number invalid");
		}
		return c;
	}

	//Taken from https://en.wikipedia.org/wiki/Smoothstep
	private float SmoothStep(float edge0, float edge1, float x)
	{
		// Scale, bias and saturate x to 0..1 range
		x = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
		// Evaluate polynomial
		return x * x * (3 - 2 * x);
	}
}
