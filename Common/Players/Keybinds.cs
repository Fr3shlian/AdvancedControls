using AdvancedControls.Common.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using System.Linq;
using AdvancedControls.Common.Configs;
using System.Collections.Generic;
using AdvancedControls.Common.GlobalItems;
using Terraria.ModLoader.IO;
using System;
using System.Reflection;

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
    public class DashKeyBindPlayer : ModPlayer
    {
        int dashBuffer = 0;

        public override void PostUpdate()
        {
            if (Player.dashDelay == 0)
            {
                if (dashBuffer == -1)
                {
                    DashLeft();
                    dashBuffer = 0;
                }
                else if (dashBuffer == 1)
                {
                    DashRight();
                    dashBuffer = 0;
                }
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.DashKeybind.JustPressed)
            {
                if (Player.dashDelay == 0)
                    if (Player.controlLeft == true)
                        DashLeft();
                    else if (Player.controlRight == true || Player.direction == 1)
                        DashRight();
                    else DashLeft();
                else if (Util.GetConfig().dashBuffer)
                    dashBuffer = Player.controlLeft ? -1 : Player.controlRight || Player.direction == 1 ? 1 : -1;
            }

            if (KeybindSystem.DashLeftKeybind.JustPressed)
                if (Player.dashDelay == 0)
                    DashLeft();
                else if (Util.GetConfig().dashBuffer)
                    dashBuffer = -1;

            if (KeybindSystem.DashRightKeybind.JustPressed)
                if (Player.dashDelay == 0)
                    DashRight();
                else if (Util.GetConfig().dashBuffer)
                    dashBuffer = 1;
        }

        private void Dismount()
        {
            if (Util.GetConfig().mountDashBehaviour == MountDashBehaviour.DashWithMount)
            {
                Player.mount._active = false;
            }
            else
            {
                Player.QuickMount();
            }
        }

        private void Remount()
        {
            AdvancedControlsConfig config = Util.GetConfig();

            if (config.mountDashBehaviour == MountDashBehaviour.DashWithMount)
                Player.mount._active = true;
            else if (config.mountDashBehaviour == MountDashBehaviour.DismountDashRemount)
                Player.QuickMount();
        }

        private void DashLeft()
        {
            bool wasMounted = Player.mount.Active;

            if (wasMounted) Dismount();

            if (Util.GetConfig().cancelHooks) Player.RemoveAllGrapplingHooks();

            Player.controlRight = false;
            Player.controlLeft = true;
            Player.releaseLeft = true;
            Player.DashMovement();

            if (wasMounted)
            {
                Player.DashMovement();
                Remount();
            }
        }

        private void DashRight()
        {
            bool wasMounted = Player.mount.Active;

            if (wasMounted) Dismount();

            if (Util.GetConfig().cancelHooks)
            {
                Player.RemoveAllGrapplingHooks();
            }

            Player.controlLeft = false;
            Player.controlRight = true;
            Player.releaseRight = true;
            Player.DashMovement();

            if (wasMounted)
            {
                Player.DashMovement();
                Remount();
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
        private static Item[] hoveredInventory = null;
        private static int slotContext = -1;

        public override bool HoverSlot(Item[] inventory, int context, int slot)
        {
            hoveredInventory = inventory;
            slotContext = context;
            hoveredSlot = slot;
            return false;
        }

        public static int GetHoveredSlot()
        {
            return hoveredSlot;
        }

        public static Item[] GetHoveredInventory()
        {
            return hoveredInventory;
        }

        public static int GetContext()
        {
            return slotContext;
        }

        public void RemoveOtherReference(int slot)
        {
            DynamicHotbarKeyBindPlayer kbp = Player.GetModPlayer<DynamicHotbarKeyBindPlayer>();

            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++)
            {
                if (slot == kbp.GetReference(i))
                {
                    kbp.UnbindReference(i);
                    return;
                }
            }
            EquipmentChangeReferenceKeyBindPlayer erp = Player.GetModPlayer<EquipmentChangeReferenceKeyBindPlayer>();

            for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++)
            {
                if (slot == erp.GetReference(i))
                {
                    erp.UnbindReference(i);
                    return;
                }
            }
        }
    }

    // --- Inventory Reference ---
    public class DynamicHotbarKeyBindPlayer : ModPlayer
    {
        private readonly int[] dynamicHotbar = Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count).ToArray();
        private int lastSelectedItem = -1;

        private int[] frameCounter = Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count).ToArray();

        public override void PostUpdate()
        {
            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++)
            {
                if (frameCounter[i] == 1)
                {
                    frameCounter[i] = -1;
                    dynamicHotbar[i] = -1;
                }
                else if (frameCounter[i] != -1) frameCounter[i]--;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Set("dynamicHotbar", dynamicHotbar, true);
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("dynamicHotbar"))
            {
                int[] data = tag.GetIntArray("dynamicHotbar");
                for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++)
                    dynamicHotbar[i] = data[i];
            }
            else for (int i = 0; i < dynamicHotbar.Length; i++)
                {
                    dynamicHotbar[i] = -1;
                }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++)
            {
                if (KeybindSystem.DynamicHotbarKeyBinds[i].JustPressed)
                {
                    if (Main.playerInventory)
                    {
                        if (dynamicHotbar[i] == -1 && HoverSlotPlayer.GetHoveredSlot() > -1 && HoverSlotPlayer.GetHoveredSlot() < 50)
                        {
                            Player.GetModPlayer<HoverSlotPlayer>().RemoveOtherReference(HoverSlotPlayer.GetHoveredSlot());
                            dynamicHotbar[i] = HoverSlotPlayer.GetHoveredSlot();
                        }
                        else frameCounter[i] = 10;
                    }
                    else if (dynamicHotbar[i] != -1)
                    {
                        DynamicHotbarAction(i);
                    }
                }
                else if (KeybindSystem.DynamicHotbarKeyBinds[i].JustReleased)
                {
                    for (int j = 0; j < KeybindSystem.DynamicHotbarKeyBinds.Count; j++)
                    {
                        if (frameCounter[j] != -1)
                        {
                            frameCounter[j] = -1;
                            DynamicHotbarAction(i);
                        }
                    }
                }
            }
        }

        private bool IsItemReferenced(int slot)
        {
            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++)
            {
                if (dynamicHotbar[i] == slot) return true;
            }

            return false;
        }

        private void DynamicHotbarAction(int slot)
        {
            if (Player.selectedItem == dynamicHotbar[slot] && lastSelectedItem != -1)
            {
                Player.selectedItem = lastSelectedItem;
                lastSelectedItem = -1;
            }
            else
            {
                if ((Player.selectedItem < 10 && !IsItemReferenced(Player.selectedItem) && IsItemReferenced(dynamicHotbar[slot])) || lastSelectedItem == -1)
                    lastSelectedItem = Player.selectedItem;

                Player.selectedItem = dynamicHotbar[slot];
            }
        }

        public int GetReference(int slot)
        {
            return dynamicHotbar[slot];
        }

        public void UnbindReference(int slot)
        {
            dynamicHotbar[slot] = -1;
        }
    }

    // --- Equipment Change Reference ---
    public class EquipmentChangeReferenceKeyBindPlayer : ModPlayer
    {
        private readonly int[] equipmentReference = Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count).ToArray();

        private readonly int[] frameCounter = Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count).ToArray();

        public override void PostUpdate()
        {
            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++)
            {
                if (frameCounter[i] == 1)
                {
                    frameCounter[i] = -1;
                    equipmentReference[i] = -1;
                }
                else if (frameCounter[i] != -1) frameCounter[i]--;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Set("equipmentReference", equipmentReference, true);
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("equipmentReference"))
            {
                int[] data = tag.GetIntArray("equipmentReference");
                for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++)
                    equipmentReference[i] = data[i];
            }
            else for (int i = 0; i < equipmentReference.Length; i++)
                {
                    equipmentReference[i] = -1;
                }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++)
            {
                if (KeybindSystem.EquipmentChangeReferenceKeyBinds[i].JustPressed)
                {
                    if (Main.playerInventory)
                    {
                        if (equipmentReference[i] == -1 && HoverSlotPlayer.GetHoveredSlot() > -1 && HoverSlotPlayer.GetHoveredSlot() < 50)
                        {
                            Player.GetModPlayer<HoverSlotPlayer>().RemoveOtherReference(HoverSlotPlayer.GetHoveredSlot());
                            equipmentReference[i] = HoverSlotPlayer.GetHoveredSlot();
                        }
                        else frameCounter[i] = 10;
                    }
                    else if (equipmentReference[i] != -1 && ItemSlot.Equippable(ref Player.inventory[equipmentReference[i]]))
                    {
                        EquipmentChangeAction(i);
                    }
                }
                else if (KeybindSystem.EquipmentChangeReferenceKeyBinds[i].JustReleased)
                {
                    for (int j = 0; j < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; j++)
                    {
                        if (frameCounter[j] != -1)
                        {
                            frameCounter[j] = -1;
                            EquipmentChangeAction(i);
                        }
                    }
                }
            }
        }

        private void EquipmentChangeAction(int slot)
        {
            int mountType = Player.inventory[equipmentReference[slot]].mountType;

            ItemSlot.SwapEquip(ref Player.inventory[equipmentReference[slot]]);
            Player.inventory[equipmentReference[slot]].favorited = true;

            if (Player.mount.Active && mountType != -1)
            {
                Player.mount.SetMount(mountType, Player);
            }
        }

        public int GetReference(int slot)
        {
            return equipmentReference[slot];
        }

        public void UnbindReference(int slot)
        {
            equipmentReference[slot] = -1;
        }
    }

    // --- Rulers ---
    public class RulerKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.RulerKeyBind.JustPressed)
            {
                Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine] = Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine] == 1 ? 0 : 1;
            }
        }
    }

    public class MechanicalRulerKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.MechanicalRulerKeyBind.JustPressed)
            {
                Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid] = Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid] == 1 ? 0 : 1;
            }
        }
    }

    // --- QoL ---
    public class InventoryHelperPlayer : ModPlayer
    {
        private static int priorSelectedItem = -1;

        //Switch back to the prior item once the player finishes using it
        public override void PostUpdate()
        {
            if (priorSelectedItem != -1 && Player.itemTime <= 0)
            {
                Player.selectedItem = priorSelectedItem;
                priorSelectedItem = -1;
            }
        }

        public static void UseItem(int slot)
        {
            if (Main.LocalPlayer.itemTime == 0)
            {
                priorSelectedItem = Main.LocalPlayer.selectedItem;
                Main.LocalPlayer.selectedItem = slot;
                Main.LocalPlayer.ItemCheck();
                Main.LocalPlayer.controlUseItem = true;
            }
        }
    }

    public class TeleportKeyBindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.TeleportKeyBind.JustPressed)
            {
                int slot = Player.FindItem(ItemID.RodOfHarmony);

                if (slot != -1)
                    InventoryHelperPlayer.UseItem(slot);
                else if ((slot = Player.FindItem(ItemID.RodofDiscord)) != -1)
                {
                    if (Util.GetConfig().preventHealthLoss && !Player.creativeGodMode && Player.HasBuff(BuffID.ChaosState))
                        return;

                    InventoryHelperPlayer.UseItem(slot);
                }
            }
        }
    }

    public class RecallKeyBindPlayer : ModPlayer
    {
        int requiredShellPhone = -1;
        int priorSelectedItem = -1;

        //Switches to the correct shellphone mode, then uses it and switches back to the previous held item
        public override void PostUpdate()
        {
            if (requiredShellPhone != -1)
            {
                if (Player.inventory[Player.selectedItem].type != requiredShellPhone)
                {
                    ShellphoneGlobal.requiredShellPhone = requiredShellPhone;
                    ItemLoader.AltFunctionUse(Player.inventory[Player.selectedItem], Player);
                }
                else
                {
                    Player.controlUseItem = true;
                    Player.ItemCheck();
                    requiredShellPhone = -1;
                }
            }
            else if (priorSelectedItem != -1 && Player.itemTime <= 0)
            {
                Player.selectedItem = priorSelectedItem;
                priorSelectedItem = -1;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.RecallKeyBind.JustPressed)
            {
                int slot;

                if ((slot = FindWishingGlass()) != -1)
                {
                    UseWishingGlass(slot, "Home");
                }
                else if (Util.GetConfig().prioritizeRecallPotions)
                {
                    slot = FindRecallPotions();

                    if (slot != -1)
                        InventoryHelperPlayer.UseItem(slot);
                    else if ((slot = FindMirror()) != -1)
                        InventoryHelperPlayer.UseItem(slot);
                    else if ((slot = FindShellPhone()) != -1)
                    {
                        UseShellPhone(slot, ItemID.Shellphone);
                    }
                }
                else
                {
                    slot = FindMirror();

                    if (slot != -1)
                        InventoryHelperPlayer.UseItem(slot);
                    else if ((slot = FindShellPhone()) != -1)
                        UseShellPhone(slot, ItemID.Shellphone);
                    else if ((slot = FindRecallPotions()) != -1)
                    {
                        InventoryHelperPlayer.UseItem(slot);
                    }
                }
            }

            if (KeybindSystem.RecallSpawnKeyBind.JustPressed)
            {
                int slot = FindWishingGlass();

                if (slot != -1)
                    UseWishingGlass(slot, "Spawn");
                else if ((slot = FindShellPhone()) != -1)
                    UseShellPhone(slot, ItemID.ShellphoneSpawn);
            }

            if (KeybindSystem.RecallOceanKeyBind.JustPressed)
            {
                int slot = FindWishingGlass();

                if (slot != -1)
                    UseWishingGlass(slot, "Beach");
                else if ((slot = Player.FindItem(ItemID.MagicConch)) != -1)
                    InventoryHelperPlayer.UseItem(slot);
                else if ((slot = FindShellPhone()) != -1)
                    UseShellPhone(slot, ItemID.ShellphoneOcean);
            }

            if (KeybindSystem.RecallUnderworldKeyBind.JustPressed)
            {
                int slot = FindWishingGlass();

                if (slot != -1)
                    UseWishingGlass(slot, "Underworld");
                else if ((slot = Player.FindItem(ItemID.DemonConch)) != -1)
                    InventoryHelperPlayer.UseItem(slot);
                else if ((slot = FindShellPhone()) != -1)
                    UseShellPhone(slot, ItemID.ShellphoneHell);
            }

            if (KeybindSystem.RecallReturnKeyBind.JustPressed)
            {
                int slot = Player.FindItem(ItemID.PotionOfReturn);

                if (slot != -1)
                    InventoryHelperPlayer.UseItem(slot);
            }

            // --- Thorium Mod ---
            if (KeybindSystem.Thorium != null)
            {
                if (KeybindSystem.RecallDeathLocationKeyBind.JustPressed)
                {
                    int slot = FindWishingGlass();

                    if (slot != -1) UseWishingGlass(slot, "DeathLocation");
                }

                if (KeybindSystem.RecallDungeonKeyBind.JustPressed)
                {
                    int slot = FindWishingGlass();

                    if (slot != -1) UseWishingGlass(slot, "Dungeon");
                }

                if (KeybindSystem.RecallTempleKeyBind.JustPressed)
                {
                    int slot = FindWishingGlass();

                    if (slot != -1) UseWishingGlass(slot, "Temple");
                }

                if (KeybindSystem.TeleportRandomKeybind.JustPressed)
                {
                    int slot = FindWishingGlass();

                    if (slot != -1) UseWishingGlass(slot, "Random");
                }
            }
        }

        private int FindRecallPotions()
        {
            return Player.FindItem(ItemID.RecallPotion);
        }

        private int FindMirror()
        {
            return Player.FindItem(new List<int>() { ItemID.Shellphone, ItemID.MagicMirror, ItemID.IceMirror });
        }

        private int FindShellPhone()
        {
            return Player.FindItem(new List<int>() { ItemID.Shellphone, ItemID.ShellphoneSpawn, ItemID.ShellphoneOcean, ItemID.ShellphoneHell });
        }

        private int FindWishingGlass()
        {
            if (KeybindSystem.Thorium == null)
                return -1;
            else return Player.FindItem(KeybindSystem.Thorium.Find<ModItem>("WishingGlass").Type);
        }

        private void UseShellPhone(int slot, int shellPhoneID)
        {
            if (Main.LocalPlayer.itemTime == 0)
            {
                requiredShellPhone = shellPhoneID;
                priorSelectedItem = Player.selectedItem;
                Player.selectedItem = slot;
            }
        }

        private object GetThoriumPlayer()
        {
            Type ThoriumPlayerHelperType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.Utilities.PlayerHelper");
            MethodInfo getThoriumPlayerMethod = ThoriumPlayerHelperType.GetMethod("GetThoriumPlayer", BindingFlags.Static | BindingFlags.Public);
            return getThoriumPlayerMethod.Invoke(null, [Player]);
        }

        private void UseWishingGlass(int slot, string destination)
        {
            object thoriumPlayerInstance = GetThoriumPlayer();

            Type ThoriumPlayerType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.ThoriumPlayer");
            FieldInfo tpDestinationField = ThoriumPlayerType.GetField("itemWishingGlassChoice", BindingFlags.Instance | BindingFlags.Public);

            Type WishingGlassChoiceType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.UI.ResourceBars.WishingGlassChoice");
            FieldInfo wishingGlassChoice = WishingGlassChoiceType.GetField(destination, BindingFlags.Static | BindingFlags.Public);
            tpDestinationField.SetValue(thoriumPlayerInstance, wishingGlassChoice.GetValue(null));

            InventoryHelperPlayer.UseItem(slot);
        }
    }
}

public class Util
{
    public static AdvancedControlsConfig GetConfig()
    {
        return ModContent.GetInstance<AdvancedControlsConfig>();
    }
}