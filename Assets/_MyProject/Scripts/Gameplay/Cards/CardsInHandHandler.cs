using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsInHandHandler : MonoBehaviour
{
   private GameplayPlayer player;

   [SerializeField] private Transform cardsHolder;
   [SerializeField] private CardInHandDisplay cardPrefab;
   [SerializeField] private GameObject backGround;
   [SerializeField] private Button closeButton;

   private List<CardInHandDisplay> shownCards = new ();

   public void Setup(GameplayPlayer _player)
   {
      player = _player;
      DeckDisplay.OnDeckClicked += ShowCards;
      CardInHandDisplay.OnClicked += CheckForBuyMinion;
      CardInHandDisplay.OnClicked += CheckForWallPurchase;
      closeButton.onClick.AddListener(HideCards);
   }

   private void OnDisable()
   {
      DeckDisplay.OnDeckClicked -= ShowCards;
      CardInHandDisplay.OnClicked -= CheckForBuyMinion;
      CardInHandDisplay.OnClicked -= CheckForWallPurchase;
      closeButton.onClick.RemoveListener(HideCards);
   }

   public void HideCards()
   {
      backGround.SetActive(false);
      ClearShownCards();
   }

   public void ShowCards(GameplayPlayer _player, CardType _type)
   {
      if (player != _player)
      {
         return;
      }

      closeButton.gameObject.SetActive(GameplayManager.Instance.GameState() == GameplayState.Gameplay);

      ClearShownCards();
      backGround.SetActive(true);

      if (_type == CardType.Ability)
      {
         ShowAbilities();
      }
      else
      {
         ShowWarriors(_type);
      }
   }

   private void ShowAbilities()
   {
      foreach (var _ownedCard in GameplayManager.Instance.GetOwnedAbilities(player.IsMy))
      {
         var _ability = GameplayManager.Instance.GetAbility(_ownedCard.UniqueId);
         if (_ability.GetTablePlace())
         {
            continue;  
         }
         var _cardDisplay = Instantiate(cardPrefab, cardsHolder);
         _cardDisplay.Setup(_ownedCard.UniqueId,true);
         shownCards.Add(_cardDisplay);
      }
   }

   private void ShowWarriors(CardType _type)
   {
      foreach (var _card in GameplayManager.Instance.GetAllCardsOfType(_type,player.IsMy).FindAll(_card => _card.CardData.CardPlace == CardPlace.Deck))
      {
         var _cardDisplay = Instantiate(cardPrefab, cardsHolder);
         _cardDisplay.Setup(_card.UniqueId,false);
         shownCards.Add(_cardDisplay);
      }
   }

   private void ClearShownCards()
   {
      foreach (var _card in shownCards)
      {
         Destroy(_card.gameObject);
      }

      shownCards.Clear();
   }

   private void CheckForBuyMinion(CardBase _card)
   {
      if (!GameplayManager.Instance.CanPlayerDoActions())
      {
         return;
      }
      if (_card == null)
      {
         return;
      }

      if (GameplayManager.Instance.GetGameplaySubState() < GameplaySubState.FinishedSelectingMinions)
      {
         return;
      }
      
      CardPlace _cardPlace = GameplayManager.Instance.GetCardPlace(_card);
      if (_cardPlace == CardPlace.Table)
      {
         return;
      }

      if (_card is AbilityCard)
      {
         return;
      }
      
      if (_card.GetIsMy() != player.IsMy)
      {
         return;
      }

      if (GameplayManager.Instance.IsResponseAction())
      {
         if (!GameplayManager.Instance.IsMyResponseAction())
         {
            return;
         }

         if (!GameplayManager.Instance.IsKeeperResponseAction)
         {
            return;
         }
      }
      else if (!GameplayManager.Instance.IsMyTurn())
      {
         return;
      }
      
      if (GameplayManager.Instance.IsAbilityActive<Famine>())
      {
         DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
         return;
      }

      if (!GameplayManager.Instance.GetAllCardsOfType(CardType.Minion, player.IsMy).Contains(_card as Minion))
      {
         return;
      }
      
      Minion _minion = (Minion)_card;
      
      if (!shownCards.Find(_display => _display.UniqueId == _minion.UniqueId))
      {
         return;
      }
      
      int _buyPrice = 10 - GameplayManager.Instance.StrangeMatterCostChange(true);
      if (GameplayManager.Instance.MyStrangeMatter() < _buyPrice)
      {
         DialogsManager.Instance.ShowOkDialog($"You need {_buyPrice} strange matter to buy minion");
         return;
      }

      DialogsManager.Instance.ShowYesNoDialog($"Do you want to buy minion for {_buyPrice} strange matter?",
         () =>
         {
            GameplayManager.Instance.BuyMinion(_card, _buyPrice);
            HideCards();
         });
   }

   private void CheckForWallPurchase(CardBase _card)
   {
      if (!GameplayManager.Instance.CanPlayerDoActions())
      {
         return;
      }
      
      if (_card == null)
      {
         return;
      }

      CardPlace _cardPlace = GameplayManager.Instance.GetCardPlace(_card);
      if (_cardPlace == CardPlace.Table)
      {
         return;
      }

      if (_card is AbilityCard)
      {
         return;
      }
      
      if (_card.GetIsMy() != player.IsMy)
      {
         return;
      }

      int _buildPrice = 5 - GameplayManager.Instance.StrangeMatterCostChange(true);

      if (!(_card is Wall))
      {
         return;
      }

      if (GameplayManager.Instance.IsResponseAction())
      {
         if (!GameplayManager.Instance.IsMyResponseAction())
         {
            return;
         }

         if (!GameplayManager.Instance.IsKeeperResponseAction)
         {
            return;
         }
      }
      else if (!GameplayManager.Instance.IsMyTurn())
      {
         return;
      }
      
      if (GameplayManager.Instance.IsAbilityActive<Famine>())
      {
         DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
         return;
      }

      if (GameplayManager.Instance.MyStrangeMatter() < _buildPrice)
      {
         DialogsManager.Instance.ShowOkDialog($"You need {_buildPrice} strange matter to build wall");
         return;
      }

      DialogsManager.Instance.ShowYesNoDialog($"Do you want to build wall for {_buildPrice} strange matter?",
         () =>
         {
            GameplayManager.Instance.BuildWall(_card, _buildPrice, null);
            HideCards();
         });
   }
}
