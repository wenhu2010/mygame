require 'NfSkillBase'

local NfComboEndAttack = NfSkillBase:New{
}

function NfComboEndAttack.onBegin(self)
    local skill = self.skill
    local attacker = self.attacker

    skill:PlayFireAnimAndEffect()

    local hitTime = skill.HitTime
    skill:AddEvent(hitTime, function( )
        local bullet = skill.BulletModel
        if bullet ~= "null" then
            skill:PlayEffect(attacker.target, bullet, 3)
        end

        -- 目标伤害计算
        local targets = skill:FindTargets(true)
        skill:CalcDamage(skill.Damage, targets, 0)

        local animLen = attacker:GetAnimLength(skill.FireAnim)

        skill:AddEvent(animLen - hitTime, function( )
            skill:MoveSrcPos(attacker)
        end)
    end)
end

function NfComboEndAttack.onEnd(self)
	self.skill:DestroyAll()
end

return NfComboEndAttack