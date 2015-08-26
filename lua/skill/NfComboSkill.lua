require 'global'

local NfComboSkill = {
    skill,
    attacker,
    attackers
}

function NfComboSkill.onBegin(self)
    local skill = self.skill

    self.attackers = skill:GetComboCharList()    

    skill:HideScene()    

    NfComboSkill.PlayCombAnimAndEffect(self)

    skill:StartCameraAnim()

    startCoroutine(function( )
        NfComboSkill.Attack(self)
    end)
end

function NfComboSkill.PlayCombAnimAndEffect(self)
    local skill = self.skill
    local attackers = self.attackers

    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c.position = Vector3.zero
        c.rotation = c.SrcRotation
        c:PlayAnim("heji")
    end
    skill:PlayEffect(self.attacker, skill.FireEffect, 3)
end

function NfComboSkill.OnStopCameraAnim(self)
    self.skill:ShowScene()

    NfComboSkill.MoveSrc(self)
end

function NfComboSkill.Attack( self )
    local skill = self.skill
    local attacker = self.attacker

    local time = skill.TotalTime
    if time < 0.01 then
        time = attacker:GetAnimLength("heji")
    end

    local dam = skill:GetComboDamage(self.attackers)

    local bulletModel = skill.BulletModel
    local hitTime = skill.HitTime

    skill:AddEvent(hitTime, function()
        local endTime = time - hitTime
        local targets = skill:FindTargets(true)

        for i=0, targets.Count-1 do
            local c = targets[i]
            local beginPos = attacker.position + Vector3(0,0.6,0)
            local endPos = Vector3(c.position.x, 0.6, c.position.z)
            skill:PlayChainEff(bulletModel, endTime, beginPos, endPos)
            skill:CalcDamage(attacker, c, dam, 0)
        end

        skill:End(endTime)
    end)
end

function NfComboSkill.onEnd(self)
    NfComboSkill.MoveSrc(self)

    self.skill:DestroyAllEff()
end

function NfComboSkill.MoveSrc(self)
    local attackers = self.attackers
    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c.position = c.SrcPos
        c:Idle()
    end
end

return NfComboSkill