using UnityEngine;
using System.Collections;

public class NfFenLieBuff : NfBuff
{
    int num;
    protected override bool Init()
    {
        num = Num;
        return true;
    }

    public override void HandleHuiHe()
    {
        if (--num < 0)
        {
            int slotnum = 0;
            for (int i = 0; i < GameMgr.GamePlayer.MAXOPENSLOTNUM; ++i)
            {
                if (Fight.Inst.IsEmptySlot(owner.camp, i))
                    ++slotnum;
            }
            if (slotnum > 0)
            {
                PushTskill();
                End();
            }
        }
    }
}
