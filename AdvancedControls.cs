using AdvancedControls.Common.Configs;
using AdvancedControls.Common.Players;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace AdvancedControls {
    public class AdvancedControls : Mod {
        AdvancedControlsConfig conf;

        public override void Load() {
            conf = ModContent.GetInstance<AdvancedControlsConfig>();

            On_Player.ScrollHotbar += On_Player_ScrollHotBar;
            On_Player.WallslideMovement += On_Player_WallslideMovement;
            On_Player.SmartSelect_GetToolStrategy += On_Player_SmartSelect_GetToolStrategy;
            On_Player.SmartSelect_PickToolForStrategy += On_Player_SmartSelect_PickToolForStrategy;
        }

        private void On_Player_SmartSelect_PickToolForStrategy(On_Player.orig_SmartSelect_PickToolForStrategy orig, Player self, int tX, int tY, int toolStrategy, bool wetTile) {
            if (conf.autoSelectRegrowthItem && toolStrategy == 3 && Main.tileAlch[Main.tile[tX, tY].TileType]) {
                int slot = self.FindItem([ItemID.StaffofRegrowth, ItemID.AcornAxe]);

                if (slot != -1) {
                    if (self.nonTorch == -1)
                    self.nonTorch = self.selectedItem;

                    self.selectedItem = slot;
                    return;
                }
            }

            orig(self, tX, tY, toolStrategy, wetTile);
        }

        private void On_Player_SmartSelect_GetToolStrategy(On_Player.orig_SmartSelect_GetToolStrategy orig, Player self, int tX, int tY, out int toolStrategy, out bool wetTile) {
            if (conf.altAutoSelectHammer && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
                for (int i = 0; i < 50; i++) {
                    if (self.inventory[i].hammer > 0) {
                        toolStrategy = 1;
                        wetTile = false;
                        return;
                    }
                }

            orig(self, tX, tY, out toolStrategy, out wetTile);
        }

        private void On_Player_WallslideMovement(On_Player.orig_WallslideMovement orig, Player self) {
            if (!ModContent.GetInstance<AdvancedControlsConfig>().disableWallClimb) orig(self);
        }

        private void On_Player_ScrollHotBar(On_Player.orig_ScrollHotbar orig, Player self, int Offset) {
            KeyBindPlayer kbp = Main.CurrentPlayer.GetModPlayer<KeyBindPlayer>();

            if (!conf.scrollEntireInventory && self.selectedItem >= 10)
                return;

            //Restore original values
            if (kbp.valuesChanged) {
                self.selectedItem = kbp.origSelectedItem;
                self.itemAnimation = kbp.origItemAnimation;
                self.itemTime = kbp.origItemTime;
                self.reuseDelay = kbp.origReuseDelay;
                PlayerInput.Triggers.Current.Hotbar1 = kbp.origHotbar1;
            }

            if (!conf.scrollEntireInventory) {
                while (Offset > 9) {
                    Offset -= 10;
                }

                while (Offset < 0) {
                    Offset += 10;
                }
            }

            int itemToSelect = self.selectedItem;

            itemToSelect += Offset;
            if (Offset != 0) {
                int num = itemToSelect - Offset;
                self.DpadRadial.ChangeSelection(-1);
                self.CircularRadial.ChangeSelection(-1);
                itemToSelect = num + Offset;
                self.nonTorch = -1;
            }

            if (self.changeItem >= 0) {
                itemToSelect = self.changeItem;
                self.changeItem = -1;
            }

            int clampVal = conf.scrollEntireInventory ? 50 : 10;
            if (itemToSelect != 58) {
                while (itemToSelect > 9) {
                    itemToSelect -= clampVal;
                }

                while (itemToSelect < 0) {
                    itemToSelect += clampVal;
                }
            }

            bool shouldPlaySound = kbp.origSelectedItem == 0 || !kbp.valuesChanged;
            //Checking for smart select here is necessary because... it somehow changes its value between the if and the call to the function that also checks it???
            if (itemToSelect != self.selectedItem && !PlayerInput.Triggers.Current.SmartSelect) kbp.SetItemToSelect(itemToSelect, false, shouldPlaySound);
        }
    }
}