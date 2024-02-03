using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AdvancedControls.Common.Configs
{
    public enum MountDashBehaviour
    {
        DashWithMount,
        DismountDashRemount,
        DismountDash
    }

    public class AdvancedControlsConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Dash")]

        [DefaultValue(MountDashBehaviour.DashWithMount)]
        public MountDashBehaviour mountDashBehaviour;
        
        [DefaultValue(true)]
        public bool cancelHooks;
    }
}
