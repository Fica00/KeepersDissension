public class BlockaderRam : CardSpecialAbility
{
    public void TryAndPush(string _firstCardId, string _secondCardId)
    {
        TryToPlaySoundEffect();
        if (CanPushCard(_firstCardId, _secondCardId))
        {
            PushCard(_secondCardId);
            TryToMoveSelf();
        }
        else
        {
            GameplayManager.Instance.DamageCardByAbility(_secondCardId, 1, TryToMoveSelf);
        }
        
        GameplayManager.Instance.MyPlayer.Actions--;
    }

    private void TryToPlaySoundEffect()
    {
        if (Card is Keeper) return;

        if (Card.Details.Faction.IsCyber)
        {
            GameplayManager.Instance.PlayAudioOnBoth("DontMindMe", Card);
        }
        else if (Card.Details.Faction.IsDragon)
        {
            GameplayManager.Instance.PlayAudioOnBoth("YouAppearToBeInMyWay", Card);
        }
        else if (Card.Details.Faction.IsForest)
        {
            GameplayManager.Instance.PlayAudioOnBoth("IAskedNicely", Card);
        }
        else if (Card.Details.Faction.IsSnow)
        {
            GameplayManager.Instance.PlayAudioOnBoth("OutOfMyWay", Card);
        }
    }

    private bool CanPushCard(string _firstCardId, string _secondCardId)
    {
        Card _secondCard = GameplayManager.Instance.GetCard(_secondCardId);
        
        if (!_secondCard.CheckCanMove())
        {
            return false;
        }
        
        int _firstCardPlace = GameplayManager.Instance.GetCard(_firstCardId).GetTablePlace().Id;
        int _secondCardPlace = _secondCard.GetTablePlace().Id;

        TablePlaceHandler _placeInFront = GameplayManager.Instance.TableHandler.CheckForPlaceInFront(_firstCardPlace, _secondCardPlace);

        if (_placeInFront == null)
        {
            return false;
        }

        if (_placeInFront.IsAbility)
        {
            return false;
        }

        if (_placeInFront.IsOccupied)
        {
            return false;
        }
        
        return true;
    }

    private void PushCard(string _cardId)
    {
        Card _secondCard = GameplayManager.Instance.GetCard(_cardId);
        // GameplayManager.Instance.ExecuteMove();
    }

    private void TryToMoveSelf(bool _didDestroyCard)
    {
        if (!_didDestroyCard)
        {
            return;
        }

        TryToMoveSelf();
    }

    private void TryToMoveSelf()
    {
        
    }
}