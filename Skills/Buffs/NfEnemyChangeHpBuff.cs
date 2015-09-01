using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfEnemyChangeHpBuff : NfBuff
{
    bool isAdd;
    
    protected override bool Init()
    {
        isAdd = false;
        return true;
    }

    public override void HandleEnemyHpChange(Character target)
    {
        float percent = Percent / 100f;

        if (isAdd == false)
        {
            if (target.MaxHp * percent >= target.HP)
            {
                ChangeAttri(owner);
                PlaySelfEffect();
                AddTbuff(owner);
                isAdd = true;
            }
        }
        else
        {
            bool has = false;
            List<Character> enemys = Fight.Inst.FindAllAttackTarget(owner);
            foreach (var e in enemys)
            {
                if (e.MaxHp * percent >= e.HP)
                {
                    has = true;
                    break;
                }
            }
            if (!has)
            {
                RestoreChangeAttri(owner);
                StopSelfEffect();
                RemoveTbuff(owner);
                isAdd = false;
            }
        }
    }
}

