using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rose_AGP.Enum
{
    public enum RoseType
    {
        Frequency,
        Length,
        Other
    }

    public enum RoseGeom
    {
        Line,
        Point,
        CellOnly,
        Other
    }

    public enum RasterLineamentAnalysis
    {
        DensityLength,
        DensityFrequency,
        GroupMeansFrequency,
        GroupMeansLength,
        RelativeEntropy,
        GroupDominanceLength,
        GroupDominanceFrequency
    }

    public enum RoseLineamentAnalysis
    {
        RoseCells,
        RoseRegional,
        RoseRegionalPoint,
        Fishnet
    }
}
