/***********************************************
* DHTML Billboard script- © Dynamic Drive (www.dynamicdrive.com)
* This notice must stay intact for use
* Visit http://www.dynamicdrive.com/ for full source code
***********************************************/

//List of transitional effects to be randomly applied to billboard:
//var billboardeffects=["GradientWipe(GradientSize=1.0 Duration=0.7)", "Inset", "Iris", "Pixelate(MaxSquare=5 enabled=false)", "RadialWipe", "RandomBars", "Slide(slideStyle='push')", "Spiral", "Stretch", "Strips", "Wheel", "ZigZag"]

var billboardeffects=["RandomBars"] //Uncomment this line and input one of the effects above (ie: "Iris") for single effect.

var tickspeed=4000 //ticker speed in miliseconds (2000=2 seconds)
var effectduration=1000 //Transitional effect duration in miliseconds
var hidecontent_from_legacy=1 //Should content be hidden in legacy browsers- IE4/NS4 (0=no, 1=yes).

var filterid=Math.floor(Math.random()*billboardeffects.length)

document.write('<style type="text/css">\n')
if (document.getElementById)
document.write('.billcontent{display:none;\n'+'filter:progid:DXImageTransform.Microsoft.'+billboardeffects[filterid]+'}\n')
else if (hidecontent_from_legacy)
document.write('#contentwrapper{display:none;}')
document.write('</style>\n')

var selectedDiv=0
var totalDivs=0

function contractboard(){
var inc=0
while (document.getElementById("billboard"+inc)){
document.getElementById("billboard"+inc).style.display="none"
inc++
}
}

function expandboard(){
var selectedDivObj=document.getElementById("billboard"+selectedDiv)
contractboard()
if (selectedDivObj.filters){
if (billboardeffects.length>1){
filterid=Math.floor(Math.random()*billboardeffects.length)
selectedDivObj.style.filter="progid:DXImageTransform.Microsoft."+billboardeffects[filterid]
}
selectedDivObj.filters[0].duration=effectduration/1000
selectedDivObj.filters[0].Apply()
}
selectedDivObj.style.display="block"
if (selectedDivObj.filters)
selectedDivObj.filters[0].Play()
selectedDiv=(selectedDiv<totalDivs-1)? selectedDiv+1 : 0
setTimeout("expandboard()",tickspeed)
}

function startbill(){
while (document.getElementById("billboard"+totalDivs)!=null)
totalDivs++
if (document.getElementById("billboard0").filters)
tickspeed+=effectduration
expandboard()
}

if (window.addEventListener)
window.addEventListener("load", startbill, false)
else if (window.attachEvent)
window.attachEvent("onload", startbill)
else if (document.getElementById)
window.onload=startbill

