using UnityEngine;
using UnityEngine.UI;

public class UILineConnector : MonoBehaviour
{
    public RectTransform point1; // Assign the RectTransform of the first UI element
    public RectTransform point2; // Assign the RectTransform of the second UI element
    public Image lineImage; // Assign a reference to an Image component for the line

    private void Update()
    {
        if (point1 != null && point2 != null && lineImage != null)
        {
            Vector3[] linePositions = new Vector3[2];
            linePositions[0] = point1.position;
            linePositions[1] = point2.position;

            lineImage.rectTransform.position = (linePositions[0] + linePositions[1]) / 2f;
            lineImage.rectTransform.sizeDelta = new Vector2(Vector3.Distance(linePositions[0], linePositions[1]), lineImage.rectTransform.sizeDelta.y);
            lineImage.rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(linePositions[1].y - linePositions[0].y, linePositions[1].x - linePositions[0].x) * Mathf.Rad2Deg);

            lineImage.enabled = true;
        }
        else
        {
            lineImage.enabled = false;
        }
    }
}
