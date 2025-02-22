using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AdvancedControls.Common.Configs {
    public enum MountDashBehaviour {
        DashWithMount,
        DismountDashRemount,
        DismountDash
    }

    public class AdvancedControlsConfig : ModConfig {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Dash")]

        [DefaultValue(MountDashBehaviour.DashWithMount)]
        public MountDashBehaviour mountDashBehaviour;

        [DefaultValue(true)]
        public bool cancelHooks;

        [DefaultValue(true)]
        public bool dashBuffer;


        [Header("Inventory")]
        [ReloadRequired]
        [DefaultValue(4)]
        [Range(0, 9)]
        public int dynamicHotbarCount;

        [ReloadRequired]
        [DefaultValue(2)]
        [Range(0, 5)]
        public int equipmentChangeReferenceCount;


        [Header("QuickUse")]
        [DefaultValue(false)]
        public bool preventHealthLoss;

        [DefaultValue(false)]
        public bool prioritizeRecallPotions;
    }
}
