require 'global'

local NfComboEndAttack = {
    skill,
    attacker
}

function NfComboEndAttack.onBegin(self)
    local skill = self.skill
    local attacker = self.attacker

    startCoroutine(function()
        
        skill:PlayFireAnimAndEffect()

        local hitTime = skill.HitTime
        waitTime(hitTime)

        local bullet = skill.BulletModel
        if bullet ~= "null" then
            skill:PlayEffect(attacker.target, bullet, 3)
        end

        -- 目标伤害计算
        local targets = skill:FindTargets(true)
        skill:CalcDamage(skill.Damage, targets, hitTime)

        local animLen = attacker:GetAnimLength(skill.FireAnim)
        waitTime(animLen - hitTime)

        skill:MoveSrcPos(attacker)

    end)
end

function NfComboEndAttack.onEnd(self)
	self.skill:DestroyAll()
end

return NfComboEndAttack