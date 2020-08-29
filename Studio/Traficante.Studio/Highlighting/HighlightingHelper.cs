using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Traficante.Studio.Highlighting
{
    public class HighlightingHelper
    {
        public IEnumerable<string> GetAvailableHighlighting()
        {
            return 
                Directory
                .GetFiles("Highlighting")
                .Where(x => x.EndsWith(".xshd", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Substring(0, x.LastIndexOf('.')));
        }

        public IHighlightingDefinition LoadHighlightingDefinition(string name)
        {
            using (Stream stream = File.OpenRead(Path.Combine("Highlighting", name + ".xshd")))
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
    }
}
