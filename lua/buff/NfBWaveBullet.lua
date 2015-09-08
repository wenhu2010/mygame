require 'NfBuffBase'
local json = require 'json'

local NfBWaveBullet = NfBuffBase:New{
    num = 0
}

function NfBWaveBullet.OnBegin(self)
    local buff = self.buff
    buff:PlaySelfEffect()
    self.num = buff.Num
    return true
end

function NfBWaveBullet.HandleWaveAttack(self)
    self.num = self.num - 1
    self.FireBullet(self)
    if self.num <= 0 then
        self.buff:End()
    end
end

function NfBWaveBullet.FireBullet(self)
    local buff = self.buff
    local owner = self.owner
    local targets = buff:FindAllAttackTarget(owner)

    local atkc = buff.Range
    if targets.Count > 0 and atkc > 0 then
        local hitTargets = FindRandTarget(targets, atkc)
        local hitParam = json.decode(buff.HitEffect)
        local bulletModel = hitParam[1]
        local hitEff = hitParam[2]
        local fireBone = hitParam[3]
        local bulletSpeed = hitParam[4]

        local bone = Helper.FindObject(owner.gameObject, fireBone)
        local beginPos = Vector3(owner.position.x, owner.position.y + 0.6, owner.position.z)
        if bone then
            beginPos = bone.transform.position
        end

        local dam = buff.TotalValue * owner.ATK
        for i=1,#hitTargets do
            local t = hitTargets[i]
            local bobj = buff:PlayEffectSync(bulletModel, -1)
            if bobj then
                local tran = bobj.transform
                tran.position = beginPos

                buff:Move(tran, t, bulletSpeed, function()
                    buff:Hit(owner, t, dam, hitEff)
                    GameObject.Destroy(bobj)
                end)
            end
        end
    end
end

return NfBWaveBullet