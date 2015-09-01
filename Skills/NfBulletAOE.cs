using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfBulletAOE : NfSkill {

    protected override void OnSkillBegin()
    {
        StartCameraAnim();
        StartCoroutine(Fire());
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }

    IEnumerator Fire()
    {
        PlayFireAnimAndEffect();
        yield return new WaitForSeconds(HitTime);

        List<Character> targets = FindTargets(true);

        float bulletSpeed = BulletSpeed;
        if (bulletSpeed > 0f)
        {
            for (int i = 0; i < targets.Count; ++i)
            {
                Character e = targets[i];

                float s = Mathf.Abs(e.position.x - attacker.position.x);
                float t = s / bulletSpeed;
                CalcDamage(attacker, e, Damage, t);
            }
        }
        else
        {
            for (int i = 0; i < targets.Count; ++i)
            {
                Character e = targets[i];
                CalcDamage(attacker, e, Damage, 0f);
            }
        }

        yield return new WaitForSeconds(TotalTime - HitTime);

        DoTskill();
        End();
    }
}
