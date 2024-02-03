using AdvancedControls.Common.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using System.Linq;
using AdvancedControls.Common.Configs;

namespace AdvancedControls.Common.Players
{
    // --- Chest controls ---
    public class LootAllKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.LootAllKeybind.JustPressed)
            {
                if (Player.chest != -1)
                {
                    ChestUI.LootAll();
                }
            }
        }
    }

    public class DepositAllKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.DepositAllKeybind.JustPressed)
            {
                if (Player.chest != -1)
                {
                    ChestUI.DepositAll(ContainerTransferContext.FromUnknown(Player));
                }
            }
        }
    }

    public class QuickStackKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.QuickStackKeybind.JustPressed)
            {
                if (Player.chest != -1)
                {
                    ChestUI.QuickStack(ContainerTransferContext.FromUnknown(Player), Player.chest == -5);
                }

                Player.QuickStackAllChests();
            }
        }
    }

    // --- Dash ---
    public class DashLeftKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if ((KeybindSystem.DashLeftKeybind.JustPressed || KeybindSystem.DashLeftKeybind.JustReleased) && Player.dashDelay == 0)
            {
                bool wasMounted = Player.mount.Active;

                if (wasMounted)
                {
                    Dismount();
                }

                Player.controlRight = false;

                if (ModContent.GetInstance<AdvancedControlsConfig>().cancelHooks)
                {
                    Player.RemoveAllGrapplingHooks();
                }

                Player.controlLeft = true;
                Player.releaseLeft = true;
                Player.controlLeft = true;
                Player.DashMovement();

                if (wasMounted)
                {
                    Player.DashMovement();

                    Remount();
                }
            }
        }

        public static void Dismount()
        {
            if (ModContent.GetInstance<AdvancedControlsConfig>().mountDashBehaviour == MountDashBehaviour.DashWithMount)
            {
                Main.CurrentPlayer.mount._active = false;
            }
            else
            {
                Main.CurrentPlayer.QuickMount();
            }
        }

        public static void Remount()
        {
            AdvancedControlsConfig instance = ModContent.GetInstance<AdvancedControlsConfig>();

            if (instance.mountDashBehaviour == MountDashBehaviour.DashWithMount)
            {
                Main.CurrentPlayer.mount._active = true;
            }
            else if (instance.mountDashBehaviour == MountDashBehaviour.DismountDashRemount)
            {
                Main.CurrentPlayer.QuickMount();
            }
        }
    }

    public class DashRightKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if ((KeybindSystem.DashRightKeybind.JustPressed || KeybindSystem.DashRightKeybind.JustReleased) && Player.dashDelay == 0)
            {
                bool wasMounted = Player.mount.Active;

                if (wasMounted)
                {
                    DashLeftKeyBindPlayer.Dismount();
                }

                Player.controlLeft = false;

                if (ModContent.GetInstance<AdvancedControlsConfig>().cancelHooks)
                {
                    Player.RemoveAllGrapplingHooks();
                }

                Player.controlRight = true;
                Player.releaseRight = true;
                Player.controlRight = true;
                Player.DashMovement();

                if (wasMounted)
                {
                    Player.DashMovement();

                    DashLeftKeyBindPlayer.Remount();
                }
            }
        }
    }

    // --- Cycle Inventory ---
    public class CycleInventoryKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.CycleInventoryLeftKeybind.JustPressed)
            {
                CycleLeft();
            }

            else if (KeybindSystem.CycleInventoryRightKeybind.JustPressed)
            {
                CycleRight();
            }
        }

        private void CycleLeft()
        {
            if (Player.selectedItem == 0)
            {
                Player.selectedItem = 49;
            }
            else
            {
                Player.selectedItem--;
            }
        }

        private void CycleRight()
        {
            if (Player.selectedItem > 48)
            {
                Player.selectedItem = 0;
            }
            else
            {
                Player.selectedItem++;
            }
        }
    }

    // --- Helper Class for Reference Buttons ---
    public class HoverSlotPlayer : ModPlayer
    {
        private static int hoveredSlot = -1;

        public override bool HoverSlot(Item[] inventory, int context, int slot)
        {
            hoveredSlot = slot;
            return false;
        }

        public static int GetHoveredSlot()
        {
            return hoveredSlot;
        }

        public static void RemoveOtherReference(int slot)
        {
            for (int i = 0; i < KeybindSystem.InventoryReferenceKeyBinds.Count; i++)
            {
                if (slot == InventoryReferenceKeyBindPlayer.inventoryReference[i])
                {
                    InventoryReferenceKeyBindPlayer.inventoryReference[i] = -1;
                    return;
                }
            }

            for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++)
            {
                if (slot == EquipmentChangeReferenceKeyBindPlayer.equipmentReference[i])
                {
                    EquipmentChangeReferenceKeyBindPlayer.equipmentReference[i] = -1;
                    return;
                }
            }
        }
    }

    // --- Inventory Reference ---
    public class InventoryReferenceKeyBindPlayer : ModPlayer
    {
        public static readonly int[] inventoryReference = Enumerable.Repeat(-1, KeybindSystem.InventoryReferenceKeyBinds.Count).ToArray();
        private static int lastSelectedItem = 0;

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            for (int i = 0; i < KeybindSystem.InventoryReferenceKeyBinds.Count; i++)
            {
                if (KeybindSystem.InventoryReferenceKeyBinds[i].JustPressed)
                {
                    if (Main.playerInventory)
                    {
                        if (HoverSlotPlayer.GetHoveredSlot() > -1 && HoverSlotPlayer.GetHoveredSlot() < 50)
                        {
                            if (inventoryReference[i] == HoverSlotPlayer.GetHoveredSlot())
                                inventoryReference[i] = -1;
                            else
                            {
                                HoverSlotPlayer.RemoveOtherReference(HoverSlotPlayer.GetHoveredSlot());
                                inventoryReference[i] = HoverSlotPlayer.GetHoveredSlot();
                            }

                            return;
                        }
                    }
                    else if (inventoryReference[i] != -1)
                    {
                        if (Player.selectedItem == inventoryReference[i])
                            Player.selectedItem = lastSelectedItem;
                        else
                        {
                            lastSelectedItem = Player.selectedItem;
                            Player.selectedItem = inventoryReference[i];
                        }
                    }
                }
            }
        }
    }

    // --- Equipment Change Reference ---
    public class EquipmentChangeReferenceKeyBindPlayer : ModPlayer
    {
        public static readonly int[] equipmentReference = Enumerable.Repeat(-1, KeybindSystem.EquipmentChangeReferenceKeyBinds.Count).ToArray();

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++)
            {
                if (KeybindSystem.EquipmentChangeReferenceKeyBinds[i].JustPressed)
                {
                    if (Main.playerInventory)
                    {
                        if (HoverSlotPlayer.GetHoveredSlot() > -1 && HoverSlotPlayer.GetHoveredSlot() < 50)
                        {
                            if (equipmentReference[i] == HoverSlotPlayer.GetHoveredSlot())
                                equipmentReference[i] = -1;
                            else
                            {
                                HoverSlotPlayer.RemoveOtherReference(HoverSlotPlayer.GetHoveredSlot());
                                equipmentReference[i] = HoverSlotPlayer.GetHoveredSlot();
                            }

                            return;
                        }
                    }
                    else if (equipmentReference[i] != -1 && ItemSlot.Equippable(ref Player.inventory[equipmentReference[i]]))
                    {
                        int mountType = Player.inventory[equipmentReference[i]].mountType;

                        ItemSlot.SwapEquip(ref Player.inventory[equipmentReference[i]]);
                        Player.inventory[equipmentReference[i]].favorited = true;

                        if (Player.mount.Active && mountType != -1)
                        {
                            Player.mount.SetMount(mountType, Player);
                        }
                    }
                }
            }
        }
    }
}
