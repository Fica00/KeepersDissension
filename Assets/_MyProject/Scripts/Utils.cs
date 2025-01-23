using UnityEngine;

public static class Utils
{
    public static int ConvertPosition(int _position)
    {
        int _totalAmountOfFields = 64;
        return _totalAmountOfFields - _position;
    }

    public static int ConvertRoomPosition(int _position, bool _isOwner)
    {
        Debug.Log("Converting: "+_position);
        if (_isOwner)
        {
            Debug.Log(_position);
            return _position;
        }

        if (_position<=0 || _position>=64)
        {
            Debug.Log(_position);
            return _position;
        }

        Debug.Log(ConvertPosition(_position));
        return ConvertPosition(_position);
    }
}
