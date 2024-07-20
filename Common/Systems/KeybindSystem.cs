using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.UI;
using Terraria;
using AdvancedControls.Common.Players;
using ReLogic.Graphics;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using AdvancedControls.Common.Configs;

namespace AdvancedControls.Common.Systems
{
    public class KeybindSystem : ModSystem
    {
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
        public static List<ModKeybind> InventoryReferenceKeyBinds { get; private set; } = new List<ModKeybind>();
        // --- Equipment Change Reference ---
        public static List<ModKeybind> EquipmentChangeReferenceKeyBinds { get; private set; } = new List<ModKeybind>();

        // --- Rulers ---
        public static ModKeybind RulerKeyBind { get; private set; }
        public static ModKeybind MechanicalRulerKeyBind { get; private set; }

        // --- QoL ---
        public static ModKeybind TeleportKeyBind { get; private set; }
        public static ModKeybind RecallKeyBind { get; private set; }
        public static ModKeybind RecallSpawnKeyBind { get; private set; }
        public static ModKeybind RecallOceanKeyBind { get; private set; }
        public static ModKeybind RecallUnderworldKeyBind { get; private set; }

        public override void Load()
        {
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
            for (int i = 0; i < ModContent.GetInstance<AdvancedControlsConfig>().InventoryReferenceCount; i++)
            {
                InventoryReferenceKeyBinds.Add(KeybindLoader.RegisterKeybind(Mod, "InventoryReference" + (i + 1), Microsoft.Xna.Framework.Input.Keys.None));
            }

            // --- Equipment Change Reference ---
            for(int i = 0; i < ModContent.GetInstance<AdvancedControlsConfig>().EquipmentChangeReferenceCount; i++)
            {
                EquipmentChangeReferenceKeyBinds.Add(KeybindLoader.RegisterKeybind(Mod, "EquipmentChangeReference" + (i + 1), Microsoft.Xna.Framework.Input.Keys.None));
            }

            //Rulers
            RulerKeyBind = KeybindLoader.RegisterKeybind(Mod, "RulerToggle", Microsoft.Xna.Framework.Input.Keys.K);
            MechanicalRulerKeyBind = KeybindLoader.RegisterKeybind(Mod, "MechanicalRulerToggle", Microsoft.Xna.Framework.Input.Keys.L);

            //QoL
            TeleportKeyBind = KeybindLoader.RegisterKeybind(Mod, "UseTeleport", Microsoft.Xna.Framework.Input.Keys.G);
            RecallKeyBind = KeybindLoader.RegisterKeybind(Mod, "Recall", Microsoft.Xna.Framework.Input.Keys.Z);
            RecallSpawnKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallSpawn", Microsoft.Xna.Framework.Input.Keys.U);
            RecallOceanKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallOcean", Microsoft.Xna.Framework.Input.Keys.I);
            RecallUnderworldKeyBind = KeybindLoader.RegisterKeybind(Mod, "RecallUnderworld", Microsoft.Xna.Framework.Input.Keys.O);
        }

        public override void Unload()
        {
            LootAllKeybind = null;
            DepositAllKeybind = null;
            QuickStackKeybind = null;

            DashKeybind = null;
            DashLeftKeybind = null;
            DashRightKeybind = null;

            CycleInventoryLeftKeybind = null;
            CycleInventoryRightKeybind = null;

            InventoryReferenceKeyBinds.Clear();
            EquipmentChangeReferenceKeyBinds.Clear();

            RulerKeyBind = null;
            MechanicalRulerKeyBind = null;
        }

        // --- For displaying which inventory slots the reference buttons are set to ---
        private LegacyGameInterfaceLayer textLayer;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryLayerIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Info Accessories Bar");

            if (inventoryLayerIndex != -1)
            {
                if (textLayer != null)
                {
                    layers.Remove(textLayer);
                }

                if(Main.playerInventory)
                {
                    textLayer = new LegacyGameInterfaceLayer("AdvancedControls: ItemSlotReference", DrawInventorySlotText, InterfaceScaleType.Game);
                    layers.Insert(inventoryLayerIndex, textLayer);
                }
            }
        }

        private bool DrawInventorySlotText()
        {
            bool drawn = false;

            for (int i = 0; i < InventoryReferenceKeyBinds.Count; i++)
            {
                if (InventoryReferenceKeyBindPlayer.inventoryReference[i] != -1)
                {
                    if (drawn)
                    {
                        Main.spriteBatch.Begin();
                    }

                    Main.spriteBatch.DrawString(((DynamicSpriteFont)FontAssets.ItemStack), (i + 1).ToString(), GetVectorForInventorySlot(InventoryReferenceKeyBindPlayer.inventoryReference[i]), Microsoft.Xna.Framework.Color.White);
                    Main.spriteBatch.End();

                    drawn = true;
                }
            }

            for (int i = 0; i < EquipmentChangeReferenceKeyBinds.Count; i++)
            {
                if (EquipmentChangeReferenceKeyBindPlayer.equipmentReference[i] != -1)
                {
                    if (drawn)
                    {
                        Main.spriteBatch.Begin();
                    }

                    Main.spriteBatch.DrawString(((DynamicSpriteFont)FontAssets.ItemStack), (i + 1 + InventoryReferenceKeyBinds.Count).ToString(), GetVectorForInventorySlot(EquipmentChangeReferenceKeyBindPlayer.equipmentReference[i]), Microsoft.Xna.Framework.Color.White);
                    Main.spriteBatch.End();

                    drawn = true;
                }
            }

            if (drawn)
            {
                Main.spriteBatch.Begin();
            }

            return true;
        }

        private static Vector2 GetVectorForInventorySlot(int slot)
        {
            int rowCount = (slot / 10);
            int columnCount = (slot % 10);

            float xFirstSlot = 46.5f * Main.UIScale;
            float yFirstSlot = 20f * Main.UIScale;

            float xSlotAdjustment = 47.5f * columnCount * Main.UIScale;
            float ySlotAdjustment = 47.5f * rowCount * Main.UIScale;

            return new Vector2(xFirstSlot + xSlotAdjustment, yFirstSlot + ySlotAdjustment);
        }
    }
}