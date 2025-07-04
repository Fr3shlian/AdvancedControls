﻿using AdvancedControls.Common.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using System.Linq;
using AdvancedControls.Common.Configs;
using AdvancedControls.Common.GlobalItems;
using Terraria.ModLoader.IO;
using System;
using System.Reflection;
using Terraria.Audio;
using System.Collections.Generic;

namespace AdvancedControls.Common.Players {
    // --- Chest controls ---
    public class LootAllKeyBindPlayer : ModPlayer {
        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (KeybindSystem.LootAllKeybind?.JustPressed ?? false) {
                if (Player.chest != -1) {
                    ChestUI.LootAll();
                }
            }
        }
    }

    public class DepositAllKeyBindPlayer : ModPlayer {
        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (KeybindSystem.DepositAllKeybind?.JustPressed ?? false) {
                if (Player.chest != -1) {
                    ChestUI.DepositAll(ContainerTransferContext.FromUnknown(Player));
                }
            }
        }
    }

    public class QuickStackKeyBindPlayer : ModPlayer {
        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (KeybindSystem.QuickStackKeybind?.JustPressed ?? false) {
                if (Player.chest != -1) {
                    ChestUI.QuickStack(ContainerTransferContext.FromUnknown(Player), Player.chest == -5);
                }

                Player.QuickStackAllChests();
            }
        }
    }

    // --- Dash ---
    public class DashKeyBindPlayer : ModPlayer {
        private int secondInput = 0;
        private int dashBuffer = 0;
        private int needRemount = 0;
        private bool wasMounted = false;

        public override void ProcessTriggers(TriggersSet triggersSet) {
            AdvancedControlsConfig conf = Util.GetConfig();

            if (conf.disableDoubleTap && secondInput == 0) {
                Player.dashTime = 0;

                //Purely for Calamity Mod
                Player.releaseLeft = false;
                Player.releaseRight = false;
            }

            switch (needRemount) {
                case 1:
                    Remount();
                    goto case 3;
                case 2:
                    Player.QuickMount();
                    goto case 3;
                case 3:
                    needRemount = 0;
                    break;
            }


            switch (secondInput) {
                case -1:
                    InputLeft();
                    goto case 3;
                case 1:
                    InputRight();
                    goto case 3;
                case 3:
                    secondInput = 0;
                    if (wasMounted) needRemount = 1;
                    else if (conf.alwaysMount) needRemount = 2;
                    break;
            }

            if (dashBuffer != 0 && Player.dashDelay == 0) {
                if (conf.bufferCurrentDirection) dashBuffer = 2;

                switch (dashBuffer) {
                    case -1:
                        Dash(-1);
                        goto case 3;
                    case 1:
                        Dash(1);
                        goto case 3;
                    case 2:
                        Dash(GetDashDirection());
                        goto case 3;
                    case 3:
                        dashBuffer = 0;
                        break;
                }
            }

            if (KeybindSystem.DashKeybind?.JustPressed ?? false) {
                int dir = GetDashDirection();

                if (Player.dashDelay == 0)
                    Dash(dir);
                else if (conf.dashBuffer)
                    dashBuffer = dir;
            }

            if (KeybindSystem.DashLeftKeybind?.JustPressed ?? false)
                if (Player.dashDelay == 0)
                    Dash(-1);
                else if (conf.dashBuffer)
                    dashBuffer = -1;

            if (KeybindSystem.DashRightKeybind?.JustPressed ?? false)
                if (Player.dashDelay == 0)
                    Dash(1);
                else if (conf.dashBuffer)
                    dashBuffer = 1;
        }

        private int GetDashDirection() {
            if (Player.controlLeft == true)
                return -1;
            else if (Player.controlRight == true)
                return 1;

            return Player.confused ? Player.direction * -1 : Player.direction;
        }

        private void Dismount() {
            if (Util.GetConfig().mountDashBehaviour == MountDashBehaviour.DashWithMount) {
                Player.mount._active = false;
            } else {
                Player.QuickMount();
            }
        }

        private void Remount() {
            AdvancedControlsConfig config = Util.GetConfig();

            if (config.mountDashBehaviour == MountDashBehaviour.DashWithMount)
                Player.mount._active = true;
            else if (config.mountDashBehaviour == MountDashBehaviour.DismountDashRemount)
                Player.QuickMount();
        }

        private void InputLeft() {
            Player.controlRight = false;
            Player.controlLeft = true;
            if (Player.confused) Player.releaseRight = true; else Player.releaseLeft = true;
        }

        private void InputRight() {
            Player.controlLeft = false;
            Player.controlRight = true;
            if (Player.confused) Player.releaseLeft = true; else Player.releaseRight = true;
        }

        private void Dash(int direction) {
            wasMounted = Player.mount.Active;

            if (wasMounted) Dismount();

            if (Util.GetConfig().cancelHooks) Player.RemoveAllGrapplingHooks();

            if (direction == -1) InputLeft();
            else InputRight();

            secondInput = direction;
        }
    }

    // --- Cycle Inventory ---
    public class CycleInventoryKeyBindPlayer : ModPlayer {
        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (KeybindSystem.CycleInventoryLeftKeybind?.JustPressed ?? false) {
                CycleLeft();
            } else if (KeybindSystem.CycleInventoryRightKeybind?.JustPressed ?? false) {
                CycleRight();
            }
        }

        private void CycleLeft() {
            if (Player.selectedItem == 0) {
                Player.selectedItem = 49;
            } else {
                Player.selectedItem--;
            }
        }

        private void CycleRight() {
            if (Player.selectedItem > 48) {
                Player.selectedItem = 0;
            } else {
                Player.selectedItem++;
            }
        }
    }

    // --- Helper Class for Reference Buttons ---
    public class HoverSlotPlayer : ModPlayer {
        public static int HoveredSlot { get; private set; } = -1;
        public static Item[] HoveredInventory { get; private set; } = null;
        public static int HoveredSlotContext { get; private set; } = -1;

        public override bool HoverSlot(Item[] inventory, int context, int slot) {
            HoveredInventory = inventory;
            HoveredSlotContext = context;
            HoveredSlot = slot;
            return false;
        }

        public void RemoveOtherReference(int slot) {
            DynamicHotbarKeyBindPlayer kbp = Player.GetModPlayer<DynamicHotbarKeyBindPlayer>();

            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++) {
                if (slot == kbp.GetReference(i)) {
                    kbp.UnbindReference(i);
                    return;
                }
            }
            EquipmentChangeReferenceKeyBindPlayer erp = Player.GetModPlayer<EquipmentChangeReferenceKeyBindPlayer>();

            for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++) {
                if (slot == erp.EquipmentReference[i].Slot) {
                    erp.UnbindReference(i);
                    return;
                }
            }
        }
    }

    // --- Inventory Reference ---
    public class DynamicHotbarKeyBindPlayer : ModPlayer {
        private int[] dynamicHotbar = [.. Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count)];
        private readonly int[] holdTimer = Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count).ToArray();
        private int lastSelectedItem = -1;

        public override void SaveData(TagCompound tag) {
            tag.Set("dynamicHotbar", dynamicHotbar, true);
        }

        public override void LoadData(TagCompound tag) {
            if (tag.ContainsKey("dynamicHotbar")) {
                int[] arr = tag.GetIntArray("dynamicHotbar");
                int i;

                for (i = 0; i < (arr.Length < dynamicHotbar.Length ? arr.Length : dynamicHotbar.Length); i++)
                    dynamicHotbar[i] = arr[i];

                for (++i; i < dynamicHotbar.Length; i++)
                    dynamicHotbar[i] = -1;
            } else for (int i = 0; i < dynamicHotbar.Length; i++)
                    dynamicHotbar[i] = -1;
        }

        public override void ProcessTriggers(TriggersSet triggersSet) {
            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++) {
                if (KeybindSystem.DynamicHotbarKeyBinds[i].JustPressed) {
                    if (Main.playerInventory) {
                        if (dynamicHotbar[i] == -1) {
                            if (HoverSlotPlayer.HoveredSlot != -1 && HoverSlotPlayer.HoveredSlot < 50 && HoverSlotPlayer.HoveredInventory == Player.inventory) {
                                Player.GetModPlayer<HoverSlotPlayer>().RemoveOtherReference(HoverSlotPlayer.HoveredSlot);
                                dynamicHotbar[i] = HoverSlotPlayer.HoveredSlot;
                            }
                        } else holdTimer[i] = 10;
                    } else if (dynamicHotbar[i] != -1) {
                        DynamicHotbarAction(i);
                    }
                }

                if (KeybindSystem.DynamicHotbarKeyBinds[i].Current) {
                    if (holdTimer[i] == 1) {
                        holdTimer[i] = -1;
                        dynamicHotbar[i] = -1;
                    } else if (holdTimer[i] != -1) holdTimer[i]--;
                } else {
                    if (holdTimer[i] != -1) {
                        holdTimer[i] = -1;
                        DynamicHotbarAction(i);
                    }
                }
            }
        }

        private bool IsItemReferenced(int slot) {
            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++) {
                if (dynamicHotbar[i] == slot) return true;
            }

            return false;
        }

        private void DynamicHotbarAction(int slot) {
            if (Player.selectedItem == dynamicHotbar[slot] && lastSelectedItem != -1) {
                Player.selectedItem = lastSelectedItem;
                lastSelectedItem = -1;
            } else {
                if ((Player.selectedItem < 10 && !IsItemReferenced(Player.selectedItem)) || lastSelectedItem == -1)
                    lastSelectedItem = Player.selectedItem;

                Player.selectedItem = dynamicHotbar[slot];
            }

            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public int GetReference(int slot) {
            return dynamicHotbar[slot];
        }

        public void UnbindReference(int slot) {
            dynamicHotbar[slot] = -1;
        }
    }

    // --- Equipment Change Reference ---
    public class EquipmentChangeReferenceKeyBindPlayer : ModPlayer {
        public class InventoryReference(int slot = -1, Item[] inventory = null, int context = -1) : TagSerializable {
            public static readonly Func<TagCompound, InventoryReference> DESERIALIZER = DeserializeData;

            public int Slot { get; set; } = slot;
            public Item[] Inventory { get; set; } = inventory;
            public int Context { get; set; } = context;

            public ref Item GetItem() {
                return ref Inventory[Slot];
            }

            private enum InventoryType {
                Base,
                Equipment,
                MiscEquips
            }

            private int GetInventoryType() {
                if (Inventory == Main.CurrentPlayer.inventory) return (int)InventoryType.Base;
                else if (Inventory == Main.CurrentPlayer.armor) return (int)InventoryType.Equipment;
                else if (Inventory == Main.CurrentPlayer.miscEquips) return (int)InventoryType.MiscEquips;
                else return -1;
            }

            private static Item[] GetInventoryReference(InventoryType type) {
                switch (type) {
                    case InventoryType.Base: return Main.CurrentPlayer.inventory;
                    case InventoryType.Equipment: return Main.CurrentPlayer.armor;
                    case InventoryType.MiscEquips: return Main.CurrentPlayer.miscEquips;
                    default: return null;
                }
            }

            public TagCompound SerializeData() {
                return new TagCompound {
                    ["slot"] = Slot,
                    ["inventoryType"] = GetInventoryType(),
                    ["context"] = Context
                };
            }

            public static InventoryReference DeserializeData(TagCompound tag) {
                InventoryReference obj = new() {
                    Slot = tag.GetInt("slot"),
                    Inventory = GetInventoryReference((InventoryType)tag.GetInt("inventoryType")),
                    Context = tag.GetInt("context")
                };
                return obj;
            }
        }

        public InventoryReference[] EquipmentReference { get; private set; } = Enumerable.Repeat(new InventoryReference(), KeybindSystem.EquipmentChangeReferenceKeyBinds.Count).ToArray();
        private readonly InventoryReference[] equipmentTarget = Enumerable.Repeat(new InventoryReference(), KeybindSystem.EquipmentChangeReferenceKeyBinds.Count).ToArray();
        private readonly int[] holdTimer = Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count).ToArray();

        public override void SaveData(TagCompound tag) {
            tag.Set("equipmentSource", EquipmentReference, true);
            tag.Set("equipmentTarget", equipmentTarget, true);
        }

        public override void LoadData(TagCompound tag) {
            if (tag.ContainsKey("equipmentSource") && tag.ContainsKey("equipmentTarget")) {
                InventoryReference[] src = tag.Get<InventoryReference[]>("equipmentSource");
                InventoryReference[] trg = tag.Get<InventoryReference[]>("equipmentTarget");
                int i;

                for (i = 0; i < (src.Length < EquipmentReference.Length ? src.Length : EquipmentReference.Length); i++) {
                    EquipmentReference[i] = src[i];
                    equipmentTarget[i] = trg[i];
                }

                for (i = i + 1; i < EquipmentReference.Length; i++) {
                    EquipmentReference[i] = new InventoryReference();
                    equipmentTarget[i] = new InventoryReference();
                }
            } else for (int i = 0; i < EquipmentReference.Length; i++) {
                    EquipmentReference[i] = new InventoryReference();
                    equipmentTarget[i] = new InventoryReference();
                }
        }

        public override void ProcessTriggers(TriggersSet triggersSet) {
            for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++) {
                if (KeybindSystem.EquipmentChangeReferenceKeyBinds[i].JustPressed) {
                    if (Main.playerInventory) {
                        if (EquipmentReference[i].Slot == -1) {
                            if (HoverSlotPlayer.HoveredSlot != -1 && HoverSlotPlayer.HoveredInventory == Player.inventory) {
                                Player.GetModPlayer<HoverSlotPlayer>().RemoveOtherReference(HoverSlotPlayer.HoveredSlot);
                                EquipmentReference[i] = new InventoryReference(HoverSlotPlayer.HoveredSlot, HoverSlotPlayer.HoveredInventory, HoverSlotPlayer.HoveredSlotContext);
                            }
                        } else holdTimer[i] = 10;
                    } else if (equipmentTarget[i].Slot != -1) {
                        EquipmentChangeAction(i);
                    }
                }

                if (KeybindSystem.EquipmentChangeReferenceKeyBinds[i].Current) {
                    if (holdTimer[i] == 1) {
                        holdTimer[i] = -1;
                        EquipmentReference[i] = new InventoryReference();
                        equipmentTarget[i] = new InventoryReference();
                    } else if (holdTimer[i] != -1) holdTimer[i]--;
                } else {
                    if (holdTimer[i] != -1) {
                        holdTimer[i] = -1;
                        bool sameSlot = HoverSlotPlayer.HoveredSlot == EquipmentReference[i].Slot && HoverSlotPlayer.HoveredInventory == EquipmentReference[i].Inventory;

                        if (equipmentTarget[i].Slot == -1 && CanSlotAccept(EquipmentReference[i].Context, HoverSlotPlayer.HoveredSlotContext)) {
                            if (sameSlot) {
                                Item sourceItem = EquipmentReference[i].GetItem();

                                if (sourceItem.headSlot != -1) equipmentTarget[i] = new InventoryReference(0, Player.armor, ItemSlot.Context.EquipArmor);
                                else if (sourceItem.bodySlot != -1) equipmentTarget[i] = new InventoryReference(1, Player.armor, ItemSlot.Context.EquipArmor);
                                else if (sourceItem.legSlot != -1) equipmentTarget[i] = new InventoryReference(2, Player.armor, ItemSlot.Context.EquipArmor);
                                else if (sourceItem.buffType > 0 && Main.vanityPet[sourceItem.buffType]) equipmentTarget[i] = new InventoryReference(0, Player.miscEquips, ItemSlot.Context.EquipPet);
                                else if (sourceItem.buffType > 0 && Main.lightPet[sourceItem.buffType]) equipmentTarget[i] = new InventoryReference(1, Player.miscEquips, ItemSlot.Context.EquipLight);
                                else if (sourceItem.mountType != -1 && MountID.Sets.Cart[sourceItem.mountType]) equipmentTarget[i] = new InventoryReference(2, Player.miscEquips, ItemSlot.Context.EquipMinecart);
                                else if (sourceItem.mountType != -1 && !MountID.Sets.Cart[sourceItem.mountType]) equipmentTarget[i] = new InventoryReference(3, Player.miscEquips, ItemSlot.Context.EquipMount);
                                else if (Main.projHook[sourceItem.shoot]) equipmentTarget[i] = new InventoryReference(4, Player.miscEquips, ItemSlot.Context.EquipGrapple);
                                else return;
                            } else {
                                equipmentTarget[i] = new InventoryReference(HoverSlotPlayer.HoveredSlot, HoverSlotPlayer.HoveredInventory, HoverSlotPlayer.HoveredSlotContext);
                            }

                            SoundEngine.PlaySound(SoundID.MenuTick);

                            if (EquipmentReference[i].Slot > 49 && equipmentTarget[i].Slot < 49 && equipmentTarget[i].Inventory == Player.inventory) {
                                (EquipmentReference[i], equipmentTarget[i]) = (equipmentTarget[i], EquipmentReference[i]);
                            }
                        } else if (equipmentTarget[i].Slot != -1) EquipmentChangeAction(i);
                    }
                }
            }
        }

        private static bool CanSlotAccept(int context1, int context2) {
            if (context1 == ItemSlot.Context.InventoryItem || context1 == ItemSlot.Context.HotbarItem || context2 == ItemSlot.Context.InventoryItem || context2 == ItemSlot.Context.HotbarItem)
                return true;
            else if (context1 == context2)
                return true;
            else
                return false;
        }

        private bool CanTransfer(int i) {
            Item sourceItem = EquipmentReference[i].GetItem();
            InventoryReference target = equipmentTarget[i];
            bool inventoryTransfer = EquipmentReference[i].Context == target.Context || (EquipmentReference[i].Context == ItemSlot.Context.InventoryItem && target.Context == ItemSlot.Context.HotbarItem) || (EquipmentReference[i].Context == ItemSlot.Context.HotbarItem && target.Context == ItemSlot.Context.InventoryItem);

            if (inventoryTransfer) return true;
            if (sourceItem.IsAir) return true;

            else if (sourceItem.headSlot != -1 && target.Slot == 0 && target.Inventory == Player.armor)
                return true;
            else if (sourceItem.bodySlot != -1 && target.Slot == 1 && target.Inventory == Player.armor)
                return true;
            else if (sourceItem.legSlot != -1 && target.Slot == 2 && target.Inventory == Player.armor)
                return true;
            else if (sourceItem.accessory && Math.Abs(target.Context) == ItemSlot.Context.EquipAccessory)
                return true;
            else if (sourceItem.buffType > 0 && Main.vanityPet[sourceItem.buffType] && target.Context == ItemSlot.Context.EquipPet)
                return true;
            else if (sourceItem.buffType > 0 && Main.lightPet[sourceItem.buffType] && target.Context == ItemSlot.Context.EquipLight)
                return true;
            else if (sourceItem.mountType != -1 && MountID.Sets.Cart[sourceItem.mountType] && target.Context == ItemSlot.Context.EquipMinecart)
                return true;
            else if (sourceItem.mountType != -1 && !MountID.Sets.Cart[sourceItem.mountType] && target.Context == ItemSlot.Context.EquipMount)
                return true;
            else if (Main.projHook[sourceItem.shoot] && target.Context == ItemSlot.Context.EquipGrapple)
                return true;
            else if (sourceItem.ammo != AmmoID.None && target.Context == ItemSlot.Context.InventoryAmmo)
                return true;
            else if (sourceItem.IsACoin && target.Context == ItemSlot.Context.InventoryCoin)
                return true;

            return false;
        }

        private void EquipmentChangeAction(int slot) {
            if (CanTransfer(slot)) {
                ref Item source = ref EquipmentReference[slot].GetItem(), target = ref equipmentTarget[slot].GetItem();

                if (source.favorited && equipmentTarget[slot].Inventory == Player.armor || equipmentTarget[slot].Inventory == Player.miscEquips) {
                    source.favorited = false;
                    target.favorited = true;
                }

                (source, target) = (target, source);

                KeybindSystem.SetItemRefsForIndicator(source, target);

                Player.UpdateEquips(0);
                SoundEngine.PlaySound(SoundID.MenuTick);

                if (Player.mount.Active) {
                    if (equipmentTarget[slot].Context == ItemSlot.Context.EquipMinecart && MountID.Sets.Cart[Player.mount.Type]) Player.mount.SetMount(Player.miscEquips[Player.miscSlotCart].mountType, Player);
                    else if (equipmentTarget[slot].Context == ItemSlot.Context.EquipMount && !MountID.Sets.Cart[Player.mount.Type]) Player.mount.SetMount(Player.miscEquips[Player.miscSlotMount].mountType, Player);
                }
            }
        }

        public void UnbindReference(int slot) {
            EquipmentReference[slot] = new InventoryReference();
        }
    }

    // --- Rulers ---
    public class RulerKeyBindPlayer : ModPlayer {
        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (KeybindSystem.RulerKeyBind?.JustPressed ?? false) {
                Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine] = Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine] == 1 ? 0 : 1;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            if (KeybindSystem.MechanicalRulerKeyBind?.JustPressed ?? false) {
                Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid] = Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid] == 1 ? 0 : 1;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
    }

    // --- QoL ---
    public class InventoryHelperPlayer : ModPlayer {
        private int priorSelectedItem = -1;

        //Switch back to the prior item once the player finishes using it
        public override void PreUpdate() {
            if (priorSelectedItem != -1 && Player.itemTime <= 0) {
                Player.selectedItem = priorSelectedItem;
                priorSelectedItem = -1;
            }
        }

        public void _UseItem(int slot) {
            if (Player.itemTime == 0) {
                priorSelectedItem = Player.selectedItem;
                Player.selectedItem = slot;
                Player.controlUseItem = true;
                Player.ItemCheck();
            }
        }

        public static void UseItem(int slot) {
            Main.LocalPlayer.GetModPlayer<InventoryHelperPlayer>()._UseItem(slot);
        }

        public static bool FindAndUseItem(int id) {
            int slot = Main.LocalPlayer.FindItem(id);

            if (slot == -1) return false;

            UseItem(slot);
            return true;
        }

        public static bool FindAndUseItem(List<int> ids) {
            int slot = Main.LocalPlayer.FindItem(ids);

            if (slot == -1) return false;

            UseItem(slot);
            return true;
        }
    }

    public class TeleportKeyBindPlayer : ModPlayer {
        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (KeybindSystem.TeleportKeyBind?.JustPressed ?? false) {
                if (InventoryHelperPlayer.FindAndUseItem(ItemID.RodOfHarmony)) return;

                if (Util.GetConfig().preventHealthLoss && !Player.creativeGodMode && Player.HasBuff(BuffID.ChaosState)) return;
                InventoryHelperPlayer.FindAndUseItem(ItemID.RodofDiscord);
            }
        }
    }

    public class RecallKeyBindPlayer : ModPlayer {
        private int requiredShellPhone = -1;
        private int priorSelectedItem = -1;

        //Switches to the correct shellphone mode, then uses it and switches back to the previous held item
        public override void PreUpdate() {
            if (requiredShellPhone != -1) {
                if (Player.HeldItem.type != requiredShellPhone) {
                    ShellphoneGlobal.requiredShellPhone = requiredShellPhone;
                    ItemLoader.AltFunctionUse(Player.inventory[Player.selectedItem], Player);
                } else {
                    Player.controlUseItem = true;
                    Player.ItemCheck();
                    requiredShellPhone = -1;
                }
            } else if (priorSelectedItem != -1 && Player.itemTime <= 0) {
                Player.selectedItem = priorSelectedItem;
                priorSelectedItem = -1;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (KeybindSystem.RecallKeyBind?.JustPressed ?? false) {
                if (FindAndUseWishingGlass("Home")) return;

                if (Util.GetConfig().prioritizeRecallPotions) {
                    if (InventoryHelperPlayer.FindAndUseItem(ItemID.RecallPotion)) return;
                    if (InventoryHelperPlayer.FindAndUseItem([ItemID.MagicMirror, ItemID.IceMirror, ItemID.CellPhone])) return;
                } else {
                    if (InventoryHelperPlayer.FindAndUseItem([ItemID.MagicMirror, ItemID.IceMirror, ItemID.CellPhone, ItemID.RecallPotion])) return;
                }

                FindAndUseShellPhone(ItemID.Shellphone);
            }

            if (KeybindSystem.RecallSpawnKeyBind?.JustPressed ?? false) {
                if (FindAndUseWishingGlass("Spawn")) return;

                FindAndUseShellPhone(ItemID.ShellphoneSpawn);
            }

            if (KeybindSystem.RecallOceanKeyBind?.JustPressed ?? false) {
                if (FindAndUseWishingGlass("Beach")) return;
                if (InventoryHelperPlayer.FindAndUseItem(ItemID.MagicConch)) return;
                FindAndUseShellPhone(ItemID.ShellphoneOcean);
            }

            if (KeybindSystem.RecallUnderworldKeyBind?.JustPressed ?? false) {
                if (FindAndUseWishingGlass("Underworld")) return;
                if (InventoryHelperPlayer.FindAndUseItem(ItemID.DemonConch)) return;
                FindAndUseShellPhone(ItemID.ShellphoneHell);
            }

            if (KeybindSystem.RecallReturnKeyBind?.JustPressed ?? false) {
                InventoryHelperPlayer.FindAndUseItem(ItemID.PotionOfReturn);
            }

            // --- Thorium Mod ---
            if (KeybindSystem.Thorium != null) {
                if (KeybindSystem.RecallDeathLocationKeyBind?.JustPressed ?? false) {
                    FindAndUseWishingGlass("DeathLocation");
                }

                if (KeybindSystem.RecallDungeonKeyBind?.JustPressed ?? false) {
                    FindAndUseWishingGlass("Dungeon");
                }

                if (KeybindSystem.RecallTempleKeyBind?.JustPressed ?? false) {
                    FindAndUseWishingGlass("Temple");
                }

                if (KeybindSystem.TeleportRandomKeybind?.JustPressed ?? false) {
                    FindAndUseWishingGlass("Random");
                }
            }
        }

        private void UseShellPhone(int slot, int shellPhoneID) {
            if (Main.LocalPlayer.itemTime == 0) {
                requiredShellPhone = shellPhoneID;
                priorSelectedItem = Player.selectedItem;
                Player.selectedItem = slot;
            }
        }

        private void FindAndUseShellPhone(int shellPhoneID) {
            int slot = Player.FindItem([ItemID.Shellphone, ItemID.ShellphoneSpawn, ItemID.ShellphoneOcean, ItemID.ShellphoneHell]);

            if (slot != -1) UseShellPhone(slot, shellPhoneID);
        }

        private int FindWishingGlass() {
            if (KeybindSystem.Thorium == null)
                return -1;
            else return Player.FindItem(KeybindSystem.Thorium.Find<ModItem>("WishingGlass").Type);
        }

        private bool FindAndUseWishingGlass(string destination) {
            int slot = FindWishingGlass();

            if (slot == -1) return false;

            UseWishingGlass(slot, destination);
            return true;
        }

        private object GetThoriumPlayer() {
            Type ThoriumPlayerHelperType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.Utilities.PlayerHelper");
            MethodInfo getThoriumPlayerMethod = ThoriumPlayerHelperType.GetMethod("GetThoriumPlayer", BindingFlags.Static | BindingFlags.Public);
            return getThoriumPlayerMethod.Invoke(null, [Player]);
        }

        private void UseWishingGlass(int slot, string destination) {
            object thoriumPlayerInstance = GetThoriumPlayer();

            Type ThoriumPlayerType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.ThoriumPlayer");
            FieldInfo tpDestinationField = ThoriumPlayerType.GetField("itemWishingGlassChoice", BindingFlags.Instance | BindingFlags.Public);

            Type WishingGlassChoiceType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.UI.ResourceBars.WishingGlassChoice");
            FieldInfo wishingGlassChoice = WishingGlassChoiceType.GetField(destination, BindingFlags.Static | BindingFlags.Public);
            tpDestinationField.SetValue(thoriumPlayerInstance, wishingGlassChoice.GetValue(null));

            InventoryHelperPlayer.UseItem(slot);
        }
    }

    public class StorageItemKeyBindPlayer : ModPlayer {
        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (KeybindSystem.PiggyBankKeybind?.JustPressed ?? false) {
                if (InventoryHelperPlayer.FindAndUseItem(ItemID.MoneyTrough)) return;

                int slot = Player.FindItem(ItemID.PiggyBank);

                if (slot != -1) {
                    Player.selectedItem = slot;
                }
            }

            if (KeybindSystem.VoidBagKeybind?.JustPressed ?? false) {
                InventoryHelperPlayer.FindAndUseItem([ItemID.VoidLens, ItemID.ClosedVoidBag]);
            }
        }
    }

    public class Util {
        public static AdvancedControlsConfig GetConfig() {
            return ModContent.GetInstance<AdvancedControlsConfig>();
        }
    }
}