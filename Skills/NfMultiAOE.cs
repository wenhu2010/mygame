using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfMultiAOE : NfAttack
{
    protected override void StartAttack()
    {
        attacker.LookAt(targetChar);

        // 播放动画和特效        
        int atkcount = AtkC; // 攻击次数
        float atktime = HitTime; // 攻击间隔
        float time = atkcount * atktime;

        // 攻击
        OnAttack(time, atkcount, atktime);

        // 回到原来位置
        AddEvent(time, delegate
        {
            MoveBack();
            DestroyAllEff();
        });
    }

    void OnAttack(float time, float atkcount, float atktime)
    {
        PlayFireAnimAndEffect();

        // 目标伤害计算
        float dam = Damage;

        List<Character> targets = FindTargets(true);
        foreach (var t in targets)
        {
            for (int i = 0; i < atkcount; ++i)
            {
                float htime = atktime * i;
                CalcDamage(attacker, t, dam, htime);
            }
        }
    }
}
