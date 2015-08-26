require 'global'

local NfComboAttack = {
    skill,
    attacker
}

function NfComboAttack.onBegin(self)
    local skill = self.skill
    local attacker = self.attacker

    skill:StartCameraAnim()

    startCoroutine(function()
        
        local singAnim = skill.SingAnim
        if singAnim ~= "null" then
            skill:PlaySingAnimAndEffect()
            local singTime = attacker:GetAnimLength(singAnim)
            waitTime(singTime)
        end

        skill:PlayFireAnimAndEffect()

        local bullet = skill.BulletModel
        if bullet ~= "null" then
            skill:PlayEffect(attacker.target, bullet, 3)
        end

        -- 目标伤害计算
        local targets = skill:FindTargets(true)
        skill:CalcDamage(skill.Damage, targets, skill.HitTime)

        local animLen = attacker:GetAnimLength(skill.FireAnim)
        waitTime(animLen)

        skill:DoTskill()

        skill:End()

    end)
end

function NfComboAttack.onEnd(self)
	self.skill:DestroyAll()
end

return NfComboAttack