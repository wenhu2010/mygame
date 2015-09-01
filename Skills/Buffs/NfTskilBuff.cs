using UnityEngine;
using System.Collections;

public class NfTskilBuff : NfBuff
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
            PushTskill();
            End();
        }
    }
}

