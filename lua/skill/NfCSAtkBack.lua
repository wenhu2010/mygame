require 'NfCSBase'

local NfCSAtkBack = NfCSBase:New{
}

function NfCSAtkBack.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSAtkBack.Fire( self )
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local target = skill:GetLastAttackTarget()
    local targets = skill:FindAllAttackTarget(attacker)
    local destPos = target.position

    for i=1,skill.AtkC do
        skill:CalcCircalDamage(attacker, targets, destPos, skill:GetComboDamage(attackers), (i-1) * skill.HitTime)
    end

    local fireEffObj = skill:PlayEffect(skill.FireEffect, 3)
    if fireEffObj ~= null then
        fireEffObj.position = destPos
    end

    skill:End(skill.HitTime * skill.AtkC)
end

return NfCSAtkBack