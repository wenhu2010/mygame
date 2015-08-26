﻿require 'NfCSBase'

local NfCSCBullet = NfCSBase:New{
    bulletNum
}

function NfCSCBullet.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self)
    end)
end

function NfCSCBullet.Fire( self )
    local skill = self.skill
    local attackers = self.attackers
    local dam = skill:GetComboDamage(attackers)

    local sp = { Vector3(-11, 0, 2), Vector3(-11, 0, 0), Vector3(-11, 0, -2) }
    local dp = { Vector3(20, 0, 2), Vector3(20, 0, 0), Vector3(20, 0, -2) }

    for n=0,skill.AtkC-1 do
        skill:AddEvent(skill.HitTime*n, function()            
            for i=1,#sp do
                skill:FireBullet(sp[i], dp[i], skill.BulletModel, skill.BulletSpeed, 1, dam)
            end
        end)
    end
    
    skill:End(skill.TotalTime)
end

return NfCSCBullet