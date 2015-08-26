require 'NfBuffBase'

local NfHpTiggerBuff = NfBuffBase:New{
}

function NfHpTiggerBuff.OnBegin(self)
    local buff = self.buff
    buff:PlaySelfEffect()
    return true
end

function NfHpTiggerBuff.HandleBeHit(self, atk, damage, addAnger)
    local buff = self.buff
    local owner = self.owner
    local percent = buff.BaseValue
    local hp = owner.HP - damage
    if hp < owner.MaxHp * percent then
        buff:PushTskill()
        buff:AddTbuff(owner)
        buff:End()
    end

    return damage
end

return NfHpTiggerBuff