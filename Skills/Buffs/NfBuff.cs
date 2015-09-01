using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System;
using SLua;

public interface IWuDiBuff
{
}

public class NfBuff
{
    public enum AttriType
    {
        None = 0,
        ATK = 1,
        HP = 2,
        AS = 3,
        DEF = 4,
        Anger = 5,
        AngerGrow = 6,
        CRI = 7,
        AVD = 8,
        CRIR = 9,
    }

    public struct Buff
    {
        public int id;
        public int lv;
        public int attrType;
    }

    private int _id;
    public int id
    {
        get { return _id; }
    }
    public int lv;
    private int _priority;
    public int priority
    {
        get { return _priority; }
    }
    private int _weight;
    public int weight
    {
        get { return _weight; }
    }
    public Character owner;
    public Character caster;
    public BuffData buffdata;
    protected EffectObject effobj;
    protected bool forever;

    [DoNotToLua]
    public static List<Buff> myShowBuffs = new List<Buff>();
    [DoNotToLua]
    public static List<Buff> enemyShowBuffs = new List<Buff>();

    [DoNotToLua]
    LuaFunction funOnEnd;
    [DoNotToLua]
    LuaFunction funHandleHitPre;
    [DoNotToLua]
    LuaFunction funHandleHitEnd;
    [DoNotToLua]
    LuaFunction funHandleHitHp;
    [DoNotToLua]
    LuaFunction funHandleBeHit;
    [DoNotToLua]
    LuaFunction funHandleBeHitPre;
    [DoNotToLua]
    LuaFunction funHandleAddHp;
    [DoNotToLua]
    LuaFunction funHandleHuiHe;
    [DoNotToLua]
    LuaFunction funHandleWaveAttack;
    [DoNotToLua]
    LuaFunction funHandleSkillBegin;
    [DoNotToLua]
    LuaFunction funHandleSkillEnd;
    [DoNotToLua]
    LuaFunction funHandleDead;
    [DoNotToLua]
    LuaFunction funHandleEnemyHpChange;
    [DoNotToLua]
    LuaFunction funHandleEnemyDead;
    [DoNotToLua]
    LuaFunction funHandleFriendHpChange;
    [DoNotToLua]
    LuaFunction funHandleFriendCastBigSkill;
    [DoNotToLua]
    LuaFunction funHandleFriendPosChange;
    [DoNotToLua]
    LuaFunction funHandleDodge;
    [DoNotToLua]
    LuaFunction funUpdate;

    protected bool _enabled;
    public bool enabled
    {
        set { _enabled = value; }
        get { return _enabled; }
    }

    private bool _isDestroy;
    public bool isDestroy
    {
        get { return _isDestroy; }
    }

    [DoNotToLua]
    public bool Start(BuffData data, Character _caster, bool _forever)
    {
        forever = _forever;
        caster = _caster;
        _enabled = true;
        effobj = null;
        _id = data.ID;
		_priority = data.Priority;
        _weight = data.Weight;
        _isDestroy = false;

        buffdata = data;
        return Init();
    }

    [DoNotToLua]
    public bool Start()
    {
        forever = false;
        caster = owner;
        _enabled = true;
        effobj = null;
        _priority = 1;
        _weight = 0;
        _isDestroy = false;
        return Init();
    }

    protected LuaTable table = null;

    protected virtual bool Init()
    {
        table = LuaMgr.NewTable(buffdata.Mode);
        table["buff"] = this;
        table["owner"] = owner;

        funOnEnd = (LuaFunction)table["OnEnd"];
        funHandleHitPre = (LuaFunction)table["HandleHitPre"];
        funHandleHitEnd = (LuaFunction)table["HandleHitEnd"];
        funHandleHitHp = (LuaFunction)table["HandleHitHp"];
        funHandleBeHit = (LuaFunction)table["HandleBeHit"];
        funHandleBeHitPre = (LuaFunction)table["HandleBeHitPre"];
        funHandleAddHp = (LuaFunction)table["HandleAddHp"];
        funHandleHuiHe = (LuaFunction)table["HandleHuiHe"];
        funHandleWaveAttack = (LuaFunction)table["HandleWaveAttack"];
        funHandleSkillBegin = (LuaFunction)table["HandleSkillBegin"];
        funHandleSkillEnd = (LuaFunction)table["HandleSkillEnd"];
        funHandleDead = (LuaFunction)table["HandleDead"];
        funHandleEnemyHpChange = (LuaFunction)table["HandleEnemyHpChange"];
        funHandleEnemyDead = (LuaFunction)table["HandleEnemyDead"];
        funHandleFriendHpChange = (LuaFunction)table["HandleFriendHpChange"];
        funHandleFriendCastBigSkill = (LuaFunction)table["HandleFriendCastBigSkill"];
        funHandleFriendPosChange = (LuaFunction)table["HandleFriendPosChange"];
        funHandleDodge = (LuaFunction)table["HandleDodge"];
        funUpdate = (LuaFunction)table["Update"];

        return (bool)LuaMgr.CallMethod(table, "OnBegin", table);
    }

    public void End()
    {
        OnEnd();
    }

    [DoNotToLua]
    public virtual void Append()
    {
        _enabled = true;
    }

    [DoNotToLua]
    public virtual void Break()
    {
        OnEnd();
    }

    [DoNotToLua]
    public virtual void Replace()
    {
        OnEnd();
    }

    protected virtual void OnEnd()
    {
        StopSelfEffect();

        _enabled = false;
        _isDestroy = true;
        owner.RemoveBuff(this);

        if (funOnEnd != null)
        {
            funOnEnd.call(table);
        }
    }

    private List<IEnumerator> coroutines = new List<IEnumerator>();

    [DoNotToLua]
    public void StopAllCoroutines()
    {
        foreach (var c in coroutines)
        {
            owner.StopCoroutine(c);
        }
        coroutines.Clear();
    }
    [DoNotToLua]
    public void StartCoroutine(IEnumerator routine)
    {
        owner.StartCoroutine(routine);
        coroutines.Add(routine);
    }
    [DoNotToLua]
    public void StopCoroutine(IEnumerator routine)
    {
        owner.StopCoroutine(routine);
    }
    [DoNotToLua]
    public void StopCoroutine(string methodName)
    {
        owner.StopCoroutine(methodName);
    }

    public void AddEvent(float time, GameEvent.Func f)
    {
        if (time > 0f)
            StartCoroutine(DelayInvoke(time, f));
        else
            f();
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
            if (f() == true)
                break;
        }
    }

    public void AddEvent(GameEvent.FuncNoTime f)
    {
        StartCoroutine(InvokeRepeating(f));
    }

    IEnumerator DelayInvoke(float time, GameEvent.Func f)
    {
        yield return new WaitForSeconds(time);
        try
        {
            f();
        }
        catch (KeyNotFoundException e)
        {
            Debug.LogError("buff(" + id + ") " + e.Message);
            Debug.LogError(e.StackTrace);
			MessageBox.Show("buff param error", "buff(" + id + ") " + e.Message);
        }
    }

    IEnumerator DelayInvokeRepeating(float time, GameEvent.Func f, int count)
    {
        while (count > 0)
        {
            yield return new WaitForSeconds(time);
            try
            {
                f();
            }
            catch (KeyNotFoundException e)
            {
				Debug.LogError("buff(" + id + ") " + e.Message);
                Debug.LogError(e.StackTrace);
				MessageBox.Show("buff param error", "buff(" + id + ") " + e.Message);
            }
            --count;
        }
    }

    // 命中目标前执行
    [DoNotToLua]
    public virtual int HandleHitPre(Character target, int damage)
    {
        if (funHandleHitPre != null)
        {
            return LuaMgr.CallInt(funHandleHitPre, table, target, damage);
        }
        return damage;
    }

    // 命中目标后执行
    [DoNotToLua]
    public virtual void HandleHitEnd(Character target)
    {
        if (funHandleHitEnd != null)
        {
            funHandleHitEnd.call(table, target);
        }
    }

    // 处理目标掉血
    [DoNotToLua]
    public virtual int HandleHitHp(Character target, int damage, int addAnger)
    {
        if (funHandleHitHp != null)
        {
            return LuaMgr.CallInt(funHandleHitHp, table, target, damage, addAnger);
        }
        return damage;
    }

    // 被攻击时执行
    [DoNotToLua]
    public virtual int HandleBeHit(Character attacker, int damage, int addAnger)
    {
        if (funHandleBeHit != null)
        {
            return LuaMgr.CallInt(funHandleBeHit, table, attacker, damage, addAnger);
        }
        return damage;
    }

    // 被攻击前执行
    public virtual bool HandleBeHitPre(Character attacker)
    {
        if (funHandleBeHitPre != null)
        {
            return (bool)funHandleBeHitPre.call(table, attacker);
        }
        return false;
    }

    // 被攻击时执行
    [DoNotToLua]
    public virtual int HandleAddHp(Character attacker, int hp)
    {
        if (funHandleAddHp != null)
        {
            return LuaMgr.CallInt(funHandleAddHp,table, attacker, hp);
        }
        return hp;
    }

    // 每回合执行
    [DoNotToLua]
    public virtual void HandleHuiHe()
    {
        if (funHandleHuiHe != null)
        {
            funHandleHuiHe.call(table);
        }
    }

    // 每一波攻击
    [DoNotToLua]
    public virtual void HandleWaveAttack()
    {
        if (funHandleWaveAttack != null)
        {
            funHandleWaveAttack.call(table);
        }
    }

    // 每次攻击执行
    [DoNotToLua]
    public virtual NfSkill HandleSkillBegin()
    {
        if (funHandleSkillBegin != null)
        {
            return (NfSkill)funHandleSkillBegin.call(table);
        }
        return null;
    }

    // 每次攻击结束
    [DoNotToLua]
    public virtual void HandleSkillEnd(NfSkill skill)
    {
        if (funHandleSkillEnd != null)
        {
            funHandleSkillEnd.call(table, skill);
        }
    }

    // 每次攻击执行
    [DoNotToLua]
    public virtual void HandleDead()
    {
        if (funHandleDead != null)
        {
            funHandleDead.call(table);
        }
    }

    // 对方英雄掉血执行
    [DoNotToLua]
    public virtual void HandleEnemyHpChange(Character target)
    {
        if (funHandleEnemyHpChange != null)
        {
            funHandleEnemyHpChange.call(table, target);
        }
    }

    // 每次攻击执行
    [DoNotToLua]
    public virtual void HandleEnemyDead(Character target, Character attacker)
    {
        if (funHandleEnemyDead != null)
        {
            funHandleEnemyDead.call(table, target, attacker);
        }
    }

    // 友方英雄掉血执行
    [DoNotToLua]
    public virtual void HandleFriendHpChange(Character target)
    {
        if (funHandleFriendHpChange != null)
        {
            funHandleFriendHpChange.call(table, target);
        }
    }

    // 友方触发大招执行
    [DoNotToLua]
    public virtual void HandleFriendCastBigSkill(Character target)
    {
        if (funHandleFriendCastBigSkill != null)
        {
            funHandleFriendCastBigSkill.call(table, target);
        }
    }

    [DoNotToLua]
    public virtual void HandleFriendPosChange(Character target)
    {
        if (funHandleFriendPosChange != null)
        {
            funHandleFriendPosChange.call(table, target);
        }
    }

    // 被攻击时闪避
    [DoNotToLua]
    public virtual bool HandleDodge(Character attacker)
    {
        if (funHandleDodge != null)
        {
            return (bool)funHandleDodge.call(table, attacker);
        }
        return false;
    }

    [DoNotToLua]
    public virtual void Update()
    {
        if (funUpdate != null)
        {
            funUpdate.call(table);
        }
    }

    public int CalcDamage(Character attacker, int damage)
    {
		return (int)(damage + BaseValue + Grow);
    }

    public void PlaySelfEffect()
    {
        if (effobj != null)
            return;

        effobj = owner.PlayEffect(Effect, -1f);

        //if (effobj != null && data["Bind"].ToString().Equals("null") == false)
        //{
        //    string bind = (string)data["Bind"];
        //    GameObject bone = Helper.FindObject(self.gameObject, bind);
        //    if (bone != null)
        //        effobj.obj.transform.position = new Vector3(self.position.x, bone.transform.position.y, self.position.z);
        //}
    }

    public void StopSelfEffect()
    {
        if (effobj != null)
        {
            owner.DestroyEff(effobj);
            effobj = null;
        }
    }

    private IEnumerator OnDestroyEff(GameObject e, float time)
    {
        yield return new WaitForSeconds(time);
        EffectMgr.RemoveEff(e);
        GameObject.Destroy(e);
    }

    private void DestroyEff(GameObject effObj, float time)
    {
        Fight.Inst.StartCoroutine(OnDestroyEff(effObj, time));
    }

    public GameObject PlayEffectSync(string eff, float destroyTime)
    {
        GameObject obj = null;
        if (!string.IsNullOrEmpty(eff) && eff != "null")
        {
            try
            {
                obj = EffectMgr.Load(eff);
                Effect e = DataMgr.GetEffect(eff);
                if (e != null)
                {
                    if (e.time > 0f)
                        DestroyEff(obj, e.time);
                }
                else
                {
                    if (destroyTime > 0)
                        DestroyEff(obj, destroyTime);
                    else
                        DestroyEff(obj, 5f);
                }
            }
            catch (UnityException e)
            {
                Debug.LogException(e);
            }
        }
        return obj;
    }

    public GameObject PlayEffect(string eff, float destroyTime)
    {
        GameObject obj = null;
        if (!string.IsNullOrEmpty(eff) && eff != "null")
        {
            try
            {
                obj = EffectMgr.LoadAsyn(eff, owner);
                Effect e = DataMgr.GetEffect(eff);
                if (e != null)
                {
                    if (e.time > 0f)
                        DestroyEff(obj, e.time);
                }
                else
                {
                    if (destroyTime > 0)
                        DestroyEff(obj, destroyTime);
                    else
                        DestroyEff(obj, 5f);
                }
            }
            catch (UnityException e)
            {
                Debug.LogException(e);
            }
        }
        return obj;
    }

    public int Num
    {
        get { return buffdata.Num; }
    }

    public float BaseValue
    {
        get { return float.Parse(buffdata.Value); }
    }

    public float Grow
    {
        get { return lv * float.Parse(buffdata.Grow); }
    }

    public float TotalValue
    {
        get { return BaseValue + Grow; }
    }

    public float Percent
    {
        get { return buffdata.Percent + lv * buffdata.PerGrow; }
    }

    public string Anim
    {
        get { return buffdata.Anim; }
    }

    public string[] Anims
    {
        get
        {
            if (buffdata.Anim[0] == '[')
            {
                JsonData jsd = JsonMapper.ToObject(buffdata.Anim);
                string[] anims = new string[jsd.Count];
                for (int i = 0; i < jsd.Count; ++i)
                {
                    anims[i] = (string)jsd[i];
                }
				return anims;
            }

            return new string[1] {buffdata.Anim};
        }
    }

    public string Effect
    {
        get { return buffdata.Eff; }
    }

    public string HitEffect
    {
        get { return buffdata.HitEff; }
    }

    public string Sound
    {
        get { return buffdata.Sound; }
    }

    public int Attri
    {
        get { return buffdata.AttriType - 1; }
    }

    public int Tskill
    {
        get
        {
            int ret = -1;
            if (int.TryParse(buffdata.Tskill, out ret) == false)
				return -1;
            return ret;
        }
    }

    enum RangeType
    {
        Self = 0,
        AOE_5 = 1,
        AOE_9 = 2,
        AOE_4 = 3,
        AOE_8 = 4,
        LeftRight_3 = 5,
        RightAll = 6,
        EnemyAll = 7,
        EnemyFirst = 8,
        FriendAll = 9,
        BackRow = 10,
        Forward3 = 11,
    };

    public List<Character> FindTargets()
    {
        List<Character> ret;
		int range = buffdata.Range;
        switch ((RangeType)range)
        {
            case RangeType.Self:
                {
                    ret = new List<Character>();
                    ret.Add(owner);
                }break;
            case RangeType.AOE_5:
                {
                    ret = Fight.Inst.FindAOE_5Targets(owner.camp, owner.slot);
                } break;
            case RangeType.AOE_9:
                {
                    ret = Fight.Inst.FindAOE_9Targets(owner.camp, owner.slot);
                } break;
            case RangeType.AOE_4:
                {
                    ret = Fight.Inst.FindAOE_4Targets(owner.camp, owner.slot);
                } break;
            case RangeType.AOE_8:
                {
                    ret = Fight.Inst.FindAOE_8Targets(owner.camp, owner.slot);
                } break;
            case RangeType.LeftRight_3:
                {
                    ret = Fight.Inst.FindLeftRight_3Targets(owner.camp, owner.slot);
                } break;
            case RangeType.RightAll:
                {
                    ret = Fight.Inst.FindRowTargets(owner, 5);
                } break;
            case RangeType.EnemyAll:
                {
                    ret = Fight.Inst.FindAllAttackTarget(owner);
                } break;
            case RangeType.EnemyFirst:
                {
                    ret = new List<Character>();
                    Character enemy = Fight.Inst.FindAttackTarget(owner);
                    if (enemy != null)
                    {
                        ret.Add(enemy);
                    }
                } break;
            case RangeType.FriendAll:
                {
                    ret = Fight.Inst.FindAllFriend(owner);
                } break;
            case RangeType.BackRow:
                {
                    ret = Fight.Inst.FindBackRowTargets(owner);
                } break;
            case RangeType.Forward3:
                {
                    ret = Fight.Inst.FindForward3Target(owner);
                } break;
            default:
                {
                    ret = new List<Character>();
                }break;
        }
        return ret;
    }

    public List<Character> FindAllAttackTarget(Character atk)
    {
        return Fight.Inst.FindAllAttackTarget(atk);
    }

    public void ChangeAttri(Character target)
    {
		float scale = buffdata.Scale;
        if (scale.Equals(0f) == false)
        {
            iTween.ScaleTo(target.gameObject, new Vector3(scale, scale, scale), 0.4f);
        }

        JsonData valueJson = null;
        JsonData growJson = null;

        try
        {
            valueJson = JsonMapper.ToObject(buffdata.Value);
            growJson = JsonMapper.ToObject(buffdata.Grow);
        }
        catch (System.Exception)
        {
#if UNITY_EDITOR
            MessageBox.Show("buff参数错误", "buff(" + id + ") " + "Value和Grow参数错误");
            Debug.Log("buff(" + id + ") " + "Value和Grow参数错误");
#endif
            return;
        }

        if (valueJson.IsArray && growJson.IsArray)
        {
#if UNITY_EDITOR
            if (valueJson.Count != growJson.Count)
            {
                MessageBox.Show("buff参数错误", "buff(" + id + ") " + "Value和Grow数量不匹配");
                End();
                return;
            }
#endif
            for (int i = 0; i < valueJson.Count; ++i)
            {
#if UNITY_EDITOR
                if (valueJson[i].Count != 2)
                {
                    MessageBox.Show("buff参数错误", "buff(" + id + ") " + "Value参数错误");
                    End();
                    return;
                }
#endif
                int v = (int)((float)valueJson[i][1] + (float)growJson[i] * (float)lv);

				AttriType attrType = (AttriType)((int)valueJson[i][0]);
				switch ((AttriType)attrType)
                {
                    case AttriType.ATK:
                        {
                            target.ATK = target.ATK + v;
                        } break;
                    case AttriType.HP:
                        {
                            float per = (float)target.HP / (float)target.MaxHp;
                            target.MaxHp = target.MaxHp + v;
                            target._SetHp((int)(target.MaxHp * per));
                        } break;
                    case AttriType.AS:
                        {
                            target.AS = target.AS + v;
                        } break;
                    case AttriType.DEF:
                        {
                            target.DEF = target.DEF + v;
                        } break;
                    case AttriType.Anger:
                        {
                            target.Anger = target.Anger + v;
                            if (target.camp == CampType.Friend)
                            {
                                Fight.Inst.UpdateFriendAnger(target);
                            }
                        } break;
                    case AttriType.AngerGrow:
                        {
                            target.AngerGrow = target.AngerGrow + v;
                        } break;
                    case AttriType.CRI:
                        {
                            target.CRI = target.CRI + v;
                        } break;
                    case AttriType.AVD:
                        {
                            target.AVD = target.AVD + v;
                        } break;
                    case AttriType.CRIR:
                        {
                            target.CRIRate = target.CRIRate + v;
                        } break;
#if UNITY_EDITOR
                    default:
						{
                            MessageBox.Show("buff参数错误", "buff(" + id + ") " + "Value参数错误");
                            End();
                            return;

                        }
#endif
                }
            }
        }
    }

    public void RestoreChangeAttri(Character target)
    {
        float scale = buffdata.Scale;
        if (scale.Equals(0f) == false)
        {
            iTween.ScaleTo(target.gameObject, owner.SrcScale, 0.4f);
        }

        JsonData valueJson = null;
        JsonData growJson = null;
        try
        {
            valueJson = JsonMapper.ToObject(buffdata.Value);
            growJson = JsonMapper.ToObject(buffdata.Grow);
        }
        catch (System.Exception)
        {
#if UNITY_EDITOR
            MessageBox.Show("buff参数错误", "buff(" + id + ") " + "Value和Grow参数错误");
            Debug.Log("buff(" + id + ") " + "Value和Grow参数错误");
#endif
            return;
        }

        if (valueJson.IsArray && growJson.IsArray)
        {
            for (int i = 0; i < valueJson.Count; ++i)
            {
                int v = (int)((float)valueJson[i][1] + (float)growJson[i] * (float)lv);
				AttriType attrType = (AttriType)((int)valueJson[i][0]);
				switch (attrType)
                {
                    case AttriType.ATK:
                        {
                            target.ATK = target.ATK - v;
                        } break;
                    case AttriType.HP:
                        {
							float hpPer = (float)target.HP / (float)target.MaxHp;
							target.MaxHp = target.MaxHp - v;
							target._SetHp((int)(target.MaxHp * hpPer));
                        } break;
                    case AttriType.AS:
                        {
                            target.AS = target.AS - v;
                        } break;
                    case AttriType.DEF:
                        {
                            target.DEF = target.DEF - v;
                        } break;
                    case AttriType.AngerGrow:
                        {
                            target.AngerGrow = target.AngerGrow - v;
                        } break;
                    case AttriType.CRI:
                        {
                            target.CRI = target.CRI - v;
						} break;
                    case AttriType.AVD:
                        {
                            target.AVD = target.AVD - v;
                        } break;
                    case AttriType.CRIR:
                        {
                            target.CRIRate = target.CRIRate - v;
                        } break;
                    default:
                        {
                        } break;
                }
            }

        }
    }

    public enum DamageType
    {
        PercentHPDam = 1,
        Real = 2,
        HuanXue = 3,
        PercentHPTrigger = 4,
    }

    public int AddDamage(Character attacker, int damage)
    {
        float dam = damage;
        JsonData valueJson = null;
        JsonData growJson = null;
        try
        {
            valueJson = JsonMapper.ToObject(buffdata.Value);
            growJson = JsonMapper.ToObject(buffdata.Grow);
        }
        catch (System.Exception)
        {
#if UNITY_EDITOR
            MessageBox.Show("buff参数错误", "buff(" + id + ") " + "Value和Grow参数错误");
            Debug.Log("buff(" + id + ") " + "Value和Grow参数错误");
#endif
            return damage;
        }
        if (valueJson.IsArray && growJson.IsArray)
        {
            dam = 0f;
            for (int i = 0; i < valueJson.Count; ++i)
            {
                int t = (int)valueJson[i][0];
                float v = (float)valueJson[i][1];
                v = v + (float)growJson[i] * lv;

                switch ((DamageType)t)
                {
                    case DamageType.PercentHPDam:
                        {
                            dam += damage * (v - (float)owner.HP / (float)owner.MaxHp);
                        } break;
                    case DamageType.Real:
                        {
                            dam += damage + v;
                        } break;
                    case DamageType.HuanXue:
                        {
                            if (owner.isMonster)
                            {
                                if (owner.monsterType == Monster.MonsterType.Boss)
                                {
                                    dam += damage;
                                }
                                else
                                {
                                    dam += damage + v * owner.HP;
                                    float val = v * attacker.HP;
                                    attacker.BeRevHit(owner, (int)val, HitEffect);
                                }
                            }
                            else
                            {
                                dam += damage + v * owner.HP;
                                float val = v * attacker.HP;
                                attacker.BeRevHit(owner, (int)val, HitEffect);
                            }
                        } break;
                    case DamageType.PercentHPTrigger:
                        {
                            dam += damage;
                            float per = (float)owner.HP / (float)owner.MaxHp;
                            if (per <= Percent/100f)
                                dam += v;
                        } break;
                }
            }
        }
        return (int)dam;
    }

    public void AddTbuff(Character target)
    {
        string strBuff = buffdata.Tbuff;
        if (strBuff.Equals("null") == false)
        {
            if (strBuff[0] == '[')
            {
                JsonData jsd = JsonMapper.ToObject(strBuff);
                for (int i = 0; i < jsd.Count; ++i)
                {
                    int buffId = (int)jsd[i];
                    target.AddBuff(owner, buffId, lv);
                }
            }
            else
            {
                int buffId = int.Parse(strBuff);
                target.AddBuff(owner, buffId, lv);
            }
        }
    }

    public void RemoveTbuff(Character target)
    {
        string strBuff = buffdata.Tbuff;
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

    public NfSkill DoTskill(Character chara, Character target)
    {
        int id = Tskill;
        if (id == -1)
            return null;
        chara.target = target;
        return chara.DoSkill(id, lv);
    }

    public NfSkill DoTskill()
    {
        int id = Tskill;
        if (id == -1)
            return null;
        return owner.DoSkill(id, lv);
    }

    public void PushTskill()
    {
        int id = Tskill;
        if (id == -1)
            return;
        owner.PushSkill(id, lv);
    }

    public static void ShowBuff(CampType camp, int id, int lv, int attrType)
    {
        if (attrType == -1)
            return;

        List<Buff> showBuffs = myShowBuffs;
        if (camp == CampType.Enemy)
        {
            showBuffs = enemyShowBuffs;
        }

        showBuffs.RemoveAll(r => r.id == id);
        Buff b = new Buff();
        b.id = id;
        b.lv = lv;
        b.attrType = attrType;
        showBuffs.Add(b);

        Fight.Inst.m_UiInGameScript.InitBuff();
    }

    public static void ClearAllShowBuff()
    {
        myShowBuffs.Clear();
        enemyShowBuffs.Clear();
    }
}

public class NfShieldBuff : NfBuff
{
    int count = 0;
    int shield = 0;

    protected override bool Init()
    {
        count = Num;

        PlaySelfEffect();
        shield = (int)TotalValue;

        if (Anim != "null")
            owner.idleAnim = Anim;

        return true;
    }

    protected override void OnEnd()
    {
        if (Anim != "null")
            owner.idleAnim = "idle";
        owner.Idle();

        base.OnEnd();
    }

    // 被攻击时执行
    public override int HandleBeHit(Character attacker, int damage, int addAnger)
    {
        if (damage == 0)
            return damage;

        owner.PlaySound(Sound);
        owner.PlayEffect(HitEffect, 1f);
        if (shield <= damage)
        {
            OnEnd();
            return damage - shield;
        }

        shield -= damage;
        owner.ShowXiShou();

        string currAnim = owner.CurrAnim;
        if (currAnim == "idle" || currAnim == "hit")
        {
            owner.ForcePlayAnim("hit");
        }

        --count;
        if (count <= 0)
            OnEnd();

        return 0;
    }
}

public class NfDamShieldBuff : NfBuff
{
    int count = 0;

    protected override bool Init()
    {
        count = Num;

        PlaySelfEffect();

        return true;
    }

    // 被攻击时执行
    public override int HandleBeHit(Character attacker, int damage, int addAnger)
    {
        if (owner.IsDizzy || damage == 0)
            return damage;

        owner.PlaySound(Sound);
        int rdam = (int)(TotalValue + damage * Percent);
        attacker.BeRevHit(owner, rdam, HitEffect, "hit", HpLabelContainer.HitType.FanShang);
        return damage;
    }

    public override void HandleHuiHe()
    {
        if (--count < 0)
        {
            OnEnd();
        }
    }
}

public class NfRevDamShieldBuff : NfBuff
{
    int count = 0;

    protected override bool Init()
    {
        count = Num;

        PlaySelfEffect();

        return true;
    }

    // 被攻击时执行
    public override int HandleBeHit(Character attacker, int damage, int addAnger)
    {
        owner.PlayEffect(HitEffect, 3f);
        owner.PlaySound(Sound);

		if (damage == 0)
			return damage;

        owner.ShowXiShou();

        int rdam = (int)(TotalValue + damage * Percent);
        attacker.BeRevHit(owner, rdam, HitEffect, "hit", HpLabelContainer.HitType.FanShang);

        --count;
        if (count <= 0)
            OnEnd();
        return 0;
    }
}

public class NfDizzyBuff : NfBuff
{
    protected int num = 0;

    protected override bool Init()
    {
        if (owner.IsDead)
            return false;
        if (owner.IsAntiDizzy)
        {
            owner.ShowAnti();
            return false;
        }
        float per = Percent;
        if (Fight.Rand(0, 101) > per)
            return false;

        num = Num;

        //PlaySelfEffect();
        effobj = owner.PlayEffect(Effect, -1f, true);

        owner.EnableDizzy();

        return true;
    }

    public override void HandleHuiHe()
    {
        if (--num < 0)
        {
            End();
        }
    }

    protected override void OnEnd()
    {
        owner.DisableDizzy();
		base.OnEnd ();
    }
}

public class NfDizzy2Buff : NfDizzyBuff
{
    public override void HandleHuiHe()
    {
        if (--num < 0)
		{
			End();
        }
    }
}

public class NfDizzy3Buff : NfBuff
{
    float xAng;
    bool rotate;
    float time;

    protected override bool Init()
    {
        if (owner.IsDead)
			return false;
		if (owner.IsAntiDizzy)
			return false;
        float per = Percent;
        if (Fight.Rand(0, 101) > per)
            return false;

		xAng = 0f;
		time = 0f;
        rotate = true;
        return true;
    }

    public override void Update()
    {
        if (rotate == false)
            return;

        time += Time.deltaTime;
        if (time < BaseValue)
        {
            xAng += Time.deltaTime * 1500;
            owner.rotation = Quaternion.Euler(new Vector3(0, xAng, 0));
        }
        else
        {
            rotate = false;
            PlaySelfEffect();
            //self.transform.rotation = self.SrcRotation;
            owner.rotateBack();
			End();
        }
    }
}

public class NfRotBuff : NfBuff
{
    float xAng;
    bool rotate;
    float time;

    protected override bool Init()
    {
        if (owner.IsDead)
            return false;

        xAng = 0f;
        time = 0f;
        rotate = true;
        return true;
    }

    public override void Update()
    {
        if (rotate == false)
            return;

        time += Time.deltaTime;
        if (time < BaseValue)
        {
            xAng += Time.deltaTime * 1500;
            owner.rotation = Quaternion.Euler(new Vector3(0, xAng, 0));
        }
        else
        {
            rotate = false;
            PlaySelfEffect();
            //self.transform.rotation = self.SrcRotation;
            owner.rotateBack();
			End();
        }
    }
}

public class NfDizzy4Buff : NfBuff
{
    bool jump;
    float time;
    float srcY;

    protected override bool Init()
    {
        if (owner.IsDead)
			return false;
		if (owner.IsAntiDizzy)
			return false;
        float per = Percent;
        if (Fight.Rand(0, 101) > per)
            return false;

		time = 0f;
        jump = true;
        srcY = owner.position.y;
        return true;
    }

    public override void Update()
    {
        if (jump == false)
            return;

        time += Time.deltaTime;
        //S=Vot+1/2（at²)
        float h = srcY + BaseValue * time + 0.5f * (-Grow) * time * time;
        if (h <= srcY)
        {
            owner.position = new Vector3(owner.position.x, srcY, owner.position.z);
            jump = false;
			End();
        }
        else
        {
            owner.position = new Vector3(owner.position.x, h, owner.position.z);
        }
    }
}

//public class NfHpTiggerBuff : NfBuff
//{
//    protected override bool Init()
//    {
//        PlaySelfEffect();

//        return true;
//    }

//    protected override void OnEnd()
//    {
//        base.OnEnd();
//    }

//    // 被攻击时执行
//    public override int HandleBeHit(Character attacker, int damage, int addAnger)
//    {
//        float percent = BaseValue;
//        int hp = owner.HP - damage;
//        if (hp < owner.MaxHp * percent)
//        {
//            PushTskill();
//            AddTbuff(owner);
//            OnEnd();
//        }

//        return damage;
//    }
//}

public class NfJiTuiBuff : NfBuff
{
    protected delegate void MoveCallback();

    protected override bool Init()
    {
        Vector3 dir = owner.position - caster.position;
        dir.Normalize();
        Vector3 pos = owner.position + dir * BaseValue;
        float moveSpeed = float.Parse(buffdata.Grow);
        owner.isMove = true;

        Move(owner, pos, moveSpeed, delegate
        {
            MoveBack(moveSpeed);
        });
        return true;
    }

    protected void MoveBack(float moveSpeed)
    {
        AddEvent(buffdata.Percent, delegate
        {
            owner.MoveTo(owner.SrcPos, "", buffdata.PerGrow, 0f, delegate
            {
                End();
            });
        });
    }

    protected void Move(Character chara, Vector3 destPos, float moveSpeed, MoveCallback callback)
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
}

public class NfBeHitBuff : NfBuff
{
    protected override bool Init()
    {
        return true;
    }

    public override int HandleBeHit(Character attacker, int damage, int addAnger)
    {
		if (owner.isFear == false || owner.IsDizzy == false)
        	AddTbuff(owner);
        return base.HandleBeHit(attacker, damage, addAnger);
    }
}

