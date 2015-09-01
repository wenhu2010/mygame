using UnityEngine;
using System.Collections;
using LitJson;

public class NfFenShenBuff : NfBuff
{
    Character fenShen = null;

    public class NfFenShenEndBuff : NfBuff
    {
        protected override bool Init()
        {
            return true;
        }

        public override void HandleSkillEnd(NfSkill skill)
        {
            owner.visible = false;
            foreach (var b in owner.Buffs)
            {
                if (b.enabled && b is NfFenShenEndBuff == false)
                    b.End();
            }
        }
    }

    protected override bool Init()
    {
        fenShen = owner.Clone();
		fenShen.isFenShen = true;
        fenShen.visible = false;
        fenShen.EnableAntiDizzy();
        fenShen.EnableAntiFanJi();
        fenShen.EnableWuDi();
        //fenShen.gameObject.SetActive(false);

        NfFenShenEndBuff buff = new NfFenShenEndBuff();
        buff.lv = lv;
        fenShen.AddBuff(buff);

        ChangeTexture();
        PlaySelfEffect();
        return true;
    }

    protected override void OnEnd()
    {
        base.OnEnd();
		Fight.Inst.RemoveCurrAttack (fenShen);
        GameObject.Destroy(fenShen.gameObject);
    }

    public override void HandleHitEnd(Character target)
    {
        if (fenShen.visible)
            return;
        if (target.IsDead)
            return;
        float per = Percent;
        if (Fight.Rand(0, 101) > per)
            return;

        Vector3 dest = target.position;
        Vector3 dir = new Vector3(0f, 0f, 1f);
        fenShen.target = target;
        fenShen.position = dest + dir * NfSkill.CalcRadius(owner, target);
        fenShen.LookAt(target);
        fenShen.SrcPos = fenShen.position;
        fenShen.SrcRotation = fenShen.rotation;
        fenShen.visible = true;
        fenShen.EnableAlpha(true);

        DoTskill(fenShen, target);

        return;
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

                SkinnedMeshRenderer[] smr = fenShen.GetSkinnedMeshRenderer();
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
