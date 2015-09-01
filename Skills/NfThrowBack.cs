using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfThrowBack : NfSkill
{
    List<Character> jituiTargets;

    protected override void OnSkillBegin()
    {
        jituiTargets = new List<Character>();

        AddBuff(attacker);

        PlayFireAnimAndEffect();

		Character target = attacker.target;		

        List<Character> targets = Fight.Inst.GetCurrAttackers();
        foreach (var t in targets)
        {
            if (target.camp == t.camp && CanFanJi(t))
            {
                StartCoroutine(ThrowTarget(t));
            }
        }
    }

    private IEnumerator ThrowTarget(Character target)
	{
		jituiTargets.Add(target);
        yield return new WaitForSeconds(HitTime);

        Vector3 srcPos = new Vector3(target.position.x, target.position.y, target.position.z);
        Vector3 destPos = target.SrcPos;

        target.BreakSkill();
        target.position = srcPos;// BreakSkill 会回复位置
        target.Idle();

        Vector2 v2DestPos = new Vector2(destPos.x, destPos.z);
        Vector2 v2SrcPos = new Vector2(target.position.x, target.position.z);
        Vector2 dir = new Vector2(v2DestPos.x - v2SrcPos.x, v2DestPos.y - v2SrcPos.y);
        dir.Normalize();

        float g = 70f;
        float speed = 15f;
        float dist = Vector2.Distance(v2DestPos, v2SrcPos);
        float total = dist / speed;
        float h = destPos.y - target.position.y;
        float v = 0f;
        if (total > 0f) v = (h - 0.5f * (-g) * total * total) / total;
        Vector3 vo = new Vector3(dir.x * speed, v, dir.y * speed);
        Vector3 a = new Vector3(0f, -g, 0f);

        float xAng = 0;
        float time = 0;
        AddEvent(delegate
        {
            time += Time.deltaTime;
            Vector3 p = vo * time + 0.5f * a * time * time;
            target.position = srcPos + p;

            xAng += Time.deltaTime * 1000000;
            target.rotation = Quaternion.Euler(new Vector3(0, xAng, 0));

            if (time >= total)
            {
                // 目标伤害计算
                CalcDamage(attacker, target, Damage, 0f);
                target.position = target.SrcPos;
                target.rotateBack();

                EndJiTui(target);
                return true;
            }

            return false;
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
        RemoveBuff(attacker);

        DestroyAll();

//        foreach (var t in jituiTargets)
//        {
//            t.position = t.SrcPos;
//            t.Idle();
//        }
//
//        Character target = attacker.target;
//        target.position = target.SrcPos;
//        target.Idle();

		foreach (var t in jituiTargets)
		{
			t.MoveBack();
		}
    }
}
