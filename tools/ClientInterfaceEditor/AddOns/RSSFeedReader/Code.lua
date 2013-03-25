-- Author      : Gabor_Ratky
-- Create Date : 11/1/2007 2:00:58 PM

function RssFeedReader_OnCommand(arg)
	if arg == "show" then
		Frame1:Show();
	end
	
	if arg == "hide" then
		Frame1:Hide();
	end
end

SLASH_RSSFEEDREADER1 = "/rss";
SlashCmdList["RSSFEEDREADER"] = RssFeedReader_OnCommand;
