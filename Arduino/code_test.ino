#include <Stepper.h>

const int stepsPerRevolution = 2048; 
const int maxAirspeed = 240;
const int maxMainRotorRPM = 400;
bool hornON = false;
bool testLights = false;
bool muted = false; // New variable to track mute state
const int ledFire = 7;
const int ledFuel = 6;
const int horn = 5;
const int ledPitot = 4;
const int ledHyd = 2;
const int ledOil = 13;
const int testWarningLights = 3;
const int buzzerOffButton = 12;
Stepper myStepper(stepsPerRevolution, 8, 10, 9, 11);

int previousAirspeed = 0;

// LED state variables
int fireLEDState = LOW;
int fuelLEDState = LOW;
int pitotLEDState = LOW;
int hydLEDState = LOW;
int oilLEDState = LOW;
bool hornRequired = false;

void setup() {
    Serial.begin(115200);
    pinMode(horn, OUTPUT);
    pinMode(ledFire, OUTPUT);
    pinMode(ledFuel, OUTPUT);
    pinMode(ledPitot, OUTPUT);
    pinMode(ledHyd, OUTPUT);
    pinMode(ledOil, OUTPUT);
    pinMode(testWarningLights, INPUT_PULLUP);
    pinMode(buzzerOffButton, INPUT_PULLUP); // Configure pushbutton with pull-up resistor
    myStepper.setSpeed(15);
}

void loop() {
    testLights = digitalRead(testWarningLights) == LOW;
    bool buzzerOffPressed = digitalRead(buzzerOffButton) == LOW; // Check if button is pressed

    if (testLights) {
        digitalWrite(ledFire, HIGH);
        digitalWrite(ledFuel, HIGH);
        digitalWrite(ledPitot, HIGH);
        digitalWrite(ledHyd, HIGH);
        digitalWrite(ledOil, HIGH);
    } else {
        hornRequired = false;

        if (!Serial.available() > 0) {
            String receivedString = Serial.readStringUntil('\n');

            int aspdIndex = receivedString.indexOf("ASPD:");
            if (aspdIndex != -1) {
                int currentAirspeed = extractValue(receivedString, aspdIndex, 5, '/');
                int stepsToMove = map(currentAirspeed, 0, maxAirspeed, 0, stepsPerRevolution) - 
                                  map(previousAirspeed, 0, maxAirspeed, 0, stepsPerRevolution);
                myStepper.step(stepsToMove);
                previousAirspeed = currentAirspeed;
            }

            int fireIndex = receivedString.indexOf("FIRE:");
            if (fireIndex != -1) {
                int fireValue = extractValue(receivedString, fireIndex, 5, '/');
                fireLEDState = fireValue == 1 ? HIGH : LOW;
            }

            int fuelIndex = receivedString.indexOf("FUEL:");
            if (fuelIndex != -1) {
                float fuelValue = extractValue(receivedString, fuelIndex, 5, '/');
                fuelLEDState = fuelValue <= 15.85 ? HIGH : LOW;
            }

            int pitotIdx = receivedString.indexOf("PIT:");
            if (pitotIdx != -1) {
                int pitotValue = extractValue(receivedString, pitotIdx, 4, '/');
                pitotLEDState = pitotValue == 1 ? HIGH : LOW;
            }

            int mrIndex = receivedString.indexOf("MR:");
            if (mrIndex != -1) {
                float mrValue = extractValue(receivedString, mrIndex, 3, '/');
                if (mrValue >= 250 && mrValue < 350) {
                    hornRequired = true;
                }
            }

            int hydIndex = receivedString.indexOf("HYDP:");
            if (hydIndex != -1) {
                float hydValue = extractValue(receivedString, hydIndex, 5, '/');
                hydLEDState = hydValue <= 62656.3 ? HIGH : LOW;
                if (hydValue <= 62656.3) {
                    hornRequired = true;
                }
            }

            int hydQTYIndex = receivedString.indexOf("HYDQ:");
            if (hydQTYIndex != -1) {
                float hydQTYValue = extractValue(receivedString, hydQTYIndex, 5, '/');
            }

            int oilIndex = receivedString.indexOf("OILP:");
            if (oilIndex != -1) {
                float oilValue = extractValue(receivedString, oilIndex, 5, '/');
                oilLEDState = oilValue <= 835.41 ? HIGH : LOW;
            }

            Serial.println("READY");
        }

        // Update LEDs based on stored state variables
        digitalWrite(ledFire, fireLEDState);
        digitalWrite(ledFuel, fuelLEDState);
        digitalWrite(ledPitot, pitotLEDState);
        digitalWrite(ledHyd, hydLEDState);
        digitalWrite(ledOil, oilLEDState);

        // Buzzer control logic with mute function
        if (buzzerOffPressed && hornON) {
            // Mute the buzzer if the button is pressed and it's currently on
            noTone(horn);
            hornON = false;
            muted = true; // Set muted to true after pressing the button
        }

        // Turn on buzzer if a new failure condition is met and it's not muted
        if (hornRequired && !hornON && !muted) {
            tone(horn, 220);
            hornON = true;
        } else if (!hornRequired && hornON) {
            noTone(horn);
            hornON = false;
        }

        // Reset mute if a new failure condition arises
        if (hornRequired && muted) {
            muted = false; // Reset mute status when a new condition triggers the horn
        }
    }
}

// Helper function to extract integer or float values from received string
float extractValue(String &data, int startIndex, int offset, char delimiter) {
    int endIndex = data.indexOf(delimiter, startIndex + offset);
    return data.substring(startIndex + offset, endIndex).toFloat();
}
