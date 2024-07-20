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


        [Header("Inventory")]
        [ReloadRequired]
        [DefaultValue(3)]
        [Range(0, 10)]
        public int InventoryReferenceCount;

        [ReloadRequired]
        [DefaultValue(2)]
        [Range(0, 5)]
        public int EquipmentChangeReferenceCount;


        [Header("QuickUse")]
        [DefaultValue(false)]
        public bool preventHealthLoss;

        [DefaultValue(false)]
        public bool prioritizeRecallPotions;
    }
}
