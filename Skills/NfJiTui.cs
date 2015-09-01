using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfJiTui : NfSkill
{
    static float pauseTime = 0.7f;

	List<Character> jituiTargets = new List<Character>();

    protected override void OnSkillBegin()
    {
        Character target = attacker.target;
        attacker.LookAt(target);

        PlayFireAnimAndEffect();

		jituiTargets.Add(target);
        StartCoroutine(JiTuiTarget(target));
    }

    private IEnumerator JiTuiTarget(Character target)
    {
        float hitTime = HitTime;
        yield return new WaitForSeconds(hitTime);

        CalcDamage(attacker, target, Damage, 0f);

        Vector3 srcPos = new Vector3(target.position.x, target.position.y, target.position.z);
        Vector3 dir = target.position - attacker.position;
        dir.Normalize();
        Vector3 destPos = target.position + dir * 1.5f;

		if (target.CurrSkill != null && target.CurrSkill.skilldata.Func != "NfThrowBack") {
			target.BreakSkill();
		}
        target.position = srcPos;// BreakSkill 会回复位置
        target.Idle();

        MoveNoRotate(target, destPos, BulletSpeed, "idle", 1f, delegate
        {
            if (target.IsDead)
            {
                //target.PlayAnim("die");
                target.Dead();
                EndJiTui(target);
            }
            else
            {
                AddEvent(pauseTime, delegate
                {
                    MoveSrcPos(target, delegate
                    {
                        EndJiTui(target);
                    });
                });
            }
        });
    }

    private void EndJiTui(Character target)
    {
        jituiTargets.Remove(target);
        if (jituiTargets.Count == 0)
        {
            End();
        }
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();

        foreach (var t in jituiTargets)
        {
            t.MoveBack();
        }
		jituiTargets.Clear ();
    }
}
