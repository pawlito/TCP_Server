
namespace NASServerTCP
{
    interface IbytesConvertable
    {
        byte[] GetBytesFromString(string str);
        string GetString(byte[] bytes);
    }
}
