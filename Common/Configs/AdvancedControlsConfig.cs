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



        [Header("QuickUse")]

        [DefaultValue(false)]
        public bool preventHealthLoss;

        [DefaultValue(false)]
        public bool prioritizeRecallPotions;


        [Header("Keybinds")]

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableLootAllKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableDepositAllKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableQuickStackKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableDashKeybind;

        [ReloadRequired]
        [DefaultValue(false)]
        public bool enableDashLeftKeybind;

        [ReloadRequired]
        [DefaultValue(false)]
        public bool enableDashRightKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableCycleInventoryLeftKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableCycleInventoryRightKeybind;

        [ReloadRequired]
        [DefaultValue(4)]
        [Range(0, 9)]
        public int dynamicHotbarCount;

        [ReloadRequired]
        [DefaultValue(2)]
        [Range(0, 5)]
        public int equipmentChangeReferenceCount;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRulerKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableMechanicalRulerKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableTeleportKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRecallKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRecallSpawnKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRecallOceanKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRecallUnderworldKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRecallReturnKeybind;

        //--- Thorium ---
        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRecallDungeonKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRecallTempleKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableRecallDeathLocationKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableTeleportRandomKeybind;
    }
}
