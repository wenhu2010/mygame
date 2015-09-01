using UnityEngine;
using System.Collections;

public class NfFanJiBuff : NfBuff
{
    enum FanJiType
    {
        PreFanJi = 0,
        FanJi = 1
    }

    FanJiType type;

    protected override bool Init()
    {
        type = (FanJiType)Mathf.FloorToInt(BaseValue);
        PlaySelfEffect();
        return true;
    }

    private bool CanFanJi(Character target)
    {
        if (target.IsDead)
            return false;
        if (target.CurrSkill == null)
            return false;
        if (target.CurrSkill.isFanJi)
            return false;
        if (target.isActiveComboSkill)
            return false;
        if (target.IsAntiFanJi)
			return false;
		if (target.isFenShen)
			return false;
        return true;
    }

    public override int HandleBeHit(Character attacker, int damage, int addAnger)
    {
        if (type != FanJiType.FanJi)
            return damage;
        if (owner.IsDizzy || owner.IsBianYang)
            return damage;

        if (Fight.Rand(0, 101) <= Percent && CanFanJi(attacker))
        {
            NfSkill skill = DoTskill(owner, attacker);
            if (skill != null)
                skill.isFanJi = true;
        }
        return damage;
    }

    public override bool HandleBeHitPre(Character attacker)
    {
        if (type != FanJiType.PreFanJi)
            return false;
        if (owner.IsDizzy)
            return false;
        if (owner.CurrSkill != null)
        {
            if (owner.CurrSkill is NfThrowBack == false)
                return false;
            if (owner.CurrSkill is NfJiTui == false)
                return false;
        }

        if (Fight.Rand(0, 101) <= Percent && NfSkill.CanFanJi(attacker))
        {
            NfSkill skill = DoTskill(owner, attacker);
            if (skill != null)
                skill.isFanJi = true;
        }
        return false;
    }
}
