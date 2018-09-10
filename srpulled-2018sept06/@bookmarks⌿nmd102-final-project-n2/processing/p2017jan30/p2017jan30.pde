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
  fill(0,0,0);
  //pupil
  int mouseAdjX=0;
  int mouseAdjY=0;

  int deltaX = 500;
  if(isLeft){
    deltaX = 1500;
  }
  if(mouseX>1000) {
    mouseAdjX = (mouseX - deltaX)/50;
  }
  else {
    mouseAdjX = -(deltaX - mouseX)/50;
  }
  mouseAdjX=max(mouseAdjX,0);

  if(mouseY>570) {
    mouseAdjY = (mouseY - 570)/70;
  }
  else {
    mouseAdjY = -(570 - mouseY)/70;
  }

  ellipse(1000+mouseAdjX,570+mouseAdjY,10,10);
}

void draw() {
  lights();
  pushMatrix();
  // head
  fill(225,150,175);
  translate(1000,700);
  scale(1,1.2);
  sphere(200);
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
  translate(0,160,600);
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
