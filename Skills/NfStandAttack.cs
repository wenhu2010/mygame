using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfStandAttack : NfSkill
{
    protected override void OnSkillBegin()
    {
        List<Character> targets = FindTargets(true);
        if (targets.Count == 0)
        {
            End();
            return;
        }

        StartCameraAnim();

        // 播放动画和特效
        float animLen = attacker.GetAnimLength(FireAnim);
        PlayFireAnimAndEffect();

        float time = TotalTime;
        if (time < 0.01f)
            time = animLen;
        string keepEff = KeepEffect;
        if (!string.IsNullOrEmpty(keepEff))
        {
            EffectObject eff = PlayEffect(keepEff, time);
            if (eff != null)
            {
                switch (rangeType)
                {
                    case NfSkill.RangeType.FrontRow:
                        {
                            int slot = targets[0].slot;

                            if (slot >= 9)
                            {
                                slot = 11;
                            }
                            else if (slot >= 4)
                            {
                                slot = 6;
                            }
                            else if (slot >= 1)
                            {
                                slot = 2;
                            }

                            Vector3 pos = Fight.Inst.GetSlotPos(targets[0].camp, slot);
                            eff.obj.transform.position = new Vector3(pos.x, 0.01f, pos.z);
                        }break;
                }
            }
        }

        float dam = Damage;

        int atkc = AtkC;
        float hitTime = HitTime;
        if (atkc > 1)
        {
            foreach (var t in targets)
            {
                for (int i = 0; i < atkc; ++i)
                {
                    float htime = hitTime * i;
                    CalcDamage(attacker, t, dam, htime);
                }
            }
        }
        else
        {
            foreach (var t in targets)
            {
                CalcDamage(attacker, t, dam, hitTime);
            }
        }

        End(time);
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }
}
