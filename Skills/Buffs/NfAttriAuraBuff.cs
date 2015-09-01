using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfAttriAuraBuff : NfBuff
{
    Dictionary<Character, EffectObject> charEffs;

    protected override bool Init()
    {
        PlaySelfEffect();

        charEffs = new Dictionary<Character, EffectObject>();

        UpdateAuraAttri();
        return true;
    }

    private void UpdateAuraAttri()
    {
        List<Character> targets = FindTargets();
        var eff = HitEffect;
        foreach (var t in targets)
        {
            if (Attri != -1 && Attri != t.ATTRIBUTE)
                continue;

            EffectObject effObj = t.PlayEffect(eff, -1f);
            charEffs.Add(t, effObj);

            ChangeAttri(t);
            AddTbuff(t);
        }
    }

    protected override void OnEnd()
    {
        RestoreAuraAttri();
        base.OnEnd();
    }

    private void RestoreAuraAttri()
    {
        foreach (var c in charEffs.Keys)
        {
            RemoveTbuff(c);
            RestoreChangeAttri(c);
            if (charEffs.ContainsKey(c))
                c.DestroyEff(charEffs[c]);
        }
        charEffs.Clear();
    }

    public override void HandleFriendHpChange(Character target)
    {
        if (target.IsDead && charEffs.ContainsKey(target))
        {
            RemoveTbuff(target);
            RestoreChangeAttri(target);
            target.DestroyEff(charEffs[target]);
            charEffs.Remove(target);
        }
    }

    public override void HandleFriendPosChange(Character target)
    {
        RestoreAuraAttri();
        UpdateAuraAttri();
    }
}
