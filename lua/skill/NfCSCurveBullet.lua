require 'NfCSBase'

local NfCSCurveBullet = NfCSBase:New{
    bulletNum = 0
}

function NfCSCurveBullet.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    bulletNum = skill.AtkC * self.attackers.Count

    skill:AddEvent(skill.SingTime, function()
        for i=1,skill.AtkC do
            skill:AddEvent(skill.HitTime * (i - 1), function()
                self.Fire(self)
            end)
        end
    end)
end

function NfCSCurveBullet.Fire( self )
    local skill = self.skill
    local attackers = self.attackers
    local targets = skill:FindAllAttackTarget(self.attacker)

    for i=0, attackers.Count-1 do
        local attacker = attackers[i]        
        local target = GetRandTarget(targets)
        if target ~= nil then
            skill:FireCurveBullet(attacker, target, skill:GetDamage(attacker), function()
                bulletNum = bulletNum - 1
                if bulletNum <= 0 then
                    skill:End(0.3)
                end
            end)
        end
    end
end

return NfCSCurveBullet