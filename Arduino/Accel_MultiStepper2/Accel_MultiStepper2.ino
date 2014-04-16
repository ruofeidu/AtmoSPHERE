// Shows how to run three Steppers at once with varying speeds
//
// Requires the Adafruit_Motorshield v2 library 
//   https://github.com/adafruit/Adafruit_Motor_Shield_V2_Library
// And AccelStepper with AFMotor support 
//   https://github.com/adafruit/AccelStepper

// This tutorial is for Adafruit Motorshield v2 only!
// Will not work with v1 shields

#include <AccelStepper.h>
#include <Wire.h>
#include <Adafruit_MotorShield.h>
#include "utility/Adafruit_PWMServoDriver.h"



Adafruit_MotorShield AFMSbot(0x61); // Rightmost jumper closed
Adafruit_MotorShie ld AFMStop(0x60); // Default address, no jumpers
int x = 0, y = 0; 
// Connect two steppers with 200 steps per revolution (1.8 degree)
// to the top shield
Adafruit_StepperMotor *myStepper1 = AFMStop.getStepper(200, 1);
Adafruit_StepperMotor *myStepper2 = AFMStop.getStepper(200, 2);

// you can change these to DOUBLE or INTERLEAVE or MICROSTEP!
// wrappers for the first motor!
void forwardstep1() {  
  myStepper1->onestep(FORWARD, DOUBLE);
}
void backwardstep1() {  
  myStepper1->onestep(BACKWARD, DOUBLE);
}
// wrappers for the second motor!
void forwardstep2() {  
  myStepper2->onestep(FORWARD, DOUBLE);
}
void backwardstep2() {  
  myStepper2->onestep(BACKWARD, DOUBLE);
}

// Now we'll wrap the 3 steppers in an AccelStepper object
AccelStepper stepper1(forwardstep1, backwardstep1);
AccelStepper stepper2(forwardstep2, backwardstep2);

void setup()
{  
  int bottomMotor = 450;
  int topMotor=315;
  
   Serial.begin(9600); 
  
  AFMSbot.begin(); // Start the bottom shield
  AFMStop.begin(); // Start the top shield
   
   //top
  stepper1.setMaxSpeed(100.0);
  stepper1.setAcceleration(100.0);
  //stepper1.moveTo(-topMotor);
    
    //bottom
  stepper2.setMaxSpeed(100.0);
  stepper2.setAcceleration(100.0);
  
  x = 0; 
  y = 0; 
  //stepper2.moveTo(-bottomMotor);
}

void loop()
{
  String str="";
  if (Serial.available() > 0) { 
    str = Serial.readStringUntil(','); 
    x = str.toInt();
    
    str = Serial.readStringUntil('\n'); 
    y = str.toInt();
    Serial.println(x);
    Serial.println(y);
    
    stepper1.moveTo(-x);
    stepper2.moveTo(-y);
  }
  
  /*if (Serial.available() > 0) {
     int state = Serial.read(); 
     Serial.println(state);
  }
    Change direction at the limits
*/
   if (stepper1.distanceToGo() == 0)
	stepper1.moveTo(-x);

    if (stepper2.distanceToGo() == 0)
	stepper2.moveTo(-y);

    stepper1.run();
    stepper2.run();
}

