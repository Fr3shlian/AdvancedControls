using AdvancedControls.Common.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using System.Linq;
using AdvancedControls.Common.Configs;
using Terraria.ModLoader.IO;
using System;
using System.Reflection;
using Terraria.Audio;
using System.Collections.Generic;

namespace AdvancedControls.Common.Players {
    public class KeyBindPlayer : ModPlayer {
        private readonly List<IKeybind> keybinds = [];
        private readonly List<Action<KeyBindPlayer, TriggersSet>> processTriggerFunctions = [];
        private readonly List<Action<KeyBindPlayer, TagCompound>> saveDataFunctions = [];
        private readonly List<Action<KeyBindPlayer, TagCompound>> loadDataFunctions = [];

        public AdvancedControlsConfig conf;

        // --- Helpers for inventory actions ---
        private int priorSelectedItem = -1;
        public int ItemToSelect { get; private set; } = -1;
        private bool useOnceAndSwitchBack = false;
        private bool playSound = false;

        // --- Helpers for hotbar scrolling tweaks ---
        public int origSelectedItem = -1;
        public int origItemAnimation = -1;
        public int origItemTime = -1;
        public int origReuseDelay = -1;
        public bool origHotbar1 = false;
        public bool valuesChanged = false;

        // --- Helpers for Dynamic Hotbar and Equipment Change ---
        public int HoveredSlot { get; private set; } = -1;
        public Item[] HoveredInventory { get; private set; } = null;
        public int HoveredSlotContext { get; private set; } = -1;

        // --- Expose to Keybindsystem ---
        public DynamicHotbarKeyBind DynamicHotbarKb { get; private set; } = null;
        public EquipmentChangeKeyBind EquipmentChangeKb { get; private set; } = null;

        public override void Initialize() {
            conf = ModContent.GetInstance<AdvancedControlsConfig>();
            // --- Chest controls ---
            if (KeybindSystem.LootAllKeybind != null) keybinds.Add(new LootAllKeyBind());
            if (KeybindSystem.DepositAllKeybind != null) keybinds.Add(new DepositAllKeyBind());
            if (KeybindSystem.QuickStackKeybind != null) keybinds.Add(new QuickStackKeyBind());

            // --- Dash ---
            if (KeybindSystem.DashKeybind != null) keybinds.Add(new DashKeyBind());

            // --- Dynamic Hotbar ---
            if (KeybindSystem.DynamicHotbarKeyBinds.Count != 0) keybinds.Add(DynamicHotbarKb = new DynamicHotbarKeyBind());

            // --- Equipment Change ---
            if (KeybindSystem.EquipmentChangeReferenceKeyBinds.Count != 0) keybinds.Add(EquipmentChangeKb = new EquipmentChangeKeyBind());

            // --- Rulers ---
            if (KeybindSystem.RulerKeyBind != null) keybinds.Add(new RulerKeyBind());
            if (KeybindSystem.MechanicalRulerKeyBind != null) keybinds.Add(new MechanicalRulerKeyBind());

            // --- QoL ---
            if (KeybindSystem.TeleportKeyBind != null) keybinds.Add(new TeleportKeyBind());
            if (KeybindSystem.RecallKeyBind != null) keybinds.Add(new RecallKeyBind());
            if (KeybindSystem.RecallSpawnKeyBind != null) keybinds.Add(new RecallSpawnKeyBind());
            if (KeybindSystem.RecallOceanKeyBind != null) keybinds.Add(new RecallOceanKeyBind());
            if (KeybindSystem.RecallUnderworldKeyBind != null) keybinds.Add(new RecallUnderworldKeyBind());
            if (KeybindSystem.RecallReturnKeyBind != null) keybinds.Add(new RecallReturnKeyBind());
            if (KeybindSystem.PiggyBankKeybind != null) keybinds.Add(new PiggyBankKeyBind());
            if (KeybindSystem.VoidBagKeybind != null) keybinds.Add(new VoidBagKeyBind());
            if (KeybindSystem.BugNetKeyBind != null) keybinds.Add(new BugNetKeybind());

            // --- Thorium ---
            if (KeybindSystem.RecallDungeonKeyBind != null) keybinds.Add(new RecallDungeonKeyBind());
            if (KeybindSystem.RecallTempleKeyBind != null) keybinds.Add(new RecallTempleKeyBind());
            if (KeybindSystem.RecallDeathLocationKeyBind != null) keybinds.Add(new RecallDeathLocationKeyBind());
            if (KeybindSystem.TeleportRandomKeybind != null) keybinds.Add(new TeleportRandomKeybind());

            for (int i = 0; i < keybinds.Count; i++) {
                if (keybinds[i] is IProcessTriggers keybind1) processTriggerFunctions.Add(keybind1.ProcessTriggers);
                if (keybinds[i] is ISaveData keybind2) saveDataFunctions.Add(keybind2.SaveData);
                if (keybinds[i] is ILoadData keybind3) loadDataFunctions.Add(keybind3.LoadData);
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet) {
            for (int i = 0; i < processTriggerFunctions.Count; i++) {
                processTriggerFunctions[i](this, triggersSet);
            }

            // --- Helpers for inventory actions ---
            if (priorSelectedItem != -1 && Player.itemAnimation == 0 && Player.ItemTimeIsZero && Player.reuseDelay == 0) {
                Player.selectedItem = priorSelectedItem;
                SoundEngine.PlaySound(SoundID.MenuTick);
                priorSelectedItem = -1;
            }

            if (ItemToSelect != -1) {
                Player.controlUseItem = false;

                if (Player.itemAnimation == 0 && Player.ItemTimeIsZero && Player.reuseDelay == 0) {
                    if (useOnceAndSwitchBack) {
                        priorSelectedItem = Player.selectedItem;
                        Player.controlUseItem = true;
                    }

                    if (ItemToSelect == Player.selectedItem) {
                        ItemToSelect = -1;
                        priorSelectedItem = -1;
                        return;
                    }

                    if (playSound) SoundEngine.PlaySound(SoundID.MenuTick);
                    Player.selectedItem = ItemToSelect;
                    ItemToSelect = -1;

                    Player.ItemCheck();
                }
            }
        }

        // --- Helper for hotbar scrolling tweaks ---
        public override void SetControls() {
            //Mock offset calculation to check whether the player is scrolling
            int num = PlayerInput.Triggers.Current.HotbarPlus.ToInt() - PlayerInput.Triggers.Current.HotbarMinus.ToInt();
            int theorheticalScrollCD = PlayerInput.Triggers.Current.HotbarScrollCD;
            int theorheticalOffset = Player.HotbarOffset;

            if (PlayerInput.CurrentProfile.HotbarAllowsRadial && num != 0 && PlayerInput.Triggers.Current.HotbarHoldTime > PlayerInput.CurrentProfile.HotbarRadialHoldTimeRequired && PlayerInput.CurrentProfile.HotbarRadialHoldTimeRequired != -1) {
                theorheticalScrollCD = 2;
            }

            if (PlayerInput.CurrentProfile.HotbarRadialHoldTimeRequired != -1) {
                num = PlayerInput.Triggers.JustReleased.HotbarPlus.ToInt() - PlayerInput.Triggers.JustReleased.HotbarMinus.ToInt();
                if (theorheticalScrollCD == 1 && num != 0)
                    num = 0;
            }

            if (theorheticalScrollCD == 0 && num != 0) {
                theorheticalOffset += num;
            }

            if (!Main.inFancyUI && !Main.ingameOptionsWindow)
                theorheticalOffset += PlayerInput.ScrollWheelDelta / -120;

            if (!conf.scrollDuringItemUse || !Player.controlUseItem || theorheticalOffset == 0 || Main.playerInventory) {
                valuesChanged = false;
                return;
            }

            //Store original values
            origSelectedItem = Player.selectedItem;
            origItemAnimation = Player.itemAnimation;
            origItemTime = Player.itemTime;
            origReuseDelay = Player.reuseDelay;
            origHotbar1 = PlayerInput.Triggers.Current.Hotbar1;

            //Set a hotbar trigger to true and itemAnimation, itemTime and reuseDelay to 0, so HandleHotbar is always entered
            PlayerInput.Triggers.Current.Hotbar1 = true;
            Player.itemAnimation = 0;
            Player.itemTime = 0;
            Player.reuseDelay = 0;
            valuesChanged = true;
        }

        public override void SaveData(TagCompound tag) {
            for (int i = 0; i < saveDataFunctions.Count; i++) {
                saveDataFunctions[i](this, tag);
            }
        }

        public override void LoadData(TagCompound tag) {
            for (int i = 0; i < loadDataFunctions.Count; i++) {
                loadDataFunctions[i](this, tag);
            }
        }

        // --- Helpers for inventory actions ---
        public void SetItemToSelect(int slot, bool useOnceAndSwitchBack = true, bool playSound = true) {
            ItemToSelect = slot;
            this.playSound = playSound;
            this.useOnceAndSwitchBack = useOnceAndSwitchBack;
            Player.controlUseItem = false;
        }

        public bool FindAndUseItem(int id, bool useOnceAndSwitchBack = true) {
            int slot = Player.FindItem(id);

            if (slot == -1) return false;

            SetItemToSelect(slot, useOnceAndSwitchBack);
            return true;
        }

        public bool FindAndUseItem(List<int> ids, bool useOnceAndSwitchBack = true) {
            int slot = Player.FindItem(ids);

            if (slot == -1) return false;

            SetItemToSelect(slot, useOnceAndSwitchBack);
            return true;
        }

        // --- Helpers for Dynamic Hotbar and Equipment Change ---
        public override bool HoverSlot(Item[] inventory, int context, int slot) {
            HoveredInventory = inventory;
            HoveredSlotContext = context;
            HoveredSlot = slot;
            return false;
        }

        public void RemoveOtherReference(int slot) {
            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++) {
                if (slot == DynamicHotbarKb.GetReference(i)) {
                    DynamicHotbarKb.UnbindReference(i);
                    return;
                }
            }

            for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++) {
                if (slot == EquipmentChangeKb.EquipmentReference[i].Slot) {
                    EquipmentChangeKb.UnbindReference(i);
                    return;
                }
            }
        }

        // --- Helpers for recall keybinds ---
        public void FindAndUseShellPhone(int shellPhoneID) {
            int slot = Player.FindItem([ItemID.Shellphone, ItemID.ShellphoneSpawn, ItemID.ShellphoneOcean, ItemID.ShellphoneHell]);

            if (slot != -1) {
                Player.inventory[slot].type = shellPhoneID;
                SetItemToSelect(slot);
            }
        }

        // --- Thorium ---
        public int FindThoriumItem(string name) {
            if (KeybindSystem.Thorium == null) return -1;
            else return Player.FindItem(KeybindSystem.Thorium.Find<ModItem>(name).Type);
        }

        public bool FindAndUseThoriumItem(string name) {
            int slot = FindThoriumItem(name);

            if (slot == -1) return false;

            SetItemToSelect(slot);
            return true;
        }

        public bool FindAndUseWishingGlass(string destination) {
            int slot = FindThoriumItem("WishingGlass");

            if (slot == -1) return false;

            UseWishingGlass(slot, destination);
            return true;
        }

        public object GetThoriumPlayer() {
            Type ThoriumPlayerHelperType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.Utilities.PlayerHelper");
            MethodInfo getThoriumPlayerMethod = ThoriumPlayerHelperType.GetMethod("GetThoriumPlayer", BindingFlags.Static | BindingFlags.Public);
            return getThoriumPlayerMethod.Invoke(null, [Player]);
        }

        public void UseWishingGlass(int slot, string destination) {
            object thoriumPlayerInstance = GetThoriumPlayer();

            Type ThoriumPlayerType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.ThoriumPlayer");
            FieldInfo tpDestinationField = ThoriumPlayerType.GetField("itemWishingGlassChoice", BindingFlags.Instance | BindingFlags.Public);

            Type WishingGlassChoiceType = KeybindSystem.Thorium.Code.GetType("ThoriumMod.UI.ResourceBars.WishingGlassChoice");
            FieldInfo wishingGlassChoice = WishingGlassChoiceType.GetField(destination, BindingFlags.Static | BindingFlags.Public);
            tpDestinationField.SetValue(thoriumPlayerInstance, wishingGlassChoice.GetValue(null));

            SetItemToSelect(slot);
        }
    }

    public interface IKeybind { }

    public interface IProcessTriggers : IKeybind {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet);
    }

    public interface ISaveData : IKeybind {
        public void SaveData(KeyBindPlayer modPlayer, TagCompound tag);
    }

    public interface ILoadData : IKeybind {
        public void LoadData(KeyBindPlayer modPlayer, TagCompound tag);
    }

    // --- Chest controls ---
    public class LootAllKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.LootAllKeybind.JustPressed) {
                if (modPlayer.Player.chest != -1) {
                    ChestUI.LootAll();
                }
            }
        }
    }

    public class DepositAllKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.DepositAllKeybind.JustPressed) {
                if (modPlayer.Player.chest != -1) {
                    ChestUI.DepositAll(ContainerTransferContext.FromUnknown(modPlayer.Player));
                }
            }
        }
    }

    public class QuickStackKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.QuickStackKeybind.JustPressed) {
                if (modPlayer.Player.chest != -1) {
                    ChestUI.QuickStack(ContainerTransferContext.FromUnknown(modPlayer.Player), modPlayer.Player.chest == -5);
                }

                modPlayer.Player.QuickStackAllChests();
            }
        }
    }

    // --- Dash ---
    public class DashKeyBind : IProcessTriggers {
        private int secondInput = 0;
        private int dashBuffer = 0;
        private int needRemount = 0;
        private bool wasMounted = false;
        private Player player;
        private KeyBindPlayer modPlayer;

        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            player = modPlayer.Player;
            this.modPlayer = modPlayer;

            if (modPlayer.conf.disableDoubleTap && secondInput == 0) {
                player.dashTime = 0;

                //Purely for Calamity Mod
                player.releaseLeft = false;
                player.releaseRight = false;
            }

            switch (needRemount) {
                case 1:
                    Remount();
                    goto case 3;
                case 2:
                    player.QuickMount();
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
                    else if (modPlayer.conf.alwaysMount) needRemount = 2;
                    break;
            }

            if (dashBuffer != 0 && player.dashDelay == 0) {
                if (modPlayer.conf.bufferCurrentDirection) dashBuffer = 2;

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

            if (KeybindSystem.DashKeybind.JustPressed) {
                int dir = GetDashDirection();

                if (player.dashDelay == 0)
                    Dash(dir);
                else if (modPlayer.conf.dashBuffer)
                    dashBuffer = dir;
            }
        }

        private int GetDashDirection() {
            if (player.controlLeft == true)
                return -1;
            else if (player.controlRight == true)
                return 1;

            return player.confused ? player.direction * -1 : player.direction;
        }

        private void Dismount() {
            if (modPlayer.conf.mountDashBehaviour == MountDashBehaviour.DashWithMount) {
                player.mount._active = false;
            } else {
                player.QuickMount();
            }
        }

        private void Remount() {
            AdvancedControlsConfig config = modPlayer.conf;

            if (config.mountDashBehaviour == MountDashBehaviour.DashWithMount)
                player.mount._active = true;
            else if (config.mountDashBehaviour == MountDashBehaviour.DismountDashRemount)
                player.QuickMount();
        }

        private void InputLeft() {
            player.controlRight = false;
            player.controlLeft = true;
            if (player.confused) player.releaseRight = true; else player.releaseLeft = true;
        }

        private void InputRight() {
            player.controlLeft = false;
            player.controlRight = true;
            if (player.confused) player.releaseLeft = true; else player.releaseRight = true;
        }

        private void Dash(int direction) {
            wasMounted = player.mount.Active;

            if (wasMounted) Dismount();

            if (modPlayer.conf.cancelHooks) player.RemoveAllGrapplingHooks();

            if (direction == -1) InputLeft();
            else InputRight();

            secondInput = direction;
        }
    }

    // --- Inventory Reference ---
    public class DynamicHotbarKeyBind : IProcessTriggers, ISaveData, ILoadData {
        private readonly int[] dynamicHotbar = [.. Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count)];
        private readonly int[] holdTimer = [.. Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count)];
        private int lastSelectedItem = -1;
        private Player player;
        private KeyBindPlayer modPlayer;

        public void SaveData(KeyBindPlayer modPlayer, TagCompound tag) {
            tag.Set("dynamicHotbar", dynamicHotbar, true);
        }

        public void LoadData(KeyBindPlayer modPlayer, TagCompound tag) {
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

        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            player = modPlayer.Player;
            this.modPlayer = modPlayer;

            for (int i = 0; i < KeybindSystem.DynamicHotbarKeyBinds.Count; i++) {
                if (KeybindSystem.DynamicHotbarKeyBinds[i].JustPressed) {
                    if (Main.playerInventory) {
                        if (dynamicHotbar[i] == -1) {
                            if (modPlayer.HoveredSlot != -1 && modPlayer.HoveredSlot < 50 && modPlayer.HoveredInventory == player.inventory) {
                                modPlayer.RemoveOtherReference(modPlayer.HoveredSlot);
                                dynamicHotbar[i] = modPlayer.HoveredSlot;
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
            if (player.selectedItem == dynamicHotbar[slot] && lastSelectedItem != -1) {
                modPlayer.SetItemToSelect(lastSelectedItem, false);
                lastSelectedItem = -1;
            } else {
                if ((player.selectedItem < 10 && !IsItemReferenced(player.selectedItem)) || lastSelectedItem == -1)
                    lastSelectedItem = player.selectedItem;

                modPlayer.SetItemToSelect(dynamicHotbar[slot], false);
            }
        }

        public int GetReference(int slot) {
            return dynamicHotbar[slot];
        }

        public void UnbindReference(int slot) {
            dynamicHotbar[slot] = -1;
        }
    }

    // --- Equipment Change ---
    public class EquipmentChangeKeyBind : IProcessTriggers, ISaveData, ILoadData {
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

        public InventoryReference[] EquipmentReference { get; private set; } = [.. Enumerable.Repeat(new InventoryReference(), KeybindSystem.EquipmentChangeReferenceKeyBinds.Count)];
        private readonly InventoryReference[] equipmentTarget = [.. Enumerable.Repeat(new InventoryReference(), KeybindSystem.EquipmentChangeReferenceKeyBinds.Count)];
        private readonly int[] holdTimer = [.. Enumerable.Repeat(-1, KeybindSystem.DynamicHotbarKeyBinds.Count)];
        private Player player;

        public void SaveData(KeyBindPlayer modPlayer, TagCompound tag) {
            tag.Set("equipmentSource", EquipmentReference, true);
            tag.Set("equipmentTarget", equipmentTarget, true);
        }

        public void LoadData(KeyBindPlayer modPlayer, TagCompound tag) {
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

        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            player = modPlayer.Player;

            for (int i = 0; i < KeybindSystem.EquipmentChangeReferenceKeyBinds.Count; i++) {
                if (KeybindSystem.EquipmentChangeReferenceKeyBinds[i].JustPressed) {
                    if (Main.playerInventory) {
                        if (EquipmentReference[i].Slot == -1) {
                            if (modPlayer.HoveredSlot != -1 && modPlayer.HoveredInventory == player.inventory) {
                                modPlayer.RemoveOtherReference(modPlayer.HoveredSlot);
                                EquipmentReference[i] = new InventoryReference(modPlayer.HoveredSlot, modPlayer.HoveredInventory, modPlayer.HoveredSlotContext);
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
                        bool sameSlot = modPlayer.HoveredSlot == EquipmentReference[i].Slot && modPlayer.HoveredInventory == EquipmentReference[i].Inventory;

                        if (equipmentTarget[i].Slot == -1 && CanSlotAccept(EquipmentReference[i].Context, modPlayer.HoveredSlotContext)) {
                            if (sameSlot) {
                                Item sourceItem = EquipmentReference[i].GetItem();

                                if (sourceItem.headSlot != -1) equipmentTarget[i] = new InventoryReference(0, player.armor, ItemSlot.Context.EquipArmor);
                                else if (sourceItem.bodySlot != -1) equipmentTarget[i] = new InventoryReference(1, player.armor, ItemSlot.Context.EquipArmor);
                                else if (sourceItem.legSlot != -1) equipmentTarget[i] = new InventoryReference(2, player.armor, ItemSlot.Context.EquipArmor);
                                else if (sourceItem.buffType > 0 && Main.vanityPet[sourceItem.buffType]) equipmentTarget[i] = new InventoryReference(0, player.miscEquips, ItemSlot.Context.EquipPet);
                                else if (sourceItem.buffType > 0 && Main.lightPet[sourceItem.buffType]) equipmentTarget[i] = new InventoryReference(1, player.miscEquips, ItemSlot.Context.EquipLight);
                                else if (sourceItem.mountType != -1 && MountID.Sets.Cart[sourceItem.mountType]) equipmentTarget[i] = new InventoryReference(2, player.miscEquips, ItemSlot.Context.EquipMinecart);
                                else if (sourceItem.mountType != -1 && !MountID.Sets.Cart[sourceItem.mountType]) equipmentTarget[i] = new InventoryReference(3, player.miscEquips, ItemSlot.Context.EquipMount);
                                else if (Main.projHook[sourceItem.shoot]) equipmentTarget[i] = new InventoryReference(4, player.miscEquips, ItemSlot.Context.EquipGrapple);
                                else return;
                            } else {
                                equipmentTarget[i] = new InventoryReference(modPlayer.HoveredSlot, modPlayer.HoveredInventory, modPlayer.HoveredSlotContext);
                            }

                            SoundEngine.PlaySound(SoundID.MenuTick);

                            if (EquipmentReference[i].Slot > 49 && equipmentTarget[i].Slot < 49 && equipmentTarget[i].Inventory == player.inventory) {
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

            else if (sourceItem.headSlot != -1 && target.Slot == 0 && target.Inventory == player.armor)
                return true;
            else if (sourceItem.bodySlot != -1 && target.Slot == 1 && target.Inventory == player.armor)
                return true;
            else if (sourceItem.legSlot != -1 && target.Slot == 2 && target.Inventory == player.armor)
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

                if (source.IsAir && target.IsAir) return;

                if (source.favorited && equipmentTarget[slot].Inventory == player.armor || equipmentTarget[slot].Inventory == player.miscEquips) {
                    source.favorited = false;
                    target.favorited = true;
                }

                (source, target) = (target, source);

                if (source != null && source.ModItem == null) Main.instance.LoadItem(source.type);
                if (target != null && source.ModItem == null) Main.instance.LoadItem(target.type);

                KeybindSystem.SetItemRefsForIndicator(source, target);

                player.UpdateEquips(0);
                SoundEngine.PlaySound(SoundID.Grab);

                if (player.mount.Active) {
                    if (equipmentTarget[slot].Context == ItemSlot.Context.EquipMinecart && MountID.Sets.Cart[player.mount.Type]) player.mount.SetMount(player.miscEquips[Player.miscSlotCart].mountType, player);
                    else if (equipmentTarget[slot].Context == ItemSlot.Context.EquipMount && !MountID.Sets.Cart[player.mount.Type]) player.mount.SetMount(player.miscEquips[Player.miscSlotMount].mountType, player);
                }
            }
        }

        public void UnbindReference(int slot) {
            EquipmentReference[slot] = new InventoryReference();
        }
    }

    // --- Rulers ---
    public class RulerKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RulerKeyBind.JustPressed) {
                modPlayer.Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine] = modPlayer.Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine] == 1 ? 0 : 1;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
    }

    public class MechanicalRulerKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.MechanicalRulerKeyBind.JustPressed) {
                modPlayer.Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid] = modPlayer.Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid] == 1 ? 0 : 1;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
    }

    // --- QoL ---
    public class TeleportKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.TeleportKeyBind.JustPressed) {
                if (modPlayer.FindAndUseItem(ItemID.RodOfHarmony)) return;

                if (modPlayer.conf.preventHealthLoss && !modPlayer.Player.creativeGodMode && modPlayer.Player.HasBuff(BuffID.ChaosState)) return;
                modPlayer.FindAndUseItem(ItemID.RodofDiscord);
            }
        }
    }

    public class RecallKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RecallKeyBind.JustPressed) {
                if (modPlayer.FindAndUseWishingGlass("Home")) return;

                if (modPlayer.conf.prioritizeRecallPotions) {
                    if (modPlayer.FindAndUseItem(ItemID.RecallPotion)) return;
                    if (modPlayer.FindAndUseItem([ItemID.MagicMirror, ItemID.IceMirror, ItemID.CellPhone])) return;
                } else {
                    if (modPlayer.FindAndUseItem([ItemID.MagicMirror, ItemID.IceMirror, ItemID.CellPhone, ItemID.RecallPotion])) return;
                }

                modPlayer.FindAndUseShellPhone(ItemID.Shellphone);
            }
        }
    }

    public class RecallSpawnKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RecallSpawnKeyBind.JustPressed) {
                if (modPlayer.FindAndUseWishingGlass("Spawn")) return;

                modPlayer.FindAndUseShellPhone(ItemID.ShellphoneSpawn);
            }
        }
    }

    public class RecallOceanKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RecallOceanKeyBind.JustPressed) {
                if (modPlayer.FindAndUseWishingGlass("Beach")) return;
                if (modPlayer.FindAndUseItem(ItemID.MagicConch)) return;
                modPlayer.FindAndUseShellPhone(ItemID.ShellphoneOcean);
            }
        }
    }

    public class RecallUnderworldKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RecallUnderworldKeyBind.JustPressed) {
                if (modPlayer.FindAndUseWishingGlass("Underworld")) return;
                if (modPlayer.FindAndUseItem(ItemID.DemonConch)) return;
                modPlayer.FindAndUseShellPhone(ItemID.ShellphoneHell);
            }
        }
    }

    public class RecallReturnKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RecallReturnKeyBind.JustPressed) {
                modPlayer.FindAndUseItem(ItemID.PotionOfReturn);
            }
        }
    }

    public class PiggyBankKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.PiggyBankKeybind.JustPressed) {
                if (modPlayer.FindAndUseItem(ItemID.MoneyTrough)) return;

                int slot = modPlayer.Player.FindItem(ItemID.PiggyBank);

                if (slot != -1) {
                    modPlayer.Player.selectedItem = slot;
                }
            }
        }
    }

    public class VoidBagKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.VoidBagKeybind.JustPressed) {
                modPlayer.FindAndUseItem([ItemID.VoidLens, ItemID.ClosedVoidBag]);
            }
        }
    }

    public class BugNetKeybind : IProcessTriggers {
        int previousSlot = -1;
        int netUsed = 0;

        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.BugNetKeyBind.JustPressed && previousSlot == -1 && modPlayer.ItemToSelect == -1) {
                if (!modPlayer.FindAndUseItem(ItemID.GoldenBugNet, false))
                    if (!modPlayer.FindAndUseItem(ItemID.FireproofBugNet, false))
                        if (!modPlayer.FindAndUseItem(ItemID.BugNet, false))
                            return;

                previousSlot = modPlayer.Player.selectedItem;
            }

            if (previousSlot == -1) return;

            if (!KeybindSystem.BugNetKeyBind.Current && netUsed > 1) {
                modPlayer.SetItemToSelect(previousSlot, false);
                previousSlot = -1;
                netUsed = 0;
                return;
            }

            if (IsBugNet(modPlayer.Player.HeldItem.type)) {
                netUsed++;
                modPlayer.Player.controlUseItem = true;
            }
        }

        private static bool IsBugNet(int id) {
            if (id == ItemID.GoldenBugNet) return true;
            else if (id == ItemID.FireproofBugNet) return true;
            else if (id == ItemID.BugNet) return true;
            else return false;
        }
    }

    // --- Thorium ---
    public class RecallDungeonKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RecallDungeonKeyBind.JustPressed) {
                if (modPlayer.FindAndUseWishingGlass("Dungeon")) return;
                else modPlayer.FindAndUseThoriumItem("TheseusThread");
            }
        }
    }

    public class RecallTempleKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RecallTempleKeyBind.JustPressed) {
                modPlayer.FindAndUseWishingGlass("Temple");
            }
        }
    }

    public class RecallDeathLocationKeyBind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.RecallDeathLocationKeyBind.JustPressed) {
                if (modPlayer.FindAndUseWishingGlass("DeathLocation")) return;
                else modPlayer.FindAndUseThoriumItem("DeathGazersGlass");
            }
        }
    }

    public class TeleportRandomKeybind : IProcessTriggers {
        public void ProcessTriggers(KeyBindPlayer modPlayer, TriggersSet triggersSet) {
            if (KeybindSystem.TeleportRandomKeybind.JustPressed) {
                if (modPlayer.FindAndUseWishingGlass("Random")) return;
                else modPlayer.FindAndUseThoriumItem("SorcerersMirror");
            }
        }
    }
}