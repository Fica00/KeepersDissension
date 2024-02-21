using System.Linq;

public class Teleport : AbilityEffect
{
    public override void ActivateForOwner()
    {
        MoveToActivationField();
        GameplayState _state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        LifeForce _lifeForce = FindObjectsOfType<LifeForce>().ToList().Find(_lifeForce => _lifeForce.My);
        Keeper _keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        GameplayManager.Instance.SelectPlaceForSpecialAbility(_lifeForce.GetTablePlace().Id,1,PlaceLookFor.Empty, CardMovementType
        .EightDirections, false,LookForCardOwner.Both, Teleport);

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