sfHover = function() {
	var sfEls = document.getElementById("nav").getElementsByTagName("LI");
	for (var i=0; i<sfEls.length; i++) {
		sfEls[i].onmouseover=function() {
			this.className+=" sfhover";
		}
		sfEls[i].onmouseout=function() {
			this.className=this.className.replace(new RegExp(" sfhover\\b"), "");
		}
	}
}
if (window.attachEvent) window.attachEvent("onload", sfHover);

function changeImage(anImage, newSource) {
   document.images[anImage].src = '//multiverse.kothuria.com/images/navigation/' + newSource;
}

function emailAddr(addr) {
  dmn = "multiverse.net";
  e = addr + "@" + dmn;
  return e;
}

function mailLink(a) {
  s1 = "<a href='mailto:";
  s2 =  "'>";
  s3 = "</a>";
  document.write(s1 + emailAddr(a) + s2 + emailAddr(a) + s3);
}