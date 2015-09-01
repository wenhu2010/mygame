using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfAngerAuraBuff : NfBuff
{
    protected override bool Init()
    {
        PlaySelfEffect();
        return true;
    }

    public override void HandleFriendCastBigSkill(Character target)
    {
        List<Character> targets = FindTargets();
        if (targets.Contains(target))
            target.Anger = (int)TotalValue;
    }
}
