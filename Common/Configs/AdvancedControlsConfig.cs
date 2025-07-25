﻿using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace AdvancedControls.Common.Configs {
    public enum MountDashBehaviour {
        DismountDash,
        DismountDashRemount,
        DashWithMount
    }

    public enum InventoryScroll {
        Normal,
        EntireInventorySafe,
        EntireInventory
    }

    public class AdvancedControlsConfig : ModConfig {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public bool ShouldScrollEntireInventory() {
            return inventoryScroll == InventoryScroll.EntireInventory || (inventoryScroll == InventoryScroll.EntireInventorySafe && Main.LocalPlayer.selectedItem >= 10);
        }

        [Header("Dash")]

        [DefaultValue(MountDashBehaviour.DismountDash)]
        public MountDashBehaviour mountDashBehaviour;

        [DefaultValue(true)]
        public bool cancelHooks;

        [DefaultValue(true)]
        public bool dashBuffer;

        [DefaultValue(true)]
        public bool bufferCurrentDirection;

        [DefaultValue(false)]
        public bool alwaysMount;

        [DefaultValue(true)]
        public bool disableDoubleTap;



        [Header("EquipmentChange")]

        [DefaultValue(true)]
        public bool changeMount;

        [DefaultValue(true)]
        public bool showChangeIndicator;



        [Header("UseTeleport")]

        [DefaultValue(false)]
        public bool preventHealthLoss;



        [Header("RecallHome")]

        [DefaultValue(false)]
        public bool prioritizeRecallPotions;



        [Header("Tweaks")]

        [DefaultValue(InventoryScroll.Normal)]
        public InventoryScroll inventoryScroll;

        [DefaultValue(true)]
        public bool scrollDuringItemUse;

        [DefaultValue(true)]
        public bool cancelChannellingItems;

        [DefaultValue(false)]
        public bool disableWallClimb;

        [DefaultValue(true)]
        public bool altAutoSelectHammer;

        [DefaultValue(true)]
        public bool autoSelectRegrowthItem;

        [DefaultValue(true)]
        public bool autoSelectPlanterSeeds;

        [DefaultValue(true)]
        public bool matchSeedsWithPlanter;



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

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enablePiggyBankKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableVoidBagKeybind;

        [ReloadRequired]
        [DefaultValue(true)]
        public bool enableBugNetKeybind;

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
