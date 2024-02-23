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
    public class DashHelper
    {
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

        public static void DashLeft()
        {
            bool wasMounted = Main.CurrentPlayer.mount.Active;

            if (wasMounted)
            {
                Dismount();
            }

            Main.CurrentPlayer.controlRight = false;

            if (ModContent.GetInstance<AdvancedControlsConfig>().cancelHooks)
            {
                Main.CurrentPlayer.RemoveAllGrapplingHooks();
            }

            Main.CurrentPlayer.controlLeft = true;
            Main.CurrentPlayer.releaseLeft = true;
            Main.CurrentPlayer.controlLeft = true;
            Main.CurrentPlayer.DashMovement();

            if (wasMounted)
            {
                Main.CurrentPlayer.DashMovement();

                Remount();
            }
        }

        public static void DashRight()
        {
            bool wasMounted = Main.CurrentPlayer.mount.Active;

            if (wasMounted)
            {
                Dismount();
            }

            Main.CurrentPlayer.controlLeft = false;

            if (ModContent.GetInstance<AdvancedControlsConfig>().cancelHooks)
            {
                Main.CurrentPlayer.RemoveAllGrapplingHooks();
            }

            Main.CurrentPlayer.controlRight = true;
            Main.CurrentPlayer.releaseRight = true;
            Main.CurrentPlayer.controlRight = true;
            Main.CurrentPlayer.DashMovement();

            if (wasMounted)
            {
                Main.CurrentPlayer.DashMovement();

                Remount();
            }
        }
    }

    public class DashKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if ((KeybindSystem.DashKeybind.JustPressed || KeybindSystem.DashKeybind.JustReleased) && Player.dashDelay == 0)
            {
                if (Player.controlLeft == true)
                {
                    DashHelper.DashLeft();
                }
                else if (Player.controlRight == true)
                {
                    DashHelper.DashRight();
                }
                else if (Player.direction == -1)
                {
                    DashHelper.DashLeft();
                }
                else
                {
                    DashHelper.DashRight();
                }
            }
        }
    }

    public class DashLeftKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if ((KeybindSystem.DashLeftKeybind.JustPressed || KeybindSystem.DashLeftKeybind.JustReleased) && Player.dashDelay == 0)
            {
                DashHelper.DashLeft();
            }
        }
    }

    public class DashRightKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if ((KeybindSystem.DashRightKeybind.JustPressed || KeybindSystem.DashRightKeybind.JustReleased) && Player.dashDelay == 0)
            {
                DashHelper.DashRight();
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
        private int lastSelectedItem = 0;
        private bool modItemChange = false;

        public override void PostItemCheck()
        {
            if(Player.selectedItem != lastSelectedItem)
            {
                if(!modItemChange)
                {
                    lastSelectedItem = Player.selectedItem;
                }
            }
        }

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
                        {
                            Player.selectedItem = lastSelectedItem;
                            modItemChange = false;
                        }
                        else
                        {
                            Player.selectedItem = inventoryReference[i];
                            modItemChange = true;
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
