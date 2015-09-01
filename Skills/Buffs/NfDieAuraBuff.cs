using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class NfDieAuraBuff : NfBuff
{
    protected float delay;
    protected string eff;
    protected string bullet;
    protected float speed;
    protected string hitEff;
    protected bool add;

    public class Soul
    {
        public bool start = false;
        public bool fly = false;
        public EffectObject effObj;
        public float time = 0f;
        public float totaltime = 0f;
        public float animlen = 0f;
        public Vector3 srcPos;
        public Vector3 destPos;
        public Character destChara;
    }

    protected List<Soul> souls = new List<Soul>();

    protected override bool Init()
    {
        string effs = HitEffect;
        JsonData json = JsonMapper.ToObject(effs);
        eff = (string)json[0];
        bullet = (string)json[1];
        delay = (float)json[2];
        speed = (float)json[3];
        if (json.Count > 4)
            hitEff = (string)json[4];
        add = false;

        return true;
    }

    protected override void OnEnd()
    {
        base.OnEnd();
    }

    public override void HandleFriendHpChange(Character target)
    {
        if (target.IsDead)
        {
            Soul s = new Soul();
            s.srcPos = target.position;
            s.animlen = target.GetAnimLength("die");
            s.destPos = owner.position;
            s.destPos.y += 1f;
            s.destChara = owner;
            s.totaltime = Vector3.Distance(target.position, s.destPos) / speed;
            souls.Add(s);
        }
    }

    public override void Update()
    {
        List<Soul> rm = new List<Soul>();
        foreach (var s in souls)
        {
            s.time += Time.deltaTime;
            if (s.fly)
            {
                var dp = new Vector3(s.destChara.pos2v.x, 1f, s.destChara.pos2v.y);
                var sp = s.effObj.obj.transform.position;
                var dir = dp - sp;
                dir.Normalize();
                float md = Time.deltaTime * speed;
                float dist = Vector3.Distance(dp, sp);
                if (dist <= md)
                {
                    rm.Add(s);
                    ChangeAttri(owner);
                    owner.DestroyEff(s.effObj);
					owner.PlayEffect(hitEff, 3f, true);

                    if (add == false)
                    {
                        add = true;
                        PlaySelfEffect();
                    }

                    AddTbuff(owner);
                }
                else
                {
                    s.effObj.obj.transform.position += dir * md;
                }

                //Vector3 dir = s.destPos - s.effObj.obj.transform.position;
                //dir.Normalize();
                //float md = Time.deltaTime * speed;
                //if (s.time >= s.totaltime)
                //{
                //    rm.Add(s);
                //    ChangeAttri(self);
                //    self.DestroyEff(s.effObj);

                //    if (add == false)
                //    {
                //        add = true;
                //        PlaySelfEffect();
                //    }

                //    AddTbuff(self);
                //}
                //else
                //{
                //    s.effObj.obj.transform.position += dir * md;
                //}
            }
            else if (s.start)
            {
                if (s.time >= delay)
                {
                    s.fly = true;
                    s.time = 0f;
					s.effObj = owner.PlayEffect(bullet, -1, true);
                    s.effObj.obj.transform.parent = null;
                    s.effObj.obj.transform.position = s.srcPos;
                }
            }
            else if (s.time >= s.animlen)
            {
                s.start = true;
                s.time = 0f;
                EffectObject effObj = owner.PlayEffect(eff, 5f, true);
                if (effObj != null)
                {
                    effObj.obj.transform.parent = null;
                    effObj.obj.transform.position = s.srcPos;
                }
            }
        }
        foreach (var r in rm)
        {
            souls.Remove(r);
        }
    }
}
