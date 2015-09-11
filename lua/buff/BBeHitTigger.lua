require 'NfBuffBase'

local BBeHitTigger = NfBuffBase:New{
    num = 0
}

function BBeHitTigger:OnBegin()
    print "BBeHitTigger"
    local buff = self.buff
    buff:PlaySelfEffect()
    self.num = buff.Num
    return true
end

function BBeHitTigger:HandleBeHit(atk, damage, addAnger)
    local buff = self.buff
    local owner = self.owner
    local targets = buff:FindTargets()
    for i=0,targets.Count-1 do
        local t = targets[i]
        buff:AddTbuff(t)
    end
    if owner.isFear == false and owner.IsDizzy == false then
        buff:DoTskill()
    end
    return damage
end

function BBeHitTigger:HandleHuiHe()
    self.num = self.num - 1
    if self.num <= 0 then
        self.buff:End()
    end
end

return BBeHitTigger