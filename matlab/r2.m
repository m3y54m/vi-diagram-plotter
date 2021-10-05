
%I=I-2*min(I);
%      V=tsmovavg(V,'s',1000,1);
%      I=tsmovavg(I,'s',1000,1);
v1=v-i*1494;
subplot(1,3,1);
plot(t,v1);
grid on;
subplot(1,3,2);
plot(t,i);
grid on;
R=v./i;
subplot(1,3,3);
plot(t,R);
grid on;
