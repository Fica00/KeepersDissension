public static class Utils
{
    public static int ConvertPosition(int _position)
    {
        int _totalAmountOfFields = 64;
        return _totalAmountOfFields - _position;
    }

    public static int ConvertRoomPosition(int _position, bool _isOwner)
    {
        if (_isOwner)
        {
            return _position;
        }

        if (_position<=0 || _position>=64)
        {
            return _position;
        }

        return ConvertPosition(_position);
    }
}
