using System.Collections.Generic;
using System.Linq;

public class PortalCard : Card
{
    public TablePlaceHandler GetExitPlace(int _starting,int _finished)
    {
        List<PortalCard> _portals = FindObjectsOfType<PortalCard>().ToList();
        foreach (PortalCard _por in _portals)
        {
            if (_por==this)
            {
                continue;
            }

            int _exitIndex = GameplayManager.Instance.TableHandler.GetTeleportExitIndex(_starting,
                GetTablePlace().Id, _por.GetTablePlace().Id);
            TablePlaceHandler _exitPlace = GameplayManager.Instance.TableHandler.GetPlace(_exitIndex);
            return _exitPlace;
        }
        return null;
    }
}
