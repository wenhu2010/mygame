
require 'global'

local NewShowRoles = {}
local gameObject

function NewShowRoles.Awake(obj)
	gameObject = obj
end

function NewShowRoles.Start()
	local o = Helper.FindObject(gameObject, "QianNeng")
	local lookJiban = o:GetComponent("UIButton")
	EventDelegate.Add(lookJiban.onClick, function()
		print "jiban"
		local winObj = NfUIMgr.CreateWindow("")
	end)
end

return NewShowRoles