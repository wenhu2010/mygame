using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfSXJQBuff : NfBuff
{
    protected int num = 0;
    protected override bool Init()
    {
        if (Attri != owner.ATTRIBUTE)
            return false;

        PlaySelfEffect();
        ChangeAttri(owner);
        return true;
    }

    protected override void OnEnd()
    {
        RestoreChangeAttri(owner);
        base.OnEnd();
    }
}
