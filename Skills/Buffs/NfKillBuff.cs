using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;

public class NfKillBuff : NfDieAuraBuff
{
    public override void HandleFriendHpChange(Character target)
    {
        return;
    }

    public override void HandleEnemyDead(Character target, Character attacker)
    {
        if (attacker == owner)
        {
            Soul s = new Soul();
            s.srcPos = target.position;
            s.animlen = target.GetAnimLength("die");
            s.destPos = owner.position;
            s.destPos.y += 1f;
            s.destChara = owner;
            s.totaltime = Vector3.Distance(target.position, s.destPos) / speed;
            souls.Add(s);
        }
    }
}

public class NfKillComboBuff : NfBuff
{
    Character GetTarget()
    {
        return Fight.Inst.FindAttackTarget(owner);
    }

    bool isActive = false;

    protected override bool Init()
    {
        return true;
    }

    public override void HandleEnemyDead(Character target, Character attacker)
    {
        if (owner.IsDizzy)
            return;

        if (attacker != owner)
            return;

        if (isActive)
            return;

        Character e = GetTarget();
        if (e != null)
        {
            StartBuff();
        }
        else
        {
            EndBuff();
        }
    }

    private void StartBuff()
    {
        isActive = true;
        ChangeAttri(owner);
        PlaySelfEffect();
        AddTbuff(owner);
    }

    private void EndBuff()
    {
        RestoreChangeAttri(owner);
        StopSelfEffect();
        RemoveTbuff(owner);
        isActive = false;
    }

    public override void HandleSkillEnd(NfSkill skill)
    {
        if (isActive)
        {
            var e = GetTarget();
            if (e != null)
            {
                DoTskill(owner, e);
                isActive = false;
                return;
            }
        }

        owner.MoveBack();
        EndBuff();
    }
}

public class NfDeadBuff : NfBuff
{
    protected override bool Init()
    {
        owner.visible = false;
        owner.HP = 0;
        return false;
    }
}
