// C++ code
//
int x = 0;

int timer = 0;

void setup()
{
  pinMode(4, OUTPUT);
  Serial.begin(9600);
  pinMode(5, OUTPUT);
  pinMode(6, OUTPUT);

  timer += 0;
}

void loop()
{
  if (timer % 100 == 0) {
    if (random(0, 1 + 1) == 1) {
      digitalWrite(4, HIGH);
      Serial.println(timer);
    } else {
      digitalWrite(4, LOW);
    }
  }
  if (timer % 200 == 0) {
    if (random(0, 1 + 1) == 1) {
      digitalWrite(5, HIGH);
    } else {
      digitalWrite(5, LOW);
    }
  }
  if (timer % 300 == 0) {
    if (random(0, 1 + 1) == 1) {
      digitalWrite(6, HIGH);
    } else {
      digitalWrite(6, LOW);
    }
  }
  timer = (timer + 1);
  delay(10); // Delay a little bit to improve simulation performance
}