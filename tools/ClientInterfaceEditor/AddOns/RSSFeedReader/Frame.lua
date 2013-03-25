-- Author      : Gabor_Ratky
-- Create Date : 10/29/2007 10:39:22 AM

COMMON_FEED_LIST_FEEDS = {};
CURRENT_FEED_INDEX = 1;
CURRENT_ITEM_INDEX = 1;

function Frame1_OnLoad()
	this:RegisterEvent("ADDON_LOADED")
	this:RegisterForDrag("LeftButton");
end

function Frame1_OnEvent()
	if (event == "ADDON_LOADED") then
		Frame1_AddOnLoaded();
	end
end

function Frame1_AddOnLoaded()
	if (arg1 == "RSSFeedReader") then
		HighlightButton("ButtonFeed", 1);
		HighlightButton("ButtonItem", 1);
	
		ScrollFrameFeeds_Update();
		ScrollFrameItems_Update();
		ScrollFrameDescription_Update();
	end
end

function ScrollFrameFeeds_Update()
	local line; -- 1 through 5 of our window to scroll
	local offset; -- an index into our data calculated from the scroll offset
  
	local feedCount = GetFeedCount();
	FauxScrollFrame_Update(ScrollFrameFeeds, feedCount, 5, 16);
  
    for line = 1,5 do
    offset = line + FauxScrollFrame_GetOffset(ScrollFrameFeeds);
    if offset <= feedCount then
      getglobal("ButtonFeed"..line):SetText(GetFeedTitle(offset));
      getglobal("ButtonFeed"..line):Show();
    else
      getglobal("ButtonFeed"..line):Hide();
    end
  end
end

function ScrollFrameItems_Update()
	local line; -- 1 through 5 of our window to scroll
	local offset; -- an index into our data calculated from the scroll offset
  
	local feed = GetFeed(CURRENT_FEED_INDEX);
	local itemCount = #feed;
	
	FauxScrollFrame_Update(ScrollFrameItems, itemCount, 5, 16);
  
    for line = 1,5 do
    offset = line + FauxScrollFrame_GetOffset(ScrollFrameItems);
    if offset <= itemCount then
      getglobal("ButtonItem"..line):SetText(feed[offset][2]);
      getglobal("ButtonItem"..line):Show();
    else
      getglobal("ButtonItem"..line):Hide();
    end
  end
end

function ScrollFrameDescription_Update()
	local feed = GetFeed(CURRENT_FEED_INDEX);
	SummaryFontString:SetText(feed[CURRENT_ITEM_INDEX][3]);
end

function OnClickButtonFeed(index)
    CURRENT_FEED_INDEX = index + FauxScrollFrame_GetOffset(ScrollFrameFeeds);
    CURRENT_ITEM_INDEX = 1;
    
	HighlightButton("ButtonFeed", index);
	HighlightButton("ButtonItem", 1);
    
    ScrollFrameItems_Update();
    ScrollFrameDescription_Update();
end

function OnClickButtonItem(index)
    CURRENT_ITEM_INDEX = index + FauxScrollFrame_GetOffset(ScrollFrameItems);
    
	HighlightButton("ButtonItem", index);

    ScrollFrameDescription_Update();
end

function HighlightButton(name, index)
	for line = 1,5 do
		if line == index then
			getglobal(name .. line):SetTextColor(1.0, 1.0, 1.0, 1.0);
		else
			getglobal(name .. line):SetTextColor(1.0, 0.82, 0, 1.0);
		end
	end
end

 -- Helper methods
function GetFeedCount()
   local n=0;
   for title, feed in pairs(COMMON_FEED_LIST_FEEDS) do
     n=n+1;
   end
   return n;
 end
 
function GetFeedTitle(index)
   local n=0;
   for title, feed in pairs(COMMON_FEED_LIST_FEEDS) do
     n=n+1;
     if (index == n) then
		return title;
	 end
   end
   return n;
 end
 
function GetFeed(index)
   local n=0;
   for title, feed in pairs(COMMON_FEED_LIST_FEEDS) do
     n=n+1;
     if (index == n) then
		return feed;
	 end
   end
   return n;
 end

function Frame1_OnMouseDown()
	this:StartSizing();
end

function Frame1_OnMouseUp()
	this:StopMovingOrSizing();
end