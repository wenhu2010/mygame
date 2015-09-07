local NfCSCDir = require 'NfCSCDir'

local NfCSLvLong = NfCSCDir:New{
}

function NfCSLvLong.OnStopCameraAnim(self)
    NfCSBase.OnStopCameraAnim(self)
    self.skill:DestroyAllEff()
end

return NfCSLvLong