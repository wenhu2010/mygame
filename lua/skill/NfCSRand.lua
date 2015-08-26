require 'NfCSBase'

local NfCSRand = NfCSBase:New{
}

function NfCSRand.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSRand.Fire( self )
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
            skill:CalcDamage(attacker, target, skill:GetDamage(attacker), t, skill.KeepEffect)
        end
    end

    local lastTime = time
    for i=0, attackers.Count-1 do
        local attacker = attackers[i]

        if targets.Count > 0 then
            local idx = Fight.Rand(0, targets.Count)
            local target = targets[idx]
            local t = i*skill.HitTime
            time = time + t
            skill:CalcDamage(attacker, target, skill:GetDamage(attacker), lastTime + t, skill.HitEffect)
        end
    end

    skill:End(time + skill.HitTime)
end

return NfCSRand