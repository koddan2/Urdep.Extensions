using System.Globalization;
using System.Text;

namespace Align;

internal class Arguments
{
    public CultureInfo FileCulture { get; set; } = CultureInfo.InvariantCulture;
    public Encoding FileEncoding { get; set; } = Encoding.Default;
    public ICollection<string> Directories { get; set; } = [];
    public ICollection<string> Includes { get; set; } = [];
    public ICollection<string> Excludes { get; set; } = [];
}
