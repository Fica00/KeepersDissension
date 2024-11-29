using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCardImagePanel : MonoBehaviour
{
   public static Action OnShowed;
   public static Action OnClosed;
   public static ChooseCardImagePanel Instance;
   [SerializeField] private ChooseCardImageDisplay displayPrefab;
   [SerializeField] private Transform displaysHolder;
   [SerializeField] private GameObject holder;
   [SerializeField] private Button close;
   [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;

   private Action<CardBase> callBack;
   private List<GameObject> shownObjects = new();
   
   private void Awake()
   {
      Instance = this;
   }

   private void OnEnable()
   {
      ChooseCardImageDisplay.OnSelected += Select;
      close.onClick.AddListener(Cancel);
   }

   private void OnDisable()
   {      
      ChooseCardImageDisplay.OnSelected -= Select;
      close.onClick.RemoveListener(Cancel);
   }

   private void Cancel()
   {
      Select(null);
   }

   private void Select(CardBase _ability)
   {
      callBack?.Invoke(_ability);
      Close();
   }

   public void Show(List<CardBase> _abilities, Action<CardBase> _callBack, bool _allowCancel = false, bool _isMinion = false)
   {
      if (_abilities==null||_abilities.Count==0)
      {
         _callBack?.Invoke(null);
         return;
      }

      close.gameObject.SetActive(_allowCancel);
      
      OnShowed?.Invoke();
      ClearShownObjects();
      callBack = _callBack;
      foreach (var _ability in _abilities)
      {
         ChooseCardImageDisplay _display = Instantiate(displayPrefab, displaysHolder);
         _display.Setup(_ability);
         RectTransform _rect = _display.GetComponent<RectTransform>();
         _rect.sizeDelta = _isMinion ? new Vector2(245,355) : new Vector2(355,245);
         Vector3 _rotation = _rect.eulerAngles;
         _rotation.z = _isMinion ? 180 : _rotation.z;
         _rect.eulerAngles = _rotation;
         shownObjects.Add(_display.gameObject);
      }

      horizontalLayoutGroup.padding.top = _isMinion ? 0 : 33;
      holder.SetActive(true);
   }

   private void ClearShownObjects()
   {
      foreach (var _shownObject in shownObjects)
      {
         Destroy(_shownObject);
      }
      
      shownObjects.Clear();
   }

   private void Close()
   {
      OnClosed?.Invoke();
      holder.SetActive(false);
   }
}
