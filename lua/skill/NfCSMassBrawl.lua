require 'NfCSBase'

local NfCSMassBrawl = NfCSBase:New{
    targets
}

function NfCSMassBrawl.onBegin(self)
    local skill = self.skill
    local target = skill:GetFirstAttackTarget()
    local targets = skill:FindAllAttackTarget(self.attacker)
    self.targets = targets

    self.attackers = skill:GetComboCharList()    

    skill:HideScene() 

    self.PlayCombAnimAndEffect(self, target.slot)

    skill:StartCameraAnim()
    skill:HideAllFriend(self.attackers)

    ShowChars(targets, false)
    target.visible = true

    skill:AddEvent(skill.SingTime, function()
        self.Fire(self, target)
    end)
end

function NfCSMassBrawl.OnStopCameraAnim(self)
    self.skill:ShowScene()
    self.skill:ShowAllFriend()
    self:MoveSrc()
    self:IdleAll()

    ShowChars(self.targets, true)
end

function NfCSMassBrawl.PlayCombAnimAndEffect(self, slot)
    local skill = self.skill
    local attackers = self.attackers
    local pos = Fight.Inst:GetSlotPos(RevCamp(self.attacker.camp), slot)

    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c.position = pos
        c.rotation = c.SrcRotation
        c:PlayAnim("heji")
    end

    skill:PlayEffect(self.attacker, skill.SingEffect, -1)
end

function NfCSMassBrawl.Fire( self, target )
    local skill = self.skill
    local attacker = self.attacker
    local attackers = self.attackers
    local dam = skill:GetComboDamage(attackers)

    skill:PlayEffect(self.attacker, skill.FireEffect, -1)

    for n=0,skill.AtkC-1 do
        skill:CalcDamage(attacker, target, dam, skill.HitTime*n)
    end

    skill:End(skill.TotalTime)
end

return NfCSMassBrawl