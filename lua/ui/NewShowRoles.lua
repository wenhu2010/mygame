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
		print "test lua ui"
		-- local x = {
		-- 	uid=6018,
		-- 	lasttime=0,
		-- 	info="okjfljd"
		-- }

		-- send("", x, function(recv)
		-- 	utility.print(recv)
		-- end)
	end)
end

return NewShowRoles