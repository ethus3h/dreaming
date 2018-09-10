Boolean night = false;
Boolean hasBecomeNight = false;
int[] starColors = new int[1000];
int[] starXPositions = new int[1000];
int[] starYPositions = new int[1000];
int[] starRates = new int[1000];
Boolean[] starColorstates = new Boolean[1000];
int timeSinceSunset=0;
float sunsetPercentage=0;
int i=0;
void setup() {
  fullScreen();
  for(i=0; i<1000; i=i+1) {
    starColors[i] = 0;
    starXPositions[i]=(int)random(0,width);
    starYPositions[i]=(int)random(0,(int)(2*(height/3)));
    starRates[i]=(int)random(1,3);
    starColorstates[i]=true;
  }
}

void draw() {
  noStroke();
  if (night) fill(0,0,0); else fill(0,0,255);
  rect(0,0,width,(int)(2*(height/3)));
  rect(0,0,width,(int)(height/3));
  color nightSkyTop = color(0, 25, 75);
  color nightSkyBottom = color(0, 0, 0);
  color nightLandTop = color(0, 25, 75);
  color nightLandBottom = color(0, 0, 0);
  color daySkyTop = color(50, 200, 255);
  color daySkyBottom = color(100, 50, 255);
  color dayLandTop = color(200, 0, 0);
  color dayLandBottom = color(20, 255, 30);
  if (night) {
    if (timeSinceSunset < 1000) { timeSinceSunset=timeSinceSunset+1; }
    sunsetPercentage=timeSinceSunset/1000.0;
  }
  else {
    if(hasBecomeNight) {
      if (timeSinceSunset < 1000) { timeSinceSunset=timeSinceSunset+1; }
      sunsetPercentage=(1000-timeSinceSunset)/1000.0;
    }
  }
  color skyTop = lerpColor(daySkyTop,nightSkyTop,sunsetPercentage);
  color skyBottom = lerpColor(daySkyBottom,nightSkyBottom,sunsetPercentage);
  color landTop = lerpColor(dayLandTop,nightLandTop,sunsetPercentage);
  color landBottom = lerpColor(dayLandBottom,nightLandBottom,sunsetPercentage);
  setGradient(0, 0, width, (int)(2*(height/3)), skyTop, skyBottom, 2);
  setGradient(0, (int)(2*(height/3)), width, (int)(2*(height/3)), landTop, landBottom, 2);
  if (night) {
    //draw starColors
    int i=0;
    for(i=0; i<1000; i=i+1) {
      int currentStarColor = starColors[i];
      if (currentStarColor <= 100) {
        // Star has died, so spawn a new one.
        starColorstates[i] = true;
      }
      if (currentStarColor >= 255) {
        // Star has reached equinox
        starColorstates[i] = false;
      }
      int newStarColor = currentStarColor;
      if (starColorstates[i] == true) {
        // Star is growing
        if (random(0,15)<3) {
          newStarColor = currentStarColor + starRates[i];
        }
      }
      else {
        // Star is shrinking
        if (random(0,15)<3) {
          newStarColor = currentStarColor - starRates[i];
        }
      }
      if (random(0,100000)<2) {
        newStarColor=0;
      }
      starColors[i] = newStarColor;
      // Draw star
      noStroke();
      fill(255,255,255,starColors[i]);
      ellipse(starXPositions[i],starYPositions[i],5,5);
      // Draw moon
      // fill(255,255,255);
      // pushMatrix();
      // translate(500,200);
      // beginShape();
      // vertex(0, 0);
      // bezierVertex(0, 200, 50, 100, 0, 0);
      // bezierVertex(0, 200, 0, 100, 0, 0);
      // endShape();
      // popMatrix();
    }
  }
}

void setGradient(int x, int y, float w, float h, color skyTop, color skyBottom, int axis) {
  // from https://processing.org/examples/lineargradient.html
  noFill();

  if (axis == 2) {  // Top to bottom gradient
    for (int i = y; i <= y+h; i++) {
      float inter = map(i, y, y+h, 0, 1);
      color c = lerpColor(skyTop, skyBottom, inter);
      stroke(c);
      line(x, i, x+w, i);
    }
  }  
  else if (axis == 1) {  // Left to right gradient
    for (int i = x; i <= x+w; i++) {
      float inter = map(i, x, x+w, 0, 1);
      color c = lerpColor(skyTop, skyBottom, inter);
      stroke(c);
      line(i, y, i, y+h);
    }
  }
}

void mouseClicked() {
  hasBecomeNight=true;
  if (night) night=false; else night=true;
  timeSinceSunset=0;
}
