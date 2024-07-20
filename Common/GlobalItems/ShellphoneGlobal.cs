using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace AdvancedControls.Common.GlobalItems
{
    public class ShellphoneGlobal : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => new List<int>() { ItemID.Shellphone, ItemID.ShellphoneSpawn, ItemID.ShellphoneOcean, ItemID.ShellphoneHell }.Contains(entity.type);
        public static bool specialUse = false;

        //Used to make non-Player inputs be able to switch the shellphone mode
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (specialUse)
                if (item.type == ItemID.ShellphoneHell)
                    item.type = ItemID.Shellphone;
                else
                    item.type++;

            return false;
        }
    }
}
