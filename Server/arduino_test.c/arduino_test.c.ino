#include <SoftwareSerial.h>
/*
AT+CWMODE=2
AT+CWSAP="ESP_WROOM_02","hoge4009",2,3
AT+CWMODE_DEF=1
AT+CWLAP
AT+CWJAP="IODATA-7ba9ed-2G",""
*/

// 曲げセンサ抵抗には25kΩが必要
// ->47kΩ抵抗×2並列

// 

unsigned long time = 0, time_old = 0, time3 = 0, time4 = 0;
float deg1, g1;

void setup() {
  Serial.begin(115200);
  while (!Serial) {
    ;
  }
  time_old = millis();
  time4 = 50;
}

void loop() {
  /* 
  digitalWrite(13, HIGH);
  delay(500);
  digitalWrite(13, LOW);
  delay(1000);
  */
  time = millis();
  time3 = time - time_old - time4;
  time_old = time;
  time4 = 50 - time3;

  g1 = analogRead(0);
  deg1 = (g1 - 644) / 1024 * 5 / 0.00067;
  Serial.print(time);
  Serial.print(" ");
  Serial.println(g1);
  
  delay(time4);
}
