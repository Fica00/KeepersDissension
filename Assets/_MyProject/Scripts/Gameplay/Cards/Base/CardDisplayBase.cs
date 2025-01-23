using System;
using UnityEngine;

public class CardDisplayBase : MonoBehaviour
{
    [SerializeField] private GameObject hideObj;
    public virtual void Setup(Card _card)
    {
        throw new Exception("Setup card must be implemented");
    }

    public virtual void Setup(AbilityCard _ability)
    {
        throw new Exception("Setup card ability must be implemented");
    }

    public virtual bool ChangeSprite(Sprite _sprite)
    {
        throw new Exception("Change sprite must be implemented");
    }

    public virtual void ManageNameDisplay(bool _status)
    {
        throw new Exception();
    }

    public void Hide()
    {
        hideObj.SetActive(true);
    }

    public void UnHide()
    {
        hideObj.SetActive(false);
    }

    public virtual void ShowWhiteBox()
    {
        
    }
}
