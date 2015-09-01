using UnityEngine;
using System.Collections;

public class NfHuiHeAttriBuff : NfBuff
{
    protected override bool Init()
    {
        PlaySelfEffect();
        return true;
    }

    public override void HandleHuiHe()
    {
        ChangeAttri(owner);
    }
}
