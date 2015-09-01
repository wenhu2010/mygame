using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfRandCurveBullet : NfCurveBullet
{
    protected override IEnumerator FireCurveBullet()
    {
        // 播放动画和特效
        PlaySingAnimAndEffect();

        // 发射子弹
        float singTime = SingTime;
        yield return new WaitForSeconds(singTime);

        string fireBone = FireBone;
        string bulletModel = BulletModel;

        List<Character> atkTargets = new List<Character>();
        List<Character> targets = FindTargets(true);
        bulletNum = AtkC;
        if (targets.Count > 0)
        {
            for (int i = 0; i < bulletNum; ++i)
            {
                int n = Fight.Rand(0, targets.Count);
                atkTargets.Add(targets[n]);
            }
            foreach (var t in atkTargets)
            {
                Fire(t, fireBone, bulletModel);
                yield return new WaitForSeconds(0.03f);
            }
        }
        else
        {
            End();
        }
    }
}
