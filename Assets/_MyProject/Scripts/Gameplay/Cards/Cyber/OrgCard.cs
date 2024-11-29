using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrgCard : CardSpecialAbility
{
    private void Start()
    {
        Card.Stats.Damage = 2;
    }

    public override void UseAbility()
    {
        if (Player.Actions<=0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have enough actions");
            return;
        }

        if (!GameplayManager.Instance.MyTurn)
        {
            return;
        }
        
        DialogsManager.Instance.ShowYesNoDialog("Use orges Groundpound  attack?",YesUseGroundpoundAttack);
    }

    private void YesUseGroundpoundAttack()
    {
        if (Card.Details.Faction.IsCyber)
        {
            GameplayManager.Instance.PlayAudioOnBoth("AllAroundMePain", Card);
        }
        else if (Card.Details.Faction.IsDragon)
        {
            GameplayManager.Instance.PlayAudioOnBoth("FeelTheGroundShake", Card);
        }
        else if (Card.Details.Faction.IsForest)
        {
            GameplayManager.Instance.PlayAudioOnBoth("FeelTheQuake", Card);
        }
        else if (Card.Details.Faction.IsSnow)
        {
            GameplayManager.Instance.PlayAudioOnBoth("PainIsComing", Card);
        }
        Player.Actions--;
        AOEAttack(Card.GetTablePlace().Id);
    }
    

    public void AOEAttack(int _attackingPlaceId)
    {
        List<TablePlaceHandler> _availablePlaces =
            GameplayManager.Instance.TableHandler.GetPlacesAround(_attackingPlaceId,
                CardMovementType.EightDirections);

        List<Wall> _attackedWalls = new List<Wall>();
        bool _atckedDragonWall = false;
        foreach (var _availablePlace in _availablePlaces.ToList())
        {
            if (!_availablePlace.IsOccupied)
            {
                _availablePlaces.Remove(_availablePlace);
                continue;
            }
            
            CardBase _cardOnPlace = _availablePlace.GetCard();

            if (!(_cardOnPlace != null))
            {
                _availablePlaces.Remove(_availablePlace);
                continue;
            }
            
            CardBase _wallBase = _availablePlace.GetWall();
            if (_wallBase != null)
            {
                Wall _wall = _wallBase as Wall;
                if (_wall.Details.Faction.IsCyber)
                {
                    foreach (var _effect in _wall.SpecialAbilities)
                    {
                        if (_effect is WallBase)
                        {
                            _attackedWalls.Add(_wall);
                        }
                    }
                }
                else if (_wall.Details.Faction.IsDragon)
                {
                    _atckedDragonWall = true;
                }
            }
        }


        Card _attackingCard = GameplayManager.Instance.TableHandler.GetPlace(_attackingPlaceId).GetCard();
        if (_atckedDragonWall)
        {
            StartCoroutine(AttackSelf());
        }

        foreach (var _availablePlace in _availablePlaces.ToList())
        {
            if (!_availablePlace.IsOccupied)
            {
                _availablePlaces.Remove(_availablePlace);
                continue;
            }

            CardBase _cardOnPlace = _availablePlace.GetCard();

            if (!(_cardOnPlace != null))
            {
                _availablePlaces.Remove(_availablePlace);
                continue;
            }

            CardAction _additionalAction = new CardAction()
            {
                StartingPlaceId = _attackingCard.GetTablePlace().Id,
                FirstCardId = _attackingCard.Details.Id,
                FinishingPlaceId = _availablePlace.Id,
                SecondCardId = GameplayManager.Instance.TableHandler.GetPlace(_availablePlace.Id).GetCard().Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = false,
                IsMy = true,
                CanCounter = true,
                Damage = (int)Card.Stats.Damage
            };
            

            GameplayManager.Instance.ExecuteCardAction(_additionalAction);
        }

        StartCoroutine(DelayPush());

        IEnumerator DelayPush()
        {
            if (Immunity.IsActiveForMe)
            {
                yield break;
            }
            yield return new WaitForSeconds(1);
            if (_attackingCard.GetTablePlace()==null)
            {
                yield break;
            }
            if (_attackedWalls.Count==1)
            {
                GameplayManager.Instance.PushCardBack(_attackingCard.GetTablePlace().Id, _attackedWalls[0]
                .GetTablePlace().Id);
            }
            else if(_attackedWalls.Count>1)
            {
                GameplayManager.Instance.PushCardBack(_attackingCard.GetTablePlace().Id, _attackingPlaceId+7);
            }
        }

        IEnumerator AttackSelf()
        {
            if (Immunity.IsActiveForMe)
            {
                yield break;
            }
            yield return new WaitForSeconds(1.5f);
            if (Card==null || _attackingCard == null|| _attackingCard.GetTablePlace()==null)
            {
                yield break;
            }
            CardAction _destroySelf = new CardAction
            {
                StartingPlaceId = _attackingCard.GetTablePlace().Id,
                FirstCardId = _attackingCard.Details.Id,
                FinishingPlaceId = _attackingCard.GetTablePlace().Id,
                SecondCardId = _attackingCard.Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = (int)Card.Stats.Damage,
                CanCounter = false,
                GiveLoot = false
            };
            GameplayManager.Instance.ExecuteCardAction(_destroySelf);
        }
    }
}
