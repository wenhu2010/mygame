require 'NfBuffBase'

local NfBLinkHit = NfBuffBase:New{
}

function NfBLinkHit.OnBegin(self)
    return true
end

function NfBLinkHit.HandleHitHp(self, target, damage, addAnger)
    local buff = self.buff
    local owner = self.owner
    local targets = buff:FindAllAttackTarget(owner)

    local num = buff.Num
    if targets.Count > 0 and num then
        local hitTargets = {}
        for i=1,num do
            local t = GetRandTarget(targets)
            hitTargets[i] = t
            targets:Remove(t)

            if targets.Count == 0 then
                break
            end
        end

        local eff = buff.Effect
        local hitEff = buff.HitEffect
        local beginPos = Vector3(target.position.x, target.position.y + 0.6, target.position.z)
        local dam = damage * buff.Percent

        for i=1,#hitTargets do
            local t = hitTargets[i]
            local obj = buff:PlayEffectSync(eff, 0.5)
            local bo = Helper.FindObject(obj, "Begin_01")
            local eo = Helper.FindObject(obj, "End_01")
            bo.transform.position = beginPos
            eo.transform.position = Vector3(t.position.x, t.position.y + 0.6, t.position.z)
            t:BeHit(nil, dam, hitEff, false, "hit", addAnger, buff.Sound)
        end
    end
end

return NfBLinkHit