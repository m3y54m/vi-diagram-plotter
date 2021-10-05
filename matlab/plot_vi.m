  MV=V;
  MI=I;

IP=MI;
VP=MV;
subplot(1,3,1);
plot(t,VP);
grid on;
subplot(1,3,2);
plot(t,IP);
grid on;
subplot(1,3,3);
R=VP./IP;
plot(t,R);
grid on;
