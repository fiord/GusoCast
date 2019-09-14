# -*- coding: utf-8 -*-
import os
import sys
import cv2
import dlib
import numpy as np
import socket
from imutils import face_utils
from scipy.spatial import distance
import time

DEBUG = False
HOST = '127.0.0.1'
PORT = 12345
client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

cascade = cv2.CascadeClassifier("haarcascade_frontalface_alt2.xml")
face_parts_detector = dlib.shape_predictor("shape_predictor_68_face_landmarks.dat")
window = "顔認識テスト"

K = [6.5308391993466671e+002, 0.0, 3.1950000000000000e+002,
     0.0, 6.5308391993466671e+002, 2.3950000000000000e+002,
     0.0, 0.0, 1.0]
D = [7.0834633684407095e-002, 6.9140193737175351e-002, 0.0, 0.0, -1.3073460323689292e+000]

cam_matrix = np.array(K).reshape(3, 3).astype(np.float32)
dist_coeffs = np.array(D).reshape(5, 1).astype(np.float32)

object_pts = np.float32([[6.825897, 6.760612, 4.402142],
                         [1.330353, 7.122144, 6.903745],
                         [-1.330353, 7.122144, 6.903745],
                         [-6.825897, 6.760612, 4.402142],
                         [5.311432, 5.485328, 3.987654],
                         [1.789930, 5.393625, 4.413414],
                         [-1.789930, 5.393625, 4.413414],
                         [-5.311432, 5.485328, 3.987654],
                         [2.005628, 1.409845, 6.165652],
                         [-2.005628, 1.409845, 6.165652],
                         [2.774015, -2.080775, 5.048531],
                         [-2.774015, -2.080775, 5.048531],
                         [0.000000, -3.116408, 6.097667],
                         [0.000000, -7.415691, 4.070434]])

reprojectsrc = np.float32([[10.0, 10.0, 10.0],
                           [10.0, 10.0, -10.0],
                           [10.0, -10.0, -10.0],
                           [10.0, -10.0, 10.0],
                           [-10.0, 10.0, 10.0],
                           [-10.0, 10.0, -10.0],
                           [-10.0, -10.0, -10.0],
                           [-10.0, -10.0, 10.0]])

line_pairs = [[0, 1], [1, 2], [2, 3], [3, 0],
              [4, 5], [5, 6], [6, 7], [7, 4],
              [0, 4], [1, 5], [2, 6], [3, 7]]


def get_head_pose(shape):
    image_pts = np.float32([shape[17], shape[21], shape[22], shape[26], shape[36],
                            shape[39], shape[42], shape[45], shape[31], shape[35],
                            shape[48], shape[54], shape[57], shape[8]])

    _, rotation_vec, translation_vec = cv2.solvePnP(object_pts, image_pts, cam_matrix, dist_coeffs)

    reprojectdst, _ = cv2.projectPoints(reprojectsrc, rotation_vec, translation_vec, cam_matrix,
                                        dist_coeffs)

    reprojectdst = tuple(map(tuple, reprojectdst.reshape(8, 2)))

    # calc euler angle
    rotation_mat, _ = cv2.Rodrigues(rotation_vec)
    pose_mat = cv2.hconcat((rotation_mat, translation_vec))
    _, _, _, _, _, _, euler_angle = cv2.decomposeProjectionMatrix(pose_mat)

    return reprojectdst, euler_angle

def eye_size(eye):
    A = distance.euclidean(eye[1], eye[5])
    B = distance.euclidean(eye[2], eye[4])
    C = distance.euclidean(eye[0], eye[3])
    return (A+B) / (2.0*C)

class Kalman():
    #
    # Reference: http://www.cs.unc.edu/~welch/media/pdf/kalman_intro.pdf
    #

    def __init__(self, procvar, measvar):
        self.ctr = 0
        self.Q = procvar  # process variance
        self.P_k_minus_1 = 0.0  # a posteri error estimate
        self.R = measvar  # estimate of measurement variance, change to see effect

    def guess(self, input):
        DEBUG = False
        if self.ctr == 0:
            self.xhat_k_minus_1 = input  # a posteri estimate of x
            self.K = 1.0  # Kalman gain
            self.ctr = self.ctr + 1
            return input
        else:
            # time update
            xhat_k = self.xhat_k_minus_1 # a prior estimate of x, transition matrix is identity, no control
            Phat_k = self.P_k_minus_1 + self.Q # a prior estimate of error
            # measurement update
            self.K = Phat_k / (Phat_k + self.R) # Kalman gain
            estimate = xhat_k + self.K * (input - xhat_k) # a posteri estimate of x
            self.xhat_k_minus_1 = estimate # a posteri estimate of x
            self.P_k_minus_1 = (1 - self.K) * Phat_k # a posteri error estimate
            # error variance and kalman gain should become stable soon
            if DEBUG: print("Kalman:","Input",input,"Estimate",int(estimate), "ErrorVar {:.2}".format(self.P_k_minus_1), "KalmanGain {:.2}".format(self.K))
            return estimate

if __name__ == "__main__":
    capture = cv2.VideoCapture(0)
    if not capture.isOpened():
        print("Unable to connect to camera.")
        quit()

    kalmans = [[Kalman(0.5, 1) for i in range(2)] for j in range(68)]
    euler_kalmans = [Kalman(0.5, 1) for i in range(3)]

    while True:
        tick = cv2.getTickCount()

        key = cv2.waitKey(1)
        if key == 27:
            print("quit")
            break
        
        ret, img = capture.read()
        if ret == False:
            print("capture failed")
            break
        
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        
        facerect = cascade.detectMultiScale(img, scaleFactor=1.1, minNeighbors=1, minSize=(100, 100))
        if len(facerect) == 1:
            x, y, w, h = facerect[0]
            # gray_resized = cv2.resize(gray[y:y+h,x:x+w], dsize=None, fx=480/h, fy=480/h)
            # face = dlib.rectangle(0, 0, gray_resized.shape[1], gray_resized.shape[0])
            face = dlib.rectangle(x, y, x+w, y+h)
            face_parts = face_parts_detector(gray, face)
            face_parts = face_utils.shape_to_np(face_parts)
            
            # 目はkalman filterを挟んではいけません
            right_eye = eye_size(face_parts[36:42])
            left_eye = eye_size(face_parts[42:48])
            mouth = (cv2.norm(face_parts[61] - face_parts[67]) + cv2.norm(face_parts[62]-face_parts[66]) + cv2.norm(face_parts[63] - face_parts[65])) / (3.0 * cv2.norm(face_parts[60] - face_parts[64]))
            for i in range(face_parts.shape[0]):
                face_parts[i] = np.asarray([kalmans[i][j].guess(face_parts[i][j]) for j in range(2)])
            
            reprojectdst, euler_angle = get_head_pose(face_parts)
            for i in range(3):
                euler_angle[i, 0] = euler_kalmans[i].guess(euler_angle[i, 0])

            if DEBUG:
                for i, ((xx, yy)) in enumerate(face_parts[:]):
                    cv2.circle(img, (xx, yy), 1, (0, 255, 0), -1)
                    cv2.putText(img, str(i), (xx+2, yy-2), cv2.FONT_HERSHEY_SIMPLEX, 0.3, (0, 255, 0), 1)
                cv2.putText(img, "left_eye: {0}".format(round(left_eye, 3)), (20, 170), cv2.FONT_HERSHEY_PLAIN, 1, (0, 0, 255), 1, cv2.LINE_AA)
                cv2.putText(img, "right_eye: {0}".format(round(right_eye, 3)), (20, 200), cv2.FONT_HERSHEY_PLAIN, 1, (0, 0, 255), 1, cv2.LINE_AA)
                for start, end in line_pairs:
                    # reproject_start = (int(reprojectdst[start][0] * h / 480 + x), int(reprojectdst[start][1] * h / 480 + y))
                    # reproject_end = (int(reprojectdst[end][0] * h / 480 + x), int(reprojectdst[end][1] * h / 480 + y))
                    cv2.line(img, reprojectdst[start], reprojectdst[end], (0, 0, 255))
                cv2.putText(img, "X: " + "{:7.2f}".format(euler_angle[0, 0]), (20, 80), cv2.FONT_HERSHEY_SIMPLEX,
                            0.75, (0, 0, 0), thickness=2)
                cv2.putText(img, "Y: " + "{:7.2f}".format(euler_angle[1, 0]), (20, 110), cv2.FONT_HERSHEY_SIMPLEX,
                            0.75, (0, 0, 0), thickness=2)
                cv2.putText(img, "Z: " + "{:7.2f}".format(euler_angle[2, 0]), (20, 140), cv2.FONT_HERSHEY_SIMPLEX,
                            0.75, (0, 0, 0), thickness=2)
                # cv2.imshow("gray_resized", gray_resized)

            send_data = "{0} {1} {2} {3} {4} {5}".format(euler_angle[0, 0], euler_angle[1, 0], euler_angle[2, 0], left_eye, right_eye, mouth)
            client.sendto(send_data.encode("utf-8"), (HOST, PORT))
            print("send {0}".format(send_data))
        
        if DEBUG:
            fps = cv2.getTickFrequency() / (cv2.getTickCount() - tick)
            cv2.putText(img, "FPS:{0}".format(int(fps)), (10, 50), cv2.FONT_HERSHEY_PLAIN, 3, (0, 0, 255), 2, cv2.LINE_AA)
            cv2.imshow(window, img)

    
    capture.release()
    cv2.destroyAllWindows()