using UnityEngine;
using System.Collections;

public class NfAttriKeepBuff : NfAttriBuff {

    public override void Replace()
    {
        StopSelfEffect();

        _enabled = false;
        owner.RemoveBuff(this);
    }
}
