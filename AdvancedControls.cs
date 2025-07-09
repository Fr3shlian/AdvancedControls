using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace AdvancedControls {
    public class AdvancedControls : Mod {
        public override void Load() {
            On_Player.ClampHotbarOffset += Player_ClampHotbarOffset_On;
            IL_Player.ScrollHotbar += Player_ScrollHotbar_IL;
        }

        //Return immediately without clamping
        private int Player_ClampHotbarOffset_On(On_Player.orig_ClampHotbarOffset orig, int Offset) {
            return Offset;
        }

        private void Player_ScrollHotbar_IL(ILContext il) {
            ILCursor c = new(il);

            //Remove early return if (selectedItem >= 10)
            if (c.TryGotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Player>("selectedItem"),
                i => i.MatchLdcI4(10),
                i => i.MatchBge(out _),
                i => i.MatchRet()
            )) {
                c.RemoveRange(2);
            }

            //Change selectedItem clamp from 10 to 50
            c.Goto(0);
            while (c.TryGotoNext(i => i.MatchLdcI4(10))) {
                c.Next.OpCode = OpCodes.Ldc_I4_S;
                c.Next.Operand = (sbyte)50;
            }
        }
    }
}