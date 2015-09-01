using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfFenLie : NfSkill {

    protected float gravity = 45f;
    protected float speed = 15f;
    List<int> slots = new List<int>();
    List<Character> addChars = new List<Character>();
    int numChar;
    GameObject fireEff;

    protected override void OnSkillBegin()
    {
        addChars.Clear();
        slots.Clear();
        for (int i = 0; i < GameMgr.GamePlayer.MAXOPENSLOTNUM; ++i)
        {
            if (Fight.Inst.IsEmptySlot(attacker.camp, i))
                slots.Add(i);
        }
        //if (slots.Count == 0)
        //{
        //    AddTbuff(attacker);
        //    End();
        //    return;
        //}

        slots.Add(attacker.slot);

        StartCameraAnim();
        //PlaySingAnimAndEffect();

        attacker.PlayAnim(SingAnim);

        fireEff = EffectMgr.Load(SingEffect);
        if (fireEff != null)
        {
            fireEff.transform.position = attacker.position;
        }

        AddChars();

        AddEvent(HitTime, delegate
        {
            MoveChars();

            attacker.visible = false;
        });
    }

    private void AddChars()
    {
        List<Character> allChar = Fight.Inst.GetAllCharacter(attacker.camp);
        for (int num = 0; num < MaxNum && slots.Count > 0; )
        {
            int id = Buff;
            HeroData jsd_hero = DataMgr.GetHero(id);
            if (jsd_hero != null)
            {
                Monster monster = new Monster();
                monster.LV = attacker.lv;
                monster.STRENGTHENID = attacker.strengthenid;
                monster.Data = jsd_hero;

                int idx = Fight.Rand(0, slots.Count);
                int slot = slots[idx];

                Character c = Character.Create(monster);
                c.isFenLie = true;
                c.SetCamp(attacker.camp);
                c.InitUI();
                c.slot = slot;
                c.SrcPos = Fight.Inst.GetSlotPos(c.camp, slot);
                if (c.camp == CampType.Friend)
                {
                    c.SrcRotation = Quaternion.Euler(new Vector3(0, 90f, 0));
                    c.direction = new Vector3(1f, 0f, 0f);
                }
                else
                {
                    c.SrcRotation = Quaternion.Euler(new Vector3(0, -90f, 0));
                    c.direction = new Vector3(-1f, 0f, 0f);
                }
                c.position = attacker.position;
                c.ReqPassiveSkill();
                c.visible = false;

                addChars.Add(c);
                allChar.Add(c);

                slots.RemoveAt(idx);
                ++num;
            }
        }

        Fight.Inst.SortAllChar(attacker.camp);
    }

    private void MoveChars()
    {
        if (addChars.Count > 0)
        {
            numChar = addChars.Count;
            foreach (var c in addChars)
            {
                c.visible = true;

                float d = Vector3.Distance(c.SrcPos, c.position);
                float animLen = c.GetAnimLength(KeepAnim);
                if (d > 0f)
                {
                    float moveSpeed = d / animLen;
                    MoveChar(c, moveSpeed, 1f);
                }
                else
                {
                    c.ForcePlayAnim(KeepAnim);
                    AddEvent(animLen, delegate
                    {
                        OnFinishMove(c);
                    });
                }
            }
        }
        else
        {
            End();
        }
    }

    private void MoveChar(Character c, float moveSpeed, float animSpeed)
    {
        Move(c, c.SrcPos, moveSpeed, KeepAnim, animSpeed, delegate
        {
            OnFinishMove(c);
        });
    }

    private void OnFinishMove(Character c)
    {
        c.initAnim = "idle";
        c.rotation = c.SrcRotation;
        c.ForcePlayAnim(skilldata.HitAnim);
        c.PlayEffect(HitEffect, 3f);

        if (--numChar <= 0)
        {
            End(c.GetAnimLength(skilldata.HitAnim));
        }
    }

    protected override void OnSkillEnd()
    {
        if (fireEff != null)
        {
            Destroy(fireEff);
        }

        DestroyAll();
        foreach (var c in addChars)
        {
            c.visible = true;
            c.initAnim = "idle";
            c.Idle();
            c.position = c.SrcPos;
            c.rotation = c.SrcRotation;
        }

        attacker.HP = 0;
        attacker.visible = false;
        attacker.deadType = Character.DeadType.FenLie;
        attacker.Dead(delegate { });
    }
}
