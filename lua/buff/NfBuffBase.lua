require 'NfSkillBase'

NfBuffBase = {
    buff,
    owner,
    caster
}

function NfBuffBase:New(o)
    o = o or {}
    setmetatable(o, self)
    self.__index = self
    return o
end