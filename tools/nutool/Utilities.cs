using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace nutool;

internal static class Utilities
{
    public static string[] GetAllPathsToCsProjFilesRecursively(string rootPath)
    {
        return Directory.GetFiles(rootPath, "*.csproj", SearchOption.AllDirectories);
    }

    public static bool CheckIfFileHasBOM(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var bom = new byte[4];
        fileStream.ReadExactly(bom, 0, 4);
        return bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF;
    }

    public static void WriteXDocumentToStreamWithDeclaration(
        XDocument xDocument,
        Stream stream,
        bool encoderShouldEmitUTF8Identifier
    )
    {
        var xmlWriterSettings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier),
            OmitXmlDeclaration = false,
        };

        using var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings);
        xDocument.WriteTo(xmlWriter);
    }
}
