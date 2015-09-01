using UnityEngine;
using System.Collections;
using LitJson;

public class NfSnipeBuff : NfBuff
{
    public Character locker;
    int num;
    EffectObject hitEff;

    protected override bool Init()
    {
        hitEff = null;
        num = Num;

        locker = Fight.Inst.FindAttackTarget(owner);
        if (locker != null)
        {
            PlaySelfEffect();
            owner.idleAnim = Anim;
            owner.Idle();
            owner.isXuLi = true;

            hitEff = locker.PlayEffect(HitEffect, -1f);
            owner.target = locker;
            AddTbuff(owner);
            return true;
        }
        return false;
    }

    protected override void OnEnd()
    {
        base.OnEnd();
        owner.idleAnim = "idle";
        string currAnim = owner.CurrAnim;
        if (currAnim == Anim)
            owner.Idle();
        owner.isXuLi = false;

        DestroyLockerEff();
        RemoveTbuff(owner);
    }

    public override void Break()
    {
        owner.ClearPushSkill();
        base.Break();
    }

    public override void HandleEnemyHpChange(Character target)
    {
        if (target.IsDead && locker == target)
        {
            DestroyLockerEff();

            FindLocker();
            //locker = Fight.Inst.FindExistBackTarget(target);
            //if (locker == null)
            //{
            //    locker = Fight.Inst.FindAttackTarget(self);
            //}
            //if (locker != null)
            //{
            //    hitEff = locker.PlayEffect(HitEffect, -1f);
            //    self.target = locker;
            //}
        }
    }

    void FindLocker()
    {
        locker = Fight.Inst.FindAttackTarget(owner);
        if (locker != null)
        {
            hitEff = locker.PlayEffect(HitEffect, -1f);
            owner.target = locker;
        }
    }

    public void DestroyLockerEff()
    {
        if (hitEff != null)
        {
            locker.DestroyEff(hitEff);
            hitEff = null;
        }
    }

    public override void HandleHuiHe()
    {
        if (locker == null)
        {
            FindLocker();
        }

        if (--num < 0)
        {
            owner.isXuLi = false;
            PushTskill();
			enabled = false;
        }
    }

    public override NfSkill HandleSkillBegin()
    {
        OnEnd();
        return null;
    }
}
