using RimWorld;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded2;

public static class LaunchInfoUtil
{
    public static VGE2LaunchInfo ExtendedVGE2Info(this LaunchInfo info, bool createIfMissing)
    {
        var extendedInfo = info.ExtendedInfo(createIfMissing);
        if (extendedInfo == null)
            return null;

        // Null or some other type
        if (extendedInfo.vge2Data is not VGE2LaunchInfo vge2Info)
        {
            vge2Info = new VGE2LaunchInfo();
            extendedInfo.vge2Data = vge2Info;
        }

        return vge2Info;
    }
}