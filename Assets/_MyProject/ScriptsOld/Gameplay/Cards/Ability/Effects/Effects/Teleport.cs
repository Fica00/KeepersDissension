using System.Linq;
using UnityEngine;

public class Teleport : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        LifeForce _lifeForce = FindObjectsOfType<LifeForce>().ToList().Find(_lifeForce => _lifeForce.My);
        Keeper _keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        int _range = 1;
        GetPlace();

        void GetPlace()
        {
            GameplayManager.Instance.SelectPlaceForSpecialAbility(_lifeForce.GetTablePlace().Id,_range,PlaceLookFor.Empty, CardMovementType
                .EightDirections, false,LookForCardOwner.Both, (_placeId) =>
            {
                if (_placeId==-1)
                {
                    _range++;
                    Debug.Log(_range);
                    if(_range>10)
                    {
                        Debug.Log("1111111");
                        Debug.Log(_keeper,_keeper.GetTablePlace().gameObject);
                        CardAction _damageSelf = new CardAction()
                        {
                            StartingPlaceId = _keeper.GetTablePlace().Id,
                            FirstCardId = _keeper.UniqueId,
                            FinishingPlaceId = _keeper.GetTablePlace().Id,
                            SecondCardId = _keeper.UniqueId,
                            Type = CardActionType.Attack,
                            Cost = 0,
                            CanTransferLoot = false,
                            Damage = 1,
                            CanCounter = false
                        };
                        GameplayManager.Instance.ExecuteCardAction(_damageSelf);
                        RemoveAction();
                        OnActivated?.Invoke();
                        return;
                    }
                    GetPlace();
                }
                else
                {
                    Teleport(_placeId);
                }
            });
        }

        void Teleport(int _placeId)
        {
            if (_placeId==-1)
            {
                DialogsManager.Instance.ShowOkDialog("There are no empty spaces around Life Force");
                RemoveAction();
                OnActivated?.Invoke();
                return;
            }

            CardAction _teleportAction = new CardAction
            {
                StartingPlaceId = _keeper.GetTablePlace().Id,
                FirstCardId = _keeper.UniqueId,
                FinishingPlaceId = _placeId,
                Type = CardActionType.Move,
                Cost = 0,
                CanTransferLoot = false,
                Damage = 0,
                CanCounter = false
            };
            
            GameplayManager.Instance.ExecuteCardAction(_teleportAction);
            RemoveAction();
            OnActivated?.Invoke();
        }
    }
}