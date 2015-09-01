using UnityEngine;
using System.Collections;

public class NfSnipe : NfSkill
{
    protected override void OnSkillBegin()
    {
        StartCameraAnim();

        StartCoroutine(Attack());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }

    IEnumerator Attack()
    {
        AddEvent(HitTime, delegate
        {
			int buffId = Buff;
            int buffLv = lv;
            attacker.AddBuff(attacker, buffId, buffLv);
        });

        float singTime = attacker.GetAnimLength(SingAnim);
        PlaySingAnimAndEffect();
        yield return new WaitForSeconds(singTime);

        End();
    }
}

public class NfSnipeFire : NfBullet
{
    protected override void OnSkillBegin()
    {
        foreach (var b in attacker.Buffs)
        {
            if (b.isDestroy == false && b is NfSnipeBuff)
            {
                NfSnipeBuff sb = (NfSnipeBuff)b;
                //sb.DestroyLockerEff();
                attacker.target = Fight.Inst.FindAttackTarget(attacker);
				sb.End();
                break;
            }
        }

        if (attacker.target != null)
        {
            base.OnSkillBegin();
        }
        else
        {
            End();
        }
    }

    protected override void OnSkillEnd()
    {
        base.OnSkillEnd();

        foreach (var b in attacker.Buffs)
        {
            if (b.enabled && b is NfSnipeBuff)
            {
                b.End();
            }
        }
    }

    protected override IEnumerator FireBullet()
    {
        // 播放动画和特效
        PlaySingAnimAndEffect();

        yield return new WaitForSeconds(SingTime);

        ShowMesh(false);

        // 发射子弹
        bulletNum = 1;
		Fire(attacker.target, fireBone, BulletModel);
    }
}
