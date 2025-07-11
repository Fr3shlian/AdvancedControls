using AdvancedControls.Common.Configs;
using AdvancedControls.Common.Players;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace AdvancedControls {
    public class AdvancedControls : Mod {
        public override void Load() {
            On_Player.ScrollHotbar += On_Player_ScrollHotBar;
            On_Player.WallslideMovement += On_Player_WallslideMovement;
        }

        private void On_Player_WallslideMovement(On_Player.orig_WallslideMovement orig, Player self) {
            if (!ModContent.GetInstance<AdvancedControlsConfig>().disableWallClimb) orig(self);
        }

        private void On_Player_ScrollHotBar(On_Player.orig_ScrollHotbar orig, Player self, int Offset) {
            AdvancedControlsConfig conf = ModContent.GetInstance<AdvancedControlsConfig>();
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