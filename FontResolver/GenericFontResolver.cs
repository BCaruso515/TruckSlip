using PdfSharp.Fonts;
using System.Reflection;

namespace TruckSlip.FontResolver
{
    public class GenericFontResolver : IFontResolver
    {
        public static string DefaultFontName => "OpenSans";

        public byte[]? GetFont(string faceName)
        {
            if (faceName.Contains(DefaultFontName))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var myNamespace = assembly.GetName().Name;
                var stream = assembly.GetManifestResourceStream($"{myNamespace}.Resources.Fonts.{faceName}.ttf");

                if (stream == null)
                    return null;

                using var reader = new StreamReader(stream);
                var bytes = default(byte[]);

                using (var ms = new MemoryStream())
                {
                    reader.BaseStream.CopyTo(ms);
                    bytes = ms.ToArray();
                }
                return bytes;
            }
            else
                return GetFont(DefaultFontName);
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            switch (familyName)
            {
                case "Open Sans":
                case "OpenSans":
                    var fontName = isBold ? "OpenSans-Semibold" : "OpenSans-Regular";
                    return new FontResolverInfo(fontName);
                case "Arial":
                    fontName = isBold ? "OpenSans-Semibold" : "OpenSans-Regular";
                    return new FontResolverInfo(fontName);

                case "Courier New":
                case "Courier":
                    fontName = isBold ? "OpenSans-Semibold" : "OpenSans-Regular";
                    return new FontResolverInfo(fontName);

                default:
                    break;
            }
            return null;
        }
    }
}
