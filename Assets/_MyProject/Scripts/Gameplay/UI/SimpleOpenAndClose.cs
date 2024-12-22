using UnityEngine;
using UnityEngine.UI;

public class SimpleOpenAndClose : MonoBehaviour
{
    [SerializeField] private GameObject object1;
    [SerializeField] private GameObject object2;
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OpenObject);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OpenObject);
    }

    private void OpenObject()
    {
        object1.transform.localScale = Vector3.zero;
        object2.transform.localScale = Vector3.one;
        var _actions = object2.GetComponent<ElipsisActionsController>();
        if (_actions)
        {
            _actions.Setup();
        }
    }

    public void CloseObject()
    {
        object1.transform.localScale = Vector3.one;
        object2.transform.localScale = Vector3.zero;
    }
}
