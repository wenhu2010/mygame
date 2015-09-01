using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfBianShenBuff : NfBuff
{
    protected int tskill = -1;
    int num = 0;
    Dictionary<string, Texture> backupTexs = new Dictionary<string, Texture>();

    protected override bool Init()
    {
        num = Num;

        PlaySelfEffect();
        ChangeTexture();
        ChangeAttri(owner);
        InitSkill();
        return true;
    }

    void InitSkill()
    {
        string str = buffdata.Tskill;
        if (str[0] == '[')
        {
            JsonData jsd = JsonMapper.ToObject(str);
            tskill = (int)jsd[0];
            if (jsd.Count > 1)
            {
                owner.bigSkillId = (int)jsd[1];
            }
        }
        else
        {
            tskill = Tskill;
        }
    }

    void RestoreSkill()
    {
        tskill = -1;
        owner.bigSkillId = owner.saveBigSkillId;
    }

    public override void HandleHuiHe()
    {
        if (num-- <= 0)
        {
            End();
        }
    }

    public override NfSkill HandleSkillBegin()
    {
        if (owner.isActiveBigSkill || owner.isActiveComboSkill)
            return null;
        if (tskill == -1)
            return null;
        return owner.DoSkill(tskill, lv);
    }

    private void ChangeTexture()
    {
        backupTexs.Clear();

        string HitEff = HitEffect;
        if (HitEff[0] != '[')
        {
            ChangeModel(HitEff);
        }
        else
        {
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
                                backupTexs.Add(image.name, m.mainTexture);
                                m.mainTexture = image;
                            }
                        }
                    }
                }
            }
        }
    }

    protected virtual void ChangeModel(string HitEff)
    {
        owner.ChangeModel(HitEff);
    }

    protected override void OnEnd()
    {
        base.OnEnd();
        RestoreTexture();
        RestoreChangeAttri(owner);
        RestoreSkill();
    }

    void RestoreTexture()
    {
        string HitEff = HitEffect;
        if (HitEff[0] != '[')
        {
            owner.PlayEffect(Effect, 3f);
            owner.RestoreModel();
        }
        else
        {
            SkinnedMeshRenderer[] smr = owner.GetSkinnedMeshRenderer();
            foreach (var r in smr)
            {
                foreach (var m in r.materials)
                {
                    if (backupTexs.ContainsKey(m.mainTexture.name))
                    {
                        m.mainTexture = backupTexs[m.mainTexture.name];
                    }
                }
            }
            backupTexs.Clear();
        }
    }

    public override void HandleDead()
    {
        End();
    }
}

public class NfBianYangBuff : NfBianShenBuff
{
    protected override bool Init()
    {
        base.Init();

        EnableBuff(false);
        return true;
    }

    private void EnableBuff(bool enable)
    {
        foreach (var b in owner.Buffs)
        {
            if (b is NfFanJiBuff || b is NfFanZhiBuff || b is NfDamShieldBuff)
            {
                b.enabled = enable;
            }
        }
    }

    protected override void OnEnd()
    {
        base.OnEnd();

        EnableBuff(true);
    }

    public override NfSkill HandleSkillBegin()
    {
        if (tskill == -1)
            return null;
        return owner.DoSkill(tskill, lv);
    }

    protected override void ChangeModel(string HitEff)
    {
        AddEvent(0.1f, delegate {

			owner.ChangeModel(HitEff);

            float scale = buffdata.Scale;
            if (scale.Equals(0f) == false)
            {
                iTween.ScaleTo(owner.gameObject, new Vector3(scale, scale, scale), 0.4f);
            }
        });
    }
}
