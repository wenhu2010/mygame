using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfDefendLinkBuff : NfLinkBuff
{
    protected override bool Init()
    {
        return true;
    }
}

public class NfDefendBuff : NfBuff
{
    bool init;
    List<Character> targets;

    protected override bool Init()
    {
        init = false;
        targets = FindTargets();
        foreach (var target in targets)
        {
            AddTbuff(target);
        }
        return true;
    }

    protected override void OnEnd()
    {
        base.OnEnd();

        foreach (var target in targets)
        {
            RemoveTbuff(target);
        }
    }

    public override void Update()
    {
        if (!init)
        {
            foreach (var target in targets)
            {
                foreach (var b in target.Buffs)
                {
                    if (b.enabled && b is NfDefendLinkBuff)
                    {
                        NfDefendLinkBuff dlb = (NfDefendLinkBuff)b;
                        dlb.LinkTarget(owner);

                        init = true;
                    }
                }
            }
        }
    }
}
