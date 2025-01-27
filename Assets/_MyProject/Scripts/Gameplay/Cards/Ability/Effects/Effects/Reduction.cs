public class Reduction : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        bool _skipExecute = false;
        foreach (var _lifeForce in FindObjectsOfType<LifeForce>())
        {
            if (_lifeForce.Health<=0)
            {
                _skipExecute = true;
            }
        }
        
        if (_skipExecute)
        {
            MoveToActivationField();
            OnActivated?.Invoke();
            RemoveAction();
            return;
        }
        
        MoveToActivationField();
        GameplayManager.Instance.StartReduction(Finish);

        void Finish()
        {
            OnActivated?.Invoke();
            RemoveAction();
        }
    }
}
