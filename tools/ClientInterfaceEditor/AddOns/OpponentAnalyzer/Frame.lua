-- Author      : Gabor_Ratky
-- Create Date : 10/25/2007 7:18:26 PM

-- Initialize global variables used to track status
lastHostileDeath = 0;
killTimeout = 15;
killCount = 0;

-- Initialize kill log
killLog = {};

function DemoFrame_OnLoad()
	-- Register for the events we are interested in
	this:RegisterEvent("PLAYER_TARGET_CHANGED");
	this:RegisterEvent("CHAT_MSG_COMBAT_HOSTILE_DEATH")
end

function DemoFrame_OnEvent()
	-- Dispatch events to functions
	if (event == "PLAYER_TARGET_CHANGED") then
		DemoFrame_PlayerTargetChanged();
	elseif (event == "CHAT_MSG_COMBAT_HOSTILE_DEATH") then
		DemoFrame_HostileDeath();
	end
end

function DemoFrame_PlayerTargetChanged()
   	-- Make sure we have an existing target
	if (UnitExists("target") ~= 1) then return;
	end

	-- Get the level of the player and its target
	local playerLevel = UnitLevel("player");
	local targetLevel = UnitLevel("target");

	-- TODO: Set the caption to the name of the unit
	local name = UnitName("target");
	
	DemoCaption:SetText(name);

	-- TODO: Pick the appropriate texture based on the unit levels
	local textureName;
	
	if (playerLevel < targetLevel) then 
		textureName = "pumpkin"
	elseif (playerLevel == targetLevel) then
		textureName = "even"
	else
		textureName = "squirrel"
	end

	-- TODO: Set the image
	DemoImage:SetTexture("Interface\\AddOns\\OpponentAnalyzer\\" .. textureName);
end

function DemoFrame_HostileDeath()
	-- Match the received chat message against the 'slain' message regex
	local name = strmatch(arg1, "^You have slain (.+)!$")
	
	if (name) then
		-- Get the current time and the name of the dead
		local time = GetTime();
	
		-- See if less time has passed since the last death than the timeout
		if (time - lastHostileDeath < killTimeout) then
			-- Increment the kill count
			killCount = killCount + 1;
		else
			-- Reset the kill count back to one
			killCount = 1;
		end

		-- Save the name of the dead and the time of the kill
		killLog[#(killLog) + 1] = "Killed " .. name .. " at " .. date("%m/%d/%y %H:%M:%S");

		-- Store the time of the last death
		lastHostileDeath = time;
		
		-- Depending on the number of kills, play a sound file
		if (killCount == 2) then
			Image:SetTexture("Interface\\AddOns\\OpponentAnalyzer\\doublekill");
			PlaySoundFile("Interface\\AddOns\\OpponentAnalyzer\\Sounds\\doublekill.mp3");
		elseif (killCount == 3) then
			Image:SetTexture("Interface\\AddOns\\OpponentAnalyzer\\triplekill");
			PlaySoundFile("Interface\\AddOns\\OpponentAnalyzer\\Sounds\\killimanjaro.mp3");
		end
	end
end