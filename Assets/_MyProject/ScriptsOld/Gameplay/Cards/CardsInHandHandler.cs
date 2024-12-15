using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsInHandHandler : MonoBehaviour
{
   private GameplayPlayer player;

   [SerializeField] private Transform cardsHolder;
   [SerializeField] private GameObject backGround;
   [SerializeField] private Button closeButton;

   private List<CardBase> shownCards = new ();
   private Vector3 sizeOfCards = new Vector3(2, 2, 1);

   public void Setup(GameplayPlayer _player)
   {
      player = _player;

      DeckDisplay.OnDeckClicked += ShowCards;
      CardHandInteractions.OnCardClicked += CheckForBuy;
      CardHandInteractions.OnCardClicked += CheckForWallPurchase;
      closeButton.onClick.AddListener(HideCards);
   }

   private void OnDisable()
   {
      DeckDisplay.OnDeckClicked -= ShowCards;
      CardHandInteractions.OnCardClicked -= CheckForBuy;
      CardHandInteractions.OnCardClicked -= CheckForWallPurchase;
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

      closeButton.gameObject.SetActive(GameplayManager.Instance.GameState == GameplayState.Playing ||
                                       GameplayManager.Instance.GameState == GameplayState.Waiting ||
                                       GameplayManager.Instance.GameState == GameplayState.AttackResponse);

      ClearShownCards();
      backGround.SetActive(true);

      if (_type == CardType.Ability)
      {
         ShowAbilities();
      }
      else
      {
         ShowOtherCards(_type);
      }
   }

   public void ShowAbilities()
   {
      foreach (var _card in player.GetAbilities())
      {
         if (_card.CardPlace== CardPlace.Table)
         {
            continue;
         }
         _card.transform.SetParent(cardsHolder);
         _card.PositionInHand();
         _card.gameObject.AddComponent<CardHandInteractions>().Setup(_card);
         _card.transform.localScale = sizeOfCards;
         shownCards.Add(_card);
      }
   }

   private void ShowOtherCards(CardType _type)
   {
      foreach (var _card in player.GetCardsInDeck(_type))
      {
         _card.transform.SetParent(cardsHolder);
         _card.PositionInHand(true);
         _card.gameObject.AddComponent<CardHandInteractions>().Setup(_card);
         _card.transform.localScale = sizeOfCards;
         shownCards.Add(_card);
      }

   }

   private void ClearShownCards()
   {
      foreach (var _card in shownCards)
      {
         CardHandInteractions _cardHandInteractions = _card.gameObject.GetComponent<CardHandInteractions>();
         if (_cardHandInteractions != null)
         {
            Destroy(_cardHandInteractions);
         }

         if (!(_card.CardPlace == CardPlace.Hand || _card.CardPlace == CardPlace.Graveyard))
         {
            continue;
         }

         _card.ReturnFromHand();
      }

      shownCards.Clear();
   }

   private void CheckForBuy(CardBase _card)
   {
      if (_card == null)
      {
         return;
      }

      if (_card.CardPlace == CardPlace.Table)
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
      
      if (GameplayManager.Instance.GameState != GameplayState.Playing && !GameplayManager.Instance.IsKeeperResponseAction)
      {
         return;
      }
      
      
      if (GameplayManager.Instance.IsAbilityActive<Famine>())
      {
         DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
         return;
      }

      if (!shownCards.Contains(_card))
      {
         return;
      }

      if (player.GetCardsInDeck(CardType.Minion).Contains(_card as Minion))
      {
         int _buyPrice = 10 - GameplayManager.Instance.StrangeMatterCostChange();
         if (player.StrangeMatter < _buyPrice&& !GameplayCheats.HasUnlimitedGold)
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
   }

   private void CheckForWallPurchase(CardBase _card)
   {
      if (_card == null)
      {
         return;
      }

      if (_card.CardPlace == CardPlace.Table)
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

      int _buildPrice = 5 - GameplayManager.Instance.StrangeMatterCostChange();

      if (!(_card is Wall))
      {
         return;
      }

      if (GameplayManager.Instance.GameState != GameplayState.Playing && !GameplayManager.Instance.IsKeeperResponseAction)
      {
         return;
      }
      
      if (GameplayManager.Instance.IsAbilityActive<Famine>())
      {
         DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
         return;
      }

      if (player.StrangeMatter < _buildPrice && !GameplayCheats.HasUnlimitedGold)
      {
         DialogsManager.Instance.ShowOkDialog($"You need {_buildPrice} strange matter to build wall");
         return;
      }

      DialogsManager.Instance.ShowYesNoDialog($"Do you want to build wall for {_buildPrice} strange matter?",
         () =>
         {
            GameplayManager.Instance.BuildWall(_card, _buildPrice);
            HideCards();
         });
   }
}
