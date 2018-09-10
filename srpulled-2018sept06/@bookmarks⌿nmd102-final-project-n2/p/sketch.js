function setup() {
  createCanvas(windowWidth, windowHeight*0.75)
}
//background gradient inspired by http://www.dokimos.org/ajff/
function getRandomInt(min, max) {
    /* From http://stackoverflow.com/questions/1527803/generating-random-whole-numbers-in-javascript-in-a-specific-range */
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

var colorShifted=0
var hueshift=0
function draw() {
    var colorDelta=0
    if( (mouseX > (windowWidth / 2)) && (mouseY > (windowHeight / 2)) ) {
        colorDelta = 50
    }
    if( (mouseX > (windowWidth / 2)) && (mouseY < (windowHeight / 2)) ) {
        colorDelta = 150
    }
    if( (mouseX < (windowWidth / 2)) && (mouseY > (windowHeight / 2)) ) {
        colorDelta = 200
    }
    colorDelta=colorDelta+colorShifted
    fill(255,255,255,50)
    rect(0,0,windowWidth,windowHeight)
    fill(255 - colorDelta, 0 + colorDelta, 255 - colorDelta)
    noStroke()
    ellipse(mouseX + 50, mouseY + 50, 100, 100)
    fill(255 - colorDelta, 0, 0)
    rect(mouseX + 30, mouseY + 100, 40, 80, 20)
    rect(mouseX + 15, mouseY + 110, 12.5, 50, 10)
    rect(mouseX + 72.5, mouseY + 110, 12.5, 50, 10)
    fill(255 - colorDelta, 255 - colorDelta, 255 - colorDelta)
    ellipse(mouseX + 25, mouseY + 40, 25, 20)
    ellipse(mouseX + 65, mouseY + 40, 25, 20)
    rect(mouseX + 15, mouseY + 175, 12.5, 40, 10)
    rect(mouseX + 72.5, mouseY + 175, 12.5, 40, 10)
    ellipse(mouseX + 15, mouseY + 207.5, 25, 20)
    ellipse(mouseX + 76, mouseY + 207.5, 17.5, 20)
    noFill()
    stroke(0, 0, 0)
    line(mouseX + 44, mouseY + 50, mouseX + 42, mouseY + 60)
    line(mouseX + 42, mouseY + 60, mouseX + 45, mouseY + 65)
    /* from http://web.archive.org/web/20161202163130/http://stackoverflow.com/questions/5195303/set-css-attribute-in-javascript */
    try { var torm=document.getElementById("blah"); torm.parentNode.removeChild(torm) } catch(e) {}
    var styleEl = document.createElement('style'), styleSheet
    document.head.appendChild(styleEl)
    styleSheet = styleEl.sheet
    styleSheet.insertRule("canvas { filter: hue-rotate("+mouseX+"deg) blur("+mouseY/(windowHeight/5)+"px) }", 0)
    var red=Math.round((mouseX/windowWidth)*255)
    var green=Math.round((mouseY/windowHeight)*255)
    var blue=Math.round((mouseY/windowHeight)*255)
    styleSheet.insertRule("#weed { background-color: rgb("+red+","+green+","+blue+"); }", 0)
    styleEl.id="blah"
    if(hueshift<255) {
        hueshift=hueshift+1
    }
    else {
        hueshift=0
    }
    
}

function mousePressed() {
    colorShifted=getRandomInt(-254,255)
}
