using System.IO;

namespace NASServerTCP
{
    interface IhashGenerator
    {
        string GetChecksumBuffered(Stream stream);

    }

}