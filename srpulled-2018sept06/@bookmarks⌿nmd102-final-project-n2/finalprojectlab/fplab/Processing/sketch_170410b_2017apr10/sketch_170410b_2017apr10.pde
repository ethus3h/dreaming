import processing.serial.*; //import the Serial library
int end = 10; // the number 10 is ASCII for linefeed (end of serial.println), later we will look for this to break up individual messages
String serial; // declare a new string called 'serial' . A string is a sequence of characters (data type know as "char")
Serial port; // The serial port, this is a new instance of the Serial class (an Object)
float x;
float y;
float z;
int ix;
int iy;
int iz;
int clickyColorR = 0;
int clickyColorG = 0;
int clickyColorB = 0;
int clickyColorAR = 205;
int clickyColorAG = 50;
int clickyColorAB = 205;
int counterColorH = 0;
int mouseReplacementX;
int mouseReplacementY;
int inputPositionX = 0;
int inputPositionY = 0;

void setup() {
   background(0);
   size(2000, 1500, OPENGL);
   if(Serial.list().length > 0) {
     port = new Serial(this, Serial.list()[0], 9600); // initializing the object by assigning a port and baud rate (must match that of Arduino)
     port.clear(); // function from serial library that throws out the first reading, in case we started reading in the middle of a string from Arduino
     serial = port.readStringUntil(end); // function that reads the string from serial port until a println and then assigns string to our string variable (called 'serial')
     serial = null; // initially, the string will be null (empty)
   }
}

void eye(boolean isLeft) {
   // iris
   fill(255, 0, 255);
   arc(1000, 600, 80, 80, -3 * QUARTER_PI, -QUARTER_PI, OPEN);
   arc(1000, 543, 80, 80, -7 * QUARTER_PI, -5 * QUARTER_PI, OPEN);
   fill(clickyColorR, clickyColorG, clickyColorB);
   //pupil
   int mouseAdjX = 0;
   int mouseAdjY = 0;
  
   int deltaX = 500;
  
   int mouseXd = (mouseReplacementX * 2) - deltaX;
   int mouseYd = mouseReplacementY;
   if (isLeft) {
      mouseXd = mouseXd - 2000;
   }
  
   mouseAdjX = (mouseXd - deltaX) / 50;
  
   if (mouseY > 570) {
      mouseAdjY = (mouseYd - 570) / 100;
   } else {
      mouseAdjY = -(570 - mouseYd) / 100;
   }
  
   mouseAdjX = min(mouseAdjX, 20);
  
   if (isLeft) {
      if (inputPositionX < 1000) {
         mouseAdjX = -20;
      }
   }
  
   ellipse(1000 + mouseAdjX, 570 + mouseAdjY, 10, 10);
}

void draw() {
   inputPositionX = mouseX;
   inputPositionY = mouseY;
   mouseReplacementX = inputPositionX;
   mouseReplacementY = inputPositionY;
   if(Serial.list().length > 0) {
     while (port.available() > 0) { //as long as there is data coming from serial port, read it and store it
       serial = port.readStringUntil(end);
     }
   }
   if (serial != null) { //if the string is not empty, print the following
      /*  Note: the split function used below is not necessary if sending only a single variable. However, it is useful for parsing (separating) messages when
          reading from multiple inputs in Arduino. Below is example code for an Arduino sketch
      */
    
      println(serial);
    
      String[] a = split(serial, ','); //a new array (called 'a') that stores values into separate cells (separated by commas specified in your Arduino program)
      println(a[0]); //print Value1 (in cell 1 of Array - remember that arrays are zero-indexed)
      println(a[1]); //print Value2 value
    
      x = float(a[0]);
      y = float(a[1]);
      z = float(a[2]);
    
      fill(x, y, x);
      rect(1000, 700, x - 335, y - 335);
   }
   iy = (int) x;
   ix = (int) y;
   iz = (int) z;
   mouseReplacementX = mouseReplacementX + iy;
   mouseReplacementY = mouseReplacementY + ix;
   lights();
   colorMode(HSB, 100);
   if (counterColorH < 255) {
      counterColorH = counterColorH + 1 + (int)(x / 335);
   } else {
      counterColorH = 0;
   }
   fill(counterColorH, 255, 255, 2);
   rect(0, 0, 10000, 10000);
   translate((1500 - mouseReplacementX) / 10, (1500 - mouseReplacementY) / 10);
   pushMatrix();
   // head
   fill(clickyColorAR, clickyColorAG, clickyColorAB);
   translate(1000, 700);
   scale(1, 1.2);
   sphere(200);
   colorMode(RGB, 100);
   // hair
   fill(50, 20, 40, 20);
   translate(0, -100);
   scale(1, 0.8);
   sphere(250);
   translate(0, 0, 600);
   // mouth
   // Because a lot of my self-expression is via computerized text lol
   textSize(40);
   text("mouth", -65, 230);
   popMatrix();
   // nose
   pushMatrix();
   fill(50, 20, 40);
   translate(1000, 700, 600);
   rotateY(PI / 6);
   rotateX(PI / 6);
   box(40);
   popMatrix();
   // left eye
   pushMatrix();
   translate(40, 100, 600);
   eye(true);
   popMatrix();
   // right eye
   pushMatrix();
   translate(-40, 100, 600);
   eye(false);
   popMatrix();
   if(Serial.list().length > 0) {  
     if(iz < 1) {
       mouseClicked();
     }
   }
}

void mouseClicked() {
   clickyColorR = (int) random(0, 255);
   clickyColorG = (int) random(0, 255);
   clickyColorB = (int) random(0, 255);
   clickyColorAR = clickyColorR;
   clickyColorAG = clickyColorG;
   clickyColorAB = clickyColorB;
}