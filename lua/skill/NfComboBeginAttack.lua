require 'NfSkillBase'

local NfComboBeginAttack = NfSkillBase:New{
    target,
    targets,
    keepEff
}

function NfComboBeginAttack.onBegin(self)

    local skill = self.skill

    self.target = skill:GetFirstAttackTarget()
    self.attacker.target = self.target

    local attacker = self.attacker

    skill:StartCameraAnim()

    local singTime = 0
    local singAnim = skill.SingAnim
    if singAnim ~= "null" then
        skill:PlaySingAnimAndEffect()
        singTime = attacker:GetAnimLength(singAnim)
    end

    skill:AddEvent(singTime, function( )
        NfComboBeginAttack.Fire(self)
    end)
end

function NfComboBeginAttack.CanFanJi(self)
    return true
end

function NfComboBeginAttack.Fire(self)
    local skill = self.skill
    local attacker = self.attacker
    local target = self.target

    self.keepEff = skill:PlayEffect(attacker, skill.KeepEffect, -1)
    self.targets = skill:FindTargets(true)
    
    local dest = target.position + target.direction * NfSkill.CalcRadius(attacker, target)

    skill:Move(attacker, dest, skill.BulletSpeed, skill.KeepAnim, attacker.moveAnimSpeed, function()
        self.OnAttack(self, dest)
    end)
end

function NfComboBeginAttack.OnAttack(self, dest)
    local skill = self.skill
    local attacker = self.attacker
    local target = self.target

    skill:DestroyEff(self.keepEff)

    attacker.position = dest
    attacker:LookAt(target.position)

    -- 第一下攻击
    local hitTime = skill.HitTime
    local animLen = attacker:GetAnimLength(skill.FireAnim)
    skill:PlayFireAnimAndEffect()

    local breakAtk = false
    for i=0,target.Buffs.Count-1 do
        local buff = target.Buffs[i]
        if buff.enabled and buff:HandleBeHitPre(attacker) then
            breakAtk = true
            break
        end
    end

    if breakAtk then
        skill:End(animLen)
    else
        -- 目标伤害计算
        skill:CalcDamage(skill.Damage, self.targets, hitTime)

        skill:AddEvent(hitTime, function()
            skill:AddBuff(attacker)
        end)

        skill:AddEvent(animLen, function()  
            -- 第二下攻击
            local tskill = skill:DoTskill()
            if tskill ~= nil then
                skill:End()
            else
                skill:MoveSrcPos(attacker)
            end
        end)
    end
end

function NfComboBeginAttack.onEnd(self)
	self.skill:DestroyEff(self.keepEff)
	self.skill:DestroyAll()
end

return NfComboBeginAttack