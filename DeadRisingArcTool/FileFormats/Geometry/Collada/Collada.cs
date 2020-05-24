using DeadRisingArcTool.FileFormats.Geometry.Collada.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.Collada
{
    [ElementName("contributor")]
    public class Contributor
    {
        public string Author { get; set; }
        [ElementName("author_email")]
        public string AuthorEmail { get; set; }
        [ElementName("author_website")]
        public string AuthorWebsite { get; set; }
        [ElementName("authoring_tool")]
        public string AuthoringTool { get; set; }
        public string Comments { get; set; }
        public string Copyright { get; set; }
        [ElementName("source_data")]
        public string SourceDataURI { get; set; }
    }

    [ElementName("asset")]
    public class AssetInfo
    {
        public List<Contributor> Contributors { get; set; } = new List<Contributor>();
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }

    [ElementName("COLLADA", "http://www.collada.org/2004/COLLADASchema")]
    public class ColladaVersion
    {
        public string Version { get; set; } = "1.5.0";
    }

    public class ColladaDocument
    {
        public ColladaVersion Version;
        public AssetInfo AssetInfo;
    }
}
