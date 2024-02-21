using System;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCardImagePanel : MonoBehaviour
{
   public static Action OnShowed;
   public static Action OnClosed;
   public static ChooseCardImagePanel Instance;
   [SerializeField] private ChooseCardImageDisplay displayPrefab;
   [SerializeField] private Transform displaysHolder;
   [SerializeField] private GameObject holder;

   private Action<CardBase> callBack;
   private List<GameObject> shownObjects = new();
   
   private void Awake()
   {
      Instance = this;
   }

   private void OnEnable()
   {
      ChooseCardImageDisplay.OnSelected += Select;
   }

   private void OnDisable()
   {      
      ChooseCardImageDisplay.OnSelected -= Select;
   }

   private void Select(CardBase _ability)
   {
      callBack?.Invoke(_ability);
      Close();
   }

   public void Show(List<CardBase> _abilities, Action<CardBase> _callBack)
   {
      if (_abilities==null||_abilities.Count==0)
      {
         _callBack?.Invoke(null);
         return;
      }
      
      OnShowed?.Invoke();
      ClearShownObjects();
      callBack = _callBack;
      foreach (var _ability in _abilities)
      {
         ChooseCardImageDisplay _display = Instantiate(displayPrefab, displaysHolder);
         _display.Setup(_ability);
         shownObjects.Add(_display.gameObject);
      }
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
