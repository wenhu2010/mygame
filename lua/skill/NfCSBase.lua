require 'NfSkillBase'

NfCSBase = NfSkillBase:New{
    attackers
}

function NfCSBase.onBegin(self, slot)
    local skill = self.skill

    self.attackers = skill:GetComboCharList()    

    skill:HideScene() 

    self.PlayCombAnimAndEffect(self, slot)

    skill:StartCameraAnim()
    skill:HideAllFriend(self.attackers)
end

function NfCSBase.PlayCombAnimAndEffect(self, slot)
    local skill = self.skill
    local attackers = self.attackers
    local pos = Fight.Inst:GetSlotPos(self.attacker.camp, slot)

    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c.position = pos
        c.rotation = c.SrcRotation
        c:PlayAnim("heji")
    end
    skill:PlayEffect(self.attacker, skill.SingEffect, -1)
end

function NfCSBase.OnStopCameraAnim(self)
    self.skill:ShowScene()
    self.skill:ShowAllFriend()
    self.MoveSrc(self)
end

function NfCSBase.onEnd(self)
    self.skill:ShowScene()
    self.skill:ShowAllFriend()
    self.MoveSrc(self)
    self.skill:DestroyAllEff()
end

function NfCSBase.MoveSrc(self)
    local attackers = self.attackers
    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c.position = c.SrcPos
        --c:Idle()
    end
end

function NfCSBase.IdleAll(self)
    local attackers = self.attackers

    for i=0, attackers.Count-1 do
        local c = attackers[i]
        c:Idle()
    end
end
