//Stylized Water Shader by Jonathan3D http://u3d.as/A2R

using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(StylizedWater))]
public class StylizedWaterInspector : Editor
{
    string shaderName = null;

    //Non serialized, local
    StylizedWater stylizedWater;
    Material material;
    ProceduralMaterial substance;

    //Shader variables
    #region Shader parameters
    public Color waterColor;
    public Color fresnelColor;
    public Color rimColor;

    public float transparency;
    public float glossiness;
    public float fresnel;

    public float surfaceHighlightTiling;

    public float depth;
    public float depthDarkness;

    public float rimSize;
    public float rimFalloff;

    public bool worldSpaceTiling;
    public float tiling;
    public float rimTiling;

    public float refractionAmount;

    public float waveSpeed;
    public float waveStrength;

    public Texture normals;
    public Texture shadermap;

    public Cubemap reflection;

    public float tesselation;
    #endregion

    #region  Substance parameters
    public float seed;
    public float normalStrength;
    public string[] intersectionStyleList;
    public string[] waveStyleList;
    public float surfaceHighlight;
    public float surfaceHighlightSize;
    public int intersectionStyle;
    public int waveStyle;
    public bool useCustomNormals;
    public bool useCustomIntersection;
    #endregion

    //local variables
    Texture customIntersection;
    Texture customNormal;
    Texture reflectionCubemap;

    GameObject selected;
    public bool isMobileAdvanced;
    public bool isMobileBasic;

    //Toggle bools
    bool showColors = true;
    bool showSurface = true;
    bool showIntersection;
    bool showHighlights;
    bool showDepth;
    bool showWaves;

    bool hasSubstanceParams = false;

    Object[] undoObjects = new Object[2];

    //Styling
    GUIStyle groupFoldout;

    void OnEnable()
    {
        setStyling();
        selected = Selection.activeGameObject;

        getProperties();

    }  

    public override void OnInspectorGUI()
    {

        if (selected && substance)
        {
            undoObjects[0] = substance;
            undoObjects[1] = material;
        }

        EditorGUI.BeginChangeCheck();
        if (undoObjects != null)
        {
            Undo.RecordObjects(undoObjects, stylizedWater.name);
        }

        /*-----------------*/
            drawFields();
        /*-----------------*/

        //Apply values
        if (EditorGUI.EndChangeCheck())
        {
            setScriptValues(stylizedWater); //Passes the current Substance to component

            //Shader
            setShaderValues();

            //Substance
            setSubstanceParameters();

            //Changes applied, grab new values
            getProperties();

            EditorUtility.SetDirty(this);
        }


    }

    //Get shader and Substance values
    void getProperties()
    {
        stylizedWater = selected.GetComponent<StylizedWater>();

        //Get shader and substance properties

        stylizedWater.getProperties();

        material = stylizedWater.material;
        substance = stylizedWater.substance;

        //Meta
        showColors = stylizedWater.showColors;
        showSurface = stylizedWater.showSurface;
        showIntersection = stylizedWater.showIntersection;
        showHighlights = stylizedWater.showHighlights;
        showDepth = stylizedWater.showDepth;
        showWaves = stylizedWater.showWaves;

        isMobileAdvanced = stylizedWater.isMobileAdvanced;
        isMobileBasic = stylizedWater.isMobileBasic;


        shaderName = stylizedWater.shaderName;

        #region Shader parameters
        waterColor = stylizedWater.waterColor;
        fresnelColor = stylizedWater.fresnelColor;
        rimColor = stylizedWater.rimColor;

        transparency = stylizedWater.transparency;
        glossiness = stylizedWater.glossiness;
        reflectionCubemap = stylizedWater.reflectionCubemap;
        fresnel = stylizedWater.fresnel;

        surfaceHighlight = stylizedWater.surfaceHighlight;
        surfaceHighlightSize = stylizedWater.surfaceHighlightSize;
        surfaceHighlightTiling = stylizedWater.surfaceHighlightTiling;

        depth = stylizedWater.depth;
        depthDarkness = stylizedWater.depthDarkness;

        rimSize = stylizedWater.rimSize;
        rimFalloff = stylizedWater.rimFalloff;

        worldSpaceTiling = stylizedWater.worldSpaceTiling;
        tiling = stylizedWater.tiling;
        rimTiling = stylizedWater.rimTiling;

        refractionAmount = stylizedWater.refractionAmount;

        waveSpeed = stylizedWater.waveSpeed;
        waveStrength = stylizedWater.waveStrength;

        tesselation = stylizedWater.tesselation;
        #endregion

        #region Substance parameters
        seed = stylizedWater.seed;
        normalStrength = stylizedWater.normalStrength;
        intersectionStyleList = stylizedWater.intersectionStyleList;
        intersectionStyle = stylizedWater.intersectionStyle;
        waveStyle = stylizedWater.waveStyle;
        waveStyleList = stylizedWater.waveStyleList;

        useCustomNormals = stylizedWater.useCustomNormals;
        useCustomIntersection = stylizedWater.useCustomIntersection;

        customIntersection = stylizedWater.customIntersection;
        customNormal = stylizedWater.customNormal;

        normals = stylizedWater.normals;
        shadermap = stylizedWater.shadermap;

        //Substance values are not null, safe to apply
        hasSubstanceParams = stylizedWater.hasSubstanceParams; //true if has Substance
        #endregion

        setSubstanceParameters(); //Go back and re-run this function now that hasSubstanceParams is True

    }

    void drawFields()
    {
        //GUILayout.Label(bannerImage, GUILayout.ExpandWidth(true));
        //EditorGUI.DrawTextureAlpha(EditorGUILayout.RectField(new Rect(0, 0, 383, 91)), bannerImage, ScaleMode.StretchToFill);

        if (material == null || !shaderName.Contains("StylizedWater"))
        {
            EditorGUILayout.HelpBox("Please assign a \"StylizedWater\" shader to the current material", MessageType.Error);
        }
        else
        {
            EditorGUILayout.LabelField("Current shader: ", shaderName, EditorStyles.boldLabel);
        }

        #region Substance
        ProceduralMaterial mSubstance = substance;
        substance = (ProceduralMaterial)EditorGUILayout.ObjectField("Substance", substance, typeof(ProceduralMaterial), true);

        if (mSubstance != substance) {
            //Debug.Log("Substance has changed: " + mSubstance.name + " > " + substance.name);
            hasSubstanceParams = false;
        }

        if (!substance)
        {
            EditorGUILayout.HelpBox("Please assign a Substance material instance", MessageType.Error);
        }

        //If conditions aren't met, don't display fields
        if (!substance || !shaderName.Contains("StylizedWater")) return;


        EditorGUILayout.BeginHorizontal();

        seed = EditorGUILayout.FloatField("Seed", seed);
        if (GUILayout.Button("Randomize", EditorStyles.miniButton))
        {
            seed = Random.Range(1, 9999);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        #endregion

        #region Colors
        if (GUILayout.Button((showColors) ? "[-] Colors" : "[+] Colors", groupFoldout))
        {
            showColors = !showColors;
        }
        if (showColors)
        {
            waterColor = EditorGUILayout.ColorField("Water color", waterColor);
            if (!isMobileBasic)
            {
                fresnelColor = EditorGUILayout.ColorField("Fresnel color", fresnelColor);
            }
            rimColor = EditorGUILayout.ColorField("Rim color", rimColor);
        }

        EditorGUILayout.Space();
        #endregion

        #region Surface
        if (GUILayout.Button((showSurface) ? "[-] Surface" : "[+] Surface", groupFoldout))
        {
            showSurface = !showSurface;
        }
        if (showSurface)
        {
            if (!useCustomNormals)
            {
                normalStrength = EditorGUILayout.Slider("Normal strength", normalStrength, 0f, 32f);
            }
            worldSpaceTiling = EditorGUILayout.Toggle("World-space tiling", worldSpaceTiling);
            tiling = EditorGUILayout.Slider("Tiling", tiling, 0f, 1f);

            if (!isMobileBasic)
            {
                transparency = EditorGUILayout.Slider("Transparency", transparency, 0f, 1f);
                fresnel = EditorGUILayout.Slider("Fresnel", fresnel, 0f, 10f);
            }
            glossiness = EditorGUILayout.Slider("Glossiness", glossiness, 0f, 1f);


            if (!isMobileAdvanced && !isMobileBasic)
            {
                refractionAmount = EditorGUILayout.Slider("Refraction", refractionAmount, 0f, 0.2f);
                reflectionCubemap = (Texture)EditorGUILayout.ObjectField("Reflection cubemap", reflectionCubemap, typeof(Cubemap), false);
            }

        }

        EditorGUILayout.Space();
        #endregion

        #region Intersection
        if (GUILayout.Button((showIntersection) ? "[-] Intersection" : "[+] Intersection", groupFoldout))
        {
            showIntersection = !showIntersection;
        }
        if (showIntersection)
        {

            useCustomIntersection = EditorGUILayout.BeginToggleGroup("Use custom texture", useCustomIntersection);
            if (useCustomIntersection)
            {

                customIntersection = (Texture)EditorGUILayout.ObjectField("Grayscale texture", customIntersection, typeof(Texture2D), false);
                if (customIntersection == null)
                {
                    EditorGUILayout.HelpBox("Texture cannot be empty", MessageType.Warning);
                }

            }

            EditorGUILayout.EndToggleGroup();


            if (!useCustomIntersection)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Style");
                intersectionStyle = EditorGUILayout.Popup(intersectionStyle, intersectionStyleList, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();
            }


            rimSize = EditorGUILayout.Slider("Size", rimSize, 0f, 10f);
            rimFalloff = EditorGUILayout.Slider("Falloff", rimFalloff, 0f, 5f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tiling");
            if (GUILayout.Button("<<", EditorStyles.miniButtonLeft))
            {
                rimTiling -= .5f;
            }
            if (GUILayout.Button("<", EditorStyles.miniButton))
            {
                rimTiling -= .1f;
            }
            rimTiling = EditorGUILayout.FloatField(rimTiling, GUILayout.MaxWidth(25));
            if (GUILayout.Button(">", EditorStyles.miniButton))
            {
                rimTiling += .1f;
            }
            if (GUILayout.Button(">>", EditorStyles.miniButtonRight))
            {
                rimTiling += .5f;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        #endregion

        #region Surface highlights
        if (!isMobileBasic)
        {
            if (GUILayout.Button((showHighlights) ? "[-] Surface highlights" : "[+] Surface highlights", groupFoldout))
            {
                showHighlights = !showHighlights;
            }
            if (showHighlights)
            {

                surfaceHighlight = EditorGUILayout.Slider("Opacity", surfaceHighlight, 0f, 1f);
                surfaceHighlightSize = EditorGUILayout.Slider("Size", surfaceHighlightSize, 0f, 1f);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tiling");
                if (GUILayout.Button("<<", EditorStyles.miniButtonLeft))
                {
                    surfaceHighlightTiling -= .5f;
                }
                if (GUILayout.Button("<", EditorStyles.miniButton))
                {
                    surfaceHighlightTiling -= .1f;
                }
                surfaceHighlightTiling = EditorGUILayout.FloatField(surfaceHighlightTiling, GUILayout.MaxWidth(25));
                if (GUILayout.Button(">", EditorStyles.miniButton))
                {
                    surfaceHighlightTiling += .1f;
                }
                if (GUILayout.Button(">>", EditorStyles.miniButtonRight))
                {
                    surfaceHighlightTiling += .5f;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }//not mobile basic
        #endregion

        #region Depth
        if (!isMobileBasic)
        {
            if (GUILayout.Button((showDepth) ? "[-] Depth" : "[+] Depth", groupFoldout))
            {
                showDepth = !showDepth;
            }
            if (showDepth)
            {
                depth = EditorGUILayout.Slider("Depth", depth, 0f, 30f);
                depthDarkness = EditorGUILayout.Slider("Darkness", depthDarkness, 0f, 1f);
            }

            EditorGUILayout.Space();
        }
        #endregion

        #region Waves
        if (GUILayout.Button((showWaves) ? "[-] Waves" : "[+] Waves", groupFoldout))
        {
            showWaves = !showWaves;
        }
        if (showWaves)
        {

            useCustomNormals = EditorGUILayout.BeginToggleGroup("Use custom normal map", useCustomNormals);
            if (useCustomNormals)
            {
                customNormal = (Texture)EditorGUILayout.ObjectField("Normal map", customNormal, typeof(Texture2D), false);
                if (customNormal == null)
                {
                    EditorGUILayout.HelpBox("Texture cannot be empty", MessageType.Warning);
                }
            }


            EditorGUILayout.EndToggleGroup();

            if (!useCustomNormals)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Style");
                waveStyle = EditorGUILayout.Popup(waveStyle, waveStyleList, EditorStyles.toolbarDropDown);
                EditorGUILayout.EndHorizontal();
            }

            waveSpeed = EditorGUILayout.Slider("Speed", waveSpeed, 0f, 10f);
            if (!isMobileAdvanced && !isMobileBasic)
            {
                waveStrength = EditorGUILayout.Slider("Strength", waveStrength, 0f, 1f);
            }
        }

        EditorGUILayout.Space();
        #endregion

        if (shaderName.Contains("Tesselation"))
        {
            tesselation = EditorGUILayout.Slider("Tesselation", tesselation, 0.1f, 10f);
        }
    }

    //Apply values to material shader
    private void setShaderValues()
    {
        if (!material) return;

        //Excluded from mobile shaders
        if (!isMobileAdvanced || !isMobileBasic)
        {
            material.SetFloat("_Transparency", transparency);
            material.SetFloat("_RefractionAmount", refractionAmount);
            material.SetFloat("_Wavesstrength", waveStrength);

            material.SetTexture("_Reflection", reflectionCubemap);
        }

        //Excluded from Mobile Basic, but shared by Mobile Advanced and Desktop
        if (!isMobileBasic)
        {
            material.SetColor("_FresnelColor", fresnelColor);

            material.SetFloat("_Fresnelexponent", fresnel);

            material.SetFloat("_SurfaceHighlight", surfaceHighlight);
            material.SetFloat("_SurfaceHightlighttiling", surfaceHighlightTiling);
            material.SetFloat("_Surfacehightlightsize", surfaceHighlightSize);

            material.SetFloat("_Depth", depth);
            material.SetFloat("_Depthdarkness", depthDarkness);
        }

        if (worldSpaceTiling == true)
        {
            material.SetFloat("_Worldspacetiling", 1);
        }
        else
        {
            material.SetFloat("_Worldspacetiling", 0);
        }

        material.SetColor("_WaterColor", waterColor);

        material.SetColor("_RimColor", rimColor);

        material.SetFloat("_Glossiness", glossiness);

        material.SetFloat("_RimSize", rimSize);
        material.SetFloat("_Rimfalloff", rimFalloff);

        material.SetFloat("_Wavesspeed", waveSpeed);

        material.SetFloat("_Tiling", tiling);
        material.SetFloat("_Rimtiling", rimTiling);

        if (shaderName == "StylizedWater/Desktop (DX11 Tesselation)")
        {
            material.SetFloat("_Tesselation", tesselation);
        }


    }

    //Apply values to Substance material
    private void setSubstanceParameters()
    {
        if (!substance || !hasSubstanceParams) return; //Prevent from setting null values

        //Debug.Log("Setting value " + intersectionStyleList[intersectionStyle] + " to " + substance.name);

        substance.SetProceduralFloat("$randomseed", seed);
        substance.SetProceduralFloat("normalStrength", normalStrength);

        if (useCustomIntersection)
        {
            substance.SetProceduralBoolean("useCustomIntersection", useCustomIntersection);
            if (customIntersection)
            {
                substance.SetProceduralTexture("customIntersectionTex", (Texture2D)customIntersection);
            }
            stylizedWater.customIntersection = customIntersection;
        }
        else
        {
            substance.SetProceduralBoolean("useCustomIntersection", false);
            substance.SetProceduralTexture("customIntersectionTex", null);
        }

        if (useCustomNormals)
        {
            substance.SetProceduralBoolean("useCustomNormal", useCustomNormals);
            if (customNormal)
            {
                substance.SetProceduralTexture("customNormalTex", (Texture2D)customNormal);
            }
            stylizedWater.customNormal = customNormal;
        }
        else
        {
            substance.SetProceduralBoolean("useCustomNormal", false);
            substance.SetProceduralTexture("customNormalTex", null);
        }

        substance.SetProceduralEnum("intersectionStyle", intersectionStyle);
        substance.SetProceduralEnum("waveStyle", waveStyle);


        //Excecution order requires this check
        if (shadermap && normals)
        {
            //Debug.Log("SET "+ substance.name + " shadermap: " + shadermap);
            material.SetTexture("_Shadermap", shadermap);
            material.SetTexture("_Normals", normals);
        }
        else { Debug.LogError("Shadermap & normal are null"); }

        if (!substance.isProcessing) substance.RebuildTexturesImmediately();
    }

    //Saving meta data
    private void setScriptValues(StylizedWater sws)
    {
        if (!sws) return;

        sws.showColors = showColors;
        sws.showSurface = showSurface;
        sws.showIntersection = showIntersection;
        sws.showHighlights = showHighlights;
        sws.showDepth = showDepth;
        sws.showWaves = showWaves;

        sws.substance = substance;
        sws.hasSubstanceParams = hasSubstanceParams; //If Substance changed, is set to false
    }

    void setStyling()
    {
        groupFoldout = new GUIStyle(EditorStyles.toolbarDropDown);

        groupFoldout.fontSize = 11;
        groupFoldout.fixedHeight = 22;

        RectOffset groupFoldoutPadding = new RectOffset();
        groupFoldoutPadding.left = 10;
        groupFoldoutPadding.top = 4;
        groupFoldoutPadding.bottom = 5;
        groupFoldout.padding = groupFoldoutPadding;
    }

}
#endif
