require "global"

local function tableprint(data,cs)
	if data == nil then 
		print("core.print data is nil");
	end
	local cstring = "";
	cstring = cstring..cs.."{\n";
	local space = cs.."  ";
	if(type(data)=="table") then
		for k, v in pairs(data) do
			if(type(v)=="table") then
				cstring = cstring..tableprint(v,space);
			else
				cstring = cstring..space..tostring(k).." = "..tostring(v).."\n";
			end
		end
	else
		cstring = cstring..space..tostring(data) + "\n";
	end
	cstring = cstring..cs.."}\n";
	return cstring;
end

utility = {}

function utility.print(data)
	local cstring = tableprint(data,"  ")
	print(cstring)
end
