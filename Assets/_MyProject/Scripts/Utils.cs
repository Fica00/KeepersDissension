public static class Utils
{
    public static int ConvertPosition(int _position)
    {
        int _totalAmountOfFields = 64;
        return _totalAmountOfFields - _position;
    }
}
