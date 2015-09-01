using UnityEngine;
using System.Collections;
using LitJson;

public class NfLinkBuff : NfBuff
{
    GameObject linkEffObj;
    EffectObject ribbon;
    protected Character target;

    protected override bool Init()
    {
        linkEffObj = null;
        ribbon = null;

        Character t = Fight.Inst.FindBackTarget(owner);
        if (t != null)
        {
            LinkTarget(t);
            return true;
        }

        return false;
    }

    public void LinkTarget(Character beLinker)
    {
        target = beLinker;

        string eff = Effect;

        if (buffdata.Bind == "di")
        {
            PlaySelfEffect();
            //ribbon = beLinker.PlayEffect(Effect, -1f);
        }
        else
        {
            ribbon = owner.PlayEffect(eff, -1, delegate(GameObject obj)
            {
                GameObject begin = Helper.FindObject(obj, "Begin_01");
                GameObject end = Helper.FindObject(obj, "End_01");
                GameObject bone1 = owner.FindBone("Bip01");//Helper.FindObject(self.gameObject, "Bip01", false);
                GameObject bone2 = beLinker.FindBone("Bip01");//Helper.FindObject(beLinker.gameObject, "Bip01", false);
                begin.transform.position = bone1.transform.position;
                begin.transform.parent = bone1.transform;
                end.transform.position = bone2.transform.position;
                end.transform.parent = bone2.transform;
            });
        }
    }

    // 被攻击时执行
    public override int HandleBeHit(Character attacker, int damage, int addAnger)
    {
        if (target != null && target.IsDead == false)
        {
            float value = TotalValue;
            float d1 = damage * (1f - value);
            float d2 = damage * value;
            target.ShowHp();
            target.BeHit(null, (int)d2, HitEffect, false, "hit", addAnger, Sound);

            damage = (int)d1;
        }
        return damage;
    }

    public override void HandleFriendHpChange(Character friend)
    {
        if (friend == target)
        {
            if (friend.IsDead)
            {
                OnEnd();
            }
        }
    }

    protected override void OnEnd()
    {
        DestroyRibbon();
        base.OnEnd();
    }

    private void DestroyRibbon()
    {
        if (ribbon != null)
        {
            owner.DestroyEff(ribbon);
            ribbon = null;
        }
        if (linkEffObj != null)
        {
            GameObject.Destroy(linkEffObj);
            linkEffObj = null;
        }
    }
}
