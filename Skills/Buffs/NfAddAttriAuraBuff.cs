using UnityEngine;
using System.Collections;

public class NfAddAttriAuraBuff : NfAttriAuraBuff {

    protected override bool Init()
    {
        bool needAdd = true;

        foreach (var b in owner.Buffs)
        {
			if (b.enabled && b is NfAddAttriAuraBuff && b.Attri == Attri)
            {
                if (priority < b.priority)
                {
                    needAdd = true;
                    b.End();
                }
                else
                {
                    needAdd = false;
                }
            }
        }

        if (needAdd == false)
            return false;

        bool ret = base.Init();
        if (ret)
        {
            NfBuff.ShowBuff(owner.camp, id, lv, Attri);
        }

        return ret;
    }
}
