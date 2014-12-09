int pins[] = {P1_2, P1_3, P1_4, P1_5,     P2_0, P1_0, P2_4, P2_5,       P2_1, P2_2, P1_6, P2_3
};
int i = 0;
void setup()
{
  pinMode(P2_0, OUTPUT);
  pinMode(P1_2, OUTPUT);
  pinMode(P1_0, OUTPUT);
  pinMode(P1_4, OUTPUT);

  pinMode(P2_4, OUTPUT);
  pinMode(P1_7, OUTPUT);
  pinMode(P1_6, OUTPUT);
  pinMode(P2_3, OUTPUT);
  
  pinMode(P1_1, OUTPUT);
  pinMode(P1_3, OUTPUT);
  pinMode(P1_5, OUTPUT);
  pinMode(P2_1, OUTPUT);
  pinMode(P2_2, OUTPUT);
  pinMode(P2_5, OUTPUT);

//  pinMode(P2_0, INPUT_PULLUP);
//  pinMode(P2_1, INPUT_PULLUP);

  digitalWrite(P2_0, HIGH); //7
  digitalWrite(P1_2, HIGH);
  digitalWrite(P1_0, HIGH);//1
  digitalWrite(P1_4, HIGH);//5
  digitalWrite(P2_4, HIGH);//11
  digitalWrite(P1_7, HIGH);
  digitalWrite(P1_6, HIGH);//14     13
  digitalWrite(P2_3, HIGH);//10
  
  digitalWrite(P1_1, HIGH);
  digitalWrite(P1_3, HIGH);//4
  digitalWrite(P1_5, HIGH);//6
  digitalWrite(P2_1, HIGH);//8
  digitalWrite(P2_2, HIGH);//9
  digitalWrite(P2_5, HIGH);//13
//  for (i = 0; i < 200; i += 10) {
//    analogWrite(P1_6, i + 20);
//    analogWrite(P1_0, 220 - i);
//    delay(300);
//  }
  Serial.begin(9600);
  for (i = 0; i < (sizeof(pins)/sizeof(int)); i++){
//    delay(300);
//    digitalWrite(pins[i], LOW);
  }
}

void loop()
{
  if (Serial.available() > 1){
    int b1 = Serial.read();
    int b2 = Serial.read();
//    Serial.print(b1);
//    Serial.print(" ");
//    Serial.print(b2);
//    Serial.println();
//    digitalWrite(pins[0], b1 & 15 > 0 ? LOW : HIGH);
//    b1 >> = 4;
//    -- -- -- -- -- -- -- --
//    ---- -- - - - - -- ----
    for (i = 0; i < 4; i++){
      digitalWrite(pins[i + 8], (b1 & 3) > 0 ? LOW : HIGH);
//      digitalWrite(pins[i + 4], (b2 & 3) > 0 ? LOW : HIGH);
//      b1 >>= 2;
//      b2 >>= 2;
      digitalWrite(pins[i], (b2 & 1) > 0 ? LOW : HIGH);
      digitalWrite(pins[i + 4], (b2 & 16) > 0 ? LOW : HIGH);
      b1 >>= 2;
      b2 >>= 1;
//      delay(50);
    }
//    digitalWrite(RED_LED, b1 & 01 ? LOW : HIGH);
//    digitalWrite(GREEN_LED, b2 & 01 ? LOW : HIGH);
  }

//  analogWrite(P1_3, 120);
}
