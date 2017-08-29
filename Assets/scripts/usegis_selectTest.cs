using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;


public class usegis_selectTest : MonoBehaviour {
    private VectorLine selectionLine;
    private Vector2 originalPos;
    private List<Color32> lineColors;

	// Use this for initialization
	void Start () {
        lineColors = new List<Color32> (new Color32[4]);
        selectionLine = new VectorLine("Selection", new List<Vector2>(5), null, 3.0f, LineType.Continuous);
        selectionLine.capLength = 1.5f;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            StopCoroutine("CycleColor");
            selectionLine.SetColor(Color.white);
            originalPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            selectionLine.MakeRect(originalPos, Input.mousePosition);
            selectionLine.Draw();
        }
        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine("CycleColor");
            Rect rc = utils.CreateRectFromPoints(selectionLine.points2);
            Vector2[] arr = new Vector2[2];
            arr[0] = rc.min;
            arr[1] = rc.max;
            SendMessage("Find", arr, SendMessageOptions.RequireReceiver);

        }
    }

    IEnumerator CycleColor()
    {
        while (true)
        {
            for (var i = 0; i < 4; i++)
            {
                lineColors[i] = Color.Lerp(Color.yellow, Color.red, Mathf.PingPong((Time.time + i *  0.25f) * 3.0f, 1.0f));
            }
            selectionLine.SetColors(lineColors);
            yield return null;
        }
    }
}
