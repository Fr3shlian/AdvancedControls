using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace AdvancedControls.Common.GlobalItems
{
    public class ShellphoneGlobal : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => new List<int>() { ItemID.Shellphone, ItemID.ShellphoneSpawn, ItemID.ShellphoneOcean, ItemID.ShellphoneHell }.Contains(entity.type);
        public static int requiredShellPhone = -1;

        //Used to make non-Player inputs be able to switch the shellphone mode
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (requiredShellPhone != -1)
            {
                item.type = requiredShellPhone;
                requiredShellPhone = -1;
            }

            return false;
        }
    }
}
