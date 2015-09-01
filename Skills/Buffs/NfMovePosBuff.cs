using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfMovePosBuff : NfBuff
{
    Vector3 destPos;
    int destSlot;
    float cd = 0f;

    protected override bool Init()
    {
        if (owner.IsDizzy)
            return false;

        foreach (var b in owner.Buffs)
        {
            if (b.enabled && b is NfMovePosBuff)
            {
                return false;
            }
        }

        cd = Time.time + float.Parse(buffdata.Grow);

        destPos = owner.position;
        List<Character> allChar = Fight.Inst.GetAllCharacter(owner.camp);
        List<int> slots = new List<int>();
        for (int i = 0; i < GameMgr.GamePlayer.MAXOPENSLOTNUM; ++i)
        {
            bool empty = true;
            for (int n = allChar.Count - 1; n >= 0; --n)
            {
                Character e = allChar[n];
                if (e.IsDead == false && e.slot == i)
                {
                    empty = false;
                    break;
                }
            }
            CampType camp = owner.camp == CampType.Friend ? CampType.Enemy : CampType.Friend;
            if (empty && owner.slot != i && Fight.Inst.FindForwardColumnTargets(camp, i).Count > 0)
                slots.Add(i);
        }

        if (slots.Count > 0)
        {
            destSlot = slots[Fight.Rand(0, slots.Count)];
        }
        else
        {
            destSlot = owner.slot;
        }
        destPos = Fight.Inst.GetSlotPos(owner.camp, destSlot);

        AddTbuff(owner);

        owner.slot = destSlot;
        Fight.Inst.SortAllChar();

        string[] anims = Anims;
        if (anims.Length >= 3)
        {
			float len = owner.PlayAnim(anims[0]);
			AddEvent(len, delegate {
				float moveSpeed = BaseValue;
				float animSpeed = (moveSpeed / owner.moveSpeed) * owner.moveAnimSpeed;

                owner.MoveTo(destPos, anims[1], moveSpeed, animSpeed, delegate
                {
                    AddEvent(owner.PlayAnim(anims[2]), delegate
                    {
                        OnFinish();
                        PushTskill();
                    });
                });
			});
        }
        else
		{
			float moveSpeed = BaseValue;
			float animSpeed = (moveSpeed / owner.moveSpeed) * owner.moveAnimSpeed;

            owner.MoveTo(destPos, Anim, moveSpeed, animSpeed, delegate
            {
                OnFinish();
                PushTskill();
			});
        }

        return true;
    }

    void OnFinish()
    {
        RemoveTbuff(owner);

        owner.position = destPos;
        owner.SrcPos = destPos;
    }

    protected override void OnEnd()
    {
        OnFinish();
        base.OnEnd();
    }

    public override void Update()
    {
        if (Time.time >= cd)
        {
            End();
        }
    }
}
