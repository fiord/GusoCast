from socket import socket, AF_INET, SOCK_DGRAM

HOST = ""
PORT = 12345

s = socket(AF_INET, SOCK_DGRAM)
s.bind((HOST, PORT))

while True:
    msg, address = s.recvfrom(8192)
    print("message: {0}\n from:{1}".format(msg, address))

s.close()