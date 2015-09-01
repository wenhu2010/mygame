using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NpcEnterSkill : NfSkill
{
    protected override void OnSkillBegin()
    {
        attacker.SetSkillHit(null, null, null, false, 0);
        DoTskill();
        End();
    }

    protected override void OnSkillEnd()
    {
        DestroyAll();
    }
}

public class NpcEnter : NfBuff
{
    int num = 0;

    protected override bool Init()
    {
        num = Num;

        owner.EnableWuDi();
        owner.EnableDizzy();
        owner.visible = false;

        return true;
    }

    public override void HandleHuiHe()
    {
        if (--num < 0)
        {
			owner.DisableDizzy();
			
			int skillId = Tskill;
			SkillData skillJsd = DataMgr.GetSkill(skillId, 1);
			if (skillJsd != null)
			{
				string anim = skillJsd.FireAnim;
				owner.AddAnimEvent(anim, 0f, "NpcEnter", delegate() {
					owner.DisableWuDi();
					owner.visible = true;

					Fight.Inst.m_UiInGameScript.RefreshHeaders();
				});
			}

            Fight.Inst.HandleNpcShuoHua(owner);

            if (GameMgr.s_fightType == FightType.NULL)
            {
                PushTskill();
            }
            else
            {
				if (skillId != -1) {
					owner.target = Fight.Inst.FindAttackTarget(owner);
					owner.DoSkill(skillId, lv);
//                    self.PushStartSkill(Tskill, lv);
				}
            }

            //if (GameMgr.s_fightType == FightType.NULL)
            //    PushTskill();
            //else
            //    DoTskill();

            End();
        }
    }
}

public class NpcDefense : NfBuff
{
    GameObject effect;

    protected override bool Init()
    {
        if (owner.id == 1500)
        {
            if (id != 10067)
                return false;
        }
        else if (owner.id == 1903)
        {
            if (id != 10215)
                return false;
        }
        else if (owner.id == 1904)
        {
            if (id != 10216)
                return false;
        }
        else
        {
            return false;
        }

        GameObject e = Resources.Load(Effect) as GameObject;
        if (e != null)
        {
            effect = GameObject.Instantiate(e) as GameObject;
            effect.transform.position = owner.position;
            effect.transform.rotation = owner.rotation;
        }

        owner.PlayAnim(Anim);
        owner.idleAnim = "skill4_1";

		List<Character> chars = Fight.Inst.FindAllFriend (owner);
		foreach (var c in chars) {
            if (c != owner)
            {
                AnimationClip ac = c.mAnim.GetClip("idle2");
                if (ac != null)
                {
                    c.mAnim.Play("idle2");
                }
            }
		}

		StartCoroutine(ShiHua());
		StartCoroutine(AllIdle());
        return true;
    }

    IEnumerator ShiHua()
    {
        yield return new WaitForSeconds(BaseValue);

        ChangeTexture();
		owner.HP = 0;
        owner.StopAnim();

        if (owner.id == 1903 || owner.id == 1904)
        {
            float time = float.Parse(buffdata.Grow);
            yield return new WaitForSeconds(time);

            iTween.ScaleTo(owner.gameObject, new Vector3(buffdata.Scale, buffdata.Scale, buffdata.Scale), 0.1f);
            yield return new WaitForSeconds(0.1f);
            owner.visible = false;
        }
	}
	
	IEnumerator AllIdle()
	{
		yield return new WaitForSeconds(Percent);

		List<Character> chars = Fight.Inst.FindAllFriend (owner);
		foreach (var c in chars) {
            if (c != owner)
            {
                c.idleAnim = "idle";
                c.Idle();
            }
		}
	}

    private void ChangeTexture()
    {
        string HitEff = HitEffect;
        JsonData dataArray = JsonMapper.ToObject(HitEff);
        if (dataArray.IsArray)
        {
            for (int i = 0; i < dataArray.Count; i++)
            {
                string materialName = (string)dataArray[i][0];
                string textPath = (string)dataArray[i][1];

                Texture image = Resources.Load(textPath) as Texture;

                SkinnedMeshRenderer[] smr = owner.GetSkinnedMeshRenderer();
                foreach (var r in smr)
                {
                    foreach (var m in r.materials)
                    {
                        if (m.mainTexture.name == materialName)
                        {
                            m.mainTexture = image;
                        }
                    }
                }
            }
        }
    }
}
