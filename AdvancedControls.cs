using System.Collections.Generic;
using AdvancedControls.Common.Configs;
using AdvancedControls.Common.Players;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace AdvancedControls {
    public class AdvancedControls : Mod {
        AdvancedControlsConfig conf;
        bool altOverride = false;

        public override void Load() {
            conf = ModContent.GetInstance<AdvancedControlsConfig>();

            On_Player.ScrollHotbar += On_Player_ScrollHotBar;
            On_Player.WallslideMovement += On_Player_WallslideMovement;
            On_Player.SmartSelect_GetToolStrategy += On_Player_SmartSelect_GetToolStrategy;
            On_Player.SmartSelect_PickToolForStrategy += On_Player_SmartSelect_PickToolForStrategy;
        }

        private static void SmartSelect_SelectItem(Player self, int slot) {
            if (self.nonTorch == -1)
                self.nonTorch = self.selectedItem;

            self.selectedItem = slot;
        }

        private void On_Player_SmartSelect_PickToolForStrategy(On_Player.orig_SmartSelect_PickToolForStrategy orig, Player self, int tX, int tY, int toolStrategy, bool wetTile) {
            Tile curTile = Main.tile[tX, tY];
            bool enterAutoRegrowth = conf.autoSelectRegrowthItem && Main.tileAlch[curTile.TileType];
            Tile nextTile = Main.tile[tX, tY + 1];
            bool enterAutoSeed = conf.autoSelectPlanterSeeds && nextTile.TileType == TileID.PlanterBox;

            if (!altOverride && (enterAutoRegrowth || enterAutoSeed)) {
                //Copied from SmartSelect_GetToolStrategy to determine whether the player is in range
                int num = 0;
                int num2 = 0;
                if (self.position.X / 16f >= tX)
                    num = (int)(self.position.X / 16f) - tX;

                if ((self.position.X + self.width) / 16f <= tX)
                    num = tX - (int)((self.position.X + self.width) / 16f);

                if (self.position.Y / 16f >= tY)
                    num2 = (int)(self.position.Y / 16f) - tY;

                if ((self.position.Y + self.height) / 16f <= tY)
                    num2 = tY - (int)((self.position.Y + self.height) / 16f);

                if (num <= Player.tileRangeX && num2 <= Player.tileRangeY) {
                    if (enterAutoRegrowth) {
                        int slot = self.FindItem([ItemID.StaffofRegrowth, ItemID.AcornAxe]);

                        if (slot != -1) {
                            SmartSelect_SelectItem(self, slot);
                            return;
                        }

                        int seedsToFind = -1;

                        switch (curTile.TileFrameX) {
                            case 0:
                                seedsToFind = ItemID.DaybloomSeeds;
                                break;
                            case 18:
                                seedsToFind = ItemID.MoonglowSeeds;
                                break;
                            case 36:
                                seedsToFind = ItemID.BlinkrootSeeds;
                                break;
                            case 54:
                                seedsToFind = ItemID.DeathweedSeeds;
                                break;
                            case 72:
                                seedsToFind = ItemID.WaterleafSeeds;
                                break;
                            case 90:
                                seedsToFind = ItemID.FireblossomSeeds;
                                break;
                            case 108:
                                seedsToFind = ItemID.ShiverthornSeeds;
                                break;
                        }

                        if (seedsToFind != -1) slot = self.FindItem(seedsToFind);

                        if (slot != -1) {
                            SmartSelect_SelectItem(self, slot);
                            return;
                        }
                    } else if (enterAutoSeed) {
                        int seedsToFind = -1;

                        if (conf.matchSeedsWithPlanter)
                            switch (nextTile.TileFrameY) {
                                case 0:
                                    seedsToFind = ItemID.DaybloomSeeds;
                                    break;
                                case 18:
                                    seedsToFind = ItemID.MoonglowSeeds;
                                    break;
                                case 36:
                                case 54:
                                    seedsToFind = ItemID.DeathweedSeeds;
                                    break;
                                case 72:
                                    seedsToFind = ItemID.BlinkrootSeeds;
                                    break;
                                case 90:
                                    seedsToFind = ItemID.WaterleafSeeds;
                                    break;
                                case 108:
                                    seedsToFind = ItemID.ShiverthornSeeds;
                                    break;
                                case 126:
                                    seedsToFind = ItemID.FireblossomSeeds;
                                    break;
                            }

                        int slot = -1;

                        if (seedsToFind != -1) slot = self.FindItem(seedsToFind);

                        if (slot == -1) slot = self.FindItem([ItemID.DaybloomSeeds, ItemID.BlinkrootSeeds, ItemID.WaterleafSeeds, ItemID.ShiverthornSeeds, ItemID.MoonglowSeeds, ItemID.DeathweedSeeds, ItemID.FireblossomSeeds]);

                        if (slot != -1) {
                            SmartSelect_SelectItem(self, slot);
                            return;
                        }
                    }
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
                        altOverride = true;
                        return;
                    }
                }

            altOverride = false;
            orig(self, tX, tY, out toolStrategy, out wetTile);
        }

        private void On_Player_WallslideMovement(On_Player.orig_WallslideMovement orig, Player self) {
            if (!ModContent.GetInstance<AdvancedControlsConfig>().disableWallClimb) orig(self);
        }

        private void On_Player_ScrollHotBar(On_Player.orig_ScrollHotbar orig, Player self, int Offset) {
            KeyBindPlayer kbp = Main.CurrentPlayer.GetModPlayer<KeyBindPlayer>();

            //Restore original values
            if (kbp.valuesChanged) {
                self.selectedItem = kbp.origSelectedItem;
                self.itemAnimation = kbp.origItemAnimation;
                self.itemTime = kbp.origItemTime;
                self.reuseDelay = kbp.origReuseDelay;
                PlayerInput.Triggers.Current.Hotbar1 = kbp.origHotbar1;
            }

            if (PlayerInput.Triggers.Current.SmartSelect) return;

            bool shouldScrollEntireInventory = conf.ShouldScrollEntireInventory();
            if (!shouldScrollEntireInventory && self.selectedItem >= 10)
                return;

            bool noItem = self.itemAnimation == 0 && self.ItemTimeIsZero && self.reuseDelay == 0;
            if (noItem && Offset != 0) kbp.ClearDynamicHotbarMemory();

            if (!shouldScrollEntireInventory) {
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
                if (noItem) SoundEngine.PlaySound(SoundID.MenuTick);
                int num = itemToSelect - Offset;
                self.DpadRadial.ChangeSelection(-1);
                self.CircularRadial.ChangeSelection(-1);
                itemToSelect = num + Offset;
                self.nonTorch = -1;
            }

            if (self.changeItem >= 0) {
                if (noItem && itemToSelect != self.changeItem)
                    SoundEngine.PlaySound(SoundID.MenuTick);

                itemToSelect = self.changeItem;
                self.changeItem = -1;
            }

            int clampVal = shouldScrollEntireInventory ? 50 : 10;
            if (itemToSelect != 58) {
                while (itemToSelect > 9) {
                    itemToSelect -= clampVal;
                }

                while (itemToSelect < 0) {
                    itemToSelect += clampVal;
                }
            }

            if (noItem) self.selectedItem = itemToSelect;
            else {
                bool shouldPlaySound = kbp.origSelectedItem == 0 || !kbp.valuesChanged;
                kbp.SetItemToSelect(itemToSelect, false, shouldPlaySound);
                if (itemToSelect != self.selectedItem) kbp.ClearDynamicHotbarMemory();
            }
        }
    }
}