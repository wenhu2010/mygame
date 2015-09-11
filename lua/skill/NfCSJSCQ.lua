require 'NfCSBase'

local NfCSJSCQ = NfCSBase:New{
}

function NfCSJSCQ.onBegin(self)
    local skill = self.skill

    NfCSBase.onBegin(self, 2)

    skill:AddEvent(skill.SingTime, function()
        self:Fire()
    end)
end

function NfCSJSCQ.Fire(self)
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local targets = skill:FindAllAttackTarget(attacker)
    local dam = skill:GetComboDamage(attackers)

    local eff = skill:PlayEffect(skill.FireEffect, 3)
    if eff then
        eff.position = GetSlotPos(attacker.camp, 0)
        eff.rotation = attacker.SrcRotation
    end

    skill:AddEvent(skill.HitTime, function()
        local pos = Fight.Inst:GetSlotPos(RevCamp(attacker.camp), 2)
        skill:FireDirBullet(GetSlotPos(attacker.camp, 0), pos, skill.BulletModel, skill.BulletSpeed, dam, 1, 0, skill.BulletRadius)
    end)

    skill:End(skill.TotalTime)
end

function NfCSJSCQ.OnStopCameraAnim(self)
    NfCSBase.OnStopCameraAnim(self)
    self.skill:DestroyAllEff()
end

function NfCSJSCQ.onEnd(self)
    self.skill:ShowScene()
    self.skill:ShowAllFriend()
    self:MoveSrc()
end

return NfCSJSCQ