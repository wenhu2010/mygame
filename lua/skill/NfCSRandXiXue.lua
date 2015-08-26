require 'NfCSBase'

local NfCSRandXiXue = NfCSBase:New{
}

function NfCSRandXiXue.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSRandXiXue.Fire( self )
    local skill = self.skill
    local attackers = self.attackers
    local targets = skill:FindAllAttackTarget(self.attacker)

    local time = 0
    for i=0, attackers.Count-1 do
        local attacker = attackers[i]

        if targets.Count > 0 then
            local idx = Fight.Rand(0, targets.Count)
            local target = targets[idx]
            local t = i*skill.HitTime
            time = time + t
            skill:AddBuff(attacker)
            skill:CalcDamage(attacker, target, skill:GetDamage(attacker), t)
        end
    end

    skill:End(time + skill.HitTime)
end

return NfCSRandXiXue