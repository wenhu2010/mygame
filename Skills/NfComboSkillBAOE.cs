using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfComboSkillBAOE : NfComboSkill
{
    protected override IEnumerator ComboAtk()
    {
        float time = TotalTime;
        if (time < 0.01f)
        {
            time = animation["heji"].length;
        }

        yield return new WaitForSeconds(HitTime);

        List<Character> targets = FindTargets(true);

        int dam = GetComboDamage(attackers);

        float bulletSpeed = BulletSpeed;
        if (bulletSpeed > 0f)
        {
            for (int i = 0; i < targets.Count; ++i)
            {
                Character e = targets[i];

                float s = Mathf.Abs(e.position.x - attacker.position.x);
                float t = s / bulletSpeed;
                CalcDamage(attacker, e, dam, t);
            }
        }
        else
        {
            for (int i = 0; i < targets.Count; ++i)
            {
                Character c = targets[i];
                CalcDamage(attacker, c, dam, 0f);
            }
        }

        yield return new WaitForSeconds(time - HitTime);

        DoTskill();
        End();
    }

    protected override void PlayCombAnimAndEffect()
    {
        for (int i = 0; i < attackers.Count; ++i)
        {
            Character c = attackers[i];
            c.position = Fight.Inst.GetSlotPos(attacker.camp, 0);
            c.rotation = c.SrcRotation;
            c.PlayAnim("heji");
        }

        EffectObject fireEffObj = PlayEffectSync(FireEffect, 3f);
        fireEffObj.obj.transform.position = Fight.Inst.GetSlotPos(attacker.camp, 0);
        fireEffObj.obj.transform.rotation = attacker.SrcRotation;
    }

    public override bool StartCameraAnim()
    {
        bool ret = base.StartCameraAnim();
        if (ret)
            HideAllFriend(attackers);
        return ret;
    }
}
