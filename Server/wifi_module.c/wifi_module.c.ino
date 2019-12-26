#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <WiFiUDP.h>

const char *SSID = "fiord_wifi";
const char *PASS = "f10rdw1f1";
unsigned int localPort = 8888;

WiFiUDP UDP;
char packetBuffer[255];

IPAddress myIP(192, 168, 137, 214);
IPAddress hostIP(192, 168, 137, 1);
int cnt = 0;

void setup() {
  Serial.begin(115200);
  pinMode(ledPin, OUTPUT);
  pinMode(wifiStatusPin, OUTPUT);
  
  pinMode(13, OUTPUT);

  delay(500);
  digitalWrite(ledPin, HIGH);
  Serial.printLn("-");
  Serial.println("start");
  WiFi.mode(WIFI_STA);
  UDP.begin(5000);

  WiFi.begin(SSID, PASS);
  WiFi.config(myIP, WiFi.getewayIP(), WiFi.subnetMask());
  Serial.println("start_connect");
  while(WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("connected!");
  digitalWrite(wifiStatusPin, HIGH);
}

void send(char byteData[]) {
  if (UDP.beginPacket(HOSTIP, 5000)) {
    UDP.write(byteData);
    UDP.endPacket();
    Serial.println(byteData);
  }
}

void loop() {
  sprintf(packetBuffer, "from ESP:%d\n", cnt);
  Serial.print(packetBuffer);
  sendWifi(packetBuffer);
  delay(1000);
  end_loop();
}

void end_loop() {
  if (WiFi.status() != WL_CONNECTED) {
    WiFi.disconnect();
    digitalWrite(wifiStatusPin, LOW);
    Serial.println("disconnect!");
    connectWiFi();
  }
}
