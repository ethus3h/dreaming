int clickyColorR=0;
int clickyColorG=0;
int clickyColorB=0;
int clickyColorAR=205;
int clickyColorAG=50;
int clickyColorAB=205;
int counterColorH=0;

void setup() {
  // self-portrait
  // based on the cityscape
  size(2000,1500,OPENGL);
}

void eye(boolean isLeft) {
  // iris
  fill(255,0,255);
  arc(1000, 600, 80, 80, -3*QUARTER_PI, -QUARTER_PI, OPEN);
  arc(1000, 543, 80, 80, -7*QUARTER_PI, -5*QUARTER_PI, OPEN);
  fill(clickyColorR,clickyColorG,clickyColorB);
  //pupil
  int mouseAdjX=0;
  int mouseAdjY=0;
  
  int deltaX = 500;

  int mouseXd=(mouseX*2)-deltaX;
  int mouseYd=mouseY;
  if(isLeft) {
    mouseXd=mouseXd-2000;
  }

  mouseAdjX = (mouseXd - deltaX)/50;

  if(mouseY>570) {
    mouseAdjY = (mouseYd - 570)/100;
  }
  else {
    mouseAdjY = -(570 - mouseYd)/100;
  }

  mouseAdjX = min(mouseAdjX,20);

  if(isLeft) {
    if (mouseX < 1000) {
      mouseAdjX=-20;
    }
  }

  ellipse(1000+mouseAdjX,570+mouseAdjY,10,10);
}

void draw() {
  lights();
  colorMode(HSB,100);
  if(counterColorH<255) {
    counterColorH=counterColorH+1;
  }
  else {
    counterColorH=0;
  }
  fill(counterColorH,255,255,2);
  rect(0,0,10000,10000);
  translate((1500-mouseX)/10,(1500-mouseY)/10);
  pushMatrix();
  // head
  fill(clickyColorAR,clickyColorAG,clickyColorAB);
  translate(1000,700);
  scale(1,1.2);
  sphere(200);
  colorMode(RGB,100);
  // hair
  fill(50,20,40,20);
  translate(0,-100);
  scale(1,0.8);
  sphere(250);
  translate(0,0,600);
  // mouth
  // Because a lot of my self-expression is via computerized text lol
  textSize(40);
  text("mouth",-65,230);
  popMatrix();
  // nose
  pushMatrix();
  fill(50,20,40);
  translate(1000,700,600);
  rotateY(PI/6);
  rotateX(PI/6);
  box(40);
  popMatrix();
  // left eye
  pushMatrix();
  translate(40,100,600);
  eye(true);
  popMatrix();
  // right eye
  pushMatrix();
  translate(-40,100,600);
  eye(false);
  popMatrix();
}

void mouseClicked() {
  clickyColorR=(int)random(0,255);
  clickyColorG=(int)random(0,255);
  clickyColorB=(int)random(0,255);
  clickyColorAR=clickyColorR;
  clickyColorAG=clickyColorG;
  clickyColorAB=clickyColorB;
}
