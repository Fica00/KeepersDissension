using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VerticalLayoutGroupRebuild : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    private void Start()
    {
        StartCoroutine(RebuildRoutine());
    }

    private IEnumerator RebuildRoutine()
    {
        while (gameObject.activeSelf)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);
            yield return new WaitForSeconds(3f);
        }
    }
}
