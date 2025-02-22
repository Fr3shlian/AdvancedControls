using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.UI;
using Terraria;
using AdvancedControls.Common.Players;
using ReLogic.Graphics;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using AdvancedControls.Common.Configs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Animations;

namespace AdvancedControls.Common.Systems {
    public class KeybindSystem : ModSystem {
        // --- Chest controls ---
        public static ModKeybind LootAllKeybind { get; private set; }
        public static ModKeybind DepositAllKeybind { get; private set; }
        public static ModKeybind QuickStackKeybind { get; private set; }

        // --- Dash ---
        public static ModKeybind DashKeybind { get; private set; }
        public static ModKeybind DashLeftKeybind { get; private set; }
        public static ModKeybind DashRightKeybind { get; private set; }

        // --- Cycle Inventory ---
        public static ModKeybind CycleInventoryLeftKeybind { get; private set; }
        public static ModKeybind CycleInventoryRightKeybind { get; private set; }

        // --- Inventory Reference ---
        public static List<ModKeybind> DynamicHotbarKeyBinds { get; private set; } = [];
        // --- Equipment Change Reference ---
        public static List<ModKeybind> EquipmentChangeReferenceKeyBinds { get; private set; } = [];
        // For Equipment Change Indicator
        private static Item item1 = null;
        private static Item item2 = null;
        private static float alpha = 0f;

        // --- Rulers ---
        public static ModKeybind RulerKeyBind { get; private set; }
        public static ModKeybind MechanicalRulerKeyBind { get; private set; }

        // --- QoL ---
        public static ModKeybind TeleportKeyBind { get; private set; }
        public static ModKeybind RecallKeyBind { get; private set; }
        public static ModKeybind RecallSpawnKeyBind { get; private set; }
        public static ModKeybind RecallOceanKeyBind { get; private set; }
        public static ModKeybind RecallUnderworldKeyBind { get; private set; }
        public static ModKeybind RecallReturnKeyBind { get; private set; }

        // --- Thorium Mod ---
        public static ModKeybind RecallDungeonKeyBind { get; private set; }
        public static ModKeybind RecallTempleKeyBind { get; private set; }
        public static ModKeybind RecallDeathLocationKeyBind { get; private set; }
        public static ModKeybind TeleportRandomKeybind { get; private set; }
        public static Mod Thorium = null;

        public override void Load() {
            // --- Chest controls ---
            LootAllKeybind = KeybindLoader.RegisterKeybind(Mod, "LootAll", Microsoft.Xna.Framework.Input.Keys.Up);
            DepositAllKeybind = KeybindLoader.RegisterKeybind(Mod, "DepositAll", Microsoft.Xna.Framework.Input.Keys.None);
            QuickStackKeybind = KeybindLoader.RegisterKeybind(Mod, "QuickStack", Microsoft.Xna.Framework.Input.Keys.Down);

            // --- Dash ---
            DashKeybind = KeybindLoader.RegisterKeybind(Mod, "Dash", Microsoft.Xna.Framework.Input.Keys.Q);
            DashLeftKeybind = KeybindLoader.RegisterKeybind(Mod, "DashLeft", Microsoft.Xna.Framework.Input.Keys.None);
            DashRightKeybind = KeybindLoader.RegisterKeybind(Mod, "DashRight", Microsoft.Xna.Framework.Input.Keys.None);

            // --- Cycle Inventory ---
            CycleInventoryLeftKeybind = KeybindLoader.RegisterKeybind(Mod, "CycleInventoryLeft", Microsoft.Xna.Framework.Input.Keys.None);
            CycleInventoryRightKeybind = KeybindLoader.RegisterKeybind(Mod, "CycleInventoryRight", Microsoft.Xna.Framework.Input.Keys.None);

            // --- Inventory Reference ---
            for (int i = 0; i < ModContent.GetInstance<AdvancedControlsConfig>().dynamicHotbarCount; i++) {
                DynamicHotbarKeyBinds.Add(KeybindLoader.RegisterKeybind(Mod, "DynamicHotbar" + (i + 1), Microsoft.Xna.Framework.Input.Keys.None));
            }

            // --- Equipment Change Reference ---
            for (int i = 0; i < ModContent.GetInstance<AdvancedControlsConfig>().equipmentChangeReferenceCount; i++) {
                EquipmentChangeReferenceKeyBinds.Add(KeybindLoader.RegisterKeybind(Mod, "EquipmentChangeReference" + (i + 1), Microsoft.Xna.Framework.Input.Keys.None));
            }

            // --- Rulers ---
            RulerKeyBind = KeybindLoader.RegisterKeybind(Mod, "RulerToggle", Microsoft.Xna.Framework.Input.Keys.K);
            MechanicalRulerKeyBind = KeybindLoader.RegisterKeybind(Mod, "MechanicalRulerToggle", Microsoft.Xna.Framework.Input.Keys.L);

            // --- QoL ---
            TeleportKeyBind = KeybindLoader.RegisterKeybind(Mod, "UseTeleport", Microsoft.Xna.Framework.Input.Keys.G);
            RecallKeyBind = KeybindLoader.RegisterKeybind(Mod, "Recall", Microsoft.Xna.Framework.Input.Keys.NumPad1);
            RecallSpawnKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallSpawn", Microsoft.Xna.Framework.Input.Keys.NumPad2);
            RecallOceanKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallOcean", Microsoft.Xna.Framework.Input.Keys.NumPad3);
            RecallUnderworldKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallUnderworld", Microsoft.Xna.Framework.Input.Keys.NumPad4);
            RecallReturnKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallReturn", Microsoft.Xna.Framework.Input.Keys.NumPad5);

            // --- Thorium Mod ---
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium)) {
                Thorium = thorium;
                RecallDungeonKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallDungeon", Microsoft.Xna.Framework.Input.Keys.NumPad6);
                RecallTempleKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallTemple", Microsoft.Xna.Framework.Input.Keys.NumPad7);
                RecallDeathLocationKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallDeathLocation", Microsoft.Xna.Framework.Input.Keys.NumPad8);
                TeleportRandomKeybind = KeybindLoader.RegisterKeybind(Mod, "TeleportRandomLocation", Microsoft.Xna.Framework.Input.Keys.NumPad9);
            }
        }

        public override void Unload() {
            LootAllKeybind = null;
            DepositAllKeybind = null;
            QuickStackKeybind = null;

            DashKeybind = null;
            DashLeftKeybind = null;
            DashRightKeybind = null;

            CycleInventoryLeftKeybind = null;
            CycleInventoryRightKeybind = null;

            DynamicHotbarKeyBinds.Clear();
            EquipmentChangeReferenceKeyBinds.Clear();

            RulerKeyBind = null;
            MechanicalRulerKeyBind = null;

            TeleportKeyBind = null;
            RecallKeyBind = null;
            RecallSpawnKeyBind = null;
            RecallOceanKeyBind = null;
            RecallUnderworldKeyBind = null;
            RecallReturnKeyBind = null;

            if (Thorium != null) {
                RecallDungeonKeyBind = null;
                RecallTempleKeyBind = null;
                RecallDeathLocationKeyBind = null;
                TeleportRandomKeybind = null;
            }
        }

        // --- For displaying which inventory slots the reference buttons are set to ---
        private LegacyGameInterfaceLayer textLayer;
        private LegacyGameInterfaceLayer equipmentChangeLayer;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int inventoryLayerIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Info Accessories Bar");

            if (inventoryLayerIndex != -1) {
                if (textLayer != null) {
                    layers.Remove(textLayer);
                }

                if (Main.playerInventory) {
                    textLayer = new LegacyGameInterfaceLayer("AdvancedControls: ItemSlotReference", DrawInventorySlotText, InterfaceScaleType.Game);
                    layers.Insert(inventoryLayerIndex, textLayer);
                }
            }

            inventoryLayerIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Interface Logic 4");

            if (inventoryLayerIndex != -1) {
                if (equipmentChangeLayer != null) {
                    layers.Remove(equipmentChangeLayer);
                }

                equipmentChangeLayer = new LegacyGameInterfaceLayer("AdvancedControls: EquipmentChangeIndicator", DrawEquipmentChangeIndicator, InterfaceScaleType.Game);
                layers.Insert(inventoryLayerIndex, equipmentChangeLayer);
            }
        }

        public override void UpdateUI(GameTime gameTime) {
            if (alpha != 0f) {
                alpha -= 0.02f;
            }
        }

        private bool DrawInventorySlotText() {
            DynamicHotbarKeyBindPlayer kbp = Main.CurrentPlayer.GetModPlayer<DynamicHotbarKeyBindPlayer>();
            EquipmentChangeReferenceKeyBindPlayer erp = Main.CurrentPlayer.GetModPlayer<EquipmentChangeReferenceKeyBindPlayer>();

            for (int i = 0; i < DynamicHotbarKeyBinds.Count; i++) {
                if (kbp.GetReference(i) != -1)
                    Main.spriteBatch.DrawString((DynamicSpriteFont)FontAssets.ItemStack, (i + 1).ToString(), GetVectorForInventorySlot(kbp.GetReference(i)), Color.White, 0f, Vector2.Zero, Main.UIScale, SpriteEffects.None, 0f);
            }

            for (int i = 0; i < EquipmentChangeReferenceKeyBinds.Count; i++) {
                if (erp.EquipmentReference[i].Slot != -1)
                    Main.spriteBatch.DrawString((DynamicSpriteFont)FontAssets.ItemStack, "E" + (i + 1), GetVectorForInventorySlot(erp.EquipmentReference[i].Slot, -11f), Color.White, 0f, Vector2.Zero, Main.UIScale * (erp.EquipmentReference[i].Slot < 50 ? 1f : 0.6f), SpriteEffects.None, 0f);
            }

            return true;
        }

        //Values from Main
        //Inventory X: (20f + columnCount * 56 * Main.inventoryScale) * Main.UIScale
        //Inventory Y: (20f + rowCount * 56 * Main.inventoryScale) * Main.UIScale);
        //Coins/Ammo Y: int num103 = (int)(85f + (float)(num101 * 56) * inventoryScale + 20f);
        private static Vector2 GetVectorForInventorySlot(int slot, float xAdjustment = 0f) {
            if (slot < 50) {
                float inventoryScale = 0.75f;
                int rowCount = slot / 10;
                int columnCount = slot % 10;

                float xFirstSlot = 51.5f + xAdjustment;
                float yFirstSlot = 21.5f;

                float xAnySlot = 56 * columnCount * inventoryScale;
                float yAnySlot = rowCount * 56 * inventoryScale;

                float xAnySlotAdjust = 7.5f * columnCount * inventoryScale;
                float yAnySlotAdjust = 7f * rowCount * inventoryScale;

                return new Vector2((xFirstSlot + xAnySlot + xAnySlotAdjust) * Main.UIScale, (yFirstSlot + yAnySlot + yAnySlotAdjust) * Main.UIScale);
            } else {
                float inventoryScale = 0.6f;
                int rowCount = (slot - 50) % 4;
                float xPosition = (slot < 54 ? 497f : 534f) + 28f + xAdjustment;

                float yFirstSlot = 106f;
                float yAnySlot = rowCount * 56 * inventoryScale;
                float yAnySlotAdjust = rowCount * -0.5f * inventoryScale;

                return new Vector2(xPosition * Main.UIScale, (yFirstSlot + yAnySlotAdjust + yAnySlot) * Main.UIScale);
            }
        }

        private bool DrawEquipmentChangeIndicator() {
            if (alpha != 0f) {
                Texture2D tex1 = null, tex2 = null;

                if (item1 != null) {
                    if (item1.ModItem != null) tex1 = ModContent.Request<Texture2D>(item1.ModItem.Texture).Value;
                    else tex1 = TextureAssets.Item[item1.type].Value;
                }

                if (item2 != null) {
                    if (item2.ModItem != null) tex2 = ModContent.Request<Texture2D>(item2.ModItem.Texture).Value;
                    else tex2 = TextureAssets.Item[item2.type].Value;
                }

                float playerCenterX = Main.screenWidth / 2;
                float spacing = 40f;
                float abovePlayerY = Main.screenHeight / 2 - Main.CurrentPlayer.height + 5;

                if (tex1 != null) Main.spriteBatch.Draw(tex1, new Vector2(playerCenterX - spacing - tex1.Width / 2, abovePlayerY - tex1.Height / 2), Color.White * alpha);
                if (tex2 != null) Main.spriteBatch.Draw(tex2, new Vector2(playerCenterX + spacing - tex2.Width / 2, abovePlayerY - tex2.Height / 2), Color.White * alpha);
                if (tex1 != null || tex2 != null) Main.spriteBatch.DrawString((DynamicSpriteFont)FontAssets.ItemStack, "->", new Vector2(playerCenterX - 17, Main.screenHeight / 2 - Main.CurrentPlayer.height * 1.2f), Color.White * alpha, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            }

            return true;
        }

        public static void SetItemRefsForIndicator(Item item1, Item item2) {
            KeybindSystem.item1 = item1;
            KeybindSystem.item2 = item2;
            alpha = 1f;
        }
    }
}