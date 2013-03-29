world_id = "Enter a unique identifier for your world. Must contain between four and 64 characters: letters, numbers, or the underscore character (_).";
pretty_name = "Enter a readable name that will be used in the directory listing. This name should be relatively short and may contain spaces and punctuation marks.";
description = "Enter a brief text description of your world that can be several paragraphs in length.";
server_name = "Enter the DNS name or IP address of your server system (if you are running distributed server configuration, this is the name/IP of your proxy server).";
port_num = "Enter the TCP port of your world server. This corresponds to the multiverse.worldmgrport property setting in your servers.  Default is 5040.";
public = "Whether to display the world in the Multiverse public world directory once it is approved.  Only check this if you are ready to release your world for public access, pending approval by Multiverse.";
patcher_url = "Enter the URL of the web page the Client will display when it is downloading world assets.";
media_url = "Enter the URL of the world's asset repository.";
logo_url = "Enter the URL of the 150 x 90 pixel logo displayed in the Multiverse Network Nexus login page";
detail_url = "Enter the URL of a web page that provides more information about your world.";

function ietruebody(){
  return (document.compatMode && document.compatMode!="BackCompat")? document.documentElement : document.body
}

function floattip(thetext) {
  return ddrivetip(thetext, "#EDEDED", 300)
}

function ddrivetip(thetext, thecolor, thewidth){
  if (ns6||ie) {
    if (typeof thewidth!="undefined") tipobj.style.width=thewidth+"px"
      if (typeof thecolor!="undefined" && thecolor!="") tipobj.style.backgroundColor=thecolor
        tipobj.innerHTML=thetext
    enabletip=true
    return false
  }
}

function positiontip(e){
  if (enabletip){
    var curX=(ns6)?e.pageX : event.clientX+ietruebody().scrollLeft;
    var curY=(ns6)?e.pageY : event.clientY+ietruebody().scrollTop;
    //Find out how close the mouse is to the corner of the window
    var rightedge=ie&&!window.opera? ietruebody().clientWidth-event.clientX-offsetxpoint : window.innerWidth-e.clientX-offsetxpoint-20
    var bottomedge=ie&&!window.opera? ietruebody().clientHeight-event.clientY-offsetypoint : window.innerHeight-e.clientY-offsetypoint-20
    
    var leftedge=(offsetxpoint<0)? offsetxpoint*(-1) : -1000
    
    //if the horizontal distance isn't enough to accomodate the width of the context menu
    if (rightedge<tipobj.offsetWidth)
      //move the horizontal position of the menu to the left by it's width
      tipobj.style.left=ie? ietruebody().scrollLeft+event.clientX-tipobj.offsetWidth+"px" : window.pageXOffset+e.clientX-tipobj.offsetWidth+"px"
    else if (curX<leftedge)
      tipobj.style.left="5px"
    else    
      tipobj.style.left=curX+offsetxpoint+"px" //position the horizontal position of the menu where the mouse is positioned
    
    //same concept with the vertical position
    if (bottomedge<tipobj.offsetHeight)
      tipobj.style.top=ie? ietruebody().scrollTop+event.clientY-tipobj.offsetHeight-offsetypoint+"px" : window.pageYOffset+e.clientY-tipobj.offsetHeight-offsetypoint+"px"
    else
      tipobj.style.top=curY+offsetypoint+"px"
      
    tipobj.style.visibility="visible"
  }
}

function hideddrivetip(){
  if (ns6||ie){
    enabletip=false
    tipobj.style.visibility="hidden"
    tipobj.style.left="-1000px"
    tipobj.style.backgroundColor=''
    tipobj.style.width=''
  }
}

