% t1 = csvread('1.txt',1,0,[1,0,NaN,0]);
% v1 = csvread('1.txt',1,1,[1,1,NaN,1]);
% i1 = csvread('1.txt',1,2,[1,2,NaN,2]);
% 
% tr1 = csvread('1r.txt',1,0,[1,0,NaN,0]);
% vr1 = csvread('1r.txt',1,1,[1,1,NaN,1]);
% ir1 = csvread('1r.txt',1,2,[1,2,NaN,2]);

% v1=tsmovavg(v1,'s',500,1);
% i1=tsmovavg(i1,'s',500,1);
% 
% vr1=tsmovavg(vr1,'s',10,1);
% ir1=tsmovavg(ir1,'s',10,1);

data = load('1.0.mat');

t1 = data.t1;
v1 = data.v1;
i1 = data.i1;

tr1 = data.tr1;
vr1 = data.vr1;
ir1 = data.ir1;

v1=v1-i1*1494;
vr1=vr1-ir1*1494;

r1=v1./i1;
rr1=vr1./ir1;

figure(1);

subplot(1,3,1);
plot(t1,v1);
xlabel('Time (seconds)')
ylabel('Voltage (volts)')
hold on;
plot(tr1,vr1,'r');
grid on;

subplot(1,3,2);
plot(t1,i1);
hold on;
plot(tr1,ir1,'r');
grid on;

subplot(1,3,3);
plot(t1,r1);
hold on;
plot(tr1,rr1,'r');
grid on;
