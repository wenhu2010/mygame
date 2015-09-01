using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using SLua;

public class NfSkill
{
    public enum SkillType
    {
        NormalSkill,
        TriggerSkill,
        BigSkill,
        ComboSkill
    }

    public enum DamageType
    {
        PhysAtk = 1,
        MagcAtk = 2,
    }

    public enum TDamageType
    {
        Default = 0,
        LowHPHighDam = 1,
        Real = 2,
        HuanXue = 3,
        PercentHPTrigger = 4,
        AddPercentHPDam = 5,
    }

    public enum SkillDamageType
    {
        None = 0,
        ATK = 1,
        HP = 2,
        AS = 3,
        DEF = 4,
        Anger = 5,
        Percent = 6,
    }

    public enum RangeType
    {
        Self = 0,
        Single = 1,
        FrontRow = 2,
        Column = 3,
        Round = 4,
        All = 5,
		Round_4 = 6,
        LeastHp = 7,
        MirrorAOE = 8,
        RandSingle = 9,
        AOE_9 = 10,
        AOE_5 = 11,
        AOE_4 = 12,
        AOE_8 = 13,
        Round_6 = 14,
        LeftRight_3 = 15,
        ReadyCastSkill = 16,
        ForwardColumn = 17,
        FriendLeastHp = 18,
        RandSingleExcludeMe = 19,
    }

    [DoNotToLua]
    public static float ATT_ADD_FACTOR = 0.001f;
    [DoNotToLua]
    public static float ATT_DEC_FACTOR = 0.001f;

    protected GameEventMgr evtMgr = new GameEventMgr();
	public Character attacker;

    private bool isAddAnger = false;

    [DoNotToLua]
    public bool _hit;

    public bool hit
    {
        set
        {
            _hit = value;

            if (_hit && isAddAnger == false)
            {
                isAddAnger = true;
                int selfAnger = skilldata.SelfAnger;
                attacker.AddAnger(selfAnger);
            }
        }
        get { return _hit; }
    }

    bool _enabled;
    public bool enabled
    {
        set { _enabled = value; }
        get { return _enabled; }
    }

    SkillData _skillData;
    public SkillData skilldata
    {
        get
        {
            return _skillData;
        }
    }

    int _id;
    public int id
    {
        get { return _id; }
    }

    int _lv;
    public int lv
    {
        get { return _lv; }
    }

    public SkillType skillType = SkillType.NormalSkill;
    public bool isFanJi = false;

    protected Transform transform;
    protected Animation animation;
    protected GameObject gameObject;
    protected JsonData singSound;
    protected JsonData fireSound;

    protected List<SkinnedMeshRenderer> hideMesh = new List<SkinnedMeshRenderer>();
    protected List<Character> hideChars = null;

    //protected abstract void OnSkillBegin();
    //protected abstract void OnSkillEnd();
    [DoNotToLua]
    public virtual void Update()
    { 
        UpdateEvent();
    }

    [DoNotToLua]
    public void Init(Character attacter, SkillData data, int id, int lv)
    {
        _id = id;
        _lv = lv;
        _skillData = data;
        this.attacker = attacter;

        damType = (DamageType)(_skillData.DamageType);
        rangeType = (RangeType)(_skillData.Range);
        attriType = _skillData.AttriType;

        string strSingSound = _skillData.SingSound;
        singSound = JsonMapper.ToObject(strSingSound);
        string strFireSound = _skillData.FireSound;
        fireSound = JsonMapper.ToObject(strFireSound);

        InitDamage();
        InitTdamage();

        damageTranJson = JsonMapper.ToObject(_skillData.DamageTran);
    }

    [DoNotToLua]
    public void Start(NfSkill preskill)
    {
        Time.timeScale = GameMgr.timeScale;
        transform = attacker.transform;
        animation = attacker.mAnim;
        gameObject = attacker.gameObject;

        attacker.RestoreDir();

        hit = false;
        enabled = true;
        isAddAnger = false;

        try
        {
            tiggerSkill = null;
            camAnim = null;
            preSkill = preskill;

            setHideMeshList();

            OnSkillBegin();
        }
        catch (KeyNotFoundException e)
        {
            End();
            Debug.LogError("skill(" + id + ") " + e.Message);
            Debug.LogError(e.StackTrace);

#if UNITY_EDITOR
            MessageBox.Show("skill error", "skill(" + id + ") " + e.Message);
#endif
        }
    }

    private void setHideMeshList()
    {
        hideMesh.Clear();

        string fireBone = FireBone;
        if (fireBone[0] == '[')
        {
            JsonData boneJson = JsonMapper.ToObject(fireBone);
            if (boneJson.IsArray)
            {
                fireBone = (string)boneJson[0];

                for (int i = 1; i < boneJson.Count; ++i)
                {
                    string hideModelName = (string)boneJson[i];

                    SkinnedMeshRenderer[] smr = attacker.GetSkinnedMeshRenderer();
                    foreach (var r in smr)
                    {
                        if (r.name == hideModelName)
                        {
                            hideMesh.Add(r);
                            break;
                        }
                    }
                }
            }
        }
    }

    protected SkillDamageType skillDamType;
    protected float skillDamageFactor;
    protected float baseSkillDamage;
    void InitDamage()
    {
		string str = skilldata.Damage;
        JsonData[] json = JsonMapper.ToObject<JsonData[]>(str);
        int type = (int)json[0];
        skillDamType = (SkillDamageType)type;
        baseSkillDamage = (float)json[1];
		baseSkillDamage = baseSkillDamage + lv * skilldata.GrowDamage;
        skillDamageFactor = (float)json[2];
    }

    public static int Clamp(int value)
    {
        if (value < 0)
            return 0;
        return value;
    }

	public static float CalcDamage(SkillDamageType type, Character chara, float skillDamageFactor, float baseSkillDamage)
	{
		float damage = 0;
		switch (type)
		{
            case SkillDamageType.ATK:
                {
                    damage = Mathf.FloorToInt(Clamp(chara.ATK) * skillDamageFactor + baseSkillDamage);
                } break;
            case SkillDamageType.HP:
                {
                    damage = Mathf.FloorToInt(Clamp(chara.MaxHp) * skillDamageFactor + baseSkillDamage);
                } break;
            case SkillDamageType.AS:
                {
                    damage = Mathf.FloorToInt(Clamp(chara.AS) * skillDamageFactor + baseSkillDamage);
                } break;
            case SkillDamageType.DEF:
                {
                    damage = Mathf.FloorToInt(Clamp(chara.DEF) * skillDamageFactor + baseSkillDamage);
                } break;
            default:
                {
					damage = baseSkillDamage;
                } break;
        }
		return damage;
	}
	
	public float Damage
	{
		get
		{
            return CalcDamage(skillDamType, attacker, skillDamageFactor, baseSkillDamage);
		}
	}

    public float GetDamage(Character atk)
    {
        return CalcDamage(skillDamType, atk, skillDamageFactor, baseSkillDamage);
    }
	
	protected DamageType damType;
	protected RangeType rangeType;
    protected int attriType;

    protected TDamageType tdamType;
    protected float tdamBaseValue;
    protected float tdamPerValue;

    protected JsonData damageTranJson;
	
	private List<EffectObject> effects = new List<EffectObject>();

    private NfSkill _preSkill;

    public NfSkill preSkill
    {
        get { return _preSkill; }
        set
        {
            if (value != null)
                value.tiggerSkill = this;
            _preSkill = value;
        }
    }
    public NfSkill tiggerSkill;

    public int AttriType
    {
        get { return attriType; }
    }

    public void DestroyAll()
    {
        //DestroyAllEff();
    }

	private List<IEnumerator> coroutines = new List<IEnumerator>();

    [DoNotToLua]
    public void StopAllCoroutines()
    {
		foreach (var c in coroutines) {
			attacker.StopCoroutine(c);		
		}
		coroutines.Clear ();
        evtMgr.StopAll();
    }
    [DoNotToLua]
    public void StartCoroutine(IEnumerator routine)
    {
		attacker.StartCoroutine(routine);
		coroutines.Add (routine);
    }
    [DoNotToLua]
	public void StopCoroutine(IEnumerator routine)
    {
        attacker.StopCoroutine(routine);
    }
    [DoNotToLua]
    public void StopCoroutine(string methodName)
    {
        attacker.StopCoroutine(methodName);
    }

    public void Coroutine(YieldInstruction ins, LuaFunction fn)
    {
        attacker.StartCoroutine(doCoroutine(ins, fn));
    }

    private IEnumerator doCoroutine(YieldInstruction ins, LuaFunction fn)
    {
        yield return ins;

        fn.call();
    }

    public void DestroyEff(EffectObject eff)
    {
        if (eff != null && effects.Contains(eff))
        {
            if (eff.obj != null)
            {
                eff.obj.transform.parent = null;
            }
            if (eff is ChainEffectObject)
            {
                ChainEffectObject ribbon = (ChainEffectObject)eff;
                Destroy(ribbon.begin);
                Destroy(ribbon.end);
            }
            EffectMgr.RemoveEff(eff.obj);
            Destroy(eff.obj);
            eff.obj = null;

            effects.Remove(eff);
        }
    }

    public void DestroyAllEff()
    {
        foreach (EffectObject e in effects)
        {
            if (e.obj != null)
            {
                e.obj.transform.parent = null;
            }
            if (e is ChainEffectObject)
            {
                ChainEffectObject ribbon = (ChainEffectObject)e;
                Destroy(ribbon.begin);
                Destroy(ribbon.end);
            }
            EffectMgr.RemoveEff(e.obj);
            Destroy(e.obj);
            e.obj = null;
        }
        effects.Clear();
    }

    public static Object Instantiate(Object original)
    {
        return GameObject.Instantiate(original);
    }

    public static void Destroy(GameObject obj)
    {
        GameObject.Destroy(obj);
    }

    public static void Destroy(GameObject obj, float t)
    {
        GameObject.Destroy(obj, t);
    }

    IEnumerator DelayInvoke(float time, GameEvent.Func f)
    {
        yield return new WaitForSeconds(time);
        try
        {
            if (enabled)
                f();
        }
        catch (KeyNotFoundException e)
        {
            Debug.LogError("skill(" + id + ") " + e.Message);
            Debug.LogError(e.StackTrace);
            MessageBox.Show("skill param error", "skill(" + id + ") " + e.Message);
            End();
        }
    }

    IEnumerator DelayInvokeRepeating(float time, GameEvent.Func f, int count)
    {
        while (count > 0)
        {
            yield return new WaitForSeconds(time);

            try
            {
                if (enabled)
                    f();
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogError("skill(" + id + ") " + e.Message);
                Debug.LogError(e.StackTrace);
                MessageBox.Show("skill param error", "skill(" + id + ") " + e.Message);
                End();
            }
            --count;
        }
    }

    public void AddEvent(float time, GameEvent.Func f)
    {
        if (time > 0f)
        {
            StartCoroutine(DelayInvoke(time, f));
            //evtMgr.Add("", time, f);
        }
        else
        {
            f();
        }
    }

    public void AddEvent(string tag, float time, GameEvent.Func f)
    {
        if (time > 0f)
        {
            evtMgr.Add(tag, time, f);
        }
        else
        {
            f();
        }
    }

    public void StopEvent(string tag)
    {
        evtMgr.Stop(tag);
    }

    protected void UpdateEvent()
    {
        evtMgr.Update();
    }

    public void AddEvent(float time, GameEvent.Func f, int count)
    {
        StartCoroutine(DelayInvokeRepeating(time, f, count));
    }

    IEnumerator InvokeRepeating(GameEvent.FuncNoTime f)
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if (!enabled)
                break;
            if (f() == true)
                break;
        }
    }
    public void AddEvent(GameEvent.FuncNoTime f)
    {
        StartCoroutine(InvokeRepeating(f));
    }

    private IEnumerator OnDestroyEff(EffectObject e, float time)
    {
        yield return new WaitForSeconds(time);
        DestroyEff(e);
    }

    private void DestroyEff(EffectObject effObj, float time)
    {
        Fight.Inst.StartCoroutine(OnDestroyEff(effObj, time));
    }
	
	public EffectObject PlayEffect(GameObject target, string eff, float destroyTime) {
        EffectObject effObj = null;
        if (!string.IsNullOrEmpty(eff) && eff != "null")
        {
            try
            {
                GameObject obj = EffectMgr.LoadAsyn(eff, attacker);
                obj.transform.position = target.transform.position;

                effObj = new EffectObject(obj);
                effects.Add(effObj);

                SetEffectParam(target, eff, destroyTime, effObj, obj);
            }
            catch (UnityException e)
            {
                Debug.LogException(e);
            }
        }
        return effObj;
	}

    private void SetEffectParam(GameObject target, string eff, float destroyTime, EffectObject effObj, GameObject obj)
    {
        Effect e = DataMgr.GetEffect(eff);
        if (e != null)
        {
            if (e.parent == 4)
            {
                obj.transform.parent = target.transform;
                obj.transform.position = target.transform.position;
            }
            if (e.parent == 1)
            {
                obj.transform.parent = target.transform;
                obj.transform.position = target.transform.position;
                obj.transform.rotation = target.transform.rotation;
            }
            else
            {
                obj.transform.rotation = target.transform.rotation;
                if (e.bone != "null")
                {
                    GameObject bind = Helper.FindObject(target, e.bone, false);
                    if (bind != null)
                    {
                        if (e.parent == 2)
                        {
                            obj.transform.parent = target.transform;
                            obj.transform.position = new Vector3(target.transform.position.x, bind.transform.position.y, target.transform.position.z);
                        }
                        else
                        {
                            obj.transform.parent = bind.transform;
                            obj.transform.position = bind.transform.position;
                        }
                    }
                }
            }

            if (e.time > 0f)
                DestroyEff(effObj, e.time);
        }
        else
        {
            obj.transform.rotation = target.transform.rotation;

            if (destroyTime > 0)
                DestroyEff(effObj, 3f);
            else
                DestroyEff(effObj, 5f);
        }
    }

    float GetEffectAnimLen(Transform parent)
    {
        float len = 0f;
        Animator a = parent.GetComponent<Animator>();
        if (a != null)
        {
            AnimationInfo[] ai = a.GetCurrentAnimationClipState(0);
            for (int i = 0; i < ai.Length; ++i)
            {
                float l = ai[i].clip.length;
                if (l > len)
                {
                    len = l;
                }
            }
        }

        foreach (Transform t in parent)
        {
            float l = GetEffectAnimLen(t);
            if (l > len)
            {
                len = l;
            }
        }
        return len;
    }

    public EffectObject PlayEffect(Character chara, string eff, float destroyTime)
    {
        return PlayEffect(chara.gameObject, eff, destroyTime);
    }

    public EffectObject PlayEffectSync(string eff, float destroyTime)
    {
        EffectObject effObj = null;
        if (!string.IsNullOrEmpty(eff) && eff != "null")
        {
            try
            {
                GameObject obj = EffectMgr.Load(eff);

                effObj = new EffectObject(obj);
                effects.Add(effObj);

                Effect e = DataMgr.GetEffect(eff);
                if (e != null)
                {
                    if (e.time > 0f)
                        DestroyEff(effObj, e.time);
                }
                else
                {
                    if (destroyTime > 0)
                        DestroyEff(effObj, 3f);
                    else
                        DestroyEff(effObj, 5f);
                }
            }
            catch (UnityException e)
            {
                Debug.LogException(e);
            }
        }
        return effObj;
    }

    public EffectObject PlayEffect(string eff, float destroyTime)
    {
        EffectObject effObj = null;
        if (!string.IsNullOrEmpty(eff) && eff != "null")
        {
            try
            {
                GameObject obj = EffectMgr.LoadAsyn(eff, attacker);

                effObj = new EffectObject(obj);
                effects.Add(effObj);

                Effect e = DataMgr.GetEffect(eff);
                if (e != null)
                {
                    if (e.time > 0f)
                        DestroyEff(effObj, e.time);
                }
                else
                {
                    if (destroyTime > 0)
                        DestroyEff(effObj, 3f);
                    else
                        DestroyEff(effObj, 5f);
                }
            }
            catch (UnityException e)
            {
                Debug.LogException(e);
            }
        }
        return effObj;
    }

    public EffectObject PlayChainEff(string eff, float destroyTime)
    {
        EffectObject effObj = null;
        if (!string.IsNullOrEmpty(eff) && eff != "null")
        {
            try
            {
                GameObject obj = EffectMgr.Load(eff);
                effObj = new ChainEffectObject(obj);
                effects.Add(effObj);

                Effect e = DataMgr.GetEffect(eff);
                if (e != null)
                {
                    if (e.time > 0f)
                        DestroyEff(effObj, e.time);
                }
                else
                {
                    if (destroyTime > 0)
                        DestroyEff(effObj, 3f);
                    else
                        DestroyEff(effObj, 5f);
                }
            }
            catch (UnityException e)
            {
                Debug.LogException(e);
            }
        }
        return effObj;
    }

    public EffectObject PlayChainEff(string eff, float destroyTime, Vector3 beginPos, Vector3 endPos)
    {
        EffectObject ribbon = PlayChainEff(eff, destroyTime);
        GameObject begin = Helper.FindObject(ribbon.obj, "Begin_01");
        GameObject end = Helper.FindObject(ribbon.obj, "End_01");
        begin.transform.position = beginPos;
        end.transform.position = endPos;

        return ribbon;
    }

    public EffectObject PlayChainEff(Character c1, Character c2, string eff, string boneName1, string boneName2)
    {
        ChainEffectObject ribbon = null;
        if (!string.IsNullOrEmpty(eff) && eff != "null")
        {
            try
            {
                GameObject obj = EffectMgr.Load(eff);
                ribbon = new ChainEffectObject(obj);
                effects.Add(ribbon);

                GameObject begin = Helper.FindObject(ribbon.obj, "Begin_01");
                GameObject end = Helper.FindObject(ribbon.obj, "End_01");
                GameObject bone1 = c1.FindBone(boneName1);//Helper.FindObject(c1.gameObject, boneName1, false);
                GameObject bone2 = c2.FindBone(boneName2);//Helper.FindObject(c2.gameObject, boneName2, false);
                begin.transform.position = bone1.transform.position;
                begin.transform.parent = bone1.transform;
                end.transform.position = bone2.transform.position;
                end.transform.parent = bone2.transform;
                ribbon.begin = begin;
                ribbon.end = end;

                Effect e = DataMgr.GetEffect(eff);
                if (e != null)
                {
                    if (e.time > 0f)
                        DestroyEff(ribbon, e.time);
                }
            }
            catch (UnityException e)
            {
                Debug.LogException(e);
            }
        }

        return ribbon;
    }

    public void Break()
    {
        if (preSkill != null)
        {
            preSkill.Break();
        }

        StopCameraAnim();
        StopAllCoroutines();
        OnSkillEnd();

        if (attacker.hitTarget)
        {
            EndSkill();
        }
        else
        {
            foreach (NfBuff buff in attacker.Buffs)
            {
                if (buff.enabled)
                {
                    buff.HandleSkillEnd(this);
                }
            }
        }

        //DestroyAll();
        DestroyAllEff();
        ClearGeZiEffects();
        enabled = false;
        preSkill = null;
    }

    CameraPathBezierAnimator camAnim;
    IEnumerator stopCameraAnimCoroutine = null;

    public virtual bool StartCameraAnim()
    {
        if (attacker.camp == CampType.Enemy)
            return false;

        // 摄像机轨迹动画
		string camParam = skilldata.Camera;
        if (camParam.Equals("null"))
            return false;

        JsonData camJson = JsonMapper.ToObject(camParam);
        if (camJson.IsArray == false)
            return false;

        //Fight.Inst.RestoreCameraParam();

        int camPathIdx = (int)camJson[0];

        camAnim = Fight.Inst.GetCameraAnim(camPathIdx);
        camAnim.animationTarget = Fight.Inst.mainCam.transform;
        camAnim.transform.position = attacker.position;
        camAnim.transform.rotation = attacker.rotation;
        camAnim.bezier._targetPosition = attacker.position + camAnim.bezier.targetPosition;
        
        if (camAnim.nextAnimation != null)
        {
            var ca = camAnim.nextAnimation;
            ca.animationTarget = Fight.Inst.mainCam.transform;
            //ca.transform.position = attacker.position;
            //ca.transform.rotation = attacker.rotation;
            ca.bezier.targetPosition = attacker.position + ca.bezier.targetPosition;
        }
        //GameObject bip01 = Helper.FindObjectExceptInactive(attacker.gameObject, "Bip01");
        //if (bip01 != null)
        //    camAnim.bezier.target = bip01.transform;
        //else
        //    camAnim.bezier.target = attacker.transform;
        camAnim.Play();

        Fight.Inst.SetBigSkillLight(attacker);

        // 结束摄像机动画
        float time = attacker.GetAnimLength(FireAnim);
		float outtime = (float)camJson[1];
		if (outtime > 0.001f) {
			time = outtime;
		}

        if (stopCameraAnimCoroutine != null)
            StopCoroutine(stopCameraAnimCoroutine);// 需要停止之前
        if (time > 0f)
        {
            stopCameraAnimCoroutine = DelayStopCameraAnim(time);
            StartCoroutine(stopCameraAnimCoroutine);
        }

        //StopEvent("StopCamAnim");// 需要停止之前
        //if (time > 0f)
        //{
        //    AddEvent("StopCamAnim", time, delegate
        //    {
        //        Debug.LogError("stop cam anim");
        //        StopCameraAnim();
        //    });
        //}
        //StopEvent("StopCamAnim");// 需要停止之前

        return true;
    }

    IEnumerator DelayStopCameraAnim(float time)
    {
        yield return new WaitForSeconds(time);
        StopCameraAnim();
    }

    protected virtual void StopCameraAnim()
    {
        if (camAnim == null)
            return;

        Time.timeScale = GameMgr.timeScale;
        camAnim.Stop();
        camAnim.gameObject.SetActive(false);
        ShowAllFriend();

        camAnim = null;
        Fight.Inst.RestoreCameraParam();

        if (table != null)
        {
            LuaMgr.CallMethod(table, "OnStopCameraAnim", table);
        }
    }

    // 播放技能动画
    public EffectObject PlaySingAnimAndEffect()
    {
        // 播放动画和特效;
        float animLen = attacker.PlayAnim(SingAnim);
        return PlayEffect(gameObject, SingEffect, animLen);
    }

    // 播放技能动画
    public EffectObject PlayFireAnimAndEffect()
    {
        // 播放动画和特效
        float animLen = attacker.PlayAnim(FireAnim);
        return PlayEffect(gameObject, FireEffect, animLen);
    }

    // 播放技能动画，effTime特效时长
    public EffectObject PlayFireAnimAndEffect(float effTime)
    {
        // 播放动画和特效
        attacker.PlayAnim(FireAnim);
        return PlayEffect(gameObject, FireEffect, effTime);
    }

    IEnumerator DelayEnd(float time)
    {
        yield return new WaitForSeconds(time);
        End();
    }

    public void End(float time)
    {
        StartCoroutine(DelayEnd(time));
    }

    IEnumerator _EndSkill()
    {
        yield return new WaitForEndOfFrame();

        if (tiggerSkill == null)
        {
            if (preSkill != null)
            {
                preSkill.tiggerSkill = null;
                preSkill.EndSkill();
            }
            else
            {
                attacker.EndSkill(this);
            }
        }
    }

    void EndSkill()
    {
        if (tiggerSkill == null)
        {
            if (preSkill != null)
            {
                preSkill.tiggerSkill = null;
                preSkill.EndSkill();
            }
            else
            {
                attacker.EndSkill(this);
            }
        }
    }

    public void End()
    {
        StopCameraAnim();

        OnSkillEnd();

        EndSkill();

        hit = false;
        enabled = false;

        ClearGeZiEffects();
    }

    // 伤害计算
    public void CalcDamage(Character attacker, Character target, float dam, float hitTime)
    {
        CalcDamage(attacker, target, dam, hitTime, HitEffect);
    }

    public void CalcDamage(Character attacker, Character target, float dam, float hitTime, string hitEff)
    {
        if (target.IsWuDi)
            return;

        // 暴击
        bool crithit = false;
		float CRI = skilldata.CRI;
        if (attacker.CRIRate != 0)
            CRI = attacker.CRIRate;
        if (CRI > 1f)
        {
            crithit = Fight.Rand(1, 1001) <= attacker.CRI;
            if (crithit == false)
                CRI = 1f;
        }
        if (target.IsAntiCrit)
        {
            crithit = false;
            CRI = 1f;
        }
        // 抖屏
        CameraShake(hitTime);

        // 属性加成
        float attrAdd = 0f; 
		if(target == null)
			return;
        int attriSub = attacker.ATTRIBUTE - target.ATTRIBUTE;
        if (attriSub == -1 || attriSub == 3)
        {
            attrAdd = 1f + attacker.AttriValue * NfSkill.ATT_ADD_FACTOR;
        }
        else if (attriSub == 1 || attriSub == -3)
        {
            attrAdd = 1f - target.AttriValue * NfSkill.ATT_DEC_FACTOR;
        }
        if (attrAdd <= 0f)
            attrAdd = 1f;

        // 属性克制= 克制方属性值*克制系数*最终伤害
        // 最终伤害=（攻击*技能系数+技能基础伤害）/（D/100+1）*暴击系数 * 属性克制
        float dmhp;
        if (SkillDamageType.Percent == skillDamType)
            dmhp = dam;
        else
            dmhp = (dam / (Clamp(target.DEF) / 100f + 1f)) * CRI * attrAdd;

        int idam = dam > 0 ? Mathf.Max(1, (int)dmhp) : (int)dmhp;
        Hit(attacker, target, idam, hitEff, hitTime, crithit);
    }

    private static float lastShakeTime = 0f;

    protected void CameraShake(float hitTime)
    {
        int shake = skilldata.Shake;
        if (shake > 0)
        {
            AddEvent(hitTime, delegate
            {
                if (Time.time - lastShakeTime > 0.2f)
                {
                    lastShakeTime = Time.time;
                    iTween.ShakePosition(Camera.main.gameObject, new Vector3(1f, 0f, 1f), 0.2f);
                }
            });
        }
    }

    private void InitTdamage()
    {
		string str = skilldata.Tdam;
        JsonData[] json = JsonMapper.ToObject<JsonData[]>(str);
        tdamType = (TDamageType)((int)json[0]);
        float baseDam = (float)json[1];
		tdamBaseValue = baseDam + lv * skilldata.Tgdam;
        tdamPerValue = 0f;
        if (json.Length > 2)
            tdamPerValue = (float)json[2];
    }

    public static int AddTdam(Character attacker, Character target, int damage, bool check, float tdamBaseValue, float tdamPerValue, NfSkill.TDamageType tdamType, string HitEffect)
    {
        float tdam = tdamBaseValue;
        float dam = 0f;
        float per = tdamPerValue;

        switch (tdamType)
        {
            case TDamageType.LowHPHighDam:
                {
                    dam += damage * (tdam - (float)target.HP / (float)target.MaxHp);
                } break;
            case TDamageType.Real:
                {
                    dam += damage + tdam;
                } break;
            case TDamageType.HuanXue:
                {
                    bool can = true;
                    if (target.isMonster)
                    {
                        if (target.monsterType != Monster.MonsterType.Normal)
                        {
                            can = false;
                        }
                    }
                    if (can)
                    {
                        dam = damage + (int)(tdam * target.HP);

                        if (!check)
                        {
                            float val = tdam * attacker.HP;
                            attacker.BeRevHit(target, (int)val, HitEffect, null, HpLabelContainer.HitType.HuanXue);
                        }
                    }
                    else
                    {
                        dam = damage;
                    }
                } break;
            case TDamageType.PercentHPTrigger:
                {
                    dam += damage;
                    float cper = (float)target.HP / (float)target.MaxHp;
                    if (cper <= per)
                        dam += tdam;
                } break;
            case TDamageType.AddPercentHPDam:
                {
                    bool can = true;
                    if (target.isMonster)
                    {
                        if (target.monsterType != Monster.MonsterType.Normal)
                        {
                            can = false;
                        }
                    }
                    if (!can)
                    {
                        dam += damage;
                    }
                    else
                    {
                        dam += damage + tdam * target.MaxHp;
                    }
                } break;
            default:
                {
                    dam = damage;
                }break;
        }
        return (int)dam;
    }

    public int GetMaxDamage(Character attacker, Character target)
    {
        if (skilldata.Func == "NfFenLie" || skilldata.Func == "NfAddHp")
            return 0;
        // 暴击
        //float exp = (int)data["CRI"];

        // 属性加成
        float attrAdd = 0f;
        int attriSub = attacker.ATTRIBUTE - target.ATTRIBUTE;
        if (attriSub == -1 || attriSub == 3)
        {
            attrAdd = 1f + attacker.AttriValue * NfSkill.ATT_ADD_FACTOR;
        }
        else if (attriSub == 1 || attriSub == -3)
        {
            attrAdd = 1f - target.AttriValue * NfSkill.ATT_DEC_FACTOR;
        }
        if (attrAdd <= 0f)
            attrAdd = 1f;

        // 属性克制= 克制方属性值*克制系数*最终伤害
        // 最终伤害=（攻击*技能系数+技能基础伤害）/（D/100+1）*暴击系数 * 属性克制
        float dam = Damage;
        if (damageTranJson.IsArray)
        {
            float rate = 99999f;
            if (damageTranJson[0].IsArray)
            {
                for (int i = 0; i < damageTranJson.Count; ++i)
                {
                    float n = (float)damageTranJson[i][1];
                    if (rate > n)
                    {
                        rate = n;
                    }
                }
            }
            else
            {
                for (int i = 0; i < damageTranJson.Count; ++i)
                {
                    float n = (float)damageTranJson[i];
                    if (rate > n)
                    {
                        rate = n;
                    }
                }
            }
            dam *= rate;
        }
        float dmhp = (dam / (Clamp(target.DEF) / 100f + 1f)) * attrAdd;
        int idam = Mathf.Max(1, (int)dmhp);
        int tdam = AddTdam(attacker, target, idam, true, tdamBaseValue, tdamPerValue, tdamType, HitEffect);
        return tdam;
    }

    protected static readonly int[][] allSlots = new int[][] { new int[] {0,1,2,3}, 
        new int[] { 0,1,2,3,4,5,6,7,8 },
        new int[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13 } };

    public void CalcCircalDamage(Character attacker, List<Character> targets, Vector3 pos, float dam, float hitTime)
    {
        if (damageTranJson.IsArray)
        {
            foreach (var t in targets)
            {
                for (int i = 0; i < damageTranJson.Count; ++i)
                {
                    float dist = Vector3.Distance(pos, t.position);
                    float r = (float)damageTranJson[i][0];
                    float n = (float)damageTranJson[i][1];
                    if (dist <= r)
                    {
                        float v = dam * n;
                        CalcDamage(attacker, t, v, hitTime);
                        break;
                    }
                }
            }
 
            int[] slots = allSlots[(int)GameMgr.gameData.teamSlot];

            ClearGeZiEffects();
            CampType camp = attacker.camp == CampType.Friend ? CampType.Enemy : CampType.Friend;
            foreach (var s in slots)
            {
                for (int i = 0; i < damageTranJson.Count; ++i)
                {
                    float dist = Vector3.Distance(pos, Fight.Inst.GetSlotPos(camp, s));
                    float r = (float)damageTranJson[i][0];
                    if (dist <= r)
                    {
                        PlayGeZiEffect(s, camp);
                        break;
                    }
                }
            }
        }
    }

    public void CalcDamage(float dam, List<Character> targets, float hitTime)
    {
        int n = 0;
        foreach (var t in targets)
        {
            float r = 1f;
            if (n < damageTranJson.Count)
                r = (float)damageTranJson[n++];
            float val = dam * r;
            CalcDamage(attacker, t, val, hitTime);
        }
    }

    protected void Hit(Character attacter, Character target, int dam, string hitEff, float hitTime, bool crithit)
    {
        target.ShowHp();

        AddEvent(hitTime, delegate
        {
			foreach (var buff in target.Buffs)
            {
                if (buff.enabled)
                {
					if (buff.HandleDodge(attacter))
                    {
                        target.SetSkillHit(attacker, hitEff, HitSound, true, 0);
                        return;
                    }
                }
            }

            AddTbuff(target);

            dam = AddTdam(attacker, target, dam, false, tdamBaseValue, tdamPerValue, tdamType, HitEffect);

            foreach (var buff in attacker.Buffs)
            {
                if (buff.enabled)
                {
                    dam = buff.HandleHitPre(target, dam);
                }
            }

            if (attacker.ExtraDamage > 0f)
				dam += attacker.ExtraDamage;

            if (target.DecreaseDamage > 0f)
				dam = Mathf.Clamp(dam - target.DecreaseDamage, 1, dam);

			target.BeHit(attacker, dam, hitEff, crithit, skilldata.HitAnim, skilldata.HitAnger, skilldata.HitSound);
        });
    }

    public void AddTbuff(Character target)
    {
		if (skilldata.Tbuff.Equals("null") == false)
        {
			string str = skilldata.Tbuff;
            if (str[0] == '[')
            {
                JsonData jsd = JsonMapper.ToObject(str);
                for (int i = 0; i < jsd.Count; ++i)
                {
                    int buffId = (int)jsd[i];
                    target.AddBuff(attacker, buffId, lv);
                }
            }
            else
            {
                int buffId = int.Parse(str);
                target.AddBuff(attacker, buffId, lv);
            }
        }
    }

    public void RemoveTBuff(Character target)
    {
        string strBuff = skilldata.Tbuff;
        if (strBuff.Equals("null") == false)
        {
            if (strBuff[0] == '[')
            {
                JsonData jsd = JsonMapper.ToObject(strBuff);
                for (int i = 0; i < jsd.Count; ++i)
                {
                    int buffId = (int)jsd[i];
                    target.EndBuff(buffId);
                }
            }
            else
            {
                int buffId = int.Parse(strBuff);
                target.EndBuff(buffId);
            }
        }
    }

    public int Tskill
    {
        get
        {
            return skilldata.Tskill;
        }
    }

    public NfSkill DoTskill()
    {
        NfSkill skill = null;
		if (skilldata.Tskill != -1 && attacker.target != null)
        {
			skill = attacker.DoSkill(skilldata.Tskill, lv, this);
        }
        return skill;
    }

    public void AddHp(Character speller, Character target, int addHp, string hitEff, float hitTime)
    {
        target.ShowHp();
        AddEvent(hitTime, delegate
        {
            target.AddHp(speller, addHp, hitEff);
        });
    }

    public Character GetSecondAttackTarget()
    {
        Character enemy = null;
        List<Character> enemys;
        enemys = Fight.Inst.teams[attacker.camp == CampType.Friend ? 1 : 0];
        bool isSencond = false;
        for (int i = enemys.Count - 1; i >= 0; --i)
        {
            Character e = enemys[i];
            if (e.CanBeAtk)
            {
                enemy = e;
                if (isSencond)
                    break;
                isSencond = true;
            }
        }
        return enemy;
    }

    public Character GetLastAttackTarget()
    {
        return Fight.Inst.FindLastAttackTarget(attacker);
    }

    public Character GetFirstAttackTarget()
    {
        if (rangeType == RangeType.LeastHp)
        {
            List<Character> chars = Fight.Inst.FindAllAttackTarget(attacker);
            Character temp = attacker.target;
            float per = (float)temp.HP / (float)temp.MaxHp;
            foreach (var c in chars)
            {
                float p = (float)c.HP / (float)c.MaxHp;
                if (p < per)
                {
                    per = p;
                    temp = c;
                }
            }
            return temp;
        }
        else if (rangeType == RangeType.FriendLeastHp)
        {
            List<Character> chars = Fight.Inst.FindAllFriend(attacker);
            if (chars.Count > 0)
            {
                Character temp = chars[0];
                float per = (float)temp.HP / (float)temp.MaxHp;
                for (int i = 1; i < chars.Count; ++i)
                {
                    Character c = chars[i];
                    float p = (float)c.HP / (float)c.MaxHp;
                    if (p < per)
                    {
                        per = p;
                        temp = c;
                    }
                }
                return temp;
            }
        }
        Character ret = Fight.Inst.FindAttackTarget(attacker);
        if (ret == null) ret = attacker.target;
        return ret;
    }

    public Character GetFirstFriend()
    {
        return Fight.Inst.FindFriendFirstTarget(attacker);
    }

    public List<Character> FindTargets(bool enemy)
    {
        Character beAtkTarget = attacker.target;
        if (enemy)
        {
            if (beAtkTarget == null)
                return new List<Character>();
        }

        List<Character> ret = null;
		List<int> slots = new List<int> ();
        switch (rangeType)
        {
            case RangeType.Self:
                {
                    ret = new List<Character>();
                    ret.Add(attacker);
                    slots.Add(ret[0].slot);
                }break;
            case RangeType.Single:
                {
                    ret = new List<Character>();
                    if (enemy)
                    {
                        ret.Add(beAtkTarget);
                    }
                    else
                    {
                        Character c = GetFirstFriend();
                        if (c != null)
                            ret.Add(c);
                    }

                    if (ret.Count > 0)
                    {
                        slots.Add(ret[0].slot);
                    }
				}break;
            case RangeType.FrontRow:
                {
                    int count = MaxNum;
                    if (count <= 0)
                        count = 5;
                    Character t;
                    if (enemy)
                        t = beAtkTarget;
                    else
                        t = GetFirstFriend();
                    if (t == null)
                    {
                        ret = new List<Character>();
                        return ret;
                    }
                    ret = Fight.Inst.FindRowTargets(t, count);
                    if (ret.Count > 0)
                    {
                        int slot = ret[0].slot;
                        if (count == 5)
                        {
                            if (slot >= 9)
                            {
                                for (int i = 0; i < 5; ++i)
                                {
                                    slots.Add(9 + i);//9, 10, 11, 12, 13
                                }
                            }
                            else if (slot >= 4)
                            {
                                for (int i = 0; i < 5; ++i)
                                {
                                    slots.Add(4 + i);//4,5, 6, 7, 8
                                }
                            }
                            else if (slot >= 1)
                            {
                                for (int i = 0; i < 3; ++i)
                                {
                                    slots.Add(1 + i);//1, 2, 3
                                }
                            }
                        }
                        else
                        {
                            slots.Add(slot);
                            int num = 1;
                            while (num < count)
                            {
                                ++num;
                                int idx = Fight.Inst.GetNextSlotIdx(slot);
                                if (idx != -1)
                                {
                                    slots.Add(idx);
                                    slot = idx;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                }break;
            case RangeType.Column:
                {
                    int count = MaxNum;
                    if (count <= 0)
                        count = 4;
                    Character t = beAtkTarget;
                    if (t == null)
                    {
                        ret = new List<Character>();
                        return ret;
                    }
                    if (count == 4)
                    {
                        ret = Fight.Inst.FindColumnTargets(t);
                        slots = Fight.Inst.FindForwardColumnSlots(t.slot);
                    }
                    else
                    {
                        ret = Fight.Inst.FindColumnTargets(t, count);

                        slots.Add(t.slot);
                        int slot = Fight.Inst.GetBackSlotIdx(t.slot);
                        while (slot != -1 && slots.Count < count)
                        {
                            slots.Add(slot);
                            slot = Fight.Inst.GetBackSlotIdx(slot);
                        }
                    }
                }break;
            case RangeType.Round:
                {
                    Character t;
                    if (enemy)
                        t = beAtkTarget;
                    else
                        t = GetFirstFriend();
                    if (t == null)
                    {
                        ret = new List<Character>();
                        return ret;
                    }
                    ret = Fight.Inst.FindNearTargets(t);

                    slots.Add(t.slot);
                    int leftSlot = Fight.Inst.GetNextSlotIdx(t.slot);
                    int backslot = Fight.Inst.GetBackSlotIdx(t.slot);
					if (leftSlot != -1) { slots.Add(leftSlot); }
					if (backslot != -1) { slots.Add(backslot); }

                }break;
            case RangeType.All:
                {
                    if (enemy)
                        ret = Fight.Inst.FindAllAttackTarget(attacker);
                    else
                        ret = Fight.Inst.FindAllFriend(attacker);
                }break;
			case RangeType.Round_4:
				{
					Character t;
                    if (enemy)
                        t = beAtkTarget;
					else
                        t = GetFirstFriend();
                    if (t == null)
                    {
                        ret = new List<Character>();
                        return ret;
                    }
                    ret = Fight.Inst.FindNearAndBackTargets(t);

                    slots.Add(t.slot);
                    int leftSlot = Fight.Inst.GetNextSlotIdx(t.slot);
                    int backslot = Fight.Inst.GetBackSlotIdx(t.slot);
                    int nearBackslot = Fight.Inst.GetBackSlotIdx(leftSlot);
					if (leftSlot != -1) { slots.Add(leftSlot); }
					if (nearBackslot != -1) { slots.Add(nearBackslot); }
					if (backslot != -1) { slots.Add(backslot); }
				}break;
            case RangeType.LeastHp:
                {
                    ret = new List<Character>();
                    List<Character> chars;
                    Character temp;
                    if (enemy)
                    {
                        chars = Fight.Inst.FindAllAttackTarget(attacker);
                        temp = beAtkTarget;
                        if (temp.IsDead && chars.Count > 0)
                        {
                            temp = chars[0];
                            chars.Remove(temp);
                        }
                    }
                    else
                    {
                        chars = Fight.Inst.FindAllFriend(attacker);
                        temp = attacker;
                    }
                    float per = (float)temp.HP / (float)temp.MaxHp;
                    foreach (var c in chars)
                    {
                        float p = (float)c.HP / (float)c.MaxHp;
                        if (p < per)
                        {
                            per = p;
                            temp = c;
                        }
                    }
                    ret.Add(temp);
                    slots.Add(temp.slot);
                }break;
            case RangeType.MirrorAOE:
                {
                    CampType camp;
                    if (attacker.camp == CampType.Friend)
                        camp = CampType.Enemy;
                    else
                        camp = CampType.Friend;
                    
                    ret = Fight.Inst.FindAOE_9Targets(camp, attacker.slot);
                    slots = Fight.Inst.GetAOE_9SlotIdxs(attacker.slot);
                }break;
            case RangeType.RandSingle:
                {
                    List<Character> chars;
                    if (enemy)
                        chars = Fight.Inst.FindAllAttackTarget(attacker);
                    else
                        chars = Fight.Inst.FindAllFriend(attacker);

                    ret = new List<Character>();
                    int count = MaxNum;
                    if (count > 1)
                    {
                        Character t;
                        if (enemy)
                            t = beAtkTarget;
                        else
                            t = GetFirstFriend();
                        if (t != null && t.IsDead == false)
                        {
                            ret.Add(t);
                        }
                        chars.Remove(t);
                    }

					while(chars.Count > 0 && ret.Count < count)
					{
                        int idx = Fight.Rand(0, chars.Count);
						Character c = chars[idx];
						chars.Remove(c);
						ret.Add(c);
					}
                    for (int i = 0; i < ret.Count; ++i)
                    {
                        slots.Add(ret[i].slot);
                    }
                } break;
            case RangeType.AOE_9:
                {
                    if (enemy)
                    {
                        ret = Fight.Inst.FindAOE_9Targets(beAtkTarget.camp, beAtkTarget.slot);
                        slots = Fight.Inst.GetAOE_9SlotIdxs(beAtkTarget.slot);
                    }
                    else
                    {
                        ret = Fight.Inst.FindAOE_9Targets(attacker.camp, attacker.slot);
                        slots = Fight.Inst.GetAOE_9SlotIdxs(attacker.slot);
                    }
                } break;
            case RangeType.AOE_5:
                {
                    if (enemy)
                    {
                        ret = Fight.Inst.FindAOE_5Targets(beAtkTarget.camp, beAtkTarget.slot);
                        slots = Fight.Inst.GetAOE_5SlotIdxs(beAtkTarget.slot);
                    }
                    else
                    {
                        ret = Fight.Inst.FindAOE_5Targets(attacker.camp, attacker.slot);
                        slots = Fight.Inst.GetAOE_5SlotIdxs(attacker.slot);
                    }
                } break;
            case RangeType.AOE_4:
                {
                    if (enemy)
                    {
                        ret = Fight.Inst.FindAOE_4Targets(beAtkTarget.camp, beAtkTarget.slot);
                        slots = Fight.Inst.GetAOE_4SlotIdxs(beAtkTarget.slot);
                    }
                    else
                    {
                        ret = Fight.Inst.FindAOE_4Targets(attacker.camp, attacker.slot);
                        slots = Fight.Inst.GetAOE_4SlotIdxs(attacker.slot);
                    }
                } break;
            case RangeType.AOE_8:
                {
                    if (enemy)
                    {
                        ret = Fight.Inst.FindAOE_8Targets(beAtkTarget.camp, beAtkTarget.slot);
                        slots = Fight.Inst.GetAOE_8SlotIdxs(beAtkTarget.slot);
                    }
                    else
                    {
                        ret = Fight.Inst.FindAOE_8Targets(attacker.camp, attacker.slot);
                        slots = Fight.Inst.GetAOE_8SlotIdxs(attacker.slot);
                    }
                } break;
            case RangeType.Round_6:
                {
                    if (enemy)
                    {
                        ret = Fight.Inst.FindRound_6Targets(beAtkTarget.camp, beAtkTarget.slot);
                        slots = Fight.Inst.GetRound_6SlotIdxs(beAtkTarget.slot);
                    }
                    else
                    {
                        ret = Fight.Inst.FindRound_6Targets(attacker.camp, attacker.slot);
                        slots = Fight.Inst.GetRound_6SlotIdxs(attacker.slot);
                    }
                } break;
            case RangeType.LeftRight_3:
                {
                    if (enemy)
                    {
                        ret = Fight.Inst.FindLeftRight_3Targets(beAtkTarget.camp, beAtkTarget.slot);
                        slots = Fight.Inst.GetLeftRight_3SlotIdxs(beAtkTarget.slot);
                    }
                    else
                    {
                        ret = Fight.Inst.FindLeftRight_3Targets(attacker.camp, attacker.slot);
                        slots = Fight.Inst.GetLeftRight_3SlotIdxs(attacker.slot);
                    }
                } break;
            case RangeType.ReadyCastSkill:
                {
                    if (enemy)
                    {
                        ret = new List<Character>();
                        Character chara = Fight.Inst.FindNextCastSkillTarget(beAtkTarget.camp);
                        if (chara != null)
                        {
                            ret.Add(chara);
                            slots.Add(chara.slot);
                        }
                    }
                    else
                    {
                        ret = new List<Character>();
                        Character chara = Fight.Inst.FindNextCastSkillTarget(attacker.camp);
                        if (chara != null)
                        {
                            ret.Add(chara);
                            slots.Add(chara.slot);
                        }
                    }
                } break;
            case RangeType.ForwardColumn:
                {
                    if (enemy)
                    {
                        CampType camp = attacker.camp == CampType.Friend ? CampType.Enemy : CampType.Friend;
                        ret = Fight.Inst.FindForwardColumnTargets(camp, attacker.slot);
                        slots = Fight.Inst.FindForwardColumnSlots(attacker.slot);
                    }
                    else
                    {
                        ret = Fight.Inst.FindForwardColumnTargets(attacker.camp, attacker.slot);
                        slots = Fight.Inst.FindForwardColumnSlots(attacker.slot);
                    }
                } break;
            case RangeType.FriendLeastHp:
                {
                    ret = new List<Character>();
                    List<Character> chars = Fight.Inst.FindAllFriend(attacker);
                    Character temp = attacker;
                    if (chars.Count > 0)
                    {
                        temp = chars[0];
                        float per = (float)temp.HP / (float)temp.MaxHp;
                        for (int i = 1; i < chars.Count; ++i)
                        {
                            Character c = chars[i];
                            float p = (float)c.HP / (float)c.MaxHp;
                            if (p < per)
                            {
                                per = p;
                                temp = c;
                            }
                        }

                    }
                    ret.Add(temp);
                    slots.Add(temp.slot);
                } break;
            case RangeType.RandSingleExcludeMe:
                {
                    ret = new List<Character>();
                    List<Character> chars;
                    if (enemy)
                    {
                        chars = Fight.Inst.FindAllAttackTarget(attacker);
                        if (chars.Count == 0)
                        {
                            chars.Add(beAtkTarget);
                        }
                    }
                    else
                    {
                        chars = Fight.Inst.FindAllFriend(attacker);
                        if (chars.Count > 1)
                        {
                            chars.Remove(attacker);
                        }
                    }

					if (chars.Count > 0)
					{
                        int idx = Fight.Rand(0, chars.Count);
						Character c = chars[idx];
						ret.Add(c);
					}
                    for (int i = 0; i < ret.Count; ++i)
                    {
                        slots.Add(ret[i].slot);
                    }
                }break;
            default:
                {
                    ret = new List<Character>();
                } break;
        }

		ClearGeZiEffects ();
		
		for(int i=0; i<slots.Count; ++i)
		{
            if (slots[i] == -1)
                continue;
			if (attacker.camp == CampType.Friend && enemy)
			{
				slots[i] += Fight.EnemyBeginSlot;
            }
            else if (attacker.camp == CampType.Enemy && enemy == false)
            {
                slots[i] += Fight.EnemyBeginSlot;
            }

            Fight.Inst.ShowGeZiEffect(slots[i]);
		}
        m_geZiEffects = slots;

        return ret;
    }

	protected List<int> m_geZiEffects = new List<int>();

	public void ClearGeZiEffects()
	{
		for (int i= m_geZiEffects.Count-1; i>=0; --i)
		{
            Fight.Inst.HideGeZiEffect(m_geZiEffects[i]);
		}

		m_geZiEffects.Clear ();
	}

    public void PlayGeZiEffect(int slot, CampType camp)
    {
        if (camp == CampType.Enemy)
            slot += Fight.EnemyBeginSlot;

        if (m_geZiEffects.Contains(slot) == false)
        {
            Fight.Inst.ShowGeZiEffect(slot);
            m_geZiEffects.Add(slot);
        }
    }

    public static float CalcRadius(Character v1, Character v2)
    {
        return v1.radius + v2.radius;
    }

    public float TotalTime
    {
		get { return skilldata.Time; }
    }

    public float HitTime
    {
        get { return float.Parse(skilldata.HitTime); }
    }

    public float[] HitTimes
    {
        get
        {
			string str = skilldata.HitTime;
            if (str[0] == '[')
            {
                JsonData json = JsonMapper.ToObject(str);
                float[] arr = new float[json.Count];
                for (int i = 0; i < json.Count; ++i)
                {
                    arr[i] = (float)json[i];
                }
                return arr;
            }
            else
            {
                float[] arr = new float[1];
                arr[0] = float.Parse(str);
                return arr;
            }
        }
    }

    public string HitEffect
    {
		get { return skilldata.HitEffect; }
    }

    public string HitSelfEffect
    {
		get { return skilldata.HitSelfEffect; }
    }

    public string HitSound
    {
		get { return skilldata.HitSound; }
    }

    public float SingTime
    {
		get { return skilldata.SingTime; }
    }

    public string SingAnim
    {
		get { return skilldata.SingAnim; }
    }

    public string SingEffect
    {
		get { return skilldata.SingEffect; }
    }

    public JsonData SingSound
    {
        get
        {
			string str = skilldata.SingSound;
            JsonData jsd = JsonMapper.ToObject(str);
            return jsd;
        }
    }

    public string FireAnim
    {
		get { return skilldata.FireAnim; }
    }

    public string FireEffect
    {
		get { return skilldata.FireEffect; }
    }

    public JsonData FireSound
    {
        get
        {
			string str = skilldata.FireSound;
            JsonData jsd = JsonMapper.ToObject(str);
            return jsd;
        }
    }

    public string KeepAnim
    {
		get { return skilldata.KeepAnim; }
    }

    public string KeepEffect
    {
		get { return skilldata.KeepEffect; }
    }

    public float KeepTime
    {
		get { return skilldata.KeepTime; }
    }

    public int AtkC
    {
		get { return skilldata.AtkC; }
    }

    public string FireBone
    {
		get { return skilldata.FireBone; }
    }

    public string BulletModel
    {
		get { return skilldata.BulletModel; }
    }

    public float BulletSpeed
    {
		get { return float.Parse(skilldata.BulletSpeed); }
    }

    public int MaxNum
    {
		get { return skilldata.MaxNum; }
    }

    public int Buff
    {
        get
        {
            string strBuff = skilldata.Buff;
            if (strBuff.Equals("null") == false)
            {
                if (strBuff[0] == '[')
                {
                    JsonData jsdBuff = JsonMapper.ToObject(strBuff);
                    int buffId = (int)jsdBuff[0];
                    return buffId;
                }
                else
                {
                    int buffId = int.Parse(strBuff);
                    return buffId;
                }
            }
            return -1;
        }
    }

    public List<int> Buffs
    {
        get
        {
            List<int> ret = new List<int>();
            string strBuff = skilldata.Buff;
            if (strBuff.Equals("null") == false)
            {
                if (strBuff[0] == '[')
                {
                    JsonData jsdBuff = JsonMapper.ToObject(strBuff);
                    for (int i = 0; i < jsdBuff.Count; ++i)
                    {
                        ret.Add((int)jsdBuff[i]);
                    }
                }
                else
                {
                    int buffId = int.Parse(strBuff);
                    ret.Add(buffId);
                }
            }
            return ret;
        }
    }

    public int Tbuff
    {
        get
        {
            string strBuff = skilldata.Tbuff;
            if (strBuff.Equals("null") == false)
            {
                if (strBuff[0] == '[')
                {
                    JsonData jsdBuff = JsonMapper.ToObject(strBuff);
                    int buffId = (int)jsdBuff[0];
                    return buffId;
                }
                else
                {
                    int buffId = int.Parse(strBuff);
                    return buffId;
                }
            }
            return -1;
        }
    }

    public List<int> Tbuffs
    {
        get
        {
            List<int> ret = new List<int>();
            string strBuff = skilldata.Tbuff;
            if (strBuff.Equals("null") == false)
            {
                if (strBuff[0] == '[')
                {
                    JsonData jsdBuff = JsonMapper.ToObject(strBuff);
                    for (int i = 0; i < jsdBuff.Count; ++i)
                    {
                        ret.Add((int)jsdBuff[i]);
                    }
                }
                else
                {
                    int buffId = int.Parse(strBuff);
                    ret.Add(buffId);
                }
            }
            return ret;
        }
    }

    void PlaySingSound()
    {
        PlaySound(SingSound);
    }

    void PlayFireSound()
    {
        PlaySound(FireSound);
    }

    public void PlaySound(string anim)
    {
        if (SingSound.Count > 0 && anim == SingAnim)
        {
            PlaySingSound();
        }
        else if (FireSound.Count > 0 && anim == FireAnim)
        {
            PlayFireSound();
        }
    }

    private void PlaySound(JsonData jsd)
    {
        for (int i = 0; i < jsd.Count; ++i)
        {
            JsonData j = jsd[i];
            float delay = (float)j[0];
            string sound = (string)j[1];

            AddEvent(delay / 30f, delegate
            {
                attacker.PlaySound(sound);
            });
        }
    }

    protected virtual void turnTarget()
    {
    }

    public delegate void MoveCallback();
    public void Move(Character chara, Vector3 destPos, float moveSpeed, string anim, float animSpeed, MoveCallback callback)
    {
        chara.PlayAnim(anim, animSpeed);
        Move(chara, destPos, moveSpeed, callback);
    }

    public void MoveNoRotate(Character chara, Vector3 destPos, float moveSpeed, string anim, float animSpeed, MoveCallback callback)
    {
        chara.PlayAnim(anim, animSpeed);
        MoveNoRotate(chara, destPos, moveSpeed, callback);
    }

    public void MoveNoRotate(Character chara, Vector3 destPos, float moveSpeed, MoveCallback callback)
    {
        Vector3 dir = destPos - chara.position;
        dir.Normalize();

        float mttime = Vector3.Distance(chara.position, destPos) / moveSpeed;
        float mtime = 0f;

        AddEvent(delegate
        {
            mtime += Time.deltaTime;
            if (mtime >= mttime)
            {
                chara.position = destPos;
                callback();
                return true;
            }
            else
            {
                float md = Time.deltaTime * moveSpeed;
                chara.position += dir * md;
            }
            return false;
        });
    }

    public void MoveNoRotate(Transform tran, Vector3 destPos, float moveSpeed, MoveCallback callback)
    {
        Vector3 dir = destPos - tran.position;
        dir.Normalize();

        float mttime = Vector3.Distance(tran.position, destPos) / moveSpeed;
        float mtime = 0f;
        AddEvent(delegate
        {
            mtime += Time.deltaTime;
            if (mtime >= mttime)
            {
                tran.position = destPos;
                callback();
                return true;
            }
            else
            {
                float md = Time.deltaTime * moveSpeed;
                tran.position += dir * md;
            }
            return false;
        });
    }

    public void Move(Character chara, Vector3 destPos, float moveSpeed, MoveCallback callback)
    {
        float dist = Vector3.Distance(chara.position, destPos);
        if (dist > 0f)
        {
            Vector3 dir = destPos - chara.position;
            dir.Normalize();
            Quaternion toRotation = Quaternion.LookRotation(dir);
            chara.rotation = toRotation;

            float mttime = dist / moveSpeed;
            float mtime = 0f;
            AddEvent(delegate
            {
                turnTarget();
                mtime += Time.deltaTime;
                if (mtime >= mttime)
                {
                    chara.position = destPos;
                    callback();
                    return true;
                }
                else
                {
                    float md = Time.deltaTime * moveSpeed;
                    chara.position += dir * md;
                }
                return false;
            });
        }
        else
        {
            callback();
        }
    }

    public void Move(Transform tran, Vector3 destPos, float moveSpeed, MoveCallback callback)
    {
        float dist = Vector3.Distance(tran.position, destPos);
        if (dist > 0f)
        {
            Vector3 dir = destPos - tran.position;
            dir.Normalize();
            Quaternion toRotation = Quaternion.LookRotation(dir);
            tran.rotation = toRotation;

            float mttime = dist / moveSpeed;
            float mtime = 0f;
            AddEvent(delegate
            {
                turnTarget();
                mtime += Time.deltaTime;
                if (mtime >= mttime)
                {
                    tran.position = destPos;
                    callback();
                    return true;
                }
                else
                {
                    float md = Time.deltaTime * moveSpeed;
                    tran.position += dir * md;
                }
                return false;
            });
        }
        else
        {
            callback();
        }
    }

    public void Move(Transform tran, Transform tranDest, float moveSpeed, MoveCallback callback)
    {
        Vector3 dir = tranDest.position - tran.position;
        dir.Normalize();
        Quaternion toRotation = Quaternion.LookRotation(dir);
        tran.rotation = toRotation;

        AddEvent(delegate
        {
            dir = tranDest.position - tran.position;
            dir.Normalize();
            float dist = Vector3.Distance(tran.position, tranDest.position);
            float md = Time.deltaTime * moveSpeed;
            if (md >= dist)
            {
                tran.position = tranDest.position;
                callback();
                return true;
            }
            else
            {
                tran.position += dir * md;
            }
            return false;
        });
    }

    public void MoveSrcPos(Character chara, MoveCallback callback)
    {
        float moveSpeed = 50f;
        float animSpeed = (moveSpeed / attacker.moveSpeed) * attacker.moveAnimSpeed;
        Move(chara, chara.SrcPos, moveSpeed, "run", animSpeed, delegate
        {
            chara.position = new Vector3(chara.SrcPos.x, chara.SrcPos.y, chara.SrcPos.z);
            chara.rotateBack();
            chara.Idle();

            callback();
        });
    }

    public void MoveSrcPos(Character chara)
    {
        MoveSrcPos(chara, delegate
        {
            End();
        });
    }

    public delegate bool MoveUpdateCallback();
    public void MoveUpdate(Transform tran, Vector3 destPos, float moveSpeed, MoveUpdateCallback callback)
    {
        float dist = Vector3.Distance(tran.position, destPos);
        if (dist > 0f)
        {
            Vector3 dir = destPos - tran.position;
            dir.Normalize();
            Quaternion toRotation = Quaternion.LookRotation(dir);
            tran.rotation = toRotation;

            float mttime = dist / moveSpeed;
            float mtime = 0f;
            AddEvent(delegate
            {
                turnTarget();
                mtime += Time.deltaTime;
                if (mtime >= mttime)
                {
                    tran.position = destPos;
                    callback();
                    return true;
                }
                else
                {
                    float md = Time.deltaTime * moveSpeed;
                    tran.position += dir * md;
                }
                return callback();
            });
        }
        else
        {
            callback();
        }
    }

    public static bool CanFanJi(Character target)
    {
        if (target.IsDead)
            return false;
        if (target.CurrSkill == null)
            return false;
        if (target.CurrSkill.isFanJi)
            return false;
        if (target.CurrSkill.id == target.bigSkillId)
            return false;
        if (target.isActiveBigSkill || target.isActiveComboSkill)
            return false;
        if (target.IsAntiFanJi)
			return false;
		if (target.isFenShen)
            return false;
        if (target.CurrSkill.CanFanJi())
            return true;
        return false;
    }

    public void ShowMesh(bool visible)
    {
        foreach (var m in hideMesh)
        {
            m.enabled = visible;
        }
    }

    public void AddBuff(Character target)
    {
        AddBuff(target, false);
    }

    public void AddBuff(Character target, bool forever)
    {
        int buffLv = lv;

        string strBuff = skilldata.Buff;
        if (strBuff.Equals("null") == false)
        {
            if (strBuff[0] == '[')
            {
                JsonData jsd = JsonMapper.ToObject(strBuff);
                for (int i = 0; i < jsd.Count; ++i)
                {
                    int buffId = (int)jsd[i];
                    target.AddBuff(attacker, buffId, buffLv, forever);
                }
            }
            else
            {
                int buffId = int.Parse(strBuff);
                target.AddBuff(attacker, buffId, buffLv, forever);
            }
        }
    }

    public void RemoveBuff(Character target)
    {
        string strBuff = skilldata.Buff;
        if (strBuff.Equals("null") == false)
        {
            if (strBuff[0] == '[')
            {
                JsonData jsd = JsonMapper.ToObject(strBuff);
                for (int i = 0; i < jsd.Count; ++i)
                {
                    int buffId = (int)jsd[i];
                    target.EndBuff(buffId);
                }
            }
            else
            {
                int buffId = int.Parse(strBuff);
                target.EndBuff(buffId);
            }
        }
    }

    public void HideAllFriend(List<Character> attackers)
    {
        if (attacker.camp == CampType.Enemy)
            return;

        hideChars = new List<Character>();
        List<Character> friends = Fight.Inst.FindAllFriend(attacker);
        foreach (var c in friends)
        {
            if (c.visible && attackers.Contains(c) == false)
            {
                c.visible = false;
                hideChars.Add(c);
            }
        }
    }

    public void ShowAllFriend()
    {
        if (hideChars == null)
            return;

        foreach (var c in hideChars)
        {
            c.visible = true;
        }
    }

    protected LuaTable table;

    protected virtual void OnSkillBegin()
    {
        table = LuaMgr.NewTable(skilldata.Func);
        table["skill"] = this;
        table["attacker"] = attacker;
        table["target"] = attacker.target;

        LuaMgr.CallMethod(table, "onBegin", table);
    }

    protected virtual void OnSkillEnd()
    {
        LuaMgr.CallMethod(table, "onEnd", table);
    }

    public List<Character> GetComboCharList()
    {
        List<Character> attackers = new List<Character>();
        JsonData[] comb = JsonMapper.ToObject<JsonData[]>(attacker.Data.Combo);
        attackers.Add(attacker);
        for (int i = 0; i < comb.Length; ++i)
        {
            Character c = Fight.Inst.FindFriend(attacker, (int)comb[i]);
            if (c != null)
            {
                attackers.Add(c);
            }
        }
        return attackers;
    }

    protected EffectObject effScene = null;
    public void HideScene()
    {
        if (attacker.camp == CampType.Friend)
        {
            effScene = PlayEffect(gameObject, "Effects/eff_qiu/eff_qiu", -1);
            Fight.Inst.scene.SetActive(false);

            Time.timeScale = 0.7f * GameMgr.timeScale;
            GameObject light = GameObject.Find("ComboLight");
            light.GetComponent<Light>().enabled = true;

            Fight.Inst.m_UiInGameScript.gameObject.SetActive(false);
            UI3DButton.enabled = false;
        }
    }

    public void ShowScene()
    {
        if (attacker.camp == CampType.Friend && UI3DButton.enabled == false)
        {
            Fight.Inst.scene.SetActive(true);

            if (effScene != null)
                DestroyEff(effScene);
            Time.timeScale = GameMgr.timeScale;

            GameObject light = GameObject.Find("ComboLight");
            light.GetComponent<Light>().enabled = false;

            Fight.Inst.m_UiInGameScript.gameObject.SetActive(true);
            UI3DButton.enabled = true;
        }
    }

    public int GetComboDamage(List<Character> attackers)
    {
        int dam = 0;
        for (int i = 0; i < attackers.Count; ++i)
        {
            Character c = attackers[i];
            switch (skillDamType)
            {
                case SkillDamageType.ATK:
                    {
                        dam += Clamp(c.ATK);
                    } break;
                case SkillDamageType.HP:
                    {
                        dam += Clamp(c.HP);
                    } break;
                case SkillDamageType.AS:
                    {
                        dam += Clamp(c.AS);
                    } break;
                case SkillDamageType.DEF:
                    {
                        dam += Clamp(c.DEF);
                    } break;
            }
        }
        dam /= attackers.Count;
        dam = Mathf.FloorToInt(dam * skillDamageFactor + baseSkillDamage);
        return dam;
    }

    public List<Character> FindAllAttackTarget(Character chara)
    {
        return Fight.Inst.FindAllAttackTarget(chara);
    }

    public void FireDirBullet(List<Character> targets, Vector3 srcPos, Vector3 destPos, string bulletModel, float bulletSpeed, float dam, int atkC, float hitTime)
    {
        FireDirBullet(targets, srcPos, destPos, bulletModel, bulletSpeed, dam, atkC, hitTime, 1f);
    }

    public void FireDirBullet(List<Character> targets, Vector3 srcPos, Vector3 destPos, string bulletModel, float bulletSpeed, float dam, int atkC, float hitTime, float radius)
    {
        // 计算技能OBB
        Vector3 dir = destPos - srcPos;
        dir.y = 0f;
        dir.Normalize();
        Quaternion rot = Quaternion.LookRotation(dir);
        float dist = Vector3.Distance(destPos, srcPos) + 20f;
        Vector3 center = srcPos + dir.normalized * dist / 2f;
        OBB obb = new OBB(new Vector2(center.x, center.z), new Vector2(dist / 2f, radius), rot.eulerAngles.y);
        Vector2 srcPos2v = new Vector2(srcPos.x, srcPos.z);

        // 播放特效
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject eff = effObj.obj;
        eff.transform.position = srcPos;
        eff.transform.rotation = rot;

        // 检测碰撞
        int n = 0;
        foreach (Character e in targets)
        {
            if (e.CanBeAtk == false)
                continue;

            OBB obbe = new OBB(new Vector2(e.position.x, e.position.z), new Vector2(1f, 1f), e.rotation.eulerAngles.y);
            if (obb.Intersects(obbe))
            {
                float t = 0f;
                if (bulletSpeed > 0f)
                {
                    float s = Vector2.Distance(srcPos2v, e.pos2v);
                    t += s / bulletSpeed;
                }
                float r = 1f;
                if (n < damageTranJson.Count)
                    r = (float)damageTranJson[n++];
                float val = dam * r;
                for (int i = 0; i < atkC; ++i)
                {
                    CalcDamage(attacker, e, val, t + i * hitTime);
                }

                PlayGeZiEffect(e.slot, e.camp);
            }
        }
    }

    public void FireBullet(Vector3 srcPos, Vector3 destPos, string bulletModel, float bulletSpeed, float bulletRadius, float dam)
    {
        List<Character> targets = FindAllAttackTarget(attacker);

        Transform bullet = null;
        EffectObject effObj = PlayEffect(bulletModel, -1f);
        GameObject bulletObj = effObj.obj;
        bullet = bulletObj.transform;
        bullet.position = srcPos;

        MoveUpdate(bullet, destPos, bulletSpeed, delegate
        {
            foreach (Character e in targets)
            {
                if (e.IsDead)
                    continue;

                // 检测碰撞
                float dist = Vector2.Distance(new Vector2(bullet.position.x, bullet.position.z), e.pos2v);
                if (dist <= bulletRadius)
                {
                    DestroyEff(effObj);
                    PlayGeZiEffect(e.slot, e.camp);
                    CalcDamage(attacker, e, dam, 0);
                    return true;
                }
            }
            return false;
        });
    }

    public void FireCurveBullet(Character atk, Character target, float dam, GameEvent.Func callback)
    {
        Vector3 destPos = new Vector3(target.position.x, target.position.y + 1f, target.position.z);
        FireCurveBullet(atk, destPos, dam, callback);
    }

    public void FireCurveBullet(Character atk, Vector3 destPos, float dam, GameEvent.Func callback)
    {
        List<Character> targets = FindAllAttackTarget(attacker);

        float gravity = 45f;
        float speed = 15f;
        JsonData bulletJson = JsonMapper.ToObject(skilldata.BulletSpeed);
        if (bulletJson.IsArray)
        {
            gravity = (float)bulletJson[0];
            speed = (float)bulletJson[1];
        }

        Transform bullet = null;
        EffectObject effObj = PlayEffect(BulletModel, -1f);
        GameObject bulletObj = effObj.obj;
        bullet = bulletObj.transform;

        GameObject hand = atk.FindBone(FireBone);
        bullet.position = new Vector3(hand.transform.position.x, hand.transform.position.y, hand.transform.position.z);

        // 子弹碰撞检测
        Vector2 v2DestPos = new Vector2(destPos.x, destPos.z);
        Vector2 v2SrcPos = new Vector2(bullet.position.x, bullet.position.z);
        Vector2 dir = new Vector2(v2DestPos.x - v2SrcPos.x, v2DestPos.y - v2SrcPos.y);
        dir.Normalize();

        float dist = Vector2.Distance(v2DestPos, v2SrcPos);
        float total = dist / speed;
        float h = destPos.y - bullet.position.y;
        float v = (h - 0.5f * (-gravity) * total * total) / total;
        Vector3 vo = new Vector3(dir.x * speed, v, dir.y * speed);
        Vector3 a = new Vector3(0f, -gravity, 0f);
        Vector3 srcPos = new Vector3(bullet.position.x, bullet.position.y, bullet.position.z);

        float pictch = Mathf.Atan2(v, speed) * Mathf.Rad2Deg;
        bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, 0f, dir.y)) * Quaternion.Euler(-pictch, 0, 0);

        float time = 0;
        AddEvent(delegate
        {
            time += Time.deltaTime;
            Vector3 p = vo * time + 0.5f * a * time * time;
            bullet.position = srcPos + p;

            float vt = v + (-gravity) * time;
            pictch = Mathf.Atan2(vt, speed) * Mathf.Rad2Deg;
            bullet.rotation = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), new Vector3(dir.x, 0f, dir.y)) * Quaternion.Euler(-pictch, 0f, 0f);

            if (time >= total)
            {
                CalcCircalDamage(atk, targets, destPos, dam, 0f);

                DestroyEff(effObj);

                if (callback != null)
                {
                    callback();
                }
                return true;
            }

            return false;
        });
    }

    public Character Summon(int heroId, int slot)
    {
        HeroData jsd_hero = DataMgr.GetHero(heroId);
        if (jsd_hero == null)
            return null;

        Monster monster = new Monster();
        monster.LV = attacker.lv;
        monster.STRENGTHENID = attacker.strengthenid;
        monster.Data = jsd_hero;

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
        c.ReqPassiveSkill();

        Fight.Inst.AddCharacter(c);
        Fight.Inst.SortAllChar(attacker.camp);

        return c;
    }

    public List<int> GetEmptySlots()
    {
        List<Character> allChar = Fight.Inst.GetAllCharacter(attacker.camp);
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
            if (empty)
                slots.Add(i);
        }
        return slots;
    }

    protected virtual bool CanFanJi()
    {
        if (table != null)
        {
            LuaFunction fn = (LuaFunction)table["CanFanJi"];
            if (fn != null)
            {
                return (bool)fn.call(table);
            }
        }
        return false;
    }
}
