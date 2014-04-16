import com.leapmotion.leap.CircleGesture;
import com.leapmotion.leap.Gesture.State;
import com.leapmotion.leap.Gesture.Type;
import com.leapmotion.leap.Hand;
import com.leapmotion.leap.KeyTapGesture;
import com.leapmotion.leap.ScreenTapGesture;
import com.leapmotion.leap.SwipeGesture;
import com.onformative.leap.LeapMotionP5;
import processing.serial.*;
import java.util.*;
static final boolean ON_SERIAL = true; 
static Serial port;  
static final int SERIAL_ID = 2; 

LeapMotionP5 leap;
String lastGesture =
"enabling gestures: \n'c' for CircleGesture\n's' for SwipeGesture\n'k' for KeyTapGesture\n't' for ScreenTapGesture";

public void setup() {
  size(500, 500);
  textSize(17);

  leap = new LeapMotionP5(this);
  leap.enableGesture(Type.TYPE_SWIPE);
  lastGesture = "Swipe Gesture enabled.";
  
  if (ON_SERIAL) {
    for (int i = 0; i < 3; ++i) {
      println(Serial.list()[i]);
    }
    port = new Serial(this, Serial.list()[SERIAL_ID], 9600);
  }
}

public void draw() {
  background(0);
  for (Hand hand : leap.getHandList()) {
    PVector handPos = leap.getPosition(hand);
    ellipse(handPos.x, handPos.y, 20, 20);
  }
  text(lastGesture, 30, 30);
}

public void circleGestureRecognized(CircleGesture gesture, String clockwiseness) {
  if (gesture.state() == State.STATE_STOP) {
    System.out.println("//////////////////////////////////////");
    System.out.println("Gesture type: " + gesture.type().toString());
    System.out.println("ID: " + gesture.id());
    System.out.println("Radius: " + gesture.radius());
    System.out.println("Normal: " + gesture.normal());
    System.out.println("Clockwiseness: " + clockwiseness);
    System.out.println("Turns: " + gesture.progress());
    System.out.println("Center: " + leap.vectorToPVector(gesture.center()));
    System.out.println("Duration: " + gesture.durationSeconds() + "s");
    System.out.println("//////////////////////////////////////");
    lastGesture = "Gesture type: " + gesture.type().toString() + "\n";
    lastGesture += "ID: " + gesture.id() + "\n";
    lastGesture += "Radius: " + gesture.radius() + "\n";
    lastGesture += "Normal: " + gesture.normal() + "\n";
    lastGesture += "Clockwiseness: " + clockwiseness + "\n";
    lastGesture += "Turns: " + gesture.progress() + "\n";
    lastGesture += "Center: " + leap.vectorToPVector(gesture.center()) + "\n";
    lastGesture += "Duration: " + gesture.durationSeconds() + "s" + "\n";
    
  } 
  else if (gesture.state() == State.STATE_START) {
  } 
  else if (gesture.state() == State.STATE_UPDATE) {
  }
}

public void swipeGestureRecognized(SwipeGesture gesture) {
  if (gesture.state() == State.STATE_STOP) {
    //System.out.println("//////////////////////////////////////");
    //System.out.println("Gesture type: " + gesture.type());
    //System.out.println("ID: " + gesture.id());
    //System.out.println("Position: " + leap.vectorToPVector(gesture.position()));
    //System.out.println("Duration: " + gesture.durationSeconds() + "s");
    //System.out.println("Speed: " + gesture.speed());
    //System.out.println("Direction: " + leap.vectorToPVector(gesture.direction()));
    
    String dirStr = "" + gesture.direction();
    dirStr = dirStr.substring(1, dirStr.length() - 1); 
    String[] dirStrs = split(dirStr, ','); 
    float dirX =Float.parseFloat(dirStrs[0]);
    float dirY =Float.parseFloat(dirStrs[1]);
    float dirZ =Float.parseFloat(dirStrs[2]);
    
    System.out.println("Directions: " + dirX + "," + dirY + "," + dirZ);
    
    //System.out.println("//////////////////////////////////////");
    lastGesture = "Gesture type: " + gesture.type().toString() + "\n";
    lastGesture += "ID: " + gesture.id() + "\n";
    lastGesture += "Position: " + leap.vectorToPVector(gesture.position()) + "\n";
    lastGesture += "Direction: " + gesture.direction() + "\n";
    lastGesture += "Speed: " + gesture.speed() + "\n";
    lastGesture += "Duration: " + gesture.durationSeconds() + "s" + "\n";
    
    PVector dir = leap.vectorToPVector(gesture.direction());
    
    if (ON_SERIAL) {
      if (dirX > 0.5) {
        port.write(0);
        System.out.println("Forward");
      } else 
      if (dirX < -0.5) {
        port.write(1);
        System.out.println("Backward");
      }
      
      if (dirZ > 0.5) {
          System.out.println("Up");
      } else {
        System.out.println("Down"); 
      }
    }
  } 
  else if (gesture.state() == State.STATE_START) {
  } 
  else if (gesture.state() == State.STATE_UPDATE) {
  }
  
  
  
  // direction left: -0.9, right 0.9, up: ,last 0.9; down: 0.9
}

public void screenTapGestureRecognized(ScreenTapGesture gesture) {
  if (gesture.state() == State.STATE_STOP) {
    System.out.println("//////////////////////////////////////");
    System.out.println("Gesture type: " + gesture.type());
    System.out.println("ID: " + gesture.id());
    System.out.println("Position: " + leap.vectorToPVector(gesture.position()));
    System.out.println("Direction: " + gesture.direction());
    System.out.println("Duration: " + gesture.durationSeconds() + "s");
    System.out.println("//////////////////////////////////////");
    lastGesture = "Gesture type: " + gesture.type().toString() + "\n";
    lastGesture += "ID: " + gesture.id() + "\n";
    lastGesture += "Position: " + leap.vectorToPVector(gesture.position()) + "\n";
    lastGesture += "Direction: " + gesture.direction() + "\n";
    lastGesture += "Duration: " + gesture.durationSeconds() + "s" + "\n";
  } 
  else if (gesture.state() == State.STATE_START) {
  } 
  else if (gesture.state() == State.STATE_UPDATE) {
  }
}

public void KeyTapGestureRecognized(KeyTapGesture gesture) {
  if (gesture.state() == State.STATE_STOP) {
    //PVector dir = gesture.direction();
    //dir[0];
    
    lastGesture = "Gesture type: " + gesture.type().toString() + "\n";
    lastGesture += "ID: " + gesture.id() + "\n";
    lastGesture += "Position: " + leap.vectorToPVector(gesture.position()) + "\n";
    lastGesture += "Direction: " + gesture.direction() + "\n";
    lastGesture += "Duration: " + gesture.durationSeconds() + "s" + "\n";
  } 
  else if (gesture.state() == State.STATE_START) {
  } 
  else if (gesture.state() == State.STATE_UPDATE) {
  }
  
  
}

public void keyPressed() {
  if (key == 'c') {
    if (leap.isEnabled(Type.TYPE_CIRCLE)) {
      leap.disableGesture(Type.TYPE_CIRCLE);
      lastGesture = "Circle Gesture disabled.";
    } 
    else {
      leap.enableGesture(Type.TYPE_CIRCLE);
      lastGesture = "Circle Gesture enabled.";
    }
  }

  if (key == 's') {
    if (leap.isEnabled(Type.TYPE_SWIPE)) {
      leap.disableGesture(Type.TYPE_SWIPE);
      lastGesture = "Swipe Gesture disabled.";
    } 
    else {
      leap.enableGesture(Type.TYPE_SWIPE);
      lastGesture = "Swipe Gesture enabled.";
    }
  }

  if (key == 'k') {
    if (leap.isEnabled(Type.TYPE_KEY_TAP)) {
      leap.disableGesture(Type.TYPE_KEY_TAP);
      lastGesture = "KeyTap Gesture disabled.";
    } 
    else {
      leap.enableGesture(Type.TYPE_KEY_TAP);
      lastGesture = "KeyTap Gesture enabled.";
    }
  }

  if (key == 't') {
    if (leap.isEnabled(Type.TYPE_SCREEN_TAP)) {
      leap.disableGesture(Type.TYPE_SCREEN_TAP);
      lastGesture = "ScreenTap Gesture disabled. ";
    } 
    else {
      leap.enableGesture(Type.TYPE_SCREEN_TAP);
      lastGesture = "ScreenTap Gesture enabled. ";
    }
  }
}
public void stop() {
  leap.stop();
}

