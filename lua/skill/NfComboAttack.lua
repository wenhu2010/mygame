require 'NfSkillBase'

local NfComboAttack = NfSkillBase:New{
}

function NfComboAttack.onBegin(self)
    local skill = self.skill
    local attacker = self.attacker

    skill:StartCameraAnim()

    local singTime = 0
    local singAnim = skill.SingAnim
    if singAnim ~= "null" then
        skill:PlaySingAnimAndEffect()
        singTime = attacker:GetAnimLength(singAnim)
    end
    
    skill:AddEvent(singTime, function( )
        NfComboAttack.Fire(self)
    end)
end

function NfComboAttack.Fire(self)
    local skill = self.skill
    local attacker = self.attacker

    skill:PlayFireAnimAndEffect()

    local bullet = skill.BulletModel
    if bullet ~= "null" then
        skill:PlayEffect(attacker.target, bullet, 3)
    end

    -- 目标伤害计算
    local targets = skill:FindTargets(true)
    skill:CalcDamage(skill.Damage, targets, skill.HitTime)

    local animLen = attacker:GetAnimLength(skill.FireAnim)
    skill:AddEvent(animLen, function()
        skill:DoTskill()
        skill:End()
    end)
end

function NfComboAttack.onEnd(self)
	self.skill:DestroyAll()
end

return NfComboAttack