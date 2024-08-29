using System.Linq;
using UnityEngine;

public class Teleport : AbilityEffect
{
    public override void ActivateForOwner()
    {
        MoveToActivationField();
        GameplayState _state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
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
                            FirstCardId = _keeper.Details.Id,
                            FinishingPlaceId = _keeper.GetTablePlace().Id,
                            SecondCardId = _keeper.Details.Id,
                            Type = CardActionType.Attack,
                            Cost = 0,
                            IsMy = true,
                            CanTransferLoot = false,
                            Damage = 1,
                            CanCounter = false
                        };
                        GameplayManager.Instance.ExecuteCardAction(_damageSelf);
                        GameplayManager.Instance.GameState = _state;
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
                UIManager.Instance.ShowOkDialog("There are no empty spaces around Life Force");
                GameplayManager.Instance.GameState = _state;
                RemoveAction();
                OnActivated?.Invoke();
                return;
            }

            CardAction _teleportAction = new CardAction
            {
                StartingPlaceId = _keeper.GetTablePlace().Id,
                FirstCardId = _keeper.Details.Id,
                FinishingPlaceId = _placeId,
                SecondCardId = 0,
                Type = CardActionType.Move,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = 0,
                CanCounter = false
            };
            
            GameplayManager.Instance.ExecuteCardAction(_teleportAction);
            GameplayManager.Instance.GameState = _state;
            RemoveAction();
            OnActivated?.Invoke();
        }
    }
}