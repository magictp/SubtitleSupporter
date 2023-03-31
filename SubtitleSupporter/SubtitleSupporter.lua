
-- v0.4 add get current subtitle function
-- v0.3 add run whisper local function
-- v0.2 add combine video function
-- v0.1 init version with run whisper api function


script_name = "SubtitleSupporter"
script_description = "call variety AI tools to support handling subtitles"
script_author = "松田好花花"
script_version = "0.4"





function runWhisperApi(subtitles)

	local videoPath = aegisub.project_properties().video_file
	if videoPath == nil or videoPath == "" then 
		aegisub.debug.out("please open a video first\n")
		return
	end

	--choose model
	local model = "whisper-1"
	local button, controls = aegisub.dialog.display({
		{class="label", label="please select a model:", x=0, y=0},
		{class="dropdown", name="model", items={"whisper-1"}, value="whisper-1", x=0, y=1}
	}, {})
	if button == false then
		--aegisub.debug.out("cancelled\n")
		return
	end

	for key, val in pairs(controls) do
		if key == "model" then
			model = val
		end
	end

	
	local isError = false
	--call api
	local fh= io.popen(".\\automation\\SubtitleSupporter\\SubtitleSupporter.exe -h WhisperApi -m "..model.." -f \""..videoPath.."\"")
	--aegisub.debug.out(model.."\n")
	for l in fh:lines() do 
		if string.sub(l,1,string.len("NO RESULT"))=="NO RESULT" then 
			aegisub.debug.out("Cannot get result from whisper")
			return
		end
		if isError or string.sub(l,1,string.len("ERROR"))=="ERROR" then 
			isError = true;
			aegisub.debug.out(l.."\n")
		else
			-- get last subtitle since I don't know how to new an subtitle object
			local line = subtitles[#subtitles]
			-- handle result string
			-- in my case [0.0 --> 2.6] 素敵なお花に惚れ惚れしてしまいます。
			local l1 = split(l, "] ")[1]
			-- aegisub.debug.out(l1.."\n")
			local t  = split(l, "] ")[2]
			-- aegisub.debug.out(t.."\n")
			local k1 = split(l1," --> ")[1]
			-- aegisub.debug.out(k1.."\n")
			local k = string.sub(k1, 2, -3)
			-- aegisub.debug.out(k.."\n")
			local v = split(l1," --> ")[2]
			-- aegisub.debug.out(v.."\n")

			-- ae time is in millsecound
			line.start_time = k * 1000
			line.end_time = v * 1000
			line.text = t

			-- append subtitle
			subtitles[0] = line
		end
	end
end


function runWhisperLocal(subtitles)

	local videoPath = aegisub.project_properties().video_file
	--aegisub.debug.out(videoPath.."\n")
	if videoPath == nil or videoPath == "" then 
		aegisub.debug.out("please open a video first\n")
		return
	end

	--choose model
	local model = "medium"
	local button, controls = aegisub.dialog.display({
		{class="label", label="please select a model:", x=0, y=0},
		{class="dropdown", name="model", items={"base","small","medium", "large"}, value="medium", x=0, y=1}
	}, {})
	if button == false then
		--aegisub.debug.out("cancelled\n")
		return
	end

	for key, val in pairs(controls) do
		if key == "model" then
			model = val
		end
	end

	
	local isError = false
	--call api
	local fh= io.popen(".\\automation\\SubtitleSupporter\\SubtitleSupporter.exe -h WhisperLocal -m "..model.." -f \""..videoPath.."\"")
	for l in fh:lines() do 
		if string.sub(l,1,string.len("NO RESULT"))=="NO RESULT" then 
			aegisub.debug.out("Cannot get result from whisper")
			return
		end
		if isError or string.sub(l,1,string.len("ERROR"))=="ERROR" then 
			isError = true;
			aegisub.debug.out(l.."\n")
		else
			-- get last subtitle since I don't know how to new an subtitle object
			local line = subtitles[#subtitles]
			-- handle result string
			-- in my case [0.0 --> 2.6] 素敵なお花に惚れ惚れしてしまいます。
			local l1 = split(l, "] ")[1]
			-- aegisub.debug.out(l1.."\n")
			local t  = split(l, "] ")[2]
			-- aegisub.debug.out(t.."\n")
			local k1 = split(l1," --> ")[1]
			-- aegisub.debug.out(k1.."\n")
			local k = string.sub(k1, 2, -3)
			-- aegisub.debug.out(k.."\n")
			local v = split(l1," --> ")[2]
			-- aegisub.debug.out(v.."\n")

			-- ae time is in millsecound
			line.start_time = k * 1000
			line.end_time = v * 1000
			line.text = t

			-- append subtitle
			subtitles[0] = line
		end
	end
end

function runCurrentSub(subtitles)
	--choose model
	local coor = "10,10,20,20"
	local confidence = "0.05"

	local videoPath = aegisub.project_properties().video_file
	if videoPath == nil or videoPath == "" then 
		aegisub.debug.out("please open a video first\n")
		return
	end


	local button, controls = aegisub.dialog.display({
		{class="label", label="please input current subtitle coordinate", x=0, y=0},
		{class="label", label="in format x1,y1,x2,y2 where x1,y1 are for top-left corner and x2,y2 are for bottom-right corner", x=0, y=1},
		{class="textbox", name="coor", value="10,10,20,20", x=0, y=2},
		{class="label", label="please input confidence between 0 and 1", x=0, y=3},
		{class="textbox", name="confidence", value="0.05", x=0, y=4}
	}, {})
	if button == false then
		--aegisub.debug.out("cancelled\n")
		return
	end

	for key, val in pairs(controls) do
		if key == "coor" then
			coor = val
		end
		if key == "confidence" then
			confidence = val
		end
	end

	
	local isError = false
	--call api
	local fh, error= io.popen(".\\automation\\SubtitleSupporter\\SubtitleSupporter.exe -h CurrentSub -f \""..videoPath.."\" -coor "..coor.." -conf "..confidence)
	
	if fh == nil then
		aegisub.debug.out(error)
		return
	end
	for l in fh:lines() do 
		--aegisub.debug.out(l.."\n")
		if string.sub(l,1,string.len("NO RESULT"))=="NO RESULT" then 
			aegisub.debug.out("Cannot get result from whisper")
			return
		end
		if isError or string.sub(l,1,string.len("ERROR"))=="ERROR" then 
			isError = true;
			aegisub.debug.out(l.."\n")
		else
			-- get last subtitle since I don't know how to new an subtitle object
			local line = subtitles[#subtitles]
			-- handle result string
			-- in my case [0.0 --> 2.6] 素敵なお花に惚れ惚れしてしまいます。
			local l1 = split(l, "] ")[1]
			-- aegisub.debug.out(l1.."\n")
			local t  = split(l, "] ")[2]
			-- aegisub.debug.out(t.."\n")
			local k1 = split(l1," --> ")[1]
			-- aegisub.debug.out(k1.."\n")
			local k = string.sub(k1, 2, -3)
			-- aegisub.debug.out(k.."\n")
			local v = split(l1," --> ")[2]
			-- aegisub.debug.out(v.."\n")

			-- ae time is in millsecound
			line.start_time = k * 1000
			line.end_time = v * 1000
			line.text = t

			-- append subtitle
			subtitles[0] = line
		end
	end
end

function runCombine()
	local videoPath = aegisub.project_properties().video_file
	if videoPath == nil or videoPath == "" then 
		aegisub.debug.out("please open a video first\n")
		return
	end

	local filePath = aegisub.dialog.open("please select an ass file","","","ass files (.ass)|*.ass", false, true)
	if filePath == nil then 
		return
	end
	-- aegisub.debug.out(videoPath.."\n")
	-- aegisub.debug.out(fileName.."\n")
	local isError = false
	local outputFilepath = ""
	--call api
	local fh= io.popen(".\\automation\\SubtitleSupporter\\SubtitleSupporter.exe -h Combine -f \""..videoPath.."\" -ass "..filePath.." -qsv false")
	for l in fh:lines() do 
	 	--aegisub.debug.out(l.."\n")
		if isError or string.sub(l,1,string.len("ERROR"))=="ERROR" then 
			isError = true;
			aegisub.debug.out(l.."\n")
		end
	end
end




--for test dialog 
function test7(subtitles, selected_lines, active_line)
	local a, b = aegisub.dialog.display({{class="label", label="Test..."}}, {})
	report_dialog_result(a, b)
	aegisub.progress.set(50)
	a, b = aegisub.dialog.display(
		{
			{class="edit", name="foo", text="", x=0, y=0},
			{class="intedit", name="e1", value=20, x=0, y=1},
			{class="intedit", name="e2", value=30, min=10, max=50, x=1, y=1},
			{class="floatedit", name="e3", value=19.95, x=0, y=2},
			{class="floatedit", name="e4", value=123.63423, min=-4.3, max=2091, x=1, y=2},
			{class="floatedit", name="e5", value=-4, step=0.21, x=2, y=2},
			{class="floatedit", name="e6", value=22, min=0, max=100, step=1.4, x=3, y=2},
			{class="textbox", name="e7", text="hmm wuzzis say?", x=0, y=3, width=4},
			{class="dropdown", name="l1", items={"abc", "def", "ghi"}, value="def", x=0, y=4},
			{class="dropdown", name="l2", items={"abc", "def", "ghi"}, value="doesnotexist", x=1, y=4},
			{class="checkbox", name="b1", value=true, label='checked', x=0, y=5},
			{class="checkbox", name="b2", value=false, label='cleared', x=1, y=5},
			{class="color", name="c1", value="#00ff11", x=0, y=6},
			{class="color", name="c2", value="&H0011ff00", x=1, y=6},
			{class="coloralpha", name="c3", value="#aabbccdd", x=0, y=7},
			{class="coloralpha", name="c4", value="&Hddccbbaa&", x=1, y=7},
			{class="alpha", name="c5", value="#12", x=0, y=8},
			{class="alpha", name="c6", value="&H12&", x=1, y=8}
		},
		{"foo", "bar"})
	report_dialog_result(a, b)
end
function report_dialog_result(button, controls)
	aegisub.debug.out("Dialog closed: ")
	if button == false then
		aegisub.debug.out("cancelled\n")
	elseif button == true then
		aegisub.debug.out("clicked Ok\n")
	else
		aegisub.debug.out("clicked '" .. button .. "'\n")
	end
	for key, val in pairs(controls) do
		local printable = (val == true and "true") or (val == false and "false") or tostring(val)
		aegisub.debug.out("%s: %s\n", key, printable)
	end
	aegisub.debug.out(" - - - - -\n")
end


function split(str, delim)
    -- Eliminate bad cases...
    if string.find(str, delim) == nil then
        return { str }
    end

    local result = {}
    local pat = "(.-)" .. delim .. "()"
    local lastPos
    for part, pos in string.gmatch(str, pat) do
        table.insert(result, part)
        lastPos = pos
    end
    table.insert(result, string.sub(str, lastPos))
    return result
end

-- aegisub.register_macro("testDialog", "testDialog", test7)
aegisub.register_macro("Get Current Subtitle", "Get current subtitle in video", runCurrentSub)
aegisub.register_macro("Run Whisper Api", " To run whisper api", runWhisperApi)
aegisub.register_macro("Combine Video with Subtitle", " To combine video with subtitle", runCombine)
-- aegisub.register_macro("Run Whisper Local", " To run whisper local", runWhisperLocal)