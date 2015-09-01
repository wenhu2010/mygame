using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NfCharamMoveSkill : NfSkill
{
    public NfCharmBuff charmBuff;
    Vector3 destPos;
    bool isEndBuff;

    protected override void OnSkillBegin()
    {
		foreach (var b in attacker.Buffs)
		{
			if (b is NfCharmBuff)
			{
				charmBuff = b as NfCharmBuff;
				break;
			}
		}
        destPos = charmBuff.destPos;
        isEndBuff = charmBuff.isEnd;

        attacker.SkillHit();
        Move();
    }

    protected override void OnSkillEnd()
    {
        attacker.position = destPos;
        attacker.SrcPos = destPos;

        charmBuff.RemoveTbuff(attacker);

        if (isEndBuff)
            charmBuff.End();
    }

    void Move()
    {
        attacker.slot = charmBuff.destSlot;
        if (attacker.camp == CampType.Friend)
        {
            attacker.camp = CampType.Enemy;
            Fight.Inst.teams[0].Remove(attacker);
            Fight.Inst.teams[1].Add(attacker);
        }
        else
        {
            attacker.camp = CampType.Friend;
            Fight.Inst.teams[1].Remove(attacker);
            Fight.Inst.teams[0].Add(attacker);
        }
        Fight.Inst.SortAllChar();

        if (isEndBuff)
            charmBuff.StopSelfEffect();

        charmBuff.AddTbuff(attacker);

        destPos = Fight.Inst.GetSlotPos(attacker.camp, attacker.slot);

        if (attacker.camp == CampType.Friend)
        {
            attacker.SrcRotation = Quaternion.Euler(new Vector3(0, 90, 0));
            attacker.direction = new Vector3(1f, 0f, 0f);
        }
        else
        {
            attacker.SrcRotation = Quaternion.Euler(new Vector3(0, -90, 0));
            attacker.direction = new Vector3(-1f, 0f, 0f);
        }

        attacker.MoveTo(destPos, "run", attacker.moveSpeed, attacker.moveAnimSpeed, delegate
        {
            End();
        });
    }
}

public class NfCharmBuff : NfBuff {

    public Vector3 destPos;
    public bool isEnd;
    public int destSlot;
    int num;

    protected override bool Init()
    {
        if (owner.IsAntiCharm)
        {
            owner.ShowAnti();
            return false;
        }

        if (owner.IsDizzy)
            return false;

        if (Fight.Inst.FindAllFriend(owner).Count == 1)
            return false;

        foreach (NfBuff b in owner.Buffs)
        {
            if (b.isDestroy == false && b is NfCharmBuff)
            {
                b.Replace();
            }
        }

        num = Num;
        if (CharmCharacter(false))
        {
            PlaySelfEffect();
            return true;
        }

        return false;
    }

    bool CharmCharacter(bool isEnd)
    {
        this.isEnd = isEnd;
        destSlot = -1;
        destPos = owner.position;

		var destCamp = owner.camp == CampType.Friend ? CampType.Enemy : CampType.Friend;
		List<Character> allChar = Fight.Inst.GetAllCharacter(destCamp);
        for (int i = GameMgr.GamePlayer.MAXOPENSLOTNUM - 1; i >= 0; --i)
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
            if (empty)
            {
                destSlot = i;
                break;
            }
        }
        if (destSlot == -1)
            return false;

        MoveCharacter();

        if (isEnd)
        {
            End();
        }
        return owner.srccamp != destCamp;
    }

    private void MoveCharacter()
    {
        owner.slot = destSlot;
        if (owner.camp == CampType.Friend)
        {
            owner.camp = CampType.Enemy;
            Fight.Inst.teams[0].Remove(owner);
            Fight.Inst.teams[1].Add(owner);
        }
        else
        {
            owner.camp = CampType.Friend;
            Fight.Inst.teams[1].Remove(owner);
            Fight.Inst.teams[0].Add(owner);
        }
        Fight.Inst.SortAllChar();

        destPos = Fight.Inst.GetSlotPos(owner.camp, owner.slot);
        owner.SetSrcPos(destPos);

        if (owner.camp == CampType.Friend)
        {
            owner.SrcRotation = Quaternion.Euler(new Vector3(0, 90, 0));
            owner.direction = new Vector3(1f, 0f, 0f);
        }
        else
        {
            owner.SrcRotation = Quaternion.Euler(new Vector3(0, -90, 0));
            owner.direction = new Vector3(-1f, 0f, 0f);
        }

        owner.MoveTo(destPos, owner.moveSpeed, owner.moveAnimSpeed);
    }

    public override void HandleWaveAttack()
    {
        if (--num <= 0 && owner.isMove == false && owner.IsDizzy == false)
        {
            CharmCharacter(true);
        }
    }
}
