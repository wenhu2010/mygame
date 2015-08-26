require 'NfBuffBase'

local NfBLhpHatk = NfBuffBase:New{
}

function NfBLhpHatk.OnBegin(self)
    local buff = self.buff
    buff:PlaySelfEffect()
    return true
end

function NfBLhpHatk.HandleBeHit(self, atk, damage, addAnger)
    local owner = self.owner
    local hp = owner.HP - damage
    local atk = (1 - hp/owner.MaxHp) * buff.Value
    owner.ATK = owner.ATK + atk
    return damage
end

return NfBLhpHatk