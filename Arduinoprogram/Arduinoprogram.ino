#include <ArduinoJson.h>

#define INPUT_PIN1 2 
#define INPUT_PIN2 3
#define INPUT_PIN3 4
#define INPUT_PIN4 5
#define INPUT_PIN5 6
#define INPUT_PIN6 7

// Размер буфера JSON
const int bufferSize = 200;
char jsonBuffer[bufferSize];
StaticJsonDocument<bufferSize> jsonDoc;
long curtime = 0;
//-1 проверка
//0 - не передавать
//1 - передавать
int mode = -1;

void setup() {
  Serial.begin(57600);
  pinMode(INPUT_PIN1, INPUT_PULLUP);
  pinMode(INPUT_PIN2, INPUT_PULLUP);
  pinMode(INPUT_PIN3, INPUT_PULLUP);
  pinMode(INPUT_PIN4, INPUT_PULLUP);
  pinMode(INPUT_PIN5, INPUT_PULLUP);
  pinMode(INPUT_PIN6, INPUT_PULLUP);
}

void loop() {
  if (mode == -1){
    while(true){
      if (Serial.available() > 0) {
        String receivedData = Serial.readStringUntil('\n');
        deserializeJson(jsonDoc, receivedData);
        String receivedCommand = jsonDoc["command"];
        if (receivedCommand == "check") {
          Serial.println("Checked!");
          curtime = 0;
          mode = 0;
          break;
        }
      }
    }
  }

  if (Serial.available() > 0) {
    jsonDoc.clear(); 
    String receivedData = Serial.readStringUntil('\n');
    deserializeJson(jsonDoc, receivedData);
    String receivedCommand = jsonDoc["command"];
    if (receivedCommand == "gv") {
      mode = 1;
    }
    else if (receivedCommand == "ngv") {
      mode = 0;
    }
    else if (receivedCommand == "check") {
      Serial.println("Checked!");
      curtime = 0;
      mode = 0;
    }
  }

  if(mode == 1) giveValues();
}

void giveValues(){
  jsonDoc.clear(); // Очистить JSON-объект
  JsonArray jsonArray = jsonDoc.createNestedArray("v");
  for (int i = 2; i <= 7; i++) {
    jsonArray.add(digitalRead(i));
  }
  jsonDoc["t"] = curtime;

  serializeJson(jsonDoc, jsonBuffer, bufferSize);
  Serial.println(jsonBuffer);
  curtime++;

  delay(10);
}
