require 'NfBuffBase'
local json = require 'json'

local NfBShanDian = NfBuffBase:New{
    num = 0
}

function NfBShanDian.OnBegin(self)
    local buff = self.buff
    buff:PlaySelfEffect()
    self.num = buff.Num
    return true
end

function NfBShanDian.Fire(self)
    local buff = self.buff
    local owner = self.owner
    local targets = buff:FindAllAttackTarget(owner)

    local atkc = buff.Range
    if targets.Count > 0 and atkc > 0 then
        local hitTargets = {}
        for i=1,atkc do
            local t = GetRandTarget(targets)
            hitTargets[i] = t
            targets:Remove(t)

            if targets.Count == 0 then
                break
            end
        end

        local hitParam = json.decode(buff.HitEffect)
        local hitEff = hitParam[2]
        local beginPos = Vector3(owner.position.x, owner.position.y + 0.6, owner.position.z)
        local dam = buff.TotalValue * owner.ATK

        for i=1,#hitTargets do
            local t = hitTargets[i]
            local obj = buff:PlayEffectSync(hitParam[1], 0.5)
            local bo = Helper.FindObject(obj, "Begin_01")
            local eo = Helper.FindObject(obj, "End_01")
            bo.transform.position = beginPos
            eo.transform.position = Vector3(t.position.x, t.position.y + 0.6, t.position.z)
            buff:Hit(owner, t, dam, hitEff)
        end
    end
end

function NfBShanDian.HandleWaveAttack(self)
    local buff = self.buff
    local owner = self.owner
    self.num = self.num - 1
    self.Fire(self)
    if self.num <= 0 then
        buff:End()
    end
end

return NfBShanDian